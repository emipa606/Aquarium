using System;
using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace Aquarium
{
    // Token: 0x02000016 RID: 22
    public class Toils_AQRemoving
    {
        // Token: 0x0600006C RID: 108 RVA: 0x00004C1C File Offset: 0x00002E1C
        public static Toil FinalizeRemoving(TargetIndex RemoveFrom)
        {
            Toil toil = new Toil();
            toil.initAction = delegate ()
            {
                Job curJob = toil.actor.CurJob;
                Thing thing = curJob.GetTarget(RemoveFrom).Thing;
                Thing removedThing = null;
                CompAquarium AQComp = thing.TryGetComp<CompAquarium>();
                if (AQComp != null && AQComp.fishData.Count > 0)
                {
                    List<string> newList = new List<string>();
                    int newIndex = 0;
                    bool removed = false;
                    foreach (string value in AQComp.fishData)
                    {
                        newIndex++;
                        string prevDefVal = CompAquarium.StringValuePart(value, 1);
                        int prevHealth = CompAquarium.NumValuePart(value, 2);
                        int prevAge = CompAquarium.NumValuePart(value, 3);
                        int prevAct = CompAquarium.NumValuePart(value, 4);
                        if (prevAct == 2 && !removed)
                        {
                            Thing thing2 = ThingMaker.MakeThing(ThingDef.Named(prevDefVal), null);
                            thing2.stackCount = 1;
                            GenPlace.TryPlaceThing(thing2, toil.actor.Position, toil.actor.Map, ThingPlaceMode.Near, out Thing newFishBag, null, null, default);
                            removed = true;
                            toil.actor.skills.Learn(SkillDefOf.Animals, 80f, false);
                            newIndex--;
                            CompAQFishInBag CBag = newFishBag.TryGetComp<CompAQFishInBag>();
                            newFishBag.stackCount = 1;
                            if (CBag != null)
                            {
                                CBag.fishhealth = prevHealth;
                                CBag.age = prevAge;
                            }
                            removedThing = newFishBag;
                        }
                        else
                        {
                            string newValue = CompAquarium.CreateValuePart(newIndex, prevDefVal, prevHealth, prevAge, prevAct);
                            newList.Add(newValue);
                        }
                    }
                    if (removed)
                        AQComp.numFish--;
                    AQComp.fishData = newList;
                }
                if (removedThing != null)
                {
                    StoragePriority currentPriority = StoreUtility.CurrentStoragePriorityOf(removedThing);
                    if (StoreUtility.TryFindBestBetterStoreCellFor(removedThing, toil.actor, toil.actor.Map, currentPriority, toil.actor.Faction, out IntVec3 c, true))
                    {
                        curJob.SetTarget(TargetIndex.B, removedThing);
                        curJob.count = removedThing.stackCount;
                        curJob.SetTarget(TargetIndex.C, c);
                        return;
                    }
                    if (curJob.GetCachedDriverDirect != null)
                        curJob.GetCachedDriverDirect.EndJobWith(JobCondition.Incompletable);
                }
            };
            toil.defaultCompleteMode = ToilCompleteMode.Instant;
            return toil;
        }
    }
}
