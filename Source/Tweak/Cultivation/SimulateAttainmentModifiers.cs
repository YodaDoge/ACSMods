using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using XiaWorld;

namespace ACS_Yoda_Tweaks
{
	public class SimulateAttainmentModifiers : Mod
	{
		public override Meta Info => _info;
		private static Meta _info = new Meta("SimulateAttainmentModifiers", "Always apply negative attainment stuff existing on map", false);
		public SimulateAttainmentModifiers(bool defaultEnabled) : base(defaultEnabled)
		{
		}

		[HarmonyPatch]
		public static class Patch
		{
			private static string AttainmentMod = "AbilityLvAddV";

			[HarmonyPrefix]
			[HarmonyPatch(typeof(NpcPractice), nameof(NpcPractice.GetTreeExpNeed), new System.Type[] { typeof(int), typeof(int) })]
			public static bool GetTreeExpNeed(NpcPractice __instance, int dif, int yinyang, ref float __result)
			{
				if (!IsYodaMachine)
					return true;
				try
				{
					var npc = ThingMgr.Instance.NpcList.FirstOrDefault(x => x.PropertyMgr?.Practice == __instance);
					if (npc == null || !npc.IsRealPlayerThing)
						return true;

					int num = 0;
					num = (__instance.Gong.YinYang == 0 || __instance.LogicStage < 10) ? __instance.GetDaoHang() :
						yinyang == -1 ? __instance.GetYinDaoHang() :
						yinyang == 1 ? __instance.GetYangDaoHang()
						: Mathf.Min(__instance.GetYinDaoHang(), __instance.GetYangDaoHang());

					bool hasAttainmentTalisman = MindfulDresser.Patch.GetTali(npc).Any(x => x.Key.EquptData?.Any(e => e.name == AttainmentMod) == true);
					if (!hasAttainmentTalisman)
					{
						var shroudingValue = FindShroudingTalisman(npc);
						num += shroudingValue;
					}

					int itemMods = FindAttainmentItems(npc);
					num += itemMods;
					float cangJingGeLearnBuff = CangJingGeMgr.Instance.GetCangJingGeLearnBuff();
					__result = 1f * (float)dif * (400f + (float)(2 * num * num)) * (1f + GameEventMgr.Instance.GetCurDriver().TreeExpAddtion) * (1f - cangJingGeLearnBuff);
					return false;
				}
				catch (Exception ex)
				{
					ShowMessage(ex);
					return true;
				}
			}

			private static int FindAttainmentItems(Npc npc)
			{
				int itemMods = 0;
				if (npc.Sex == g_emNpcSex.Male)
				{
					var chest = npc.map.Things.FindItems(npc, 9999, 1, "Item_SP_Clothes4", nocheckAct: true)?.FirstOrDefault(); //Shroud Tunic
					if (chest != null)
						itemMods -= 8;

					var pants = npc.map.Things.FindItems(npc, 9999, 1, "Item_SP_Trousers4", nocheckAct: true)?.FirstOrDefault(); //Shroud pants
					if (pants != null)
						itemMods -= 8;
				}
				else
				{
					var chest = npc.map.Things.FindItems(npc, 9999, 1, "Item_SP_Clothes8", nocheckAct: true)?.FirstOrDefault(); //Flow Tunic
					if (chest != null)
						itemMods -= 8;

					var pants = npc.map.Things.FindItems(npc, 9999, 1, "Item_SP_Trousers8", nocheckAct: true)?.FirstOrDefault(); //Flow pants
					if (pants != null)
						itemMods -= 8;
				}

				var seed = npc.map.Things.FindItems(npc, 9999, 1, "Item_Dan_LingYuanZhong", nocheckAct: true)?.FirstOrDefault(); //SpiritSeed
				if (seed != null)
					itemMods -= 25;
				return itemMods;
			}

			private static float lastCheck = 0;
			private static int lastVal = 0;
			const float checkInterval = 2f;
			private static int FindShroudingTalisman(Npc npc)
			{
				if (World.Instance.TolSecond - lastCheck < checkInterval)
					return lastVal;

				lastCheck = World.Instance.TolSecond;

				string heavenShroudingSpellName = "Spell_TongTian2";
				var fu = new List<ItemThing>();
				fu.AddRange(npc.map.Things.FindItems(npc, 9999, 9999, "Item_SpellLv3", con: x => x.m_spell == heavenShroudingSpellName, nocheckAct: true) ?? new List<ItemThing>());
				fu.AddRange(npc.map.Things.FindItems(npc, 9999, 9999, "Item_SpellLv2", con: x => x.m_spell == heavenShroudingSpellName, nocheckAct: true) ?? new List<ItemThing>());
				fu.AddRange(npc.map.Things.FindItems(npc, 9999, 9999, "Item_Spell", con: x => x.m_spell == heavenShroudingSpellName, nocheckAct: true) ?? new List<ItemThing>());
				AddLog("found fu" + fu.Count.ToString());

				var mod = fu.Any() ? fu.Min(x => (int)Math.Round((x.EquptData?.FirstOrDefault(f => f.name == AttainmentMod)?.addv * x.GetQualityEquipValue()) ?? 0)) : 0;

				lastVal = mod;
				return lastVal;
			}
		}
	}
}
