using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace StoryFramework
{
    public class SpawnSettings
    {
        public SpawnMode mode = SpawnMode.Stockpile; 
        public List<ThingValue> spawnList = new List<ThingValue>();
        public List<ThingSkyfaller> skyfallers = new List<ThingSkyfaller>();
    }

    public enum SpawnMode
    {
        Stockpile,
        Target,
        DropPod,
        Scatter
    }
}
