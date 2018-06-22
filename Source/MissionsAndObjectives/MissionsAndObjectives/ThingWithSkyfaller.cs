using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using System.Xml;

namespace MissionsAndObjectives
{
    public sealed class ThingWithSkyfaller
    {
        public ThingDef def;
        public ThingDef skyfaller;

        public ThingWithSkyfaller()
        {
        }

        public ThingWithSkyfaller(ThingDef def, ThingDef skyfaller)
        {
            this.def = def;
            this.skyfaller = skyfaller;
        }

        public string Summary
        {
            get
            {
                return this.skyfaller + "x " + ((this.def == null) ? "null" : this.def.label);
            }
        }

        public void LoadDataFromXmlCustom(XmlNode xmlRoot)
        {
            string s = xmlRoot.FirstChild.Value;
            string[] array = s.Split(new char[]
            {
                ','
            });
            DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "def", array[0]);
            DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "skyfaller", array[1]);
        }

        public override string ToString()
        {
            return this.def.ToString() + "," + this.skyfaller.ToString();
        }

        public override int GetHashCode()
        {
            return (int)this.def.shortHash + this.skyfaller.shortHash;
        }
    }
}
