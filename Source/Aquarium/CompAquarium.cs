using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;
using Random = System.Random;

namespace Aquarium;

public class CompAquarium : ThingComp
{
    private const int Ticks = 300;

    private const float maxPerspective = 0.25f;

    private const float swimAdjustment = 0.5f;

    private const float maxWandering = 0.07f;

    private const float wanderingSpeed = 0.0025f;

    internal const int FishAgeSpread = 450000;

    private const int BreedingTicks = 300000;

    private const float DegradeRateBalancer = 0.125f;

    internal static readonly int oldFishAge = 3600000 * (int)Controller.Settings.FishLife;

    [NoTranslate] private readonly string debugTexPath = "Things/Fish/UI/Debug_Icon";

    internal float cleanPct = 1f;

    private int currentBeauty;

    internal List<string> fishData = [];

    private List<float[]> fishWandering = [];

    private bool fishydebug;

    internal float foodPct;

    private bool keepFilling;

    private Sustainer mixSustainer;

    internal int numFish;

    internal List<Thing> reservedFish = [];

    private List<Thing> savedBeauty = [];

    private int selectedDecoration = -1;

    private string selectedDecorationDefName = string.Empty;

    private int selectedSand = -1;

    private string selectedSandDefName = string.Empty;

    public CompAquarium()
    {
        currentBeauty = 0;
    }

    private CompProperties_CompAquarium Props => (CompProperties_CompAquarium)props;

    private bool ShouldPushHeatNow =>
        parent.SpawnedOrAnyParentSpawned && parent.AmbientTemperature < Props.targetTemp;

    public List<string> ReachableDefs
    {
        get
        {
            var currentDefs = (from x in parent.Map.listerThings.ThingsInGroup(ThingRequestGroup.HaulableEver)
                where DefsCacher.AQBagDefs.Contains(x.def) && !x.Position.Fogged(parent.Map) &&
                      !x.IsForbidden(Faction.OfPlayerSilentFail)
                orderby x.def.label
                select x.def.defName).ToList();
            currentDefs.RemoveDuplicates();
            return currentDefs;
        }
    }

    public override void PostExposeData()
    {
        base.PostExposeData();
        Scribe_Values.Look(ref numFish, "numFish");
        Scribe_Values.Look(ref selectedSandDefName, "selectedSandDefName", string.Empty);
        Scribe_Values.Look(ref selectedDecorationDefName, "selectedDecorationDefName", string.Empty);
        Scribe_Values.Look(ref cleanPct, "cleanPct", 1f);
        Scribe_Values.Look(ref foodPct, "foodPct");
        Scribe_Values.Look(ref keepFilling, "keepFilling");
        Scribe_Collections.Look(ref fishData, "fishData", LookMode.Value);
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

    private Vector3 getDrawPos(int currentFishNumber, Vector3 baseVector, out float perspective)
    {
        var startInMiddle = true;
        perspective = 1f;
        if (Math.Ceiling(Props.maxFish / (decimal)2) % 2 == 0)
        {
            startInMiddle = false;
        }

        var xModifier = 0f;
        var zModifier = 0f;
        for (var i = 1; i <= currentFishNumber; i++)
        {
            var even = i % 2 == 0;
            var half = (float)Math.Ceiling((double)i / 2);
            var subEven = half % 2 == 0;
            var halfHalf = (float)Math.Ceiling((double)i / 2 / 2);
            if (startInMiddle)
            {
                halfHalf = (float)Math.Ceiling(((double)i - 2) / 2 / 2);
                if (halfHalf < 0)
                {
                    halfHalf = 0;
                }
            }

            if (startInMiddle && even || !startInMiddle && subEven)
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
                    xModifier = -0.61f * halfHalf;
                }
                else
                {
                    xModifier = +0.59f * halfHalf;
                }
            }
        }

        baseVector.z += zModifier;
        baseVector.x += xModifier;

