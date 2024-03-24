using RimWorld;
using Verse;
using Verse.AI;

namespace Aquarium;

public class WorkGiver_AQManageFishBowl : WorkGiver_Scanner
{
    public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForDef(DefsCacher.AQFishBowlDef);

    public override PathEndMode PathEndMode => PathEndMode.Touch;

    public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
    {
        if (!pawn.CanReserveAndReach(t, PathEndMode.Touch, Danger.None))
        {
            return false;
        }

        if (!AQUtility.AddOrRemove(t, out var add, out var fishDef, out var remove))
        {
            return false;
        }

        return add && AQUtility.GetClosestFishInBag(pawn, fishDef, t) != null || remove;
    }

    public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
    {
        Thing f = null;
        JobDef useDef = null;
        if (AQUtility.AddOrRemove(t, out var add, out var fishDef, out var remove))
        {
            if (add)
            {
                f = AQUtility.GetClosestFishInBag(pawn, fishDef, t);
                if (f != null)
                {
                    useDef = DefsCacher.AQManagingAddDef;
                }
            }
            else if (remove)
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