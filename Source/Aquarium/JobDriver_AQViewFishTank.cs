using RimWorld;
using Verse;
using Verse.AI;

namespace Aquarium;

public class JobDriver_AQViewFishTank : JobDriver_VisitJoyThing
{
    private Thing FishyThing => job?.GetTarget(TargetIndex.A).Thing;

    protected override void WaitTickAction(int delta)
    {
        pawn.GainComfortFromCellIfPossible(delta);
        if (FishyThing == null)
        {
            return;
        }

        var num = AQUtility.GetJoyGainFactor(
            FishyThing.GetStatValue(StatDefOf.Beauty) / FishyThing.def.GetStatValueAbstract(StatDefOf.Beauty),
            FishyThing);
        var extraJoyGainFactor = num > 0f ? num : 0f;
        JoyUtility.JoyTickCheckEnd(pawn, delta, JoyTickFullJoyAction.EndJob, extraJoyGainFactor, (Building)FishyThing);
        AQUtility.ApplyMoodBoostAndInspire(pawn, FishyThing, delta);
    }
}