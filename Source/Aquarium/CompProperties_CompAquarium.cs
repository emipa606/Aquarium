using Verse;

namespace Aquarium;

public class CompProperties_CompAquarium : CompProperties
{
    public float heatPerSecond = 12f;

    public int maxFish = 9;

    public bool powerNeeded = true;

    public float targetTemp = 24f;

    public CompProperties_CompAquarium()
    {
        compClass = typeof(CompAquarium);
    }
}