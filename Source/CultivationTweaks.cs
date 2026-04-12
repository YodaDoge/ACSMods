using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using XiaWorld;

namespace ACS_Yoda_Tweaks
{
	public class CultivationTweaks : Mod
	{
		public override Meta Info => _info;
		private static Meta _info = new Meta("AutoPause", "Auto Pause on Load", false);

		private const float MinStable = 50.3f;
		private const float MaxStable = 70f;
		private const float MinBC = 55f; //+45 with divinity = 100

		private static bool _pauseAfterLoad = false;

		public CultivationTweaks(bool defaultEnabled) : base(defaultEnabled)
		{
		}


		[HarmonyPatch]
		public static class Patch
		{
			[HarmonyPrefix]
			[HarmonyPatch(typeof(BehaviourBasePractice), "Check")]
			public static bool GetBalancedCultivationActivity(ref JobBase __result, BehaviourBasePractice __instance, Npc npc, int seachr = 10000, bool tryfind = false)
			{
				//if (npc.Name == "Artisan Qi")
				//{
				//	ShowMessage($"QI {npc.JobEngine == null } prop {npc.PropertyMgr.Practice == null}  ");
				//}

				if (npc.JobEngine.NeedWait()
				|| (npc.IsRent && !npc.HasSpecialFlag(g_emNpcSpecailFlag.NoLeaveMap))
				|| (npc.HealthState != g_emNpcHealthState.Normal)
				|| (npc.Rank != g_emNpcRank.Disciple))
					return true;

				if (npc.Rank == g_emNpcRank.Disciple && npc.CanDoDiscipleWork && npc.GongKind == g_emGongKind.Dao && npc.CanDoMagic() 
					&& npc.PropertyMgr.Practice.PracticeMode == g_emPracticeBehaviourKind.None && npc.PropertyMgr.Practice.CurNeck?.Kind != g_emGongBottleNeckType.God)
				{
					bool doFun = ShouldDoFun(npc);
					if (doFun)
					{
						__result = npc.Fun.GetFun(out var fun);
						if (__result is JobLookAtSky jobLookAtSky)
						{
							jobLookAtSky.FunID = fun.ID;
						}
						else if (__result is JobPlayWithBuilding jobPlayWithBuilding)
						{
							jobPlayWithBuilding.FunID = fun.ID;
						}
					}
					else if (npc.PropertyMgr.Practice.TouchNeck)
					{
						__result = JobMgr.Instance.CreateJob("JobPracticeSkill", null);
					}
					else
					{
						ShowMessage(npc.Name + " should do practice");
						__result = JobMgr.Instance.CreateJob("JobPractice", null);
					}
					return false;
				}
				return true;
			}

			public static bool ShouldDoFun(Npc npc)
			{
				return ShouldDoFun(npc.Needs.GetNeedValue(g_emNeedType.MindState), GetMinMindState(npc)+2, MaxStable -2);
			}

			private static bool ShouldDoFun(float myValue, float minValue, float maxValue)
			{
				float clampedValue = Mathf.Clamp(myValue, minValue, maxValue);

				float t = (clampedValue - minValue) / (maxValue - minValue);

				float chance = 1.0f - t;

				float roll = World.RandomRange(0f, 1f);

				return roll < chance;
			}

			private static float GetMinMindState(Npc npc)
			{ 
				return npc.PropertyMgr.Practice.TouchNeck ? MinBC : MinStable;
			}

			[HarmonyPostfix]
			[HarmonyPatch(typeof(ToilAbsorbLing), "OnEnterToil")]
			public static void OnEnterToil(ToilAbsorbLing __instance, KStateQUnit unit)
			{
				if (__instance.Job.CMD.def.Param == 6)
				{
					__instance.npc.SetSpecialFlag(g_emNpcSpecailFlag.FLAG_PRACTIVING);
				}
			}

			[HarmonyPostfix]
			[HarmonyPatch(typeof(ToilAbsorbLing), "OnLeaveToil")]
			public static void OnLeaveToil(ToilAbsorbLing __instance, KStateQUnit unit)
			{
				if (__instance.Job.CMD.def.Param == 6 && __instance.npc.HasSpecialFlag(g_emNpcSpecailFlag.FLAG_PRACTIVING))
				{
					//does nothing if flag doesnt exist, but we might be able to make it negative - hence the check
					__instance.npc.SubSpecialFlag(g_emNpcSpecailFlag.FLAG_PRACTIVING);
				}
			}

			[HarmonyPostfix]
			[HarmonyPatch(typeof(ToilAbsorbLing), "OnStepToil")]
			public static void OnStepToil(ToilAbsorbLing __instance, float dt, KStateQUnit unit)
			{
				var npc = __instance.npc;
				if (!npc.IsPlayerThing || npc.GongKind == g_emGongKind.God || npc.GongKind == g_emGongKind.Body)
					return;

				if (__instance.Job.CMD.def.Param == 6 && npc.Needs.GetNeedValue(g_emNeedType.MindState) < GetMinMindState(npc))
				{
					npc.JobEngine.InterruptJob();
				}
			}

			[HarmonyPostfix]
			[HarmonyPatch(typeof(ToilPractice), "OnStepToil")]
			public static void PracticeCultivationToil(ToilPractice __instance, ref bool ___m_bDid, float dt, KStateQUnit unit)
			{
				if (__instance.npc.PropertyMgr.Practice.PracticeMode != g_emPracticeBehaviourKind.None)
					return;

				if (__instance.npc.Needs.GetNeedValue(g_emNeedType.MindState) < GetMinMindState(__instance.npc))
				{
					___m_bDid = true;
					unit.IsFinished = true;
				}
			}

			[HarmonyPostfix]
			[HarmonyPatch(typeof(ToilPracticeSkill), "OnStepToil")]
			public static void PracticeSkillToil(ToilPracticeSkill __instance, ref bool ___m_bDid, float dt, KStateQUnit unit)
			{
				if (__instance.npc.PropertyMgr.Practice.PracticeMode != g_emPracticeBehaviourKind.None)
					return;

				if (__instance.npc.Needs.GetNeedValue(g_emNeedType.MindState) < GetMinMindState(__instance.npc))
				{
					___m_bDid = true;
					unit.IsFinished = true;
				}
			}

			[HarmonyPostfix]
			[HarmonyPatch(typeof(JobPracticeSkill), "GetToilList")]
			public static void PractiseAtCultivationSpot(JobPracticeSkill __instance, ref List<ToilBase> __result)
			{
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