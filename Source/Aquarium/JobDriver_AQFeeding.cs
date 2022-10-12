using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace Aquarium;

public class JobDriver_AQFeeding : JobDriver
{
    private const TargetIndex Feedable = TargetIndex.A;

    private const TargetIndex Foodybits = TargetIndex.B;

    private const int FeedDuration = 300;

    private Thing FeedThing => job.GetTarget(TargetIndex.A).Thing;

    private CompAquarium AQComp => FeedThing.TryGetComp<CompAquarium>();

    private Thing Food => job.GetTarget(Foodybits).Thing;

    public override bool TryMakePreToilReservations(bool errorOnFailed)
    {
        return pawn.Reserve(FeedThing, job, 1, -1, null, errorOnFailed) &&
               pawn.Reserve(Food, job, 1, -1, null, errorOnFailed);
    }

    protected override IEnumerable<Toil> MakeNewToils()
    {
        this.FailOnDespawnedNullOrForbidden(Feedable);
        AddEndCondition(() => AQComp.foodPct > 0.95f ? JobCondition.Succeeded : JobCondition.Ongoing);
        yield return Toils_General.DoAtomic(delegate { job.count = AQUtility.GetFoodNumToFullyFeed(AQComp); });
        var reserveFood = Toils_Reserve.Reserve(Foodybits);
        yield return reserveFood;
        yield return Toils_Goto.GotoThing(Foodybits, PathEndMode.ClosestTouch)
            .FailOnDespawnedNullOrForbidden(Foodybits).FailOnSomeonePhysicallyInteracting(Foodybits);
        yield return Toils_Haul.StartCarryThing(Foodybits, false, true).FailOnDestroyedNullOrForbidden(Foodybits);
        yield return Toils_Haul.CheckForGetOpportunityDuplicate(reserveFood, Foodybits, TargetIndex.None, true);
        yield return Toils_Goto.GotoThing(Feedable, PathEndMode.Touch);
        yield return Toils_General.Wait(FeedDuration).FailOnDestroyedNullOrForbidden(Foodybits)
            .FailOnDestroyedNullOrForbidden(Feedable).FailOnCannotTouch(Feedable, PathEndMode.Touch)
            .WithProgressBarToilDelay(Feedable);
        yield return Toils_AQFeed.FinalizeFeeding(Feedable, Foodybits);
        if (!job.GetTarget(Foodybits).HasThing)
        {
            EndJobWith(JobCondition.Incompletable);
        }

        yield return Toils_Reserve.Reserve(Foodybits);
        yield return Toils_Reserve.Reserve(TargetIndex.C);
        yield return Toils_Goto.GotoThing(Foodybits, PathEndMode.ClosestTouch);
        yield return Toils_Haul.StartCarryThing(Foodybits);
        var carry = Toils_Haul.CarryHauledThingToCell(TargetIndex.C);
        yield return carry;
        yield return Toils_Haul.PlaceHauledThingInCell(TargetIndex.C, carry, true);
    }
}