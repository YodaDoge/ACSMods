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
	public class Everywhere : Mod
	{
		public override Meta Info => _info;
		private static Meta _info = new Meta("Everywhere", "Map wide branch area", false);

		private static bool _pauseAfterLoad = false;

		public Everywhere(bool defaultEnabled) : base(defaultEnabled)
		{
		}

		[HarmonyPatch]
		public static class Patch
		{
			private static Dictionary<AreaTang, List<int>> areaToNpcs = new Dictionary<AreaTang, List<int>>();

			[HarmonyPrefix]
			[HarmonyPatch(typeof(AreaTang), "Step")]
			public static void Step(AreaTang __instance, ref List<int> ___Npcs, float dt)
			{
				if (!_info.Enabled) return;
				try
				{
					if (__instance.T >= 5f - 0.01f)
					{
						foreach (var item in ThingMgr.Instance.NpcList.Where(x => x.TangJoined == __instance.BindTang).Select(x => x.ID))
						{
							if (!___Npcs.Contains(item))
								___Npcs.Add(item);
						}
					}

				}
				catch (Exception ex)
				{
					ShowMessage(ex.ToString());
				}
			}

			[HarmonyPrefix]
			[HarmonyPatch(typeof(AreaTang), "OnNpcLeaveArea")]
			public static bool OnNpcLeaveArea(Npc npc, int key)
			{
				if (!_info.Enabled) return true;
				return false;
			}
		}
	}
}