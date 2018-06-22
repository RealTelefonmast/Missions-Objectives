using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace MissionsAndObjectives
{
    public class ThingTracker : IExposable
    {
        private bool any = false;

        private List<ThingValue> targetsToCheck = new List<ThingValue>();

        public Dictionary<ThingDef, bool> discoveredThings = new Dictionary<ThingDef, bool>();

        public Dictionary<ThingDef, int> madeThings = new Dictionary<ThingDef, int>();

        public Dictionary<ThingDef, int> destroyedThings = new Dictionary<ThingDef, int>();

        public ThingTracker()
        {
        }

        public ThingTracker(List<ThingValue> defs, bool flag)
        {
            targetsToCheck = defs;
            foreach(ThingValue tv in targetsToCheck)
            {
                ThingDef def = tv.def;
                if (!this.discoveredThings.Keys.Contains(def))
                {
                    this.madeThings.Add(def, 0);
                    this.destroyedThings.Add(def, 0);
                    this.discoveredThings.Add(def, false);
                }
            }
            any = flag;
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref any, "any");
            Scribe_Collections.Look(ref this.madeThings, "madeThings");
            Scribe_Collections.Look(ref this.discoveredThings, "discoveredThings");
            Scribe_Collections.Look(ref this.destroyedThings, "destroyedThings");
        }

        public int GetTargetCount
        {
            get
            {
                return targetsToCheck.Sum(tv => tv.value);
            }
        }

        public bool AllDiscovered
        {
            get
            {
                if (any)
                {
                    return this.discoveredThings.Values.Any(b => b);
                }
                return this.discoveredThings.Values.All(b => b);
            }
        }

        public bool AllKilled
        {
            get
            {
                if(any)
                {
                    return targetsToCheck.Any(tv => destroyedThings[tv.def] >= tv.value);
                }
                return GetCountKilled >= targetsToCheck.Sum(tv => tv.value);
            }
        }

        public int GetCountKilled
        {
            get
            {
                if (!destroyedThings.ToList().NullOrEmpty())
                {
                    return destroyedThings.Values.Sum();
                }
                return 0;
            }
        }

        public int GetCountDiscovered
        {
            get
            {
                return this.discoveredThings.Values.Count(v => v);
            }
        }

        public bool AllMade
        {
            get
            {
                if (any)
                {
                    return targetsToCheck.Any(tv => madeThings[tv.def] >= tv.value);
                }
                return GetCountMade >= targetsToCheck.Sum(tv => tv.value);
            }
        }

        public int GetCountMade
        {
            get
            {
                return madeThings.Keys.Sum(k => madeThings[k]);
            }
        }

        public int GetCountMadeFor(ThingDef def)
        {
            return madeThings[def];
        }

        public void Destroy(ThingDef def)
        {
            if (destroyedThings.Keys.Contains(def))
            {
                destroyedThings[def] += 1;
            }
        }

        public void Discover(ThingDef def)
        {
            if (this.discoveredThings.Keys.Contains(def))
            {
                this.discoveredThings[def] = true;
            }
        }

        public void Make(ThingDef def)
        {
            if (madeThings.Keys.Contains(def))
            {
                madeThings[def] += 1;
            }
        }
    }
}
