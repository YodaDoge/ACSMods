using ACS_Yoda_Tweaks;
using FairyGUI;
using HarmonyLib;
using KTV;
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
using static XiaWorld.HumanoidEvolutionMgr;

namespace ACS_Yoda_Tweaks.AutoA2H
{
	public partial class A2H : Mod
	{
		public override Meta Info => _info;
		public const string Name = "AutoA2H";
		private static Meta _info = new Meta(Name, "Animal Thoughts Automation", true);
		const int MaxFrags = 40;
		const int MaxMem = 10;

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
				ShowMessage(ex.ToString());
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

		public static void ThinkIfYouCan(Npc npc)
		{
			if (!AutoNPC.ContainsKey(npc.ID))
			{
				return;
			}

			try
			{
				if (TryFormFinalFrag(npc))
					return;

				var groupedThoughts = npc.A2H.thinkFrags.Concat(npc.A2H.thinkFragCaches).GroupBy(x => x.frags[0]);

				var wantedForCombo = groupedThoughts.Where(x => IsWantedFrag(npc, x.Key) && x.Count() >= 2).ToList();
				var canAggNow = wantedForCombo.Where(x => x.Count() > 2);

				if (canAggNow.Any())
				{
					var existingAggs = npc.A2H.thinkAggregates ?? new List<ThinkAggregate>();
					var priorizedFrags = canAggNow.OrderBy(c => existingAggs.Count(existingAgg => existingAgg.Combine == HMgr.Fragments.GetDef(c.First().frags[0]).Type)).ToList();

					var target = priorizedFrags.First();

					int freeMemorySlots = MaxMem - npc.A2H.thinkFragCaches.Count;
					if (freeMemorySlots > 0)
					{
						var toMemorize = npc.A2H.thinkFrags.Where(x => x.frags[0] != target.Key && IsWantedFrag(npc, x.frags[0]))
							.Skip(1).Take(freeMemorySlots).ToList();

						foreach (var f in toMemorize)
							npc.A2H.MoveThink_Cache2Think(f);
					}

					npc.A2H.RemoveAllTState();

					foreach (var f in target)
					{
						f.TState = 1;
					}

					var finalThought = priorizedFrags.First().Key;

					StartAggrThink(npc, finalThought);
					return;
				}

				//Look for meditation targets
				if (!canAggNow.Any() && wantedForCombo.Any() && npc.A2H.thinkFrags.Count < MaxFrags) //TODO: check if we can move to cache
				{
					//TODO: Sort smart
					var wantedComboByPrio = new HashSet<string>(wantedForCombo.OrderBy(x => x.Key).Select(x => x.Key));
					HashSet<string> almostCombo = new HashSet<string>(wantedComboByPrio.Select(x => x));

					var thinkTarget = ThingMgr.Instance.GetThingList(g_emThingType.Item)
											.FirstOrDefault(x => x.def.Frags != null && x.def.Frags.Any(f => wantedComboByPrio.Contains(f.Frag)));
					if (thinkTarget == null)
						thinkTarget = ThingMgr.Instance.GetThingList(g_emThingType.Building)
											.FirstOrDefault(x => x.def.Frags != null && x.def.Frags.Any(f => wantedComboByPrio.Contains(f.Frag)));
					if (thinkTarget != null)
					{
						npc.AddCommand("StudyThing", thinkTarget);
						return;
					}
				}
								
			}
			catch (Exception ex)
			{
				KLog.Dbg(ex.ToString());
			}

		}

		private static bool TryFormFinalFrag(Npc npc)
		{
			var aggs = npc.A2H.thinkAggregates ?? new List<ThinkAggregate>();

			var wantedAggsByType = aggs.Where(x => IsWantedFrag(npc, x.frag))
					.Where(x => !string.IsNullOrEmpty(x.Combine)).GroupBy(x => x.Combine).ToList();

			if (wantedAggsByType.Count >= 3)
			{
				//red green blue
				//TODO: Get oldest aggs for real thought	
				List<ThinkAggregate> targetList = new List<ThinkAggregate>();
				foreach (var ag in wantedAggsByType.OrderBy(x => SortedAggTypes.IndexOf(x.Key)))
				{
					var frst = ag.FirstOrDefault();
					targetList.Add(ag.FirstOrDefault());
				}
				CombineAggs(targetList, npc);
				return true;
			}
			return false;
		}

		const int MaxFinals = 10;

		//Panel_NpcPractice._AddFNode
		private static void CombineAggs(List<ThinkAggregate> list, Npc npc)
		{
			var raceDef = HMgr.RaceInfos.GetDef(npc.RaceDefName);

			ThinkFinal thinkFinal = HMgr.GetThinkFinal(list, raceDef);
			if (thinkFinal != null)
			{
				npc.A2H.AddThinkFinal(thinkFinal);
				foreach (ThinkAggregate item in list)
				{
					npc.A2H.RemoveThinkAgg(item);
				}
				ShowMessage($"{npc.Name} combined a thought");
			}
			else
			{
				ShowMessage($"{npc.Name} ERROR COMBINE");
			}
		}


		private static void StartAggrThink(Npc npc, string fragName)
		{
			//Wnd_A2HCreateAgg.CreateAgg method
			var think2Consider = npc.A2H.thinkFrags.Where(x => x.frags[0] == fragName).ToList();

			HEFragmentDef def = IManagerModule_LoopInterval<HumanoidEvolutionMgr>.Instance.Fragments.GetDef(fragName);
			ThinkAggregate aggregate = HMgr.GetAggregate(think2Consider, F2A.SafeGet(def.Type));

			float num = 600f;
			if (npc.A2H.thinkFinals != null)
			{
				num -= (float)(50 * npc.A2H.thinkFinals.Count);
			}
			num = Mathf.Max(10f, num);
			npc.JobEngine.BeginJob(JobMgr.Instance.CreateJob("JobYsThink", null, num));
			npc.A2H.SetConsiderS(2, npc);
			MessageMgr.Instance.RemoveMessage(34001, new List<Thing> { npc });
		}

		[HarmonyPatch]
		public static class AnimalPatch
		{

			[HarmonyPostfix]
			[HarmonyPatch(typeof(HumanoidEvolutionMgr), "_NpcAddThink", new Type[] { typeof(Npc) })]
			public static void ThinkAdded(Npc npc)
			{
				ThinkIfYouCan(npc);
			}

			[HarmonyPostfix]
			[HarmonyPatch(typeof(JobStudyThing), "OnToilFinish", new Type[] { typeof(Npc) })]
			public static void ThinkAdded(ToilBase toil, g_emJobToilState state)
			{
				ThinkIfYouCan(toil.npc);
			}
		}

	}
}