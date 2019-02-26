using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace StoryFramework
{
    public class Requisites : Editable
    {
        public List<ObjectiveDef> objectives = new List<ObjectiveDef>();
        public List<MissionDef> missions = new List<MissionDef>();
        public List<ResearchProjectDef> researchProjects = new List<ResearchProjectDef>();
        public List<ThingValue> things = new List<ThingValue>();
        public List<IncidentDef> incidents = new List<IncidentDef>();
        public List<JobDef> jobs = new List<JobDef>();
        public bool failedObjectives = false;
        public bool failedMissions = false;

        public bool anyList = false;
        public bool anyObjective = false;
        public bool anyMission = false;
        public bool anyResearch = false;
        public bool anyThing = false;

        public override IEnumerable<string> ConfigErrors()
        {
            if(!incidents.NullOrEmpty() && !jobs.NullOrEmpty())
            {
                yield return "Cannot have both incidents and jobs as a requisite.";
            }
        }

        public bool StatusForType<T>(T t)
        {
            if(t is ResearchProjectDef)
            {
                return ResearchDone;
            }
            if (t is MissionDef)
            {
                return MissionsReady;
            }
            if (t is ObjectiveDef)
            {
                return ObjectivesReady;
            }
            if (t is ThingValue)
            {
                return OwnsAllThings;
            }
            return false;
        }

        public bool IsFulfilled(JobDef job = null, IncidentDef incident = null, bool isIncident = false, Map map = null)
        {
            if (anyList)
            {
                return ResearchDone || ObjectivesReady || MissionsReady || OwnsAllThings || IncidentReady(incident) || (isIncident ? JobsBeingDone(map) : JobReady(job));
            }
            return ResearchDone && ObjectivesReady && MissionsReady && OwnsAllThings && (IncidentReady(incident) || (isIncident ? JobsBeingDone(map) : JobReady(job)));
        }

        public bool Impossible
        {
            get
            {
                return MissionImpossible || ObjectiveImpossible;
            }
        }

        public bool MissionLocked
        {
            get
            {
                if (anyList)
                {
                    if(objectives.NullOrEmpty() || things.NullOrEmpty() || researchProjects.NullOrEmpty() || jobs.NullOrEmpty())
                    {
                        return true;
                    }
                }
                return !missions.NullOrEmpty();
            }
        }

        private bool MissionImpossible
        {
            get
            {
                if (!missions.NullOrEmpty())
                {
                    if (anyMission)
                    {
                        return missions.All(m => m.CurrentState == MOState.Failed);
                    }
                    return missions.Any(m => m.CurrentState == MOState.Failed);
                }
                return false;
            }
        }

        private bool ObjectiveImpossible
        {
            get
            {
                if (!objectives.NullOrEmpty())
                {
                    if (anyObjective)
                    {
                        return objectives.All(m => m.CurrentState == MOState.Failed);
                    }
                    return objectives.Any(m => m.CurrentState == MOState.Failed);
                }
                return false;
            }
        }

        public bool JobsBeingDone(Map map)
        {
            if (!jobs.NullOrEmpty())
            {
                return map.mapPawns.FreeColonistsSpawned.Any(p => jobs.Contains(p.CurJobDef));
            }
            return true;
        }

        public bool IncidentReady(IncidentDef def)
        {
            if (!incidents.NullOrEmpty())
            {
                if (def != null)
                {
                    return incidents.Contains(def);
                }
                return false;
            }
            return true;
        }

        public bool JobReady(JobDef def)
        {
            if (!jobs.NullOrEmpty())
            {
                if (def != null)
                {
                    return jobs.Contains(def);
                }
                return false;
            }
            return true;
        }

        public bool MissionsReady
        {
            get
            {
                if (!missions.NullOrEmpty())
                {
                    if (anyMission)
                    {
                        if (failedMissions)
                        {
                            missions.Any(m => m.IsComplete(out bool failed) && failed);
                        }
                        return missions.Any(m => m.IsComplete(out bool failed) && !failed);
                    }
                    if (failedMissions)
                    {
                        return missions.All(m => m.IsComplete(out bool failed) && failed);
                    }
                    return missions.All(m => m.IsComplete(out bool failed) && !failed);
                }
                return true;
            }
        }

        public bool ObjectivesReady
        {
            get
            {
                if (!objectives.NullOrEmpty())
                {
                    if (anyObjective)
                    {
                        if (failedObjectives)
                        {
                            return objectives.Any(o => o.IsFailed);
                        }
                        return objectives.Any(o => o.IsFinished);
                    }
                    if (failedObjectives)
                    {
                        return objectives.All(o => o.IsFailed);
                    }
                    return objectives.All(o => o.IsFinished);
                }
                return true;
            }
        }

        public bool ResearchDone
        {
            get
            {
                if (!researchProjects.NullOrEmpty())
                {
                    if (anyResearch)
                    {
                        return researchProjects.Any(r => r.IsFinished);
                    }
                    return researchProjects.All(r => r.IsFinished);
                }
                return true;
            }
        }

        public bool OwnsAllThings
        {
            get
            {
                if (!things.NullOrEmpty())
                {
                    foreach (Map map in Find.Maps)
                    {
                        if (map.IsPlayerHome)
                        {
                            int owned = 0;
                            foreach (ThingValue tv in things)
                            {
                                if (tv.ThingDef != null)
                                {
                                    IEnumerable<Thing> list = map.listerThings.ThingsOfDef(tv.ThingDef).Where(t => t.IsInValidStorage());
                                    int num = 0;
                                    foreach (Thing item in list)
                                    {
                                        bool notValid = false;
                                        if (tv.Stuff != null && item.Stuff != tv.Stuff)
                                        {
                                            notValid = true;
                                        }
                                        bool flag = item.TryGetQuality(out QualityCategory qc);
                                        if (tv.CustomQuality && flag && qc != tv.QualityCategory)
                                        {
                                            notValid = true;
                                        }
                                        if (!notValid)
                                        {
                                            num += item.stackCount;
                                            if (num >= tv.value)
                                            {
                                                owned++;
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                            return anyThing ? owned > 0 : owned == things.Count;
                        }
                    }
                    return false;
                }
                return true;
            }
        }
    }
}
