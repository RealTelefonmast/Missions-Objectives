using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using System.Xml;

namespace MissionsAndObjectives
{
    public sealed class ThingDistancer
    {
        public ThingDef def;
        public int distance;

        public ThingDistancer()
        {
        }

        public ThingDistancer(ThingDef def, int distance)
        {
            this.def = def;
            this.distance = distance;
        }

        public string Summary
        {
            get
            {
                return this.distance + "x " + ((this.def == null) ? "null" : this.def.label);
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
            DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "distance", array[1]);
        }

        public override string ToString()
        {
            return this.def.ToString() + "," + this.distance.ToString();
        }

        public override int GetHashCode()
        {
            return (int)this.def.shortHash + this.distance << 16;
        }
    }
}
