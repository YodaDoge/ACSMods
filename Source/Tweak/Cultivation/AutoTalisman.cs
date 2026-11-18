using HarmonyLib;
using Light2D;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using XiaWorld;

namespace ACS_Yoda_Tweaks
{
	public class AutoTalismanToggle : Mod
	{
		public override Meta Info => _info;
		private static Meta _info = new Meta("AutoTalisman", "Auto Toggle suitable Talisman", false);
		public AutoTalismanToggle(bool defaultEnabled) : base(defaultEnabled)
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
				if (IsYodaMachine)
				{
					AquireTalisman(__instance.Worker, TaliType.Adventure, _adventureSpeedFu);
				}

				ToggleTalisman(__instance.Worker, TaliType.Adventure);
			}

			private static void AquireTalisman(Npc npc, TaliType workType, IEnumerable<string> spells)
			{
				if (npc.GongKind != g_emGongKind.Dao)
					return;
				var tali = GetUsefulTali(npc, workType).ToList();
				foreach (var usefulSpell in spells)
				{
					var talisman = tali.FirstOrDefault(x => x.Key.m_spell == usefulSpell);
					if (talisman.Key == null)
					{
						AddLog("{0} does not have {1}, looking for one", npc.GetName(), usefulSpell);
						if (MindfulDresser.LookForTalisman(npc, usefulSpell, true))
						{
							RequeueJob(npc);
							break;
						}
					}
					else
					{
						AddLog("{0} already has {1} {3} for {2}", npc.GetName(), talisman.Key.GetName(), usefulSpell, talisman.Key.def.Name);
						if (talisman.Key.def.Name == TalismanTiers[0])
							break;

						var nextTier = talisman.Key.def.Name == TalismanTiers[1] ? 1 : 2;
						if (MindfulDresser.LookForTalisman(npc, usefulSpell, true, minTier: nextTier))
						{
							RequeueJob(npc);
							break;
						}
					}
				}
			}

			private static void RequeueJob(Npc npc)
			{
				var curCmd = npc.JobEngine.CurJob.CMD;
				npc.JobEngine.InterruptJob();
				npc.AddCommand(curCmd.CommandType, curCmd.WorkParam, curCmd.WorkParam2, curCmd.WorkParam3);
			}

			private const string StoryFu = "Spell_ReadNovel";
			private const string HeavenSentFu = "Spell_TongTian1";
			private const string DarkArmorFu = "Spell_Shield1";
			private static string[] _mindstateFu = new string[] { "Spell_MindState2", "Spell_Fu", "Spell_MindState1" };
			private static string[] _cultivationSpeedFu = new string[] { "Spell_DeepPracticeSpeedSpecialCoefficient2", "Spell_DeepPracticeSpeedSpecialCoefficient1" };
			private static string[] _adventureSpeedFu = new string[] { "Spell_WorldMapFlySpeed", "Spell_MoveSpeed2", };

