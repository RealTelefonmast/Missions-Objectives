using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;

namespace StoryFramework
{
    public enum StoryIncidentType
    {
        CustomWorker,
        MissionStart
    }

    public class StoryIncidentDef : IncidentDef
    {
        public StoryIncidentType incidentType = StoryIncidentType.MissionStart;
        public Requisites requisites = new Requisites();
        public List<MissionDef> missionUnlocks = new List<MissionDef>();
        public List<IncidentProperties> incidentProperties = new List<IncidentProperties>();

        public override IEnumerable<string> ConfigErrors()
        {
            if(incidentType == StoryIncidentType.MissionStart && missionUnlocks.NullOrEmpty())
            {
                yield return "IncidentType is 'MissionStart' but no missions are defined in the 'missionUnlocks' list.";
            }
            if (incidentType == StoryIncidentType.CustomWorker && incidentProperties.NullOrEmpty())
            {
                yield return "IncidentType is 'CustomWorker' but no IncidentProperties are defined in the 'incidentProperties' list.";
            }
        }
    }
}