        return baseVector;
    }

    public override void PostDraw()
    {
        if (!parent.Spawned || parent.Map != Current.Game.CurrentMap)
        {
            return;
        }

        selectedSand = !string.IsNullOrEmpty(selectedSandDefName)
            ? DefsCacher.AQSandDefs.IndexOf((from sandDef in DefsCacher.AQSandDefs
                where sandDef.defName == selectedSandDefName
                select sandDef).First())
            : -1;
        selectedDecoration = !string.IsNullOrEmpty(selectedDecorationDefName)
            ? DefsCacher.AQDecorationDefs.IndexOf((from decorationDef in DefsCacher.AQDecorationDefs
                where decorationDef.defName == selectedDecorationDefName
                select decorationDef).First())
            : -1;

        var tankXRadius = parent.def.size.x / (float)2;
        var tankZRadius = parent.def.size.z / (float)2;
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
        Matrix4x4 matrix4X;
        Vector3 size;
        Graphic imageGraphic;
        string path;
        if (Props.maxFish > 1)
        {
            if (selectedSand >= 0)
            {
                var sandLocation = parent.DrawPos;
                sandLocation.z -= 0.1f;
                sandLocation.y += 0.0254545468f;
                size = new Vector3(parent.def.size.x - 0.54f, 1.0f, parent.def.size.z - 1.3f);
                path = parent.Graphic.path.Replace("_Empty", "_Sand");
                imageGraphic = GraphicDatabase.Get<Graphic_Single>(path, ShaderDatabase.Cutout, Vector2.one,
                    DefsCacher.AQSandDefs[selectedSand].graphicData.color);
                matrix4X = default;
                matrix4X.SetTRS(sandLocation, Quaternion.AngleAxis(0, Vector3.up), size);
                Graphics.DrawMesh(MeshPool.plane10, matrix4X, imageGraphic.MatSingle, 0);
            }

            if (selectedDecoration >= 0)
            {
                var decorationLocation = parent.DrawPos;
                decorationLocation.z += 0.11f;
                decorationLocation.y += 0.0354545468f;
                size = new Vector3(parent.def.size.x - 0.54f, 1.0f, parent.def.size.z - 0.8f);
                path = $"{parent.Graphic.path.Replace("_Empty", "_Decoration")}_{selectedDecoration}";
                imageGraphic = GraphicDatabase.Get<Graphic_Single>(path, ShaderDatabase.TransparentPostLight,
                    Vector2.one, Color.white);
                matrix4X = default;
                matrix4X.SetTRS(decorationLocation, Quaternion.AngleAxis(0, Vector3.up), size);
                Graphics.DrawMesh(MeshPool.plane10, matrix4X, imageGraphic.MatSingle, 0);
            }
        }

        if (fishData.Count > 0)
        {
            var count = 0;
            var rand = new Random();
            var totalFish = fishData.Count;
            fishWandering ??= [];
            if (fishData.Count > fishWandering.Count)
            {
                for (var i = fishWandering.Count - 1; i < fishData.Count; i++)
                {
                    if (Controller.Settings.FishMovesAround)
                    {
                        fishWandering.Add(rand.Next(0, 2) < 1
                            ?
                            [
                                (float)(rand.NextDouble() * tankXRadius), (float)(rand.NextDouble() * tankZRadius), 0,
                                rand.Next(0, 2)
                            ]
                            :
                            [
                                (float)(rand.NextDouble() * -tankXRadius), (float)(rand.NextDouble() * -tankZRadius), 0,
                                rand.Next(0, 2)
                            ]);
                    }
                    else
                    {
                        fishWandering.Add([0, 0, 0, rand.Next(0, 2)]);
                    }
                }
            }

            foreach (var value in fishData)
            {
                if (NumValuePart(value, 4) == 1)
                {
                    continue;
                }

                var iterator = count;
                count++;
                var age = NumValuePart(value, 3);
                var ageFactor = Mathf.Lerp(0.5f, 1f, Math.Min(age, (float)oldFishAge) / oldFishAge);
                var drawPos = parent.DrawPos;
                var perspective = 1f;
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
                        {
                            fishWandering[iterator][1] = -tankZRadius;
                        }

                        if (fishWandering[iterator][1] > tankZRadius)
                        {
                            fishWandering[iterator][1] = tankZRadius;
                        }

                        fishWandering[iterator][2] += RandomFloat(-wanderingSpeed, wanderingSpeed);
                        if (fishWandering[iterator][2] < -maxPerspective)
                        {
                            fishWandering[iterator][2] = -maxPerspective;
                        }

                        if (fishWandering[iterator][2] > maxPerspective)
                        {
                            fishWandering[iterator][2] = maxPerspective;
                        }

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
                        drawPos = getDrawPos(count, drawPos, out perspective);
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
                        {
                            fishWandering[iterator][1] = -maxWandering;
                        }

                        if (fishWandering[iterator][1] > maxWandering)
                        {
                            fishWandering[iterator][1] = maxWandering;
                        }
                    }
                }

                drawPos.x += fishWandering[iterator][0];
                drawPos.z += fishWandering[iterator][1];
                drawPos.y += 0.0454545468f;
                var defName = StringValuePart(value, 1);
                var gfxName = wordsToNumbers(defName.Replace("AQFishInBag", ""));
                var imagePath = $"Things/Fish/Fish{gfxName}";
                var adjustedSizeVector = ageFactor * perspective;
                size = new Vector3(adjustedSizeVector, 1f, adjustedSizeVector);
                imageGraphic = GraphicDatabase.Get<Graphic_Single>(imagePath, ShaderDatabase.TransparentPostLight,
                    Vector2.one, Color.white);
                matrix4X = default;
                matrix4X.SetTRS(drawPos, Quaternion.AngleAxis(0, Vector3.up), size);
                Graphics.DrawMesh(fishWandering[count - 1][3] == 0 ? MeshPool.plane10Flip : MeshPool.plane10,
                    matrix4X, imageGraphic.MatSingle, 0);
            }
        }

        if (Props.maxFish <= 1)
        {
            return;
        }

        var waterLocation = parent.DrawPos;
        waterLocation.z += 0.11f;
        waterLocation.y += 0.0554545468f;
        size = new Vector3(parent.def.size.x - 0.54f, 1.0f, parent.def.size.z - 0.8f);
        path = "Things/Tanks/Water";
        if (parent.def.size.z > 2)
        {
            path = "Things/Tanks/WaterHigh";
        }

        imageGraphic =
            GraphicDatabase.Get<Graphic_Single>(path, ShaderDatabase.TransparentPostLight, Vector2.one,
                Color.white);
        matrix4X = default;
        matrix4X.SetTRS(waterLocation, Quaternion.AngleAxis(0, Vector3.up), size);
        Graphics.DrawMesh(MeshPool.plane10, matrix4X, imageGraphic.MatSingle, 0);
    }


    public override void CompTick()
    {
        base.CompTick();
        if (parent.Spawned)
        {
            if (Controller.Settings.AllowTankSounds && Props.powerNeeded && isPowered())
            {
                doTankSound();
            }

            if (parent.IsHashIntervalTick(Ticks))
            {
                if (Props.powerNeeded && isPowered() && ShouldPushHeatNow)
                {
                    GenTemperature.PushHeat(parent.PositionHeld, parent.MapHeld, Props.heatPerSecond * 5f);
                }

                if (numFish > 0)
                {
                    var ageFactor = getAvgAgeMultiplier(fishData);
                    degradeFood(numFish, foodPct, ageFactor, out var newFoodPct);
                    foodPct = newFoodPct;
                    degradeWater(numFish, foodPct, cleanPct, ageFactor, out var newCleanPct);
                    cleanPct = newCleanPct;
                    effectFish(numFish, fishData, foodPct, cleanPct, out var newNumFish, out var newFishData);
                    numFish = newNumFish;
                    fishData = newFishData;
                }

                if (fishydebug)
                {
                    AQUtility.DebugFishData(this, Props.maxFish);
                }
            }

            if (parent.IsHashIntervalTick(BreedingTicks))
            {
                tankBreeding();
            }
        }
        else
        {
            numFish = 0;
            dumpFish(fishData);
            fishData = [];
            GenerateBeauty(fishData);
            foodPct = 0f;
            cleanPct = 1f;
        }

        numFish = fishData.Count;
        if (Comp_Aquarium_VNPE.VNPELoaded)
        {
            Comp_Aquarium_VNPE.VNPE_Check(parent as Building);
        }
    }

    private static float getAvgAgeMultiplier(List<string> fdata)
    {
        var factor = 1f;
        var sum = 0f;
        var count = 0;
        if (fdata.Count > 0)
        {
            foreach (var value in fdata)
            {
                if (NumValuePart(value, 4) == 1)
                {
                    continue;
                }

                var age = NumValuePart(value, 3);
                sum += Mathf.Lerp(0.75f, 1f, Math.Min(oldFishAge, age) / (float)oldFishAge);
                count++;
            }
        }

        if (count > 0)
        {
            factor = sum / count;
        }

        return factor;
    }


    public static float RandomFloat(float min, float max)
    {
        return Rand.Range(min, max);
    }

    private void dumpFish(List<string> incomingFishData)
    {
        if (parent.Map == null || parent.Map.ParentFaction != Faction.OfPlayerSilentFail ||
            incomingFishData is not { Count: > 0 })
        {
            return;
        }

        foreach (var value in incomingFishData)
        {
            var health = NumValuePart(value, 2);
            var age = NumValuePart(value, 3);
            var newThing = ThingMaker.MakeThing(ThingDef.Named(StringValuePart(value, 1)));
            newThing.stackCount = 1;

            GenPlace.TryPlaceThing(newThing, parent.Position, parent.Map, ThingPlaceMode.Near, out var newFishbag);
            if (newFishbag == null)
            {
                continue;
            }

            newFishbag.stackCount = 1;
            var CBag = newFishbag.TryGetComp<CompAQFishInBag>();
            if (CBag == null)
            {
                continue;
            }

            CBag.age = age;
            CBag.fishhealth = health;
        }
    }

    private void cleanBeautyItems()
    {
        if (!parent.Spawned)
        {
            return;
        }

        var parentArea = parent.OccupiedRect();
        var thingsToDestroy = new HashSet<Thing>();
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

    public void GenerateBeauty(List<string> incomingFishData)
    {
        if (savedBeauty != null)
        {
            foreach (var beautyItem in savedBeauty)
            {
                if (!beautyItem.Destroyed)
                {
                    beautyItem.Destroy();
                }
            }
        }

        cleanBeautyItems();
        savedBeauty = [];
        if (incomingFishData is { Count: > 0 })
        {
            var beauty = 0f;
            foreach (var value in incomingFishData)
            {
                if (!DefsCacher.AQBagDefs.Any(def => def.defName == StringValuePart(value, 1)))
                {
                    continue;
                }

                var fish = ThingDef.Named(StringValuePart(value, 1));
                beauty += fish.statBases.GetStatValueFromList(StatDefOf.MarketValue, 0);
            }

            currentBeauty = (int)Math.Round(beauty / 8);
            var currentBeautyBinaryReversed = new string(Convert.ToString(currentBeauty, 2).Reverse().ToArray());
            for (var i = 0; i < currentBeautyBinaryReversed.Length; i++)
            {
                if (currentBeautyBinaryReversed[i] == '0')
                {
                    continue;
                }

                var valueToAdd = Convert.ToInt32(Math.Pow(2, i));
                var thingToSpawn = ThingDef.Named($"AQBeauty{numberToWords(valueToAdd).CapitalizeFirst()}");
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

    private static void degradeFood(int numberOfFish, float foodAmount, float ageF, out float newFoodPct)
    {
        var factor = Controller.Settings.DegradeFoodFactor / 100f * 0.0625f * ageF;
        newFoodPct = foodAmount;
        newFoodPct -= (0.01f + (numberOfFish * RandomFloat(0.01f, 0.04f))) * factor;
        if (newFoodPct < 0f)
        {
            newFoodPct = 0f;
        }
    }

    private static void degradeWater(int numberFish, float foodAmount, float cleanAmount, float ageF,
        out float newCleanPct)
    {
        var factor = Controller.Settings.DegradeWaterFactor / 100f * 0.025f * ageF;
        newCleanPct = cleanAmount;
        newCleanPct -= numberFish * RandomFloat(0.02f, 0.03f) * factor;
        newCleanPct -= Mathf.Lerp(0f, 0.02f, foodAmount) * factor;
        if (newCleanPct < 0f)
        {
            newCleanPct = 0f;
        }
    }

    private void effectFish(int numberOfFish, List<string> incomingFishData, float foodAmount, float cleanAmount,
        out int newNumFish,
        out List<string> newFishData)
    {
        newNumFish = numberOfFish;
        newFishData = incomingFishData;
        if (numberOfFish <= 0)
        {
            return;
        }

        var degradingHealth = 0;
        if (foodAmount <= 0f)
        {
            degradingHealth++;
        }

        if (cleanAmount <= 0.25f)
        {
            degradingHealth++;
        }

        if (parent.AmbientTemperature is > 55f or < 1f)
        {
            degradingHealth++;
        }

        var newIndex = 0;
        var changedFishData = new List<string>();
        if (newFishData.Count > 0)
        {
            foreach (var value in newFishData)
            {
                var died = false;
                NumValuePart(value, 0);
                var defVal = StringValuePart(value, 1);
                var health = NumValuePart(value, 2);
                var age = NumValuePart(value, 3);
                var action = NumValuePart(value, 4);
                var ageDegradingHealth = 0;
                if (action != 1)
                {
                    age += Ticks;
                    if (age + (int)RandomFloat(-450000f, 450000f) > oldFishAge)
                    {
                        ageDegradingHealth = 1;
                    }

                    var poorhealth = degradingHealth + ageDegradingHealth;
                    if (poorhealth > 0)
                    {
                        health -= (int)Math.Max(1f,
                            poorhealth * Mathf.Lerp(0.5f, 1f,
                                Math.Max(1f, Math.Min(oldFishAge, age) / (float)oldFishAge)));
                        if (!Controller.Settings.ImmortalFish && health <= 0)
                        {
                            died = true;
                        }
                    }
                    else if (health < 100)
                    {
                        health += (int)Math.Max(1f,
                            Mathf.Lerp(1f, 0.5f, Math.Max(1f, Math.Min(oldFishAge, age) / (float)oldFishAge)));
                        if (health > 100)
                        {
                            health = 100;
                        }
                    }
                }

                if (!died)
                {
                    newIndex++;
                    var newValue = CreateValuePart(newIndex, defVal, health, age, action);
                    changedFishData.Add(newValue);
                }
                else
                {
                    if (parent.Spawned)
                    {
                        var thingWithComps = parent;
                        if (thingWithComps?.Map != null && parent.Map.ParentFaction == Faction.OfPlayerSilentFail)
                        {
                            if (Controller.Settings.DoDeathMsgs)
                            {
                                Messages.Message("Aquarium.FishDied".Translate(), parent,
                                    MessageTypeDefOf.NegativeEvent, false);
                            }

                            AQUtility.DoSpawnTropicalFishMeat(parent, age);
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

        if (incomingFishData.Count != changedFishData.Count)
        {
            GenerateBeauty(changedFishData);
        }

        newFishData = changedFishData;
    }

    internal static string CreateValuePart(int newIndex, string defVal, int health, int age, int action)
    {
        return $"{newIndex};{defVal};{health};{age};{action}";
    }

    internal static int NumValuePart(string value, int pos)
    {
        char[] divider =
        [
            ';'
        ];
        var segments = value.Split(divider);
        try
        {
            return int.Parse(segments[pos]);
        }
        catch (FormatException)
        {
            Log.Message($"Unable to parse Segment: '{segments[pos]}' : {pos}");
        }

        return 0;
    }

    internal static string StringValuePart(string value, int pos)
    {
        char[] divider =
        [
            ';'
        ];
        return value.Split(divider)[pos];
    }

    private void startMixSustainer()
    {
        var info = SoundInfo.InMap(parent, MaintenanceType.PerTick);
        mixSustainer = DefsCacher.AQSoundDef.TrySpawnSustainer(info);
    }

    private void doTankSound()
    {
        if (mixSustainer == null || mixSustainer.Ended)
        {
            startMixSustainer();
            return;
        }

        mixSustainer.Maintain();
    }

    public override IEnumerable<Gizmo> CompGetGizmosExtra()
    {
        string graphicPath;
        if (Props.maxFish > 1)
        {
            selectedSand = !string.IsNullOrEmpty(selectedSandDefName)
                ? DefsCacher.AQSandDefs.IndexOf((from sandDef in DefsCacher.AQSandDefs
                    where sandDef.defName == selectedSandDefName
                    select sandDef).First())
                : -1;
            selectedDecoration = !string.IsNullOrEmpty(selectedDecorationDefName)
                ? DefsCacher.AQDecorationDefs.IndexOf((from decorationDef in DefsCacher.AQDecorationDefs
                    where decorationDef.defName == selectedDecorationDefName
                    select decorationDef).First())
                : -1;

            graphicPath = "Things/Fish/UI/Sand_Icon";
            var defaultLabel = selectedSand >= 0
                ? $"{("Aquarium." + DefsCacher.AQSandDefs[selectedSand].label).Translate()}\n{"Aquarium.Sand".Translate()}"
                : (string)"Aquarium.Sand".Translate();
            yield return new Command_Action
            {
                defaultLabel = defaultLabel,
                icon = ContentFinder<Texture2D>.Get(graphicPath),
                defaultDesc = "Aquarium.SandDesc".Translate(),
                action = selectSand
            };
            graphicPath = "Things/Fish/UI/Decoration_Icon";
            defaultLabel = selectedDecoration >= 0
                ? $"{("Aquarium." + DefsCacher.AQDecorationDefs[selectedDecoration].label).Translate()}\n{"Aquarium.Decorations".Translate()}"
                : "Aquarium.Decorations".Translate();
            yield return new Command_Action
            {
                defaultLabel = defaultLabel,
                icon = ContentFinder<Texture2D>.Get(graphicPath),
                defaultDesc = "Aquarium.DecorationsDesc".Translate(),
                action = selectDecorations
            };
        }

        for (var i = 1; i <= Props.maxFish; i++)
        {
            ThingDef fishDef = null;
            var fishHealth = 0;
            var fishAge = 0;
            var fishAction = 0;
            string fishstring = null;
            if (fishData.Count > 0)
            {
                foreach (var value in fishData)
                {
                    if (NumValuePart(value, 0) != i)
                    {
                        continue;
                    }

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

            var numLabel = $"{i}: ";
            string fishLabel = numLabel + "Aquarium.NoFish".Translate();
            graphicPath = "Things/Fish/UI/NoFish";
            string fishDesc = "Aquarium.FishSelection".Translate();
            if (fishDef != null)
            {
                var fishGfx = fishstring.Replace("AQFishInBag", "");
                if (fishstring == "AQRandomFish")
                {
                    graphicPath = keepFilling ? "Things/Fish/UI/RandomEternal_Icon" : "Things/Fish/UI/Random_Icon";
                }
                else
                {
                    graphicPath = $"Things/Fish/Fish{wordsToNumbers(fishGfx)}";
                }

                switch (fishAction)
                {
                    case 1:
                        fishLabel = numLabel + "Aquarium.Adding".Translate();
                        break;
                    case 2:
                        fishLabel = numLabel + "Aquarium.Removing".Translate();
                        break;
                    default:
                        fishLabel = numLabel + fishDef.LabelCap + "\nH: " + (fishHealth / 100f).ToStringPercent() +
                                    ", " + fishAge.ToString() + " " + "Aquarium.days".Translate();
                        break;
                }
            }

            var fishIcon = ContentFinder<Texture2D>.Get(graphicPath);
            yield return new Command_Action
            {
                defaultLabel = fishLabel,
                icon = fishIcon,
                defaultDesc = fishDesc,
                action = delegate { selectFish(fishDef, numLabel); }
            };
            if (fishDef == null || fishstring == "AQRandomFish" && keepFilling)
            {
                break;
            }
        }

        if (Prefs.DevMode)
        {
            yield return new Command_Toggle
            {
                icon = ContentFinder<Texture2D>.Get(debugTexPath),
                defaultLabel = "Debug Mode",
                defaultDesc = "Send debug messages to Log",
                isActive = () => fishydebug,
                toggleAction = delegate { toggleDebug(fishydebug); }
            };
        }
    }

    private void toggleDebug(bool flag)
    {
        fishydebug = !flag;
    }

    private void selectFish(ThingDef aFishDef, string fishLabel)
    {
        var selIndex = Convert.ToInt32(fishLabel.Split(':')[0]);
        var list = new List<FloatMenuOption>();
        string text = "Aquarium.DoNothing".Translate();
        list.Add(new FloatMenuOption(text, delegate { fishSelection(null, 0, ActionType.Nothing); },
            MenuOptionPriority.Default, null, null, 29f));
        if (aFishDef != null)
        {
            text = "Aquarium.RemoveFish".Translate();
            list.Add(new FloatMenuOption(text, delegate
                {
                    fishSelection(aFishDef, selIndex, ActionType.Remove);
                    keepFilling = false;
                },
                MenuOptionPriority.Default, null, null, 29f));
        }
        else
        {
            list.Add(new FloatMenuOption("Aquarium.RandomFish".Translate(),
                delegate
                {
                    fishSelection(DefsCacher.AQRandomFishDef, selIndex, ActionType.Add);
                    keepFilling = false;
                },
                MenuOptionPriority.Default,
                null, null, 29f));
            list.Add(new FloatMenuOption("Aquarium.RandomFishUntilFull".Translate(),
                delegate
                {
                    fishSelection(DefsCacher.AQRandomFishDef, selIndex, ActionType.Add);
                    keepFilling = true;
                },
                MenuOptionPriority.Default,
                null, null, 29f));
            if (ReachableDefs.Count == 0)
            {
                list.Add(new FloatMenuOption("Aquarium.NoReachableFish".Translate(), null,
                    MenuOptionPriority.Default, null, null, 29f));
            }

            foreach (var defName in ReachableDefs)
            {
                var potfishDef = ThingDef.Named(defName);
                text = "Aquarium.AddFish".Translate() + " " + potfishDef.LabelCap;
                list.Add(new FloatMenuOption(text,
                    delegate
                    {
                        fishSelection(potfishDef, selIndex, ActionType.Add);
                        keepFilling = false;
                    }, MenuOptionPriority.Default,
                    null, null, 29f));
            }
        }

        Find.WindowStack.Add(new FloatMenu(list));
    }

    private void selectSand()
    {
        var list = new List<FloatMenuOption>();
        for (var i = DefsCacher.AQSandDefs.Count - 1; i >= 0; i--)
        {
            var textToAdd = $"Aquarium.{DefsCacher.AQSandDefs[i].label}".Translate();
            var sandDef = DefsCacher.AQSandDefs[i];
            list.Add(new FloatMenuOption(textToAdd, delegate { sandSelection(sandDef.defName); },
                MenuOptionPriority.Default, null, null, 29f));
        }

        var sortedList = list.OrderBy(option => option.Label).ToList();
        sortedList.Insert(0,
            new FloatMenuOption("Aquarium.Nothing".Translate(), delegate { sandSelection(string.Empty); },
                MenuOptionPriority.Default, null, null, 29f));
        Find.WindowStack.Add(new FloatMenu(sortedList));
    }

    private void selectDecorations()
    {
        var list = new List<FloatMenuOption>();
        foreach (var thingDef in DefsCacher.AQDecorationDefs)
        {
            var textToAdd = $"Aquarium.{thingDef.label}".Translate();
            var decorationDef = thingDef;
            list.Add(new FloatMenuOption(textToAdd, delegate { decorationSelection(decorationDef.defName); },
                MenuOptionPriority.Default, null, null, 29f));
        }

        var sortedList = list.OrderBy(option => option.Label).ToList();
        sortedList.Insert(0,
            new FloatMenuOption("Aquarium.Nothing".Translate(), delegate { decorationSelection(string.Empty); },
                MenuOptionPriority.Default, null, null, 29f));
        Find.WindowStack.Add(new FloatMenu(sortedList));
    }

    private void fishSelection(ThingDef selfishDef, int fishIndex, ActionType actionType)
    {
        switch (actionType)
        {
            case ActionType.Add:
                var newFishData = new List<string>();
                var newIndex = 0;
                if (fishData.Count > 0)
                {
                    foreach (var value3 in fishData)
                    {
                        NumValuePart(value3, 0);
                        var prevfishdefName = StringValuePart(value3, 1);
                        var prevHealth = NumValuePart(value3, 2);
                        var prevAge = NumValuePart(value3, 3);
                        var prevAction = NumValuePart(value3, 4);
                        newIndex++;
                        var newValue = CreateValuePart(newIndex, prevfishdefName, prevHealth, prevAge, prevAction);
                        newFishData.Add(newValue);
                    }
                }

                newIndex++;
                var addValue = CreateValuePart(newIndex, selfishDef.defName, 0, 0, 1);
                newFishData.Add(addValue);
                fishData = newFishData;
                break;
            case ActionType.Remove:
                var newFishData2 = new List<string>();
                var cancel = false;
                if (fishData.Count > 0)
                {
                    foreach (var value in fishData)
                    {
                        var row = NumValuePart(value, 0);
                        var prevfishdefName2 = StringValuePart(value, 1);
                        var prevHealth2 = NumValuePart(value, 2);
                        var prevAge2 = NumValuePart(value, 3);
                        var prevAction2 = NumValuePart(value, 4);
                        if (row == fishIndex)
                        {
                            switch (prevAction2)
                            {
                                case 0:
                                {
                                    var newValue2 = CreateValuePart(row, prevfishdefName2, prevHealth2, prevAge2, 2);
                                    newFishData2.Add(newValue2);
                                    break;
                                }
                                case 1:
                                {
                                    var newValue3 = CreateValuePart(row, prevfishdefName2, prevHealth2, prevAge2, 3);
                                    newFishData2.Add(newValue3);
                                    cancel = true;
                                    break;
                                }
                                default:
                                    newFishData2.Add(value);
                                    break;
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
                    var cancelFishData = new List<string>();
                    var newIndex2 = 0;
                    if (fishData.Count > 0)
                    {
                        foreach (var value2 in fishData)
                        {
                            newIndex2++;
                            var prevAction3 = NumValuePart(value2, 4);
                            if (prevAction3 != 3)
                            {
                                var newValue4 = CreateValuePart(newIndex2, StringValuePart(value2, 1),
                                    NumValuePart(value2, 2), NumValuePart(value2, 3), prevAction3);
                                cancelFishData.Add(newValue4);
                            }
                            else
                            {
                                newIndex2--;
                            }
                        }
                    }

                    fishData = cancelFishData;
                }

                break;
        }
    }

    private void sandSelection(string defName)
    {
        selectedSandDefName = defName;
    }

    private void decorationSelection(string defName)
    {
        selectedDecorationDefName = defName;
    }

    public override string CompInspectStringExtra()
    {
        return "Aquarium.TankInfo".Translate(cleanPct.ToStringPercent(), foodPct.ToStringPercent(), currentBeauty,
            Math.Max(Props.maxFish - fishData.Count, 0));
    }


    private static int wordsToNumbers(string word)
    {
        for (var i = 0; i < 500; i++)
        {
            if (numberToWords(i) == word.ToLower())
            {
                return i;
            }
        }

        return 1;
    }

    private static string numberToWords(int number)
    {
        switch (number)
        {
            case 0:
                return "zero";
            case < 0:
                return $"minus{numberToWords(Math.Abs(number))}";
        }

        var words = "";

        if (number / 1000000 > 0)
        {
            words += $"{numberToWords(number / 1000000)}million";
            number %= 1000000;
        }

        if (number / 1000 > 0)
        {
            words += $"{numberToWords(number / 1000)}thousand";
            number %= 1000;
        }

        if (number / 100 > 0)
        {
            words += $"{numberToWords(number / 100)}hundred";
            number %= 100;
        }

        if (number <= 0)
        {
            return words;
        }

        var unitsMap = new[]
        {
            "zero", "one", "two", "three", "four", "five", "six", "seven", "eight", "nine", "ten", "eleven",
            "twelve", "thirteen", "fourteen", "fifteen", "sixteen", "seventeen", "eighteen", "nineteen"
        };
        var tensMap = new[]
            { "zero", "ten", "twenty", "thirty", "forty", "fifty", "sixty", "seventy", "eighty", "ninety" };

        if (number < 20)
        {
            words += unitsMap[number];
        }
        else
        {
            words += tensMap[number / 10];
            if (number % 10 > 0)
            {
                words += unitsMap[number % 10];
            }
        }

        return words;
    }


    private bool isPowered()
    {
        if (!parent.Spawned)
        {
            return false;
        }

        if (parent.IsBrokenDown())
        {
            return false;
        }

        var cpt = parent.TryGetComp<CompPowerTrader>();
        return cpt is { PowerOn: true };
    }

    private void tankBreeding()
    {
        if (!Controller.Settings.AllowBreed || numFish <= 1 || numFish >= Props.maxFish || fishData.Count <= 0)
        {
            return;
        }

        var breeders = new List<string>();
        var potentials = new List<string>();
        var newFishList = new List<string>();
        var newIndex = 0;
        var birth = false;
        foreach (var fish in fishData)
        {
            newIndex++;
            if (NumValuePart(fish, 3) >= oldFishAge / 2 && NumValuePart(fish, 4) == 0)
            {
                var fishDef = StringValuePart(fish, 1);
                if (breeders.Contains(fishDef))
                {
                    potentials.AddDistinct(fishDef);
                }

                breeders.Add(fishDef);
            }

            newFishList.Add(fish);
        }

        if (potentials.Count > 0 && RandomFloat(1f, 100f) <= Controller.Settings.BreedChance)
        {
            newIndex++;
            var babyFishDef = potentials.RandomElement();
            var newValue = CreateValuePart(newIndex, babyFishDef, 75, 0, 0);
            newFishList.Add(newValue);
            birth = true;
        }

        if (!birth)
        {
            return;
        }

        fishData = newFishList;
        GenerateBeauty(fishData);
        numFish++;
    }

    public override void PostSpawnSetup(bool respawningAfterLoad)
    {
        base.PostSpawnSetup(respawningAfterLoad);
        GenerateBeauty(fishData);
    }

    public void CheckToContinue()
    {
        if (!keepFilling)
        {
            return;
        }

        if (fishData.Count >= Props.maxFish)
        {
            keepFilling = false;
            return;
        }

        fishSelection(DefsCacher.AQRandomFishDef, fishData.Count, ActionType.Add);
    }

    private float foodPercentPerMeal()
    {
        if (fishData.Count == 0)
        {
            return 0;
        }

        return 2f / fishData.Count;
    }

    public bool NeedFood()
    {
        if (fishData.Count == 0 || foodPct == 1f)
        {
            return false;
        }

        return foodPct < 0.1f || foodPct + foodPercentPerMeal() < 1f;
    }

    public void AddFood()
    {
        foodPct = Math.Min(1f, foodPct + foodPercentPerMeal());
    }

    private enum ActionType
    {
        Nothing,
        Add,
        Remove
    }
}