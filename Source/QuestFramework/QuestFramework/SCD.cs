using RimWorld;
using Verse;

namespace StoryFramework
{
    [DefOf]
    public static class SCD
    {
        public static StoryControlDef MainStoryControlDef = StoryControlDef.Named("MainStoryControlDef");

        public static JobDef DoMissionObjective = DefDatabase<JobDef>.GetNamed("DoMissionObjective", true);
    }
}
