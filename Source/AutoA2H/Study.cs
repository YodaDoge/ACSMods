using System.Collections.Generic;
using System.Linq;
using XiaWorld;
using static XiaWorld.HumanoidEvolutionMgr;

namespace ACS_Yoda_Tweaks.AutoA2H
{
	public partial class A2H
	{

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
	}
}