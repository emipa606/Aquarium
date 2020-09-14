using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Aquarium
{
	// Token: 0x02000010 RID: 16
	public class JoyGiver_AQViewFishTank : JoyGiver
	{
		// Token: 0x0600005E RID: 94 RVA: 0x00004464 File Offset: 0x00002664
		public override Job TryGiveJob(Pawn pawn)
		{
			bool allowedOutside = JoyUtility.EnjoyableOutsideNow(pawn, null);
			var tankDefs = (from tankDef in DefDatabase<ThingDef>.AllDefsListForReading where tankDef.defName.StartsWith("AQFishTank") select tankDef.defName).ToList();
            foreach (var tankDef in tankDefs)
            {
				try
				{
					ThingDef AQBowlDef = ThingDef.Named(tankDef);
                    candidates.AddRange(pawn.Map.listerThings.ThingsOfDef(AQBowlDef).Where(delegate(Thing thing)
					{
						if (!AQUtility.HasFish(thing))
						{
							return false;
						}
						if (thing.Faction != Faction.OfPlayer || thing.IsForbidden(pawn) || (!allowedOutside && !thing.Position.Roofed(thing.Map)) || !pawn.CanReserveAndReach(thing, PathEndMode.Touch, Danger.None, 1, -1, null, false) || !thing.IsPoliticallyProper(pawn))
						{
							return false;
						}
						Room room = thing.GetRoom(RegionType.Set_Passable);
						return room != null && AQUtility.IsValidAquaRoom(pawn, room);
					}));
				} catch
                {

                }
			}
            Job result2;
            if (!candidates.TryRandomElementByWeight((Thing target) => Mathf.Max(target.GetStatValue(StatDefOf.Beauty, true), 0.5f), out Thing result))
			{
				result2 = null;
			}
			else
			{
				result2 = JobMaker.MakeJob(def.jobDef, result);
			}
            candidates.Clear();
			return result2;
		}

		// Token: 0x04000023 RID: 35
		private static readonly List<Thing> candidates = new List<Thing>();
	}
}
