using RimWorld;
using Verse;
using Verse.AI;

namespace Aquarium;

public class Toils_AQClean
{
    public static Toil FinalizeCleaning(TargetIndex cleanable)
    {
        var toil = new Toil();
        toil.initAction = delegate
        {
            var AQComp = toil.actor.CurJob.GetTarget(cleanable).Thing.TryGetComp<CompAquarium>();
            if (AQComp == null)
            {
                return;
            }

            AQComp.cleanPct = 1f;
            toil.actor.skills.Learn(SkillDefOf.Animals, 50f);
        };
        toil.defaultCompleteMode = ToilCompleteMode.Instant;
        return toil;
    }
}