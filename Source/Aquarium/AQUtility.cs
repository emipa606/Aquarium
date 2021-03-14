using System;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Aquarium
{
    // Token: 0x02000003 RID: 3
    public class AQUtility
    {
        // Token: 0x04000002 RID: 2
        internal const float AvgFishValue = 30f;

        // Token: 0x04000003 RID: 3
        internal const int FoodVal = 10;

        private static int hammeringCounter;

        // Token: 0x06000005 RID: 5 RVA: 0x00002128 File Offset: 0x00000328
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
                    Math.Min(CompAquarium.oldFishAge, CompAquarium.NumValuePart(listing, 3)) / CompAquarium.oldFishAge);
            }

            return beautyFactor * fishFactor * agefactor;
        }

        // Token: 0x06000006 RID: 6 RVA: 0x00002210 File Offset: 0x00000410
        internal static void ApplyMoodBoostAndInspire(Pawn pawn, Thing FishyThing)
        {
            var CompAQ = FishyThing.TryGetComp<CompAquarium>();
            var needs = pawn.needs;
            if (needs?.mood != null)
            {
                var fishFactor = 1f;
                var agefactor = 1f;
                var compare = 30f;
                if (CompAQ != null && CompAQ.numFish > 0 && pawn.IsHashIntervalTick(1000))
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
                                CompAquarium.oldFishAge);
                        }
                    }

                    if (IsInspired((int) (CompAQ.numFish * 25 * fishFactor * agefactor)))
                    {
                        var fishRelaxDef = ThoughtDef.Named("AQObserveFish");
                        pawn.needs.mood.thoughts.memories.TryGainMemory(fishRelaxDef);
                    }
                }
            }

            if (CompAQ == null || CompAQ.numFish <= 0 || !Controller.Settings.AllowInspire ||
                !pawn.IsHashIntervalTick(1000) || pawn.IsPrisoner ||
                !IsInspired((int) (CompAQ.numFish * Controller.Settings.BaseInspChance)) ||
                pawn.mindState.inspirationHandler.Inspired)
            {
                return;
            }

            var IDef = (from x in DefDatabase<InspirationDef>.AllDefsListForReading
                where x.Worker.InspirationCanOccur(pawn)
                select x).RandomElementByWeightWithFallback(x => x.Worker.CommonalityFor(pawn));
            pawn.mindState.inspirationHandler.TryStartInspiration_NewTemp(IDef);
        }

        // Token: 0x06000007 RID: 7 RVA: 0x00002438 File Offset: 0x00000638
        private static bool IsInspired(int chance)
        {
            return CompAquarium.RandomFloat(1f, 100f) < chance;
        }

        // Token: 0x06000008 RID: 8 RVA: 0x00002450 File Offset: 0x00000650
        internal static int GetFoodNumToFullyFeed(CompAquarium AQComp)
        {
            return Math.Max(0, (int) ((1f - AQComp.foodPct) * 10f * Math.Max(0, AQComp.numFish)));
        }

        // Token: 0x06000009 RID: 9 RVA: 0x00002479 File Offset: 0x00000679
        internal static int GetCleanTime(CompAquarium AQComp)
        {
            return Math.Max(120, (int) ((1f - AQComp.cleanPct) * 10f * 2f * 60f));
        }

        // Token: 0x0600000A RID: 10 RVA: 0x000024A4 File Offset: 0x000006A4
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

        // Token: 0x0600000B RID: 11 RVA: 0x00002548 File Offset: 0x00000748
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

        // Token: 0x0600000C RID: 12 RVA: 0x00002644 File Offset: 0x00000844
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

                num = Math.Max(1, (int) ((1f - CA.foodPct) * 10f * Math.Max(1, CA.numFish)));
            }

            if (num <= 0)
            {
                return null;
            }

            var potentials = p.Map.listerThings.ThingsOfDef(ThingDef.Named("AQFishFood"));
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

        // Token: 0x0600000D RID: 13 RVA: 0x0000277C File Offset: 0x0000097C
        internal static bool HasFish(Thing thing)
        {
            var CAQ = thing.TryGetComp<CompAquarium>();
            return CAQ != null && CAQ.numFish > 0;
        }

        // Token: 0x0600000E RID: 14 RVA: 0x000027A0 File Offset: 0x000009A0
        internal static bool IsValidAquaRoom(Pawn pawn, Room room)
        {
            return room.Role != RoomRoleDefOf.Bedroom && room.Role != RoomRoleDefOf.PrisonCell &&
                   room.Role != RoomRoleDefOf.PrisonBarracks ||
                   pawn.ownership?.OwnedRoom != null && pawn.ownership.OwnedRoom == room || pawn.IsPrisoner &&
                   (room.Role == RoomRoleDefOf.PrisonCell || room.Role == RoomRoleDefOf.PrisonBarracks) &&
                   room == pawn.Position.GetRoom(pawn.Map);
        }

        // Token: 0x0600000F RID: 15 RVA: 0x00002834 File Offset: 0x00000A34
        internal static void DoSpawnTropicalFishMeat(Thing parent, int age)
        {
            if (!parent.Spawned || parent.Map == null)
            {
                return;
            }

            var stack = Math.Max(1, (int) Mathf.Lerp(1f, 10f, age / (float) CompAquarium.oldFishAge));
            var TPMode = ThingPlaceMode.Near;
            var thing = ThingMaker.MakeThing(ThingDef.Named("AQFishMeat"));
            thing.stackCount = Math.Min(thing.def.stackLimit, stack);
            GenPlace.TryPlaceThing(thing, parent.Position, parent.Map, TPMode);
        }

        // Token: 0x06000010 RID: 16 RVA: 0x000028C0 File Offset: 0x00000AC0
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
}