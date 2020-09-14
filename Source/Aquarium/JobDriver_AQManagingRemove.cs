using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace Aquarium
{
	// Token: 0x0200000C RID: 12
	public class JobDriver_AQManagingRemove : JobDriver
	{
		// Token: 0x1700000C RID: 12
		// (get) Token: 0x06000050 RID: 80 RVA: 0x000041B0 File Offset: 0x000023B0
		protected Thing Thing
		{
			get
			{
				return job.GetTarget(TargetIndex.A).Thing;
			}
		}

		// Token: 0x1700000D RID: 13
		// (get) Token: 0x06000051 RID: 81 RVA: 0x000041D1 File Offset: 0x000023D1
		protected CompAquarium AQComp
		{
			get
			{
				return Thing.TryGetComp<CompAquarium>();
			}
		}

		// Token: 0x06000052 RID: 82 RVA: 0x000041DE File Offset: 0x000023DE
		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return pawn.Reserve(Thing, job, 1, -1, null, errorOnFailed);
		}

		// Token: 0x06000053 RID: 83 RVA: 0x00004200 File Offset: 0x00002400
		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
			yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
			yield return Toils_General.Wait(600, TargetIndex.None).FailOnDestroyedNullOrForbidden(TargetIndex.A).FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch).WithProgressBarToilDelay(TargetIndex.A, false, -0.5f);
			yield return Toils_AQRemoving.FinalizeRemoving(TargetIndex.A);
			yield return Toils_Reserve.Reserve(TargetIndex.B, 1, -1, null);
			yield return Toils_Reserve.Reserve(TargetIndex.C, 1, -1, null);
			yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.ClosestTouch);
			yield return Toils_Haul.StartCarryThing(TargetIndex.B, false, false, false);
			Toil carry = Toils_Haul.CarryHauledThingToCell(TargetIndex.C);
			yield return carry;
			yield return Toils_Haul.PlaceHauledThingInCell(TargetIndex.C, carry, true, false);
			yield break;
		}

		// Token: 0x04000020 RID: 32
		private const TargetIndex RemoveFrom = TargetIndex.A;

		// Token: 0x04000021 RID: 33
		private const int RemoveDuration = 600;
	}
}
