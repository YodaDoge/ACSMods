using HarmonyLib;
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
			if (!IsAutoNPC(npc))
			{
				return;
			}

			if (npc.JobEngine.CurJob?.jobdef.Name == "JobYsThink")
			{
				MessageMgr.Instance.RemoveMessage(34001, new List<Thing> { npc });
				return;
			}

			InitNullLists(npc);
			try
			{
				if (TryFormFinalFrag(npc))
					return;

				List<ThinkFragScoring> scorings = CreateThinkFragScoring(npc);

				ThinkFragScoring newAgg = scorings.FirstOrDefault(x => x.existingTotalCount >= 3 && (x.existingAggTypeCount == 0 || npc.A2H.CanThinkCount == 0));
				if (newAgg)
				{
					var raceDef = HMgr.RaceInfos.GetDef(npc.RaceDefName);
					RefreshMemory(scorings, npc, raceDef, newAgg.Name);

					npc.A2H.RemoveAllTState();
					newAgg.frags.ForEach(x => x.TState = 1);

					var finalThought = newAgg.Name;

					StartAggrThink(npc, finalThought);
					return;
				}

				if (npc.A2H.CanThinkCount <= 10)
				{
					var studyForNewAggType = scorings.Where(x => x.existingAggTypeCount == 0 && x.existingTotalCount == 2).ToList();
					if (TryStudy(npc, studyForNewAggType))
					{
						MessageMgr.Instance.RemoveMessage(34001, new List<Thing> { npc });
						return;
					}
				}

				//study anything we keep score of
				if (npc.A2H.CanThinkCount <= 1)
				{
					RefreshMemory(scorings, npc);
					if (TryStudy(npc, scorings))
					{
						MessageMgr.Instance.RemoveMessage(34001, new List<Thing> { npc });
					}
				}
			}
			catch (Exception ex)
			{
				ShowMessage(npc?.GetName() + " " + ex);
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

		private static Dictionary<string, string> FragAggName2AggDefName = new Dictionary<string, string>
			{
				{ "Scene", "AScene" },
				{ "Target", "ATarget" },
				{ "Emotion", "AEmotion" }
			};

		private static void StartAggrThink(Npc npc, string fragName)
		{
			//Wnd_A2HCreateAgg.CreateAgg method
			var think2Consider = npc.A2H.thinkFrags.Where(x => x.frags[0] == fragName).ToList();

			HEFragmentDef def = IManagerModule_LoopInterval<HumanoidEvolutionMgr>.Instance.Fragments.GetDef(fragName);
			ThinkAggregate aggregate = HMgr.GetAggregate(think2Consider, FragAggName2AggDefName.SafeGet(def.Type));

			float num = 600f;
			if (npc.A2H.thinkFinals != null)
			{
				num -= (float)(50 * npc.A2H.thinkFinals.Count);
			}
			num = Mathf.Max(10f, num);

			var thinkJob = JobMgr.Instance.CreateJob("JobYsThink", null, num);
			npc.JobEngine.BeginJob(thinkJob);
			npc.A2H.SetConsiderS(2, npc);
			MessageMgr.Instance.RemoveMessage(34001, new List<Thing> { npc });
		}

		[HarmonyPatch]
		public static class ThinkPatch
		{
			//called by study behaviour
			[HarmonyPostfix]
			[HarmonyPatch(typeof(JobYsThink), "GetToilList")]
			public static void GetToilList(JobYsThink __instance, ref List<ToilBase> __result)
			{
				if (!_info.Enabled)
					return;
				var npc = __instance.Worker;
				//using JobGoToWalkable code here
				if (!npc.CheckKeyStayOK(npc.Key))
				{
					var safeKey = WorldMgr.Instance.curWorld.map.GetWalkableAround(npc.Key, 50, noself: true, 0, (int key) => npc.CheckKeyStayOK(key));
					var toilGo = ToilGoto.GotoGrid(safeKey, (npc.map.CheckPath(npc.Key, safeKey, g_emPathEndMode.OnPos, nearest: false, fogpath: false) > 0) ? g_emPathEndMode.OnPos : g_emPathEndMode.Immediately);
					__result.Insert(0, toilGo);
				}
			}
		}
	}
}