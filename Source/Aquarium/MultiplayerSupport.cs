using System.Reflection;
using HarmonyLib;
using Multiplayer.API;
using Verse;

namespace Aquarium;

[StaticConstructorOnStartup]
internal static class MultiplayerSupport
{
    private static readonly Harmony harmony = new Harmony("rimworld.pelador.aquarium.multiplayersupport");

    static MultiplayerSupport()
    {
        if (!MP.enabled)
        {
            return;
        }

        MP.RegisterSyncMethod(typeof(CompAquarium), "FishSelection");
        MP.RegisterSyncMethod(typeof(CompAquarium), "SandSelection");
        MP.RegisterSyncMethod(typeof(CompAquarium), "DecorationSelection");
        MP.RegisterSyncMethod(typeof(CompAquarium), "ToggleDebug");
        MethodInfo[] array =
        [
            AccessTools.Method(typeof(CompAquarium), "RandomFloat")
        ];
        foreach (var methodInfo in array)
        {
            FixRNG(methodInfo);
        }
    }

    private static void FixRNG(MethodInfo method)
    {
        harmony.Patch(method, new HarmonyMethod(typeof(MultiplayerSupport), "FixRNGPre"),
            new HarmonyMethod(typeof(MultiplayerSupport), "FixRNGPos"));
    }

    private static void FixRNGPre()
    {
        Rand.PushState(Find.TickManager.TicksAbs);
    }

    private static void FixRNGPos()
    {
        Rand.PopState();
    }
}