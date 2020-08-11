using System;
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
		// Token: 0x06000001 RID: 1 RVA: 0x00002050 File Offset: 0x00000250
		static MultiplayerSupport()
		{
			if (!MP.enabled)
			{
				return;
			}
			MP.RegisterSyncMethod(typeof(CompAquarium), "FishSelection", null);
			MP.RegisterSyncMethod(typeof(CompAquarium), "ToggleDebug", null);
			MethodInfo[] array = new MethodInfo[]
			{
				AccessTools.Method(typeof(CompAquarium), "RandomFloat", null, null)
			};
			for (int i = 0; i < array.Length; i++)
			{
				MultiplayerSupport.FixRNG(array[i]);
			}
		}

		// Token: 0x06000002 RID: 2 RVA: 0x000020D6 File Offset: 0x000002D6
		private static void FixRNG(MethodInfo method)
		{
			MultiplayerSupport.harmony.Patch(method, new HarmonyMethod(typeof(MultiplayerSupport), "FixRNGPre", null), new HarmonyMethod(typeof(MultiplayerSupport), "FixRNGPos", null), null, null);
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

		// Token: 0x04000001 RID: 1
		private static Harmony harmony = new Harmony("rimworld.pelador.aquarium.multiplayersupport");
	}
}
