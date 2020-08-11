using System;
using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace Aquarium
{
	// Token: 0x02000016 RID: 22
	public class Toils_AQRemoving
	{
		// Token: 0x0600006C RID: 108 RVA: 0x00004C1C File Offset: 0x00002E1C
		public static Toil FinalizeRemoving(TargetIndex RemoveFrom)
		{
			Toil toil = new Toil();
			toil.initAction = delegate()
			{
				Job curJob = toil.actor.CurJob;
				Thing thing = curJob.GetTarget(RemoveFrom).Thing;
				Thing removedThing = null;
				CompAquarium AQComp = thing.TryGetComp<CompAquarium>();
				if (AQComp != null && AQComp.fishData.Count > 0)
				{
					List<string> newList = new List<string>();
					int newIndex = 0;
					bool removed = false;
					foreach (string value in AQComp.fishData)
					{
						newIndex++;
						string prevDefVal = CompAquarium.stringValuePart(value, 1);
						int prevHealth = CompAquarium.numValuePart(value, 2);
						int prevAge = CompAquarium.numValuePart(value, 3);
						int prevAct = CompAquarium.numValuePart(value, 4);
						if (prevAct == 2 && !removed)
						{
							Thing thing2 = ThingMaker.MakeThing(ThingDef.Named(prevDefVal), null);
							thing2.stackCount = 1;
							Thing newFishBag;
							GenPlace.TryPlaceThing(thing2, toil.actor.Position, toil.actor.Map, ThingPlaceMode.Near, out newFishBag, null, null, default(Rot4));
							removed = true;
							AQComp.numFish--;
							toil.actor.skills.Learn(SkillDefOf.Animals, 80f, false);
							newIndex--;
							CompAQFishInBag CBag = newFishBag.TryGetComp<CompAQFishInBag>();
							if (CBag != null)
							{
								CBag.fishhealth = prevHealth;
								CBag.age = prevAge;
							}
							removedThing = newFishBag;
						}
						else
						{
							string newValue = CompAquarium.CreateValuePart(newIndex, prevDefVal, prevHealth, prevAge, prevAct);
							newList.Add(newValue);
						}
					}
					AQComp.fishData = newList;
				}
				if (removedThing != null)
				{
					StoragePriority currentPriority = StoreUtility.CurrentStoragePriorityOf(removedThing);
					IntVec3 c;
					if (StoreUtility.TryFindBestBetterStoreCellFor(removedThing, toil.actor, toil.actor.Map, currentPriority, toil.actor.Faction, out c, true))
					{
						curJob.SetTarget(TargetIndex.B, removedThing);
						curJob.count = removedThing.stackCount;
						curJob.SetTarget(TargetIndex.C, c);
						return;
					}
					curJob.GetCachedDriverDirect.EndJobWith(JobCondition.Incompletable);
				}
			};
			toil.defaultCompleteMode = ToilCompleteMode.Instant;
			return toil;
		}
	}
}
