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
		private static Meta _info = new Meta("AutoPause", "Auto Pause on Load", true);

		private static bool _doPause = false;
		
		[HarmonyPatch]
		public static class Patch
		{
			[HarmonyPostfix]
			[HarmonyPatch(typeof(Wnd_GameMain), "OnInit")]
			public static void OnInit_Postfix(Wnd_GameMain __instance)
			{
				if (!_info.Enabled) return;

				_doPause = true;
				if (MainManager.Instance != null)
					MainManager.Instance.Pause();
			}

			[HarmonyPostfix]
			[HarmonyPatch(typeof(Wnd_GameMain), "OnUpdate")]
			public static void OnUpdate_Postfix(Wnd_GameMain __instance)
			{
				if (!_info.Enabled) return;

				if (_doPause && MainManager.Instance != null)
				{
					MainManager.Instance.Pause();
					_doPause = false;
				}

			}
		}
		//[HarmonyPatch(typeof(GameMain), "GameStart")]
		//public static class MainManager_Run_Patch
		//{
		//	[HarmonyPostfix]
		//	public static void Postfix(GameMain __instance, bool test = false)
		//	{
		//		KLog.Dbg("Pause after game load");
		//		if(MainManager.Instance != null)
		//			MainManager.Instance.Pause();
		//	}
		//}
	}
}