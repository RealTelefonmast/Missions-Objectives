using Verse;
using RimWorld;
using UnityEngine;

namespace StoryFramework
{
    public class StoryControlDef : Def
    {
        public string activeIconPath = "UI/Icons/Active";
        public string finishedIconPath = "UI/Icons/Finished";
        public string failedIconPath = "UI/Icons/Failed";
        public string repeatableIconPath = "UI/Icons/Repeat";
        public string backGroundPath = "UI/MissionBanner";
        public string objectiveMarkerPath = "UI/ObjectiveMarker";
        public ColorInt color = new ColorInt(45, 45, 45);
        public ColorInt borderColor = new ColorInt(255, 255, 255);

        public static StoryControlDef Named(string defName)
        {
            return DefDatabase<StoryControlDef>.GetNamed(defName, true);
        }
    }
}
