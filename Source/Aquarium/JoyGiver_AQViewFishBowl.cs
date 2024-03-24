using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Aquarium;

public class JoyGiver_AQViewFishBowl : JoyGiver
{
    private static readonly List<Thing> candidates = [];

    public override Job TryGiveJob(Pawn pawn)
    {
        var allowedOutside = JoyUtility.EnjoyableOutsideNow(pawn);
        Job result2;
        try
        {
            candidates.AddRange(pawn.Map.listerThings.ThingsOfDef(DefsCacher.AQFishBowlDef).Where(delegate(Thing thing)
            {
                if (!AQUtility.HasFish(thing))
                {
                    return false;
                }

                if (thing.Faction != Faction.OfPlayer || thing.IsForbidden(pawn) ||
                    !allowedOutside && !thing.Position.Roofed(thing.Map) ||
                    !pawn.CanReserveAndReach(thing, PathEndMode.Touch, Danger.None) ||
                    !thing.IsPoliticallyProper(pawn))
                {
                    return false;
                }

                var room = thing.GetRoom();
                return room != null && AQUtility.IsValidAquaRoom(pawn, room);
            }));
            result2 = !candidates.TryRandomElementByWeight(
                target => Mathf.Max(target.GetStatValue(StatDefOf.Beauty), 0.5f), out var result)
                ? null
                : JobMaker.MakeJob(def.jobDef, result);
        }
        finally
        {
            candidates.Clear();
        }

        return result2;
    }
}