using FairyGUI;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting.Contexts;
using System.Text;
using UnityEngine;
using XiaWorld;
using XiaWorld.UI.InGame;

namespace ACS_Yoda_Tweaks
{
	public class CharacterSelection : Mod
	{
		public override Meta Info => _info;
		private static Meta _info = new Meta("BetterCharSelect", "Always use solo disciple", true);

		private static bool _pauseAfterLoad = false;

		public CharacterSelection(bool defaultEnabled) : base(defaultEnabled)
		{
		}

		[HarmonyPatch]
		public static class Patch
		{
			[HarmonyPostfix]
			[HarmonyPatch(typeof(Wnd_GameMain), "OnInit")]
			public static void MultiEatItem(Wnd_GameMain __instance)
			{
				try
				{
					var itemCmds = ThingUICommandDefine.sThingUICommands.GetValueSafe(g_emSelectThingSort.Item);
					var eatCmd = itemCmds.FirstOrDefault(x => x.Name == TFMgr.Get("食用给与")); //eat
					eatCmd.Act = delegate (Thing t, AreaBase a)
					{
						ItemThing item = t as ItemThing;
						Wnd_SelectNpc.Instance.Select(delegate (List<int> npcids)
						{
							if (npcids != null && npcids.Count > 0)
							{
								foreach (int npcid in npcids)
								{
									item.DoDropFromBag();
									TongLingMgr.Instance.TriggerJingGuaiStory("JGRuin", t, runaway: true);
									Npc npc = ThingMgr.Instance.FindThingByID(npcid) as Npc;
									npc.AddCommand("EatItem", item);
								}
							}
						}, maxcount: item.FreeCount);
					};
				}
				catch (Exception ex)
				{
					ShowMessage(ex);
				}
			}


			[HarmonyPostfix]
			[HarmonyPatch(typeof(Wnd_SelectNpc), "OnShowUpdate")]
			[HarmonyPriority(1000)]
			public static void AutoUseSingleAdventureNpc(Wnd_SelectNpc __instance, List<Npc> ___npcs, params object[] objs)
			{
				if (___npcs.Count == 1)
				{
					try
					{
						var lst = __instance.UIInfo.m_n25;
						lst.AddSelection(0, false, true);
						__instance.UIInfo.m_n27.onClick.Call();
					}
					catch (Exception ex)
					{
						ShowMessage(ex);
					}

				}
			}

			[HarmonyPostfix]
			[HarmonyPatch(typeof(Wnd_SelectNpc), "OnListClick")]
			[HarmonyPriority(1000)]
			public static void InstantOkSelect(Wnd_SelectNpc __instance, EventContext context)
			{
				if (!Input.GetKey(KeyCode.LeftControl))
					return;

				var okButton = __instance.UIInfo.m_n27;
				if (okButton.enabled)
					okButton.onClick.Call();
			}
		}
	}
}