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
	public partial class UITweaks : Mod
	{
		public override Meta Info => _info;
		private static Meta _info = new Meta("WorldMapTweaks", "Enable Immortal Save/Load", true);

		public UITweaks(bool defaultEnabled) : base(defaultEnabled)
		{
		}

		[HarmonyPatch]
		public static class Patch
		{
			[HarmonyPostfix]
			[HarmonyPatch(typeof(UI_Panel_ThingInfo), "ConstructFromXML")]
			public static void MoveMiracles(UI_Panel_ThingInfo __instance, XML xml)
			{
				__instance.m_magicbnts.columnCount = 4;
				var bg = __instance.m_n72;
				bg.width *= 1.3f;
				bg.height *= 1.34f;
				bg.y -= 30;
				__instance.m_magicbnts.y -= 30;
			}

			[HarmonyPostfix]
			[HarmonyPatch(typeof(UI_ThingMagicButton), "ConstructFromXML")]
			public static void SmallMiracleFont(UI_ThingMagicButton __instance, XML xml)
			{
				__instance.m_title.fontsize -= 3;
			}

			//[HarmonyPatch(typeof(Panel_ThingInfo), "ShowBuffItem")]
			//static void Postfix(Panel_ThingInfo __instance, int uiitemidx,	string name, string tooltip, float fillAmount, string stack, UI_Panel_ThingInfo ___Panel)
			//{
			//	var icon = ___Panel.m_buffs.GetChildAt(uiitemidx) as UI_bufficon;
			//	if (uiitemidx % 2 == 0) 
			//		icon.y += 2; 
			//	//icon.z -= 20;
			//}

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

			//public static IEnumerable<GImage> GetPublicGImages(UI_Panel_ThingInfo instance)
			//{
			//	return typeof(UI_Panel_ThingInfo)
			//		.GetFields(BindingFlags.Instance | BindingFlags.Public)
			//		.Where(f => f.FieldType == typeof(GImage))
			//		.Select(f => (GImage)f.GetValue(instance));
			//}

			[HarmonyPostfix]
			[HarmonyPatch(typeof(JobPaintCharm), "OnLeaveJob")]
			public static void ShorterTalismanNames(JobPaintCharm __instance, KStateQUnit unit)
			{
				if (IsYodaMachine)
				{
					var itm = ThingMgr.Instance.FindThingByID((__instance.CMD as CommandPaintCharm).CharmItemId) as ItemThing;

					var name = itm.GetName().Replace("Talisman of", string.Empty)
											.Replace(" Talisman", string.Empty)
											.Replace("Illustration of ", string.Empty)
											.Replace("Everlasting", "Lasting")
											.Trim();
					itm.SetName(name);
				}
			}
		}
	}
}