			[HarmonyPostfix]
			[HarmonyPatch(typeof(NpcPractice), "Up2Disciple")]
			public static void AutoEquipTalisman(Npc ___me)
			{
				if (!IsYodaMachine || !___me.LikeHuman) //too special => Yoda Only
					return;

				try
				{
					if (!___me.IsRealPlayerThing)
						return;

					MindfulDresser.LookForTalisman(___me, StoryFu, false); // for GC Breakthrough
					MindfulDresser.LookForTalisman(___me, HeavenSentFu, false); //heavenSent
					MindfulDresser.LookForTalisman(___me, DarkArmorFu, false); //DarkArmor
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

				if (npc.GongKind != g_emGongKind.Dao)
					return;

				var practice = npc.PropertyMgr.Practice;
				var type = practice.CurNeck.Kind == g_emGongBottleNeckType.Thunder ? TaliType.Battle : TaliType.Breakthrough;

				//AddLog("{0} enter neck type {1}", __instance.Worker.GetName(), practice.CurNeck.Kind);
				if (practice.CurNeck.Kind == g_emGongBottleNeckType.God)
				{
					//var kind = (__instance.CMD as CommandBrokenNeck).brokenkind;
					// age/attainment/xx tribulation
					type = TaliType.Battle;
				}
				if(type == TaliType.Breakthrough)
				{
					if (IsYodaMachine)
					{
						AquireTalisman(__instance.Worker, TaliType.Breakthrough, new[] { HeavenSentFu });
						AquireTalisman(__instance.Worker, TaliType.Breakthrough, new[] { StoryFu });
						AquireTalisman(__instance.Worker, TaliType.Breakthrough, _mindstateFu);
					}
				}
				ToggleTalisman(npc, type);
			}

			[HarmonyPostfix]
			[HarmonyPatch(typeof(JobPracticeSkill), "OnEnterJob")]
			public static void JobPracticeSkill(JobPracticeSkill __instance, KStateQUnit unit)
			{
				if (!_info.Enabled)
					return;
				if (IsYodaMachine)
				{
					AquireCultivationTali(__instance);
				}

				ToggleTalisman(__instance.Worker, TaliType.Cultivation);
			}

			private static void AquireCultivationTali(JobBase __instance)
			{
				AquireTalisman(__instance.Worker, TaliType.Cultivation, new[] { HeavenSentFu });
				AquireTalisman(__instance.Worker, TaliType.Cultivation, _cultivationSpeedFu);
				AquireTalisman(__instance.Worker, TaliType.Cultivation, _mindstateFu);
			}

			[HarmonyPostfix]
			[HarmonyPatch(typeof(JobAbsorbLing), "OnEnterJob")]
			public static void JobAbsorbLing(JobAbsorbLing __instance, KStateQUnit unit)
			{
				if (!_info.Enabled)
					return;
				if (IsYodaMachine)
				{
					AquireCultivationTali(__instance);
				}
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
				if (IsYodaMachine)
				{
					AquireCultivationTali(__instance);
				}
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

					var talismans = GetEquippedTalisman(npc).ToList();
					int maxActiveFu = GetMaxActiveFu(npc);

					//bool log = IsYodaMachine;
					//if (log)
					//{
					//	AddLog("{0} equip {1}", npc.Name, wantedType);
					//	foreach (var x in talismans)
					//	{
					//		AddLog("{0}: {1}", x.Key.GetName(), IsFuPositive(x.Key, wantedType));
					//	}
					//}
					var usefulTali = talismans.Where(x => IsFuPositive(x.Key, wantedType) == true);
					if (usefulTali.Count(x => npc.Equip.CheckActive(x.Value)) == maxActiveFu)
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
		public static int GetMaxActiveFu(Npc npc)
		{
			int maxActiveFu = 3 + npc.AddActiveFuCount + RuntimeVar.Var.ExtraFuActive;
			maxActiveFu = Mathf.Clamp(maxActiveFu, 0, 6);
			return maxActiveFu;
		}

		public static IEnumerable<KeyValuePair<ItemThing, g_emEquipType>> GetUsefulTali(Npc me, TaliType type)
		{
			return GetEquippedTalisman(me).Where(x => IsFuPositive(x.Key, type) == true);
		}

		public static IEnumerable<KeyValuePair<ItemThing, g_emEquipType>> GetEquippedTalisman(Npc me)
		{
			for (g_emEquipType fuSlot = g_emEquipType.Fu1; fuSlot < g_emEquipType._FuEnd; fuSlot++)
			{
				ItemThing equip = me.Equip.GetEquip(fuSlot);
				if (equip != null)
				{
					yield return new KeyValuePair<ItemThing, g_emEquipType>(equip, fuSlot);
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

		public static string[] TalismanTiers = new string[] { "Item_SpellLv3", "Item_SpellLv2", "Item_Spell" };
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
