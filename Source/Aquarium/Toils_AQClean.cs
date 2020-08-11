using System;
using RimWorld;
using Verse;
using Verse.AI;

namespace Aquarium
{
	// Token: 0x02000014 RID: 20
	public class Toils_AQClean
	{
		// Token: 0x06000068 RID: 104 RVA: 0x00004B64 File Offset: 0x00002D64
		public static Toil FinalizeCleaning(TargetIndex cleanable)
		{
			Toil toil = new Toil();
			toil.initAction = delegate()
			{
				CompAquarium AQComp = toil.actor.CurJob.GetTarget(cleanable).Thing.TryGetComp<CompAquarium>();
				if (AQComp != null)
				{
					AQComp.cleanPct = 1f;
					toil.actor.skills.Learn(SkillDefOf.Animals, 50f, false);
				}
			};
			toil.defaultCompleteMode = ToilCompleteMode.Instant;
			return toil;
		}
	}
}
