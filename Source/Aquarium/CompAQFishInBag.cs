using System;
using RimWorld;
using Verse;

namespace Aquarium
{
	// Token: 0x02000004 RID: 4
	public class CompAQFishInBag : ThingComp
	{
		// Token: 0x17000001 RID: 1
		// (get) Token: 0x06000012 RID: 18 RVA: 0x0000296C File Offset: 0x00000B6C
		private CompProperties_AQFishInBag Props
		{
			get
			{
				return (CompProperties_AQFishInBag)this.props;
			}
		}

		// Token: 0x06000013 RID: 19 RVA: 0x0000297C File Offset: 0x00000B7C
		public override void PostExposeData()
		{
			base.PostExposeData();
			Scribe_Values.Look<int>(ref this.age, "age", 0, false);
			Scribe_Values.Look<int>(ref this.fishhealth, "fishhealth", 100, false);
			Scribe_Values.Look<int>(ref this.ticksInBagRemain, "ticksInBagRemain", 180000, false);
		}

		// Token: 0x06000014 RID: 20 RVA: 0x000029CC File Offset: 0x00000BCC
		public override void CompTick()
		{
			base.CompTick();
			if (this.parent.IsHashIntervalTick(300))
			{
				bool died = false;
				this.age += 300;
				if (this.age + (int)CompAquarium.RandomFloat(-450000f, 450000f) > CompAquarium.oldFishAge)
				{
					this.fishhealth--;
				}
				if (this.parent.Spawned)
				{
					this.ticksInBagRemain -= 300;
					if (this.ticksInBagRemain <= 0)
					{
						this.fishhealth--;
					}
					if (this.parent.AmbientTemperature < 1f || this.parent.AmbientTemperature > 55f)
					{
						this.fishhealth--;
					}
				}
				if (this.fishhealth <= 0)
				{
					died = true;
				}
				if (died)
				{
					if (this.parent.Spawned)
					{
						ThingWithComps parent = this.parent;
						if (((parent != null) ? parent.Map : null) != null && this.parent.Map.ParentFaction == Faction.OfPlayerSilentFail)
						{
							if (Controller.Settings.DoDeathMsgs)
							{
								Messages.Message("Aquarium.FishDied".Translate(), this.parent, MessageTypeDefOf.NegativeEvent, false);
							}
							AQUtility.DoSpawnTropicalFishMeat(this.parent, this.age);
						}
					}
					this.parent.Destroy(DestroyMode.Vanish);
				}
			}
		}

		// Token: 0x06000015 RID: 21 RVA: 0x00002B33 File Offset: 0x00000D33
		public override void CompTickRare()
		{
			base.CompTickRare();
		}

		// Token: 0x06000016 RID: 22 RVA: 0x00002B3C File Offset: 0x00000D3C
		public override string CompInspectStringExtra()
		{
			float ageDays = (float)this.age / 60000f;
			float fishhealthpct = (float)this.fishhealth / 100f;
			float hoursRemain = (float)this.ticksInBagRemain / 2500f;
			return "Aquarium.BagInfo".Translate(ageDays.ToString("F2"), fishhealthpct.ToStringPercent(), hoursRemain.ToString("F2"));
		}

		// Token: 0x04000004 RID: 4
		public const int bagTicks = 300;

		// Token: 0x04000005 RID: 5
		public int age;

		// Token: 0x04000006 RID: 6
		public int fishhealth = 100;

		// Token: 0x04000007 RID: 7
		public int ticksInBagRemain = 180000;
	}
}
