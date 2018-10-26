using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;

namespace StoryFramework
{
    public class PawnSettings
    {
        public ThingDef def;
        public PawnKindDef kindDef;
        public FactionDef factionDef;
        public Gender? gender = Gender.None;
        public int minAmount = 1;
        public bool anyHediff = false;
        public bool anySkill = false;

        public bool PawnSatisfies(Pawn pawn)
        {
            if (def != null && pawn.def != def)
            {
                return false;
            }
            if (kindDef != null && pawn.kindDef != kindDef)
            {
                return false;
            }
            if (factionDef != null && (pawn.Faction != null && pawn.Faction.def != factionDef))
            {
                return false;
            }
            if (gender.HasValue && pawn.gender != gender.Value)
            {
                return false;
            }
            return true;
        }

        public bool MapSatisfies(Map map)
        {
            int count = 0;
            foreach(Pawn pawn in map.mapPawns.AllPawns)
            {
                if (PawnSatisfies(pawn))
                {
                    count++;
                    if (count == minAmount)
                    {
                        return true;
                    }
                }
            }
            return count >= minAmount;
        }
    }
}
