using System;
using Verse;

namespace Aquarium
{
	// Token: 0x02000007 RID: 7
	public class CompProperties_CompAquarium : CompProperties
	{
		// Token: 0x06000037 RID: 55 RVA: 0x00003F07 File Offset: 0x00002107
		public CompProperties_CompAquarium()
		{
			this.compClass = typeof(CompAquarium);
		}

		// Token: 0x04000014 RID: 20
		public int maxFish = 9;

		// Token: 0x04000015 RID: 21
		public bool powerNeeded = true;

		// Token: 0x04000016 RID: 22
		public float targetTemp = 24f;

		// Token: 0x04000017 RID: 23
		public float heatPerSecond = 12f;
	}
}
