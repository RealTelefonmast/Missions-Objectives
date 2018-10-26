using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;

namespace StoryFramework
{
    public class ThingSettings
    {
        public ThingDef stuff;
        public QualityCategory? minQuality;
        public List<string> tradeTags       = new List<string>(),
                            weaponTags      = new List<string>(),
                            techHediffsTags = new List<string>();
        public int minAmount = 1;
    }
}
