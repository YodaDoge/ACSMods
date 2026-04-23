using FairyGUI;
using ModLoaderLite.Config;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using UnityEngine;
using XiaWorld;
using XiaWorld.UI.InGame;
using XLua.TemplateEngine;
using static FairyGUI.MovieClip;
using static XiaWorld.AuctionData;

namespace ACS_Yoda_Tweaks.AutoA2H
{
	public partial class A2H : Mod
	{
		internal class AutoThoughtsWindow : Window
		{
			public GComponent Frame;

			public GList ConfigList;

			public event EventCallback0 ConfigUpdated;
			
			public AutoThoughtsWindow()
			{
				base.contentPane = UIPackage.CreateObject("ModLoaderLite", "ConfigWindow").asCom;
				Frame = base.contentPane.GetChild("frame").asCom;
				var title = Frame.GetChild("title").asTextField;
				title.text = "Auto Think";
				Frame.GetChild("n6").width = title.width = 205; //title background
				base.closeButton = Frame.GetChild("n5");
				closeButton.visible = false;
				ConfigList = base.contentPane.GetChild("n1").asList;
				base.contentPane.GetChild("enter").asButton.visible = false;

				frame.width = ConfigList.width = width = 250;
				ConfigList.margin = listMargin;

				var btnSave = (GButton)UIPackage.CreateObjectFromURL("ui://ncbwb41mv9j6ah");
				btnSave.name = "btnCopy";
				btnSave.title = btnSave.text = "Copy";
				btnSave.onClick.Add(e => { _copy = GetCheckedThoughts(_npc); });

				var btnPaste = (GButton)UIPackage.CreateObjectFromURL("ui://ncbwb41mv9j6ah");
				btnPaste.name = "btnPaste";
				btnPaste.title = btnPaste.text = "Paste";
				btnPaste.onClick.Add(e =>
				{
					if (_npc != null && _copy != null)
						Update(_npc, _copy);
				});
				AddChild(btnSave);
				AddChild(btnPaste);

				btnSave.SetPosition(position.x, position.y + 5, position.z - 1);
				btnPaste.SetPosition(position.x, position.y + 32, position.z - 1);

				txtSearch = (UI_InputTextField)UIPackage.CreateObject("InGame", "InputTextField");
				txtSearch.m_title.onChanged.Add(TextChange);
				txtSearch.SetPosition(position.x + 10, position.y + height - 50, position.z - 1);
				txtSearch.m_title.promptText = "Search";
				AddChild(txtSearch);

				GButton btnToggle = (GButton)UIPackage.CreateObjectFromURL("ui://ncbwb41mv9j6ah");
				btnToggle.name = "Toggle";
				btnToggle.title = "Toggle";
				btnToggle.text = "Toggle";
				btnToggle.onClick.Add(ToggleAll);
				btnToggle.SetPosition(txtSearch.position.x + txtSearch.width + 5, position.y + height - 50, position.z - 1);
				AddChildAt(btnToggle, GetChildIndex(txtSearch) + 1);
			}

			private void ToggleAll(EventContext context)
			{
				var firstBtn = buttons.FirstOrDefault(x => x.visible);
				if (firstBtn == null)
					return;

				var checkState = firstBtn.GetChild("cb").asButton.selected;

				buttons.ForEach(btn =>
				{
					if (btn.visible)
					{
						btn.GetChild("cb").asButton.selected = !checkState;
					}
				});
			}

			private static UI_InputTextField txtSearch;

			private void TextChange(EventContext context)
			{
				try
				{
					var sender = context.sender as GTextInput;
					//TODO: remove/add buttons due to search
					var txt = sender.text.ToUpper();
					ConfigList.RemoveChildren();

					bool isEmpty = string.IsNullOrEmpty(txt);
					foreach (var btn in buttons)
					{
						var shardName = btn.GetChild("id").text;
						btn.visible = isEmpty || GetThinkToolTip(null, shardName).ToUpper().Contains(txt);
						if (btn.visible)
							ConfigList.AddChild(btn);
					}
					ConfigList.container.EnsureSizeCorrect();
				}
				catch (Exception ex)
				{
					Mod.ShowMessage(ex);
				}

			}

