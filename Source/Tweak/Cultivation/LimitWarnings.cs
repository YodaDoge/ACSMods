using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using XiaWorld;
using static XiaWorld.AuctionData;
using static XiaWorld.OutspreadMgr;

namespace ACS_Yoda_Tweaks
{
	public partial class CultivationTweaks : Mod
	{
		[HarmonyPatch]
		public static class PatchLimits
		{
			[HarmonyPostfix]
			[HarmonyPatch(typeof(Thing), "AddCommand")]
			public static void ExploreCommandRemoveExplorationNeckwarning(Thing __instance, string type, params object[] param)
			{
				if (!(__instance is Npc npc) || !npc.IsPlayerThing || !npc.IsSmartRace)
					return;

				if (HasAdventureNeck(npc) && type == "GoMapExplore")
				{
					RemoveLimitWarning(npc);
				}
			}

			[HarmonyPostfix]
			[HarmonyPatch(typeof(JobBrokenNeck), "OnEnterJob")]
			public static void OnEnterBCToil(JobBrokenNeck __instance, KStateQUnit unit)
			{
				RemoveLimitWarning(__instance.Worker);
			}

			private static void RestoreLimitWarning(Npc npc)
			{
				MessageMgr.Instance.AddMessage(35, new List<Thing> { npc });
				Wnd_MessageBox.Instance.UpdateMessage();
			}

			private static void RemoveLimitWarning(Npc npc)
			{
				var msgs = MessageMgr.Instance.m_mapLevelMsg.Where(x => x._AttributID == 35 && x._ThingID?.Contains(npc.ID) == true).ToList();
				foreach (var msg in msgs)
				{
					
					AddLog("Removed limit msg for " + npc.Name);
					msg._OnlyBox = true;
					//MessageMgr.Instance.RemoveMessage(35, new List<Thing> { npc });
					Wnd_MessageBox.Instance.UpdateMessage();
				}
			}

			[HarmonyPostfix]
			[HarmonyPatch(typeof(Command), "FinishCommand")]
			public static void FinishCommand(Command __instance, bool del = false, bool debug = false, bool mustRemove = false)
			{
				if (!(__instance.OwnerThing is Npc npc) || !(__instance is CommandGoMapExplore cmd))
					return;

				if (HasAdventureNeck(npc))
				{
					RestoreLimitWarning(npc);
				}
			}

				//[HarmonyPostfix]
				//[HarmonyPatch(typeof(CommandGoMapExplore), "CouldBeFind")]
				//public static void StartAdventure(CommandGoMapExplore __instance, Npc npc)
				//{

				//}

			[HarmonyPostfix]
			[HarmonyPatch(typeof(JobLeave2Explore), "OnLeaveJob")]
			public static void RestoreLimitMessageOnFail(JobLeave2Explore __instance, KStateQUnit unit)
			{
				var npc = __instance.Worker;
				if (HasAdventureNeck(npc) && npc.CheckCommand("GoMapExplore", checkcount: true)?.Any(x => x != null) != true)
				{
					RestoreLimitWarning(npc);
				}
			}

			private static bool HasAdventureNeck(Npc npc)
			{
				return npc.PropertyMgr.Practice.TouchNeck && npc.PropertyMgr.Practice.CurNeck?.Kind == g_emGongBottleNeckType.Explore;
			}





			[HarmonyPostfix]
			[HarmonyPatch(typeof(JobBrokenNeck), "OnLeaveJob")]
			public static void RestoreLimitMessageOnFail(JobBrokenNeck __instance, KStateQUnit unit)
			{
				bool fail = __instance.Worker.PropertyMgr.Practice.TouchNeck;

				if (fail)
				{
				//handled by message "failed breakthrough";
					//RestoreLimitWarning(npc);
					//RemoveLimitWarning(__instance.Worker);
				}
			}

		}
	}
}
