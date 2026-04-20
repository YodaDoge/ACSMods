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
	public class CopyBuildThing : Mod
	{
		public override Meta Info => _info;
		private static Meta _info = new Meta("BuildingCopy", "BuildingCopy", true);

		private static bool _doPause = false;

		public CopyBuildThing(bool defaultEnabled) : base(defaultEnabled)
		{
		}

		[HarmonyPatch]
		public static class Patch
		{
			private static BuildingThing _lastSelectThing;

			[HarmonyPrefix]
			[HarmonyPatch(typeof(UILogicMode_Select), "OnModeLeave")]
			public static void CopyBuildData(UILogicMode_Select __instance, Thing ___lastthing)
			{
				if (__instance.CurSelectThing != null && __instance.CurSelectType == g_emSelectThingSort.Building && __instance.SelectThings.Count == 1)
				{
					if (__instance.CurSelectThing is BuildingThing building)
					{
						if (building.IsPlayerThing)
						{
							_lastSelectThing = building;
							return;
						}
					}
				}
				_lastSelectThing = null;
			}

			[HarmonyPrefix]
			[HarmonyPatch(typeof(UILogicMode_Build), "OnModeEnter")]
			public static void PasteBuildData(UILogicMode_Build __instance, ref ThingDef ___LastStuff, ref BuildingThing ___MoveBuilding, params object[] objs)
			{
				if (_lastSelectThing == null)
					return;
				if (objs.Length == 0)
					___MoveBuilding = null;
				___LastStuff = _lastSelectThing.StuffDef;
				__instance.LastBuildName = _lastSelectThing.def.Name;
			}
		}
	}
}