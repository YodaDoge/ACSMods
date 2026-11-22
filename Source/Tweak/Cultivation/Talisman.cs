using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using XiaWorld;

namespace ACS_Yoda_Tweaks
{
	public enum TaliType
	{
		Cultivation,
		Adventure,
		Battle,
		Breakthrough,
		Outer, 
		Alchemy
	}

	public static class Talisman
	{
		public static void ToggleTalisman(Npc npc, TaliType wantedType)
		{
			try
			{
				if (!npc.IsRealPlayerThing || (npc.Rank != g_emNpcRank.Worker && npc.GongKind != g_emGongKind.Dao))
					return;

				var talismans = GetEquipped(npc).ToList();
				int maxActiveFu = GetMaxActiveFu(npc);

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
				Mod.ShowMessage(ex);
			}
		}

		public static void YoinkTalisman(Npc npc, TaliType wantedType)
		{
			if (Mod.IsYodaMachine)
			{
				if (wantedType == TaliType.Breakthrough)
				{
					Talisman.GetFirstFoundSpell(npc, new[] { HeavenSentFu });
					Talisman.GetFirstFoundSpell(npc, new[] { StoryFu });
					Talisman.GetFirstFoundSpell(npc, _mindstateFu);
				}
				else if (wantedType == TaliType.Cultivation)
				{
					Talisman.GetFirstFoundSpell(npc, new[] { HeavenSentFu });
					Talisman.GetFirstFoundSpell(npc, _cultivationSpeedFu);
					Talisman.GetFirstFoundSpell(npc, _mindstateFu);
				}
				else if (wantedType == TaliType.Adventure)
				{
					Talisman.GetFirstFoundSpell(npc, new[] { HeavenSentFu });
					Talisman.GetFirstFoundSpell(npc, _adventureSpeedFu);
				}
			}
		}

		public static void GetFirstFoundSpell(Npc npc, IEnumerable<string> spells)
		{
			if (npc.GongKind == g_emGongKind.Body || npc.GongKind == g_emGongKind.God || !npc.IsRealPlayerThing)
				return;
			foreach (var usefulSpell in spells)
			{
				var result = MindfulDresser.LookForTalisman(npc, usefulSpell);
				if (result == MindfulDresser.TaliSearchResult.MaxTier || result == MindfulDresser.TaliSearchResult.Found)
					return;
			}
		}

		public const string StoryFu = "Spell_ReadNovel";
		public const string HeavenSentFu = "Spell_TongTian1";
		private const string DarkArmorFu = "Spell_Shield1";
		private static string[] _mindstateFu = new string[] { "Spell_MindState2", "Spell_Fu", "Spell_MindState1" };
		private static string[] _cultivationSpeedFu = new string[] { "Spell_DeepPracticeSpeedSpecialCoefficient2", "Spell_DeepPracticeSpeedSpecialCoefficient1" };
		private static string[] _adventureSpeedFu = new string[] { "Spell_WorldMapFlySpeed", "Spell_MoveSpeed2", };
		public static string[] outerTali = new string[] {
		"Spell_Nutrition1",
		"Spell_Lu" /*Everlasting Status: Butcher&Harvest YIeld; + movespeed*/ ,
		"Spell_Fatigue",
		"Spell_GlobalEfficiency",
		"Spell_MoveSpeed2", //SpiritTravel
		"Spell_Happy",
		"Spell_LunHui_ShenShen", /*Prayer: -rec consum; +mood*/ 
		"Spell_MoveSpeed1" };

		public static void Disable(Npc npc, string spell)
		{
			var talisman = Talisman.GetEquipped(npc).FirstOrDefault(x => x.Key.m_spell == spell);
			if (talisman.Key != null)
			{
				npc.Equip.CloseItemthing(talisman.Key, talisman.Value);
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
			return GetEquipped(me).Where(x => IsFuPositive(x.Key, type) == true);
		}

		public static ItemThing GetEquipped(Npc me, string spellName)
		{
			return GetEquipped(me).FirstOrDefault(x => x.Key.m_spell == spellName).Key;
		}

		public static IEnumerable<KeyValuePair<ItemThing, g_emEquipType>> GetEquipped(Npc me)
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

		public static string[] TalismanTiers = new string[] { "Item_Spell", "Item_SpellLv2", "Item_SpellLv3",  };
		private static string[] BCProbs = new string[] { "MindState" };
		private static string[] CultivationProbs = new string[] { "MindState", "PracticeSpeed" };
		private static string[] BattleProps = new string[] { "Shield", "Fabao", "Spell", "Artifact" };
		private static string[] AdventureProps = new string[] { "FindSpeed", "WorldMapFly" };
		private static string[] AlchemyProps = new string[] { "DanMake" };//, "DanMake_YieldAddP", "DanMake_YieldAddP", "DanMake_SpeedAddV" };

		static Dictionary<TaliType, string[]> associatedMods = new Dictionary<TaliType, string[]>() {
		{ TaliType.Cultivation, CultivationProbs },
		{ TaliType.Battle, BattleProps },
		{ TaliType.Breakthrough, BCProbs },
		{ TaliType.Adventure, AdventureProps },
		{ TaliType.Outer, outerTali},
		{ TaliType.Alchemy, AlchemyProps }
		};

		public static bool? IsFuPositive(ItemThing fuItem, TaliType fuType)
		{
			if (fuItem.m_spell == HeavenSentFu && (fuType == TaliType.Cultivation || fuType == TaliType.Adventure || fuType == TaliType.Breakthrough))
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
	}

}