			private static List<string> _copy = new List<string>();

			static Margin entryMargin = new Margin() { left = 2, bottom = 2, top = 2, right = 2 };
			static Margin listMargin = new Margin() { left = 5, bottom = 2, top = 2, right = 4 };

			private static HashSet<string> dummyFragNames = new HashSet<string>() { "Empty", "Template" };
			private static HumanoidEvolutionMgr HMgr => HumanoidEvolutionMgr.Instance;
			private static Npc _npc;

			List<GButton> buttons = new List<GButton>();

			public void Update(Npc npc, IEnumerable<string> autoThoughts)
			{
				_npc = npc;
				buttons.Clear();
				txtSearch.text = string.Empty;
				var fragments = HMgr.Fragments.ForEachKey.Select(x => HMgr.Fragments.GetDef(x.Key))
					.Where(x => x != null)
					.GroupBy(x => x?.Type);

				var rDef = HMgr.RaceInfos.GetDef(npc.RaceDefName);
				var heRule = HMgr.Rules.GetDef(rDef.RaceRule);
				ConfigList.RemoveChildren();

				var scores = IsYodaMachine && AutoNPC.ContainsKey(npc.ID) ? CreateThinkFragScoring(npc) : null;
				bool first = true;
				foreach (var thoughtTypes in fragments)
				{
					//GButton titleEntry = ConfigList.AddItemFromPool().asButton;
					//titleEntry.title = string.Empty;// thoughtTypes.Key;
					//if (first)
					//{
					//	first = false;
					//	titleEntry.text = "Auto Think \n";
					//}

					//titleEntry.GetController("type").selectedIndex = 0;

					var rgbVal = "#" + HMgr.GetColor(heRule, thoughtTypes.FirstOrDefault().Name);
					ColorUtility.TryParseHtmlString(rgbVal, out Color typeColor);

					foreach (var shardType in thoughtTypes.Where(x => !dummyFragNames.Contains(x.DisplayName)).OrderBy(x => x.DisplayName))
					{
						GButton listEntry = ConfigList.AddItemFromPool().asButton;
						listEntry.GetController("type").selectedIndex = 1; //Checkbox
						listEntry.margin = entryMargin;

						listEntry.title = shardType.DisplayName + "  (Lv " + shardType.Level + ")";
						if (scores != null)
						{
							var score = scores.FirstOrDefault(x => x.Name == shardType.Name);
							if (score)
								listEntry.title += " S:" + score.Score;
						}

						listEntry.SetTooltip(GetThinkToolTip, shardType.Name);
						listEntry.GetChild("id").text = shardType.Name;

						var chkBox = listEntry.GetChild("cb").asButton;
						chkBox.selected = autoThoughts.Contains(shardType.Name);
						chkBox.enabled = false; //used by daddy

						listEntry.onClick.Add(ButtonClicked);
						listEntry.color = typeColor;
						buttons.Add(listEntry);
					}
				}
			}

			private void ButtonClicked(EventContext context)
			{
				if (context.sender is GButton btn)
				{
					var cb = btn.GetChild("cb").asButton;
					cb.selected = !cb.selected;
				}
			}

			private string GetThinkToolTip(GObject obj, object frag)
			{
				return GetThinkToolTip(obj, frag?.ToString());
			}

			private string GetThinkToolTip(GObject obj, string frag)
			{
				if (frag is string)
					return IManagerModule_LoopInterval<HumanoidEvolutionMgr>.Instance.GetFragDesc(_npc, frag?.ToString(), true);
				return "No Tooltip";
			}

			public List<string> GetCheckedThoughts(Npc npc)
			{
				List<string> autoThoughts = new List<string>();
				foreach (var gButton in buttons)
				{
					if (gButton.GetController("type").selectedIndex != 1)
						continue;

					var checkState = gButton.GetChild("cb").asButton.selected;
					if (checkState)
						autoThoughts.Add(gButton.GetChild("id").text);
				}
				return autoThoughts;
			}

			internal void ClearButtons()
			{
				buttons.Clear();
			}
		}
	}
}
