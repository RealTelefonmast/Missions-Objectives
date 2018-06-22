using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;
using RimWorld;

namespace MissionsAndObjectives
{
    public enum MissionIncidentType
    {
        CustomWorker,
        MissionStart
    }

    public class MissionIncidentDef : IncidentDef
    {
        public MissionIncidentType type = MissionIncidentType.MissionStart;

        public List<JobDef> anyJobNeeded = new List<JobDef>();

        public List<ThingDef> thingRequisites = new List<ThingDef>();

        public List<MissionDef> missionRequisites = new List<MissionDef>();

        public List<ObjectiveDef> objectiveRequisites = new List<ObjectiveDef>();

        public IncidentProperties customIncident;

        public List<MissionDef> missionUnlocks;

        public override IEnumerable<string> ConfigErrors()
        {
            if (type == MissionIncidentType.CustomWorker && customIncident == null)
            {
                yield return "MissionIncidentDef is missing field 'customIncident'.";
            }
            if(type == MissionIncidentType.MissionStart && missionUnlocks.NullOrEmpty())
            {
                yield return "MissionIncidentDef unlocks missions but has no MissionDefs in field 'missionUnlocks'.";
            }
        }

        public WorldComponent_Missions Missions
        {
            get
            {
                return Find.World.GetComponent<WorldComponent_Missions>();
            }
        }

        public bool ObjectivesDone
        {
            get
            {
                if (!objectiveRequisites.NullOrEmpty())
                {
                    return objectiveRequisites.All((ObjectiveDef x) => x.IsFinished);
                }
                return true;
            }
        }

        public bool MissionsDone
        {
            get
            {
                if (!missionRequisites.NullOrEmpty())
                {
                    return Missions.Missions.All((Mission x) => missionRequisites.Contains(x.def) && x.def.IsFinished);
                }
                return true;
            }
        }

        public bool ThingsAvailable(Map map)
        {
            if (!thingRequisites.NullOrEmpty())
            {
                return map.listerThings.AllThings.All((Thing x) => thingRequisites.Contains(x.def));
            }
            return true;
        }

        public bool JobsAvailable(Map map)
        {
            if (!anyJobNeeded.NullOrEmpty())
            {
                IEnumerable<Pawn> pawns = map.mapPawns.AllPawns.Where((Pawn x) => x.IsColonist && anyJobNeeded.Contains(x.CurJob.def));
                if (pawns.ToList().NullOrEmpty())
                {
                    return false;
                }
            }
            return true;
        }

        public bool CanStart(Map map)
        {
            if (Missions.Missions.All((Mission x) => !missionUnlocks.Contains(x.def)))
            {
                if (ObjectivesDone && MissionsDone && ThingsAvailable(map) && JobsAvailable(map))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
