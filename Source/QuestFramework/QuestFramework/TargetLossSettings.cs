using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace StoryFramework
{
    public class TargetLossSettings
    {
        public List<ThingValue> targets = new List<ThingValue>();
        public bool any = false;
        public int minColonistsToLose = 0;
    }
}
