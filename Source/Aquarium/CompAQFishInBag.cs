using System;
using RimWorld;
using Verse;

namespace Aquarium;

public class CompAQFishInBag : ThingComp
{
    private const int bagTicks = 300;

    public int age;

    public int fishhealth = 100;

    public int ticksInBagRemain = 180000;

    private CompProperties_AQFishInBag Props => (CompProperties_AQFishInBag)props;

    public override void PostExposeData()
    {
        base.PostExposeData();
        Scribe_Values.Look(ref age, "age");
        Scribe_Values.Look(ref fishhealth, "fishhealth", 100);
        Scribe_Values.Look(ref ticksInBagRemain, "ticksInBagRemain", 180000);
    }

    public override void CompTick()
    {
        base.CompTick();
        if (!parent.IsHashIntervalTick(bagTicks))
        {
            return;
        }

        var died = false;
        age += bagTicks;
        if (!Controller.Settings.ImmortalFish &&
            Math.Min(CompAquarium.oldFishAge, age + (int)CompAquarium.RandomFloat(-450000f, 450000f)) >
            CompAquarium.oldFishAge)
        {
            fishhealth--;
        }

        if (parent.Spawned)
        {
            ticksInBagRemain -= bagTicks;
            if (ticksInBagRemain <= 0)
            {
                fishhealth--;
            }

            if (parent.AmbientTemperature is < 1f or > 55f)
            {
                fishhealth--;
            }
        }

        if (fishhealth <= 0)
        {
            died = true;
        }

        if (!died)
        {
            return;
        }

        if (parent.Spawned)
        {
            var thingWithComps = parent;
            if (thingWithComps?.Map != null && parent.Map.ParentFaction == Faction.OfPlayerSilentFail)
            {
                if (Controller.Settings.DoDeathMsgs)
                {
                    Messages.Message("Aquarium.FishDied".Translate(), parent, MessageTypeDefOf.NegativeEvent,
                        false);
                }

                AQUtility.DoSpawnTropicalFishMeat(parent, age);
            }
        }

        parent.Destroy();
    }


    public override string CompInspectStringExtra()
    {
        var ageDays = age / 60000f;
        var fishhealthpct = fishhealth / 100f;
        var hoursRemain = ticksInBagRemain / 2500f;
        return "Aquarium.BagInfo".Translate(ageDays.ToString("F2"), fishhealthpct.ToStringPercent(),
            hoursRemain.ToString("F2"));
    }
}