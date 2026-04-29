using FairyGUI;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Linq;
using UnityEngine;
using XiaWorld;
using XiaWorld.UI.InGame;
using static XiaWorld.JianghuMgr;

namespace ACS_Yoda_Tweaks
{
	public class ReactiveDisciples : Mod
	{
		public override Meta Info => _info;
		private static Meta _info = new Meta("DiscipleWhip", "Reactive Disciples", true);

		private static bool _pauseAfterLoad = false;

		public ReactiveDisciples(bool defaultEnabled) : base(defaultEnabled)
		{
		}

		[HarmonyPatch]
		public static class Patch
		{
			private static HashSet<Type> _alwaysSkipJobs = new HashSet<Type> { typeof(JobPlayWithBuilding), typeof(JobPlayWithSelf),
			typeof(JobPlayWithSth), typeof(JobIdle), typeof(JobPractice), typeof(JobPracticeSkill), typeof(JobLookAtSky),
			typeof(JobBasePractice), typeof(JobMoveThingTo),
			typeof(JobMoveBuilding) , typeof(JobHarvest), typeof(JobPlant), typeof(JobCleanFloor), typeof(JobCutoff),
			typeof(JobBuild), typeof(JobCleanFloor), typeof(JobRemoveFloor), typeof(JobFree), typeof(JobAbsorbLing)
			};

			public static void TryCancelJob(Npc npc)
			{
				if (npc.JobEngine.CurJob == null)
					return;

				//var toil = npc.JobEngine.CurJob.GetCurToil();
				if (_alwaysSkipJobs.Contains(npc.JobEngine.CurJob.GetType()) && npc.JobEngine.CurJob.CanInterruptJob())
				{
					AddLog($"{npc.GetName()} interrupted {npc.JobEngine.CurJob?.jobdef?.Name} ");

					if (npc.InBuilding != null)
					{
						AddLog("Is walkable: " + npc.InBuilding.map.CheckGridWalkAble(npc.InBuilding.Key));
					}
					npc.JumpOutFromBuilding(true);
					npc.JobEngine.CurJob.InterruptJob();
				}
			}

			[HarmonyPostfix]
			[HarmonyPatch(typeof(UILogicMode_IndividualCommand), "OnApplyFinish")]
			public static void OnApplyFinish(UILogicMode_IndividualCommand __instance, Thing ___BindThing, ref bool did)
			{
				if (!_info.Enabled) return;

				if (!did)
					return;
				if (__instance.Type >= g_emIndividualCommandType._MAGICBEGIN && __instance.Type <= g_emIndividualCommandType._MAGIC_END)
				{
					if (___BindThing is Npc npc && npc.IsPlayerThing)
					{
						TryCancelJob(npc); //cancel bullshit when ordered to do stuff
					}
				}
			}

			[HarmonyPostfix]
			[HarmonyPatch(typeof(Thing), "AddCommand")]
			public static void AddCommand(Thing __instance, string type, params object[] param)
			{
				if (!_info.Enabled) return;

				if (!(__instance is Npc npc) || !npc.IsPlayerThing || !npc.IsSmartRace)
					return;
				AddLog($"{npc.GetName()} received cmd {type} is executing {npc.JobEngine.CurJob?.CMD?.CommandType}");
				TryCancelJob(npc);
			}

		}
	}

}