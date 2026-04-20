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
			[HarmonyPostfix]
			[HarmonyPatch(typeof(NpcPractice), "Up2Disciple")]
			public static void Up2Disciple(JobTakeRest __instance)
			{
				var npc = __instance.Worker;
				if (!npc.IsSmartRace)
					return;
				if (npc.MyBed != null)
				{
					var beds = CountBedsInRoom(npc.MyBed);
					if (beds == 1)
						return;
					else
						npc.SetBed(null);
				}
				FindFittingBed(npc);
			}

			[HarmonyPrefix]
			[HarmonyPatch(typeof(JobTakeRest), "GetToilList")]
			public static void SmarterBedFind(JobTakeRest __instance)
			{
				var npc = __instance.Worker;
				if (!npc.IsSmartRace)
					return;
				if (npc.MyBed == null)
				{
					FindFittingBed(npc);
				}
			}

			private static void FindFittingBed(Npc npc)
			{
				//npc.map.Things.FindBuildingForTag
				var bed = npc.map.Things.FindBuilding(npc, 1000, "Sleep", 0, needworkspace: true, false, 0, 9999, (BuildingThing t) =>
				{
					bool normal = t.HaveFreeOwner() && GameDefine.CanShareThisBed(t, npc) > 0;
					if (normal)
					{
						int sleepBuildings = CountBedsInRoom(t);
						bool needSingleRoom = npc.Rank > g_emNpcRank.Worker;
						bool isFit = needSingleRoom ? sleepBuildings == 1 : sleepBuildings > 1;
						return isFit;
					}
					return normal;
				}, null, checkowner: false);

				if (bed != null)
				{
					npc.SetBed(bed);
				}
				else
					ShowMessage($"{npc.Rank} {npc.GetName()} couldnt find a bed");
			}

			private static int CountBedsInRoom(BuildingThing t)
			{
				return t.AtRoom.m_lisThingsInRoom.Count(x => x.TagData.CheckTag("Sleep") > 0);
			}
		}
	}

}