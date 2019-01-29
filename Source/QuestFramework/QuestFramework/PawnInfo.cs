using RimWorld;
using Verse;

namespace StoryFramework
{
    public class PawnInfo
    {
        public ThingDef def;
        public PawnKindDef kindDef;
        public FactionDef factionDef;
        public Gender? gender = Gender.None;

        public PawnInfo(Pawn pawn)
        {
            if (pawn != null)
            {
                this.def = pawn.def;
                this.kindDef = pawn.kindDef;
                this.factionDef = pawn.Faction?.def;
                this.gender = pawn.gender;
            }
        }
    }
}
