using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Aquarium
{
    // Token: 0x02000006 RID: 6
    public class CompAquarium : ThingComp
    {
        // Token: 0x17000002 RID: 2
        // (get) Token: 0x06000019 RID: 25 RVA: 0x00002BE2 File Offset: 0x00000DE2
        private CompProperties_CompAquarium Props
        {
            get
            {
                return (CompProperties_CompAquarium)props;
            }
        }

        // Token: 0x0600001A RID: 26 RVA: 0x00002BF0 File Offset: 0x00000DF0
        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref numFish, "numFish", 0, false);
            Scribe_Values.Look(ref selectedSandDefName, "selectedSandDefName", string.Empty, false);
            Scribe_Values.Look(ref selectedDecorationDefName, "selectedDecorationDefName", string.Empty, false);
            Scribe_Values.Look(ref cleanPct, "cleanPct", 1f, false);
            Scribe_Values.Look(ref foodPct, "foodPct", 0f, false);
            Scribe_Collections.Look(ref fishData, "fishData", LookMode.Value, Array.Empty<object>());
        }

        public override void PostDestroy(DestroyMode mode, Map previousMap)
        {
            if (savedBeauty != null)
            {
                foreach (var beautyItem in savedBeauty)
                {
                    beautyItem.Destroy();
                }
            }
            base.PostDestroy(mode, previousMap);
        }

        private Vector3 GetDrawPos(int currentFishNumber, Vector3 baseVector, out float perspective)
        {
            bool startInMiddle = true;
            perspective = 1f;
            if ((Math.Ceiling((decimal)(Props.maxFish / 2)) % 2) == 0)
            {
                startInMiddle = false;
            }
            var xModifier = 0f;
            var zModifier = 0f;
            for (int i = 1; i <= currentFishNumber; i++)
            {
                var even = (i % 2 == 0);
                var half = (float)Math.Ceiling((double)i / 2);
                var subEven = (half % 2) == 0;
                var halfHalf = (float)Math.Ceiling((double)i / 2 / 2);
                if (startInMiddle)
                {
                    halfHalf = (float)Math.Ceiling(((double)i - 2) / 2 / 2);
                    if (halfHalf < 0)
                        halfHalf = 0;
                }
                if ((startInMiddle && even) || (!startInMiddle && subEven))
                {
                    zModifier = 0.5f;
                    perspective = 1f;
                }
                else
                {
                    zModifier = 0;
                    perspective = 1.15f;
                }
                if (!startInMiddle)
                {
                    if (even)
                    {
                        xModifier = -0.28f * halfHalf;
                    }
                    else
                    {
                        xModifier = +0.32f * halfHalf;
                    }
                }
                else
                {
                    if (subEven)
                    {
                        xModifier = -0.61f * (halfHalf);
                    }
                    else
                    {
                        xModifier = +0.59f * (halfHalf);
                    }
                }
            }

            baseVector.z += zModifier;
            baseVector.x += xModifier;

            return baseVector;
        }

        // Token: 0x0600001B RID: 27 RVA: 0x00002C58 File Offset: 0x00000E58
        public override void PostDraw()
        {
            if (!parent.Spawned || parent.Map != Current.Game.CurrentMap || fishData.Count <= 0)
            {
                return;
            }
            selectedSand = !string.IsNullOrEmpty(selectedSandDefName)
                ? DefsCacher.AQSandDefs.IndexOf((from sandDef in DefsCacher.AQSandDefs where sandDef.defName == selectedSandDefName select sandDef).First())
                : -1;
            selectedDecoration = !string.IsNullOrEmpty(selectedDecorationDefName)
                ? DefsCacher.AQDecorationDefs.IndexOf((from decorationDef in DefsCacher.AQDecorationDefs where decorationDef.defName == selectedDecorationDefName select decorationDef).First())
                : -1;

            int count = 0;
            var rand = new System.Random();
            var totalFish = fishData.Count();
            if (fishWandering == null)
                fishWandering = new List<float[]>();

            float tankXRadius = parent.def.size.x / 2;
            float tankZRadius = parent.def.size.z / 2;
            if (tankXRadius > 1)
            {
                tankXRadius += tankXRadius * 0.45f;
            }
            if (tankZRadius > 1)
            {
                tankZRadius += tankZRadius * 0.25f;
            }
            tankXRadius *= swimAdjustment;
            tankZRadius *= swimAdjustment;
            Matrix4x4 matrix4x;
            Vector3 Size;
            Graphic ImageGraphic;
            if (Props.maxFish > 1)
            {
                if (selectedSand >= 0)
                {
                    var sandLocation = parent.DrawPos;
                    sandLocation.z -= 0.1f;
                    sandLocation.y += 0.0254545468f;
                    Size = new Vector3(parent.def.size.x - 0.54f, 1.0f, 0.7f);
                    var path = parent.Graphic.path.Replace("_Empty", "_Sand");
                    ImageGraphic = GraphicDatabase.Get<Graphic_Single>(path, ShaderDatabase.Cutout, Vector2.one, DefsCacher.AQSandDefs[selectedSand].graphicData.color);
                    matrix4x = default;
                    matrix4x.SetTRS(sandLocation, Quaternion.AngleAxis(0, Vector3.up), Size);
                    Graphics.DrawMesh(MeshPool.plane10, matrix4x, ImageGraphic.MatSingle, 0);
                }
                if (selectedDecoration >= 0)
                {
                    var decorationLocation = parent.DrawPos;
                    decorationLocation.z += 0.11f;
                    decorationLocation.y += 0.0354545468f;
                    Size = new Vector3(parent.def.size.x - 0.54f, 1.0f, 1.2f);
                    var path = $"{parent.Graphic.path.Replace("_Empty", "_Decoration")}_{selectedDecoration}";
                    ImageGraphic = GraphicDatabase.Get<Graphic_Single>(path, ShaderDatabase.TransparentPostLight, Vector2.one, Color.white);
                    matrix4x = default;
                    matrix4x.SetTRS(decorationLocation, Quaternion.AngleAxis(0, Vector3.up), Size);
                    Graphics.DrawMesh(MeshPool.plane10, matrix4x, ImageGraphic.MatSingle, 0);
                }
            }

            if (fishData.Count > fishWandering.Count)
            {
                for (int i = fishWandering.Count - 1; i < fishData.Count; i++)
                {
                    if (Controller.Settings.FishMovesAround)
                    {
                        if (rand.Next(0, 2) < 1)
                        {
                            fishWandering.Add(new float[] { (float)(rand.NextDouble() * tankXRadius), (float)(rand.NextDouble() * tankZRadius), 0, rand.Next(0, 2) });
                        }
                        else
                        {
                            fishWandering.Add(new float[] { (float)(rand.NextDouble() * -tankXRadius), (float)(rand.NextDouble() * -tankZRadius), 0, rand.Next(0, 2) });
                        }
                    }
                    else
                    {
                        fishWandering.Add(new float[] { 0, 0, 0, rand.Next(0, 2) });
                    }
                }
            }

            foreach (string value in fishData)
            {
                if (NumValuePart(value, 4) == 1)
                {
                    continue;
                }
                var iterator = count;
                count++;
                int age = NumValuePart(value, 3);
                float ageFactor = Mathf.Lerp(0.5f, 1f, Math.Min(age, (float)oldFishAge) / oldFishAge);
                Vector3 drawPos = parent.DrawPos;
                float perspective = 1f;
                if (Controller.Settings.FishMovesAround && Props.maxFish > 1)
                {
                    drawPos.z += 0.17f;
                    if (!Find.TickManager.Paused)
                    {
                        var direction = fishWandering[iterator][3];
                        if (direction == 0)
                        {
                            fishWandering[iterator][0] += RandomFloat(-wanderingSpeed, 0);
                            if (fishWandering[iterator][0] < -tankXRadius)
                            {
                                fishWandering[iterator][0] = -tankXRadius;
                                fishWandering[iterator][3] = 1f;
                            }
                        }
                        else
                        {
                            fishWandering[iterator][0] += RandomFloat(0, wanderingSpeed);
                            if (fishWandering[iterator][0] > tankXRadius)
                            {
                                fishWandering[iterator][0] = tankXRadius;
                                fishWandering[iterator][3] = 0;
                            }
                        }
                        fishWandering[iterator][1] += RandomFloat(-wanderingSpeed, wanderingSpeed);
                        if (fishWandering[iterator][1] < -tankZRadius)
                            fishWandering[iterator][1] = -tankZRadius;
                        if (fishWandering[iterator][1] > tankZRadius)
                            fishWandering[iterator][1] = tankZRadius;
                        fishWandering[iterator][2] += RandomFloat(-wanderingSpeed, wanderingSpeed);
                        if (fishWandering[iterator][2] < -maxPerspective)
                            fishWandering[iterator][2] = -maxPerspective;
                        if (fishWandering[iterator][2] > maxPerspective)
                            fishWandering[iterator][2] = maxPerspective;
                        perspective += fishWandering[iterator][2];
                        if (rand.Next(1000) == 999)
                        {
                            fishWandering[iterator][3] = fishWandering[iterator][3] == 0 ? 1 : (float)0;
                        }
                    }
                }
                else
                {
                    drawPos.z -= 0.1f;
                    if (totalFish > 1)
                    {
                        drawPos = GetDrawPos(count, drawPos, out perspective);
                    }
                    if (!Find.TickManager.Paused)
                    {
                        var direction = fishWandering[iterator][3];
                        if (direction == 0)
                        {
                            fishWandering[iterator][0] += RandomFloat(-wanderingSpeed, 0);
                            if (fishWandering[iterator][0] < -maxWandering)
                            {
                                fishWandering[iterator][0] = -maxWandering;
                                fishWandering[iterator][3] = 1f;
                            }
                        }
                        else
                        {
                            fishWandering[iterator][0] += RandomFloat(0, wanderingSpeed);
                            if (fishWandering[iterator][0] > maxWandering)
                            {
                                fishWandering[iterator][0] = maxWandering;
                                fishWandering[iterator][3] = 0;
                            }
                        }
                        fishWandering[iterator][1] += RandomFloat(-wanderingSpeed, wanderingSpeed);
                        if (fishWandering[iterator][1] < -maxWandering)
                            fishWandering[iterator][1] = -maxWandering;
                        if (fishWandering[iterator][1] > maxWandering)
                            fishWandering[iterator][1] = maxWandering;
                    }
                }
                drawPos.x += fishWandering[iterator][0];
                drawPos.z += fishWandering[iterator][1];
                drawPos.y += 0.0454545468f;
                string defname = StringValuePart(value, 1);
                var gfxName = WordsToNumbers(defname.Replace("AQFishInBag", ""));
                string ImagePath = $"Things/Fish/Fish{gfxName}";
                var adjustedSizeVector = ageFactor * perspective;
                Size = new Vector3(adjustedSizeVector, 1f, adjustedSizeVector);
                ImageGraphic = GraphicDatabase.Get<Graphic_Single>(ImagePath, ShaderDatabase.TransparentPostLight, Vector2.one, Color.white);
                matrix4x = default;
                matrix4x.SetTRS(drawPos, Quaternion.AngleAxis(0, Vector3.up), Size);
                if (fishWandering[count - 1][3] == 0)
                {
                    Graphics.DrawMesh(MeshPool.plane10Flip, matrix4x, ImageGraphic.MatSingle, 0);
                }
                else
                {
                    Graphics.DrawMesh(MeshPool.plane10, matrix4x, ImageGraphic.MatSingle, 0);
                }
            }
            if (Props.maxFish > 1)
            {
                var waterLocation = parent.DrawPos;
                waterLocation.z += 0.11f;
                waterLocation.y += 0.0554545468f;
                Size = new Vector3(parent.def.size.x - 0.54f, 1.0f, 1.2f);
                var path = "Things/Tanks/Water";
                ImageGraphic = GraphicDatabase.Get<Graphic_Single>(path, ShaderDatabase.TransparentPostLight, Vector2.one, Color.white);
                matrix4x = default;
                matrix4x.SetTRS(waterLocation, Quaternion.AngleAxis(0, Vector3.up), Size);
                Graphics.DrawMesh(MeshPool.plane10, matrix4x, ImageGraphic.MatSingle, 0);
            }
        }


        // Token: 0x0600001C RID: 28 RVA: 0x00002ED0 File Offset: 0x000010D0
        public override void CompTick()
        {
            base.CompTick();
            if (parent.Spawned)
            {
                if (Controller.Settings.AllowTankSounds && Props.powerNeeded && IsPowered())
                {
                    DoTankSound();
                }
                if (parent.IsHashIntervalTick(Ticks))
                {
                    if (Props.powerNeeded && IsPowered() && ShouldPushHeatNow)
                    {
                        GenTemperature.PushHeat(parent.PositionHeld, parent.MapHeld, Props.heatPerSecond * 5f);
                    }
                    float ageFactor = GetAvgAgeMultiplier(fishData);
                    DegradeFood(numFish, foodPct, ageFactor, out float newFoodPct);
                    foodPct = newFoodPct;
                    DegradeWater(numFish, foodPct, cleanPct, ageFactor, out float newCleanPct);
                    cleanPct = newCleanPct;
                    EffectFish(numFish, fishData, foodPct, cleanPct, out int newNumFish, out List<string> newFishData);
                    numFish = newNumFish;
                    fishData = newFishData;
                    if (fishydebug)
                    {
                        AQUtility.DebugFishData(this, Props.maxFish);
                    }
                }
                if (parent.IsHashIntervalTick(BreedingTicks))
                {
                    TankBreeding();
                }
            }
            else
            {
                numFish = 0;
                DumpFish(fishData);
                fishData = new List<string>();
                GenerateBeauty(fishData);
                foodPct = 0f;
                cleanPct = 1f;
            }
            numFish = fishData.Count;
        }

        // Token: 0x0600001D RID: 29 RVA: 0x00003070 File Offset: 0x00001270
        internal float GetAvgAgeMultiplier(List<string> fdata)
        {
            float factor = 1f;
            float sum = 0f;
            int count = 0;
            if (fdata.Count > 0)
            {
                foreach (string value in fdata)
                {
                    if (NumValuePart(value, 4) != 1)
                    {
                        int age = NumValuePart(value, 3);
                        sum += Mathf.Lerp(0.75f, 1f, Math.Min(oldFishAge, age) / oldFishAge);
                        count++;
                    }
                }
            }
            if (count > 0)
            {
                factor = sum / count;
            }
            return factor;
        }

        // Token: 0x0600001E RID: 30 RVA: 0x00003118 File Offset: 0x00001318
        public override void CompTickRare()
        {
            base.CompTickRare();
        }

        // Token: 0x0600001F RID: 31 RVA: 0x00003120 File Offset: 0x00001320
        public static float RandomFloat(float min, float max)
        {
            return Rand.Range(min, max);
        }

        // Token: 0x06000020 RID: 32 RVA: 0x0000312C File Offset: 0x0000132C
        private void DumpFish(List<string> fishData)
        {
            if (parent.Map != null && parent.Map.ParentFaction == Faction.OfPlayerSilentFail && fishData != null && fishData.Count > 0)
            {
                foreach (string value in fishData)
                {
                    int health = NumValuePart(value, 2);
                    int age = NumValuePart(value, 3);
                    Thing newThing = ThingMaker.MakeThing(ThingDef.Named(StringValuePart(value, 1)), null);
                    newThing.stackCount = 1;
                    if (newThing != null)
                    {
                        GenPlace.TryPlaceThing(newThing, parent.Position, parent.Map, ThingPlaceMode.Near, out Thing newFishbag, null, null, default);
                        if (newFishbag != null)
                        {
                            newFishbag.stackCount = 1;
                            CompAQFishInBag CBag = newFishbag.TryGetComp<CompAQFishInBag>();
                            if (CBag != null)
                            {
                                CBag.age = age;
                                CBag.fishhealth = health;
                            }
                        }
                    }
                }
            }
        }

        public void CleanBeautyItems()
        {
            var parentArea = parent.OccupiedRect();
            HashSet<Thing> thingsToDestroy = new HashSet<Thing>();
            foreach (var cell in parentArea.Cells)
            {
                foreach (var thing in cell.GetThingList(parent.Map))
                {
                    if (!thing.def.defName.StartsWith("AQBeauty"))
                    {
                        continue;
                    }
                    thingsToDestroy.Add(thing);
                }
            }
            foreach (var thing in thingsToDestroy)
            {
                if (!thing.Destroyed)
                {
                    thing.Destroy();
                }
            }
        }

        public void GenerateBeauty(List<string> fishData)
        {
            if (savedBeauty != null)
            {
                foreach (var beautyItem in savedBeauty)
                {
                    if (!beautyItem.Destroyed)
                        beautyItem.Destroy();
                }
            }
            CleanBeautyItems();
            savedBeauty = new List<Thing>();
            if (fishData != null && fishData.Count > 0)
            {
                var beauty = 0f;
                foreach (string value in fishData)
                {
                    if (!BagDefs().Contains(StringValuePart(value, 1)))
                    {
                        continue;
                    }
                    var fish = ThingDef.Named(StringValuePart(value, 1));
                    beauty += fish.statBases.GetStatValueFromList(StatDefOf.MarketValue, 0);
                }
                currentBeauty = (int)Math.Round(beauty / 8);
                var currentBeautyBinaryReversed = new string(Convert.ToString(currentBeauty, 2).Reverse().ToArray());
                for (int i = 0; i < currentBeautyBinaryReversed.Length; i++)
                {
                    if (currentBeautyBinaryReversed[i] == '0')
                    {
                        continue;
                    }
                    var valueToAdd = Convert.ToInt32(Math.Pow(2, i));
                    var thingToSpawn = ThingDef.Named($"AQBeauty{NumberToWords(valueToAdd).CapitalizeFirst()}");
                    thingToSpawn.size = parent.def.size;
                    var position = parent.Position;
                    if (thingToSpawn.size.x == 4)
                    {
                        position.x -= 1;
                        position.z -= 1;
                    }
                    savedBeauty.Add(GenSpawn.Spawn(thingToSpawn, position, parent.Map));
                }
            }
            else
            {
                currentBeauty = 0;
            }
        }

        // Token: 0x06000021 RID: 33 RVA: 0x00003238 File Offset: 0x00001438
        private void DegradeFood(int numFish, float foodPct, float ageF, out float newFoodPct)
        {
            float factor = Controller.Settings.DegradeFoodFactor / 100f * 0.0625f * ageF;
            newFoodPct = foodPct;
            newFoodPct -= (0.01f + numFish * RandomFloat(0.01f, 0.04f)) * factor;
            if (newFoodPct < 0f)
            {
                newFoodPct = 0f;
            }
        }

        // Token: 0x06000022 RID: 34 RVA: 0x00003298 File Offset: 0x00001498
        private void DegradeWater(int numFish, float foodPct, float cleanPct, float ageF, out float newCleanPct)
        {
            float factor = Controller.Settings.DegradeWaterFactor / 100f * 0.025f * ageF;
            newCleanPct = cleanPct;
            newCleanPct -= numFish * RandomFloat(0.02f, 0.03f) * factor;
            newCleanPct -= Mathf.Lerp(0f, 0.02f, foodPct) * factor;
            if (newCleanPct < 0f)
            {
                newCleanPct = 0f;
            }
        }

        // Token: 0x06000023 RID: 35 RVA: 0x0000330C File Offset: 0x0000150C
        private void EffectFish(int numFish, List<string> fishData, float foodPct, float cleanPct, out int newNumFish, out List<string> newFishData)
        {
            newNumFish = numFish;
            newFishData = fishData;
            if (numFish > 0)
            {
                int degradingHealth = 0;
                if (foodPct <= 0f)
                {
                    degradingHealth++;
                }
                if (cleanPct <= 0.25f)
                {
                    degradingHealth++;
                }
                if (parent.AmbientTemperature > 55f || parent.AmbientTemperature < 1f)
                {
                    degradingHealth++;
                }
                int newIndex = 0;
                List<string> changedFishData = new List<string>();
                if (newFishData.Count > 0)
                {
                    foreach (string value in newFishData)
                    {
                        bool died = false;
                        NumValuePart(value, 0);
                        string defval = StringValuePart(value, 1);
                        int health = NumValuePart(value, 2);
                        int age = NumValuePart(value, 3);
                        int action = NumValuePart(value, 4);
                        int agedegradingHealth = 0;
                        if (action != 1)
                        {
                            age += Ticks;
                            if (age + (int)RandomFloat(-450000f, 450000f) > oldFishAge)
                            {
                                agedegradingHealth = 1;
                            }
                            int poorhealth = degradingHealth + agedegradingHealth;
                            if (poorhealth > 0)
                            {
                                health -= (int)Math.Max(1f, poorhealth * Mathf.Lerp(0.5f, 1f, Math.Max(1f, age / (float)oldFishAge)));
                                if (health <= 0)
                                {
                                    died = true;
                                }
                            }
                            else if (health < 100)
                            {
                                health += (int)Math.Max(1f, Mathf.Lerp(1f, 0.5f, Math.Max(1f, age / (float)oldFishAge)));
                                if (health > 100)
                                {
                                    health = 100;
                                }
                            }
                        }
                        if (!died)
                        {
                            newIndex++;
                            string newValue = CreateValuePart(newIndex, defval, health, age, action);
                            changedFishData.Add(newValue);
                        }
                        else
                        {
                            if (parent.Spawned)
                            {
                                ThingWithComps parent = this.parent;
                                if ((parent?.Map) != null && this.parent.Map.ParentFaction == Faction.OfPlayerSilentFail)
                                {
                                    if (Controller.Settings.DoDeathMsgs)
                                    {
                                        Messages.Message("Aquarium.FishDied".Translate(), this.parent, MessageTypeDefOf.NegativeEvent, false);
                                    }
                                    AQUtility.DoSpawnTropicalFishMeat(this.parent, age);
                                }
                            }
                            newNumFish--;
                            if (newNumFish < 0)
                            {
                                newNumFish = 0;
                            }
                        }
                    }
                }
                if (fishData.Count != changedFishData.Count)
                {
                    GenerateBeauty(changedFishData);
                }
                newFishData = changedFishData;
            }
        }

        // Token: 0x06000024 RID: 36 RVA: 0x00003570 File Offset: 0x00001770
        internal static string CreateValuePart(int newIndex, string defVal, int health, int age, int action)
        {
            return string.Concat(new string[]
            {
                newIndex.ToString(),
                ";",
                defVal,
                ";",
                health.ToString(),
                ";",
                age.ToString(),
                ";",
                action.ToString()
            });
        }

        // Token: 0x06000025 RID: 37 RVA: 0x000035D8 File Offset: 0x000017D8
        internal static int NumValuePart(string value, int pos)
        {
            char[] divider = new char[]
            {
                ';'
            };
            string[] segments = value.Split(divider);
            try
            {
                return int.Parse(segments[pos]);
            }
            catch (FormatException)
            {
                Log.Message("Unable to parse Segment: '" + segments[pos] + "' : " + pos.ToString(), false);
            }
            return 0;
        }

        // Token: 0x06000026 RID: 38 RVA: 0x0000363C File Offset: 0x0000183C
        internal static string StringValuePart(string value, int pos)
        {
            char[] divider = new char[]
            {
                ';'
            };
            return value.Split(divider)[pos];
        }

        // Token: 0x06000027 RID: 39 RVA: 0x00003660 File Offset: 0x00001860
        public void StartMixSustainer()
        {
            SoundInfo info = SoundInfo.InMap(parent, MaintenanceType.PerTick);
            mixSustainer = SoundDef.Named("AQFishTank").TrySpawnSustainer(info);
        }

        // Token: 0x06000028 RID: 40 RVA: 0x00003695 File Offset: 0x00001895
        private void DoTankSound()
        {
            if (mixSustainer == null)
            {
                StartMixSustainer();
                return;
            }
            if (mixSustainer.Ended)
            {
                StartMixSustainer();
                return;
            }
            mixSustainer.Maintain();
        }

        // Token: 0x06000029 RID: 41 RVA: 0x000036C5 File Offset: 0x000018C5
        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            string graphicPath;
            string defaultLabel;
            if (Props.maxFish > 1)
            {
                selectedSand = !string.IsNullOrEmpty(selectedSandDefName)
                ? DefsCacher.AQSandDefs.IndexOf((from sandDef in DefsCacher.AQSandDefs where sandDef.defName == selectedSandDefName select sandDef).First())
                : -1;
                selectedDecoration = !string.IsNullOrEmpty(selectedDecorationDefName)
                    ? DefsCacher.AQDecorationDefs.IndexOf((from decorationDef in DefsCacher.AQDecorationDefs where decorationDef.defName == selectedDecorationDefName select decorationDef).First())
                    : -1;

                graphicPath = "Things/Fish/UI/Sand_Icon";
                defaultLabel = selectedSand >= 0
                    ? $"{("Aquarium." + DefsCacher.AQSandDefs[selectedSand].label).Translate()}\n{"Aquarium.Sand".Translate()}"
                    : (string)"Aquarium.Sand".Translate();
                yield return new Command_Action
                {
                    defaultLabel = defaultLabel,
                    icon = ContentFinder<Texture2D>.Get(graphicPath, true),
                    defaultDesc = "Aquarium.SandDesc".Translate(),
                    action = delegate ()
                    {
                        SelectSand();
                    }
                };
                graphicPath = "Things/Fish/UI/Decoration_Icon";
                defaultLabel = selectedDecoration >= 0
                    ? $"{("Aquarium." + DefsCacher.AQDecorationDefs[selectedDecoration].label).Translate()}\n{"Aquarium.Decorations".Translate()}"
                    : (string)"Aquarium.Decorations".Translate();
                yield return new Command_Action
                {
                    defaultLabel = defaultLabel,
                    icon = ContentFinder<Texture2D>.Get(graphicPath, true),
                    defaultDesc = "Aquarium.DecorationsDesc".Translate(),
                    action = delegate ()
                    {
                        SelectDecorations();
                    }
                };
            }
            for (int i = 1; i <= Props.maxFish; i++)
            {
                ThingDef fishDef = null;
                int fishHealth = 0;
                int fishAge = 0;
                int fishAction = 0;
                string fishstring = null;
                if (fishData.Count > 0)
                {
                    foreach (string value in fishData)
                    {
                        if (NumValuePart(value, 0) == i)
                        {
                            fishstring = StringValuePart(value, 1);
                            if (fishstring != null)
                            {
                                fishDef = ThingDef.Named(fishstring);
                            }
                            fishHealth = NumValuePart(value, 2);
                            fishAge = (int)(NumValuePart(value, 3) / 60000f);
                            fishAction = NumValuePart(value, 4);
                            break;
                        }
                    }
                }
                string numLabel = i.ToString() + ": ";
                string fishLabel = numLabel + "Aquarium.NoFish".Translate();
                graphicPath = "Things/Fish/UI/NoFish";
                string fishDesc = "Aquarium.FishSelection".Translate();
                if (fishDef != null)
                {
                    var fishGfx = fishstring.Replace("AQFishInBag", "");
                    graphicPath = fishstring == "AQRandomFish" ? $"Things/Fish/UI/Random_Icon" : $"Things/Fish/Fish{WordsToNumbers(fishGfx)}";
                    if (fishAction == 1)
                    {
                        fishLabel = numLabel + "Aquarium.Adding".Translate();
                    }
                    else if (fishAction == 2)
                    {
                        fishLabel = numLabel + "Aquarium.Removing".Translate();
                    }
                    else
                    {
                        fishLabel = numLabel + fishDef.LabelCap + "\nH: " + (fishHealth / 100f).ToStringPercent() + ", " + fishAge.ToString() + " " + "Aquarium.days".Translate();
                    }
                }
                Texture2D fishIcon = ContentFinder<Texture2D>.Get(graphicPath, true);
                yield return new Command_Action
                {
                    defaultLabel = fishLabel,
                    icon = fishIcon,
                    defaultDesc = fishDesc,
                    action = delegate ()
                    {
                        SelectFish(fishDef, numLabel);
                    }
                };
            }
            if (Prefs.DevMode)
            {
                yield return new Command_Toggle
                {
                    icon = ContentFinder<Texture2D>.Get(debugTexPath, true),
                    defaultLabel = "Debug Mode",
                    defaultDesc = "Send debug messages to Log",
                    isActive = (() => fishydebug),
                    toggleAction = delegate ()
                    {
                        ToggleDebug(fishydebug);
                    }
                };
            }
            yield break;
        }

        // Token: 0x0600002A RID: 42 RVA: 0x000036D5 File Offset: 0x000018D5
        public void ToggleDebug(bool flag)
        {
            fishydebug = !flag;
        }

        // Token: 0x0600002B RID: 43 RVA: 0x000036E4 File Offset: 0x000018E4
        private void SelectFish(ThingDef afishDef, string fishLabel)
        {
            int selindex = Convert.ToInt32(fishLabel.Split(':')[0]);
            List<FloatMenuOption> list = new List<FloatMenuOption>();
            string text = "Aquarium.DoNothing".Translate();
            list.Add(new FloatMenuOption(text, delegate ()
            {
                FishSelection(null, 0, ActionType.Nothing);
            }, MenuOptionPriority.Default, null, null, 29f, null, null));
            if (afishDef != null)
            {
                text = "Aquarium.RemoveFish".Translate();
                list.Add(new FloatMenuOption(text, delegate ()
                {
                    FishSelection(afishDef, selindex, ActionType.Remove);
                }, MenuOptionPriority.Default, null, null, 29f, null, null));
            }
            else
            {
                var randomFishDef = DefDatabase<ThingDef>.GetNamed("AQRandomFish");
                list.Add(new FloatMenuOption("Aquarium.RandomFish".Translate(), delegate ()
                {
                    FishSelection(randomFishDef, selindex, ActionType.Add);
                }, MenuOptionPriority.Default, null, null, 29f, null, null));
                if (ReachableDefs.Count == 0)
                {
                    list.Add(new FloatMenuOption("Aquarium.NoReachableFish".Translate(), null, MenuOptionPriority.Default, null, null, 29f, null, null));
                }
                foreach (string defName in ReachableDefs)
                {
                    ThingDef potfishDef = ThingDef.Named(defName);
                    text = "Aquarium.AddFish".Translate() + " " + potfishDef.LabelCap;
                    list.Add(new FloatMenuOption(text, delegate ()
                    {
                        FishSelection(potfishDef, selindex, ActionType.Add);
                    }, MenuOptionPriority.Default, null, null, 29f, null, null));
                }
            }
            Find.WindowStack.Add(new FloatMenu(list));
        }

        private void SelectSand()
        {
            List<FloatMenuOption> list = new List<FloatMenuOption>();
            for (int i = DefsCacher.AQSandDefs.Count - 1; i >= 0; i--)
            {
                var textToAdd = $"Aquarium.{DefsCacher.AQSandDefs[i].label}".Translate();
                var sandDef = DefsCacher.AQSandDefs[i];
                list.Add(new FloatMenuOption(textToAdd, delegate ()
                {
                    selectedSandDefName = sandDef.defName;
                }, MenuOptionPriority.Default, null, null, 29f, null, null));
            }
            var sortedList = list.OrderBy(option => option.Label).ToList();
            Find.WindowStack.Add(new FloatMenu(sortedList));
        }

        private void SelectDecorations()
        {
            List<FloatMenuOption> list = new List<FloatMenuOption>();
            for (int i = 0; i < DefsCacher.AQDecorationDefs.Count; i++)
            {
                var textToAdd = $"Aquarium.{DefsCacher.AQDecorationDefs[i].label}".Translate();
                var decorationDef = DefsCacher.AQDecorationDefs[i];
                list.Add(new FloatMenuOption(textToAdd, delegate ()
                {
                    selectedDecorationDefName = decorationDef.defName;
                }, MenuOptionPriority.Default, null, null, 29f, null, null));
            }
            var sortedList = list.OrderBy(option => option.Label).ToList();
            Find.WindowStack.Add(new FloatMenu(sortedList));
        }

        // Token: 0x0600002C RID: 44 RVA: 0x00003850 File Offset: 0x00001A50
        public void FishSelection(ThingDef selfishDef, int fishindex, ActionType actionType)
        {
            switch (actionType)
            {
                case ActionType.Add:
                    List<string> newFishData = new List<string>();
                    int newindex = 0;
                    if (fishData.Count > 0)
                    {
                        foreach (string value3 in fishData)
                        {
                            NumValuePart(value3, 0);
                            string prevfishdefName = StringValuePart(value3, 1);
                            int prevHealth = NumValuePart(value3, 2);
                            int prevAge = NumValuePart(value3, 3);
                            int prevAction = NumValuePart(value3, 4);
                            newindex++;
                            string newValue = CreateValuePart(newindex, prevfishdefName, prevHealth, prevAge, prevAction);
                            newFishData.Add(newValue);
                        }
                    }
                    newindex++;
                    string addValue = CreateValuePart(newindex, selfishDef.defName, 0, 0, 1);
                    newFishData.Add(addValue);
                    fishData = newFishData;
                    break;
                case ActionType.Remove:
                    List<string> newFishData2 = new List<string>();
                    bool cancel = false;
                    if (fishData.Count > 0)
                    {
                        foreach (string value in fishData)
                        {
                            int row = NumValuePart(value, 0);
                            string prevfishdefName2 = StringValuePart(value, 1);
                            int prevHealth2 = NumValuePart(value, 2);
                            int prevAge2 = NumValuePart(value, 3);
                            int prevAction2 = NumValuePart(value, 4);
                            if (row == fishindex)
                            {
                                if (prevAction2 == 0)
                                {
                                    string newValue2 = CreateValuePart(row, prevfishdefName2, prevHealth2, prevAge2, 2);
                                    newFishData2.Add(newValue2);
                                }
                                else if (prevAction2 == 1)
                                {
                                    string newValue3 = CreateValuePart(row, prevfishdefName2, prevHealth2, prevAge2, 3);
                                    newFishData2.Add(newValue3);
                                    cancel = true;
                                }
                                else
                                {
                                    newFishData2.Add(value);
                                }
                            }
                            else
                            {
                                newFishData2.Add(value);
                            }
                        }
                    }
                    fishData = newFishData2;
                    if (cancel)
                    {
                        List<string> cancelFishData = new List<string>();
                        int newindex2 = 0;
                        if (fishData.Count > 0)
                        {
                            foreach (string value2 in fishData)
                            {
                                newindex2++;
                                int prevAction3 = NumValuePart(value2, 4);
                                if (prevAction3 != 3)
                                {
                                    string newValue4 = CreateValuePart(newindex2, StringValuePart(value2, 1), NumValuePart(value2, 2), NumValuePart(value2, 3), prevAction3);
                                    cancelFishData.Add(newValue4);
                                }
                                else
                                {
                                    newindex2--;
                                }
                            }
                        }
                        fishData = cancelFishData;
                    }
                    break;
                default:
                    break;
            }
        }

        // Token: 0x0600002D RID: 45 RVA: 0x00003AD0 File Offset: 0x00001CD0
        public override string CompInspectStringExtra()
        {
            return "Aquarium.TankInfo".Translate(cleanPct.ToStringPercent(), foodPct.ToStringPercent(), currentBeauty);
        }

        // Token: 0x0600002F RID: 47 RVA: 0x00003C24 File Offset: 0x00001E24
        private List<string> BagDefs()
        {
            var bagDefs = (from bagdef in DefDatabase<ThingDef>.AllDefsListForReading where bagdef.defName.StartsWith("AQFishInBag") select bagdef.defName).ToList();
            bagDefs.SortBy((string s) => ThingDef.Named(s).label);
            return bagDefs;
        }

        private static int WordsToNumbers(string word)
        {
            for (int i = 0; i < 500; i++)
            {
                if (NumberToWords(i) == word.ToLower())
                {
                    return i;
                }
            }
            return 1;
        }

        private static string NumberToWords(int number)
        {
            if (number == 0)
                return "zero";

            if (number < 0)
                return "minus" + NumberToWords(Math.Abs(number));

            string words = "";

            if ((number / 1000000) > 0)
            {
                words += NumberToWords(number / 1000000) + "million";
                number %= 1000000;
            }

            if ((number / 1000) > 0)
            {
                words += NumberToWords(number / 1000) + "thousand";
                number %= 1000;
            }

            if ((number / 100) > 0)
            {
                words += NumberToWords(number / 100) + "hundred";
                number %= 100;
            }

            if (number > 0)
            {
                var unitsMap = new[] { "zero", "one", "two", "three", "four", "five", "six", "seven", "eight", "nine", "ten", "eleven", "twelve", "thirteen", "fourteen", "fifteen", "sixteen", "seventeen", "eighteen", "nineteen" };
                var tensMap = new[] { "zero", "ten", "twenty", "thirty", "forty", "fifty", "sixty", "seventy", "eighty", "ninety" };

                if (number < 20)
                    words += unitsMap[number];
                else
                {
                    words += tensMap[number / 10];
                    if ((number % 10) > 0)
                        words += unitsMap[number % 10];
                }
            }

            return words;
        }


        // Token: 0x06000030 RID: 48 RVA: 0x00003CE8 File Offset: 0x00001EE8
        private bool IsPowered()
        {
            if (!parent.Spawned)
            {
                return false;
            }
            if (parent.IsBrokenDown())
            {
                return false;
            }
            CompPowerTrader CPT = parent.TryGetComp<CompPowerTrader>();
            return CPT != null && CPT.PowerOn;
        }

        // Token: 0x17000003 RID: 3
        // (get) Token: 0x06000031 RID: 49 RVA: 0x00003D2D File Offset: 0x00001F2D
        private bool ShouldPushHeatNow
        {
            get
            {
                return parent.SpawnedOrAnyParentSpawned && parent.AmbientTemperature < Props.targetTemp;
            }
        }

        public List<string> ReachableDefs
        {
            get
            {
                var bagDefs = BagDefs();
                var currentDefs = (from x in parent.Map.listerThings.ThingsInGroup(ThingRequestGroup.HaulableEver)
                                   where bagDefs.Contains(x.def.defName) && !x.Position.Fogged(parent.Map) && !x.IsForbidden(Faction.OfPlayerSilentFail)
                                   orderby x.def.label
                                   select x.def.defName).ToList();
                currentDefs.RemoveDuplicates();
                return currentDefs;
            }
        }

        // Token: 0x06000032 RID: 50 RVA: 0x00003D5C File Offset: 0x00001F5C
        internal void TankBreeding()
        {
            if (Controller.Settings.AllowBreed && numFish > 1 && numFish < Props.maxFish && fishData.Count > 0)
            {
                List<string> breeders = new List<string>();
                List<string> potentials = new List<string>();
                List<string> newFishList = new List<string>();
                int newindex = 0;
                bool birth = false;
                foreach (string fish in fishData)
                {
                    newindex++;
                    if (NumValuePart(fish, 3) >= oldFishAge / 2 && NumValuePart(fish, 4) == 0)
                    {
                        string fishdef = StringValuePart(fish, 1);
                        if (breeders.Contains(fishdef))
                        {
                            potentials.AddDistinct(fishdef);
                        }
                        breeders.Add(fishdef);
                    }
                    newFishList.Add(fish);
                }
                if (potentials.Count > 0 && RandomFloat(1f, 100f) <= Controller.Settings.BreedChance)
                {
                    newindex++;
                    string babyFishDef = potentials.RandomElement();
                    string newValue = CreateValuePart(newindex, babyFishDef, 75, 0, 0);
                    newFishList.Add(newValue);
                    birth = true;
                }
                if (birth)
                {
                    fishData = newFishList;
                    GenerateBeauty(fishData);
                    numFish++;
                }
            }
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            GenerateBeauty(fishData);
        }

        public enum ActionType
        {
            Nothing,
            Add,
            Remove
        }

        // Token: 0x04000008 RID: 8
        private const int Ticks = 300;

        internal static float maxPerspective = 0.25f;

        internal static float swimAdjustment = 0.5f;

        internal static float maxWandering = 0.07f;

        internal static float wanderingSpeed = 0.0025f;

        // Token: 0x04000009 RID: 9
        internal static int oldFishAge = 3600000 * (int)Controller.Settings.FishLife;

        // Token: 0x0400000A RID: 10
        internal const int FishAgeSpread = 450000;

        // Token: 0x0400000B RID: 11
        private const int BreedingTicks = 300000;

        // Token: 0x0400000C RID: 12
        internal int numFish;

        internal int currentBeauty = 0;

        internal int selectedDecoration = -1;

        internal int selectedSand = -1;

        internal string selectedDecorationDefName = string.Empty;

        internal string selectedSandDefName = string.Empty;

        // Token: 0x0400000D RID: 13
        internal float cleanPct = 1f;

        // Token: 0x0400000E RID: 14
        internal float foodPct;

        // Token: 0x0400000F RID: 15
        internal List<string> fishData = new List<string>();

        internal List<float[]> fishWandering = new List<float[]>();

        internal List<Thing> reservedFish = new List<Thing>();

        internal List<Thing> savedBeauty = new List<Thing>();

        // Token: 0x04000010 RID: 16
        internal bool fishydebug;

        // Token: 0x04000011 RID: 17
        private const float DegradeRateBalancer = 0.125f;

        // Token: 0x04000012 RID: 18
        public Sustainer mixSustainer;

        // Token: 0x04000013 RID: 19
        [NoTranslate]
        private readonly string debugTexPath = "Things/Fish/UI/Debug_Icon";
    }
}
