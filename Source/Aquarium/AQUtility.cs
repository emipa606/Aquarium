using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Aquarium;

public class AQUtility
{
    internal const float AvgFishValue = 30f;

    internal const int FoodVal = 10;

    private static int hammeringCounter;

    internal static float GetJoyGainFactor(float beautyFactor, Thing FishyThing)
    {
        var fishFactor = 1f;
        var agefactor = 1f;
        var compare = 30f;
        var CompAQ = FishyThing.TryGetComp<CompAquarium>();
        if (CompAQ == null)
        {
            return beautyFactor * fishFactor * agefactor;
        }

        var list = CompAQ.fishData;
        if (list.Count <= 0)
        {
            return beautyFactor * fishFactor * agefactor;
        }

        foreach (var listing in list)
        {
            var fishdef = ThingDef.Named(CompAquarium.StringValuePart(listing, 1));
            if (fishdef == null)
            {
                continue;
            }

            var value = Math.Max(10f, Math.Min(50f, fishdef.BaseMarketValue));
            fishFactor *= value / compare;
            agefactor *= Mathf.Lerp(0.75f, 1f,
                Math.Min(CompAquarium.oldFishAge, CompAquarium.NumValuePart(listing, 3)) /
                (float)CompAquarium.oldFishAge);
        }

        return beautyFactor * fishFactor * agefactor;
    }

    internal static void ApplyMoodBoostAndInspire(Pawn pawn, Thing FishyThing)
    {
        var CompAQ = FishyThing.TryGetComp<CompAquarium>();
        var needs = pawn.needs;
        if (needs?.mood != null)
        {
            var fishFactor = 1f;
            var agefactor = 1f;
            var compare = 30f;
            if (CompAQ is { numFish: > 0 } && pawn.IsHashIntervalTick(1000))
            {
                var list = CompAQ.fishData;
                if (list.Count > 0)
                {
                    foreach (var listing in list)
                    {
                        var fishdef = ThingDef.Named(CompAquarium.StringValuePart(listing, 1));
                        if (fishdef == null)
                        {
                            continue;
                        }

                        var value = Math.Max(10f, Math.Min(50f, fishdef.BaseMarketValue));
                        fishFactor *= value / compare;
                        agefactor *= Mathf.Lerp(0.75f, 1f,
                            Math.Min(CompAquarium.oldFishAge, CompAquarium.NumValuePart(listing, 3)) /
                            (float)CompAquarium.oldFishAge);
                    }
                }

                if (IsInspired((int)(CompAQ.numFish * 25 * fishFactor * agefactor)))
                {
                    var fishRelaxDef = ThoughtDef.Named("AQObserveFish");
                    pawn.needs.mood.thoughts.memories.TryGainMemory(fishRelaxDef);
                }
            }
        }

        if (CompAQ == null || CompAQ.numFish <= 0 || !Controller.Settings.AllowInspire ||
            !pawn.IsHashIntervalTick(1000) || pawn.IsPrisoner ||
            !IsInspired((int)(CompAQ.numFish * Controller.Settings.BaseInspChance)) ||
            pawn.mindState.inspirationHandler.Inspired)
        {
            return;
        }

        var IDef = (from x in DefDatabase<InspirationDef>.AllDefsListForReading
            where x.Worker.InspirationCanOccur(pawn)
            select x).RandomElementByWeightWithFallback(x => x.Worker.CommonalityFor(pawn));
        pawn.mindState.inspirationHandler.TryStartInspiration(IDef);
    }

    private static bool IsInspired(int chance)
    {
        return CompAquarium.RandomFloat(1f, 100f) < chance;
    }

    internal static int GetFoodNumToFullyFeed(CompAquarium AQComp)
    {
        return Math.Max(0, (int)((1f - AQComp.foodPct) * 10f * Math.Max(0, AQComp.numFish)));
    }

    internal static int GetCleanTime(CompAquarium AQComp)
    {
        return Math.Max(120, (int)((1f - AQComp.cleanPct) * 10f * 2f * 60f));
    }

    internal static bool AddOrRemove(Thing t, out bool Add, out ThingDef fishAddDef, out bool Remove)
    {
        Remove = Add = false;
        fishAddDef = null;
        var CA = t.TryGetComp<CompAquarium>();
        if (CA == null)
        {
            return false;
        }

        var listing = CA.fishData;
        if (listing.Count <= 0)
        {
            return false;
        }

        foreach (var value in listing)
        {
            var action = CompAquarium.NumValuePart(value, 4);
            if (action == 1)
            {
                Add = true;
                var fishDefString = CompAquarium.StringValuePart(value, 1);
                if (fishDefString == "AQRandomFish")
                {
                    if (hammeringCounter != 0)
                    {
                        hammeringCounter--;
                        return false;
                    }

                    var reachableFish = CA.ReachableDefs;
                    if (reachableFish.Count > 0)
                    {
                        fishDefString = reachableFish.RandomElement();
                    }
                    else
                    {
                        hammeringCounter = 20;
                        return false;
                    }
                }

                fishAddDef = ThingDef.Named(fishDefString);
                return true;
            }

            if (action != 2)
            {
                continue;
            }

            Remove = true;
            return true;
        }

        return false;
    }

