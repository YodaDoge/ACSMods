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
	public class DiscipleWhip : Mod
	{
		public override Meta Info => _info;
		private static Meta _info = new Meta("DiscipleWhip", "Reactive Disciples", true);

		private static bool _pauseAfterLoad = false;

		public DiscipleWhip(bool defaultEnabled) : base(defaultEnabled)
		{
		}

		[HarmonyPatch]
		public static class Patch
		{
			private static Type[] _alwaysSkipJobs = new Type[] { typeof(JobPlayWithBuilding), typeof(JobPlayWithSelf),
			typeof(JobPlayWithSth), typeof(JobIdle), typeof(JobPractice), typeof(JobPracticeSkill), typeof(JobLookAtSky), typeof(JobBasePractice) };
			public static void TryCancelJob(Npc npc)
			{
				if (npc.JobEngine.CurJob == null)
					return;

				var toil = npc.JobEngine.CurJob.GetCurToil();
				if (npc.JobEngine.CurJob.CanInterruptJob())
					npc.JobEngine.CurJob.InterruptJob();
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

				if (npc.JobEngine.CurJob != null)
				{
					bool isTrash = _alwaysSkipJobs.Contains(npc.JobEngine.CurJob.GetType());
					if (isTrash || (npc.JobEngine.CurJob is JobAbsorbLing ling && ling.CMD.def.Param == 6)) //meditation
						TryCancelJob(npc);
				}
			}

		}
	}

}