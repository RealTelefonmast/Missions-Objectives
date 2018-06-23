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

        private ObjectiveType type = ObjectiveType.None;

        private List<ThingValue> targetsToCheck = new List<ThingValue>();

        public Dictionary<ThingDef, int> discoveredThings = new Dictionary<ThingDef, int>();

        public Dictionary<ThingDef, int> madeThings = new Dictionary<ThingDef, int>();

        public Dictionary<ThingDef, int> destroyedThings = new Dictionary<ThingDef, int>();

        public Dictionary<PawnKindDef, int> killedThings = new Dictionary<PawnKindDef, int>();

        public ThingTracker()
        {
        }

        public ThingTracker(List<ThingValue> defs, ObjectiveType type, bool flag)
        {
            targetsToCheck = defs;
            this.type = type;
            any = flag;
            foreach (ThingValue tv in targetsToCheck)
            {
                ThingDef def = tv.ThingDef;
                if (def != null)
                {
                    if (type == ObjectiveType.Construct || type == ObjectiveType.Craft)
                    {
                        if (!madeThings.ContainsKey(def))
                        {
                            this.madeThings.Add(def, 0);
                        }
                    }
                    if (type == ObjectiveType.Discover)
                    {
                        if (!discoveredThings.ContainsKey(def))
                        {
                            this.discoveredThings.Add(def, 0);
                        }
                    }
                    if (type == ObjectiveType.Destroy)
                    {
                        if (!destroyedThings.ContainsKey(def))
                        {
                            this.destroyedThings.Add(def, 0);
                        }
                    }
                    if (type == ObjectiveType.Hunt)
                    {
                        if (!killedThings.ContainsKey(tv.PawnKindDef))
                        {
                            this.killedThings.Add(tv.PawnKindDef, 0);
                        }                     
                    }
                }
            }
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref any, "any");
            Scribe_Collections.Look(ref this.madeThings, "madeThings");
            Scribe_Collections.Look(ref this.discoveredThings, "discoveredThings");
            Scribe_Collections.Look(ref this.destroyedThings, "destroyedThings");
            Scribe_Collections.Look(ref this.killedThings, "killedThings");
        }

        //General

        public int GetTargetCount
        {
            get
            {
                return targetsToCheck.Sum(tv => tv.value);
            }
        }

        //Discovered

        public int GetCountDiscovered
        {
            get
            {
                return discoveredThings.Values.Sum();
            }
        }

        public bool AllDiscovered
        {
            get
            {
                if (any)
                {
                    return targetsToCheck.Any(tv => discoveredThings[tv.ThingDef] >= tv.value);
                }
                return GetCountDiscovered >= targetsToCheck.Sum(tv => tv.value);
            }
        }

        // Killed-Desroyed Sum

        public bool AllDestroyedKilled
        {
            get
            {
                return AllPawnsKilled && AllThingsDestroyed;
            }
        }

        public int GetSumKilledDestroyed
        {
            get
            {
                return GetCountKilledPawns + GetCountDestroyedThings;
            }
        }

        // Killed

        public bool AllPawnsKilled
        {
            get
            {
                if (any)
                {
                    return targetsToCheck.Any(tv => killedThings.ContainsKey(tv.PawnKindDef) && killedThings[tv.PawnKindDef] >= tv.value);
                }
                return GetCountKilledPawns >= targetsToCheck.Sum(tv => tv.value);
            }
        }

        public int GetCountKilledPawns
        {
            get
            {
                if (!killedThings.ToList().NullOrEmpty())
                {
                    return killedThings.Values.Sum();
                }
                return 0;
            }
        }

        // Destroyed

        public bool AllThingsDestroyed
        {
            get
            {
                if (any)
                {
                    return targetsToCheck.Any(tv => destroyedThings.ContainsKey(tv.ThingDef) && destroyedThings[tv.ThingDef] >= tv.value);
                }
                return GetCountDestroyedThings >= targetsToCheck.Sum(tv => tv.value);
            }
        }

        public int GetCountDestroyedThings
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

        // Made

        public bool AllMade
        {
            get
            {
                if (any)
                {
                    return targetsToCheck.Any(tv => madeThings[tv.ThingDef] >= tv.value);
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

        // Voids

        public void Kill(PawnKindDef def)
        {
            if (killedThings.ContainsKey(def))
            {
                killedThings[def] += 1;
            }
        }

        public void Destroy(ThingDef def)
        {
            if (destroyedThings.ContainsKey(def))
            {
                destroyedThings[def] += 1;
            }
        }

        public void Discover(ThingDef def)
        {
            if (discoveredThings.ContainsKey(def))
            {
                discoveredThings[def] += 1;
            }
        }

        public void Make(ThingDef def)
        {
            if (madeThings.ContainsKey(def))
            {
                madeThings[def] += 1;
            }
        }
    }
}
