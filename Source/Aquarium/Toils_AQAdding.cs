using System;
using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace Aquarium
{
	// Token: 0x02000013 RID: 19
	public class Toils_AQAdding
	{
		// Token: 0x06000066 RID: 102 RVA: 0x00004B04 File Offset: 0x00002D04
		public static Toil FinalizeAdding(TargetIndex addable, TargetIndex fish)
		{
			Toil toil = new Toil();
			toil.initAction = delegate()
			{
				Job curJob = toil.actor.CurJob;
				Thing thing = curJob.GetTarget(addable).Thing;
				Thing fishThing = curJob.GetTarget(fish).Thing;
				if (thing != null && fishThing != null)
				{
					CompAquarium AQComp = thing.TryGetComp<CompAquarium>();
					if (AQComp != null)
					{
						List<string> newList = new List<string>();
						int newIndex = 0;
						if (AQComp.fishData.Count > 0)
						{
							foreach (string value in AQComp.fishData)
							{
								string prevDefVal = CompAquarium.StringValuePart(value, 1);
								int prevHealth = CompAquarium.NumValuePart(value, 2);
								int prevAge = CompAquarium.NumValuePart(value, 3);
								int prevAct = CompAquarium.NumValuePart(value, 4);
								newIndex++;
								bool Added = false;
								if (prevAct == 1)
								{
									if (prevDefVal == fishThing.def.defName && !Added)
									{
										int health = 100;
										int age = 0;
										CompAQFishInBag CBag = fishThing.TryGetComp<CompAQFishInBag>();
										if (CBag != null)
										{
											health = CBag.fishhealth;
											age = CBag.age;
										}
										string newValue = CompAquarium.CreateValuePart(newIndex, fishThing.def.defName, health, age, 0);
										newList.Add(newValue);
										AQComp.numFish++;
									}
									else
									{
										string newValue2 = CompAquarium.CreateValuePart(newIndex, prevDefVal, prevHealth, prevAge, prevAct);
										newList.Add(newValue2);
									}
								}
								else
								{
									string newValue3 = CompAquarium.CreateValuePart(newIndex, prevDefVal, prevHealth, prevAge, prevAct);
									newList.Add(newValue3);
								}
							}
						}
						AQComp.fishData = newList;
					}
				}
				fishThing.Destroy(DestroyMode.Vanish);
				toil.actor.skills.Learn(SkillDefOf.Animals, 75f, false);
			};
			toil.defaultCompleteMode = ToilCompleteMode.Instant;
			return toil;
		}
	}
}
