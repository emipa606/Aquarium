using PipeSystem;
using Verse;

namespace Aquarium;

[StaticConstructorOnStartup]
public static class Comp_Aquarium_VNPE
{
    public static readonly bool VNPELoaded = ModsConfig.IsActive("VanillaExpanded.VNutrientE");

    public static void VNPE_Check(Building aquarium)
    {
        if (!aquarium.IsHashIntervalTick(GenTicks.TickLongInterval))
        {
            return;
        }

        var compResource = aquarium.GetComp<CompResource>();
        var compAquarium = aquarium.GetComp<CompAquarium>();

        if (compAquarium == null || compResource is not { PipeNet: { } net })
        {
            return;
        }

        var stored = net.Stored;
        while (stored > 0 && compAquarium.NeedFood())
        {
            net.DrawAmongStorage(1, net.storages);
            stored--;
            compAquarium.AddFood();
        }
    }
}