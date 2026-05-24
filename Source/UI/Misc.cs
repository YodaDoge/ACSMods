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
			public static void MoveBuffs(UI_Panel_ThingInfo __instance, XML xml)
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
			public static void MoveBuffs(UI_ThingMagicButton __instance, XML xml)
			{
				__instance.m_title.fontsize -= 3;
			}

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