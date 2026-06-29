using FairyGUI;
using FairyGUI.Utils;
using HarmonyLib;
using KTV;
using Light2D;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using XiaWorld;
using XiaWorld.UI.InGame;
using static GameWatch.OLDDATA;

namespace ACS_Yoda_Tweaks
{
	public class WorldMapTweaks : Mod
	{
		public override Meta Info => _info;
		private static Meta _info = new Meta("WorldMapTweaks", "Enable Immortal Save/Load", true);

		public WorldMapTweaks(bool defaultEnabled) : base(defaultEnabled)
		{
		}

		[HarmonyPatch]
		public static class Patch
		{
			private static PopupMenu MainMenu;

			private static EventListener _defaultClick;



			[HarmonyPostfix]
			[HarmonyPatch(typeof(Wnd_World), "OnInit")]
			public static void AddSoftRecallShortcut(Wnd_World __instance)
			{
				var btn = __instance.UIInfo.m_callbacknpc;
				SetOnClick(__instance, btn, true);
			}

			[HarmonyPostfix]
			[HarmonyPatch(typeof(Wnd_World), "_UpdataNpc")]
			public static void DiscipleListAddSoftRecall(Wnd_World __instance, PlacesMgr.MapExploreData data, Npc npc, UI_BntPlaceNpcs npcbtn)
			{
				var btn = npcbtn.m_n13;
				if (npcbtn.m_n13.enabled)
					SetOnClick(__instance, btn, false);
			}

			private static void SetOnClick(Wnd_World __instance, GButton btn, bool update)
			{
				btn.onClick.Clear();
				btn.onClick.Add(delegate (EventContext context)
				{
					SoftRecall(__instance, context, update);

				});
				btn.GetChildAt(0).parent.tooltips = "Ctrl: Remove Repeat";
			}

			[HarmonyPostfix]
			[HarmonyPatch(typeof(UI_BntPlaceNpcs), "ConstructFromXML")]
			public static void BiggerDiscipleList(UI_BntPlaceNpcs __instance, XML xml)
			{
				var txtName = __instance.m_name;
				//txtName.fontsize += 4;

				txtName.width += 45;
				txtName.align = AlignType.Left;
				txtName.root.margin = new Margin() { left = 5 };
				var lblTime = __instance.m_time;
				lblTime.width -= 3; //default 80
				lblTime.align = AlignType.Right;
				lblTime.x += 13;
			}

			[HarmonyPostfix]
			[HarmonyPatch(typeof(UI_BntPlace_Npc), "ConstructFromXML")]
			public static void BiggerDiscipleWorldMapName(UI_BntPlace_Npc __instance, XML xml)
			{
				var txtName = __instance.m_title;
				txtName.fontsize += 2;
				txtName.width += 30;
			}

			private static string[] GoTexts = new string[3]
				{
					TFMgr.Get("历练"),
					TFMgr.Get("驻扎"),
					TFMgr.Get("征伐")
				};

