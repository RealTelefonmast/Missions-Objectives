using System.Collections.Generic;
using Verse;

namespace StoryFramework
{
    public class StoryThingDefExtension : DefModExtension
    {
        public bool OnStart = false;
        public bool Any = false;
        public List<ObjectiveDef> objectiveRequisites = new List<ObjectiveDef>();
    }
}
