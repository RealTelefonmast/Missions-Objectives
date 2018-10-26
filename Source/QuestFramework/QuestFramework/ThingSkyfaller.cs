using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using RimWorld;
using Verse;

namespace StoryFramework
{
    public class ThingSkyfaller : Editable
    {
        public ThingDef def;
        public ThingDef skyfaller;
        public float chance = 1f;

        public override IEnumerable<string> ConfigErrors()
        {
            if(def != null && skyfaller == null)
            {
                yield return "Skyfaller listing in spawnSettings is missing second part for def: " + def;
            }
        }

        public void LoadDataFromXmlCustom(XmlNode xmlRoot)
        {
            DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "def", xmlRoot.Name);
            string Child = Regex.Replace(xmlRoot.FirstChild.Value, @"\s+", "");
            string[] array = Child.Split(new char[]
            {
                ','
            });
            DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "skyfaller", array[0]);
            if(array.Count() == 2)
            {
                chance = (float)ParseHelper.FromString(array[1], typeof(float));
            }
        }
    }
}
