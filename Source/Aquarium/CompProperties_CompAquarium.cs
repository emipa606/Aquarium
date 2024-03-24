using Verse;

namespace Aquarium;

public class CompProperties_CompAquarium : CompProperties
{
    public readonly float heatPerSecond = 12f;

    public readonly int maxFish = 9;

    public readonly bool powerNeeded = true;

    public readonly float targetTemp = 24f;

    public CompProperties_CompAquarium()
    {
        compClass = typeof(CompAquarium);
    }
}