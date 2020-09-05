using System;
using RimWorld;
using Verse;
using Verse.AI;

namespace Aquarium
{
	// Token: 0x0200001B RID: 27
	public class WorkGiver_AQManageFishBowl : WorkGiver_Scanner
	{
		// Token: 0x17000018 RID: 24
		// (get) Token: 0x06000082 RID: 130 RVA: 0x00004F09 File Offset: 0x00003109
		public override ThingRequest PotentialWorkThingRequest
		{
			get
			{
				return ThingRequest.ForDef(ThingDef.Named("AQFishBowl"));
			}
		}

		// Token: 0x17000019 RID: 25
		// (get) Token: 0x06000083 RID: 131 RVA: 0x00004F1A File Offset: 0x0000311A
		public override PathEndMode PathEndMode
		{
			get
			{
				return PathEndMode.Touch;
			}
		}

		// Token: 0x06000084 RID: 132 RVA: 0x00004F20 File Offset: 0x00003120
		public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
            return pawn.CanReserveAndReach(t, PathEndMode.Touch, Danger.None, 1, -1, null, false) && AQUtility.AddOrRemove(t, out bool Add, out ThingDef fdef, out bool Remove) && ((Add && AQUtility.GetClosestFishInBag(pawn, fdef, t) != null) || Remove);
        }

		// Token: 0x06000085 RID: 133 RVA: 0x00004F68 File Offset: 0x00003168
		public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
			Thing f = null;
			JobDef useDef = null;
            if (AQUtility.AddOrRemove(t, out bool Add, out ThingDef fAddDef, out bool Remove))
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
