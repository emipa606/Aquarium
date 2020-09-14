using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Aquarium
{
    [StaticConstructorOnStartup]
    internal static class DefsCacher
    {
        public static List<ThingDef> AQFishTankDefs;

        static DefsCacher()
        {
            AQFishTankDefs = (from tankDef in DefDatabase<ThingDef>.AllDefsListForReading where tankDef.defName.StartsWith("AQFishTank") select tankDef).ToList();
        }
    }
}
