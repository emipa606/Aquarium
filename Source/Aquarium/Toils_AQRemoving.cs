using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace Aquarium;

public class Toils_AQRemoving
{
    public static Toil FinalizeRemoving(TargetIndex RemoveFrom)
    {
        var toil = new Toil();
        toil.initAction = delegate
        {
            var curJob = toil.actor.CurJob;
            var thing = curJob.GetTarget(RemoveFrom).Thing;
            Thing removedThing = null;
            var aqComp = thing.TryGetComp<CompAquarium>();
            if (aqComp != null && aqComp.fishData.Count > 0)
            {
                var newList = new List<string>();
                var newIndex = 0;
                var removed = false;
                foreach (var value in aqComp.fishData)
                {
                    newIndex++;
                    var prevDefVal = CompAquarium.StringValuePart(value, 1);
                    var prevHealth = CompAquarium.NumValuePart(value, 2);
                    var prevAge = CompAquarium.NumValuePart(value, 3);
                    var prevAct = CompAquarium.NumValuePart(value, 4);
                    if (prevAct == 2 && !removed)
                    {
                        var thing2 = ThingMaker.MakeThing(ThingDef.Named(prevDefVal));
                        thing2.stackCount = 1;
                        GenPlace.TryPlaceThing(thing2, toil.actor.Position, toil.actor.Map, ThingPlaceMode.Near,
                            out var newFishBag);
                        removed = true;
                        toil.actor.skills.Learn(SkillDefOf.Animals, 80f);
                        newIndex--;
                        var cBag = newFishBag.TryGetComp<CompAQFishInBag>();
                        newFishBag.stackCount = 1;
                        if (cBag != null)
                        {
                            cBag.fishhealth = prevHealth;
                            cBag.age = prevAge;
                        }

                        removedThing = newFishBag;
                    }
                    else
                    {
                        var newValue =
                            CompAquarium.CreateValuePart(newIndex, prevDefVal, prevHealth, prevAge, prevAct);
                        newList.Add(newValue);
                    }
                }

                if (removed)
                {
                    aqComp.numFish--;
                }

                aqComp.fishData = newList;
                aqComp.GenerateBeauty(aqComp.fishData);
            }

            if (removedThing == null)
            {
                return;
            }

            var currentPriority = StoreUtility.CurrentStoragePriorityOf(removedThing);
            if (StoreUtility.TryFindBestBetterStoreCellFor(removedThing, toil.actor, toil.actor.Map,
                    currentPriority, toil.actor.Faction, out var c))
            {
                curJob.SetTarget(TargetIndex.B, removedThing);
                curJob.count = removedThing.stackCount;
                curJob.SetTarget(TargetIndex.C, c);
                return;
            }

            curJob.GetCachedDriverDirect?.EndJobWith(JobCondition.Incompletable);
        };
        toil.defaultCompleteMode = ToilCompleteMode.Instant;
        return toil;
    }
}