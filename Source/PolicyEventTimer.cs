using FairyGUI;
using HarmonyLib;
using KTV;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XiaWorld;
using static XiaWorld.OutspreadMgr;

namespace ACS_Yoda_Tweaks
{
	public class PolicyEventTimer : Mod
	{
		public override Meta Info => _info;
		private static Meta _info = new Meta("AgencyEventTimer", "Show time to next policy event", false);

		private static bool _pauseAfterLoad = false;

		public PolicyEventTimer(bool defaultEnabled) : base(defaultEnabled)
		{
		}

		[HarmonyPatch]
		public static class Patch
		{
			private const string Name = "PolicyEventTimer";

			[HarmonyPostfix]
			[HarmonyPatch(typeof(Wnd_OpenOutsWindow), "OnInit")]
			public static void OnInit_Postfix(Wnd_OpenOutsWindow __instance)
			{
				try
				{
					var lbl = new GRichTextField();
					lbl.text = "unkown";
					lbl.name = Name;

					__instance.UIInfo.AddChild(lbl);
				}
				catch (Exception ex)
				{
					ShowMessage(ex);
				}
			}

			[HarmonyPostfix]
			[HarmonyPatch(typeof(Wnd_OpenOutsWindow), "UpRegionPolicy")]
			public static void UpRegionPolicy(Wnd_OpenOutsWindow __instance, Region region)
			{
				ShowNextEventTime(__instance, region, region?.RegionName);
			}

			[HarmonyPostfix]
			[HarmonyPatch(typeof(Wnd_OpenOutsWindow), "ShowRegion")]
			public static void ShowNextEventTime(Wnd_OpenOutsWindow __instance, OutspreadMgr.Region ___region, string regionname)
			{
				try
				{
					var lbl = __instance.UIInfo.GetChild(Name);
					lbl.text = "?";

					var nextStory = (___region.RegionPolicy.lastPolicyTime + GetPolicyInterval(___region)) - World.Instance.TolSecondD;

					if (nextStory > 180)
					{
						nextStory /= 600;
						lbl.text = $"{nextStory:N1} d";
					}
					else
						lbl.text = $"{nextStory:N0} s";

					var target = __instance.UIMain.m_qingxiang.position;
					lbl.SetPosition(target.x - 85, target.y + 40 , target.z - 1);
					//ShowMessage($"{region.DisplayName} using {region.Policy} next story in  {nextStory} days");
				}
				catch (Exception ex)
				{
					ShowMessage(ex);
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