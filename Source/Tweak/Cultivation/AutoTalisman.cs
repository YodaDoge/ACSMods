using HarmonyLib;
using Light2D;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using XiaWorld;
using static XiaWorld.AuctionData;

namespace ACS_Yoda_Tweaks
{
	public class AutoTalisman : Mod
	{
		public override Meta Info => _info;
		private static Meta _info = new Meta("AutoTalisman", "Auto Toggle suitable Talisman", false);
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
				if (!_info.Enabled)
					return;
				ToggleTalisman(__instance.Worker, TaliType.Adventure);
			}

			private static string[] _mindstateFu = new string[] { "Spell_MindState2", "Spell_Fu", "Spell_MindState1"};
			private static string[] _cultivationSpeedFu = new string[] { "Spell_DeepPracticeSpeedSpecialCoefficient2", "Spell_DeepPracticeSpeedSpecialCoefficient1" };
			private static string[] _adventureSpeedFu = new string[] {"Spell_WorldMapFlySpeed", "Spell_MoveSpeed2", };


			[HarmonyPostfix]
			[HarmonyPatch(typeof(NpcPractice), "Up2Disciple")]
			public static void AutoEquipTalisman(Npc ___me)
			{
				if (!IsYodaMachine) //too special => Yoda Only
					return;

				try
				{
					if (!___me.IsRealPlayerThing)
						return;

					MindfulDresser.LookForTalisman(___me, "Spell_ReadNovel", false); // for GC Breakthrough
					MindfulDresser.LookForTalisman(___me, "Spell_TongTian1", false); //heavenSent
					MindfulDresser.LookForTalisman(___me, "Spell_Shield1", false); //DarkArmor
					_mindstateFu.FirstOrDefault(x => MindfulDresser.LookForTalisman(___me, x, false));
					_cultivationSpeedFu.FirstOrDefault(x => MindfulDresser.LookForTalisman(___me, x, false));
					_adventureSpeedFu.FirstOrDefault(x => MindfulDresser.LookForTalisman(___me, x, false));
				}
				catch (Exception ex)
				{
					ShowMessage(ex);
				}
			}

			[HarmonyPostfix]
			[HarmonyPatch(typeof(JobBrokenNeck), "OnEnterJob")]
			public static void EquipBCFu(JobBrokenNeck __instance, KStateQUnit unit)
			{
				if (!_info.Enabled)
					return;

				var npc = __instance.Worker;
				var practice = npc.PropertyMgr.Practice;
				var type = practice.CurNeck.Kind == g_emGongBottleNeckType.Thunder ? TaliType.Battle : TaliType.Breakthrough;
				ToggleTalisman(npc, type);
			}

			[HarmonyPostfix]
			[HarmonyPatch(typeof(JobPracticeSkill), "OnEnterJob")]
			public static void JobPracticeSkill(JobPracticeSkill __instance, KStateQUnit unit)
			{
				if (!_info.Enabled)
					return;
				ToggleTalisman(__instance.Worker, TaliType.Cultivation);
			}

			[HarmonyPostfix]
			[HarmonyPatch(typeof(JobAbsorbLing), "OnEnterJob")]
			public static void JobAbsorbLing(JobAbsorbLing __instance, KStateQUnit unit)
			{
				if (!_info.Enabled)
					return;
				ToggleTalisman(__instance.Worker, TaliType.Cultivation);
			}

			[HarmonyPostfix]
			[HarmonyPatch(typeof(JobPractice), "OnEnterJob")]
			public static void JobPractice(JobPractice __instance, KStateQUnit unit)
			{
				if (!_info.Enabled)
					return;
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
				if (!_info.Enabled)
					return;
				ToggleTalisman(__instance.Worker, TaliType.Battle);
			}

			public static void ToggleTalisman(Npc npc, TaliType wantedType)
			{
				try
				{
					if (!npc.IsRealPlayerThing || npc.Rank != g_emNpcRank.Disciple)
						return;

					var talismans = MindfulDresser.Patch.GetTali(npc).ToList();

					int maxActiveFu = 3 + npc.AddActiveFuCount + RuntimeVar.Var.ExtraFuActive;
					maxActiveFu = Mathf.Clamp(maxActiveFu, 0, 6);

					//bool log = IsYodaMachine;
					//if (log)
					//{
					//	AddLog("{0} equip {1}", npc.Name, wantedType);
					//	foreach (var x in talismans)
					//	{
					//		AddLog("{0}: {1}", x.Key.GetName(), IsFuPositive(x.Key, wantedType));
					//	}
					//}

					if (talismans.Count(x => IsFuPositive(x.Key, wantedType) == true && npc.Equip.CheckActive(x.Value)) == maxActiveFu)
						return;

					foreach (var negative in talismans.Where(x => IsFuPositive(x.Key, wantedType) == false))
					{
						npc.Equip.CloseItemthing(negative.Key, negative.Value);
					}


					var inActivePositive = talismans.Where(x => IsFuPositive(x.Key, wantedType) == true && !npc.Equip.CheckActive(x.Value)).ToArray();
					var usedSlots = talismans.Where(x => npc.Equip.CheckActive(x.Value)).ToList();

					foreach (var fu in inActivePositive)
					{
						if (usedSlots.Count() >= maxActiveFu)
						{
							var neutralToDisable = usedSlots.FirstOrDefault(x => IsFuPositive(x.Key, wantedType) == null);

							if (neutralToDisable.Key == null) //default of the struct
								break;

							npc.Equip.CloseItemthing(neutralToDisable.Key, neutralToDisable.Value);
							usedSlots.Remove(neutralToDisable);
						}
						npc.Equip.ActiveItemThing(fu.Key, fu.Value);
					}
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
		}

		private static string[] BCProbs = new string[] { "MindState" };
		private static string[] CultivationProbs = new string[] { "MindState", "PracticeSpeed" };
		private static string[] BattleProps = new string[] { "Shield", "Fabao", "Spell", "Artifact" };
		private static string[] AdventureProps = new string[] { "FindSpeed", "WorldMapFly" };

		static Dictionary<TaliType, string[]> associatedMods = new Dictionary<TaliType, string[]>() {
		{ TaliType.Cultivation, CultivationProbs },
		{ TaliType.Battle, BattleProps },
		{ TaliType.Breakthrough, BCProbs },
		{ TaliType.Adventure, AdventureProps } };

		public static bool? IsFuPositive(ItemThing fuItem, TaliType fuType)
		{
			if (fuItem.GetName().Contains("Heavensent") && (fuType == TaliType.Cultivation || fuType == TaliType.Adventure || fuType == TaliType.Breakthrough))
				return true;
			var fuMods = fuItem.EquptData;

			var targetMods = associatedMods[fuType];
			var matchingMods = fuMods.Where(x => HasModMatch(x, targetMods)).ToArray();
			if (!matchingMods.Any())
			{
				return null;
			}

			var modValues = matchingMods.Select(item => item.addp + item.addv + item.baddp + item.baddv);
			bool allPositive = modValues.All(x => x >= 0);
			return allPositive;
		}

		private static bool HasModMatch(ItemEquptData x, string[] targetMods)
		{
			return x.Type == 0 && targetMods.Any(t => x.name.IndexOf(t) >= 0);
		}

		//private static bool GetModVal(ItemEquptData itm, string[] targetMods)
		//{
		//	return itm.Type == 0 && targetMods.Where(t => itm.name.IndexOf(t) >= 0).Sum(x => 
		//}
	}
}
