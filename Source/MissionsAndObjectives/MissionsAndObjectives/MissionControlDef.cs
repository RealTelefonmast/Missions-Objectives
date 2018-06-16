using Verse;
using RimWorld;
using UnityEngine;

namespace MissionsAndObjectives
{
    public class MissionControlDef : Def
    {
        public string boxActive = "UI/Icons/Active";

        public string boxFinished = "UI/Icons/Finished";

        public string boxFailed = "UI/Icons/Failed";

        public ColorInt color = new ColorInt(45, 45, 45);

        public ColorInt borderColor = new ColorInt(255, 255, 255);

        public static MissionControlDef Named(string defName)
        {
            return DefDatabase<MissionControlDef>.GetNamed(defName, true);
        }
    }

    [DefOf]
    public static class MCD
    {
        public static MissionControlDef MainMissionControlDef = MissionControlDef.Named("MainMissionControlDef");

        public static JobDef DoMissionObjective = DefDatabase<JobDef>.GetNamed("DoMissionObjective", true);
    }
}
