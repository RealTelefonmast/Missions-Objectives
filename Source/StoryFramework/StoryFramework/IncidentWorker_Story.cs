using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace StoryFramework
{
    public class IncidentWorker_Story : IncidentWorker
    {
        public StoryIncidentDef Def => base.def as StoryIncidentDef;
    }
}
