using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;

namespace StoryFramework
{
    public class StoryObjectDef : Def
    {
        public Requisites requisites = new Requisites();
        public TimerSettings timer = new TimerSettings();
        public FailConditions failConditions;

        public StoryAction startAction;
        public StoryAction completeAction;
        public StoryAction failAction;
    }
}
