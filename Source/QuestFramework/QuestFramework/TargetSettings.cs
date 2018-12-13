using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace StoryFramework
{
    public class TargetSettings
    { 
        public List<ThingValue> targets = new List<ThingValue>();
        public ThingSettings thingSettings;
        public PawnSettings pawnSettings;
        public bool any = false;
        public bool consume = false;

        public void CheckForDuplicates(ObjectiveDef def)
        {
            List<ThingDef> defList = new List<ThingDef>();
            List<PawnKindDef> pawnList = new List<PawnKindDef>();
            if (targets.NullOrEmpty())
            {
                return;
            }
            foreach(ThingValue tv in targets)
            {
                if(tv.PawnKindDef == null || tv.ThingDef == null)
                {
                    if(tv.ThingDef == null)
                    {
                        Log.Error("Error in " + def.defName + ": defName '" + tv.defName + "' does not exist.");
                    }
                }
                if(tv.IsPawnKindDef && !pawnList.Contains(tv.PawnKindDef))
                {
                    pawnList.Add(tv.PawnKindDef);
                }
                else if (!defList.Contains(tv.ThingDef))
                {
                    defList.Add(tv.ThingDef);
                }else
                if(defList.Contains(tv.ThingDef) || pawnList.Contains(tv.PawnKindDef))
                {
                    Log.Error("targetList in TargetSettings of " + def.defName + " contains unnecessary duplicate defs named '" + tv.defName + "'. Use the value to adjust the amount.");
                }
            }
        }
    }
}
