using System.Linq;
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
            this.defaultExplanation = "MissionTimerDesc".Translate();
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
            return base.GetExplanation();
        }

        public override AlertReport GetReport()
        {
            if (Find.World.GetComponent<WorldComponent_Missions>().Missions.Any((Mission x) => x.Objectives.Any((obj => obj.Active && !obj.Finished && obj.GetTimer > 0))))
            {
                return AlertReport.Active;
            }
            return false;
        }
    }
}
