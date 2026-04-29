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