using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace StoryFramework
{
    public class ScenPart_Story : ScenPart
    {
        public List<MissionDef> lockedMissions = new List<MissionDef>();
        public List<MissionDef> unlockedMissions = new List<MissionDef>();
        public List<MissionDef> finishedMissions = new List<MissionDef>();

        public List<IncidentProperties> incidents = new List<IncidentProperties>();

        public override void PostWorldGenerate()
        {
            base.PostWorldGenerate();
            WorldComponent_Story.Story.SetupScenario(this);
        }

        public override void PostMapGenerate(Map map)
        {
            base.PostMapGenerate(map);
            foreach(var props in incidents)
            {
                props.TryExecute(map);
            }
        }

        public List<MissionDef> MissionsToStart
        {
            get
            {
                List<MissionDef> missions = new List<MissionDef>();
                missions.AddRange(unlockedMissions);
                missions.AddRange(finishedMissions);
                return missions;
            }
        }

        public override string Summary(Scenario scen)
        {
            StringBuilder sb = new StringBuilder(base.Summary(scen));
            string locked = "";
            string unlocked = "";
            string finished = "";
            foreach (MissionDef def in lockedMissions)
            {
                locked += "   - " + def.LabelCap + "\n";
            }
            foreach (MissionDef def in unlockedMissions)
            {
                unlocked += "   - " + def.LabelCap + "\n";
            }
            foreach (MissionDef def in finishedMissions)
            {
                finished += "   - " + def.LabelCap + "\n";
            }
            sb.AppendLine();
            sb.AppendLine("ScenPart_SMO".Translate());
            if (!unlockedMissions.NullOrEmpty())
            {
                sb.AppendLine("UnlockedMissions_SMO".Translate() + ": ");
                sb.AppendLine(unlocked);
            }
            if (!lockedMissions.NullOrEmpty())
            {
                sb.AppendLine("LockedMissions_SMO".Translate() + ": ");
                sb.AppendLine(locked);
            }
            if (!finishedMissions.NullOrEmpty())
            {
                sb.AppendLine("FinishedMissions_SMO".Translate() + ": ");
                sb.AppendLine(finished);
            }
            return sb.ToString().TrimEndNewlines();
        }
    }
}
