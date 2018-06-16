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

        public List<ThingDistancer> distanceFromThings = new List<ThingDistancer>();

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
