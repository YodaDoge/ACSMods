using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using XiaWorld;

namespace ACS_Yoda_Tweaks
{
	public class AutoPause : Mod
	{
		public override Meta Info => _info;
		private static Meta _info = new Meta("AutoPause", "Auto Pause on Load", false);

		private static bool _pauseAfterLoad = false;

		public AutoPause(bool defaultEnabled) : base(defaultEnabled)
		{
		}

		[HarmonyPatch]
		public static class Patch
		{
			[HarmonyPostfix]
			[HarmonyPatch(typeof(Wnd_GameMain), "OnInit")]
			public static void OnInit_Postfix(Wnd_GameMain __instance)
			{
				if (!_info.Enabled) return;

				_pauseAfterLoad = true;
				if (MainManager.Instance != null)
					MainManager.Instance.Pause();
			}

			[HarmonyPostfix]
			[HarmonyPatch(typeof(Wnd_GameMain), "OnUpdate")]
			public static void OnUpdate_Postfix(Wnd_GameMain __instance)
			{
				if (!_info.Enabled) return;

				if ((_pauseAfterLoad || !Application.isFocused) && MainManager.Instance != null)
				{
					MainManager.Instance.Pause();
					_pauseAfterLoad = false;
				}

			}
		}
	}
}