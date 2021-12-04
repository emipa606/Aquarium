using RimWorld;
using Verse;
using Verse.AI;

namespace Aquarium;

public class WorkGiver_AQCleanFishTank : WorkGiver_Scanner
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

        var trigger = Controller.Settings.RespondClean / 100f;
        if (CA.cleanPct <= trigger)
        {
            return true;
        }

        return false;
    }

    public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
    {
        return new Job(DefsCacher.AQCleaningDef, t);
    }
}