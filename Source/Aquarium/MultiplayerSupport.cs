using System.Reflection;
using HarmonyLib;
using Multiplayer.API;
using Verse;

namespace Aquarium
{
    // Token: 0x02000002 RID: 2
    [StaticConstructorOnStartup]
    internal static class MultiplayerSupport
    {
        // Token: 0x04000001 RID: 1
        private static readonly Harmony harmony = new("rimworld.pelador.aquarium.multiplayersupport");

        // Token: 0x06000001 RID: 1 RVA: 0x00002050 File Offset: 0x00000250
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
            {
                AccessTools.Method(typeof(CompAquarium), "RandomFloat")
            };
            foreach (var methodInfo in array)
            {
                FixRNG(methodInfo);
            }
        }

        // Token: 0x06000002 RID: 2 RVA: 0x000020D6 File Offset: 0x000002D6
        private static void FixRNG(MethodInfo method)
        {
            harmony.Patch(method, new HarmonyMethod(typeof(MultiplayerSupport), "FixRNGPre"),
                new HarmonyMethod(typeof(MultiplayerSupport), "FixRNGPos"));
        }

        // Token: 0x06000003 RID: 3 RVA: 0x00002110 File Offset: 0x00000310
        private static void FixRNGPre()
        {
            Rand.PushState(Find.TickManager.TicksAbs);
        }

        // Token: 0x06000004 RID: 4 RVA: 0x00002121 File Offset: 0x00000321
        private static void FixRNGPos()
        {
            Rand.PopState();
        }
    }
}