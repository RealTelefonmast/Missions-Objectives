using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI;

namespace StoryFramework
{
    public class Objective : IExposable, ILoadReferenceable
    {
        public ObjectiveDef def;
        public Mission parentMission;
        public ThingTracker thingTracker;
        public TravelTracker travelTracker;
        private bool startedOnce = false;
        private bool finishedOnce = false;
        private bool failedOnce = false;
        private float workDone = 0;
        private int timer = -1;
        public MOState LatestState = MOState.Inactive;
        public TargetInfo lastTarget;

        public Objective(){}

        public Objective(Mission parent)
        {
            parentMission = parent;
        }

        public void SetUp(Mission parent)
        {
            this.parentMission = parent;
            timer = def.timer.GetTotalTime;
            if(thingTracker == null && def.targetSettings != null)
            {
                thingTracker = new ThingTracker(def.targetSettings, def.type);
            }
            if(travelTracker == null)
            {
                travelTracker = new TravelTracker(def.travelSettings);
            }
        }

        public void ExposeData()
        {
            Scribe_Defs.Look(ref def, "def");
            Scribe_Deep.Look(ref travelTracker, "travelTracker", new object[]
            {
                def.travelSettings
            });
            Scribe_Deep.Look(ref thingTracker, "thingTracker", new object[]
            {
                def.targetSettings
            });
            Scribe_Values.Look(ref LatestState, "LatestState");
            Scribe_References.Look(ref parentMission, "parent");
            Scribe_Values.Look(ref startedOnce, "startedOnce");
            Scribe_Values.Look(ref finishedOnce, "finishedOnce");
            Scribe_Values.Look(ref failedOnce, "failedOnce");
            Scribe_Values.Look(ref workDone, "workDone");
            Scribe_Values.Look(ref timer, "timer");
            Scribe_TargetInfo.Look(ref lastTarget, "lastTarget");
        }

        public string GetUniqueLoadID()
        {
            return def.defName + "Objective";
        }

        public void ObjectiveTick()
        {
            if (LatestState == MOState.Active)
            {
                Notify_Start();
                if (timer > 0)
                {
                    timer -= 1;
                }
                if (Find.TickManager.TicksGame % GenTicks.TickRareInterval == 0)
                {
                    if (def.type == ObjectiveType.Travel)
                    {
                        travelTracker.UpdateCaravans();
                    }
                    if (def.type == ObjectiveType.PawnCheck)
                    {
                        foreach (Map map in Find.Maps.Where(m => m.IsPlayerHome))
                        {
                            thingTracker.UpdatePawnCheck(map);
                        }
                    }
                    if (def.type == ObjectiveType.Own)
                    {
                        List<Thing> tempList = new List<Thing>();
                        List<Map> Maps = Find.Maps;
                        foreach (Map map in Maps)
                        {
                            List<SlotGroup> slots = map.haulDestinationManager.AllGroupsListForReading;
                            foreach (SlotGroup slot in slots)
                            {
                                tempList.AddRange(slot.HeldThings);
                            }
                        }
                        thingTracker.CheckOwnedItems(tempList);
                    }
                }
                if (ObjectiveComplete)
                {
                    Notify_Finish();
                    return;
                }
                if (ObjectiveFailed)
                {
                    Notify_Fail();
                    return;
                }
            }
            LatestState = CurrentState;
        }

        public void Notify_Start()
        {
            if (!startedOnce)
            {
                foreach (IncidentProperties props in def.incidentsOnStart)
                {
                    props.Notify_Execute(thingTracker?.LastTarget.Map ?? Find.AnyPlayerHomeMap, thingTracker?.LastTarget.IsValid ?? false ? thingTracker.LastTarget : lastTarget, def, IncidentCondition.Started);
                }
                startedOnce = true;
                OnStart();
            }
        }

        public void Notify_Finish()
        {
            if (!finishedOnce)
            {
                Map map = thingTracker?.LastTarget.Map ?? Find.AnyPlayerHomeMap;
                TargetInfo targetInfo = thingTracker?.LastTarget.IsValid ?? false ? thingTracker.LastTarget : lastTarget;
                def.result?.Notify_Execute(map, targetInfo, def, IncidentCondition.Finished);
                foreach (IncidentProperties props in this.def.incidentsOnCompletion)
                {
                    props?.Notify_Execute(map, targetInfo, def, IncidentCondition.Finished);
                }
                StoryManager.StoryHandler.AllStations.Station(this.def)?.objectives.Remove(this.def);
                Messages.Message("FinishedObjective_SMO".Translate() + ": " + def.LabelCap, MessageTypeDefOf.PositiveEvent);
                finishedOnce = true;
                if (def.targetSettings?.consume ?? false)
                {
                    thingTracker.ConsumeTargets();
                }
                OnFinish();
            }
        }

