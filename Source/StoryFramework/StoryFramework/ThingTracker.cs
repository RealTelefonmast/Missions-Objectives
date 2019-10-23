using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.Sound;
using RimWorld;

namespace StoryFramework
{
    //IDEA: When 

    public class TrackedTarget
    {
        public TargetInfo Target;
        public string targetID;


    }

    public class ThingTracker : IExposable
    {

        public void ExposeData()
        {

        }

        public ThingTracker() { }

        public void Tick()
        {

        }

        public void ProcessInput<T>()
        {

        }
    }
}
