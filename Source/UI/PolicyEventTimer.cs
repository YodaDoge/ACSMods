using FairyGUI;
using HarmonyLib;
using KTV;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XiaWorld;
using static XiaWorld.AuctionData;
using static XiaWorld.OutspreadMgr;

namespace ACS_Yoda_Tweaks
{
	public class PolicyEventTimer : Mod
	{
		public override Meta Info => _info;
		private static Meta _info = new Meta("AgencyEventTimer", "Show next policy event time", false);

		private static bool _pauseAfterLoad = false;

		public PolicyEventTimer(bool defaultEnabled) : base(defaultEnabled)
		{
		}

		[HarmonyPatch]
		public static class Patch
		{
			[HarmonyPostfix]
			[HarmonyPatch(typeof(Wnd_OpenOutsWindow), "SetFouse")]
			public static void FocusUpdate(Wnd_OpenOutsWindow __instance, EventContext context, ref OutspreadMgr.Region ___region)
			{
				ShowNextEventTime(__instance, ___region);
			}

			[HarmonyPostfix]
			[HarmonyPatch(typeof(Wnd_OpenOutsWindow), "UpRegionPolicy")]
			public static void ShowNextEventTime(Wnd_OpenOutsWindow __instance, OutspreadMgr.Region region)
			{
				try
				{
					OutspreadMgr.Instance.Step(0.0001f);
					var nextStory = (region.RegionPolicy.lastPolicyTime + GetPolicyInterval(region)) - World.Instance.TolSecondD;

					string next = OutspreadMgr.Instance.GetPolicyDef(region.Policy)?.DisplayName;
					if (nextStory > 180)
					{
						nextStory /= 600;
						next += $" {nextStory:N1}d";
					}
					else
						next += $" {nextStory:N0}s";

					__instance.UIMain.m_qingxiang.title = next;

					//ShowMessage($"{region.DisplayName} using {region.Policy} next story in  {nextStory} days");
				}
				catch (Exception)
				{
				}
			}

			private static float specFocusInterval;
			private static float Policy_Interval_Scale;
			private static Dictionary<string, float> PolicyInterval;

			private static float GetPolicyInterval(Region region)
			{
				float num = 0f;
				num = ((!region.SpecialFocus) ? PolicyInterval[region.Policy ?? string.Empty] : specFocusInterval);
				return num * Policy_Interval_Scale;
			}

			[HarmonyPostfix]
			[HarmonyPatch(typeof(OutspreadMgr), "OnStep")]
			public static void ShowRegion(OutspreadMgr __instance, ref Dictionary<string, float> ___PolicyInterval, float ___specFocusInterval, float dt)
			{
				specFocusInterval = ___specFocusInterval;
				Policy_Interval_Scale = __instance.Policy_Interval_Scale;
				PolicyInterval = ___PolicyInterval;
			}

		}
	}
}