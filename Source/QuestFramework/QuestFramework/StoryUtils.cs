using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using Verse;
using System;
using RimWorld;
using System.Text.RegularExpressions;

namespace StoryFramework
{
    [StaticConstructorOnStartup]
    public static class StoryUtils
    {
        public static Rect IconLabel(this Listing_Standard listing, Texture2D tex, string label, Vector2 dimensions)
        {
            Rect rect = listing.GetRect(Text.CalcHeight(label, listing.ColumnWidth - dimensions.x));
            GUI.DrawTexture(new Rect(rect.x, rect.y, dimensions.x, dimensions.y), tex);
            Widgets.Label(new Rect(rect.x + dimensions.x, rect.y, rect.width - 29f, rect.height), label);
            listing.Gap(listing.verticalSpacing);
            return rect;
        }

        public static bool ButtonLabel(this Listing_Standard listing, string label, string highlightTag = null)
        { 
            Rect rect = listing.GetRect(30f);
            Widgets.DrawHighlightIfMouseover(rect);
            Widgets.Label(rect, label);
            bool result = Widgets.ButtonInvisible(rect);
            if (highlightTag != null)
            {
                UIHighlighter.HighlightOpportunity(rect, highlightTag);
            }
            listing.Gap(listing.verticalSpacing);
            return result;
        }

        public static List<ThingDef> ThingDefs(this List<ThingSkyfaller> skyfallers)
        {
            List<ThingDef> defs = new List<ThingDef>();
            foreach(ThingSkyfaller skyfaller in skyfallers)
            {
                defs.Add(skyfaller.innerThing);
            }
            return defs;
        }

        public static bool ThingFitsAt(this ThingDef thing, Map map, IntVec3 cell)
        {
            CellRect.CellRectIterator iterator = GenAdj.OccupiedRect(cell, Rot4.North, thing.size).GetIterator();
            while (!iterator.Done())
            {
                IntVec3 c = iterator.Current;
                if (!c.InBounds(map) || c.Fogged(map) || !c.Standable(map) || (c.Roofed(map) && c.GetRoof(map).isThickRoof))
                {
                    return false;
                }
                iterator.MoveNext();
            }
            return true;
        }

        public static List<Thing> AllThings(this IEnumerable<IntVec3> list, Map map)
        {
            List<Thing> things = new List<Thing>();
            foreach(IntVec3 c in list)
            {
                things.AddRange(c.GetThingList(map));
            }
            return things;
        }

        public static bool ThingIsValid(this Thing thing, ThingSettings s)
        {
            if(thing == null)
            {
                return false;
            }
            if (!thing.def.MadeFromStuff || (s.stuff != null && thing.Stuff != s.stuff))
            {
                return false;
            }
            if (s.minQuality != null && !(thing.TryGetQuality(out QualityCategory qc) && qc >= s.minQuality))
            {
                return false;
            }
            if (!s.tradeTags.NullOrEmpty() && (!thing.def.tradeTags?.Any(s.tradeTags.Contains) ?? true))
            {
                return false;
            }
            if (!s.weaponTags.NullOrEmpty() && (!thing.def.weaponTags?.Any(s.weaponTags.Contains) ?? true))
            {
                return false;
            }
            if (!s.techHediffsTags.NullOrEmpty() && (!thing.def.techHediffsTags?.Any(s.techHediffsTags.Contains) ?? true))
            {
                return false;
            }
            return true;
        }

        public static ThingValue ThingValue<T>(this Dictionary<ThingValue, int> pairs, T t)
        {
            foreach(ThingValue thingValue in pairs.Keys)
            {
                if (t is ThingDef)
                {
                    if (thingValue.ThingDef == t as ThingDef)
                    {
                        return thingValue;
                    }
                }
                else if(t is PawnKindDef)
                {
                    if (thingValue.PawnKindDef == t as PawnKindDef)
                    {
                        return thingValue;
                    }
                }
            }
            return null;
        }

        public static ObjectiveStation Station(this List<ObjectiveStation> list, ThingDef thingDef)
        {
            foreach (ObjectiveStation station in list)
            {
                if (station.station.def == thingDef)
                {
                    return station;
                }
            }
            return null;
        }

        public static ObjectiveStation Station(this List<ObjectiveStation> list, ObjectiveDef objective = null, Thing thing = null)
        {
            foreach (ObjectiveStation station in list)
            {
                if (station.objectives.Contains(objective) || station.station == thing)
                {
                    return station;
                }
            }
            return null;
        }

        public static List<ObjectiveDef> StationObjectives(this List<ObjectiveStation> list, Thing thing)
        {
            if (!list.NullOrEmpty())
            {
                foreach (ObjectiveStation station in list)
                {
                    if (station.station == thing)
                    {
                        return station.objectives;
                    }
                }
            }
            return new List<ObjectiveDef>();
        }

        public static List<Thing> Stations(this List<ObjectiveStation> list)
        {
            List<Thing> things = new List<Thing>();
            foreach (ObjectiveStation station in list)
            {
                things.Add(station.station);
            }
            return things;
        }

        public static List<ObjectiveDef> Objectives(this List<ObjectiveStation> list)
        {
            List<ObjectiveDef> objectives = new List<ObjectiveDef>();
            foreach (ObjectiveStation station in list)
            {
                objectives.AddRange(station.objectives);
            }
            return objectives;
        }

        public static List<ThingDef> AllThingDefs(this List<ThingValue> list)
        {
            List<ThingDef> defs = new List<ThingDef>();
            foreach(ThingValue tv in list)
            {
                defs.Add(tv.ThingDef);
            }
            return defs;
        }

        public static string GetTimerText(int ticks, MOState state)
        {
            string label = "";
            int timer = ticks;
            if (state == MOState.Finished || state == MOState.Failed || ticks == 0)
            {
               return label = "---";
            }
            if (timer > GenDate.TicksPerYear)
            {
                label = Math.Round((decimal)timer / GenDate.TicksPerYear, 1) + "y";
            }
            else if (timer > GenDate.TicksPerDay)
            {
                label = Math.Round((decimal)timer / GenDate.TicksPerDay, 1) + "d";
            }
            else if (timer < GenDate.TicksPerDay)
            {
                label = Math.Round((decimal)timer / GenDate.TicksPerHour, 1) + "h";
            }
            return label;
        }

        public static string GetModNameFromMission(MissionDef def)
        {
            return LoadedModManager.RunningMods.Where(mcp => mcp.AllDefs.Contains(def)).RandomElement().Identifier;
        }

        public static void DrawMenuSectionColor(Rect rect, int thiccness, ColorInt colorBG, ColorInt colorBorder)
        {
            GUI.color = colorBG.ToColor;
            GUI.DrawTexture(rect, BaseContent.WhiteTex);
            GUI.color = colorBorder.ToColor;
            Widgets.DrawBox(rect, thiccness);
            GUI.color = Color.white;
        }
    }
}
