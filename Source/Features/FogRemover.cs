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
	public class FogRemover : Mod
	{
		public override Meta Info => _info;
		private static Meta _info = new Meta("FogRemover", "Remove Map Fog", false);

		private static bool _doPause = false;

		public FogRemover(bool defaultEnabled) : base(defaultEnabled)
		{
		}

		[HarmonyPatch]
		public static class Patch
		{
			private static Map revealedMap;

			[HarmonyPostfix]
			[HarmonyPatch(typeof(World), "Step")]
			public static void WorldStep(float dt)
			{
				if (!_info.Enabled || revealedMap == World.Instance.map || !World.Instance.IsStart)
					return;


				revealedMap  = World.Instance.map;
				World.Instance.map.SetNoFog();
				MapRender.Instance.Fog.clearFog = true;
			}
		}
	}
}