using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
			[HarmonyPatch(typeof(CommandGoMapExplore), "CouldBeFind")]
			public static void StartAdventure(CommandGoMapExplore __instance, Npc npc)
			{
				if (HasAdventureNeck(npc))
				{
					var msg = MessageMgr.Instance.m_mapLevelMsg.FirstOrDefault(x => x._AttributID == 35 && x._ThingID?.Contains(npc.ID) == true);
					if (msg != null)
					{
						MessageMgr.Instance.RemoveMessage(35, new List<Thing> { npc });
						Wnd_MessageBox.Instance.UpdateMessage();
					}
				}
			}

			[HarmonyPostfix]
			[HarmonyPatch(typeof(JobLeave2Explore), "OnLeaveJob")]
			public static void RestoreLimitMessageOnFail(JobLeave2Explore __instance, KStateQUnit unit)
			{
				var npc = __instance.Worker;
				if (HasAdventureNeck(npc) && npc.CheckCommand("CommandGoMapExplore", checkcount:true)?.Any(x => x != null) != true)
				{
					MessageMgr.Instance.AddMessage(35, new List<Thing> { npc });
					Wnd_MessageBox.Instance.UpdateMessage();
				}
			}

			private static bool HasAdventureNeck(Npc npc)
			{
				return npc.PropertyMgr.Practice.TouchNeck && npc.PropertyMgr.Practice.CurNeck?.Kind == g_emGongBottleNeckType.Explore;
			}

			[HarmonyPostfix]
			[HarmonyPatch(typeof(JobPaintCharm), "OnLeaveJob")]
			public static void ShorterTalismanNames(JobPaintCharm __instance, KStateQUnit unit)
			{
				if (IsYodaMachine)
				{
					var itm = ThingMgr.Instance.FindThingByID((__instance.CMD as CommandPaintCharm).CharmItemId) as ItemThing;

					var name = itm.GetName().Replace("Talisman of", string.Empty)
											.Replace(" Talisman", string.Empty)
											.Replace("Illustration of ", string.Empty)
											.Replace("Everlasting", "Lasting")
											.Trim();
					itm.SetName(name);
				}
			}

			[HarmonyPostfix]
			[HarmonyPatch(typeof(JobBrokenNeck), "OnEnterJob")]
			public static void OnEnterBCToil(JobBrokenNeck __instance, KStateQUnit unit)
			{
				if (MessageMgr.UseOldMessage)
				{
					MsgMgr.Instance.RemoveMsg(26, __instance.Worker);
				}
				else
				{
					MessageMgr.Instance.RemoveMessage(35, new List<Thing> { __instance.Worker });
				}

			}

			[HarmonyPostfix]
			[HarmonyPatch(typeof(JobBrokenNeck), "OnLeaveJob")]
			public static void RestoreLimitMessageOnFail(JobBrokenNeck __instance, KStateQUnit unit)
			{
				bool fail = __instance.Worker.PropertyMgr.Practice.TouchNeck;

				if (fail)
				{
					if (MessageMgr.UseOldMessage)
					{
						MsgMgr.Instance.AddMsg(26, __instance.Worker);
					}
					else
					{
						MessageMgr.Instance.AddMessage(35, new List<Thing> { __instance.Worker });
					}
				}
			}

		}
	}
}
