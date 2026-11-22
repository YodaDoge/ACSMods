using FairyGUI;
using HarmonyLib;
using ModLoaderLite;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml.Linq;
using UnityEngine;
using XiaWorld;
using XiaWorld.UI.InGame;
using static CSCallLua;
using static Panel_ThingInfo;
using static XiaWorld.ThingsBag;

namespace ACS_Yoda_Tweaks
{
	public class MiscFix : Mod
	{
		public override Meta Info => _info;
		private static Meta _info = new Meta("MiscFix", "", true);

		public MiscFix(bool defaultEnabled) : base(defaultEnabled)
		{
		}

		[HarmonyPatch]
		public static class Patch
		{
			[HarmonyPostfix]
			[HarmonyPatch(typeof(Wnd_GameDIY), "_RefreshEnv")]
			public static void ShowCorrectSeedInDifficultyWindow(Wnd_GameDIY __instance, UI_WindowGameDiy ___UIInfo)
			{
				___UIInfo.m_Evn.m_n239.title = World.Instance.map.Seed;
			}

			[HarmonyPrefix]
			[HarmonyPatch(typeof(BuildingProduce), "UpdateTaskBeg")]
			public static bool AlchemyUseStorage(BuildingProduce __instance, BuildingProduce.BuildingProduceData task)
			{
				try
				{
					if (__instance.Building.IsAlchemyBuilding())
					{
						for (int index = 0; index < task.thingDef.Item.BeMade.CostItems.Count; ++index)
						{
							ItemCostData costItem = task.thingDef.Item.BeMade.CostItems[index];
							ThingsBag.GetingData getingData = __instance.Building.Bag.AddBegItem(costItem.name, costItem.rcount, ThingsBag.emBegType.None, costItem.tag, costItem.minlevel, costItem.maxlevel, GameDefine.ProdeceLinkWork[__instance.Building.def.Building.ProduceKind], task.ID);
							if (getingData != null)
							{
								getingData.cmd.BegType = emBegType.MakeItem;
								getingData.cmd.ActDisable = true;
							}
							else
							{
								__instance.Building.Bag.FinishBegByRemoteStorage();
								__instance.Building.Step(0.001f); //maybe speeds up reaction of alchemist
							}
						}
						return false;
					}
				}
				catch (System.Exception ex)
				{
					ShowMessage(ex);
				}

				return true;
			}



			[HarmonyPrefix]
			[HarmonyPatch(typeof(Wnd_GameMain), "ShowMainMenu")]
			public static void RemoveDuplicateMLL(Wnd_GameMain __instance, PopupMenu ___MainMenu)
			{
				try
				{
					var children = ___MainMenu.list.GetChildren().OfType<GButton>().Where(x => x.title.StartsWith("MLL")).Skip(1).ToList();
					foreach (var item in children)
					{
						___MainMenu.list.RemoveChild(item);
					}
				}
				catch (System.Exception ex)
				{
					ShowMessage(ex);
				}
			}

			[HarmonyPrefix]
			[HarmonyPatch(typeof(JobLeaveMap), "OnCheckFaildCondition")]
			public static bool FixPlayerCharactersStillLeaving(JobLeaveMap __instance, ref bool __result)
			{
				if (__instance.Worker.IsRealPlayerThing)
				{
					__instance.Worker.Leave = 0;
					var interrupt = __instance.InterruptJob(checkInterrupt: false, forceSuccess: true);
					AddLog($"{__instance.Worker.GetName()} cancels leave {interrupt} ");
					__result = false;
					return false;
				}
				return true;
			}
		}
	}
}