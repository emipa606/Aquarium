﻿using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace Aquarium
{
    // Token: 0x02000009 RID: 9
    public class JobDriver_AQCleaning : JobDriver
    {
        // Token: 0x04000019 RID: 25
        private const TargetIndex Cleanable = TargetIndex.A;

        // Token: 0x17000004 RID: 4
        // (get) Token: 0x0600003B RID: 59 RVA: 0x00003F78 File Offset: 0x00002178
        private Thing CleanThing => job.GetTarget(TargetIndex.A).Thing;

        // Token: 0x17000005 RID: 5
        // (get) Token: 0x0600003C RID: 60 RVA: 0x00003F99 File Offset: 0x00002199
        private CompAquarium AQComp => CleanThing.TryGetComp<CompAquarium>();

        // Token: 0x0600003D RID: 61 RVA: 0x00003FA6 File Offset: 0x000021A6
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(CleanThing, job, 1, -1, null, errorOnFailed);
        }

        // Token: 0x0600003E RID: 62 RVA: 0x00003FC8 File Offset: 0x000021C8
        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            AddEndCondition(delegate
            {
                if (AQComp.cleanPct > 0.95f)
                {
                    return JobCondition.Succeeded;
                }

                return JobCondition.Ongoing;
            });
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
            yield return Toils_General.Wait(AQUtility.GetCleanTime(AQComp))
                .FailOnDestroyedNullOrForbidden(TargetIndex.A).FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch)
                .WithProgressBarToilDelay(TargetIndex.A).WithEffect(EffecterDefOf.Clean, TargetIndex.A)
                .PlaySustainerOrSound(() => SoundDefOf.Interact_CleanFilth);
            yield return Toils_AQClean.FinalizeCleaning(TargetIndex.A);
        }
    }
}