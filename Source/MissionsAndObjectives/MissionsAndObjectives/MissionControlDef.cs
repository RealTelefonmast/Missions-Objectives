using Verse;
using RimWorld;
using UnityEngine;

namespace MissionsAndObjectives
{
    public class MissionControlDef : Def
    {
        public string bannerTexture;

        public string boxActive;

        public string boxFinished;

        public string boxFailed;

        public float workFloat;

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
