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
    public class Objective : IExposable
    {
        public ObjectiveDef def;

        public Mission parent;

        public bool finishedOnce = false;

        public bool failedOnce = false;

        private float progress;

        private int timer;

        public Objective()
        {
        }

        public Objective(ObjectiveDef def, Mission parent)
        {
            this.def = def;
            this.parent = parent;
            this.timer = def.TimerTicks;
        }

        public void ExposeData()
        {
            Scribe_Defs.Look(ref def, "def");
            Scribe_Deep.Look(ref parent, "parent");
            Scribe_Values.Look(ref finishedOnce, "finishedOnce");
            Scribe_Values.Look(ref failedOnce, "failedOnce");
            Scribe_Values.Look(ref progress, "progress");
            Scribe_Values.Look(ref timer, "timer");
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
            if (!def.skillRequirements.All(sr => pawn.skills.skills.Any(sr2 => sr2.def == sr.skillDef && sr2.levelInt >= sr.skillLevel)))
            {
                return false;
            }
            if (def.objectiveType == ObjectiveType.Hunt)
            {
                if (!TargetPawnsAvailable)
                {
                    return false;
                }
            }
            if (def.objectiveType == ObjectiveType.Examine)
            {
                if (!def.targetThings.NullOrEmpty() && thing != null && !def.targetThings.Contains(thing.def))
                {
                    return false;
                }
            }
            if (def.objectiveType == ObjectiveType.Examine)
            {

                if (!Find.AnyPlayerHomeMap.listerThings.AllThings.Any((Thing x) => def.targetThings.Contains(x.def)))
                {
                    return false;
                }
            }
            return true;
        }

        public bool Active
        {
            get
            {
                if (GetTimer == def.TimerTicks)
                {
                    return RequisitesComplete;
                }
                return true;
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

        public bool DependantComplete
        {
            get
            {
                if (!def.dependantOn.NullOrEmpty())
                {
                    return def.dependantOn.All(o => parent.parent.AllObjectives.Find(o2 => o2.def == o).Finished);
                }
                return false;
            }
        }

        public bool TargetThingsAvailable
        {
            get
            {
                return def.targetThings.All(def => !Find.AnyPlayerHomeMap.listerThings.ThingsOfDef(def).NullOrEmpty());
            }
        }

        public bool TargetPawnsAvailable
        {
            get
            {
                return def.targetPawns.All(def => !Find.AnyPlayerHomeMap.mapPawns.AllPawns.Any(pawn => pawn.kindDef == def));
            }
        }

        public bool Finished
        {
            get
            {
                if (def.workCost > 0)
                {
                    if (GetProgress >= def.workCost)
                    {
                        return true;
                    }
                }
                if (def.objectiveType == ObjectiveType.Construct || def.objectiveType == ObjectiveType.Craft)
                {
                    if (TargetThingsAvailable)
                    {
                        return true;
                    };
                }
                if (def.objectiveType == ObjectiveType.Wait)
                {
                    return GetTimer == 0;
                }
                if (DependantComplete)
                {
                    return true;
                }
                return false;
            }
        }

        public bool Failed
        {
            get
            {
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
                return GetProgress / def.workCost;
            }
        }

        // void Methods

        public void PassTime(int passTicks)
        {
            timer = (int)Mathf.Max(timer - passTicks, 0f);
        }

        public void DoWork(float workAmount)
        {
            progress = Mathf.Min(GetProgress + workAmount, def.workCost);
        }

        public void Notify_Finish()
        {
            if (!finishedOnce)
            {

            }
            finishedOnce = true;
        }

        public void Notify_Fail()
        {
            if (!failedOnce)
            {

            }
            failedOnce = true;
        }
    }
}
