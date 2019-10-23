using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace StoryFramework
{
    public class StoryTimer : IExposable
    {
        private readonly int defaultTime = 0;
        private int timeLeft = -1;
        public int DefaultTime => defaultTime;
        public int TimeLeft => timeLeft;

        public StoryTimer(TimerSettings settings)
        {
            defaultTime = settings.TotalTimeTicks();
            timeLeft = defaultTime;
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref timeLeft, "timeLeft");
        }

        public void Tick()
        {
            if (timeLeft > 0)
                timeLeft--;
        }

        public void Terminate()
        {
            timeLeft = 0;
        }

        public bool Finished => timeLeft <= 0;

        public string GetTimerText(StoryState state)
        {
            string label = "";
            if (state == StoryState.Finished || state == StoryState.Failed || timeLeft <= 0)
            {
                return label = "---";
            }
            if (timeLeft > GenDate.TicksPerYear)
            {
                label = Math.Round((decimal)timeLeft / GenDate.TicksPerYear, 1) + "y";
            }
            else if (timeLeft > GenDate.TicksPerDay)
            {
                label = Math.Round((decimal)timeLeft / GenDate.TicksPerDay, 1) + "d";
            }
            else if (timeLeft < GenDate.TicksPerDay)
            {
                label = Math.Round((decimal)timeLeft / GenDate.TicksPerHour, 1) + "h";
            }
            return label;
        }
    }
}
