using Verse;

namespace StoryFramework
{
    public class CustomObjectiveSettings
    {
        public ColorInt progressBarColor = new ColorInt(40, 170, 225);
        //This label describes the objective in the objective's tab (below the actual label)
        public string shortLabel;
        //This label appears above the target box, explaining what they are used for
        public string targetLabel;
        public bool usesStation = false;
    }
}
