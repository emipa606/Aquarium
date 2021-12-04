using Verse;

namespace Aquarium;

public class Placeworker_AQFishBowl : PlaceWorker
{
    public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map,
        Thing thingToIgnore = null, Thing thing = null)
    {
        if (!loc.InBounds(map))
        {
#if DEBUG
                Log.Message("Not in bounds of map");
#endif
            return false;
        }

        if (loc.Filled(map))
        {
#if DEBUG
                Log.Message("Location is filled already");
#endif
            return false;
        }

        var list = map.thingGrid.ThingsListAt(loc);
        foreach (var thingy in list)
        {
            if (thingy.def.IsBlueprint || thingy.def.IsFrame)
            {
#if DEBUG
                    Log.Message("Blueprint/frame is in the way");
#endif
                return false;
            }

            if (thingy.def.category == ThingCategory.Plant)
            {
#if DEBUG
                    Log.Message("Plant is in the way");
#endif
                return false;
            }

            if (thingy.def.category == ThingCategory.Building && thingy.def.surfaceType != SurfaceType.Item &&
                thingy.def.surfaceType != SurfaceType.Eat && thingy.def.defName != "ShipHullTile")
            {
#if DEBUG
                    Log.Message("Its a building but not a table or SOS2 floor");
#endif
                return false;
            }

            if (thingy.def.category == ThingCategory.Item)
            {
#if DEBUG
                    Log.Message("Its an item in the way");
#endif
                return false;
            }
        }

        return true;
    }
}