using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using XiaWorld;
using static XiaWorld.HumanoidEvolutionMgr;

namespace ACS_Yoda_Tweaks.AutoA2H
{
	public partial class A2H
	{
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
				if (newAggType)
				{
					RefreshMemory(scorings, npc, raceDef, newAggType.Name);

					npc.A2H.RemoveAllTState();
					newAggType.frags.ForEach(x => x.TState = 1);

					var finalThought = newAggType.Name;

					StartAggrThink(npc, finalThought);
					return;
				}


				if (npc.A2H.thinkFragCaches.Count < 5 && npc.A2H.CanThinkCount <= 0)
					RefreshMemory(scorings, npc, raceDef);

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



			}
			catch (Exception ex)
			{
				ShowMessage(npc?.GetName() + " " + ex.ToString());
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
			if (!npc.CheckKeyStayOK(npc.Key))
			{
				var walksafe = JobMgr.Instance.CreateJob("JobGoToWalkable", null, null);
				npc.JobEngine.BeginJob(walksafe);
			}
			npc.JobEngine.SetNextJob("JobYsThink", null, num);
			npc.A2H.SetConsiderS(2, npc);
			MessageMgr.Instance.RemoveMessage(34001, new List<Thing> { npc });
		}
	}
}