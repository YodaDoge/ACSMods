using ACS_Yoda_Tweaks;
using FairyGUI;
using HarmonyLib;
using ModLoaderLite;
using ModLoaderLite.Config;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using XiaWorld;
using XiaWorld.UI.InGame;
using XLua.TemplateEngine;
using static XiaWorld.HumanoidEvolutionMgr;

namespace ACS_Yoda_Tweaks.AutoA2H
{
	public partial class A2H : Mod
	{
		public override Meta Info => _info;
		public const string Name = "AutoA2H";
		private static Meta _info = new Meta(Name, "Animal Autothink", false);

		public A2H() : base(_info.Enabled)
		{
		}

		public A2H(bool defaultEnabled) : base(defaultEnabled)
		{
		}

		public static Dictionary<int, List<string>> AutoNPC = new Dictionary<int, List<string>>();
		public static HumanoidEvolutionMgr HMgr => IManagerModule_LoopInterval<HumanoidEvolutionMgr>.Instance;

		private static List<string> SortedAggTypes = new List<string>
			{
				 "AScene",
				 "ATarget",
				 "AEmotion"
			};

		public static void InitNpcCache(Dictionary<int, List<string>> help = null)
		{
			try
			{
				var saved = help ?? MLLMain.GetSaveOrDefault<Dictionary<int, List<string>>>(_info.Name);
				AutoNPC = saved ?? new Dictionary<int, List<string>>();
			}
			catch (Exception ex)
			{
				ShowMessage(ex);
				KLog.Dbg(ex.ToString());
			}
		}

		public static bool IsWantedFrag(Npc npc, string fragName)
		{
			return AutoNPC[npc.ID].Contains(fragName) && !IsUsedFrag(npc, fragName);
		}

		//copy of Panel_NpcPractice.IsUsedFrag
		private static bool IsUsedFrag(Npc npc, string frag)
		{
			return !npc.A2H.NoEffectFrag.IsNoEffectFrag(frag);
		}

		private static HEFragmentDef GetFragDef(ThinkFrag frag)
		{
			return HMgr.Fragments.GetDef(frag.frags[0]);
		}

		private static HEFragmentDef GetFragDef(IGrouping<string, ThinkFrag> frags) => GetFragDef(frags.First());

		private static string EmotionType = "AEmotion";
		

		[HarmonyPatch]
		public static class AnimalPatch
		{
			[HarmonyPostfix]
			[HarmonyPatch(typeof(HumanoidEvolutionMgr), "_NpcAddThink", new Type[] { typeof(Npc) })]
			public static void ThinkAdded(Npc npc)
			{
				if (!_info.Enabled)
					return;

				ThinkIfYouCan(npc);
			}

			//called by study behaviour
			[HarmonyPostfix]
			[HarmonyPatch(typeof(HumanoidEvolutionMgr), "_NpcAddThink", new Type[] { typeof(Npc), typeof(ThinkFrag) })]
			public static void ThinkAdded(Npc npc, ThinkFrag newThink)
			{
				if (!_info.Enabled)
					return;

				ThinkIfYouCan(npc);
			}
		}

	}
}