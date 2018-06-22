using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;

namespace MissionsAndObjectives
{
    public class IncidentFilter
    {
        public List<TerrainDef> terrainToAvoid = new List<TerrainDef>();

        public List<ThingDef> spawnAt = new List<ThingDef>();

        public List<ThingValue> distanceFromThings = new List<ThingValue>();

        public AreaCheck avoidRoofs = AreaCheck.Allow;

        public AreaCheck avoidHome = AreaCheck.Allow;
    }

    public enum AreaCheck
    {
        Prefer,
        Avoid,
        Allow
    }
}
