using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace StoryFramework
{
    public class Mission : IExposable, ILoadReferenceable
    {
        public MissionDef def;
        public List<Objective> objectives = new List<Objective>();
        public FailTracker failTracker;
        private MOState cachedState = MOState.None;
        private bool seen = false;
        private int timer = -1;
        private int repeatCount = 0;
        public Mission(){}

        public Mission(MissionDef def)
        {
            this.def = def;
            SetUp();
        }

        private void SetUp()
        {
            if (def.timer.GetTotalTime > 0)
            {
                timer = def.timer.GetTotalTime;
            }
            if (!def.objectives.NullOrEmpty())
            {
                foreach (ObjectiveDef objective in def.objectives)
                {
                    MakeObjective(objective);
                }
            }
            if(failTracker == null && def.failConditions != null)
            {
                failTracker = new FailTracker(def.failConditions);
            }
            Notify_Change();
        }

        public void ExposeData()
        {
            Scribe_Defs.Look(ref def, "def");
            Scribe_Collections.Look(ref objectives, "objectives", LookMode.Deep, new object[]
            {
                this
            });
            Scribe_Deep.Look(ref failTracker, "failTracker", new object[]
            {
                def.failConditions, true
            });
            Scribe_Values.Look(ref cachedState, "cachedState");
            Scribe_Values.Look(ref seen, "seen");
            Scribe_Values.Look(ref timer, "timer");
        }

        public void MakeObjective(ObjectiveDef def)
        {
            Type type = def.customClass;
            Objective objective = (Objective)Activator.CreateInstance(type);
            objective.def = def;
            objective.SetUp(this);
            objectives.Add(objective);
        }

        public string GetUniqueLoadID()
        {
            return def.defName + "Mission";
        }

        public void MissionTick()
        {
            if (LatestState == MOState.Active)
            {
                foreach (Objective objective in objectives)
                {
                    objective.ObjectiveTick();
                }
                if (timer > 0)
                {
                    timer--;
                }
                if (Find.TickManager.TicksGame % GenTicks.TickRareInterval == 0)
                {
                    Notify_Change();                
                }
            }
        }

        public void Notify_Change()
        {
            LatestState = CurrentState;
            if(LatestState == MOState.Finished)
            {
                Messages.Message("FinishedMission_SMO".Translate() + ": " + def.LabelCap, MessageTypeDefOf.PositiveEvent);
            }
            if (LatestState == MOState.Failed)
            {
                Messages.Message("FailedMission_SMO".Translate() + ": " + def.LabelCap, MessageTypeDefOf.NegativeEvent);
            }
            if (LatestState == MOState.Finished && def.repeatable)
            {
                Reset();
            }
        }

        public void Notify_Seen()
        {
            if (!seen)
            {
                seen = true;
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

        public bool ReadyToTurnIn
        {
            get
            {
                return objectives.All(o => o.CurrentState == MOState.Finished);
            }
        }

        public bool Seen
        {
            get
            {
                return seen;
            }
        }

        public bool HasTimer
        {
            get
            {
                return def.timer.GetTotalTime > 0f;
            }
        }

        public bool Visible
        {
            get
            {
                if (LatestState == MOState.Finished)
                {
                    return !def.hideOnComplete;
                }
                return true;
            }
        }

        public bool CanProgress
        {
            get
            {               
                return objectives.Where(o => o.CurrentState == MOState.Inactive).All(o => !o.def.requisites?.Impossible ?? true);
            }
        }

        private bool Failed
        {
            get
            {
                return LatestState == MOState.Failed || !CanProgress || timer == 0 || (failTracker?.Failed ?? false);
            }
        }

        private bool Finished
        {
            get
            {
                return  LatestState == MOState.Finished || objectives.All(o => o.CurrentState == MOState.Finished);
            }
        }

        public MOState LatestState
        {
            get
            {
                return this.cachedState;
            }
            set
            {
                this.cachedState = value;
            }
        }

        private MOState CurrentState
        {
            get
            {
                MOState state = MOState.Inactive;
                if (Failed)
                {
                    state = MOState.Failed;
                }
                else
                if (Finished)
                {
                    state = MOState.Finished;
                }
                else
                {
                    state = MOState.Active;
                }
                return state;
            }
        }

        public Objective ObjectiveByDef(ObjectiveDef def)
        {
            return objectives.Find(o => o.def == def);
        }

        public void Reset()
        {
            LatestState = MOState.None;
            seen = false;
            objectives.Clear();
            SetUp();
            repeatCount++;
        }
    }
}
