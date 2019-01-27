using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;

namespace StoryFramework
{
    public class JobDriver_Objective : JobDriver
    {
        private IntVec3 LastInteractionCell;

        public override void ExposeData()
        {
            Scribe_Values.Look(ref LastInteractionCell, "LastCell");
            base.ExposeData();
        }

        private Job_Story Job
        {
            get
            {
                return job as Job_Story;
            }
        }

        private Objective Objective
        {
            get
            {
                return Job.objective;
            }
        }

        private Thing Station
        {
            get
            {
                return TargetA.Thing;
            }
        }

        private int Distance
        {
            get
            {
                List<ThingValue> targets = Objective.def.targetSettings?.targets;
                if (!targets.NullOrEmpty())
                {
                    return targets.Find(tv => tv.ThingDef == Station.def).value;
                }
                return 1;
            }
        }

        public override void Notify_Starting()
        {
            LastInteractionCell = InteractionCell;
            base.Notify_Starting();
        }

        private IntVec3 InteractionCell
        {
            get
            {
                if (Station != null)
                {
                    if (Distance > 1)
                    {
                        IntVec3 root = Station.Position;
                        if (CellFinder.TryFindRandomCellNear(root, Map, Distance * Distance, new Predicate<IntVec3>(v => v.DistanceTo(root) >= Distance && v.DistanceTo(root) <= (Distance + 1) && GenSight.LineOfSight(v, root, Map)), out IntVec3 result))
                        {
                            return result;
                        }
                    }
                    return Station.InteractionCell;
                }
                return pawn.Position;
            }
        }

        public override string GetReport()
        {
            return "ObjectiveJob_SMO".Translate() + ": " + Objective.def.LabelCap;
        }

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(TargetA, job);
        }

        private bool Fail
        {
            get
            {
                if (Objective.CurrentState != MOState.Active)
                {
                    return true;
                }
                return false;
            }
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            this.FailOn(() => Fail);
            yield return Toils_Reserve.Reserve(TargetIndex.A);
            yield return Toils_Goto.GotoCell(LastInteractionCell, PathEndMode.OnCell);

            Toil objective = new Toil();
            objective.tickAction = delegate
            {
                Pawn actor = objective.actor;
                float num = 1.1f;
                if(actor.SpecialDisplayStats().Any(s => s.stat == StatDefOf.ResearchSpeed))
                {
                    num *= actor.GetStatValue(StatDefOf.ResearchSpeed, true);
                }
                if (actor.SpecialDisplayStats().Any(s => s.stat == StatDefOf.ResearchSpeedFactor))
                {
                    num *= Station.GetStatValue(StatDefOf.ResearchSpeedFactor, true);
                }
                foreach (SkillRequirement SR in Objective.def.skillRequirements)
                {
                    actor.skills.Learn(SR.skill, 0.11f, false);
                }
                Objective.PerformWork(num);
                actor.GainComfortFromCellIfPossible();
            };
            objective.WithProgressBar(TargetIndex.A, () => Objective.GetWorkPct, false, -0.5f);
            objective.defaultCompleteMode = ToilCompleteMode.Delay;
            objective.defaultDuration = 4000;
            objective.finishActions.Add(delegate
            {
                Objective.lastTarget = GetActor();
            });
            yield return objective;
        }
    }
}
