using ACS_Yoda_Tweaks;
using HarmonyLib;
using rail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using UnityEngine;
using XiaWorld;
using XiaWorld.Fight;
using static XiaWorld.AuctionData;

public class MindfulDresser : Mod
{
	public override Meta Info => _info;
	private static Meta _info = new Meta("WorkerAutoEquip", "Smarter Dressers", true);

	public MindfulDresser(bool defaultEnabled) : base(defaultEnabled)
	{
	}

	public static bool LookForTalisman(Npc me, string spell, bool instantAssign = true)
	{
		ItemThing itemThing = me.map.Things.FindItem(me, 9999, "Item_SpellLv3", con: x => !IsFengShuiItem(x) && x.m_spell == spell);
		itemThing = itemThing ?? me.map.Things.FindItem(me, 9999, "Item_SpellLv2", con: x => !IsFengShuiItem(x) && x.m_spell == spell);
		itemThing = itemThing ?? me.map.Things.FindItem(me, 9999, "Item_Spell", con: x => !IsFengShuiItem(x) && x.m_spell == spell);
		if (itemThing != null)
		{
			Command command = me.AddCommand("EquipItem", itemThing);
			if (instantAssign)
				me.WearCMD = command.ID;
			return true;
		}
		return false;
	}
	private static bool IsFengShuiItem(ItemThing thing) => thing.FSItemState > 0;

	[HarmonyPatch]
	public static class Patch
	{
		static Dictionary<Npc, float> lastCheckDict = new Dictionary<Npc, float>();
		const float checkInterval = 60f;

		private static string[] genericTalisman = new string[] {
		"Spell_MoveSpeed2", //SpiritTravel
		"Spell_GlobalEfficiency",
		"Spell_Fatigue",
		"Spell_Lu" /*Everlasting Status: Butcher&Harvest YIeld; + movespeed*/ ,
		"Spell_Nutrition1",
		"Spell_Happy",
		"Spell_LunHui_ShenShen", /*Prayer: -rec consum; +mood*/ 
		"Spell_MoveSpeed1" };

		private static string[] maybeTalisman = new string[] { "Spell_TongTian1", /*Heavensent: FiveAttributes*/  "Spell_Intelligence" /*Civil Prosperity: Learnspeed*/ };

		private static string[] crafterTalisman = new string[] { "Spell_MadeQualityAddValue" /*Craftsmanship: Crafting QUality gain*/ };

		[HarmonyPrefix]
		[HarmonyPatch(typeof(ToilEquipItem), "OnEnterToil")]
		public static void Step(ToilEquipItem __instance, KStateQUnit unit)
		{
			if (!_info.Enabled) return;

			var npc = __instance.npc;
			var horse = npc.ItemInHand.GetHorseData();
			if (horse == null)
				return;
			npc.NoRideHorse = npc.GetSpeed() >= horse.Speed;
		}

		[HarmonyPostfix]
		[HarmonyPatch(typeof(NpcPractice), "Up2Disciple")]
		public static void Up2Disciple(Npc ___me)
		{
			try
			{
				var me = ___me;
				if (!___me.IsRealPlayerThing)
					return;
				
				var t = me.Equip.FindTool(ItemSickle);
				if (t != null)
					me.Equip.UnEquipItem(t);

				t = me.Equip.FindTool(ItemAxe);
				if (t != null)
					me.Equip.UnEquipItem(t);

				t = me.Equip.FindTool(ItemPickAxe);
				if (t != null)
					me.Equip.UnEquipItem(t);

			}
			catch (Exception ex)
			{
				ShowMessage(ex);
			}
		}

		[HarmonyPostfix]
		[HarmonyPatch(typeof(NpcFeeling), "Step")]
		public static void Step(float dt, NpcFeeling __instance, ref Npc ___me)
		{
			var me = ___me;
			float lastWearCheck = lastCheckDict.GetOrCreate(me);
			if (World.Instance.TolSecond - lastWearCheck < checkInterval)
				return;

			bool hasValidWearCMD = me.WearCMD > 0 && CommandMgr.Instance.FindCommandByID(me.WearCMD) != null;
			if (hasValidWearCMD)
				return;

			if (me.PropertyMgr.MoodData.CheckMood("NeedTrousers"))
				FindClothing(me, "_LableTrousers");

			if (me.PropertyMgr.MoodData.CheckMood("NeedClothes"))
				FindClothing(me, "_LableClothes");

			if (!_info.Enabled) return;

			bool validForAutoEquip = me.AutoWear && !me.IsVistor && me.IsSmartRace && me.EnemyType != g_emEnemyType.PlayerAttacker && (me.Rank == g_emNpcRank.Worker || me.GongKind == g_emGongKind.Dao);
			if (!validForAutoEquip)
				return;


			if (me.Rank == g_emNpcRank.Worker)
			{
				if (LookForTool(me, "Item_SmallBell"))
					return;

				if (LookForTool(me, "Item_Handkerchief"))
					return;

				//TODO: Scraper/facemask if appropiate job is assigned
				//TODO: Feature - FindBetterTool

				var equippedTali = GetTali(me).Select(x => x.Key).ToList();
				int maxActiveFu = 3 + me.AddActiveFuCount +
								  (me.IsRealPlayerThing ? RuntimeVar.Var.ExtraFuActive : 0);
				maxActiveFu = Mathf.Clamp(maxActiveFu, 0, 6);
				if (maxActiveFu < 3)
				{
					//TODO: if crafter => check craftingtable
					foreach (var usefulTaliName in genericTalisman)
					{
						bool hasTali = equippedTali.Any(x => x.m_spell == usefulTaliName);

						if (!hasTali && LookForTalisman(me, usefulTaliName))
						{
							return;
						}
					}
				}
			}
			else if (me.GongKind == g_emGongKind.Dao)
			{
				if (LookForTool(me, "Item_Dice"))
					return;
				if (LookForTool(me, "Item_SmallBell"))
					return;
				if (LookForTool(me, "Item_PerfumeBag"))
					return;
				if (LookForTool(me, "Item_Bracelet"))
					return;
			}
			lastCheckDict[me] = World.Instance.TolSecond;
		}

