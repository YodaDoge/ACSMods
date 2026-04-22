using FairyGUI;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting.Contexts;
using System.Text;
using UnityEngine;
using XiaWorld;
using XiaWorld.UI.InGame;

namespace ACS_Yoda_Tweaks
{
	public class SkipCharacterPrompt : Mod
	{
		public override Meta Info => _info;
		private static Meta _info = new Meta("BetterCharSelect", "Always use solo disciple", true);

		private static bool _pauseAfterLoad = false;

		public SkipCharacterPrompt(bool defaultEnabled) : base(defaultEnabled)
		{
		}

		[HarmonyPatch]
		public static class Patch
		{
			[HarmonyPostfix]
			[HarmonyPatch(typeof(Wnd_SelectNpc), "OnShowUpdate")]
			[HarmonyPriority(1000)]
			public static void OnShowUpdate(Wnd_SelectNpc __instance, List<Npc> ___npcs, params object[] objs)
			{
				if (___npcs.Count == 1)
				{
					try
					{
						var lst = __instance.UIInfo.m_n25;
						lst.AddSelection(0, false, true);
						__instance.UIInfo.m_n27.onClick.Call();
					}
					catch (Exception ex)
					{
						ShowMessage(ex);
					}

				}
			}
		}
	}
}