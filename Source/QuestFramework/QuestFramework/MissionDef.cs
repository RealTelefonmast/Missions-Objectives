using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using RimWorld;
using Verse;

namespace StoryFramework
{
    public class MissionDef : Def
    {
        public List<ObjectiveDef> objectives = new List<ObjectiveDef>();
        public Requisites requisites = new Requisites();
        public TimerSettings timer = new TimerSettings();
        public FailConditions failConditions;
        public bool chronological = false;
        public bool hideOnComplete = false;
        public bool repeatable = false;
        public string modName;

        public override IEnumerable<string> ConfigErrors()
        {
            if (objectives.NullOrEmpty())
            {
                Log.Warning("MissionDef " + defName + " has an empty objectives list, this should not be the case.");
            }
            if (chronological && objectives.Any(o => o.requisites != null))
            {
                Log.Warning("MissionDef " + defName + " has one ore more objective that use requisites, which will be ignored because the mission is chronological.");
            }
            if (repeatable)
            {
                if (timer.GetTotalTime > 0)
                {
                    yield return "A timer cannot be set in a repeatable mission.";
                }
            }
        }

        public string ModIdentifier
        {
            get
            {
                return Regex.Replace(modName, @"\s+", "");
            }
        }

        public bool ModBound
        {
            get
            {
                return !modName.NullOrEmpty();
            }
        }

        public bool ModLoaded
        {
            get
            {
                return LoadedModManager.RunningMods.Any(mcp => mcp.Identifier == ModIdentifier);
            }
        }

        public bool HardLocked
        {
            get
            {
                if (DefDatabase<ScenarioDef>.AllDefs.Any(s => s.scenario.AllParts.Any(sp => sp is ScenPart_Story && (sp as ScenPart_Story).UnlockedMissions.Contains(this))))
                {
                    return true;
                }
                if(requisites.missions.Any(m => m.HardLocked && m.CurrentState == MOState.Inactive) && requisites.MissionLocked)
                {
                    return true;
                }
                return false;
            }
        }

        public bool Locked
        {
            get
            {
                if(DefDatabase<ScenarioDef>.AllDefs.Any(s => s.scenario.AllParts.Any(sp => sp is ScenPart_Story && (sp as ScenPart_Story).UnlockedMissions.Contains(this))))
                {
                    return true;
                }
                if(DefDatabase<StoryIncidentDef>.AllDefs.Any(s => s.missionUnlocks.Contains(this)))
                {
                    return true;
                }
                return false;
            }
        }

        public bool CanStartNow(JobDef def = null, IncidentDef incident = null)
        {
            if (ModBound)
            {
                return !Locked && requisites.IsFulfilled(def, incident) && ModLoaded;
            }
            return !Locked && requisites.IsFulfilled(def, incident);
        }

        public bool IsComplete(out bool failed)
        {
            failed = CurrentState == MOState.Failed ? true : false;
            return CurrentState == MOState.Finished || CurrentState == MOState.Failed;
        }

        public bool IsFinished
        {
            get
            {
                return CurrentState == MOState.Finished;
            }
        }

        public bool IsSeen
        {
            get
            {
                return StoryManager.StoryHandler.GetMissionSeen(this);
            }
        }

        public MOState CurrentState
        {
            get
            {
                return StoryManager.StoryHandler.GetMissionState(this);
            }
        }
    }
}
