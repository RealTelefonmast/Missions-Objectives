using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI;
using UnityEngine;

namespace MissionsAndObjectives
{
    public class Objective : IExposable, IDisposable
    {
        public ObjectiveDef def;

        public Mission parent;

        public bool finishedOnce = false;

        public bool failedOnce = false;

        public bool startedOnce = false;

        private float progress;

        private int timer;

        public ThingTracker thingTracker;

        public Objective()
        {
        }

        public Objective(Mission parent)
        {
            this.parent = parent;
        }

        public Objective(ObjectiveDef def, Mission parent)
        {
            this.def = def;
            this.parent = parent;
            this.timer = def.TimerTicks;
            if(thingTracker == null)
            {
                thingTracker = new ThingTracker(def.targets, def.objectiveType, def.anyTarget);
            }
        }

        public void ExposeData()
        {
            Scribe_Defs.Look(ref def, "def");
            Scribe_Values.Look(ref finishedOnce, "finishedOnce");
            Scribe_Values.Look(ref failedOnce, "failedOnce");
            Scribe_Values.Look(ref startedOnce, "startedOnce");
            Scribe_Values.Look(ref progress, "progress");
            Scribe_Values.Look(ref timer, "timer");
            Scribe_Deep.Look(ref thingTracker, "killTracker", new object[] {
                def.targets,
                def.objectiveType,
                def.anyTarget
            });
        }

        public void Dispose()
        {
            this.parent = null;
            this.def = null;
            this.thingTracker = null;
        }

        // bool Getters
        public bool CanDoObjective(Pawn pawn, Thing thing)
        {
            if (!Active || def.IsManualJob || Failed || Finished || pawn == null)
            {
                return false;
            }
            if (!pawn.CanReserveAndReach(thing, PathEndMode.Touch, Danger.Some))
            {
                return false;
            }
            if (!def.stationDefs.NullOrEmpty() && thing != null && !def.stationDefs.Contains(thing.def))
            {
                return false;
            }
            if (!def.targets.NullOrEmpty() && thing != null && !def.targets.Any(tv => tv.ThingDef == thing.def))
            {
                return false;
            }
            if (!def.skillRequirements.NullOrEmpty() && !def.skillRequirements.All(sr => sr.PawnSatisfies(pawn)))
            {
                return false;
            }
            if (def.objectiveType == ObjectiveType.Examine || def.objectiveType == ObjectiveType.Research)
            {
                return true;
            }
            return false;
        }

        public bool Active
        {
            get
            {
                return !parent.Failed && Visible;
            }
        }

        public bool Visible
        {
            get
            {
                if (Finished)
                {
                    return !def.hideOnComplete;
                }
                if (GetTimer == def.TimerTicks)
                {
                    return RequisitesComplete;
                }
                return true;
            }
        }

        public bool RequiresInactive
        {
            get
            {
                if (!def.objectiveRequisites.NullOrEmpty())
                {
                    return def.objectiveRequisites.Any(o => !parent.parent.AllObjectives.Find(o2 => o2.def == o).Visible);
                }
                return false;
            }
        }

        public bool CanNeverActivate
        {
            get
            {
                if (!def.objectiveRequisites.NullOrEmpty())
                {
                    return def.objectiveRequisites.Any(o => parent.parent.AllObjectives.Find(o2 => o2.def == o).Failed);
                }
                return false;
            }
        }

        public bool RequisitesComplete
        {
            get
            {
                if (!def.objectiveRequisites.NullOrEmpty())
                {
                    return def.objectiveRequisites.All(o => parent.parent.AllObjectives.Find(o2 => o2.def == o).Finished);
                }
                return true;
            }
        }

        public bool Finished
        {
            get
            {
                if (failedOnce)
                {
                    return false;
                }
                if (def.workAmount > 0)
                {
                    if (GetProgress >= def.workAmount)
                    {
                        return true;
                    }
                }
                if(def.objectiveType == ObjectiveType.Destroy || def.objectiveType == ObjectiveType.Hunt)
                {
                    return thingTracker.AllDestroyedKilled;
                }
                if (def.objectiveType == ObjectiveType.Discover)
                {
                    return thingTracker.AllDiscovered;
                }
                if (def.objectiveType == ObjectiveType.Construct || def.objectiveType == ObjectiveType.Craft)
                {
                    if (thingTracker.AllMade)
                    {
                        return true;
                    };
                }
                if (def.objectiveType == ObjectiveType.Wait)
                {
                    return GetTimer == 0;
                }
                return false;
            }
        }

        public bool Failed
        {
            get
            {
                if (WorldComponent_Missions.MissionHandler.Missions.Any(m => m.Objectives.Any(o => o.Finished && def.failOn.Contains(o.def))))
                {
                    return true;
                }
                if (def.TimerTicks > 0 && def.objectiveType != ObjectiveType.Wait && GetTimer <= 0)
                {
                    return true;
                }
                return false;
            }
        }

        // int Getters

        public int GetTimer
        {
            get
            {
                return timer;
            }
        }

        public string GetTimerText
        {
            get
            {
                string label = "";
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
                if (this.Finished)
                {
                    label = "---";
                }
                return label;
            }
        }

        // float Getters

        public float GetProgress
        {
            get
            {
                return progress;
            }
        }

        public float ProgressPct
        {
            get
            {
                return GetProgress / def.workAmount;
            }
        }

        // void Methods

        public void PassTime(int passTicks)
        {
            timer = (int)Mathf.Max(timer - passTicks, 0f);
        }

        public void DoWork(float workAmount)
        {
            progress = Mathf.Min(GetProgress + workAmount, def.workAmount);
        }

        public void Notify_Start()
        {
            if (!startedOnce)
            {
                //Messages.Message("FinishedObjective".Translate() + ": " + def.LabelCap, MessageTypeDefOf.PositiveEvent);
                foreach (IncidentProperties props in def.incidentsOnStart)
                {
                    props.Notify_Execute(Find.AnyPlayerHomeMap, thingTracker.target);
                }
            }
            startedOnce = true;
        }

        public void Notify_Finish()
        {
            if (!finishedOnce)
            {
                Messages.Message("FinishedObjective".Translate() + ": " + def.LabelCap, MessageTypeDefOf.PositiveEvent);
                foreach (IncidentProperties props in def.incidentsOnCompletion)
                {
                    props.Notify_Execute(Find.AnyPlayerHomeMap, thingTracker.target);
                }
            }
            finishedOnce = true;
        }

        public void Notify_Fail()
        {
            if (!failedOnce)
            {
                Messages.Message("FailedObjective".Translate() + ": " + def.LabelCap, MessageTypeDefOf.NegativeEvent);
                foreach (IncidentProperties props in def.incidentsOnFail)
                {
                    props.Notify_Execute(Find.AnyPlayerHomeMap, thingTracker.target);                  
                }
            }
            failedOnce = true;
        }
    }
}
