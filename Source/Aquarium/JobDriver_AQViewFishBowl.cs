using RimWorld;
using Verse;
using Verse.AI;

namespace Aquarium;

public class JobDriver_AQViewFishBowl : JobDriver_VisitJoyThing
{
    private Thing FishyThing => job.GetTarget(TargetIndex.A).Thing;

    protected override void WaitTickAction()
    {
        var num = AQUtility.GetJoyGainFactor(
            FishyThing.GetStatValue(StatDefOf.Beauty) / FishyThing.def.GetStatValueAbstract(StatDefOf.Beauty),
            FishyThing);
        var extraJoyGainFactor = num > 0f ? num : 0f;
        pawn.GainComfortFromCellIfPossible();
        JoyUtility.JoyTickCheckEnd(pawn, JoyTickFullJoyAction.EndJob, extraJoyGainFactor, (Building)FishyThing);
        AQUtility.ApplyMoodBoostAndInspire(pawn, FishyThing);
    }
}