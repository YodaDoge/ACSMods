using FairyGUI;
using HarmonyLib;
using JP;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting.Contexts;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using XiaWorld;
using XiaWorld.UI.InGame;

namespace ACS_Yoda_Tweaks
{
	public class RightClick : Mod
	{
		public override Meta Info => _info;
		private static Meta _info = new Meta("RightClick", "Rightclick like its 2005", true);

		private static bool _pauseAfterLoad = false;

		public RightClick(bool defaultEnabled) : base(defaultEnabled)
		{
		}

		[HarmonyPatch]
		public static class Patch
		{
			//[HarmonyPostfix]
			//[HarmonyPatch(typeof(Wnd_SelectNpc), "OnListClick")]
			//[HarmonyPriority(1000)]
			//public static void InstantOkSelect(Wnd_SelectNpc __instance, EventContext context)
			//{
			//	if (!Input.GetKey(KeyCode.LeftControl))
			//		return;

			//	var okButton = __instance.UIInfo.m_n27;
			//	if (okButton.enabled)
			//		okButton.onClick.Call();
			//}


			//[HarmonyPatch(typeof(XiaWorld.UILogicMode_Select), nameof(XiaWorld.UILogicMode_Select.RightClick_Global))]
			//[HarmonyPrefix]
			//static bool RightClick_Global_Prefix()
			//{
			//	// replacement logic
			//	return false; // skip original
			//}

			[HarmonyPrefix]
			[HarmonyPatch(typeof(XiaWorld.UILogicMgr), nameof(XiaWorld.UILogicMgr.OnMapClick))]
			public static bool OnWorldClick(XiaWorld.UILogicMgr __instance, Vector3 pos, int key, int bnt)
			{
				if (GameWatch.Instance.Mode != g_emGameMode.HardCore && GameWatch.Instance.Mode != g_emGameMode.Normal)
					return true;

				if (bnt == 1)
				{
					var curMode = UILogicMgr.Instance.GetCurMode();
					var thing = (curMode as UILogicMode_Select)?.CurSelectThing;

					if (thing is Npc me)
					{
						if (me.FightBody.IsFighting || !me.IsRealPlayerThing)
							return true;

						var map = World.Instance.map;
						var things = map.Things.GetThingsAtGrid(key);

						Collider2D[] array = Physics2D.OverlapPointAll(pos, 1024);
						Collider2D[] array2 = array;

						foreach (Collider2D collider2D in array2)
						{
							if (collider2D != null)
							{
								Npc npc = collider2D.GetComponentInParent<NpcView>().npc;
								if (npc.IsSelectAble && !npc.IsRealPlayerThing && !npc.FightBody.IsFighting && npc.IsSmartRace)
								{
									GameWatch.Instance.PlayUIAudio("Sound/UI/clicknpc");
									me.AddCommand("TryTalk", npc);
									return false;
								}
							}
						}

						foreach (var item in things)
						{
							if (item is ItemThing itm && itm.Camp == XiaWorld.Fight.g_emFightCamp.Player)
							{
								me.AddCommand("EquipItem", itm);
								GameWatch.Instance.PlayUIAudio("Sound/UI/click");
								return false;
							}
							else if (item is BuildingThing building)
							{
								//AddLog(building.def.Name);
								if (building.def.Name == "Building_BookShelf_CangJing")
								{
									AddLog(me.Rank.ToString());
									AddLog(me.GongKind.ToString());
									if (me.CanDoMagic() && me.Rank == g_emNpcRank.Disciple && me.GongKind == g_emGongKind.Dao)
										Wnd_CangJingGeWindow.Instance.Show(building, 1, me);
								}

								//Assign Bed/Practice°
							}
						}
					}
				}
				return true;
			}

		}
	}
}