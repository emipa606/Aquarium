using System;
using UnityEngine;
using Verse;

namespace Aquarium
{
	// Token: 0x02000012 RID: 18
	public class Settings : ModSettings
	{
		// Token: 0x06000063 RID: 99 RVA: 0x00004630 File Offset: 0x00002830
		public void DoWindowContents(Rect canvas)
		{
			float gap = 8f;
			Listing_Standard listing_Standard = new Listing_Standard();
			listing_Standard.ColumnWidth = canvas.width;
			listing_Standard.Begin(canvas);
			listing_Standard.Gap(gap);
			listing_Standard.CheckboxLabeled("Aquarium.DoDeathMsgs".Translate(), ref this.DoDeathMsgs, null);
			listing_Standard.Gap(gap);
			listing_Standard.CheckboxLabeled("Aquarium.AllowTankSounds".Translate(), ref this.AllowTankSounds, null);
			listing_Standard.Gap(gap);
			listing_Standard.CheckboxLabeled("Aquarium.AllowInspire".Translate(), ref this.AllowInspire, null);
			listing_Standard.Gap(gap);
			checked
			{
				listing_Standard.Label("Aquarium.BaseInspChance".Translate() + "  " + (int)this.BaseInspChance, -1f, null);
				this.BaseInspChance = (float)((int)listing_Standard.Slider((float)((int)this.BaseInspChance), 1f, 10f));
				listing_Standard.Gap(gap);
				listing_Standard.Label("Aquarium.FishLife".Translate() + "  " + (int)this.FishLife, -1f, null);
				this.FishLife = (float)((int)listing_Standard.Slider((float)((int)this.FishLife), 1f, 3f));
				listing_Standard.Gap(gap);
				listing_Standard.Label("Aquarium.DegradeWaterFactor".Translate() + "  " + (int)this.DegradeWaterFactor, -1f, null);
				this.DegradeWaterFactor = (float)((int)listing_Standard.Slider((float)((int)this.DegradeWaterFactor), 50f, 200f));
				listing_Standard.Gap(gap);
				listing_Standard.Label("Aquarium.DegradeFoodFactor".Translate() + "  " + (int)this.DegradeFoodFactor, -1f, null);
				this.DegradeFoodFactor = (float)((int)listing_Standard.Slider((float)((int)this.DegradeFoodFactor), 50f, 200f));
				listing_Standard.Gap(gap);
				listing_Standard.Label("Aquarium.RespondClean".Translate() + "  " + (int)this.RespondClean, -1f, null);
				this.RespondClean = (float)((int)listing_Standard.Slider((float)((int)this.RespondClean), 25f, 75f));
				listing_Standard.Gap(gap);
				listing_Standard.Label("Aquarium.RespondFood".Translate() + "  " + (int)this.RespondFood, -1f, null);
				this.RespondFood = (float)((int)listing_Standard.Slider((float)((int)this.RespondFood), 25f, 75f));
				listing_Standard.Gap(gap);
				listing_Standard.CheckboxLabeled("Aquarium.AllowBreed".Translate(), ref this.AllowBreed, null);
				listing_Standard.Gap(gap);
				listing_Standard.Label("Aquarium.BreedChance".Translate() + "  " + (int)this.BreedChance, -1f, null);
				this.BreedChance = (float)((int)listing_Standard.Slider((float)((int)this.BreedChance), 25f, 75f));
				listing_Standard.Gap(gap);
				listing_Standard.Gap(gap);
				listing_Standard.End();
			}
		}

		// Token: 0x06000064 RID: 100 RVA: 0x00004990 File Offset: 0x00002B90
		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look<bool>(ref this.DoDeathMsgs, "DoDeathMsgs", true, false);
			Scribe_Values.Look<bool>(ref this.AllowTankSounds, "AllowTankSounds", true, false);
			Scribe_Values.Look<bool>(ref this.AllowInspire, "AllowInspire", true, false);
			Scribe_Values.Look<float>(ref this.BaseInspChance, "BaseinspChance", 5f, false);
			Scribe_Values.Look<float>(ref this.FishLife, "FishLife", 1f, false);
			Scribe_Values.Look<float>(ref this.DegradeWaterFactor, "DegradeWaterFactor", 100f, false);
			Scribe_Values.Look<float>(ref this.DegradeFoodFactor, "DegradeFoodFactor", 100f, false);
			Scribe_Values.Look<float>(ref this.RespondClean, "RespondClean", 50f, false);
			Scribe_Values.Look<float>(ref this.RespondFood, "RespondFood", 50f, false);
			Scribe_Values.Look<bool>(ref this.AllowBreed, "AllowBreed", true, false);
			Scribe_Values.Look<float>(ref this.BreedChance, "BreedChance", 50f, false);
		}

		// Token: 0x04000024 RID: 36
		public bool DoDeathMsgs = true;

		// Token: 0x04000025 RID: 37
		public bool AllowTankSounds = true;

		// Token: 0x04000026 RID: 38
		public bool AllowInspire = true;

		// Token: 0x04000027 RID: 39
		public float BaseInspChance = 5f;

		// Token: 0x04000028 RID: 40
		public float FishLife = 1f;

		// Token: 0x04000029 RID: 41
		public float DegradeWaterFactor = 100f;

		// Token: 0x0400002A RID: 42
		public float DegradeFoodFactor = 100f;

		// Token: 0x0400002B RID: 43
		public float RespondClean = 50f;

		// Token: 0x0400002C RID: 44
		public float RespondFood = 50f;

		// Token: 0x0400002D RID: 45
		public bool AllowBreed = true;

		// Token: 0x0400002E RID: 46
		public float BreedChance = 50f;
	}
}
