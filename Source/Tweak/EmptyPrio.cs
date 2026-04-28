using HarmonyLib;
using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using XiaWorld;
using XiaWorld.UI.InGame;
using static XiaWorld.AuctionData;

namespace ACS_Yoda_Tweaks
{
	public class EmptyPrio : Mod
	{
		public override Meta Info => _info;
		private static Meta _info = new Meta("EmptyPrio", "Recruit without Priorities", false);

		private static bool _pauseAfterLoad = false;

		public EmptyPrio(bool defaultEnabled) : base(defaultEnabled)
		{
		}

		[HarmonyPatch]
		public static class Patch
		{
			[HarmonyPostfix]
			[HarmonyPatch(typeof(Npc), "ChangeRank")]
			public static void ChangeRank(Npc __instance, g_emNpcRank rank, bool changerank = true, bool vistor = false, bool checksame = true)
			{
				if (!_info.Enabled || rank != g_emNpcRank.Worker)
					return;

				for (g_emBehaviourWorkKind g_emBehaviourWorkKind2 = g_emBehaviourWorkKind.Treat; g_emBehaviourWorkKind2 < g_emBehaviourWorkKind.Count; g_emBehaviourWorkKind2++)
				{
					if (g_emBehaviourWorkKind2 == g_emBehaviourWorkKind.Xiulian || g_emBehaviourWorkKind2 == g_emBehaviourWorkKind.Rest
					|| g_emBehaviourWorkKind2 == g_emBehaviourWorkKind.Care || g_emBehaviourWorkKind2 == g_emBehaviourWorkKind.Clean)
					{
						continue;
					}
					__instance.JobEngine.ChangeBehaviourEnable(g_emBehaviourWorkKind2, v: false);

					if (g_emBehaviourWorkKind2 == g_emBehaviourWorkKind.Carry)
					{
						__instance.JobEngine.ChangeBehaviourEnable(g_emBehaviourWorkKind.Clean, false);
					}
				}
				__instance.HardZJ = true; //WORK HARD
			}

			
		}
	}
}