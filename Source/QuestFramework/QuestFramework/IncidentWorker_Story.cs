using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace StoryFramework
{
    public class IncidentWorker_Story : IncidentWorker
    {
        public StoryIncidentDef Def
        {
            get
            {
                return this.def as StoryIncidentDef;
            }
        }

        protected override bool CanFireNowSub(IncidentParms parms)
        {
            if (Def.requisites.IsFulfilled(null,null,true, Find.Maps.Find(m => m.Tile == parms.target.Tile)))
            {
                return true;
            }
            return base.CanFireNowSub(parms);
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            if(Def.incidentType == StoryIncidentType.MissionStart)
            {
                foreach(MissionDef def in Def.missionUnlocks)
                {
                    StoryManager.StoryHandler.ActivateMission(def);
                }
            }
            else
            {
                foreach(IncidentProperties props in Def.incidentProperties)
                {
                    props.Notify_Execute(parms.target as Map, null, null, null);
                }
            }
            return base.TryExecuteWorker(parms);
        }


    }
}
