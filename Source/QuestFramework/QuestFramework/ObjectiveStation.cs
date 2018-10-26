using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace StoryFramework
{
    public class ObjectiveStation : IExposable
    {
        public Thing station;
        public List<Objective> objectives = new List<Objective>();
        public bool active;

        public ObjectiveStation(Thing station, Objective objective, bool active)
        {
            this.station = station;
            this.objectives.Add(objective);
            this.active = active;
        }

        public void AddObjective(Objective objective)
        {
            if (!objectives.Contains(objective))
            {
                objectives.Add(objective);
            }
        }

        public void ExposeData()
        {
            Scribe_References.Look(ref station, "thing");
            Scribe_Collections.Look(ref objectives, "objective", LookMode.Reference);
            Scribe_Values.Look(ref active, "active");
        }
    }
}
