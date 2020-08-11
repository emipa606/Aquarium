using System;
using System.Collections.Generic;
using Verse;

namespace Aquarium
{
	// Token: 0x02000011 RID: 17
	public class Placeworker_AQFishBowl : PlaceWorker
	{
		// Token: 0x06000061 RID: 97 RVA: 0x00004548 File Offset: 0x00002748
		public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null, Thing thing = null)
		{
			if (!loc.InBounds(map))
			{
				return false;
			}
			if (loc.Filled(map))
			{
				return false;
			}
			List<Thing> list = map.thingGrid.ThingsListAt(loc);
			for (int i = 0; i < list.Count; i++)
			{
				Thing thingy = list[i];
				if (thingy.def.IsBlueprint || thingy.def.IsFrame)
				{
					return false;
				}
				if (thingy.def.category == ThingCategory.Plant)
				{
					return false;
				}
				if (thingy.def.category == ThingCategory.Building && thingy.def.surfaceType != SurfaceType.Item && thingy.def.surfaceType != SurfaceType.Eat)
				{
					return false;
				}
				if (thingy.def.category == ThingCategory.Item)
				{
					return false;
				}
			}
			return true;
		}
	}
}
