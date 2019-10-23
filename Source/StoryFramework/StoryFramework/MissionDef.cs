using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StoryFramework
{
    public class MissionDef : StoryObjectDef
    {
        public Type type = typeof(Mission);
        public List<ObjectiveDef> objectives;
    }
}
