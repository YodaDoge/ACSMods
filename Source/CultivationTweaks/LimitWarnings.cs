using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XiaWorld;

namespace ACS_Yoda_Tweaks
{
	public partial class CultivationTweaks : Mod
	{

		[HarmonyPatch]
		public static partial class Patch
		{
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
			public static void OnExitBCToil(JobBrokenNeck __instance, KStateQUnit unit)
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
