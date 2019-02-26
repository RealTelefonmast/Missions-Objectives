using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;

namespace StoryFramework
{
    public abstract class StoryObjectDef
    {
        public Requisites requisites = new Requisites();
        public TimerSettings timer = new TimerSettings();
        public FailConditions failConditions;
    }

    public abstract class StoryObject
    {

    }
}
