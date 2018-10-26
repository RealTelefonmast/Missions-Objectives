using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace StoryFramework
{
    public class ThingTracker : IExposable
    {
        private ObjectiveType type = ObjectiveType.None;
        private TargetSettings targetSettings;
        private List<int> tempInts = new List<int>();
        public Dictionary<ThingValue, int> TargetsDone = new Dictionary<ThingValue, int>();
        private Dictionary<Thing, int> CurrentlyOwnedTargets = new Dictionary<Thing, int>();
        private List<Pawn> trackedPawns = new List<Pawn>();
        private int count = 0;
        public TargetInfo LastTarget;

        public ThingTracker(){ }

        public ThingTracker(TargetSettings settings)
        {
            targetSettings = settings;
        }

        public ThingTracker(TargetSettings settings, ObjectiveType type)
        {
            targetSettings = settings;
            this.type = type;
            Reset();
        }

        public void ExposeData()
        {
            if (!TargetsDone.Values.ToList().NullOrEmpty())
            {
                tempInts = TargetsDone.Values.ToList();
            }
            Scribe_Values.Look(ref type, "type");
            Scribe_Values.Look(ref count, "count");
            Scribe_Collections.Look(ref tempInts, "tempInts", LookMode.Value);
            Scribe_Collections.Look<Thing, int>(ref CurrentlyOwnedTargets, "TempOwned", LookMode.Deep, LookMode.Value);
            Scribe_Collections.Look(ref trackedPawns, "trackedPawns", LookMode.Value);
            Scribe_TargetInfo.Look(ref LastTarget, "lastTarget");
            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                for (int i = 0; i < targetSettings.targets.Count; i++)
                {
                    TargetsDone.Add(targetSettings.targets[i], tempInts[i]);
                }
            }
        }

        public bool WorksWithPawns
        {
            get
            {
                return type == ObjectiveType.Kill || type == ObjectiveType.Recruit;
            }
        }

        public int GetTargetCount
        {
            get
            {
                if(targetSettings.pawnSettings != null)
                {
                    return trackedPawns.Where(p => !p.DestroyedOrNull()).Count() + count;
                }
                if(targetSettings.thingSettings != null)
                {
                    return CurrentlyOwnedTargets.Where(t => !t.Key.DestroyedOrNull()).Count() + count;
                }
                return TargetsDone.Sum(tv => tv.Value);
            }
        }

        public int GetNeedCount(ThingDef def)
        {
            ThingValue thingvalue = targetSettings.targets.Find(tv => tv.ThingDef == def);
            if (thingvalue != null)
            {
                return thingvalue.value;
            }
            return 0;
        }

        public bool AllDone
        {
            get
            {
                if(targetSettings.pawnSettings != null)
                {
                    return GetTargetCount >= targetSettings.pawnSettings.minAmount;
                }
                if(targetSettings.thingSettings != null)
                {
                    return GetTargetCount >= targetSettings.thingSettings.minAmount;
                }
                if (targetSettings.any)
                {
                    return targetSettings.targets.Any(tv => TargetsDone[tv] >= tv.value);
                }
                return GetTargetCount >= TargetsDone.Sum(tv => tv.Key.value);
            }
        }

        public void Reset()
        {
            TargetsDone.Clear();
            CurrentlyOwnedTargets.Clear();
            foreach (ThingValue tv in targetSettings.targets)
            {
                TargetsDone.Add(tv, 0);
            }
        }

        public void ConsumeTargets()
        {
            trackedPawns.ForEach(p => p.DeSpawn());
            for(int i = 0; i < CurrentlyOwnedTargets.Count; i++)
            {
                Thing thing = CurrentlyOwnedTargets.ElementAt(i).Key;
                CurrentlyOwnedTargets.Remove(thing);
                thing.Destroy();
            }
        }

        public bool ResolveButtonInput(Rect rect)
        {
            bool result = false;
            if (type == ObjectiveType.Own || type == ObjectiveType.PawnCheck)
            {
                result = true;
                if (Widgets.ButtonInvisible(rect))
                {
                    SoundDefOf.Click.PlayOneShotOnCamera(null); 
                    Map map = Find.CurrentMap;
                    if (type == ObjectiveType.Own)
                    {
                        List<Thing> tempList = new List<Thing>();
                        List<SlotGroup> slots = map.haulDestinationManager.AllGroupsListForReading;
                        foreach (SlotGroup slot in slots)
                        {
                            tempList.AddRange(slot.HeldThings);
                        }
                        CheckOwnedItems(tempList);
                    }
                    if (type == ObjectiveType.PawnCheck)
                    {
                        UpdatePawnCheck(map);
                    }
                }
            }
            return result;
        }

        public void CheckOwnedItems(List<Thing> things)
        {
            for(int i = 0; i < things.Count; i++)
            {
                if (targetSettings.thingSettings != null)
                {
                    if (things[i].ThingIsValid(targetSettings.thingSettings))
                    {
                        if (!CurrentlyOwnedTargets.TryGetValue(things[i], out int lastStack))
                        {
                            CurrentlyOwnedTargets.Add(things[i], things[i].stackCount);
                        }
                    }
                }
                else
                {
                    Thing thing = things[i];
                    thing = thing.GetInnerIfMinified();
                    ThingDef def = thing.def;
                    ThingValue tv = TargetsDone.ThingValue(def);
                    if (tv != null && TargetsDone[tv] < tv.value)
                    {
                        int value = 0;
                        bool flag = CurrentlyOwnedTargets.TryGetValue(thing, out int lastStack);
                        value = flag ? thing.stackCount - lastStack : thing.stackCount;
                        int total = TargetsDone[tv] + value;
                        if (total > tv.value)
                        {
                            int excess = total - tv.value;
                            value = value - excess;
                        }
                        TargetsDone[tv] += value;
                        if (!flag)
                        {
                            CurrentlyOwnedTargets.Add(thing, thing.stackCount);
                        }
                        else
                        {
                            CurrentlyOwnedTargets[thing] = thing.stackCount;
                        }
                    }
                }
            }
            for (int i = 0; i < CurrentlyOwnedTargets.Count; i++)
            {
                Thing thing = CurrentlyOwnedTargets.ElementAt(i).Key;
                if (!things.Contains(thing))
                {
                    ThingValue tv = TargetsDone.ThingValue(thing.def);
                    int value = CurrentlyOwnedTargets[thing];
                    int total = TargetsDone[tv] - value;
                    if (total < 0)
                    {
                        int excess = total;
                        value = value + excess;
                    }
                    TargetsDone[tv] -= GetCount(value, tv, CurrentlyOwnedTargets.Keys.ToList());
                    CurrentlyOwnedTargets.Remove(thing);
                }
            }
        }

        private int GetCount(int value, ThingValue thingValue, List<Thing> things)
        {
            IEnumerable<Thing> listOfDef = things.Where(t => t.def == thingValue.ThingDef);
            if (listOfDef.Count() > 1)
            {
                int sum = CurrentlyOwnedTargets.Sum(t => t.Value);
                if (sum > thingValue.value && sum - value < thingValue.value)
                {
                    return thingValue.value - (sum - value);
                }
            }
            return value;
        }

        public void UpdatePawnCheck(Map map)
        {
            foreach (Pawn pawn in map.mapPawns.AllPawns)
            {
                if (!trackedPawns.Contains(pawn))
                {
                    if (targetSettings.pawnSettings.PawnSatisfies(pawn))
                    {
                        trackedPawns.Add(pawn);
                    }
                }
            }
        }

        public void ProcessTarget<T>(T t, IntVec3 cell, Map map, ObjectiveType type, Thing craftedThing = null, Pawn pawn = null)
        {
            if (this.type == type)
            {
                if (targetSettings.thingSettings != null)
                {
                    if (craftedThing.ThingIsValid(targetSettings.thingSettings))
                    {
                        if (type == ObjectiveType.Destroy)
                        {
                            count++;
                        }
                        else
                        if (!CurrentlyOwnedTargets.TryGetValue(craftedThing, out int value))
                        {
                            CurrentlyOwnedTargets.Add(craftedThing, craftedThing.stackCount);
                            LastTarget = craftedThing;
                        }
                    }
                }
                else if (targetSettings.pawnSettings != null)
                {
                    if(pawn != null)
                    {
                        if (targetSettings.pawnSettings.PawnSatisfies(pawn))
                        {
                            if (type == ObjectiveType.Kill)
                            {
                                count++;
                            }else
                            if (!trackedPawns.Contains(pawn))
                            {
                                trackedPawns.Add(pawn);
                                LastTarget = craftedThing;
                            }
                        }
                    }        
                }
                else
                {
                    ThingValue tv = TargetsDone.ThingValue(t);
                    if (tv != null && TargetsDone[tv] < tv.value && tv.ThingFits(craftedThing))
                    {
                        TargetsDone[tv] += 1;
                        LastTarget = new TargetInfo(cell, map, true);
                    }
                }
            }
        }
    }
}