        public void Notify_Fail()
        {
            if (!failedOnce)
            {
                foreach (IncidentProperties props in def.incidentsOnFail)
                {
                    props.Notify_Execute(thingTracker?.LastTarget.Map ?? Find.AnyPlayerHomeMap, thingTracker?.LastTarget.IsValid ?? false ? thingTracker.LastTarget : lastTarget, def, IncidentCondition.Failed);
                }
                StoryManager.StoryHandler.AllStations.Station(this.def)?.objectives.Remove(this.def);
                Messages.Message("FailedObjective_SMO".Translate() + " " + def.LabelCap, MessageTypeDefOf.NegativeEvent);
                failedOnce = true;
                OnFail();
            }
        }

        //Fires once when objective starts
        public virtual void OnStart() { }
        //Fires once when objective finishes
        public virtual void OnFinish() { }
        //Fires once when objective fails
        public virtual void OnFail() { }

        //The main conditions for the Objective are virtual incase someone wants to make their own objective
        //This defines whether or not the objective is gonna tick
        public virtual bool Active
        {
            get
            {
                if (ObjectiveComplete)
                {
                    return !def.hideOnComplete;
                }
                if (startedOnce)
                {
                    return true;
                }
                if (parentMission.def.chronological)
                {
                    int index = parentMission.objectives.IndexOf(this);
                    if(index > 0)
                    {
                        if(parentMission.objectives[(index - 1)].CurrentState != MOState.Finished)
                        {
                            return false;
                        }
                    }                    
                }
                return def.requisites?.IsFulfilled() ?? true;
            }
        }

            //If this is true, the objective will be finished
        public virtual bool ObjectiveComplete
        {
            get
            {
                if (def.timer.GetTotalTime > 0 && def.timer.continueWhenFinished)
                {
                    if(timer > 0)
                    {
                        return false;
                    }
                }
                if (finishedOnce || parentMission.LatestState == MOState.Finished)
                {
                    return true;
                }
                switch (def.type)
                {
                    case ObjectiveType.ConstructOrCraft:
                    case ObjectiveType.Kill:
                    case ObjectiveType.Recruit:
                    case ObjectiveType.Destroy:
                    case ObjectiveType.Own:
                    case ObjectiveType.PawnCheck:
                        return thingTracker.AllDone;
                    case ObjectiveType.Research:
                        return FinishedWork;
                    case ObjectiveType.Wait:
                        return GetTimer == 0f;
                    case ObjectiveType.Travel:
                        return travelTracker.TravelComplete();

                }
                return false;
            }
        }
            
            //If this is true, the objective will be failed
            //Checks for all vanilla ObjectiveTypes' fail conditions
        public virtual bool ObjectiveFailed
        {
            get
            {
                if (finishedOnce)
                {
                    return false;
                }
                if(failedOnce || parentMission.LatestState == MOState.Failed || (def.requisites?.Impossible ?? false))
                {
                    return true;
                }
                if(def.type != ObjectiveType.Wait && def.timer.GetTotalTime > 0)
                {
                    return GetTimer <= 0;
                }           
                return false;
            }
        }

        public List<Pawn> CapablePawns
        {
            get
            {
                List<Pawn> pawns = new List<Pawn>();
                foreach(Map map in Find.Maps.Where(m => m.IsPlayerHome))
                {
                    pawns.AddRange(map.mapPawns.AllPawns.Where(p => p.IsColonist && ( def.skillRequirements.NullOrEmpty() || def.skillRequirements.All(sr => sr.PawnSatisfies(p)))));
                }
                return pawns;
            }
        }

        public int GetTimer
        {
            get
            {
                return timer;
            }
            set
            {
                timer = value;
            }
        }

        public bool FinishedWork
        {
            get
            {
                return workDone >= def.workAmount;
            }
        }

        public float GetWorkDone
        {
            get
            {
                return workDone;
            }
            set
            {
                workDone = value;
            }
        }

        public float GetWorkPct
        {
            get
            {
                return GetWorkDone / def.workAmount;
            }
        }

        public void PerformWork( float amount)
        {
            amount *= 0.009f;
            if (DebugSettings.fastResearch)
            {
                amount *= 500f;
            }
            workDone += amount;
        }

        public MOState CurrentState
        {
            get
            {
                MOState state = MOState.Inactive;
                if (ObjectiveFailed)
                {
                    state = MOState.Failed;
                }else
                if (ObjectiveComplete)
                {
                    state = MOState.Finished;
                }else
                if (Active)
                {
                    state = MOState.Active;
                }
                return state;
            }
        }
    }
}
