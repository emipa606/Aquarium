using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Aquarium
{
	// Token: 0x02000003 RID: 3
	public class AQUtility
	{
		// Token: 0x06000005 RID: 5 RVA: 0x00002128 File Offset: 0x00000328
		internal static float GetJoyGainFactor(float beautyFactor, Thing FishyThing)
		{
			float fishFactor = 1f;
			float agefactor = 1f;
			float compare = 30f;
			CompAquarium CompAQ = FishyThing.TryGetComp<CompAquarium>();
			if (CompAQ != null)
			{
				List<string> list = CompAQ.fishData;
				if (list.Count > 0)
				{
					foreach (string listing in list)
					{
						ThingDef fishdef = ThingDef.Named(CompAquarium.stringValuePart(listing, 1));
						if (fishdef != null)
						{
							float value = Math.Max(10f, Math.Min(50f, fishdef.BaseMarketValue));
							fishFactor *= value / compare;
							agefactor *= Mathf.Lerp(0.75f, 1f, (float)(Math.Min(CompAquarium.oldFishAge, CompAquarium.numValuePart(listing, 3)) / CompAquarium.oldFishAge));
						}
					}
				}
			}
			return beautyFactor * fishFactor * agefactor;
		}

		// Token: 0x06000006 RID: 6 RVA: 0x00002210 File Offset: 0x00000410
		internal static void ApplyMoodBoostAndInspire(Pawn pawn, Thing FishyThing)
		{
			CompAquarium CompAQ = FishyThing.TryGetComp<CompAquarium>();
			Pawn_NeedsTracker needs = pawn.needs;
			if (((needs != null) ? needs.mood : null) != null)
			{
				float fishFactor = 1f;
				float agefactor = 1f;
				float compare = 30f;
				if (CompAQ != null && CompAQ.numFish > 0 && pawn.IsHashIntervalTick(1000))
				{
					List<string> list = CompAQ.fishData;
					if (list.Count > 0)
					{
						foreach (string listing in list)
						{
							ThingDef fishdef = ThingDef.Named(CompAquarium.stringValuePart(listing, 1));
							if (fishdef != null)
							{
								float value = Math.Max(10f, Math.Min(50f, fishdef.BaseMarketValue));
								fishFactor *= value / compare;
								agefactor *= Mathf.Lerp(0.75f, 1f, (float)(Math.Min(CompAquarium.oldFishAge, CompAquarium.numValuePart(listing, 3)) / CompAquarium.oldFishAge));
							}
						}
					}
					if (AQUtility.IsInspired((int)((float)(CompAQ.numFish * 25) * fishFactor * agefactor)))
					{
						ThoughtDef fishRelaxDef = ThoughtDef.Named("AQObserveFish");
						pawn.needs.mood.thoughts.memories.TryGainMemory(fishRelaxDef, null);
					}
				}
			}
			if (CompAQ != null && CompAQ.numFish > 0 && Controller.Settings.AllowInspire && pawn.IsHashIntervalTick(1000) && !pawn.IsPrisoner && AQUtility.IsInspired((int)((float)CompAQ.numFish * Controller.Settings.BaseInspChance)) && !pawn.mindState.inspirationHandler.Inspired)
			{
				InspirationDef IDef = (from x in DefDatabase<InspirationDef>.AllDefsListForReading
				where x.Worker.InspirationCanOccur(pawn)
				select x).RandomElementByWeightWithFallback((InspirationDef x) => x.Worker.CommonalityFor(pawn), null);
				pawn.mindState.inspirationHandler.TryStartInspiration_NewTemp(IDef, null);
			}
		}

		// Token: 0x06000007 RID: 7 RVA: 0x00002438 File Offset: 0x00000638
		internal static bool IsInspired(int chance)
		{
			return CompAquarium.RandomFloat(1f, 100f) < (float)chance;
		}

		// Token: 0x06000008 RID: 8 RVA: 0x00002450 File Offset: 0x00000650
		internal static int GetFoodNumToFullyFeed(CompAquarium AQComp)
		{
			return Math.Max(0, (int)((1f - AQComp.foodPct) * 10f * (float)Math.Max(0, AQComp.numFish)));
		}

		// Token: 0x06000009 RID: 9 RVA: 0x00002479 File Offset: 0x00000679
		internal static int GetCleanTime(CompAquarium AQComp)
		{
			return Math.Max(120, (int)((1f - AQComp.cleanPct) * 10f * 2f * 60f));
		}

		// Token: 0x0600000A RID: 10 RVA: 0x000024A4 File Offset: 0x000006A4
		internal static bool AddOrRemove(Thing t, out bool Add, out ThingDef fishAddDef, out bool Remove)
		{
			bool result = false;
			bool flag = Remove = false;
			Add = flag;
			fishAddDef = null;
			CompAquarium CA = t.TryGetComp<CompAquarium>();
			if (CA != null)
			{
				List<string> listing = CA.fishData;
				if (listing.Count > 0)
				{
					foreach (string value in listing)
					{
						int action = CompAquarium.numValuePart(value, 4);
						if (action == 1)
						{
							Add = true;
							fishAddDef = ThingDef.Named(CompAquarium.stringValuePart(value, 1));
							return true;
						}
						if (action == 2)
						{
							Remove = true;
							return true;
						}
					}
					return result;
				}
			}
			return result;
		}

		// Token: 0x0600000B RID: 11 RVA: 0x00002548 File Offset: 0x00000748
		internal static Thing GetClosestFishInBag(Pawn p, ThingDef def, Thing t)
		{
			Thing result = null;
			List<Thing> potentials = p.Map.listerThings.ThingsOfDef(ThingDef.Named(def.defName));
			if (potentials.Count > 0)
			{
				Thing bestThing = null;
				float bestScore = 0f;
				foreach (Thing potential in potentials)
				{
					if (potential.Spawned)
					{
						CompAQFishInBag CBag = potential.TryGetComp<CompAQFishInBag>();
						if (CBag != null && p.CanReserveAndReach(potential, PathEndMode.Touch, Danger.None, 1, -1, null, false))
						{
							int ticksLeft = CBag.ticksInBagRemain;
							float distance = p.Position.DistanceTo(potential.Position);
							float score = 1f / (float)ticksLeft * Mathf.Lerp(1f, 0.01f, distance / 9999f);
							if (score > bestScore)
							{
								bestScore = score;
								bestThing = potential;
							}
						}
					}
				}
				if (bestThing != null)
				{
					return bestThing;
				}
			}
			return result;
		}

		// Token: 0x0600000C RID: 12 RVA: 0x00002644 File Offset: 0x00000844
		internal static Thing GetClosestFeed(Pawn p, Thing t)
		{
			CompAquarium CA = t.TryGetComp<CompAquarium>();
			int num = 0;
			if (CA != null)
			{
				if (CA.numFish < 1)
				{
					return null;
				}
				num = Math.Max(1, (int)((1f - CA.foodPct) * 10f * (float)Math.Max(1, CA.numFish)));
			}
			if (num > 0)
			{
				List<Thing> potentials = p.Map.listerThings.ThingsOfDef(ThingDef.Named("AQFishFood"));
				Thing BestThing = null;
				float BestScore = 0f;
				if (potentials.Count > 0)
				{
					foreach (Thing potential in potentials)
					{
						if (potential.Spawned && p.CanReserveAndReach(potential, PathEndMode.Touch, Danger.None, 1, -1, null, false))
						{
							float stackFactor = 1f;
							if (potential.stackCount < num)
							{
								stackFactor = 0.5f;
							}
							float distance = p.Position.DistanceTo(potential.Position);
							float score = Mathf.Lerp(1f, 0.01f, distance / 9999f) * stackFactor;
							if (score > BestScore)
							{
								BestScore = score;
								BestThing = potential;
							}
						}
					}
				}
				return BestThing;
			}
			return null;
		}

		// Token: 0x0600000D RID: 13 RVA: 0x0000277C File Offset: 0x0000097C
		internal static bool HasFish(Thing thing)
		{
			CompAquarium CAQ = thing.TryGetComp<CompAquarium>();
			return CAQ != null && CAQ.numFish > 0;
		}

		// Token: 0x0600000E RID: 14 RVA: 0x000027A0 File Offset: 0x000009A0
		internal static bool IsValidAquaRoom(Pawn pawn, Room room)
		{
			return (room.Role != RoomRoleDefOf.Bedroom && room.Role != RoomRoleDefOf.PrisonCell && room.Role != RoomRoleDefOf.PrisonBarracks) || (pawn.ownership != null && pawn.ownership.OwnedRoom != null && pawn.ownership.OwnedRoom == room) || (pawn.IsPrisoner && (room.Role == RoomRoleDefOf.PrisonCell || room.Role == RoomRoleDefOf.PrisonBarracks) && room == pawn.Position.GetRoom(pawn.Map, RegionType.Set_Passable));
		}

		// Token: 0x0600000F RID: 15 RVA: 0x00002834 File Offset: 0x00000A34
		internal static void DoSpawnTropicalFishMeat(Thing parent, int age)
		{
			if (parent.Spawned && ((parent != null) ? parent.Map : null) != null)
			{
				int stack = Math.Max(1, (int)Mathf.Lerp(1f, 10f, (float)age / (float)CompAquarium.oldFishAge));
				ThingPlaceMode TPMode = ThingPlaceMode.Near;
				Thing thing = ThingMaker.MakeThing(ThingDef.Named("AQFishMeat"), null);
				thing.stackCount = Math.Min(thing.def.stackLimit, stack);
				GenPlace.TryPlaceThing(thing, parent.Position, parent.Map, TPMode, null, null, default(Rot4));
			}
		}

		// Token: 0x06000010 RID: 16 RVA: 0x000028C0 File Offset: 0x00000AC0
		internal static void DebugFishData(CompAquarium CompAQ, int maxNum = 0)
		{
			Log.Message(string.Concat(new string[]
			{
				CompAQ.parent.Label,
				" : ",
				CompAQ.numFish.ToString(),
				" : ",
				maxNum.ToString()
			}), false);
			List<string> list = CompAQ.fishData;
			if (list.Count > 0)
			{
				foreach (string text in list)
				{
					Log.Message(text, false);
				}
			}
		}

		// Token: 0x04000002 RID: 2
		internal const float AvgFishValue = 30f;

		// Token: 0x04000003 RID: 3
		internal const int FoodVal = 10;
	}
}
