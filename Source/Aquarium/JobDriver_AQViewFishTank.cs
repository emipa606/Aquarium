using System;
using RimWorld;
using Verse;
using Verse.AI;

namespace Aquarium
{
	// Token: 0x0200000E RID: 14
	public class JobDriver_AQViewFishTank : JobDriver_VisitJoyThing
	{
		// Token: 0x1700000F RID: 15
		// (get) Token: 0x06000058 RID: 88 RVA: 0x000042CC File Offset: 0x000024CC
		private Thing FishyThing
		{
			get
			{
				return job.GetTarget(TargetIndex.A).Thing;
			}
		}

		// Token: 0x06000059 RID: 89 RVA: 0x000042F0 File Offset: 0x000024F0
		protected override void WaitTickAction()
		{
			float num = AQUtility.GetJoyGainFactor(FishyThing.GetStatValue(StatDefOf.Beauty, true) / FishyThing.def.GetStatValueAbstract(StatDefOf.Beauty, null), FishyThing);
			float extraJoyGainFactor = (num > 0f) ? num : 0f;
			pawn.GainComfortFromCellIfPossible(false);
			JoyUtility.JoyTickCheckEnd(pawn, JoyTickFullJoyAction.EndJob, extraJoyGainFactor, (Building)FishyThing);
			AQUtility.ApplyMoodBoostAndInspire(pawn, FishyThing);
		}
	}
}
