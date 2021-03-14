using RimWorld;
using Verse;
using Verse.AI;

namespace Aquarium
{
    // Token: 0x02000019 RID: 25
    public class WorkGiver_AQFeedFishBowl : WorkGiver_Scanner
    {
        // Token: 0x17000014 RID: 20
        // (get) Token: 0x06000078 RID: 120 RVA: 0x00004D9C File Offset: 0x00002F9C
        public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForDef(ThingDef.Named("AQFishBowl"));

        // Token: 0x17000015 RID: 21
        // (get) Token: 0x06000079 RID: 121 RVA: 0x00004DAD File Offset: 0x00002FAD
        public override PathEndMode PathEndMode => PathEndMode.Touch;

        // Token: 0x0600007A RID: 122 RVA: 0x00004DB0 File Offset: 0x00002FB0
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
            if (CA.foodPct <= trigger && CA.numFish > 0 && AQUtility.GetClosestFeed(pawn, t) != null)
            {
                return true;
            }

            return false;
        }

        // Token: 0x0600007B RID: 123 RVA: 0x00004E0C File Offset: 0x0000300C
        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            var feeddef = DefDatabase<JobDef>.GetNamed("AQFeeding", false);
            var feed = AQUtility.GetClosestFeed(pawn, t);
            Job newJob = null;
            if (feeddef != null && feed != null)
            {
                newJob = new Job(feeddef, t, feed);
            }

            return newJob;
        }
    }
}