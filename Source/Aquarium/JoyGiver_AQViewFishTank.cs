using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Aquarium;

public class JoyGiver_AQViewFishTank : JoyGiver
{
    private static readonly List<Thing> candidates = new List<Thing>();

    public override Job TryGiveJob(Pawn pawn)
    {
        var allowedOutside = JoyUtility.EnjoyableOutsideNow(pawn);
        foreach (var tankDef in DefsCacher.AQFishTankDefs)
        {
            try
            {
                candidates.AddRange(pawn.Map.listerThings.ThingsOfDef(tankDef).Where(delegate(Thing thing)
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
            }
            catch
            {
                // ignored
            }
        }

        var result2 =
            !candidates.TryRandomElementByWeight(target => Mathf.Max(target.GetStatValue(StatDefOf.Beauty), 0.5f),
                out var result)
                ? null
                : JobMaker.MakeJob(def.jobDef, result);
        candidates.Clear();
        return result2;
    }
}