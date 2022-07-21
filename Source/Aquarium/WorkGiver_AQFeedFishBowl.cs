using RimWorld;
using Verse;
using Verse.AI;

namespace Aquarium;

public class WorkGiver_AQFeedFishBowl : WorkGiver_Scanner
{
    public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForDef(DefsCacher.AQFishBowlDef);

    public override PathEndMode PathEndMode => PathEndMode.Touch;

    public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
    {
        if (!pawn.CanReserveAndReach(t, PathEndMode.Touch, Danger.None))
        {
            return false;
        }

        var ca = t.TryGetComp<CompAquarium>();
        if (ca == null)
        {
            return false;
        }

        var trigger = Controller.Settings.RespondFood / 100f;
        return ca.foodPct <= trigger && AQUtility.GetAmountOfFish(ca.fishData) > 0 &&
               AQUtility.GetClosestFeed(pawn, t) != null;
    }

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