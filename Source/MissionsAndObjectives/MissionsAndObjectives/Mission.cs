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

        public bool failed = false;

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
            }
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref failed, "failed");
            Scribe_Values.Look(ref seen, "seen");
            Scribe_Collections.Look(ref Objectives, "Objectives");
            Scribe_Defs.Look(ref def, "def");
            Scribe_Deep.Look(ref parent, "parent");
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
            amount *= MCD.MainMissionControlDef.workFloat;
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
