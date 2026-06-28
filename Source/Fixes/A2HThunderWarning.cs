using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using XiaWorld;
using XiaWorld.Fight;

namespace ACS_Yoda_Tweaks
{
	public class A2HThunderWarning : Mod
	{
		public override Meta Info => _info;
		private static Meta _info = new Meta("A2HThunerwarning", "A2HThunerwarning", false);

		public A2HThunderWarning(bool defaultEnabled) : base(defaultEnabled)
		{
		}

		[HarmonyPatch]
		public static class Patch
		{
			private static HashSet<Npc> _warned = new HashSet<Npc>();

			[HarmonyPostfix]
			[HarmonyPatch(typeof(Npc), "OnStep")]
			public static void OnInit_Postfix(Npc __instance, float dt)
			{
				var npc = __instance;
				if (npc.IsRealPlayerThing && npc.AnimalHumanFrom != null)
				{
					if (!_warned.Contains(npc) && npc.AnimalHumanFrom.ThunderComing >= 98 && npc.AnimalHumanFrom.ThunderComing <= 102)
					{
						_warned.Add(npc);
						ShowMessage(npc.GetName() + " will soon face their shapeshift Tribulation!");
					}
				}
			}


			[HarmonyPrefix]
			[HarmonyPatch(typeof(BehaviourMakeItem), MethodType.Constructor, new Type[] { typeof(g_emBehaviourKind), typeof(g_emBehaviourWorkKind), typeof(int), typeof(g_emProduceKind) })]
			public static void FixWorkTypeOfRites(g_emBehaviourKind kind, ref g_emBehaviourWorkKind workkind, int MPriority, g_emProduceKind makekind)
			{
				// Change the workkind to your desired value here
				if (makekind == g_emProduceKind.Sacrifice)
				{
					AddLog("Behaviour change!");
					workkind = g_emBehaviourWorkKind.Clean;
				}
			}
		}
	}
}