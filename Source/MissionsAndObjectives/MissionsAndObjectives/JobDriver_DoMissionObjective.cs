using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;

namespace MissionsAndObjectives
{
    public class JobDriver_DoMissionObjective : JobDriver
    {
        public JobWithObjects Job
        {
            get
            {
                return this.job as JobWithObjects;
            }
        }

        public Pawn Actor
        {
            get
            {
                return this.pawn;
            }
        }

        public Thing Station
        {
            get
            {
                return TargetA.Thing;
            }
        }

        public IntVec3 InteractionCell
        {
            get
            {
                if (ObjectiveDef.distanceToTarget > 1)
                {
                    return Map.AllCells.Where(v => v.DistanceTo(Station.Position) > Objective.def.distanceToTarget && v.DistanceTo(Station.Position) < Objective.def.distanceToTarget + 1 && GenSight.LineOfSight(v, Station.Position, Map)).RandomElement();
                }
                return Station.InteractionCell;
            }
        }

        public Mission Mission
        {
            get
            {
                return Job.jobInfo.objectA as Mission;
            }
        }

        public ObjectiveDef ObjectiveDef
        {
            get
            {
                return Objective.def;
            }
        }

        public Objective Objective
        {
            get
            {
                return Job.jobInfo.objectB as Objective;
            }
        }

        public override string GetReport()
        {
            return ObjectiveDef.LabelCap;
        }

        public override bool TryMakePreToilReservations()
        {
            return this.pawn.Reserve(this.job.targetA, this.job);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            yield return Toils_Reserve.Reserve(TargetIndex.A);
            yield return Toils_Goto.GotoCell(InteractionCell, PathEndMode.OnCell);

            Toil doObjective = new Toil();
            doObjective.tickAction = delegate
            {
                Pawn actor = doObjective.actor;

                float num = 1.1f;
                if (ObjectiveDef.EffectiveStat != null && actor.SpecialDisplayStats.Any((StatDrawEntry x) => x.stat == ObjectiveDef.EffectiveStat))
                {
                    num *= actor.GetStatValue(ObjectiveDef.EffectiveStat, true);
                }
                if (ObjectiveDef.EffectiveStatFactor != null)
                {
                    num *= this.TargetThingA.GetStatValue(ObjectiveDef.EffectiveStatFactor, true);
                }
                foreach (SkillRequirement SR in ObjectiveDef.skillRequirements)
                {
                    actor.skills.Learn(SR.skillDef, 0.11f, false);
                }
                Mission.WorkPerformed(ObjectiveDef, num);
                actor.GainComfortFromCellIfPossible();
            };
            doObjective.FailOn(() => Objective.Finished || ObjectiveDef.distanceToTarget > 0 ? !GenSight.LineOfSight(Actor.Position, TargetA.Cell, Map) ||  Actor.Position.DistanceTo(TargetA.Cell) > ObjectiveDef.distanceToTarget : false);
            doObjective.WithProgressBar(TargetIndex.A, () => Objective.ProgressPct, false, -0.5f);
            doObjective.defaultCompleteMode = ToilCompleteMode.Delay;
            doObjective.defaultDuration = 4000;
            yield return doObjective;
        }
    }
}