		private static void FindClothing(Npc me, string itemTag)
		{
			ItemThing itemThing = me.map.Things.FindItem(me, 200, null, 0, issort: false, "_WearAble", 0, 9999, delegate (ItemThing it)
			{
				if (it.def.Item.Equip.NeedSex != g_emNpcSex.None && it.def.Item.Equip.NeedSex != me.Sex)
				{
					return false;
				}
				return it.TagData.CheckTag(itemTag) > 0;
			});
			if (itemThing != null)
			{
				Command command = me.AddCommand("EquipItem", itemThing);
				me.WearCMD = command.ID;
			}
		}

		private const string ItemAxe = "Item_SysAxe";

		[HarmonyPrefix]
		[HarmonyPatch(typeof(BehaviourCutoff), "Check")]
		public static bool UpgradeAxe(ref JobBase __result, Npc npc, int seachr = 10000, bool tryfind = false)
		{
			try
			{
				__result = LookForUpgrade(npc, ItemAxe);
				return __result == null;
			}
			catch (Exception ex)
			{
				ShowMessage(ex);
			}
			return true;
		}
		private const string ItemPickAxe = "Item_SysPickaxe";

		[HarmonyPrefix]
		[HarmonyPatch(typeof(BehaviourMine), "Check")]
		public static bool UpgradePick(ref JobBase __result, Npc npc, int seachr = 10000, bool tryfind = false)
		{
			__result = LookForUpgrade(npc, ItemPickAxe);
			return __result == null;
		}
		private const string ItemSickle = "Item_SysSickle";
		[HarmonyPrefix]
		[HarmonyPatch(typeof(BehaviourPlant), "Check")]
		public static bool UpgradePlow(ref JobBase __result, Npc npc, int seachr = 10000, bool tryfind = false)
		{
			__result = LookForUpgrade(npc, ItemSickle);
			return __result == null;
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(ThingsData), "FindItem")]
		public static void DontEquipDivine(Npc npc, int r, string name, int okey = 0, bool issort = false, string tag = null, int tagmin = 0, int tagmax = 9999, Func<ItemThing, bool> con = null, bool StrictSort = false)
		{
			if (tag == "_WearAble" && npc.IsPlayerThing && con != null)
			{
				var original = con;
				Func<ItemThing, bool> chain = itm =>
				{
					if (itm.Rate >= 9 || IsFengShuiItem(itm))
						return false;
					return original(itm);
				};
				con = chain;
			}
		}

		private static bool LookForTool(Npc me, string tool)
		{
			if (me.Equip.FindTool(tool) == null)
			{
				ItemThing itemThing = me.map.Things.FindItem(me, 9999, tool, con: x => !IsFengShuiItem(x));
				if (itemThing != null && me.CheckEquipCell(itemThing) != g_emEquipType.None)
				{
					Command command = me.AddCommand("EquipItem", itemThing);
					me.WearCMD = command.ID;

					return true;
				}
			}
			return false;
		}

		private static JobBase LookForUpgrade(Npc me, string tool)
		{
			var current = GetEfficency(me, me.Equip.FindTool(tool));
			var itemThing = me.map.Things.FindItems(me, 9999, 20, tool, con: x => !IsFengShuiItem(x) && GetEfficency(me, x) > current)?.OrderByDescending(x => GetEfficency(me, x))?.FirstOrDefault();
			if (itemThing != null && me.CheckEquipCell(itemThing) != g_emEquipType.None)
			{
				Command command = me.AddCommand("EquipItem", itemThing);
				me.WearCMD = command.ID;
				return JobMgr.Instance.CreateJob("JobEquipItem", command); ;
			}
			return null;
		}

		private static float GetEfficency(Npc me, ItemThing item)
		{
			if (item == null)
				return 0f;
			float qualityEquipValue = item.GetQualityEquipValue();
			float num = 1f * qualityEquipValue;
			if (item.StuffDef != null && item.StuffDef.Item.BeMaterial != null)
			{
				num = item.StuffDef.Item.BeMaterial.WorkSpeedCoefficientWhenBeMain;
			}
			num += (float)me.CheckSpecialFlag(g_emNpcSpecailFlag.UpgradeEquipModifier);
			return num;
		}

		public static IEnumerable<KeyValuePair<ItemThing, g_emEquipType>> GetTali(Npc me)
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
	}
}
