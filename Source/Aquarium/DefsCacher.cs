using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Aquarium;

[StaticConstructorOnStartup]
internal static class DefsCacher
{
    public static readonly List<ThingDef> AQFishTankDefs;

    public static readonly List<ThingDef> AQSandDefs;

    public static readonly List<ThingDef> AQDecorationDefs;

    public static readonly List<ThingDef> AQBagDefs;

    public static readonly SoundDef AQSoundDef;

    public static readonly ThingDef AQRandomFishDef;

    public static readonly ThingDef AQFishBowlDef;

    public static readonly ThingDef AQFishFoodDef;
    public static readonly ThingDef AQFishMeatDef;

    public static readonly JobDef AQCleaningDef;

    public static readonly JobDef AQFeedingDef;
    public static readonly JobDef AQManagingAddDef;
    public static readonly JobDef AQManagingRemoveDef;

    static DefsCacher()
    {
        AQFishTankDefs = (from tankDef in DefDatabase<ThingDef>.AllDefsListForReading
            where tankDef.defName.StartsWith("AQFishTank")
            select tankDef).ToList();
        AQFishBowlDef = ThingDef.Named("AQFishBowl");
        AQSandDefs = (from sandDef in DefDatabase<ThingDef>.AllDefsListForReading
            where sandDef.defName.StartsWith("AQSand")
            orderby sandDef.defName
            select sandDef).ToList();
        AQDecorationDefs = (from decorationDef in DefDatabase<ThingDef>.AllDefsListForReading
            where decorationDef.defName.StartsWith("AQDecoration")
            orderby decorationDef.defName
            select decorationDef).ToList();
        AQBagDefs = (from bagDef in DefDatabase<ThingDef>.AllDefsListForReading
            where bagDef.defName.StartsWith("AQFishInBag")
            orderby bagDef.label
            select bagDef).ToList();

        AQSoundDef = SoundDef.Named("AQFishTank");
        AQFishFoodDef = ThingDef.Named("AQFishFood");
        AQFishMeatDef = ThingDef.Named("AQFishMeat");
        AQRandomFishDef = DefDatabase<ThingDef>.GetNamed("AQRandomFish");
        AQCleaningDef = DefDatabase<JobDef>.GetNamed("AQCleaning", false);
        AQFeedingDef = DefDatabase<JobDef>.GetNamed("AQFeeding", false);
        AQManagingAddDef = DefDatabase<JobDef>.GetNamed("AQManagingAdd", false);
        AQManagingRemoveDef = DefDatabase<JobDef>.GetNamed("AQManagingRemove", false);
    }
}