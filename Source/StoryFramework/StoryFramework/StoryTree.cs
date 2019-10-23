using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StoryFramework
{
    public class StoryTree
    {
        public Dictionary<string, List<ObjectiveDef>> ObjectivePaths = new Dictionary<string, List<ObjectiveDef>>();

        public void GeneratePathsFor(MissionDef def)
        {

        }
    }

    public class MissionNode
    {
        public MissionDef previous;
        public MissionDef current;
        public MissionDef[] next;

        public ObjectiveNode objectives;
    }

    public class ObjectiveNode
    {
        public ObjectiveDef previous;
        public ObjectiveDef current;
        public ObjectiveDef[] next;
    }
}
