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

		public struct ThinkFragScoring
		{
			public int CalcScore()
			{
				int existScore = ExistsAsAgg ? -100 : 0;
				bool isEmotion = AggType == EmotionType;
				return existScore
				+ Math.Min(3, existingTotalCount) * 12
				+ existingAggTypeCount * -20
				+ level * 11
				+ (isEmotion ? level * 5 : 0)
				+ ExistingMemories * 4; 
			}
			public List<ThinkFrag> frags;
			public string Name;
			public string AggType;
			public int existingTotalCount;
			public int existingAggTypeCount;
			public int level;
			public bool ExistsAsAgg;
			public int ExistingMemories;
			public int Score;

			public static implicit operator bool(ThinkFragScoring score)
			{
				return score.existingTotalCount > 0;
			}
		}



		private static string EmotionType = "AEmotion";
		public static void ThinkIfYouCan(Npc npc)
		{
			if (!AutoNPC.ContainsKey(npc.ID) || npc.JobEngine.CurJob?.jobdef.Name == "JobYsThink")
			{
				return;
			}

			if (npc.A2H.thinkFrags == null)
				npc.A2H.thinkFrags = new List<ThinkFrag>();
			if (npc.A2H.thinkFragCaches == null)
				npc.A2H.thinkFragCaches = new List<ThinkFrag>();
			if (npc.A2H.thinkAggregates == null)
				npc.A2H.thinkAggregates = new List<ThinkAggregate>();

			try
			{
				if (TryFormFinalFrag(npc))
					return;

				var raceDef = HMgr.RaceInfos.GetDef(npc.RaceDefName);
				List<ThinkFragScoring> scorings = CreateThinkFragScoring(npc);


				ThinkFragScoring newAggType = scorings.FirstOrDefault(x => x.existingAggTypeCount == 0 && x.existingTotalCount == 3);

				//study for new agg type
				if (!newAggType && npc.A2H.CanThinkCount <= 10)
				{
					var studyForNewAggType = scorings.Where(x => x.existingAggTypeCount == 0 && x.existingTotalCount == 2).ToList();
					if (TryStudy(npc, studyForNewAggType))
						return;
				}

				//max reached => think anything useful
				if (!newAggType && npc.A2H.CanThinkCount <= 1)
				{
					newAggType = scorings.FirstOrDefault(x => x.existingAggTypeCount <= 1 && x.existingTotalCount == 3 && !x.ExistsAsAgg);

					//study anything..
					if (!newAggType && npc.A2H.thinkFrags.Count < raceDef.MaxThink)
					{
						TryStudy(npc, scorings);
						return;
					}
				}

				if (newAggType)
				{
					RefreshMemory(scorings, npc, raceDef, newAggType.Name);

					npc.A2H.RemoveAllTState();
					newAggType.frags.ForEach(x => x.TState = 1);

					var finalThought = newAggType.Name;

					StartAggrThink(npc, finalThought);
				}

			}
			catch (Exception ex)
			{
				ShowMessage(npc?.GetName() + " " + ex.ToString());
				KLog.Dbg(ex.ToString());
			}
		}

		private static bool HasAgg(Npc npc, string name)
		{
			throw new NotImplementedException();
		}

		private static bool TryStudy(Npc npc, List<ThinkFragScoring> scorings)
		{
			if (!scorings.Any())
				return false;

			var raceDef = HMgr.RaceInfos.GetDef(npc.RaceDefName);

			var canStudyToCompletion = scorings.Where(x => x.existingTotalCount == 2 && !x.frags.Any(a => a.Conflict > 0)).ToList(); //conflict == has learned through study
			if (canStudyToCompletion.Any())
			{
				if (npc.A2H.thinkFrags.Count >= raceDef.MaxThink)  //user has to make space manually TODO: automatically remove least useful memorized thought and dismiss current think to make space
					return false;

				var thinkTarget = FindStudyTarget(canStudyToCompletion);

				var existingCmd = npc.CheckCommand("StudyThing", checkcount: true)?.FirstOrDefault(x => x != null);

				if (thinkTarget != null && existingCmd == null)
				{
					npc.AddCommand("StudyThing", thinkTarget);
					return true;
				}
			}

			return false;
		}

		private static bool first = true;

		private static List<ThinkFragScoring> CreateThinkFragScoring(Npc npc)
		{
			var wantedThoughs = npc.A2H.thinkFrags.Concat(npc.A2H.thinkFragCaches)
													.Where(x => IsWantedFrag(npc, x.frags[0]))
													.GroupBy(x => x.frags[0]);

			var existingAggs = npc.A2H.thinkAggregates.Where(x => AutoNPC[npc.ID].Contains(x.frag)).ToList();


			List<ThinkFragScoring> scorings = new List<ThinkFragScoring>();

			foreach (var frag in wantedThoughs)
			{
				var def = GetFragDef(frag.First());
				var items = frag.ToList();
				var scoring = new ThinkFragScoring()
				{
					AggType = "A" + def.Type,
					existingAggTypeCount = existingAggs.Count(a => a.Combine == "A" + def.Type),
					existingTotalCount = items.Count(),
					level = def.Level,
					ExistsAsAgg = existingAggs.Any(x => x.frags.First() == frag.Key),
					Name = frag.Key,
					ExistingMemories = npc.A2H.thinkFragCaches.Count(x => x.frags[0] == frag.Key),
					frags = items
				};
				scoring.Score = scoring.CalcScore();
				scorings.Add(scoring);
			}
			scorings = scorings.OrderByDescending(x => x.Score).ToList();
			return scorings;
		}

		private static Thing FindStudyTarget(List<ThinkFragScoring> canStudyToCompletion)
		{
			List<Thing> potentialStudyTargets = new List<Thing>();
			foreach (var thingType in studyItemTypes)
			{
				var found = ThingMgr.Instance.GetThingList(thingType)?
										.Where(thing =>
										{
											if (thing?.def?.Frags?.Any(f => canStudyToCompletion.Any(a => a.Name == f.Frag)) == true)
											{
												var item = thing as ItemThing;
												if (item == null)
													return true;
												else
												{
													if (item.EquipByWho + item.InWhoseBag + item.InWhoseHand <= 0 && item.FreeCount > 0 && item.InDark == false)
														return true;
												}
											}
											return false;

										});
				if (found != null && found.Any())
					potentialStudyTargets.AddRange(found);
			}
			Thing thinkTarget = null;
			string fragWeWillComplete = null;

			foreach (var think in canStudyToCompletion)
			{
				if (thinkTarget != null)
					break;
				thinkTarget = potentialStudyTargets.FirstOrDefault(thing => thing.def.Frags.Any(f => think.Name == f.Frag));
				fragWeWillComplete = think.Name;
			}


			return thinkTarget;
		}

		private static void RefreshMemory(List<ThinkFragScoring> scorings, Npc npc, HERaceInfoDef raceDef, string nameToIgnore = null)
		{
			//TODO: remove irrelevant
			int relevantMemories = 0;
			HashSet<string> memorizedTypes = new HashSet<string>();
			foreach (var scoredFrag in scorings)
			{
				bool shouldIgnore = scoredFrag.Name == nameToIgnore;
				bool allAreMemorized = Math.Min(scoredFrag.existingTotalCount, 3) == scoredFrag.ExistingMemories;

				if (shouldIgnore || allAreMemorized)
				{
					relevantMemories += scoredFrag.ExistingMemories;
					memorizedTypes.Add(scoredFrag.Name);
					continue;
				}
				if (relevantMemories >= raceDef.MaxThinkCache)
					break;
					
				foreach (var toMemorize in npc.A2H.thinkFrags.Where(x => x.frags[0] == scoredFrag.Name).ToList())
				{
					if (npc.A2H.thinkFragCaches.Count >= raceDef.MaxThinkCache)
					{
						//Prune one
						var canRemove = npc.A2H.thinkFragCaches.FirstOrDefault(x => !memorizedTypes.Contains(x.frags[0]) && x.frags[0] != toMemorize.frags[0]);

						//we are full, nothing can be removed
						if (canRemove == null)
							break;

						npc.A2H.thinkFragCaches.Remove(canRemove); ;
					}

					toMemorize.RemoveCountDown = raceDef.ThinkLast;
					npc.A2H.MoveThink_Think2Cache(toMemorize);
					memorizedTypes.Add(scoredFrag.Name);
					relevantMemories++;
				}

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