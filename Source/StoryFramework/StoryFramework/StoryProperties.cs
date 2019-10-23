using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StoryFramework
{
    public class StoryProperties
    {
        public List<ObjectiveDef> startsObjectves = new List<ObjectiveDef>();
        public List<MissionDef> startsMissions = new List<MissionDef>();
        public List<ObjectiveDef> cancelsObjectives = new List<ObjectiveDef>();
        public List<MissionDef> cancelsMissions = new List<MissionDef>();
    }
}
