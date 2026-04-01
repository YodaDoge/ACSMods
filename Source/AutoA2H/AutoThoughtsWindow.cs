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
	internal class AutoThoughtsWindow : Window
	{
		public GComponent Frame;

		public GList ConfigList;

		public event EventCallback0 ConfigUpdated;

		public AutoThoughtsWindow()
		{
			base.contentPane = UIPackage.CreateObject("ModLoaderLite", "ConfigWindow").asCom;
			Frame = base.contentPane.GetChild("frame").asCom;
			Frame.text = "Auto Think";
			base.closeButton = Frame.GetChild("n5");
			closeButton.visible = false;
			ConfigList = base.contentPane.GetChild("n1").asList;
			base.contentPane.GetChild("enter").asButton.visible = false;

			//frame.margin = contentMargin;
			//frame.width = 200;
			frame.width = ConfigList.width = width = 250;
			ConfigList.margin = contentMargin;

		}

		public void AddCopyPasteButtons(Wnd_A2HCreateAgg parent)
		{
			try
			{
				var btnSave = (GButton)UIPackage.CreateObjectFromURL("ui://ncbwb41mv9j6ah");
				btnSave.name = "btnCopy";
				btnSave.title = btnSave.text = "Copy";
				btnSave.onClick.Add(e => { _copy = GetCheckedThoughts(_npc); });

				var btnPaste = (GButton)UIPackage.CreateObjectFromURL("ui://ncbwb41mv9j6ah");
				btnPaste.name = "btnPaste";
				btnPaste.title = btnPaste.text = "Paste";
				btnPaste.onClick.Add(e =>
				{
					if (_npc != null && _copy.Any())
						Update(_npc, _copy);
				});
				parent.AddChild(btnSave);
				parent.AddChild(btnPaste);

				btnSave.SetPosition(position.x, position.y + 2, position.z - 1);

				btnPaste.SetPosition(position.x, position.y + 30, position.z - 1);
			}
			catch (Exception ex)
			{
				Mod.ShowMessage(ex.ToString());
			}


		}

		private static List<string> _copy = new List<string>();

		static Margin ourMargin = new Margin() { left = 2, bottom = 2, top = 2, right = 2 };
		static Margin contentMargin = new Margin() { left = 5, bottom = 2, top = 2, right = 4 };

		IEnumerable<IGrouping<string, HEFragmentDef>> fragments;
		private static HashSet<string> dummyFragNames = new HashSet<string>() { "Empty", "Template" };
		private static HumanoidEvolutionMgr HMgr => HumanoidEvolutionMgr.Instance;
		private static Npc _npc;

		public void Update(Npc npc, IEnumerable<string> autoThoughts)
		{
			_npc = npc;

			fragments = HMgr.Fragments.ForEachKey.Select(x => HMgr.Fragments.GetDef(x.Key))
				.Where(x => x != null)
				.GroupBy(x => x?.Type);

			var rDef = HMgr.RaceInfos.GetDef(npc.RaceDefName);
			var heRule = HMgr.Rules.GetDef(rDef.RaceRule);

			ConfigList.RemoveChildrenToPool();
			bool first = true;
			foreach (var thoughtTypes in fragments)
			{
				GButton titleEntry = ConfigList.AddItemFromPool().asButton;
				titleEntry.title = string.Empty;// thoughtTypes.Key;
				if (first)
				{
					first = false;
					titleEntry.text = "Auto Think \n";
				}

				titleEntry.GetController("type").selectedIndex = 0;

				var rgbVal = "#" + HMgr.GetColor(heRule, thoughtTypes.FirstOrDefault().Name);
				bool hasColor = ColorUtility.TryParseHtmlString(rgbVal, out Color typeColor);
				//if (hasColor)
				//	titleEntry.titleColor = typeColor;

				int active = 0;
				foreach (var shardType in thoughtTypes.Where(x => !dummyFragNames.Contains(x.DisplayName)).OrderBy(x => x.DisplayName))
				{
					GButton listEntry = ConfigList.AddItemFromPool().asButton;
					listEntry.GetController("type").selectedIndex = 1; //Checkbox
					listEntry.margin = ourMargin;


					listEntry.title = shardType.DisplayName + "  (Lv " + shardType.Level + ")";
					//gButton2.titleColor = shardType.GetLevelColor();
					listEntry.SetTooltip(GetThinkToolTip, shardType.Name);

					listEntry.GetChild("id").text = shardType.Name;

					var chkBox = listEntry.GetChild("cb").asButton;
					bool selected = autoThoughts.Contains(shardType.Name);
					chkBox.selected = selected;
					chkBox.enabled = false; //used by daddy
					if (selected)
						active++;

					listEntry.onClick.Add(ButtonClicked);
					listEntry.color = typeColor;
				}
				//gButton.title += $" ({active}) ";
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
			if (frag is string)
				return IManagerModule_LoopInterval<HumanoidEvolutionMgr>.Instance.GetFragDesc(_npc, frag?.ToString(), true);
			return "No Tooltip";
		}

		public List<string> GetCheckedThoughts(Npc npc)
		{
			List<string> autoThoughts = new List<string>();
			for (int i = 0; i < ConfigList.numItems; i++)
			{
				var gButton = ConfigList.GetChildAt(i).asButton;
				if (gButton.GetController("type").selectedIndex != 1)
					continue;

				var checkState = gButton.GetChild("cb").asButton.selected;
				if (checkState)
					autoThoughts.Add(gButton.GetChild("id").text);
			}
			return autoThoughts;
		}
	}
}
