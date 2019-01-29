using System.Collections.Generic;
using System.Linq;
using Verse;

namespace StoryFramework
{
    public class FailConditions
    {
        // If any of these are finished, the objective automatically fails
        public List<ObjectiveDef> objectives = new List<ObjectiveDef>();
        public List<MissionDef> missions = new List<MissionDef>();
        public TargetLossSettings targetSettings = new TargetLossSettings();
        public bool whenFinished = true;

        public bool Failed
        {
            get
            {
                if (AnyObjective || AnyMission)
                {
                    return true;
                }
                return false;
            }
        }

        private bool AnyObjective
        {
            get
            {
                if (!objectives.NullOrEmpty())
                {
                    if (whenFinished)
                    {
                        return objectives.Any(o => o.IsFinished);
                    }
                    else
                    {
                        return objectives.Any(o => o.CurrentState == MOState.Failed);
                    }
                }
                return false;
            }
        }

        private bool AnyMission
        {
            get
            {
                if (!missions.NullOrEmpty())
                {
                    if (whenFinished)
                    {
                        return missions.Any(m => m.IsFinished);
                    }
                    else
                    {
                        return missions.Any(m => m.CurrentState == MOState.Failed);
                    }
                }
                return false;
            }
        }
    }
}
