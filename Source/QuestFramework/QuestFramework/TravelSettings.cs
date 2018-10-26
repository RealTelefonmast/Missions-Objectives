using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using RimWorld.Planet;

namespace StoryFramework
{
    public class TravelSettings
    {
        public TravelMode mode = TravelMode.Reach;
        public int minDistance = 0;
        public int minSilver;
        public WorldObjectDef destination;
        public TileSettings tileSettings = new TileSettings();
        public FactionSettings factionSettings = new FactionSettings();
    }

    public class FactionSettings
    {
        public List<FactionDef> factions;
        public bool any = false;
        public int raidAmount = 1;
        public int tradeAmount = 1;

        public int Value(TravelMode mode)
        {
            return mode == TravelMode.Raid ? raidAmount : tradeAmount;
        }

        public int ValueForMode(TravelMode mode)
        {
            int value = Value(mode);
            if (any)
            {
                return value;
            }
            return value * factions.Count;
        }

        public bool FactionInteractionComplete(Dictionary<FactionDef, int> pairs, TravelMode mode)
        {
            int amountAny = (mode == TravelMode.Raid ? raidAmount : tradeAmount);
            int amount = amountAny * factions.Count;
            if (any)
            {
                return pairs.Any(k => k.Value == amountAny);
            }
            return factions.All(pairs.Keys.Contains) && pairs.Sum(k => k.Value) == amount;
        }
    }

    public class TileSettings
    {
        public BiomeDef biome;
        public RoadDef road;
        public RiverDef river;
        public Hilliness? hilliness;

        public bool TileFits(int tile)
        {
            Tile Tile = Find.WorldGrid[tile];
            if (biome != null && Tile.biome != biome)
            {
                return false;
            }
            if(road != null && !Tile.Roads.Any(r => r.road == road))
            {
                return false;
            }
            if (river != null && !Tile.Rivers.Any(r => r.river == river))
            {
                return false;
            }
            if(hilliness != null && Tile.hilliness != hilliness.Value)
            {
                return false;
            }
            return true;
        }
    }

    public enum TravelMode
    {
        Reach,
        Explore,
        Raid,
        Trade
    }
}
