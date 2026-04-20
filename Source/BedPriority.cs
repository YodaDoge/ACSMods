using FairyGUI;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Linq;
using UnityEngine;
using XiaWorld;
using XiaWorld.UI.InGame;
using static XiaWorld.JianghuMgr;

namespace ACS_Yoda_Tweaks
{
	public class BedPriority : Mod
	{
		public override Meta Info => _info;
		private static Meta _info = new Meta("BedPriority", "Smarter Bed Selection", true);

		private static bool _pauseAfterLoad = false;

		public BedPriority(bool defaultEnabled) : base(defaultEnabled)
		{
		}

		[HarmonyPatch]
		public static class Patch
		{
			[HarmonyPrefix]
			[HarmonyPatch(typeof(JobTakeRest), "GetToilList")]
			public static void SmarterBedFind(JobTakeRest __instance)
			{
				if (!__instance.Worker.IsSmartRace)
					return;

				if (__instance.Worker.MyBed == null)
				{
					bool needSingleRoom = __instance.Worker.Rank > g_emNpcRank.Worker;
					//__instance.Worker.map.Things.FindBuildingForTag
					var bed = __instance.Worker.map.Things.FindBuilding(__instance.Worker, 1000, "Sleep", 0, needworkspace: true, false, 0, 9999, (BuildingThing t) =>
					{
						bool normal = t.HaveFreeOwner() && GameDefine.CanShareThisBed(t, __instance.Worker) > 0;
						if (normal)
						{
							var sleepBuildings = t.AtRoom.m_lisThingsInRoom.Count(x => x.TagData.CheckTag("Sleep") > 0);
							bool isFit = needSingleRoom ? sleepBuildings == 1 : sleepBuildings > 1;
							return isFit;
						}
						return normal;
					}, null, checkowner: false);

					if (bed != null)
					{
						__instance.Worker.SetBed(bed);
					}
					else
						ShowMessage($"{__instance.Worker.Rank} {__instance.Worker.GetName()} couldnt find a bed");
				}
			}


		}
	}

}