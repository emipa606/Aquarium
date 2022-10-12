using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace Aquarium;

public class JobDriver_AQCleaning : JobDriver
{
    private const TargetIndex Cleanable = TargetIndex.A;

    private Thing CleanThing => job.GetTarget(TargetIndex.A).Thing;

    private CompAquarium AQComp => CleanThing.TryGetComp<CompAquarium>();

    public override bool TryMakePreToilReservations(bool errorOnFailed)
    {
        return pawn.Reserve(CleanThing, job, 1, -1, null, errorOnFailed);
    }

    protected override IEnumerable<Toil> MakeNewToils()
    {
        this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
        AddEndCondition(() => AQComp.cleanPct > 0.95f ? JobCondition.Succeeded : JobCondition.Ongoing);
        yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
        yield return Toils_General.Wait(AQUtility.GetCleanTime(AQComp))
            .FailOnDestroyedNullOrForbidden(TargetIndex.A).FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch)
            .WithProgressBarToilDelay(TargetIndex.A).WithEffect(EffecterDefOf.Clean, TargetIndex.A)
            .PlaySustainerOrSound(() => SoundDefOf.Interact_CleanFilth);
        yield return Toils_AQClean.FinalizeCleaning(TargetIndex.A);
    }
}