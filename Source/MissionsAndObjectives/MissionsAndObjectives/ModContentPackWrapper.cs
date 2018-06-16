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

        public ModContentPackWrapper()
        {
        }

        public ModContentPackWrapper(string packName)
        {
            this.packName = packName;
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref packName, "packName");
            Scribe_Values.Look(ref toggled, "toggled");
        }
    }
}
