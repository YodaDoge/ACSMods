using FairyGUI;
using HarmonyLib;
using rail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using XiaWorld;
using XiaWorld.LSBTree;
using static ACS_Yoda_Tweaks.Mod;

namespace ACS_Yoda_Tweaks
{
	public class AmbientLightMod : Mod
	{
		public override Meta Info => _info;
		private static Meta _info = new Meta("AmbientLight", "Brighter Daylight", false, x => Patch.Toggle(x.Enabled));

		public AmbientLightMod(bool defaultEnabled) : base(defaultEnabled)
		{
		}

		[HarmonyPatch]
		public static class Patch
		{
			internal static void Toggle(bool enabled)
			{
				if (enabled && World.Instance != null)
				{
					UpdateAmbientLight(World.Instance);
				}
				else if (World.Instance != null)
				{
					RenderSettings.ambientSkyColor = defaultLight;
				}
			}


			public static bool Disabled = false;
			static float skyLightDay = 0.45f;
			static float skyLightNight = 0.35f;

			static float seasonTintStrength = 0.03f;

			static float seasonTintStrengthNightFactor = 0.2f;
			static float seasonTintStrengthDayFactor = 0.7f;

			//TODO: Winter total darker => negative offset; Summer generally brighter => all positive offset
			private static readonly Color[] SeasonalLightTints = new Color[]
			{
				new Color(0, seasonTintStrength, 0),      // Spring (Green)
				new Color(seasonTintStrength, seasonTintStrength, 0),   //  Summer (Yellow)
				new Color(seasonTintStrength, 0, 0),       //  Autumn (Red)
				new Color(seasonTintStrength*0.2f, seasonTintStrength*0.2f, seasonTintStrength *0.7f)      //  Winter (Blue)
			};

			private static float secsPerHour = 600f / 24f;
			private static float dawnStart = 5.6f * secsPerHour;
			private static float dawnEnd = 8.10f * secsPerHour;

			private static float duskStart = 17.8f * secsPerHour;
			private static float duskEnd = 21.0f * secsPerHour;

			private static int lastSec;
			public static readonly Color defaultLight = new Color(0.35f, 0.35f, 0.35f);

			[HarmonyPostfix]
			[HarmonyPatch(typeof(World), "Step")]
			public static void Postfix(float dt, World __instance)
			{
				//KLog.Dbg($"Ambilight Hit { _info.Enabled} color {RenderSettings.ambientSkyColor}");
				if (!_info.Enabled) return;

				var daySecond = __instance.DaySecond;
				var now = (int)daySecond;
				if (now == lastSec)
					return;
				lastSec = now;
				UpdateAmbientLight(__instance);
			}

			public static void UpdateAmbientLight(World __instance)
			{
				float daySecond = __instance.DaySecond;
				var t = CalcT(daySecond);

				var val = Mathf.SmoothStep(skyLightNight, skyLightDay, t);
				var dayCycleColour = Color.white * val;

				var seasonIdx = (int)(__instance.Weather.GetSeason() - 1);
				var day = __instance.DayCount + 1;
				var seasonTint = GetSeasonTint(seasonIdx, day);

				seasonTint *= Mathf.SmoothStep(seasonTintStrengthNightFactor, seasonTintStrengthDayFactor, t);
				dayCycleColour += seasonTint;

				RenderSettings.ambientSkyColor = dayCycleColour;
			}

			private static float CalcT(float daySecond)
			{
				// 1. Dawn: Fades 0.0 -> 1.0
				if (daySecond >= dawnStart && daySecond <= dawnEnd)
				{
					return (daySecond - dawnStart) / (dawnEnd - dawnStart);
				}

				// 2. Full Day: Constant 1.0
				if (daySecond > dawnEnd && daySecond < duskStart)
				{
					return 1f;
				}

				// 3. Dusk: Fades 1.0 -> 0.0
				if (daySecond >= duskStart && daySecond <= duskEnd)
				{
					float t = (daySecond - duskStart) / (duskEnd - duskStart);
					return 1f - t;
				}

				// 4. Night: Constant 0.0
				return 0f;
			}

			public static Color GetSeasonTint(int season, int day)
			{
				float daysPerSeason = 28f;
				float midPoint = 15f;

				// 1. Calculate how many days we are past the current peak (Day 14)
				// If day is 14, diff is 0. If day is 28, diff is 14. If day is 1, diff is 15.
				float diff = (day - midPoint + daysPerSeason) % daysPerSeason;

				// 2. Convert to 0.0 - 1.0 progress between this peak and the next
				float t = diff / daysPerSeason;

				// 3. Get the two colors to blend between
				Color currentPeak = SeasonalLightTints[season];
				Color nextPeak = SeasonalLightTints[(season + 1) % 4];

				return new Color(
					Mathf.SmoothStep(currentPeak.r, nextPeak.r, t),
					Mathf.SmoothStep(currentPeak.g, nextPeak.g, t),
					Mathf.SmoothStep(currentPeak.b, nextPeak.b, t)
				);
			}

			[HarmonyPatch(typeof(Wnd_GameMain), "__clickFengshui")]
			public static void Postfix(EventContext context, Wnd_GameMain __instance)
			{
				//if kept enbabled feng shui mode is too bright
				if (__instance.openFengshui && _info.Enabled)
					Toggle(false);
			}
		}
	}
}