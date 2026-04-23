using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using XiaWorld;

namespace ACS_Yoda_Tweaks
{
	public partial class CultivationTweaks : Mod
	{
		[HarmonyPatch]
		public static partial class Patch
		{
			[HarmonyPostfix]
			[HarmonyPatch(typeof(ToilPracticeSkill), "OnStepToil")]
			public static void PracticeSkillToil(ToilPracticeSkill __instance, float dt, KStateQUnit unit)
			{
				if (!_info.Enabled) return;

				if (__instance.npc.PropertyMgr.Practice.PracticeMode != g_emPracticeBehaviourKind.None)
					return;

				if (__instance.npc.Needs.GetNeedValue(g_emNeedType.MindState) < GetMinMindState(__instance.npc))
				{
					__instance.npc.JobEngine.InterruptJob();
				}
			}

			[HarmonyPostfix]
			[HarmonyPatch(typeof(JobPracticeSkill), "GetToilList")]
			public static void PracticeAtCultivationSpot(JobPracticeSkill __instance, ref List<ToilBase> __result)
			{
				if (!_info.Enabled) return;

				if (__instance.Worker.MyPractice != null && __instance.Worker.MyPractice.CheckWorkSpace(check: true) > 0
					&& __instance.Worker.MyPractice.GetWalkAbleTouchGrid(__instance.Worker.Key, nobase: false, onlycheck: true) > 0)
				{
					var buildingThing = __instance.Worker.MyPractice;
					if (buildingThing != null)
					{
						int num = buildingThing.CheckWorkSpace();
						__result.Clear();
						__result.Add(new ToilLockWorkSpace(buildingThing, num));
						__result.Add(ToilGoto.GotoThing(buildingThing, g_emPathEndMode.Touch, num).SetFogFind(b: true));
						__result.Add(new ToilJump2Building(buildingThing, num));
						__result.Add(new ToilPracticeSkill(World.RandomRange(1, 4)));
					}
				}
			}

		}
	}
}
