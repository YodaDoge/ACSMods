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
			public static void PanelThingInfo_Update(Panel_ThingInfo __instance, float dt, UI_Panel_ThingInfo ___Panel)
			{
				//disabled until bug is removed that makes missing mentors valid for teaching...
				//if (!IsYodaMachine)
				//	return;

				if (!___Panel.visible || __instance.thing?.ThingType != g_emThingType.Npc || !UnityEngine.Input.GetKey(KeyCode.LeftShift) || !UnityEngine.Input.GetKeyDown(KeyCode.T))
					return;
				if (__instance.things != null && __instance.things.Count > 1)
					return;

				var npc = (Npc)__instance.thing;
				var master = npc.PropertyMgr.Practice?.Master;
				
				if (!npc.IsRealPlayerThing || master == null || !master.IsSelectAble || !master.IsAlive || master.IsInRemote || !master.IsValid || master.map != World.Instance.map
					|| master.InTomb || master.IsPuppet || master.IsZombie)
					return;

				UIMainMenuListDef_Data data = new UIMainMenuListDef_Data
				{
					Icon = "res/Sprs/ui/icon_chuanshou01"
				};
				UILogicMgr.Instance.ChangeMode(g_emUILogicMode.IndividualCommand, data, false, master, g_emIndividualCommandType.Teach);
			}
		}
	}
}
