using ACS_Yoda_Tweaks;
using FairyGUI;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XiaWorld;
using XiaWorld.LSBTree;
using XiaWorld.ThingStep;
using XiaWorld.UI.InGame;
using static ACS_Yoda_Tweaks.Mod;
using static XiaWorld.AuctionNpc;

public class SpiritAnimalPlayFix : Mod
{
	public override Meta Info => _info;
	private static Meta _info = new Meta("SpiritAnimalPlayFix", "Fix Spirit Animal Leisure", true);
	
	[HarmonyPatch]
	public static class Patch
	{
		private static HashSet<string> _funjobs = new HashSet<string>()
		{
			"JobLsFun", "JobLsHelpPlay", "JobLsFollow", "JobAtk4Fun"
		};

		[HarmonyPatch(typeof(JobBase), "OnStepJob")]
		public static void Postfix(float dt, KStateQUnit unit, JobBase __instance)
		{
			if (!_info.Enabled) return;

			if (__instance.Worker.IsLingShou && _funjobs.Contains(__instance.jobdef.Name))
			{
				bool isWorkingFuntype = __instance is JobLsFun jl && jl.lsFunType != "Lay"; //lay is already working
				if (!isWorkingFuntype)
					__instance.Worker.Needs.AddNeedValue(g_emNeedType.Fun, dt);
			}
		}
	}

}