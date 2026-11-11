using FairyGUI;
using HarmonyLib;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Diagnostics;
using XiaWorld;
using XiaWorld.UI.InGame;
using static CSCallLua;

namespace ACS_Yoda_Tweaks
{
	public class MiscFix : Mod
	{
		public override Meta Info => _info;
		private static Meta _info = new Meta("MiscFix", "", true);

		public MiscFix(bool defaultEnabled) : base(defaultEnabled)
		{
		}

		[HarmonyPatch]
		public static class Patch
		{
			[HarmonyPostfix]
			[HarmonyPatch(typeof(Wnd_GameDIY), "_RefreshEnv")]
			public static void OnShowUpdate(Wnd_GameDIY __instance, UI_WindowGameDiy ___UIInfo)
			{
				___UIInfo.m_Evn.m_n239.title = World.Instance.map.Seed;
			}

			[HarmonyPrefix]
			[HarmonyPatch(typeof(JobLeaveMap), "OnCheckFaildCondition")]
			public static bool FixPlayerCharactersStillLeaving(JobLeaveMap __instance, ref bool __result)
			{
				if (__instance.Worker.IsRealPlayerThing)
				{
					var interrupt = __instance.InterruptJob(checkInterrupt: false, forceSuccess: true);
					__instance.Worker.Leave = 0;
					AddLog($"{__instance.Worker.GetName()} cancels leave {interrupt} ");
					__result = false;
					return false;
				}
				return true;
			}
		}
	}
}