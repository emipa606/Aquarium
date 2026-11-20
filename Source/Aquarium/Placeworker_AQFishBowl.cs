using System.Linq;
using Verse;

namespace Aquarium;

public class Placeworker_AQFishBowl : PlaceWorker
{
    public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map,
        Thing thingToIgnore = null, Thing thing = null)
    {
        if (!loc.InBounds(map))
        {
            return false;
        }

        if (loc.Filled(map))
        {
            return false;
        }

        var list = map.thingGrid.ThingsListAt(loc);
        foreach (var thingy in list.Select(thingDef => thingDef.def))
        {
            if (thingy.IsBlueprint || thingy.IsFrame)
            {
                return false;
            }

            switch (thingy.category)
            {
                case ThingCategory.Plant:
                case ThingCategory.Building when thingy.surfaceType != SurfaceType.Item &&
                                                 thingy.surfaceType != SurfaceType.Eat &&
                                                 thingy.defName != "ShipHullTile":
                case ThingCategory.Item:
                    return false;
            }
        }

        return true;
    }
}