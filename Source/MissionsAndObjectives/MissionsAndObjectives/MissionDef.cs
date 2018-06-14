using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace MissionsAndObjectives
{
    public class MissionDef : Def
    {
        public ResearchProjectDef basePrerequeisite;

        public List<MissionDef> prerequisites;

        public List<ObjectiveDef> objectives = new List<ObjectiveDef>();

        public bool hideOnComplete = false;

        public bool IncidentBound
        {
            get
            {
                return DefDatabase<MissionIncidentDef>.AllDefsListForReading.Any((MissionIncidentDef x) => x.missionUnlocks.Contains(this));
            }
        }

        public bool IsFinished
        {
            get
            {
                return Find.World.GetComponent<WorldComponent_Missions>().Missions.Find(m => m.def == this).Objectives.All(o => o.Finished);
            }
        }

        public bool PrerequisitesCompleted
        {
            get
            {
                if (basePrerequeisite != null)
                {
                    if (!basePrerequeisite.IsFinished)
                    {
                        return false;
                    }
                }
                if (this.prerequisites != null)
                {
                    for (int i = 0; i < this.prerequisites.Count; i++)
                    {
                        if (!this.prerequisites[i].IsFinished)
                        {
                            return false;
                        }
                    }
                }
                return true;
            }
        }

        public bool CanStartNow
        {
            get
            {
                return this.PrerequisitesCompleted;
            }
        }
    }
}
