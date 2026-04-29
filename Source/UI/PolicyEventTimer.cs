using FairyGUI;
using HarmonyLib;
using KTV;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XiaWorld;
using XiaWorld.UI.InGame;
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

			static int fontSize = 0;

			[HarmonyPostfix]
			[HarmonyPatch(typeof(Wnd_QuickCityWindow), "UpRegionPolicy")]
			public static void UpRegionPolicy(Wnd_QuickCityWindow __instance, OutspreadMgr.Region region, UI_QuickCityItem qbtn)
			{
				if (region == null)
					return;
				try
				{
					OutspreadMgr.Instance.Step(0.00001f);
					qbtn.m_qingxiang.title = PolicyEventString(region, true);
					var txt = qbtn.m_qingxiang.GetTextField();
					txt.singleLine = false;
					if (fontSize == 0)
					{
						fontSize = txt.fontsize - 2;
					}
					txt.fontsize = fontSize;
					//txt.width = 150;
					txt.align = AlignType.Center;
				}
				catch (Exception)
				{

				}
			}

			[HarmonyPostfix]
			[HarmonyPatch(typeof(Wnd_OpenOutsWindow), "UpRegionPolicy")]
			public static void ShowNextEventTime(Wnd_OpenOutsWindow __instance, OutspreadMgr.Region region)
			{
				try
				{
					var btn = __instance.UIMain.m_qingxiang;

					OutspreadMgr.Instance.Step(0.00001f);
					string next = PolicyEventString(region);
					btn.title = next;
					//ShowMessage($"{region.DisplayName} using {region.Policy} next story in  {nextStory} days");
				}
				catch (Exception)
				{
				}
			}

			private static string PolicyEventString(Region region, bool lineBreak = false)
			{
				var nextStory = (region.RegionPolicy.lastPolicyTime + GetPolicyInterval(region)) - World.Instance.TolSecondD;

				string next = OutspreadMgr.Instance.GetPolicyDef(region.Policy)?.DisplayName;
				next += lineBreak ? "\r\n" : " ";
				if (nextStory > 180)
				{
					nextStory /= 600;
					next += $"{nextStory:N1}d";
				}
				else
					next += $"{nextStory:N0}s";
				return next;
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