    internal static Thing GetClosestFishInBag(Pawn p, ThingDef def, Thing t)
    {
        var potentials = p.Map.listerThings.ThingsOfDef(ThingDef.Named(def.defName));
        if (potentials.Count <= 0)
        {
            return null;
        }

        Thing bestThing = null;
        var bestScore = 0f;
        foreach (var potential in potentials)
        {
            if (!potential.Spawned)
            {
                continue;
            }

            var CBag = potential.TryGetComp<CompAQFishInBag>();
            if (CBag == null || !p.CanReserveAndReach(potential, PathEndMode.Touch, Danger.None))
            {
                continue;
            }

            var ticksLeft = CBag.ticksInBagRemain;
            var distance = p.Position.DistanceTo(potential.Position);
            var score = 1f / ticksLeft * Mathf.Lerp(1f, 0.01f, distance / 9999f);
            if (!(score > bestScore))
            {
                continue;
            }

            bestScore = score;
            bestThing = potential;
        }

        return bestThing;
    }

    internal static Thing GetClosestFeed(Pawn p, Thing t)
    {
        var CA = t.TryGetComp<CompAquarium>();
        var num = 0;
        if (CA != null)
        {
            if (CA.numFish < 1)
            {
                return null;
            }

            num = Math.Max(1, (int)((1f - CA.foodPct) * 10f * Math.Max(1, CA.numFish)));
        }

        if (num <= 0)
        {
            return null;
        }

        var potentials = p.Map.listerThings.ThingsOfDef(DefsCacher.AQFishFoodDef);
        Thing BestThing = null;
        var BestScore = 0f;
        if (potentials.Count <= 0)
        {
            return null;
        }

        foreach (var potential in potentials)
        {
            if (!potential.Spawned || !p.CanReserveAndReach(potential, PathEndMode.Touch, Danger.None))
            {
                continue;
            }

            var stackFactor = 1f;
            if (potential.stackCount < num)
            {
                stackFactor = 0.5f;
            }

            var distance = p.Position.DistanceTo(potential.Position);
            var score = Mathf.Lerp(1f, 0.01f, distance / 9999f) * stackFactor;
            if (!(score > BestScore))
            {
                continue;
            }

            BestScore = score;
            BestThing = potential;
        }

        return BestThing;
    }

    internal static bool HasFish(Thing thing)
    {
        var CAQ = thing.TryGetComp<CompAquarium>();
        return CAQ != null && GetAmountOfFish(CAQ.fishData) > 0;
    }

    internal static int GetAmountOfFish(List<string> fishData)
    {
        if (fishData == null || fishData.Count == 0)
        {
            return 0;
        }

        var counter = 0;
        foreach (var text in fishData)
        {
            if (CompAquarium.NumValuePart(text, 4) == 1)
            {
                continue;
            }

            if (string.IsNullOrEmpty(CompAquarium.StringValuePart(text, 1)))
            {
                continue;
            }

            if (CompAquarium.StringValuePart(text, 1) == "AQRandomFish")
            {
                continue;
            }

            counter++;
        }

        return counter;
    }

    internal static bool IsValidAquaRoom(Pawn pawn, Room room)
    {
        return room.Role != RoomRoleDefOf.Bedroom && room.Role != RoomRoleDefOf.PrisonCell &&
               room.Role != RoomRoleDefOf.PrisonBarracks ||
               pawn.ownership?.OwnedRoom != null && pawn.ownership.OwnedRoom == room || pawn.IsPrisoner &&
               (room.Role == RoomRoleDefOf.PrisonCell || room.Role == RoomRoleDefOf.PrisonBarracks) &&
               room == pawn.Position.GetRoom(pawn.Map);
    }

    internal static void DoSpawnTropicalFishMeat(Thing parent, int age)
    {
        if (!parent.Spawned || parent.Map == null)
        {
            return;
        }

        var stack = Math.Max(1, (int)Mathf.Lerp(1f, 10f, age / (float)CompAquarium.oldFishAge));
        var TPMode = ThingPlaceMode.Near;
        var thing = ThingMaker.MakeThing(DefsCacher.AQFishMeatDef);
        thing.stackCount = Math.Min(thing.def.stackLimit, stack);
        GenPlace.TryPlaceThing(thing, parent.Position, parent.Map, TPMode);
    }

    internal static void DebugFishData(CompAquarium CompAQ, int maxNum = 0)
    {
        Log.Message(string.Concat(CompAQ.parent.Label, " : ", CompAQ.numFish.ToString(), " : ", maxNum.ToString()));
        var list = CompAQ.fishData;
        if (list.Count <= 0)
        {
            return;
        }

        foreach (var text in list)
        {
            Log.Message(text);
        }
    }
}