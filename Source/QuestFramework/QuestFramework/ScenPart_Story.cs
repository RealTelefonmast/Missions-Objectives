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

        public override string Summary(Scenario scen)
        {
            StringBuilder sb = new StringBuilder(base.Summary(scen));
            string locked = "";
            string unlocked = "";
            string finished = "";
            foreach (MissionDef def in LockedMissions)
            {
                locked += "   - " + def.LabelCap + "\n";
            }
            foreach (MissionDef def in UnlockedMissions)
            {
                unlocked += "   - " + def.LabelCap + "\n";
            }
            foreach (MissionDef def in FinishedMissions)
            {
                finished += "   - " + def.LabelCap + "\n";
            }
            sb.AppendLine();
            sb.AppendLine("ScenPart_SMO".Translate());
            if (!UnlockedMissions.NullOrEmpty())
            {
                sb.AppendLine("UnlockedMissions_SMO".Translate() + ": ");
                sb.AppendLine(unlocked);
            }
            if (!LockedMissions.NullOrEmpty())
            {
                sb.AppendLine("LockedMissions_SMO".Translate() + ": ");
                sb.AppendLine(locked);
            }
            if (!FinishedMissions.NullOrEmpty())
            {
                sb.AppendLine("FinishedMissions_SMO".Translate() + ": ");
                sb.AppendLine(finished);
            }
            return sb.ToString().TrimEndNewlines();
        }
    }
}
