using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace Aquarium;

public class JobDriver_AQManagingAdd : JobDriver
{
    private const TargetIndex AddTo = TargetIndex.A;

    private const TargetIndex FishToAdd = TargetIndex.B;

    private const int AddDuration = 720;

    private Thing AddThing => job.GetTarget(AddTo).Thing;

    protected CompAquarium AQComp => AddThing.TryGetComp<CompAquarium>();

    private Thing Fish => job.GetTarget(FishToAdd).Thing;

    public override bool TryMakePreToilReservations(bool errorOnFailed)
    {
        return pawn.Reserve(AddThing, job, 1, -1, null, errorOnFailed) &&
               pawn.Reserve(Fish, job, 1, -1, null, errorOnFailed);
    }

    protected override IEnumerable<Toil> MakeNewToils()
    {
        this.FailOnDespawnedNullOrForbidden(AddTo);
        yield return Toils_General.DoAtomic(delegate { job.count = 1; });
        var reserveFish = Toils_Reserve.Reserve(FishToAdd);
        yield return reserveFish;
        yield return Toils_Goto.GotoThing(FishToAdd, PathEndMode.ClosestTouch)
            .FailOnDespawnedNullOrForbidden(FishToAdd).FailOnSomeonePhysicallyInteracting(FishToAdd);
        yield return Toils_Haul.StartCarryThing(FishToAdd, false, true).FailOnDestroyedNullOrForbidden(FishToAdd);
        yield return Toils_Haul.CheckForGetOpportunityDuplicate(reserveFish, FishToAdd, TargetIndex.None, true);
        yield return Toils_Goto.GotoThing(AddTo, PathEndMode.Touch);
        yield return Toils_General.Wait(AddDuration).FailOnDestroyedNullOrForbidden(FishToAdd)
            .FailOnDestroyedNullOrForbidden(AddTo).FailOnCannotTouch(AddTo, PathEndMode.Touch)
            .WithProgressBarToilDelay(AddTo);
        yield return Toils_AQAdding.FinalizeAdding(AddTo, FishToAdd);
    }
}