using RimWorld;
using Verse;
using Verse.AI;

namespace Aquarium;

public class Toils_AQFeed
{
    public static Toil FinalizeFeeding(TargetIndex feedable, TargetIndex food)
    {
        var toil = new Toil();
        toil.initAction = delegate
        {
            var curJob = toil.actor.CurJob;
            var thing = curJob.GetTarget(feedable).Thing;
            Thing excessThing = null;
            var AQComp = thing.TryGetComp<CompAquarium>();
            if (AQComp != null)
            {
                var numFoodToAdd = AQUtility.GetFoodNumToFullyFeed(AQComp);
                if (numFoodToAdd > 0)
                {
                    var foodybits = curJob.GetTarget(food).Thing;
                    if (foodybits != null)
                    {
                        var numFoodHave = foodybits.stackCount;
                        int numAdding;
                        if (numFoodHave <= numFoodToAdd)
                        {
                            numAdding = numFoodHave;
                            foodybits.Destroy();
                        }
                        else
                        {
                            numAdding = numFoodToAdd;
                            foodybits.stackCount -= numAdding;
                            GenPlace.TryPlaceThing(foodybits, toil.actor.Position, toil.actor.Map,
                                ThingPlaceMode.Near, out var newFoodDrop);
                            excessThing = newFoodDrop;
                        }

                        AQComp.foodPct += numAdding * 0.1f;
                        if (AQComp.foodPct > 1f)
                        {
                            AQComp.foodPct = 1f;
                        }

                        toil.actor.skills.Learn(SkillDefOf.Animals, 35f);
                    }
                }
            }

            if (excessThing == null)
            {
                curJob.SetTarget(TargetIndex.B, null);
                return;
            }

            var currentPriority = StoreUtility.CurrentStoragePriorityOf(excessThing);
            if (StoreUtility.TryFindBestBetterStoreCellFor(excessThing, toil.actor, toil.actor.Map, currentPriority,
                    toil.actor.Faction, out var c))
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