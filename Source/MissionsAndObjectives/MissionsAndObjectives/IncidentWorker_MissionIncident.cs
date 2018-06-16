using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;

namespace MissionsAndObjectives
{
    public class IncidentWorker_MissionIncident : IncidentWorker
    {
        public MissionIncidentDef Def
        {
            get
            {
                return this.def as MissionIncidentDef;
            }
        }

        public WorldComponent_Missions Missions
        {
            get
            {
                return WorldComponent_Missions.MissionHandler;
            }
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            if (Def.type == MissionIncidentType.MissionStart)
            {
                foreach (MissionDef mission in Def.missionUnlocks)
                {
                    Missions.AddNewMission(mission);
                }
            }
            else
            {
                IncidentWorker worker = (IncidentWorker)Activator.CreateInstance(Def.customWorker);
                if (worker.CanFireNow(parms.target))
                {
                    return worker.TryExecute(parms);
                }
            }
            return true;

        }

        protected override bool CanFireNowSub(IIncidentTarget target)
        {
            if (!Def.CanStart(Find.Maps.Find((Map x) => x.Tile == target.Tile)))
            {
                return false;
            }
            return true;
        }
    }
}
