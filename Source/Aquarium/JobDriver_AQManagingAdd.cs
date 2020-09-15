using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace Aquarium
{
	// Token: 0x0200000B RID: 11
	public class JobDriver_AQManagingAdd : JobDriver
	{
		// Token: 0x17000009 RID: 9
		// (get) Token: 0x06000049 RID: 73 RVA: 0x000040E4 File Offset: 0x000022E4
		protected Thing AddThing
		{
			get
			{
				return job.GetTarget(AddTo).Thing;
			}
		}

		// Token: 0x1700000A RID: 10
		// (get) Token: 0x0600004A RID: 74 RVA: 0x00004105 File Offset: 0x00002305
		protected CompAquarium AQComp
		{
			get
			{
				return AddThing.TryGetComp<CompAquarium>();
			}
		}

		// Token: 0x1700000B RID: 11
		// (get) Token: 0x0600004B RID: 75 RVA: 0x00004114 File Offset: 0x00002314
		protected Thing Fish
		{
			get
			{
				return job.GetTarget(FishToAdd).Thing;
			}
		}

		// Token: 0x0600004C RID: 76 RVA: 0x00004138 File Offset: 0x00002338
		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return pawn.Reserve(AddThing, job, 1, -1, null, errorOnFailed) && pawn.Reserve(Fish, job, 1, -1, null, errorOnFailed);
		}

		// Token: 0x0600004D RID: 77 RVA: 0x00004189 File Offset: 0x00002389
		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOnDespawnedNullOrForbidden(AddTo);
			yield return Toils_General.DoAtomic(delegate
			{
				job.count = 1;
			});
			Toil reserveFish = Toils_Reserve.Reserve(FishToAdd, 1, -1, null);
			yield return reserveFish;
			yield return Toils_Goto.GotoThing(FishToAdd, PathEndMode.ClosestTouch).FailOnDespawnedNullOrForbidden(FishToAdd).FailOnSomeonePhysicallyInteracting(FishToAdd);
			yield return Toils_Haul.StartCarryThing(FishToAdd, false, true, false).FailOnDestroyedNullOrForbidden(FishToAdd);
			yield return Toils_Haul.CheckForGetOpportunityDuplicate(reserveFish, FishToAdd, TargetIndex.None, true, null);
			yield return Toils_Goto.GotoThing(AddTo, PathEndMode.Touch);
			yield return Toils_General.Wait(AddDuration, TargetIndex.None).FailOnDestroyedNullOrForbidden(FishToAdd).FailOnDestroyedNullOrForbidden(AddTo).FailOnCannotTouch(AddTo, PathEndMode.Touch).WithProgressBarToilDelay(AddTo, false, -0.5f);
			yield return Toils_AQAdding.FinalizeAdding(AddTo, FishToAdd);
			yield break;
		}

		// Token: 0x0400001D RID: 29
		private const TargetIndex AddTo = TargetIndex.A;

		// Token: 0x0400001E RID: 30
		private const TargetIndex FishToAdd = TargetIndex.B;

		// Token: 0x0400001F RID: 31
		private const int AddDuration = 720;
	}
}
