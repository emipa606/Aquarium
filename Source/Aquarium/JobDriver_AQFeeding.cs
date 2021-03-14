using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace Aquarium
{
    // Token: 0x0200000A RID: 10
    public class JobDriver_AQFeeding : JobDriver
    {
        // Token: 0x0400001A RID: 26
        private const TargetIndex Feedable = TargetIndex.A;

        // Token: 0x0400001B RID: 27
        private const TargetIndex Foodybits = TargetIndex.B;

        // Token: 0x0400001C RID: 28
        private const int FeedDuration = 300;

        // Token: 0x17000006 RID: 6
        // (get) Token: 0x06000041 RID: 65 RVA: 0x00003FF8 File Offset: 0x000021F8
        private Thing FeedThing => job.GetTarget(TargetIndex.A).Thing;

        // Token: 0x17000007 RID: 7
        // (get) Token: 0x06000042 RID: 66 RVA: 0x00004019 File Offset: 0x00002219
        private CompAquarium AQComp => FeedThing.TryGetComp<CompAquarium>();

        // Token: 0x17000008 RID: 8
        // (get) Token: 0x06000043 RID: 67 RVA: 0x00004028 File Offset: 0x00002228
        private Thing Food => job.GetTarget(Foodybits).Thing;

        // Token: 0x06000044 RID: 68 RVA: 0x0000404C File Offset: 0x0000224C
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(FeedThing, job, 1, -1, null, errorOnFailed) &&
                   pawn.Reserve(Food, job, 1, -1, null, errorOnFailed);
        }

        // Token: 0x06000045 RID: 69 RVA: 0x0000409D File Offset: 0x0000229D
        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedNullOrForbidden(Feedable);
            AddEndCondition(delegate
            {
                if (AQComp.foodPct > 0.95f)
                {
                    return JobCondition.Succeeded;
                }

                return JobCondition.Ongoing;
            });
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
}