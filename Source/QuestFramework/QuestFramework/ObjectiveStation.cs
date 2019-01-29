using System.Collections.Generic;
using Verse;

namespace StoryFramework
{
    public class ObjectiveStation : IExposable
    {
        public Thing station;
        public List<ObjectiveDef> objectives = new List<ObjectiveDef>();
        public bool active;

        public ObjectiveStation()
        {
        }

        public ObjectiveStation(Thing station, ObjectiveDef objective, bool active)
        {
            this.station = station;
            this.objectives.Add(objective);
            this.active = active;
        }

        public void AddObjective(ObjectiveDef objective)
        {
            if (!objectives.Contains(objective))
            {
                objectives.Add(objective);
            }
        }

        public void ExposeData()
        {
            Scribe_References.Look(ref station, "thing");
            Scribe_Collections.Look(ref objectives, "objective", LookMode.Def);
            Scribe_Values.Look(ref active, "active");
        }
    }
}
