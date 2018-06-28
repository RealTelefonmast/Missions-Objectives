using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace MissionsAndObjectives
{
    public class Mission : IExposable
    {
        public MissionDef def;

        public WorldComponent_Missions parent;

        public bool seen = false;

        public List<Objective> Objectives = new List<Objective>();

        public Mission() : base()
        {
        }

        public Mission(MissionDef def, WorldComponent_Missions parent)
        {
            this.parent = parent;
            this.def = def;
            foreach (ObjectiveDef objective in def.objectives)
            {
                Objectives.Add(new Objective(objective, this));
                if (!parent.StartedObjectives.Contains(objective))
                {
                    parent.StartedObjectives.Add(objective);
                }
            }
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref seen, "seen");
            Scribe_Collections.Look(ref Objectives, "Objectives", LookMode.Deep, new object[] {
                this,
            });
            Scribe_Defs.Look(ref def, "def");
            Scribe_References.Look(ref parent, "parent");
        }

        public void Kill()
        {
            parent.Missions.Remove(this);
            this.def = null;
            this.parent = null;
            this.Objectives.Clear();
        }

        public void Reset()
        {
            this.Objectives.ForEach(o => o.Reset());
            this.seen = false;
        }

        public bool Failed
        {
            get
            {
                IEnumerable<Objective> inActive = Objectives.Where(o => !o.Visible && !o.RequiresInactive);
                if (inActive.Count() > 0)
                {
                    return inActive.All(o => o.CanNeverActivate);
                }
                return false;
            }
        }

        public Objective ObjectiveByDef(ObjectiveDef def)
        {
            return Objectives.Find(o => o.def == def);
        }

        public void TimePassed(Objective objective, int ticks)
        {
            if (objective != null)
            {
                objective.PassTime(ticks);
            }
        }

        public void WorkPerformed(ObjectiveDef objective, float amount)
        {
            amount *= 0.009f;
            if (DebugSettings.fastResearch)
            {
                amount *= 500f;
            }
            if (objective != null)
            {
                ObjectiveByDef(objective).DoWork(amount);
            }
        }
    }
}
