using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using XiaWorld;
using XiaWorld.UI.InGame;

namespace ACS_Yoda_Tweaks
{
	public class MentorTeach : Mod
	{
		public override Meta Info => _info;
		private static Meta _info = new Meta("MentorTeach", "MentorTeach", false);
		public MentorTeach(bool defaultEnabled) : base(defaultEnabled)
		{
		}

		[HarmonyPatch]
		public static class Patch
		{
			[HarmonyPostfix]
			[HarmonyPatch(typeof(Panel_ThingInfo), "OnUpdate")]
			public static void EquipAdventureFu(Panel_ThingInfo __instance, float dt, UI_Panel_ThingInfo ___Panel)
			{
				if (!___Panel.visible || __instance.thing?.ThingType != g_emThingType.Npc || !UnityEngine.Input.GetKey(KeyCode.LeftShift) || !UnityEngine.Input.GetKeyDown(KeyCode.T))
					return;
				if (__instance.things != null && __instance.things.Count > 1)
					return;
				AddLog("Teach Shortcut");
				var npc = (Npc)__instance.thing;
				if (!npc.IsRealPlayerThing || npc.PropertyMgr.Practice?.Master == null)
					return;
				UIMainMenuListDef_Data data = new UIMainMenuListDef_Data
				{
					Icon = "res/Sprs/ui/icon_chuanshou01"
				};
				UILogicMgr.Instance.ChangeMode(g_emUILogicMode.IndividualCommand, data, false, npc.PropertyMgr.Practice.Master, g_emIndividualCommandType.Teach);
			}
		}
	}
}
