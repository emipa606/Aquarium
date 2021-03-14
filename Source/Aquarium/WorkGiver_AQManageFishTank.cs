using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;

namespace Aquarium
{
    // Token: 0x0200001C RID: 28
    public class WorkGiver_AQManageFishTank : WorkGiver_Scanner
    {
        // Token: 0x1700001A RID: 26
        // (get) Token: 0x06000087 RID: 135 RVA: 0x00004FD8 File Offset: 0x000031D8
        public override ThingRequest PotentialWorkThingRequest =>
            ThingRequest.ForDef(DefsCacher.AQFishTankDefs.TakeRandom(1).First());

        // Token: 0x1700001B RID: 27
        // (get) Token: 0x06000088 RID: 136 RVA: 0x00004FE9 File Offset: 0x000031E9
        public override PathEndMode PathEndMode => PathEndMode.Touch;

        // Token: 0x06000089 RID: 137 RVA: 0x00004FEC File Offset: 0x000031EC
        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            return pawn.CanReserveAndReach(t, PathEndMode.Touch, Danger.None) &&
                   AQUtility.AddOrRemove(t, out var Add, out var fdef, out var Remove) &&
                   (Add && AQUtility.GetClosestFishInBag(pawn, fdef, t) != null || Remove);
        }

        // Token: 0x0600008A RID: 138 RVA: 0x00005034 File Offset: 0x00003234
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
                        useDef = DefDatabase<JobDef>.GetNamed("AQManagingAdd", false);
                    }
                }
                else if (Remove)
                {
                    useDef = DefDatabase<JobDef>.GetNamed("AQManagingRemove", false);
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
}