using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace StoryFramework
{
    public class Requisites : Editable
    {
        //Requsites are a default way to check whether to start a story object
        public List<ResearchProjectDef> research = new List<ResearchProjectDef>();
        public List<MissionDef> missions = new List<MissionDef>();
        public List<ObjectiveDef> objectives = new List<ObjectiveDef>();
        public List<ThingValue> things = new List<ThingValue>();
        public bool anyResearch = false;
        public bool failedMissions = false;
        public bool anyMission = false;
        public bool failedObjectives = false;
        public bool anyObjective = false;
        public bool onlyStored = false;
        public bool anyThing = false;

        public bool anyRequisite = false;

        public bool CheckNow()
        {
            if (things.NullOrEmpty() ? true : Find.TickManager.TicksGame % 750 == 0)
            {

            }
            return false;
        }

        public bool StatusForType<T>(T t)
        {
            if (t is ResearchProjectDef)
            {
                return ResearchReady;
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

        public bool ResearchReady
        {
            get
            {
                if (!research.NullOrEmpty())
                {
                    return anyResearch ? research.Any(r => r.IsFinished) : research.All(r => r.IsFinished);
                }
                return true;
            }
        }

        public bool MissionsReady
        {
            get
            {
                if (!missions.NullOrEmpty())
                {
                    return anyMission ? missions.Any(m => m.IsFinished) : missions.All(m => m.IsFinished);
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
                    return anyObjective ? objectives.Any(o => o.IsFinished) : objectives.All(o => o.IsFinished);
                }
                return true;
            }
        }

        public bool OwnsAllThings
        {
            get
            {

            }
        }
    }
}
