using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace Aquarium;

public class Toils_AQAdding
{
    public static Toil FinalizeAdding(TargetIndex addable, TargetIndex fish)
    {
        var toil = new Toil();
        toil.initAction = delegate
        {
            var curJob = toil.actor.CurJob;
            var tankThing = curJob.GetTarget(addable).Thing;
            var fishThing = curJob.GetTarget(fish).Thing;
            if (tankThing != null && fishThing != null)
            {
                var AQComp = tankThing.TryGetComp<CompAquarium>();
                if (AQComp != null)
                {
                    var newList = new List<string>();
                    var newIndex = 0;
                    if (AQComp.fishData.Count > 0)
                    {
                        var tryToAdd = true;
                        foreach (var value in AQComp.fishData)
                        {
                            var prevDefVal = CompAquarium.StringValuePart(value, 1);
                            var prevHealth = CompAquarium.NumValuePart(value, 2);
                            var prevAge = CompAquarium.NumValuePart(value, 3);
                            var prevAct = CompAquarium.NumValuePart(value, 4);
                            newIndex++;
                            if (prevAct == 1 && tryToAdd &&
                                (prevDefVal == fishThing.def.defName || prevDefVal == "AQRandomFish"))
                            {
                                var health = 100;
                                var age = 0;
                                var CBag = fishThing.TryGetComp<CompAQFishInBag>();
                                if (CBag != null)
                                {
                                    health = CBag.fishhealth;
                                    age = CBag.age;
                                }

                                var newValue = CompAquarium.CreateValuePart(newIndex, fishThing.def.defName, health,
                                    age, 0);
                                newList.Add(newValue);
                                AQComp.numFish++;
                                tryToAdd = false;
                            }
                            else
                            {
                                var newValue3 = CompAquarium.CreateValuePart(newIndex, prevDefVal, prevHealth,
                                    prevAge, prevAct);
                                newList.Add(newValue3);
                            }
                        }
                    }

                    AQComp.fishData = newList;
                    AQComp.GenerateBeauty(AQComp.fishData);
                }
            }

            fishThing?.Destroy();

            toil.actor.skills.Learn(SkillDefOf.Animals, 75f);
        };
        toil.defaultCompleteMode = ToilCompleteMode.Instant;
        return toil;
    }
}