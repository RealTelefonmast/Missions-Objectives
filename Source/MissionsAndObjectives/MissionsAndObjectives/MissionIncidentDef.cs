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
        public MissionIncidentType incidentType = MissionIncidentType.MissionStart;

        public List<JobDef> anyJobNeeded = new List<JobDef>();

        public List<ThingValue> thingRequisites = new List<ThingValue>();

        public List<MissionDef> missionRequisites = new List<MissionDef>();

        public List<ObjectiveDef> objectiveRequisites = new List<ObjectiveDef>();

        public List<IncidentProperties> incidentProperties = new List<IncidentProperties>();

        public List<MissionDef> missionUnlocks = new List<MissionDef>();

        public override IEnumerable<string> ConfigErrors()
        {
            if (incidentType == MissionIncidentType.CustomWorker && incidentProperties.NullOrEmpty())
            {
                yield return "MissionIncidentDef is missing field 'customIncident'.";
            }
            if(incidentType == MissionIncidentType.MissionStart && missionUnlocks.NullOrEmpty())
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
                return thingRequisites.All(tv => map.listerThings.ThingsOfDef(tv.ThingDef).Count >= tv.value);
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
            if (ObjectivesDone && MissionsDone && ThingsAvailable(map) && JobsAvailable(map))
            {
                if (incidentType == MissionIncidentType.MissionStart)
                {
                    if (Missions.Missions.All((Mission x) => !missionUnlocks.NullOrEmpty() && !missionUnlocks.Contains(x.def)))
                    {
                        return true;
                    }
                    return false;
                }
                return true;
            }
            return false;
        }
    }
}
