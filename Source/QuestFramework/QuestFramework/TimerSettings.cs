using UnityEngine;
using RimWorld;
using Verse;

namespace StoryFramework
{
    public class TimerSettings 
    {
        public int ticks = 0;
        public float days = 0;
        public bool continueWhenFinished = false;
        //Experimental - Not Used
        public bool critical = false;

        public int GetTotalTime
        {
            get
            {
                return ticks + Mathf.RoundToInt((float)GenDate.TicksPerDay * days);
            }
        }
    }
}
