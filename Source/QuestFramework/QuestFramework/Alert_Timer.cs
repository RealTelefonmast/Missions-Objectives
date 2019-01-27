using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

namespace StoryFramework
{
    public class Alert_Timer : Alert_Critical
    {
        public Alert_Timer()
        {
            base.defaultLabel = "TimerAlert_SMO".Translate();
        }

        public bool TimerRunningOut()
        {
            StoryManager manager = StoryManager.StoryHandler;
            float threshold = manager.timerAlertMinHours;
            if (manager.Missions.Any(m => m.LatestState == MOState.Active && m.HasTimer && m.GetTimer < GenDate.TicksPerHour * threshold) || manager.GetObjectives.Any(m => m.CurrentState == MOState.Active && m.HasTimer && m.GetTimer < GenDate.TicksPerHour * threshold))
            {
                return true;
            }
            return false;
        }

        public override string GetExplanation()
        {
            StringBuilder sb = new StringBuilder();
            StoryManager manager = StoryManager.StoryHandler;
            foreach (Mission mission in manager.Missions)
            {
                if(mission.HasTimer && mission.LatestState == MOState.Active && mission.GetTimer < GenDate.TicksPerHour * manager.timerAlertMinHours)
                {
                    sb.AppendLine("    - " + mission.def.LabelCap + ": " + StoryUtils.GetTimerText(mission.GetTimer, MOState.Active));
                } 
            }
            foreach (Objective objective in manager.GetObjectives)
            {
                if (objective.def.type != ObjectiveType.Wait && objective.HasTimer && objective.CurrentState == MOState.Active && objective.GetTimer < GenDate.TicksPerHour * manager.timerAlertMinHours)
                {
                    sb.AppendLine("    - " + objective.def.LabelCap + ": " + StoryUtils.GetTimerText(objective.GetTimer, MOState.Active));
                }
            }
            return "TimerAlertDesc_SMO".Translate(sb.ToString());
        }

        public override AlertReport GetReport()
        {
            return TimerRunningOut();
        }
    }
}
