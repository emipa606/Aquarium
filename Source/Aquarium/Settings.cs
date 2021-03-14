﻿using UnityEngine;
using Verse;

namespace Aquarium
{
    // Token: 0x02000012 RID: 18
    public class Settings : ModSettings
    {
        // Token: 0x0400002D RID: 45
        public bool AllowBreed = true;

        // Token: 0x04000026 RID: 38
        public bool AllowInspire = true;

        // Token: 0x04000025 RID: 37
        public bool AllowTankSounds = true;

        // Token: 0x04000027 RID: 39
        public float BaseInspChance = 5f;

        // Token: 0x0400002E RID: 46
        public float BreedChance = 50f;

        // Token: 0x0400002A RID: 42
        public float DegradeFoodFactor = 100f;

        // Token: 0x04000029 RID: 41
        public float DegradeWaterFactor = 100f;

        // Token: 0x04000024 RID: 36
        public bool DoDeathMsgs = true;

        // Token: 0x04000028 RID: 40
        public float FishLife = 1f;

        public bool FishMovesAround = true;

        // Token: 0x0400002B RID: 43
        public float RespondClean = 50f;

        // Token: 0x0400002C RID: 44
        public float RespondFood = 50f;

        // Token: 0x06000063 RID: 99 RVA: 0x00004630 File Offset: 0x00002830
        public void DoWindowContents(Rect canvas)
        {
            var gap = 8f;
            var listing_Standard = new Listing_Standard
            {
                ColumnWidth = canvas.width
            };
            listing_Standard.Begin(canvas);
            listing_Standard.Gap(gap);
            listing_Standard.CheckboxLabeled("Aquarium.DoDeathMsgs".Translate(), ref DoDeathMsgs);
            listing_Standard.Gap(gap);
            listing_Standard.CheckboxLabeled("Aquarium.AllowTankSounds".Translate(), ref AllowTankSounds);
            listing_Standard.Gap(gap);
            listing_Standard.CheckboxLabeled("Aquarium.FishMovesAround".Translate(), ref FishMovesAround,
                "Aquarium.FishMovesAroundTooltip".Translate());
            listing_Standard.Gap(gap);
            listing_Standard.CheckboxLabeled("Aquarium.AllowInspire".Translate(), ref AllowInspire);
            listing_Standard.Gap(gap);
            checked
            {
                listing_Standard.Label("Aquarium.BaseInspChance".Translate() + "  " + (int) BaseInspChance);
                BaseInspChance = (int) listing_Standard.Slider((int) BaseInspChance, 1f, 10f);
                listing_Standard.Gap(gap);
                listing_Standard.Label("Aquarium.FishLife".Translate() + "  " + (int) FishLife);
                FishLife = (int) listing_Standard.Slider((int) FishLife, 1f, 3f);
                listing_Standard.Gap(gap);
                listing_Standard.Label("Aquarium.DegradeWaterFactor".Translate() + "  " + (int) DegradeWaterFactor);
                DegradeWaterFactor = (int) listing_Standard.Slider((int) DegradeWaterFactor, 50f, 200f);
                listing_Standard.Gap(gap);
                listing_Standard.Label("Aquarium.DegradeFoodFactor".Translate() + "  " + (int) DegradeFoodFactor);
                DegradeFoodFactor = (int) listing_Standard.Slider((int) DegradeFoodFactor, 50f, 200f);
                listing_Standard.Gap(gap);
                listing_Standard.Label("Aquarium.RespondClean".Translate() + "  " + (int) RespondClean);
                RespondClean = (int) listing_Standard.Slider((int) RespondClean, 25f, 75f);
                listing_Standard.Gap(gap);
                listing_Standard.Label("Aquarium.RespondFood".Translate() + "  " + (int) RespondFood);
                RespondFood = (int) listing_Standard.Slider((int) RespondFood, 25f, 75f);
                listing_Standard.Gap(gap);
                listing_Standard.CheckboxLabeled("Aquarium.AllowBreed".Translate(), ref AllowBreed);
                listing_Standard.Gap(gap);
                listing_Standard.Label("Aquarium.BreedChance".Translate() + "  " + (int) BreedChance);
                BreedChance = (int) listing_Standard.Slider((int) BreedChance, 25f, 75f);
                listing_Standard.Gap(gap);
                listing_Standard.Gap(gap);
                listing_Standard.End();
            }
        }

        // Token: 0x06000064 RID: 100 RVA: 0x00004990 File Offset: 0x00002B90
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
            Scribe_Values.Look(ref BreedChance, "BreedChance", 50f);
        }
    }
}