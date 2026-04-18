using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using XiaWorld;
using XiaWorld.LSBTree;

namespace ACS_Yoda_Tweaks
{
	public partial class CultivationTweaks : Mod
	{
		public override Meta Info => _info;
		private static Meta _info = new Meta("CultivationTweaks", "Meditation and Cultivation Tweaks", true);

		private const float MinStable = 50.3f;
		private const float MaxStable = 70f;
		private const float MinBC = MinStable; //+45 with divinity = 100

		private static bool _pauseAfterLoad = false;

		private static bool NeedsRest(Npc npc)
		{
			return npc.Needs.GetNeedValue(g_emNeedType.Rest) < 35;
		}

		public CultivationTweaks(bool defaultEnabled) : base(defaultEnabled)
		{
		}
		private static bool frist = true;

		[HarmonyPatch]
		public static partial class Patch
		{
			[HarmonyPostfix]
			[HarmonyPatch(typeof(ToilAbsorbLing), "OnEnterToil")]
			public static void UseSpiritRootSympathyWhileMeditation(ToilAbsorbLing __instance, KStateQUnit unit)
			{
				if (!_info.Enabled) return;

				if (__instance.Job.CMD.def.Param == 6)
				{
					__instance.npc.SetSpecialFlag(g_emNpcSpecailFlag.FLAG_PRACTIVING);
				}
			}

			[HarmonyPostfix]
			[HarmonyPatch(typeof(ToilAbsorbLing), "OnLeaveToil")]
			public static void UseSpiritRootSympathyWhileMeditation_Exit(ToilAbsorbLing __instance, KStateQUnit unit)
			{
				if (!_info.Enabled) return;

				if (__instance.Job.CMD.def.Param == 6 && __instance.npc.HasSpecialFlag(g_emNpcSpecailFlag.FLAG_PRACTIVING))
				{
					//does nothing if flag doesnt exist, but we might be able to make it negative - hence the check
					__instance.npc.SubSpecialFlag(g_emNpcSpecailFlag.FLAG_PRACTIVING);
				}
			}

			[HarmonyPostfix]
			[HarmonyPatch(typeof(ToilAbsorbLing), "OnStepToil")]
			public static void AutoCancelMeditation(ToilAbsorbLing __instance, float dt, KStateQUnit unit)
			{
				if (!_info.Enabled) return;

				var npc = __instance.npc;
				if (!npc.IsPlayerThing || npc.GongKind == g_emGongKind.God || npc.GongKind == g_emGongKind.Body)
					return;

				if (__instance.Job.CMD.def.Param == 6 &&
				(npc.Needs.GetNeedValue(g_emNeedType.MindState) < GetMinMindState(npc) || npc.PropertyMgr.Practice.TouchNeck))
				{
					npc.JobEngine.InterruptJob();
				}
			}

			[HarmonyPostfix]
			[HarmonyPatch(typeof(ToilPractice), "OnStepToil")]
			public static void AutoCancelCultivation(ToilPractice __instance, float dt, KStateQUnit unit)
			{
				if (!_info.Enabled) return;

				if (__instance.npc.PropertyMgr.Practice.PracticeMode != g_emPracticeBehaviourKind.None
					|| NeedsRest(__instance.npc)) //cultivation fills sleep need
					return;

				if (__instance.npc.PropertyMgr.Practice.TouchNeck ||
				__instance.npc.Needs.GetNeedValue(g_emNeedType.MindState) < GetMinMindState(__instance.npc))
				{
					__instance.npc.JobEngine.InterruptJob(forceSuccess: true);
				}
			}

		}
	}
}