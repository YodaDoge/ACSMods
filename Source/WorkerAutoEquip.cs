using ACS_Yoda_Tweaks;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XiaWorld;
using XiaWorld.Fight;

public class WorkerAutoEquip : Mod
{
	public override Meta Info => _info;
	private static Meta _info = new Meta("WorkerAutoEquip", "Workers Auto Equip Trinkets", true);

	public WorkerAutoEquip(bool defaultEnabled) : base(defaultEnabled)
	{
	}

	[HarmonyPatch]
	public static class Patch
	{
		static Dictionary<Npc, float> lastCheckDict = new Dictionary<Npc, float>();
		static HashSet<Npc> activeWorkers = new HashSet<Npc>();
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

		[HarmonyPostfix]
		[HarmonyPatch(typeof(NpcFeeling), "Step")]
		public static void Step(float dt, NpcFeeling __instance, ref Npc ___me)
		{
			if (!_info.Enabled) return;

			var me = ___me;
			bool validForAutoEquip = me.AutoWear && !me.IsVistor && me.IsSmartRace && me.EnemyType != g_emEnemyType.PlayerAttacker && me.Rank == g_emNpcRank.Worker;
			if (!validForAutoEquip)
				return;

			if (!activeWorkers.Contains(me))
			{
				activeWorkers.Add(me);
			}

			float lastWearCheck = lastCheckDict.GetOrCreate(me);
			if (World.Instance.TolSecond - lastWearCheck < checkInterval)
				return;

			bool hasValidWearCMD = me.WearCMD > 0 && CommandMgr.Instance.FindCommandByID(me.WearCMD) != null;
			if (hasValidWearCMD)
				return;

			if (LookForTool(me, "Item_SmallBell"))
				return;

			if (LookForTool(me, "Item_Handkerchief"))
				return;

			//TODO: Scraper/facemask if appropiate job is assigned
			//TODO: Feature - FindBetterTool

			//var talismanLabel = g_emItemLable.Spell;
			var equippedTali = GetTali(me).Select(x => x.Key).ToList();

			if (equippedTali.Count < 3)
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

			lastCheckDict[me] = World.Instance.TolSecond;
		}


		[HarmonyPrefix]
		[HarmonyPatch(typeof(ThingsData), "FindItem")]
		public static void FindItem(Npc npc, int r, string name, int okey = 0, bool issort = false, string tag = null, int tagmin = 0, int tagmax = 9999, Func<ItemThing, bool> con = null, bool StrictSort = false)
		{
			if (tag == "_WearAble" && npc.IsPlayerThing && con != null)
			{
				Func<ItemThing, bool> chain = itm =>
				{
					if (itm.Rate >= 9)
						return false;
					return con(itm);
				};
			}
		}

		private static bool LookForTalisman(Npc me, string spell)
		{

			ItemThing itemThing = me.map.Things.FindItem(me, 9999, "Item_SpellLv3", con: x => x.m_spell == spell);
			itemThing = itemThing ?? me.map.Things.FindItem(me, 9999, "Item_SpellLv2", con: x => x.m_spell == spell);
			itemThing = itemThing ?? me.map.Things.FindItem(me, 9999, "Item_Spell", con: x => x.m_spell == spell);
			if (itemThing != null)
			{
				Command command = me.AddCommand("EquipItem", itemThing);
				me.WearCMD = command.ID;
				return true;
			}
			return false;
		}

		private static bool LookForTool(Npc me, string tool)
		{
			if (me.Equip.FindTool(tool) == null)
			{
				ItemThing itemThing = me.map.Things.FindItem(me, 9999, tool);
				if (itemThing != null && me.CheckEquipCell(itemThing) != g_emEquipType.None)
				{
					Command command = me.AddCommand("EquipItem", itemThing);
					me.WearCMD = command.ID;

					KLog.Dbg($"{me.Name} is getting himself a {itemThing}");

					return true;
				}
			}
			return false;
		}

		private static IEnumerable<KeyValuePair<ItemThing, bool>> GetTali(Npc me)
		{
			for (g_emEquipType fuSlot = g_emEquipType.Fu1; fuSlot < g_emEquipType._FuEnd; fuSlot++)
			{
				ItemThing equip = me.Equip.GetEquip(fuSlot);
				if (equip != null && me.CheckEquipCell(equip) != g_emEquipType.None)
				{
					yield return new KeyValuePair<ItemThing, bool>(equip, me.Equip.CheckActive(fuSlot));
				}
			}
		}


	}
}
