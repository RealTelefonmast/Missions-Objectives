using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using System.Text.RegularExpressions;

namespace MissionsAndObjectives
{
    public class MissionDef : Def
    {
        public ResearchProjectDef basePrerequeisite;

        public List<MissionDef> prerequisites;

        public List<ObjectiveDef> objectives = new List<ObjectiveDef>();

        public string modIdentifier;

        public bool hideOnComplete = false;

        public bool repeatable = false;

        public bool IncidentBound
        {
            get
            {
                if (DefDatabase<MissionIncidentDef>.AllDefsListForReading.Any(x => !x.missionUnlocks.NullOrEmpty() && x.missionUnlocks.Contains(this)))
                {
                    return true;
                }
                if (DefDatabase<MissionIncidentDef>.AllDefsListForReading.Any(x => x.incidentProperties.Any(x2 => !x2.missionUnlocks.NullOrEmpty() && x2.missionUnlocks.Contains(this))))
                {
                    return true;
                }
                if (DefDatabase<ObjectiveDef>.AllDefsListForReading.Any(o => o.incidentsOnCompletion.Any(ic => !ic.missionUnlocks.NullOrEmpty() && ic.missionUnlocks.Contains(this)) || o.incidentsOnFail.Any(ic => !ic.missionUnlocks.NullOrEmpty() && ic.missionUnlocks.Contains(this))))
                {
                    return true;
                }
                return false;
            }
        }

        public string ModIdentifier
        {
            get
            {
                return Regex.Replace(modIdentifier, @"\s+", "");
            }
        }

        public bool ModBound
        {
            get
            {
                return modIdentifier != null;
            }
        }

        public bool ModLoaded
        {
            get
            {
                return LoadedModManager.RunningMods.Any(mcp => mcp.Identifier == ModIdentifier);
            }
        }

        public bool IsFinished
        {
            get
            {
                if (WorldComponent_Missions.MissionHandler.Missions.Any(m => m.def == this))
                {
                    return Find.World.GetComponent<WorldComponent_Missions>().Missions.Find(m => m.def == this).Objectives.All(o => o.Finished);
                }
                return false;
            }
        }

        public bool Visible
        {
            get
            {
                if (IsFinished)
                {
                    return !hideOnComplete;
                }
                return true;
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
                if (ModBound)
                {
                    return this.PrerequisitesCompleted && ModLoaded;
                }
                return this.PrerequisitesCompleted;
            }
        }
    }
}
