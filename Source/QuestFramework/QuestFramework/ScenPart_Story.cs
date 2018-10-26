using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace StoryFramework
{
    public class ScenPart_Story : ScenPart
    {
        public List<MissionDef> LockedMissions = new List<MissionDef>();
        public List<MissionDef> UnlockedMissions = new List<MissionDef>();
        public List<MissionDef> FinishedMissions = new List<MissionDef>();

        public override void PostMapGenerate(Map map)
        {
            base.PostMapGenerate(map);
            StoryManager manager = StoryManager.StoryHandler;
            manager.LockedMissions = LockedMissions;
            manager.Missions?.RemoveAll(m => LockedMissions.Any(d => m.def == d));
            foreach (MissionDef def in UnlockedMissions)
            {
                manager.ActivateMission(def);
            }
            foreach (MissionDef def in FinishedMissions)
            {
                Mission mission = manager.ActivateMission(def);
                if (mission != null)
                {
                    mission.LatestState = MOState.Finished;
                }
            }
        }
    }
}
