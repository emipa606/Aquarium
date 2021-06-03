﻿using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;

namespace Aquarium
{
    // Token: 0x0200001A RID: 26
    public class WorkGiver_AQFeedFishTank : WorkGiver_Scanner
    {
        // Token: 0x17000016 RID: 22
        // (get) Token: 0x0600007D RID: 125 RVA: 0x00004E51 File Offset: 0x00003051
        public override ThingRequest PotentialWorkThingRequest =>
            ThingRequest.ForDef(DefsCacher.AQFishTankDefs.TakeRandom(1).First());

        // Token: 0x17000017 RID: 23
        // (get) Token: 0x0600007E RID: 126 RVA: 0x00004E62 File Offset: 0x00003062
        public override PathEndMode PathEndMode => PathEndMode.Touch;

        // Token: 0x0600007F RID: 127 RVA: 0x00004E68 File Offset: 0x00003068
        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            if (!pawn.CanReserveAndReach(t, PathEndMode.Touch, Danger.None))
            {
                return false;
            }

            var CA = t.TryGetComp<CompAquarium>();
            if (CA == null)
            {
                return false;
            }

            var trigger = Controller.Settings.RespondFood / 100f;
            if (CA.foodPct <= trigger && AQUtility.GetAmountOfFish(CA.fishData) > 0 &&
                AQUtility.GetClosestFeed(pawn, t) != null)
            {
                return true;
            }

            return false;
        }

        // Token: 0x06000080 RID: 128 RVA: 0x00004EC4 File Offset: 0x000030C4
        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            var feed = AQUtility.GetClosestFeed(pawn, t);
            Job newJob = null;
            if (feed != null)
            {
                newJob = new Job(DefsCacher.AQFeedingDef, t, feed);
            }

            return newJob;
        }
    }
}