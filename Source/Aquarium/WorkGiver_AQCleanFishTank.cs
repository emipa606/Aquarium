using System;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;

namespace Aquarium
{
	// Token: 0x02000018 RID: 24
	public class WorkGiver_AQCleanFishTank : WorkGiver_Scanner
	{
		// Token: 0x17000012 RID: 18
		// (get) Token: 0x06000073 RID: 115 RVA: 0x00004D08 File Offset: 0x00002F08
		public override ThingRequest PotentialWorkThingRequest
		{
			get
			{
				return ThingRequest.ForDef((from tankDef in DefDatabase<ThingDef>.AllDefsListForReading where tankDef.defName.StartsWith("AQFishTank") select tankDef).ToList().TakeRandom(1).First());
			}
		}

		// Token: 0x17000013 RID: 19
		// (get) Token: 0x06000074 RID: 116 RVA: 0x00004D19 File Offset: 0x00002F19
		public override PathEndMode PathEndMode
		{
			get
			{
				return PathEndMode.Touch;
			}
		}

		// Token: 0x06000075 RID: 117 RVA: 0x00004D1C File Offset: 0x00002F1C
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

		// Token: 0x06000076 RID: 118 RVA: 0x00004D68 File Offset: 0x00002F68
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
