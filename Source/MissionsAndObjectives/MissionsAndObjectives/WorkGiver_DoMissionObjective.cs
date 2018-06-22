using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI;

namespace MissionsAndObjectives
{
    public class WorkGiver_DoMissionObjective : WorkGiver_Scanner
    {
        private Mission Mission;

        private Objective Objective;

        public WorldComponent_Missions Missions
        {
            get
            {
                return Find.World.GetComponent<WorldComponent_Missions>();
            }
        }

        public override Job NonScanJob(Pawn pawn)
        {
            return base.NonScanJob(pawn);
        }

        public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
        {
            if (Missions.CapablePawnsTotal.Contains(pawn))
            {
                if (!Missions.Missions.NullOrEmpty())
                {
                    if (!Missions.StationDefs.NullOrEmpty())
                    {
                        return Missions.tempThingList.AsEnumerable();
                    }
                }
            }
            return null;
        }

        public override ThingRequest PotentialWorkThingRequest
        {
            get
            {
                return ThingRequest.ForGroup(ThingRequestGroup.Nothing);
            }
        }

        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            if (pawn != null && t != null)
            {
                foreach (Mission mission in Missions.Missions)
                {
                    Mission = mission;
                    foreach (Objective objective in mission.Objectives)
                    {
                        if (objective.CanDoObjective(pawn, t))
                        {
                            Objective = objective;
                            this.def.verb = Objective.def.label;
                            this.def.workType.verb = Objective.def.label;
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public override bool Prioritized
        {
            get
            {
                return true;
            }
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            if (pawn.CanReserveAndReach(t, PathEndMode.Touch, Danger.Some))
            {
                JobInfo info = new JobInfo(Mission, Objective);
                return new JobWithObjects(MCD.DoMissionObjective, t, info);
            }
            return null;
        }

        public override float GetPriority(Pawn pawn, TargetInfo t)
        {
            if (Objective.def.EffectiveStatFactor != null)
            {
                return t.Thing.GetStatValue(Objective.def.EffectiveStatFactor, true);
            }
            return 1;
        }
    }
}
