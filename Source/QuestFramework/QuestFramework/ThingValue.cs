using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Verse;
using RimWorld;
using System.Text.RegularExpressions;

namespace StoryFramework
{
    public sealed class ThingValue : IExposable
    {
        public ThingDef Stuff;
        public QualityCategory QualityCategory = QualityCategory.Normal;
        public string defName = null;
        public int value = 1;
        public float chance = 1f;
        public bool CustomQuality = false;

        public ThingValue()
        {
        }

        public void ExposeData()
        {
            Scribe_Defs.Look(ref Stuff, "Stuff");
            Scribe_Values.Look(ref QualityCategory, "qc");
            Scribe_Values.Look(ref defName, "defName");
            Scribe_Values.Look(ref value, "value");
            Scribe_Values.Look(ref chance, "chance");
            Scribe_Values.Look(ref CustomQuality, "cq");
        }

        public bool IsPawnKindDef
        {
            get
            {
                return DefDatabase<PawnKindDef>.GetNamedSilentFail(defName) != null;
            }
        }

        public ThingDef ResolvedStuff
        {
            get
            {
                if (ThingDef.MadeFromStuff)
                {
                    if (Stuff != null)
                    {
                        return Stuff;
                    }
                    return GenStuff.DefaultStuffFor(ThingDef);
                }
                return null;
            }
        }

        public ThingDef ThingDef
        {
            get
            {
                ThingDef def = DefDatabase<ThingDef>.GetNamedSilentFail(defName);
                if (def == null)
                {
                    if (PawnKindDef != null)
                    {
                        def = DefDatabase<ThingDef>.GetNamedSilentFail(PawnKindDef.race.defName);
                    }
                }
                return def;
            }
        }

        public PawnKindDef PawnKindDef
        {
            get
            {
                return DefDatabase<PawnKindDef>.GetNamedSilentFail(defName);
            }
        }

        public bool ThingFits(Thing thing)
        {
            if (Stuff != null && thing.Stuff != Stuff)
            {
                return false;
            }
            if (CustomQuality && !(thing.TryGetQuality(out QualityCategory qc) && qc == QualityCategory))
            {
                return false;
            }
            return true;
        }

        public void LoadDataFromXmlCustom(XmlNode xmlRoot)
        {
            string Child = Regex.Replace(xmlRoot.FirstChild.Value, @"\s+", "");
            string ThingValueString = AdjustString(Child, out string ThingConditionString);
            if (ThingConditionString.NullOrEmpty())
            {
                if (ThingValueString.Contains(','))
                {
                    string[] array = ThingValueString.Split(',');
                    this.defName = array[0];
                    this.value = (int)ParseHelper.FromString(array[1], typeof(int));
                    if (array.Count() == 3)
                    {
                        this.chance = (float)ParseHelper.FromString(array[2], typeof(float));
                    }
                    return;
                }
                this.defName = ThingValueString;
            }
            else
            {
                ReadThingCondition(ThingConditionString, out defName);
                if (!ThingValueString.NullOrEmpty())
                {
                    if (ThingValueString.Contains(','))
                    {
                        string[] array = ThingValueString.Split(',');
                        this.value = (int)ParseHelper.FromString(array[0], typeof(int));
                        this.chance = (float)ParseHelper.FromString(array[1], typeof(float));
                        return;
                    }
                    this.value = (int)ParseHelper.FromString(ThingValueString, typeof(int));
                }
            }
        }

        private string AdjustString(string s, out string condition)
        {
            condition = "";
            if (s.Contains('('))
            {
                int from = s.IndexOf('(') +1;
                int to = s.IndexOf(')');
                condition = s.Substring(from, to - from);
                if (s.Length > to + 2)
                {
                    s = s.Substring(to + 2);
                }
                return "";
            }
            return s;
        }

        private void ReadThingCondition(string s, out string defName)
        {
            defName = null;
            if (s.Contains(","))
            {
                string[] array = s.Split(',');
                defName = array[0];
                QualityCategory = (QualityCategory)ParseHelper.FromString(array[1], typeof(QualityCategory));
                CustomQuality = true;
                if (s.Where(c => c == ',').Count() == 2)
                {
                    DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "Stuff", array[2]);
                }
                return;
            }
            defName = s;
        }

        public string Summary
        {
            get
            {
                return defName + QualityCategory + Stuff + value + chance;
            }
        }

        public override string ToString()
        {
            string defName = this.defName;
            string quality = QualityCategory.ToString();
            string stuff = Stuff?.defName;
            if(quality.NullOrEmpty() && stuff.NullOrEmpty())
            {
                return defName + "," + value + "," + chance;
            }
            return "(" + defName + "," + QualityCategory + "," + Stuff + ")," + value + "," + chance;
        }

        public override int GetHashCode()
        {
            return this.ToString().GetHashCode() + this.value << 16;
        }
    }
}
