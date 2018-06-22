using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using System.Xml;

namespace MissionsAndObjectives
{
    public sealed class ThingValue
    {
        public ThingDef def;
        public int value = 1;

        public ThingValue()
        {
        }

        public ThingValue(ThingDef def, int value)
        {
            this.def = def;
            this.value = value;
        }

        public string Summary
        {
            get
            {
                return this.value + " Value For " + ((this.def == null) ? "null" : this.def.label);
            }
        }

        public void LoadDataFromXmlCustom(XmlNode xmlRoot)
        {        
            string s = xmlRoot.FirstChild.Value;
            if (s.Contains(","))
            {
                string[] array = s.Split(new char[]
                    {
                    ','
                    });
                DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "def", array[0]);
                this.value = (int)ParseHelper.FromString(array[1], typeof(int));
                return;
            }
            DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "def", s);
        }

        public override string ToString()
        {
            return this.def.ToString() + "," + this.value.ToString();
        }

        public override int GetHashCode()
        {
            return (int)this.def.shortHash + this.value << 16;
        }
    }
}
