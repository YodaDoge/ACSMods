using ACS_Yoda_Tweaks;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XiaWorld;


public class MasterNoBreakGuard : Mod
{
	public override Meta Info => _info;
	private static Meta _info = new Meta("MasterNoBreakGuard", "Mentor won't guard", false);
	
	[HarmonyPatch]
	public static class Patch
	{
		private static bool first = true;

		[HarmonyPrefix]
		[HarmonyPatch(typeof(BehaviourMasterHelp), nameof(BehaviourMasterHelp.Check))]
		static bool Prefix(BehaviourMasterHelp __instance, Npc npc, int seachr, bool tryfind, ref JobBase __result)
		{
			if (!_info.Enabled) return true;
			__result = null;

			if (first)
			{
				first = false;
				KLog.Dbg("YodaDoge Tweaks disabled Mentor Guard");
			}

			// Skip
			return false;
		}
	}
}

