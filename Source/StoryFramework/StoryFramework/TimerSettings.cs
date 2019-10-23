using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;

namespace StoryFramework
{
    public enum TimerMode
    {
        Wait,
        Limit
    }

    public class TimerSettings
    {
        public int ticks = 0;
        public float days = 0;
        public TimerMode mode = TimerMode.Limit;

        public int TotalTimeTicks()
        {
            return ticks + Mathf.RoundToInt(GenDate.TicksPerDay * days);
        }
    }
}
