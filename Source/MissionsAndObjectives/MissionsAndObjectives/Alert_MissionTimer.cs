using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using UnityEngine;

namespace MissionsAndObjectives
{

    public class Alert_MissionTimer : Alert
    {

        public Alert_MissionTimer()
        {
            this.defaultLabel = "MissionTimer".Translate();
            this.defaultPriority = AlertPriority.Critical;
        }

        protected override Color BGColor
        {
            get
            {
                float num = Pulser.PulseBrightness(0.5f, Pulser.PulseBrightness(0.5f, 0.6f));
                return new Color(num, num, num) * Color.cyan;
            }
        }

        public override string GetExplanation()
        {
            StringBuilder allTimers = new StringBuilder();
            foreach (Mission mission in WorldComponent_Missions.MissionHandler.Missions)
            {
                foreach (Objective objective in mission.Objectives.Where(o => o.Active))
                {
                    if (objective.def.timerDays > 0)
                    {
                        allTimers.AppendLine("    " + objective.def.LabelCap + "  " + objective.GetTimerText);
                    }
                }
            }
            return "MissionTimerDesc".Translate(new object[] { allTimers });
        }

        public override AlertReport GetReport()
        {
            if (WorldComponent_Missions.MissionHandler.Missions.Any((Mission x) => x.Objectives.Any((obj => obj.Active && !obj.Finished && obj.GetTimer > 0))))
            {
                return AlertReport.Active;
            }
            return false;
        }

    }

}
