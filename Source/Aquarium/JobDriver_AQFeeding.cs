using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace Aquarium
{
	// Token: 0x0200000A RID: 10
	public class JobDriver_AQFeeding : JobDriver
	{
		// Token: 0x17000006 RID: 6
		// (get) Token: 0x06000041 RID: 65 RVA: 0x00003FF8 File Offset: 0x000021F8
		protected Thing FeedThing
		{
			get
			{
				return this.job.GetTarget(TargetIndex.A).Thing;
			}
		}

		// Token: 0x17000007 RID: 7
		// (get) Token: 0x06000042 RID: 66 RVA: 0x00004019 File Offset: 0x00002219
		protected CompAquarium AQComp
		{
			get
			{
				return this.FeedThing.TryGetComp<CompAquarium>();
			}
		}

		// Token: 0x17000008 RID: 8
		// (get) Token: 0x06000043 RID: 67 RVA: 0x00004028 File Offset: 0x00002228
		protected Thing Food
		{
			get
			{
				return this.job.GetTarget(TargetIndex.B).Thing;
			}
		}

		// Token: 0x06000044 RID: 68 RVA: 0x0000404C File Offset: 0x0000224C
		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return this.pawn.Reserve(this.FeedThing, this.job, 1, -1, null, errorOnFailed) && this.pawn.Reserve(this.Food, this.job, 1, -1, null, errorOnFailed);
		}

		// Token: 0x06000045 RID: 69 RVA: 0x0000409D File Offset: 0x0000229D
		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
			base.AddEndCondition(delegate
			{
				if (this.AQComp.foodPct > 0.95f)
				{
					return JobCondition.Succeeded;
				}
				return JobCondition.Ongoing;
			});
			yield return Toils_General.DoAtomic(delegate
			{
				this.job.count = AQUtility.GetFoodNumToFullyFeed(this.AQComp);
			});
			Toil reserveFood = Toils_Reserve.Reserve(TargetIndex.B, 1, -1, null);
			yield return reserveFood;
			yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.ClosestTouch).FailOnDespawnedNullOrForbidden(TargetIndex.B).FailOnSomeonePhysicallyInteracting(TargetIndex.B);
			yield return Toils_Haul.StartCarryThing(TargetIndex.B, false, true, false).FailOnDestroyedNullOrForbidden(TargetIndex.B);
			yield return Toils_Haul.CheckForGetOpportunityDuplicate(reserveFood, TargetIndex.B, TargetIndex.None, true, null);
			yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
			yield return Toils_General.Wait(300, TargetIndex.None).FailOnDestroyedNullOrForbidden(TargetIndex.B).FailOnDestroyedNullOrForbidden(TargetIndex.A).FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch).WithProgressBarToilDelay(TargetIndex.A, false, -0.5f);
			yield return Toils_AQFeed.FinalizeFeeding(TargetIndex.A, TargetIndex.B);
			if (!this.job.GetTarget(TargetIndex.B).HasThing)
			{
				base.EndJobWith(JobCondition.Incompletable);
			}
			yield return Toils_Reserve.Reserve(TargetIndex.B, 1, -1, null);
			yield return Toils_Reserve.Reserve(TargetIndex.C, 1, -1, null);
			yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.ClosestTouch);
			yield return Toils_Haul.StartCarryThing(TargetIndex.B, false, false, false);
			Toil carry = Toils_Haul.CarryHauledThingToCell(TargetIndex.C);
			yield return carry;
			yield return Toils_Haul.PlaceHauledThingInCell(TargetIndex.C, carry, true, false);
			yield break;
		}

		// Token: 0x0400001A RID: 26
		private const TargetIndex Feedable = TargetIndex.A;

		// Token: 0x0400001B RID: 27
		private const TargetIndex Foodybits = TargetIndex.B;

		// Token: 0x0400001C RID: 28
		private const int FeedDuration = 300;
	}
}
