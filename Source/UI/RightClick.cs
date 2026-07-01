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

			[HarmonyPatch(typeof(XiaWorld.UILogicMgr), nameof(XiaWorld.UILogicMgr.OnMapClick))]
			[HarmonyPrefix]
			static bool OnMapClick_Prefix(XiaWorld.UILogicMgr __instance, Vector3 pos, int key, int bnt)
			{

				if (bnt == 1)
				{
					var curMode = UILogicMgr.Instance.GetCurMode();
					var thing = (curMode as UILogicMode_Select)?.CurSelectThing;
					AddLog(thing.ToString());

					if (thing is Npc me)
					{
						AddLog("me");
						var map = World.Instance.map;
						var things = map.Things.GetThingsAtGrid(key);

						Collider2D[] array = Physics2D.OverlapPointAll(pos, 1024);
						Collider2D[] array2 = array;

						Npc npc = null;
						foreach (Collider2D collider2D in array2)
						{
							if (collider2D != null)
							{
								npc = collider2D.GetComponentInParent<NpcView>().npc;
								if (npc.IsSelectAble)
								{
									GameWatch.Instance.PlayUIAudio("Sound/UI/clicknpc");
									me.AddCommand("TryTalk", npc);
									return false;
								}
							}
						}

						foreach (var item in things)
						{
							AddLog(item.ToString());
							if (item is ItemThing itm)
							{
								me.AddCommand("EquipItem", itm);
								GameWatch.Instance.PlayUIAudio("Sound/UI/click");
								return false;
							}
						}
					}
				}
				return true;
			}

			//[HarmonyPostfix]
			//[HarmonyPatch(typeof(UI_Panel_ThingInfo), "ConstructFromXML")]
			//public static void MoveBuffsBottom(UI_Panel_ThingInfo __instance, XML xml)
			//{
			//	__instance.m_buffs.rootContainer.y -= 30;
			//	__instance.m_buffs.container.y -= 30;
			//	__instance.m_buffs.displayObject.y -= 30;
			//	__instance.m_buffs.width = __instance.width;
			//	__instance.m_buffs.align = AlignType.Left;
			//	__instance.m_buffs.verticalAlign = VertAlignType.Top;
			//	AddLog("is null "+ (__instance.m_buffs.parent.parent == null));
			//	AddLog(__instance.m_buffs.y.ToString());
			//	GetPublicGImages(__instance).ForEach(x => x.height += 30);
			//}
		}
	}
}