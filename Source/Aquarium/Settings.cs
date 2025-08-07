using UnityEngine;
using Verse;

namespace Aquarium;

public class Settings : ModSettings
{
    public bool AllowBreed = true;

    public bool AllowInspire = true;

    public bool AllowTankSounds = true;

    public float BaseInspChance = 5f;

    public float BreedChance = 50f;

    public float DegradeFoodFactor = 100f;

    public float DegradeWaterFactor = 100f;

    public bool DoDeathMsgs = true;

    public float FishLife = 1f;

    public bool FishMovesAround = true;

    public bool ImmortalFish;

    public float RespondClean = 50f;

    public float RespondFood = 50f;

    public void DoWindowContents(Rect canvas)
    {
        const float gap = 6f;
        var listingStandard = new Listing_Standard
        {
            ColumnWidth = canvas.width
        };
        listingStandard.Begin(canvas);
        listingStandard.Gap(gap);
        listingStandard.CheckboxLabeled("Aquarium.DoDeathMsgs".Translate(), ref DoDeathMsgs);
        listingStandard.Gap(gap);
        listingStandard.CheckboxLabeled("Aquarium.AllowTankSounds".Translate(), ref AllowTankSounds);
        listingStandard.Gap(gap);
        listingStandard.CheckboxLabeled("Aquarium.FishMovesAround".Translate(), ref FishMovesAround,
            "Aquarium.FishMovesAroundTooltip".Translate());
        listingStandard.Gap(gap);
        listingStandard.CheckboxLabeled("Aquarium.AllowInspire".Translate(), ref AllowInspire);
        listingStandard.Gap(gap);
        checked
        {
            listingStandard.Label("Aquarium.BaseInspChance".Translate() + "  " + (int)BaseInspChance);
            BaseInspChance = (int)listingStandard.Slider((int)BaseInspChance, 1f, 10f);
            listingStandard.Gap(gap);
            listingStandard.CheckboxLabeled("Aquarium.ImmortalFish".Translate(), ref ImmortalFish);
            if (!ImmortalFish)
            {
                listingStandard.Gap(gap);
                listingStandard.Label("Aquarium.FishLife".Translate() + "  " + (int)FishLife);
                FishLife = (int)listingStandard.Slider((int)FishLife, 1f, 3f);
            }

            listingStandard.Gap(gap);
            listingStandard.Label("Aquarium.DegradeWaterFactor".Translate() + "  " + (int)DegradeWaterFactor);
            DegradeWaterFactor = (int)listingStandard.Slider((int)DegradeWaterFactor, 0, 200f);
            listingStandard.Gap(gap);
            listingStandard.Label("Aquarium.DegradeFoodFactor".Translate() + "  " + (int)DegradeFoodFactor);
            DegradeFoodFactor = (int)listingStandard.Slider((int)DegradeFoodFactor, 0, 200f);
            listingStandard.Gap(gap);
            listingStandard.Label("Aquarium.RespondClean".Translate() + "  " + (int)RespondClean);
            RespondClean = (int)listingStandard.Slider((int)RespondClean, 25f, 75f);
            listingStandard.Gap(gap);
            listingStandard.Label("Aquarium.RespondFood".Translate() + "  " + (int)RespondFood);
            RespondFood = (int)listingStandard.Slider((int)RespondFood, 25f, 75f);

            listingStandard.Gap(gap);
            listingStandard.CheckboxLabeled("Aquarium.AllowBreed".Translate(), ref AllowBreed);
            listingStandard.Gap(gap);
            listingStandard.Label("Aquarium.BreedChance".Translate() + "  " + (int)BreedChance);
            BreedChance = (int)listingStandard.Slider((int)BreedChance, 25f, 75f);
            listingStandard.Gap(gap);
            if (Controller.CurrentVersion != null)
            {
                listingStandard.Gap();
                GUI.contentColor = Color.gray;
                listingStandard.Label("Aquarium.CurrentModVersion".Translate(Controller.CurrentVersion));
                GUI.contentColor = Color.white;
            }

            listingStandard.Gap(gap);
            listingStandard.End();
        }
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref DoDeathMsgs, "DoDeathMsgs", true);
        Scribe_Values.Look(ref AllowTankSounds, "AllowTankSounds", true);
        Scribe_Values.Look(ref FishMovesAround, "FishMovesAround", true);
        Scribe_Values.Look(ref AllowInspire, "AllowInspire", true);
        Scribe_Values.Look(ref BaseInspChance, "BaseinspChance", 5f);
        Scribe_Values.Look(ref FishLife, "FishLife", 1f);
        Scribe_Values.Look(ref DegradeWaterFactor, "DegradeWaterFactor", 100f);
        Scribe_Values.Look(ref DegradeFoodFactor, "DegradeFoodFactor", 100f);
        Scribe_Values.Look(ref RespondClean, "RespondClean", 50f);
        Scribe_Values.Look(ref RespondFood, "RespondFood", 50f);
        Scribe_Values.Look(ref AllowBreed, "AllowBreed", true);
        Scribe_Values.Look(ref ImmortalFish, "ImmortalFish");
        Scribe_Values.Look(ref BreedChance, "BreedChance", 50f);
    }
}