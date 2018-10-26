using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using UnityEngine;

namespace StoryFramework
{
    public class CustomObjectiveSettings
    {
        public ColorInt progressBarColor;
        //This label describes the objective in the objective's tab (below the actual label)
        public string shortLabel;
        //This label appears above the target box, explaining what they are used for
        public string targetLabel;
        public bool usesStation = false;
    }
}
