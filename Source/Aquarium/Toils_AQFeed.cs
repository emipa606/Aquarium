using System;
using RimWorld;
using Verse;
using Verse.AI;

namespace Aquarium
{
	// Token: 0x02000015 RID: 21
	public class Toils_AQFeed
	{
		// Token: 0x0600006A RID: 106 RVA: 0x00004BBC File Offset: 0x00002DBC
		public static Toil FinalizeFeeding(TargetIndex feedable, TargetIndex food)
		{
			Toil toil = new Toil();
			toil.initAction = delegate()
			{
				Job curJob = toil.actor.CurJob;
				Thing thing = curJob.GetTarget(feedable).Thing;
				Thing excessThing = null;
				CompAquarium AQComp = thing.TryGetComp<CompAquarium>();
				if (AQComp != null)
				{
					int numFoodToAdd = AQUtility.GetFoodNumToFullyFeed(AQComp);
					if (numFoodToAdd > 0)
					{
						Thing foodybits = curJob.GetTarget(food).Thing;
						if (foodybits != null)
						{
							int numFoodHave = foodybits.stackCount;
							int numAdding;
							if (numFoodHave <= numFoodToAdd)
							{
								numAdding = numFoodHave;
								foodybits.Destroy(DestroyMode.Vanish);
							}
							else
							{
								numAdding = numFoodToAdd;
								foodybits.stackCount -= numAdding;
                                GenPlace.TryPlaceThing(foodybits, toil.actor.Position, toil.actor.Map, ThingPlaceMode.Near, out Thing newFoodDrop, null, null, default);
                                excessThing = newFoodDrop;
							}
							AQComp.foodPct += (float)numAdding * 0.1f;
							if (AQComp.foodPct > 1f)
							{
								AQComp.foodPct = 1f;
							}
							toil.actor.skills.Learn(SkillDefOf.Animals, 35f, false);
						}
					}
				}
				if (excessThing == null)
				{
					curJob.SetTarget(TargetIndex.B, null);
					return;
				}
				StoragePriority currentPriority = StoreUtility.CurrentStoragePriorityOf(excessThing);
                if (StoreUtility.TryFindBestBetterStoreCellFor(excessThing, toil.actor, toil.actor.Map, currentPriority, toil.actor.Faction, out IntVec3 c, true))
                {
                    curJob.SetTarget(TargetIndex.B, excessThing);
                    curJob.count = excessThing.stackCount;
                    curJob.SetTarget(TargetIndex.C, c);
                    return;
                }
                curJob.SetTarget(TargetIndex.B, null);
			};
			toil.defaultCompleteMode = ToilCompleteMode.Instant;
			return toil;
		}
	}
}
