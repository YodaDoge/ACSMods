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
				if (!_info.Enabled || World.Instance.TolSecond <= 3f) 
				return;

				_pauseAfterLoad = true;
			}
			static int ticks = 0;
			static int tickUntilPause = 10;
			[HarmonyPostfix]
			[HarmonyPatch(typeof(MainManager), "Step")]
			public static void OnUpdate_Postfix(float dt)
			{
				if (!_info.Enabled) 
					return;

				if (_pauseAfterLoad && ticks < tickUntilPause)
				{
					ticks++;
					return;
				}

				if (_pauseAfterLoad)
				{
					MainManager.Instance.Pause();
					_pauseAfterLoad = false;
					ticks = 0;
				}

				if (Application.isFocused && _focusLossPause)
				{ 
					_focusLossPause = false;
					MainManager.Instance.Run();
				}

				if (!Application.isFocused && MainManager.Instance.Runing)
				{
					MainManager.Instance.Pause();
					_focusLossPause = true;
				}
			}
			private static bool _focusLossPause = false;
		}
	}
}