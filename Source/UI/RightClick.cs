using FairyGUI;
using HarmonyLib;
using JP;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting.Contexts;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using XiaWorld;
using XiaWorld.UI.InGame;

namespace ACS_Yoda_Tweaks
{
	public class RightClick : Mod
	{
		public override Meta Info => _info;
		private static Meta _info = new Meta("RightClick", "Rightclick Support", true);

		private static bool _pauseAfterLoad = false;

		public RightClick(bool defaultEnabled) : base(defaultEnabled)
		{
		}

		[HarmonyPatch]
		public static class Patch
		{
			[HarmonyPrefix]
			[HarmonyPatch(typeof(XiaWorld.UILogicMgr), nameof(XiaWorld.UILogicMgr.OnMapClick))]
			public static bool OnWorldClick(XiaWorld.UILogicMgr __instance, Vector3 pos, int key, int bnt)
			{
				if (GameWatch.Instance.Mode != g_emGameMode.HardCore && GameWatch.Instance.Mode != g_emGameMode.Normal)
					return true;

				if (bnt != 1)
					return true;

				var me = (UILogicMgr.Instance.GetCurMode() as UILogicMode_Select)?.CurSelectThing as Npc;

				if (me == null || me.FightBody.IsFighting || !me.IsRealPlayerThing)
					return true;

				Collider2D[] array = Physics2D.OverlapPointAll(pos, 1024);
				Collider2D[] array2 = array;

				foreach (Collider2D collider2D in array2)
				{
					if (collider2D != null)
					{
						Npc npc = collider2D.GetComponentInParent<NpcView>().npc;
						if (npc.IsSelectAble && !npc.IsRealPlayerThing && !npc.FightBody.IsFighting && npc.IsSmartRace)
						{
							GameWatch.Instance.PlayUIAudio("Sound/UI/clicknpc");
							me.AddCommand("TryTalk", npc);
							ReactiveDisciples.TryCancelJob(me);
							return false;
						}
					}
				}

				foreach (var thing in World.Instance.map.Things.GetThingsAtGrid(key))
				{
					if (thing is ItemThing itm && itm.Camp == XiaWorld.Fight.g_emFightCamp.Player)
					{
						me.AddCommand("EquipItem", itm);
						ReactiveDisciples.TryCancelJob(me);
						GameWatch.Instance.PlayUIAudio("Sound/UI/click");
						return false;
					}
					else if (thing is BuildingThing building)
					{
						//AddLog(building.def.Name);
						if (building.def.Name == "Building_BookShelf_CangJing")
						{
							if (me.CanDoMagic() && me.Rank == g_emNpcRank.Disciple && me.GongKind == g_emGongKind.Dao)
								Wnd_CangJingGeWindow.Instance.Show(building, 1, me);
						}
						else if (building.BuildingState >= g_emBuildingState.Working && building.TagData.CheckTag("Practice") > 0)
						{
							if (building.CheckOwner(me))
								return false;

							var isBed = building.def.Building.IsBed > 0;
							if (building.Owners?.Count > 0)
							{
								foreach (var o in building.Owners.ToArray())
								{
									if (isBed)
										o.SetBed(null);
									else
										o.SetPracticePlace(null);
								}
							}

							if (isBed)
								me.SetBed(building);
							else if (me.Rank == g_emNpcRank.Disciple)
								me.SetPracticePlace(building);

							GameWatch.Instance.PlayUIAudio("Sound/UI/click");
							return false;
						}
					}
				}

				return true;
			}
		}
	}
}
