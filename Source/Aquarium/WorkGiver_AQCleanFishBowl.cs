using System;
using RimWorld;
using Verse;
using Verse.AI;

namespace Aquarium
{
	// Token: 0x02000017 RID: 23
	public class WorkGiver_AQCleanFishBowl : WorkGiver_Scanner
	{
		// Token: 0x17000010 RID: 16
		// (get) Token: 0x0600006E RID: 110 RVA: 0x00004C72 File Offset: 0x00002E72
		public override ThingRequest PotentialWorkThingRequest
		{
			get
			{
				return ThingRequest.ForDef(ThingDef.Named("AQFishBowl"));
			}
		}

		// Token: 0x17000011 RID: 17
		// (get) Token: 0x0600006F RID: 111 RVA: 0x00004C83 File Offset: 0x00002E83
		public override PathEndMode PathEndMode
		{
			get
			{
				return PathEndMode.Touch;
			}
		}

		// Token: 0x06000070 RID: 112 RVA: 0x00004C88 File Offset: 0x00002E88
		public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
			if (pawn.CanReserveAndReach(t, PathEndMode.Touch, Danger.None, 1, -1, null, false))
			{
				CompAquarium CA = t.TryGetComp<CompAquarium>();
				if (CA != null)
				{
					float trigger = Controller.Settings.RespondClean / 100f;
					if (CA.cleanPct <= trigger)
					{
						return true;
					}
				}
			}
			return false;
		}

		// Token: 0x06000071 RID: 113 RVA: 0x00004CD4 File Offset: 0x00002ED4
		public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
			JobDef cleandef = DefDatabase<JobDef>.GetNamed("AQCleaning", false);
			Job newJob = null;
			if (cleandef != null)
			{
				newJob = new Job(cleandef, t);
			}
			return newJob;
		}
	}
}
