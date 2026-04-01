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
using static XiaWorld.HumanoidEvolutionMgr;

namespace ACS_Yoda_Tweaks.AutoA2H
{
	public partial class A2H : Mod
	{
		public override Meta Info => _info;
		public const string Name = "AutoA2H";
		private static Meta _info = new Meta(Name, "Animal Thoughts Automation", true);

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

		private static HEFragmentDef GetFragDef(ThinkFrag frag)
		{
			return HMgr.Fragments.GetDef(frag.frags[0]);
		}

		private static HEFragmentDef GetFragDef(IGrouping<string, ThinkFrag> frags) => GetFragDef(frags.First());

		private struct ThinkFragScoring
		{
			string Name;
			string AggType;
			int existingCount;
			int existingAggTypeCount;
			object Reference;

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

				var raceDef = HMgr.RaceInfos.GetDef(npc.RaceDefName);

				var wantedThoughs = npc.A2H.thinkFrags.Concat(npc.A2H.thinkFragCaches)
										.Where(x => IsWantedFrag(npc, x.frags[0])).GroupBy(x => x.frags[0]);



				var existingAggs = npc.A2H.thinkAggregates ?? new List<ThinkAggregate>();
				existingAggs = existingAggs.Where(x => AutoNPC[npc.ID].Contains(x.frag)).ToList();

				var potentialAggs = wantedThoughs.Where(x => x.Count() >= 2)
						.OrderBy(c => existingAggs.Count(existingAgg => existingAgg.Combine == GetFragDef(c).Type)) //amount of aggs we have asc
						.ThenByDescending(x => GetFragDef(x).Level).ToList(); //then level desc

				var readyAggs = potentialAggs.Where(x => x.Count() >= 3).ToList();
				bool isExistingAggType = readyAggs.Any() && existingAggs.Any(x => GetFragDef(readyAggs.FirstOrDefault()).Type != x.Combine);

				if (isExistingAggType && npc.A2H.thinkFrags.Count < Math.Min(readyAggs.Count * 10, 32)) //get more frags for diversity
					return;


				var aggAboutToExpire = existingAggs.FirstOrDefault(x => x.RemoveCountDown <= 2);
				if (aggAboutToExpire != null)
				{
					//Ignore all types which we already have
					var existingTypes = existingAggs.Select(x => x.Combine).Distinct().ToList();
					potentialAggs.RemoveAll(toRemove => existingTypes.Contains(GetFragDef(toRemove).Type));

					//forget all for which we have type and expires soon
					npc.A2H.thinkFragCaches.RemoveAll(x => x.RemoveCountDown <= 3 && GetFragDef(x).Type == aggAboutToExpire.Combine);

					if (npc.A2H.thinkFragCaches.Count >= raceDef.MaxThinkCache)
					{
						//remove  low lvl memories von denen wir schon ein agg typ haben
					}
				}

				if (readyAggs.Any())
				{
					var target = readyAggs.First();

					int freeMemorySlots = raceDef.MaxThinkCache - npc.A2H.thinkFragCaches.Count;
					if (freeMemorySlots > 0)
					{
						var toMemorize = npc.A2H.thinkFrags.Where(x => x.frags[0] != target.Key && IsWantedFrag(npc, x.frags[0]))
							.Skip(1).Take(freeMemorySlots).ToList();

						foreach (var f in toMemorize)
						{
							npc.A2H.MoveThink_Think2Cache(f);
							f.RemoveCountDown = raceDef.ThinkLast;

						}
					}

					npc.A2H.RemoveAllTState();

					foreach (var f in target)
					{
						f.TState = 1;
					}

					var finalThought = readyAggs.First().Key;

					StartAggrThink(npc, finalThought);
					return;
				}

				//Look for meditation targets
				if (potentialAggs.Any() && npc.A2H.thinkFrags.Count < raceDef.MaxThink)
				{
					var wantedComboByPrio = potentialAggs.FirstOrDefault();
					var fragsToStudy = new List<string>(potentialAggs.Select(x => x.Key));
					List<Thing> potentialStudyTargets = new List<Thing>();

					foreach (var thingType in studyItemTypes)
					{
						var found = ThingMgr.Instance.GetThingList(thingType)?
												.Where(x =>
												{
													if (x?.def?.Frags?.Any(f => fragsToStudy.Contains(f.Frag)) == true)
													{
														var itm = x as ItemThing;
														if (itm == null)
															return true;
														if (itm.EquipByWho + itm.InWhoseBag + itm.InWhoseHand <= 0 && itm.FreeCount > 0 && itm.InDark == false)
															return true;
													}
													return false;

												});
						if (found != null)
							potentialStudyTargets.AddRange(found);
					}

					var thinkTarget = potentialStudyTargets.OrderBy(x => fragsToStudy.IndexOf(x.def.Frags[0].Frag)).FirstOrDefault();
					if (thinkTarget != null && npc.CheckCommand("StudyThing") == null)
					{

						ShowMessage($"{npc.GetName()} Study  " + thinkTarget.GetName() + " for " + thinkTarget.def.Frags[0].Frag);
						npc.AddCommand("StudyThing", thinkTarget);
						return;
					}
				}

			}
			catch (Exception ex)
			{
				ShowMessage(ex.ToString());
				KLog.Dbg(ex.ToString());
			}
		}



		private static g_emThingType[] studyItemTypes = new g_emThingType[] { g_emThingType.Building, g_emThingType.Item, g_emThingType.Plant, g_emThingType.None, };

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

			//called by study behaviour
			[HarmonyPostfix]
			[HarmonyPatch(typeof(HumanoidEvolutionMgr), "_NpcAddThink", new Type[] { typeof(Npc), typeof(ThinkFrag) })]
			public static void ThinkAdded(Npc npc, ThinkFrag newThink)
			{
				ThinkIfYouCan(npc);
			}

			//[HarmonyPostfix]
			//[HarmonyPatch(typeof(JobStudyThing), "OnToilFinish")]
			//public static void ThinkAdded(ToilBase toil, g_emJobToilState state)
			//{
			//	ThinkIfYouCan(toil.npc);
			//}
		}

	}
}