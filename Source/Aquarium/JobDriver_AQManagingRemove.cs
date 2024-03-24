using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace Aquarium;

public class JobDriver_AQManagingRemove : JobDriver
{
    private const TargetIndex RemoveFrom = TargetIndex.A;

    private const int RemoveDuration = 600;

    private Thing Thing => job.GetTarget(RemoveFrom).Thing;

    protected CompAquarium AQComp => Thing.TryGetComp<CompAquarium>();

    public override bool TryMakePreToilReservations(bool errorOnFailed)
    {
        return pawn.Reserve(Thing, job, 1, -1, null, errorOnFailed);
    }

    protected override IEnumerable<Toil> MakeNewToils()
    {
        this.FailOnDespawnedNullOrForbidden(RemoveFrom);
        yield return Toils_Goto.GotoThing(RemoveFrom, PathEndMode.Touch);
        yield return Toils_General.Wait(RemoveDuration).FailOnDestroyedNullOrForbidden(RemoveFrom)
            .FailOnCannotTouch(RemoveFrom, PathEndMode.Touch).WithProgressBarToilDelay(RemoveFrom);
        yield return Toils_AQRemoving.FinalizeRemoving(RemoveFrom);
        yield return Toils_Reserve.Reserve(TargetIndex.B);
        yield return Toils_Reserve.Reserve(TargetIndex.C);
        yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.ClosestTouch);
        yield return Toils_Haul.StartCarryThing(TargetIndex.B);
        var carry = Toils_Haul.CarryHauledThingToCell(TargetIndex.C);
        yield return carry;
        yield return Toils_Haul.PlaceHauledThingInCell(TargetIndex.C, carry, true);
    }
}