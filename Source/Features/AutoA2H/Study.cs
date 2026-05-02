using System.Collections.Generic;
using System.Linq;
using XiaWorld;
using static XiaWorld.HumanoidEvolutionMgr;

namespace ACS_Yoda_Tweaks.AutoA2H
{
	public partial class A2H
	{
		private static bool HasStudyCommand(Npc npc) => npc.CheckCommand("StudyThing", checkcount: true)?.Any(x => x != null) == true;

		private static g_emThingType[] studyItemTypes = new g_emThingType[] { g_emThingType.Building, g_emThingType.Item, g_emThingType.Plant, g_emThingType.None, };


		private static bool TryStudy(Npc npc, List<ThinkFragScoring> scorings)
		{
			try
			{
				if (!scorings.Any())
					return false;
				if (HasStudyCommand(npc))
					return true;

				var raceDef = HMgr.RaceInfos.GetDef(npc.RaceDefName);

				var canStudyToCompletion = scorings.Where(x => x.existingTotalCount == 2 && !x.frags.Any(a => a.Conflict > 0)).ToList(); //conflict == has learned through study
				if (canStudyToCompletion.Any())
				{
					if (npc.A2H.thinkFrags.Count >= raceDef.MaxThink)
					{
						RefreshMemory(scorings, npc, raceDef);
						if (npc.A2H.thinkFrags.Count >= raceDef.MaxThink)
							return false;
					}
					var thinkTarget = FindStudyTarget(canStudyToCompletion);

					if (thinkTarget != null)
					{
						npc.AddCommand("StudyThing", thinkTarget);
						return true;
					}
				}

			}
			catch (System.Exception ex)
			{
				ShowMessage(ex);
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