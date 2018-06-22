using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace MissionsAndObjectives
{
    public class ModContentPackWrapper : IExposable
    {
        public string packName;

        public bool toggled = true;

        public MissionControlDef MCD;

        public ModContentPackWrapper()
        {
        }

        public ModContentPackWrapper(string packName)
        {
            this.packName = packName;
            if(MCD == null)
            {
                MCD = MCP.AllDefs.ToList().Find(def => def is MissionControlDef) as MissionControlDef;
            }
        }

        public void Toggle()
        {
            toggled = !toggled;
        }

        public ModContentPack MCP
        {
            get
            {
                return LoadedModManager.RunningMods.ToList().Find(mcp => mcp.Identifier == packName);
            }
        }

        public void ExposeData()
        {
            Scribe_Defs.Look(ref MCD, "MCD");
            Scribe_Values.Look(ref packName, "packName");
            Scribe_Values.Look(ref toggled, "toggled");
        }
    }
}
