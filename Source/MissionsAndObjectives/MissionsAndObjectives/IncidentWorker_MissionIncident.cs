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
            if (Def.incidentType == MissionIncidentType.MissionStart)
            {
                foreach (MissionDef mission in Def.missionUnlocks)
                {
                    Missions.AddNewMission(mission);
                }
            }
            else
            {              
                if(!Def.incidentProperties.NullOrEmpty())
                {
                    foreach (IncidentProperties props in Def.incidentProperties)
                    {
                        if (Def.Worker.CanFireNow(parms.target))
                        {
                            props.Notify_Execute(parms.target as Map, null);
                        }
                    }
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
