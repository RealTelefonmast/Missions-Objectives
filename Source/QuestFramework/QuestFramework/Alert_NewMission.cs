using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

namespace StoryFramework
{
    public class Alert_NewMission : Alert
    {
        public Alert_NewMission()
        {
            defaultLabel = "MissionAlert_SMO".Translate();
            defaultExplanation = "MissionAlertDesc_SMO".Translate();
            defaultPriority = AlertPriority.Critical;
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
            UIHighlighter.HighlightTag("MainTab-StoryTab-Closed");
            foreach(ModContentPackWrapper MCPW in StoryManager.StoryHandler.ModFolder)
            {
                if (MCPW.HasUnseenMissions)
                {
                    UIHighlighter.HighlightTag(MCPW.identifier + "-StoryHighlight");
                }
            }
            return base.GetExplanation();
        }

        public override AlertReport GetReport()
        {
            if(StoryManager.StoryHandler.Missions.Any(m => !m.Seen))
            {
                return AlertReport.Active;
            }
            return false;
        }
    }
}
