using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace Aquarium;

[HarmonyPatch(typeof(FishingUtility), nameof(FishingUtility.GetCatchesFor))]
public static class FishingUtility_GetCatchesFor
{
    public static void Postfix(ref List<Thing> __result, ref bool rare)
    {
        if (!Rand.Chance(0.05f))
        {
            return;
        }

        __result.Add(ThingMaker.MakeThing(DefsCacher.AQBagDefs.RandomElement()));
        rare = true;
    }
}