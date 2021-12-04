using RimWorld;
using Verse;
using Verse.AI;

namespace Aquarium;

public class WorkGiver_AQFeedFishTank : WorkGiver_Scanner
{
    public override ThingRequest PotentialWorkThingRequest =>
        ThingRequest.ForGroup(ThingRequestGroup.BuildingArtificial);

    public override PathEndMode PathEndMode => PathEndMode.Touch;

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