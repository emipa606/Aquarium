using UnityEngine;
using Verse;

namespace Aquarium
{
    // Token: 0x02000008 RID: 8
    public class Controller : Mod
    {
        // Token: 0x04000018 RID: 24
        public static Settings Settings;

        // Token: 0x0600003A RID: 58 RVA: 0x00003F61 File Offset: 0x00002161
        public Controller(ModContentPack content) : base(content)
        {
            Settings = GetSettings<Settings>();
        }

        // Token: 0x06000038 RID: 56 RVA: 0x00003F43 File Offset: 0x00002143
        public override string SettingsCategory()
        {
            return "Aquarium.Name".Translate();
        }

        // Token: 0x06000039 RID: 57 RVA: 0x00003F54 File Offset: 0x00002154
        public override void DoSettingsWindowContents(Rect canvas)
        {
            Settings.DoWindowContents(canvas);
        }
    }
}