using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Aquarium
{
    // Token: 0x0200000F RID: 15
    public class JoyGiver_AQViewFishBowl : JoyGiver
    {
        // Token: 0x04000022 RID: 34
        private static readonly List<Thing> candidates = new();

        // Token: 0x0600005B RID: 91 RVA: 0x00004380 File Offset: 0x00002580
        public override Job TryGiveJob(Pawn pawn)
        {
            var allowedOutside = JoyUtility.EnjoyableOutsideNow(pawn);
            Job result2;
            try
            {
                var AQBowlDef = ThingDef.Named("AQFishBowl");
                candidates.AddRange(pawn.Map.listerThings.ThingsOfDef(AQBowlDef).Where(delegate(Thing thing)
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
                if (!candidates.TryRandomElementByWeight(
                    target => Mathf.Max(target.GetStatValue(StatDefOf.Beauty), 0.5f), out var result))
                {
                    result2 = null;
                }
                else
                {
                    result2 = JobMaker.MakeJob(def.jobDef, result);
                }
            }
            finally
            {
                candidates.Clear();
            }

            return result2;
        }
    }
}