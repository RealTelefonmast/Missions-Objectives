using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace StoryFramework
{
    public class TravelTracker : IExposable
    {
        private List<Caravan> Caravans = new List<Caravan>();
        private Dictionary<FactionDef, int> FactionInteractions = new Dictionary<FactionDef, int>();
        private bool ExploredTile = false;
        public TravelSettings settings;
        
        public TravelTracker() { }

        public TravelTracker(TravelSettings settings)
        {
            this.settings = settings;
        }

        public void ExposeData()
        {
            Scribe_Collections.Look(ref Caravans, "Caravans", LookMode.Reference);
            Scribe_Collections.Look(ref FactionInteractions, "FactionInteractions", LookMode.Def, LookMode.Value);
            Scribe_Values.Look(ref ExploredTile, "ExploredTile");
        }

        public int CountFor(FactionDef def)
        {
            return FactionInteractions.TryGetValue(def);
        }

        public int CurrentCount
        {
            get
            {
                return FactionInteractions.Sum(v => v.Value);
            }
        }

        public List<int> CurrentTiles
        {
            get
            {
                List<int> tiles = new List<int>();
                foreach (Caravan caravan in Caravans)
                {
                    tiles.Add(caravan.Tile);
                }
                return tiles;
            }
        }

        public void UpdateCaravans()
        {
            List<Caravan> caravans = Find.WorldObjects.Caravans;
            foreach (Caravan caravan in caravans)
            {
                if (!Caravans.Contains(caravan))
                {
                    Caravans.Add(caravan);
                }             
            }
        }

        public void TryExplore(int tile)
        {
            foreach (Map map in Find.Maps.Where(m => m.IsPlayerHome))
            {
                if (Find.WorldGrid.ApproxDistanceInTiles(map.Tile, tile) > settings.minDistance && settings.tileSettings.TileFits(tile))
                {
                    ExploredTile = true;
                }
            }
        }

        public void Notify_Interacted(FactionDef def, TravelMode mode, int profit)
        {
            bool profitBool = settings.minSilver > 0 ? true : false;
            if (mode == settings.mode)
            {
                if (settings.factionSettings.factions.Contains(def))
                {
                    if (settings.minSilver != 0)
                    {
                        if (profitBool)
                        {
                           if(profit < settings.minSilver)
                            {
                                return;
                            } 
                        }else
                        {
                            if (profit > settings.minSilver)
                            {
                                return;
                            }
                        }
                    }
                    if (!FactionInteractions.TryGetValue(def, out int value))
                    {
                        FactionInteractions.Add(def, 1);
                        return;
                    }
                    if (value < settings.factionSettings.ValueForMode(settings.mode))
                    {
                        value++;
                    }
                }
            }
        }

        public bool TravelComplete()
        {
            switch (settings.mode)
            {
                case TravelMode.Reach:
                    foreach (int tile in CurrentTiles)
                    {
                        if (settings.tileSettings.TileFits(tile))
                        {
                            if (settings.destination != null)
                            {
                                WorldObject worldObject = Find.WorldObjects.WorldObjectAt(tile, settings.destination);
                                if (!settings.factionSettings.factions.NullOrEmpty() && worldObject != null)
                                {
                                    return settings.factionSettings.factions.Any(f => f == worldObject.Faction?.def);
                                }
                                return worldObject != null;
                            }
                            return true;
                        }
                    }
                    break;
                case TravelMode.Raid:
                case TravelMode.Trade:
                    return settings.factionSettings.FactionInteractionComplete(FactionInteractions, settings.mode);
                case TravelMode.Explore:
                    return ExploredTile;
            }
            return false;
        }
    }
}
