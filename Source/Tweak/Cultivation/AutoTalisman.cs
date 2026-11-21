using HarmonyLib;
using Light2D;
using System;
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
			[HarmonyPrefix]
			[HarmonyPatch(typeof(JobLeave2Explore), "OnEnterJob")]
			public static void EquipAdventureFu(JobLeave2Explore __instance, KStateQUnit unit)
			{
				if (!_info.Enabled)
					return;

				Talisman.ToggleTalisman(__instance.Worker, TaliType.Adventure);
			}

			//JobMakeItem CommandMakeItem => Alch YIeld

			[HarmonyPrefix]
			[HarmonyPatch(typeof(CommandMgr), "AddCommand", new[] { typeof(string), typeof(Thing), typeof(object[]) })]
			public static void EquipFu(string type, Thing thing, ref object[] objs)
			{
				if (!IsYodaMachine)
					return;
				try
				{
					var npc = thing as Npc;
					if (type == "GoMapExplore")
						Talisman.YoinkTalisman(npc, TaliType.Adventure);
					else if (type == "AbsorbLing" && objs.Contains("ClosedDoor")) 
						Talisman.YoinkTalisman(npc, TaliType.Cultivation);
					else if (type == "BrokenNeck")
						Talisman.YoinkTalisman(npc, GetBreakType(npc));
				}
				catch (Exception ex)
				{
					Mod.ShowMessage(ex);
				}
			}

			[HarmonyPostfix]
			[HarmonyPatch(typeof(JobBrokenNeck), "OnLeaveJob")]
			public static void DisableTalismanAfterBreakthrough(JobBrokenNeck __instance, KStateQUnit unit)
			{
				if (!_info.Enabled)
					return;
				Talisman.Disable(__instance.Worker, Talisman.StoryFu);
			}

			[HarmonyPostfix]
			[HarmonyPatch(typeof(JobBrokenNeck), "OnEnterJob")]
			public static void EquipBCFu(JobBrokenNeck __instance, KStateQUnit unit)
			{
				if (!_info.Enabled || !__instance.Worker.IsRealPlayerThing)
					return;
				var npc = __instance.Worker;

				if (npc.GongKind != g_emGongKind.Dao)
					return;
				TaliType type = GetBreakType(npc);

				Talisman.ToggleTalisman(npc, type);
			}

			private static TaliType GetBreakType(Npc npc)
			{
				var practice = npc.PropertyMgr.Practice;
				var type = practice.CurNeck.Kind == g_emGongBottleNeckType.Thunder ? TaliType.Battle : TaliType.Breakthrough;

				//AddLog("{0} enter neck type {1}", __instance.Worker.GetName(), practice.CurNeck.Kind);
				if (practice.CurNeck.Kind == g_emGongBottleNeckType.God)
				{
					//var kind = (__instance.CMD as CommandBrokenNeck).brokenkind;
					// age/attainment/xx tribulation
					type = TaliType.Battle;
				}

				return type;
			}

			[HarmonyPostfix]
			[HarmonyPatch(typeof(JobPracticeSkill), "OnEnterJob")]
			public static void JobPracticeSkill(JobPracticeSkill __instance, KStateQUnit unit)
			{
				if (!_info.Enabled)
					return;

				Talisman.ToggleTalisman(__instance.Worker, TaliType.Cultivation);
			}

			[HarmonyPostfix]
			[HarmonyPatch(typeof(JobAbsorbLing), "OnEnterJob")]
			public static void JobAbsorbLing(JobAbsorbLing __instance, KStateQUnit unit)
			{
				if (!_info.Enabled)
					return;
				Talisman.ToggleTalisman(__instance.Worker, TaliType.Cultivation);
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
				Talisman.ToggleTalisman(__instance.Worker, TaliType.Cultivation);
			}

			[HarmonyPostfix]
			[HarmonyPatch(typeof(JobFight), "OnEnterJob")]
			public static void EquipBattleFu(JobFight __instance, KStateQUnit unit)
			{
				if (!_info.Enabled)
					return;
				Talisman.ToggleTalisman(__instance.Worker, TaliType.Battle);
			}
		}
	}
}
