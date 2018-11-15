using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace StoryFramework
{
    public class FailTracker : IExposable
    {
        public FailConditions settings;
        public Dictionary<ThingValue, int> TargetsLost = new Dictionary<ThingValue, int>();
        private List<int> tempInts = new List<int>();
        public int lostColonists = 0;

        public FailTracker(FailConditions settings, bool reloading = false)
        {
            this.settings = settings;
            if (!reloading)
            {
                foreach (ThingValue tv in settings.targetSettings.targets)
                {
                    TargetsLost.Add(tv, 0);
                }
            }
        }

        public void ExposeData()
        {
            if (!TargetsLost.Values.ToList().NullOrEmpty())
            {
                tempInts = TargetsLost.Values.ToList();
            }
            Scribe_Values.Look(ref lostColonists, "dedBois");
            Scribe_Collections.Look(ref tempInts, "tempInts", LookMode.Value);
            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                for (int i = 0; i < settings.targetSettings.targets.Count; i++)
                {
                    TargetsLost.Add(settings.targetSettings.targets[i], tempInts[i]);
                }
            }
        }

        public int TotalCount
        {
            get
            {
                return settings.targetSettings.targets.Sum(t => t.value);
            }
        }

        public bool Failed
        {
            get
            {           
                if (settings.Failed)
                {
                    return true;
                }
                if (settings.targetSettings.minColonistsToLose > 0 && lostColonists >= settings.targetSettings.minColonistsToLose)
                {
                    return true;
                }
                if (!settings.targetSettings.targets.NullOrEmpty())
                {
                    if (settings.targetSettings.any)
                    {
                        return TargetsLost.Any(t => t.Key.value > 0 && t.Value == t.Key.value);
                    }
                    return TargetsLost.All(t => t.Value == t.Key.value);
                }
                return false;
            }
        }

        public void ProcessTarget(ThingDef def, bool isColonist = false)
        {
            if (isColonist)
            {
                lostColonists++;
            }
            ThingValue thingValue = TargetsLost.ThingValue(def);
            if (thingValue != null && TargetsLost[thingValue] < thingValue.value)
            {
                TargetsLost[thingValue] += 1;
            }
        }
    }
}
