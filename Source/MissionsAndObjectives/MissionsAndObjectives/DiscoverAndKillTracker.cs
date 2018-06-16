using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace MissionsAndObjectives
{
    public class DiscoverAndKillTracker : IExposable
    {
        private List<ThingDef> thingsToCheck = new List<ThingDef>();

        private List<PawnKindDef> pawnsToCheck = new List<PawnKindDef>();

        private Dictionary<ThingDef, bool> discoveredThings = new Dictionary<ThingDef, bool>();

        private int Amount;

        private int destroyedThings = 0;

        public DiscoverAndKillTracker()
        {
        }

        public DiscoverAndKillTracker(List<ThingDef> defs, List<PawnKindDef> pawns, int amt)
        {
            thingsToCheck = defs;
            pawnsToCheck = pawns;
            Amount = amt;
            foreach(ThingDef def in thingsToCheck)
            {
                if (!discoveredThings.Keys.Contains(def))
                {
                    discoveredThings.Add(def, false);
                }
            }
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref destroyedThings, "destroyedThings", 0, true);
            Scribe_Collections.Look(ref discoveredThings, "discoveredThings");
        }

        public int GetCountKilled
        {
            get
            {
                if(destroyedThings > Amount)
                {
                    return Amount;
                }
                return destroyedThings;
            }
        }

        public int GetCountDiscovered
        {
            get
            {
                return discoveredThings.Keys.Where(k => discoveredThings[k] == true).Count();
            }
        }

        public bool AllDiscovered
        {
            get
            {
                return discoveredThings.Values.All(b => true);
            }
        }

        public void Destroy(ThingDef def = null, PawnKindDef pawn = null)
        {
            if (thingsToCheck.Contains(def) || pawnsToCheck.Contains(pawn))
            {
                destroyedThings += 1;
            }
        }

        public void Discover(ThingDef def)
        {
            if (discoveredThings.Keys.Contains(def))
            {
                discoveredThings[def] = true;
            }
        }
    }
}
