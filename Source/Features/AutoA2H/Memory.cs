using System;
using System.Collections.Generic;
using System.Linq;
using XiaWorld;
using static XiaWorld.HumanoidEvolutionMgr;

namespace ACS_Yoda_Tweaks.AutoA2H
{
	public partial class A2H
	{

		private static void RefreshMemory(List<ThinkFragScoring> scorings, Npc npc, HERaceInfoDef raceDef = null, string nameToIgnore = null)
		{
			if (raceDef == null)
				raceDef =  HMgr.RaceInfos.GetDef(npc.RaceDefName);

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
					if (relevantMemories >= raceDef.MaxThinkCache)
						break;
					else
						continue;
				}

				foreach (var toMemorize in npc.A2H.thinkFrags.Where(x => x.frags[0] == scoredFrag.Name).ToList())
				{
					if (npc.A2H.thinkFragCaches.Count >= raceDef.MaxThinkCache)
					{
						//Prune one which is either not wanted or lesser in scoring
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
				if (relevantMemories >= raceDef.MaxThinkCache)
					break;

			}
		}
	}
}