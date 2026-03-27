using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using XiaWorld;
using static ACS_Yoda_Tweaks.Mod;

namespace ACS_Yoda_Tweaks
{
	public class HandworkPriority : Mod
	{
		public override Meta Info => _info;
		private static Meta _info = new Meta("HandworkPriority", "Handwork before Stonework", true);
		[HarmonyPatch]
		public static class Patch
		{
			[HarmonyPrefix]
			[HarmonyPatch(typeof(BehaviourMakeItem), nameof(BehaviourMakeItem.Check))]
			static bool Prefix(
			BehaviourMakeItem __instance,
			ref JobBase __result,
			Npc npc,
			int seachr,
			bool tryfind,
			g_emProduceKind ___Kind)
			{
				if (!_info.Enabled) return true;

				if (___Kind != g_emProduceKind.Handwork && ___Kind != g_emProduceKind.StoneWork)
					return true;

				Command command = CommandMgr.Instance.FindCommand(
					"MakeItem",
					npc.Key,
					seachr,
					npc,
					delegate (Command c)
					{
						// Custom logic for filtering the command
						CommandMakeItem commandMakeItem = c as CommandMakeItem;
						return commandMakeItem != null && (commandMakeItem.ProduceKind == g_emProduceKind.StoneWork || commandMakeItem.ProduceKind == g_emProduceKind.Handwork);
					},
					delegate (Command c)
					{
						// Custom logic for sorting/priority
						if (c.OwnerThing != null && c.OwnerThing.ThingType == g_emThingType.Building)
						{
							BuildingThing buildingThing = c.OwnerThing as BuildingThing;
							return buildingThing.def.Building.ProducePriority * -10000;
						}
						return 0;
					}
				);

				if (command == null)
				{
					__result = null;
				}
				else
				{
					__result = JobMgr.Instance.CreateJob("JobMakeItem", command);
				}

				return false;
			}
		}
	}
}
