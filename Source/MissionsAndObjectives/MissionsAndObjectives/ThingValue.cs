using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using UnityEngine;
using System.Xml;
using System.Collections;

namespace MissionsAndObjectives
{
    public sealed class ThingValue
    {
        private string defName;
        public int value = 1;     

        public ThingValue()
        {
        }

        public ThingValue(string defName, int value)
        {
            this.defName = defName;
            this.value = value;
        }

        public string Summary
        {
            get
            {
                return this.value + " Value For " + ((this.defName == null) ? "null" : this.defName);
            }
        }

        public ThingDef ThingDef
        {
            get
            {
                ThingDef def = DefDatabase<ThingDef>.GetNamed(defName, false);
                if(def == null)
                {
                    def = DefDatabase<ThingDef>.GetNamed(PawnKindDef.race.defName, false);
                }
                return def;
            }
        }

        public PawnKindDef PawnKindDef
        {
            get
            {
                return DefDatabase<PawnKindDef>.GetNamed(defName, false);
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
                this.defName = array[0];
                this.value = (int)ParseHelper.FromString(array[1], typeof(int));
                return;
            }
            this.defName = s;
        }

        public override string ToString()
        {
            return this.defName + "," + this.value.ToString();
        }

        public override int GetHashCode()
        {
            return (int)this.defName.GetHashCode() + this.value << 16;
        }
    }
}
