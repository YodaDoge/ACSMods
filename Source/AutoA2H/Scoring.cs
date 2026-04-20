using System.Collections.Generic;
using System.Linq;
using XiaWorld;
using static XiaWorld.HumanoidEvolutionMgr;

namespace ACS_Yoda_Tweaks.AutoA2H
{
	public partial class A2H
	{
		public struct ThinkFragScoring
		{
			public int CalcScore()
			{
				int existScore = ExistsAsAgg ? -100 : 0;
				bool isEmotion = AggType == EmotionType;
				return existScore
				+ existingAggTypeCount * -22
				+ level * 11
				+ (isEmotion ? level * 5 : 0)
				//+ Math.Min(3, existingTotalCount) * 7
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

		internal static List<ThinkFragScoring> CreateThinkFragScoring(Npc npc)
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

	}
}