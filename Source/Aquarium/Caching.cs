using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Aquarium
{
    [StaticConstructorOnStartup]
    internal static class DefsCacher
    {
        public static List<ThingDef> AQFishTankDefs;

        public static List<ThingDef> AQSandDefs;

        public static List<ThingDef> AQDecorationDefs;

        static DefsCacher()
        {
            AQFishTankDefs = (from tankDef in DefDatabase<ThingDef>.AllDefsListForReading
                where tankDef.defName.StartsWith("AQFishTank")
                select tankDef).ToList();
            AQSandDefs = (from sandDef in DefDatabase<ThingDef>.AllDefsListForReading
                where sandDef.defName.StartsWith("AQSand")
                orderby sandDef.defName
                select sandDef).ToList();
            AQDecorationDefs = (from decorationDef in DefDatabase<ThingDef>.AllDefsListForReading
                where decorationDef.defName.StartsWith("AQDecoration")
                orderby decorationDef.defName
                select decorationDef).ToList();
        }
    }
}