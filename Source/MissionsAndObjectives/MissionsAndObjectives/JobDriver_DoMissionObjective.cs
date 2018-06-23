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

        public int Distance
        {
            get
            {
                return ObjectiveDef.targets.Find(tv => tv.ThingDef == Station.def).value;
            }
        }

        public IntVec3 InteractionCell
        {
            get
            {
                if (Distance > 1)
                {
                    return Map.AllCells.Where(v => v.DistanceTo(Station.Position) > Distance && v.DistanceTo(Station.Position) < Distance + 1 && GenSight.LineOfSight(v, Station.Position, Map)).RandomElement();
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
            return "objectiveJob".Translate() + ": " + ObjectiveDef.LabelCap;
        }

        public override bool TryMakePreToilReservations()
        {
            return this.pawn.Reserve(this.job.targetA, this.job);
        }

        public bool Fail
        {
            get
            {
                if(Objective.Failed || Objective.Finished)
                {
                    return true;
                }
                if(Distance > 0 ? (!GenSight.LineOfSight(Actor.Position, TargetA.Cell, Map) || Actor.Position.DistanceTo(TargetA.Cell) > (Distance * 2)) : false)
                {
                    return true;
                }
                return false;
            }
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
                    actor.skills.Learn(SR.skill, 0.11f, false);
                }
                Mission.WorkPerformed(ObjectiveDef, num);
                actor.GainComfortFromCellIfPossible();
            };
            doObjective.FailOn(() => Fail);
            doObjective.WithProgressBar(TargetIndex.A, () => Objective.ProgressPct, false, -0.5f);
            doObjective.defaultCompleteMode = ToilCompleteMode.Delay;
            doObjective.defaultDuration = 4000;
            yield return doObjective;
        }
    }
}
