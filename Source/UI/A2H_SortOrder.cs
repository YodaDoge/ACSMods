using ACS_Yoda_Tweaks;
using FairyGUI;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Text;
using UnityEngine;
using XiaWorld;
using XiaWorld.UI.InGame;
using static ACS_Yoda_Tweaks.Mod;
using static XiaWorld.HumanoidEvolutionMgr;


public class A2H_SortOrder : Mod
{
	public override Meta Info => _info;
	private static Meta _info = new Meta("ThoughtManagement", "Animal Thoughts QoL", true);

	public A2H_SortOrder(bool defaultEnabled) : base(defaultEnabled)
	{
	}

	[HarmonyPatch]
	public static class Patch
	{
		[HarmonyPrefix]
		[HarmonyPatch(typeof(Wnd_A2HCreateAgg), "ShowNpc")]
		public static void Prefix(Npc npc)
		{
			if (!_info.Enabled) 
				return;

			if (npc.ThinkableAnimal && npc.A2H.thinkFrags != null)
			{
					var fragger = IManagerModule_LoopInterval<HumanoidEvolutionMgr>.Instance.Fragments;
					var defLookup = new HashSet<string>(npc.A2H.thinkFrags.Select(x => x.frags[0])).ToDictionary(key => key, val => fragger.GetDef(val));
					npc.A2H.thinkFrags = npc.A2H.thinkFrags.OrderBy(x => defLookup[x.frags[0]].Type)
															.ThenBy(x => defLookup[x.frags[0]].Level)
															.ThenBy(x => defLookup[x.frags[0]].Name).ToList();
				
			}

			return;
		}

		[HarmonyPostfix]
		[HarmonyPatch(typeof(Wnd_A2HCreateAgg), "RenderThink")]
		public static void ThinkItemRender(Wnd_A2HCreateAgg __instance, Npc ___npc, UI_A2HThinkConsider uithink, HumanoidEvolutionMgr.ThinkFrag think, bool fullsentence)
		{
			try
			{
				if (!_info.Enabled)
					return;

				if (uithink == null || uithink.m_bg.grayed == true)
					return; // aint messing with vanilla

				bool isMemoryFrag = ___npc.A2H.thinkFragCaches?.Contains(think) == true;
				bool isMemorizedType = ___npc.A2H.thinkFragCaches?.Any(x => x.frags[0] == think.frags[0]) == true;
				uithink.m_desc.richTextField.textFormat.bold = !isMemoryFrag && isMemorizedType;
			}
			catch (Exception ex)
			{
				ShowMessage(ex);
			}

		}
	}
}
