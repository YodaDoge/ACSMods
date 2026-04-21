using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using XiaWorld;
using XiaWorld.Magic;

namespace ACS_Yoda_Tweaks
{
	public partial class CultivationTweaks : Mod
	{
		public static float GetMinMindState(Npc npc)
		{
			return npc.PropertyMgr.Practice.TouchNeck ? MinBC : MinStable;
		}

		[HarmonyPatch]
		public static partial class Patch
		{
			[HarmonyPrefix]
			[HarmonyPatch(typeof(BehaviourBasePractice), "Check")]
			public static bool GetBalancedCultivationActivity(ref JobBase __result, BehaviourBasePractice __instance, ref Npc npc, int seachr = 10000, bool tryfind = false)
			{
				if (!_info.Enabled || (npc.Rank != g_emNpcRank.Disciple))
					return true;

				try
				{
					if (npc.JobEngine.NeedWait()
					|| (npc.IsRent && !npc.HasSpecialFlag(g_emNpcSpecailFlag.NoLeaveMap))
					|| (npc.HealthState != g_emNpcHealthState.Normal))
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
							if (npc.MyPractice != null && npc.PropertyMgr.Practice.CheckMagic("ClosedDoor"))
							{
								//see: UILogicMode_IndividualCommand
								var cmd = npc.AddCommand("ClosedDoor", npc.MyPractice.Key, npc.MyPractice, 2); 
								cmd.WorkParam3 = "ClosedDoor";
								__result = JobMgr.Instance.CreateJob("JobAbsorbLing", cmd);
							}
							else
							{
								__result = JobMgr.Instance.CreateJob("JobPractice", null);
							}

						}
						return false;
					}
				}
				catch (Exception ex)
				{
					ShowMessage(ex);
				}
				return true;

			}

			public static bool ShouldDoFun(Npc npc)
			{
				return ShouldDoFun(npc.Needs.GetNeedValue(g_emNeedType.MindState), GetMinMindState(npc) + 2, MaxStable);
			}

			private static bool ShouldDoFun(float myValue, float minValue, float maxValue)
			{
				float clampedValue = Mathf.Clamp(myValue, minValue, maxValue);
				float t = (clampedValue - minValue) / (maxValue - minValue);

				float chance = 1.0f - t;
				float roll = World.RandomRange(0f, 1f);
				return roll < chance;
			}
		}
	}
}
