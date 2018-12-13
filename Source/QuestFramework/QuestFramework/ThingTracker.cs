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
        private List<Thing> trackedThings = new List<Thing>();
        private int count = 0;
        public TargetInfo LastTarget;

        public ThingTracker() { }

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
            Scribe_Collections.Look(ref trackedPawns, "trackedPawns", LookMode.Reference);
            Scribe_Collections.Look(ref trackedThings, "trackedThings", LookMode.Reference);
            Scribe_TargetInfo.Look(ref LastTarget, "lastTarget");
            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                for (int i = 0; i < targetSettings.targets.Count; i++)
                {
                    TargetsDone.Add(targetSettings.targets[i], tempInts[i]);
                }
            }
        }

        public void TrackerTick()
        {
            TargetsDone.RemoveAll(t => t.Key == null);
            trackedPawns.RemoveAll(t => !t.Spawned);
            trackedThings.RemoveAll(t => !t.Spawned);
            CurrentlyOwnedTargets.RemoveAll(t => !t.Key.Spawned);
        }

        public bool WorksWithPawns
        {
            get
            {
                return type == ObjectiveType.Kill || type == ObjectiveType.Recruit;
            }
        }

        public int GetTotalNeededCount
        {
            get
            {
                int count = 0;
                if (targetSettings.pawnSettings != null)
                {
                    count += targetSettings.pawnSettings.minAmount;
                }
                if (targetSettings.thingSettings != null)
                {
                    count += targetSettings.thingSettings.minAmount;
                }
                return targetSettings.targets.Sum(t => t.value) + count;
            }
        }

        public int GetThingCount
        {
            get
            {
                if(type == ObjectiveType.ConstructOrCraft)
                {
                    return count;
                }
                return trackedThings.Where(t => !t.DestroyedOrNull()).Count();
            }
        }

        public int GetPawnCount
        {
            get
            {
                return trackedPawns.Where(p => !p.DestroyedOrNull()).Count();
            }
        }

        public int GetTargetCount
        {
            get
            {
                return TargetsDone.Sum(tv => tv.Value);
            }
        }

        public int GetTotalCount
        {
            get
            {
                int count = 0;
                count += GetThingCount;
                count += GetPawnCount;
                return GetTargetCount + count + this.count;
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

        public int CurrentTrackedPawns
        {
            get
            {
                return trackedPawns.Where(p => !p.DestroyedOrNull()).Count();
            }
        }

        public int CurrentTrackedThings
        {
            get
            {
                return trackedThings.Where(t => !t.DestroyedOrNull()).Count();
            }
        }

        public bool AllDone
        {
            get
            {                
                if (targetSettings.any)
                {
                    if (targetSettings.pawnSettings != null && GetTotalCount >= targetSettings.pawnSettings.minAmount)
                    {
                        return true;
                    }
                    if (targetSettings.thingSettings != null && GetTotalCount >= targetSettings.thingSettings.minAmount)
                    {
                        return true;
                    }
                    return targetSettings.targets.Any(tv => TargetsDone[tv] >= tv.value);
                }
                return GetTotalCount >= GetTotalNeededCount;
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
            for (int i = 0; i < CurrentlyOwnedTargets.Count; i++)
            {
                Thing thing = CurrentlyOwnedTargets.ElementAt(i).Key;
                CurrentlyOwnedTargets.Remove(thing);
                thing.Destroy();
            }
        }

        public bool ResolveButtonInput(Rect rect)
        {
            bool result = false;
            if (type == ObjectiveType.Own)
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
                }
            }
            return result;
        }

        public void CheckOwnedItems(List<Thing> things)
        {
            for (int i = 0; i < things.Count; i++)
            {
                if (targetSettings.thingSettings != null)
                {
                    if (things[i].ThingIsValid(targetSettings.thingSettings))
                    {
                        if (!trackedThings.Contains(things[i]) && trackedThings.Count < targetSettings.thingSettings.minAmount)
                        {
                            trackedThings.Add(things[i]);
                        }
                    }
                }

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

        public void ProcessTarget<T>(T t, IntVec3 cell, Map map, ObjectiveType type, Thing thing = null, Pawn pawn = null, PawnInfo pawnInfo = null)
        {
            PawnInfo info = pawnInfo != null ? pawnInfo : new PawnInfo(pawn); 
            if (this.type == type)
            {
                if (targetSettings.thingSettings != null)
                {
                    if (thing.ThingIsValid(targetSettings.thingSettings))
                    {
                        int min = targetSettings.thingSettings.minAmount;
                        if ((type == ObjectiveType.Destroy || type == ObjectiveType.ConstructOrCraft) && count < min)
                        {
                            count++;
                        }
                        else if (!trackedThings.Contains(thing) && trackedThings.Count < min)
                        {
                            trackedThings.Add(thing);
                            LastTarget = thing;
                        }
                    }
                }
                if (targetSettings.pawnSettings != null)
                {
                    if (info != null)
                    {
                        if (targetSettings.pawnSettings.PawnSatisfies(info))
                        {
                            int min = targetSettings.pawnSettings.minAmount;
                            if(pawn != null && !trackedPawns.Contains(pawn) && trackedPawns.Count < min)
                            {
                                
                                trackedPawns.Add(pawn);
                                LastTarget = pawn;
                            }
                            else if( count < min)
                            {
                                count++;
                            }
                        }
                    }
                }
                ThingValue tv = TargetsDone.ThingValue(t);
                if (tv != null && TargetsDone[tv] < tv.value && tv.ThingFits(thing))
                {
                    TargetsDone[tv] += 1;
                    LastTarget = new TargetInfo(cell, map, true);
                }
            }
        }
    }
}
