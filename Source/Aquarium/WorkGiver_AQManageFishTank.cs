using RimWorld;
using Verse;
using Verse.AI;

namespace Aquarium;

public class WorkGiver_AQManageFishTank : WorkGiver_Scanner
{
    public override ThingRequest PotentialWorkThingRequest =>
        ThingRequest.ForGroup(ThingRequestGroup.BuildingArtificial);

    public override PathEndMode PathEndMode => PathEndMode.Touch;

    public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
    {
        return pawn.CanReserveAndReach(t, PathEndMode.Touch, Danger.None) &&
               AQUtility.AddOrRemove(t, out var Add, out var fdef, out var Remove) &&
               (Add && AQUtility.GetClosestFishInBag(pawn, fdef, t) != null || Remove);
    }

    public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
    {
        Thing f = null;
        JobDef useDef = null;
        if (AQUtility.AddOrRemove(t, out var Add, out var fAddDef, out var Remove))
        {
            if (Add)
            {
                f = AQUtility.GetClosestFishInBag(pawn, fAddDef, t);
                if (f != null)
                {
                    useDef = DefsCacher.AQManagingAddDef;
                }
            }
            else if (Remove)
            {
                useDef = DefsCacher.AQManagingRemoveDef;
            }
        }

        Job newJob = null;
        if (useDef != null)
        {
            newJob = new Job(useDef, t, f);
        }

        return newJob;
    }
}