using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Aquarium
{
	// Token: 0x0200000F RID: 15
	public class JoyGiver_AQViewFishBowl : JoyGiver
	{
		// Token: 0x0600005B RID: 91 RVA: 0x00004380 File Offset: 0x00002580
		public override Job TryGiveJob(Pawn pawn)
		{
			bool allowedOutside = JoyUtility.EnjoyableOutsideNow(pawn, null);
			Job result2;
			try
			{
				ThingDef AQBowlDef = ThingDef.Named("AQFishBowl");
				JoyGiver_AQViewFishBowl.candidates.AddRange(pawn.Map.listerThings.ThingsOfDef(AQBowlDef).Where(delegate(Thing thing)
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
                if (!JoyGiver_AQViewFishBowl.candidates.TryRandomElementByWeight((Thing target) => Mathf.Max(target.GetStatValue(StatDefOf.Beauty, true), 0.5f), out Thing result))
                {
                    result2 = null;
                }
                else
                {
                    result2 = JobMaker.MakeJob(this.def.jobDef, result);
                }
            }
			finally
			{
				JoyGiver_AQViewFishBowl.candidates.Clear();
			}
			return result2;
		}

		// Token: 0x04000022 RID: 34
		private static readonly List<Thing> candidates = new List<Thing>();
	}
}
