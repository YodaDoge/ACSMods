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
using XiaWorld.Modifier;
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
			private static HashSet<Npc> _players = new HashSet<Npc>();

			[HarmonyPostfix]
			[HarmonyPatch(typeof(Modifier_Vistor), "OnEnter")]
			public static void Vistor(Modifier_Vistor __instance)
			{
				var npc = __instance.Mgr.me;
				ShowMessage($"{npc.GetName()} state {npc.PropertyMgr.Practice.GongStateLevel}");
				FindFittingBed(npc);
			}

			[HarmonyPostfix]
			[HarmonyPatch(typeof(NpcPractice), "Up2Disciple")]
			public static void Up2Disciple(Npc ___me)
			{
				try
				{
					var npc = ___me;
					if (!npc.IsSmartRace || !npc.IsRealPlayerThing)
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
				catch (Exception ex)
				{
					ShowMessage(ex);
				}
			}

			[HarmonyPrefix]
			[HarmonyPatch(typeof(JobTakeRest), "GetToilList")]
			public static void SmarterBedFind(JobTakeRest __instance)
			{
				var npc = __instance.Worker;
				if (!npc.IsSmartRace || !npc.IsPlayerThing)
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
						bool needSingleRoom = npc.Rank > g_emNpcRank.Worker && npc.PropertyMgr.Practice.GongStateLevel >= g_emGongStageLevel.None;
						bool isFit = needSingleRoom ? sleepBuildings == 1 : sleepBuildings > 1;
						return isFit;
					}
					return normal;
				}, null, checkowner: false);

				if (bed != null)
				{
					npc.SetBed(bed);
				}
			}

			private static int CountBedsInRoom(BuildingThing t)
			{
				return t.AtRoom?.m_lisThingsInRoom.Count(x => x.TagData.CheckTag("Sleep") > 0) ?? 50; 
			}
		}
	}

}