			[HarmonyPrefix]
			[HarmonyPatch(typeof(Wnd_World), "ShowGoSelect")]
			public static bool HideLimitNpc(Wnd_World __instance, EventContext context)
			{
				int gotype = (int)(context.sender as GObject).data2;
				bool isstay = gotype == 1;
				if (World.Instance.map.Things.OnlyBossExist())
				{
					Wnd_Message.Show(string.Format(TFMgr.Get("门派存亡之际不宜外出。")), 1, null, modal: true, GoTexts[gotype], 0, 0, string.Empty);
					return false;
				}
				string name = (string)(context.sender as GObject).data;
				PlaceDef placeDef = PlacesMgr.Instance.GetPlaceDef(name);
				var validNpc = WorldMgr.Instance.curWorld.map.Things.GetPlayerActiveNpcs(g_emNpcRaceType.Wisdom);
				validNpc.RemoveAll(npc => (npc.PropertyMgr.Practice.TouchNeck && npc.PropertyMgr.Practice.CurNeck != null && npc.PropertyMgr.Practice.CurNeck.NeckCountdown > 0f && !npc.HasSpecialFlag(g_emNpcSpecailFlag.FLAG_PRACTICEDIE)) || (npc.GongKind == g_emGongKind.God && (npc.PropertyMgr.Practice.GodPracticeData.FaithJieColdDown > 0f || npc.PropertyMgr.Practice.GodPracticeData.FeishengJieColdDown > 0f)));
				Wnd_SelectNpc.Instance.Select(delegate (List<int> ids)
				{
					if (ids != null && ids.Count > 0)
					{
						List<Npc> gooutnpcs = new List<Npc>();
						foreach (int id in ids)
						{
							Npc npc = ThingMgr.Instance.FindThingByID(id) as Npc;
							if (npc.HasSpecialFlag(g_emNpcSpecailFlag.FLAG_CANTEXPLORESTAY))
							{
								Wnd_Message.Show(string.Format(TFMgr.Get("{0}正在静修中，不能外出历练。"), npc.GetName()), 1, null, modal: true, GoTexts[gotype], 0, 0, string.Empty);
							}
							else if (!npc.IsRent)
							{
								if (gotype == 2)
								{
									gooutnpcs.Add(npc);
								}
								else
								{
									gooutnpcs.Add(npc);
								}
							}
						}
						if (gotype > 0)
						{
							foreach (Npc item in gooutnpcs)
							{
								item.AddCommand("GoMapExplore", name, isstay);
							}
							return;
						}
						Wnd_Message.ShowSlider(TFMgr.Get("循环历练"), 1, delegate (float v)
						{
							int num = 0;
							if (v > 0f)
							{
								num = (int)(v * 5f);
							}
							if (v == 11f)
							{
								num = -1;
							}
							foreach (Npc item2 in gooutnpcs)
							{
								item2.AddCommand("GoMapExplore", name, isstay, (num != 1) ? num : 0);
							}
						}, modal: true, delegate (float v)
						{
							int num = 1;
							if (v > 0f)
							{
								num = (int)(v * 5f);
							}
							if (v == 11f)
							{
								num = -1;
							}
							return string.Format(TFMgr.Get("循环{0}次\n(召回可以中断循环)"), (num != -1) ? num.ToString() : TFMgr.Get("无数"));
						}, null, 11f, intmode: true);
					}
				}, g_emNpcRank.Disciple, 1, 50, null, (Npc npc) => !npc.IsRent && npc.CheckCommandSingle("GoMapExplore") == null, string.Format(TFMgr.Get("前往{0}"), placeDef.DisplayName), delegate (Npc npc)
				{
					if (!isstay)
					{
						float num = (PlacesMgr.Instance.GetCost(npc, name) * 2f + 120f) / 600f;
						string text = string.Format(TFMgr.Get("耗时：{0:N2}天\n"), num);
						if (num > 3f)
						{
							text += TFMgr.Get("[color=#B65704]耗时较长[/color]\n");
						}
						return text;
					}
					return (string)null;
				}, npclist: validNpc);
				return false;
			}

			[HarmonyPostfix]
			[HarmonyPatch(typeof(Command), "FinishCommand")]
			public static void MapExploreFinish(Command __instance, bool del = false, bool debug = false, bool mustRemove = false)
			{
				if (!(__instance.OwnerThing is Npc npc) || !(__instance is CommandGoMapExplore cmd))
					return;
				npc.RemoveSpecialFlag(g_emNpcSpecailFlag.FLAG_CANTEXPLORESTAY);
			}

			[HarmonyPostfix]
			[HarmonyPatch(typeof(UI_Bnt_SelectNpcItem), "ConstructFromXML")]
			public static void BiggerDiscipleSelectionNames(UI_Bnt_SelectNpcItem __instance, XML xml)
			{
				var lblName = __instance.m_title; // .title = text
												  //Once($"h{lblName.height} w{lblName.width} fs{lblName.fontsize}");
				__instance.m_title.singleLine = false;
				__instance.m_title.fontsize -= 4;
				__instance.m_title.width += 6;
				__instance.m_title.height += 14;
				__instance.m_title.align = AlignType.Center;
				__instance.m_title.verticalAlign = VertAlignType.Middle;
				lblName.y -= 6;
				lblName.x -= 2;
				//__instance.height += 8;

			}

			private static void SoftRecall(Wnd_World __instance, EventContext context, bool updateOpenShow)
			{
				try
				{
					GObject gObject = (GObject)context.sender;
					PlacesMgr.MapExploreData data = (PlacesMgr.MapExploreData)gObject.data;
					Npc npc = ThingMgr.Instance.FindThingByID(data.NpcID) as Npc;

					if (Input.GetKey(KeyCode.LeftControl))
					{
						List<Command> list = npc.CheckCommand("GoMapExplore", checkcount: true);
						var cmd = list?.FirstOrDefault() as CommandGoMapExplore;
						if (cmd != null)
						{
							cmd.count = 0;
							if (data.Stage > 0)
								cmd.FinishCommand();
						}
					}
					else
						PlacesMgr.Instance.CallBackNpc(data);

					if (updateOpenShow)
						__instance.GetType().GetMethod("ShowNpcInfo", BindingFlags.NonPublic | BindingFlags.Instance)
									.Invoke(__instance, new object[] { data, false });
					__instance.UpdateNpcs();
				}
				catch (Exception ex)
				{
					ShowMessage(ex);
				}
			}


		}
	}
}