using ACS_Yoda_Tweaks;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XiaWorld;


public class SmeltManual : Mod
{
	public override Meta Info => _info;
	private static Meta _info = new Meta("SmeltManual", "Disable Mentor Guard", false);

	public SmeltManual(bool defaultEnabled) : base(defaultEnabled)
	{
	}

	[HarmonyPatch]
	public static class SmeltManualPatch
	{
		[HarmonyPrefix]
		[HarmonyPatch(typeof(JobCangJingGe), "OnLeaveJob")]
		public static void Prefix(JobCangJingGe __instance, KStateQUnit unit)
		{
			if (!unit.IsFinished)
				return;
			var itm = __instance.Worker.PutDownItem();
			if (itm == null)
				return;

			CommandMgr.Instance.AddCommand("Melting", itm);
		}
	}
}

