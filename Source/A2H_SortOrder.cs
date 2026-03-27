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
		public static bool Prefix(Npc npc)
		{
			if (!_info.Enabled) return true;

			if (npc.ThinkableAnimal)
			{
				if (npc.A2H.thinkFrags != null)
				{
					KLog.Dbg("A2H Applying sortorder");
					var fragger = IManagerModule_LoopInterval<HumanoidEvolutionMgr>.Instance.Fragments;
					var defLookup = new HashSet<string>(npc.A2H.thinkFrags.Select(x => x.frags[0])).ToDictionary(key => key, val => fragger.GetDef(val));
					npc.A2H.thinkFrags = npc.A2H.thinkFrags.OrderBy(x => defLookup[x.frags[0]].Type)
															.ThenBy(x => defLookup[x.frags[0]].Level)
															.ThenBy(x => defLookup[x.frags[0]].Name).ToList();
				}
			}

			return true;
		}

		private static Npc _npc;

		private static bool _didPatch = false;

		static Color colorMemorized = new Color(0, 1, 0, 0.15f);
		static Color? colorDefault = null;

		static DefLoaderT<HEFragmentDefs, HEFragmentDef, string> frags => IManagerModule_LoopInterval<HumanoidEvolutionMgr>.Instance.Fragments;

		[HarmonyPostfix]
		[HarmonyPatch(typeof(Wnd_A2HCreateAgg), "RenderThink")]
		public static void ShowNpc_PostfixShowNpc(Wnd_A2HCreateAgg __instance, Npc ___npc, UI_A2HThinkConsider uithink, HumanoidEvolutionMgr.ThinkFrag think, bool fullsentence)
		{
			if (!_info.Enabled) return;

			if (uithink.m_bg.grayed)
				return; // aint messing with vanilla

			//uithink.m_bg.color = Color.clear;

			var a2h = ___npc.A2H;
			bool isMemoryFrag = a2h.thinkFragCaches.Contains(think);
			bool isMemorizedType = a2h.thinkFragCaches.Any(x => x.frags[0] == think.frags[0]);
			uithink.m_desc.richTextField.textFormat.bold = !isMemoryFrag && isMemorizedType;
			//if (isMemorized)
			//{
			//	uithink.m_bg.color = colorMemorized;
			//}
		}
		//m_thinks = (GList) GetChildAt(15);
		//m_vthinks = (GList) GetChildAt(16);

	}
}
