using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using XiaWorld;

namespace ACS_Yoda_Tweaks
{
	public class AutoTalisman : Mod
	{
		public override Meta Info => _info;
		private static Meta _info = new Meta("AutoTalisman", "AutoTalisman", false);
		public AutoTalisman(bool defaultEnabled) : base(defaultEnabled)
		{
		}

		[HarmonyPatch]
		public static class Patch
		{
			[HarmonyPostfix]
			[HarmonyPatch(typeof(JobLeave2Explore), "OnEnterJob")]
			public static void EquipAdventureFu(JobLeave2Explore __instance, KStateQUnit unit)
			{
				ToggleTalisman(__instance.Worker, TaliType.Adventure);
			}

			[HarmonyPostfix]
			[HarmonyPatch(typeof(JobBrokenNeck), "OnEnterJob")]
			public static void EquipBCFu(JobBrokenNeck __instance, KStateQUnit unit)
			{
				var npc = __instance.Worker;
				var practice = npc.PropertyMgr.Practice;
				if (practice.CurNeck.Kind == g_emGongBottleNeckType.Gold)
					return;
				var type = practice.CurNeck.Kind == g_emGongBottleNeckType.Thunder ? TaliType.Battle : TaliType.Breakthrough;
				ToggleTalisman(npc, type);
			}

			[HarmonyPostfix]
			[HarmonyPatch(typeof(JobPracticeSkill), "OnEnterJob")]
			public static void JobPracticeSkill(JobPracticeSkill __instance, KStateQUnit unit)
			{
				ToggleTalisman(__instance.Worker, TaliType.Cultivation);
			}

			[HarmonyPostfix]
			[HarmonyPatch(typeof(JobAbsorbLing), "OnEnterJob")]
			public static void JobAbsorbLing(JobAbsorbLing __instance, KStateQUnit unit)
			{
				ToggleTalisman(__instance.Worker, TaliType.Cultivation);
			}

			[HarmonyPostfix]
			[HarmonyPatch(typeof(JobPractice), "OnEnterJob")]
			public static void JobPractice(JobPractice __instance, KStateQUnit unit)
			{
				var npc = __instance.Worker;
				var practice = npc.PropertyMgr.Practice;
				if (practice.GongStateLevel >= g_emGongStageLevel.God)
					return;
				ToggleTalisman(__instance.Worker, TaliType.Cultivation);
			}

			[HarmonyPostfix]
			[HarmonyPatch(typeof(JobFight), "OnEnterJob")]
			public static void EquipBattleFu(JobFight __instance, KStateQUnit unit)
			{
				ToggleTalisman(__instance.Worker, TaliType.Battle);
			}

			public static void ToggleTalisman(Npc npc, TaliType wantedType)
			{
				try
				{
					if (!npc.IsRealPlayerThing)
						return;

					var talismans = MindfulDresser.Patch.GetTali(npc).ToList();
					
					int maxActiveFu = 3 + npc.AddActiveFuCount + RuntimeVar.Var.ExtraFuActive;
					maxActiveFu = Mathf.Clamp(maxActiveFu, 0, 6);

					maxActiveFu -= talismans.Where(x => npc.Equip.CheckActive(x.Value)).Count(x =>
					{
						var t = GetFuType(npc, x.Key);
						return t == wantedType || t == TaliType.Ignore;
					});

					var activeUnwanted = talismans.Where(x =>
													GetFuType(npc, x.Key) != wantedType &&
													GetFuType(npc, x.Key) != TaliType.Ignore &&
													npc.Equip.CheckActive(x.Value));
					
					foreach (var item in activeUnwanted)
						npc.Equip.CloseItemthing(item.Key, item.Value);

					var inActiveWanted = talismans.Where(x => GetFuType(npc, x.Key) == wantedType && !npc.Equip.CheckActive(x.Value));
					foreach (var item in inActiveWanted.Take(maxActiveFu))
						npc.Equip.ActiveItemThing(item.Key, item.Value);
				}
				catch (Exception ex)
				{
					ShowMessage(ex);
				}
			}
		}

		public enum TaliType
		{
			Cultivation,
			Adventure,
			Battle,
			Breakthrough,
			Ignore,
			None
		}

		private static string[] BCProbs = new string[] { "MindState" };
		private static string[] CultivationProbs = new string[] { "MindState", "PracticeSpeed" };
		private static string[] BattleProps = new string[] { "Shield", "Fabao", "Spell", "Artifact" };
		private static string[] AdventureProps = new string[] { "FindSpeed", "WorldMapFly" };

		private static bool IsMatch(List<ItemEquptData> mods, string[] targetMods)
		{
			var matches = mods.Where(x => x.Type == 0 && targetMods.Any(t => x.name.IndexOf(t) >= 0));
			bool any = false;
			foreach (var item in matches)
			{
				any = true;
				var modVal = item.addp + item.addv + item.baddp + item.baddv;
				//AddLog("mod {0} val {1}", item.name, modVal.ToString());
				if (modVal < 0)
					return false;
			}

			//else if (equptDatum.Type == 1)
			//{
			//	g_emNpcSkillType type2 = GameUlt.String2Enum<g_emNpcSkillType>(equptDatum.name);
			//	AddLog("Skill " + type2);
			//}
			//else if (equptDatum.Type == 2 && equptDatum.basefive != null)
			//{
			//	for (int i = 0; i < equptDatum.basefive.Length; i++)
			//	{
			//		AddLog("Attr " + (g_emNpcBasePropertyType)i);
			//	}
			//}

			return any;

		}

		public static TaliType GetFuType(Npc me, ItemThing item)
		{
			if (item.GetName().Contains("Heavensent"))
				return TaliType.Ignore;

			if (IsMatch(item.EquptData, CultivationProbs))
				return TaliType.Cultivation;

			if (IsMatch(item.EquptData, BCProbs))
				return TaliType.Breakthrough;

			if (IsMatch(item.EquptData, BattleProps))
				return TaliType.Battle;

			if (IsMatch(item.EquptData, AdventureProps))
				return TaliType.Adventure;

			return TaliType.None;
		}
	}
}
