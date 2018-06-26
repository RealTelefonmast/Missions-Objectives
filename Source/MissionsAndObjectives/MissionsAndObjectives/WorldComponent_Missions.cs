using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace MissionsAndObjectives
{
    public class WorldComponent_Missions : WorldComponent, ILoadReferenceable
    {
        public List<Mission> Missions = new List<Mission>();

        public List<MissionDef> FinishedMissions = new List<MissionDef>();

        public List<Thing> tempThingList = new List<Thing>();

        public List<ModContentPackWrapper> ModFolder = new List<ModContentPackWrapper>();

        public ModContentPackWrapper theme;

        public bool openedOnce = false;

        public Vector2 missionScrollPos = Vector2.zero;

        public Vector2 objectiveScrollPos = Vector2.zero;

        public override void ExposeData()
        {
            Scribe_Values.Look(ref objectiveScrollPos, "objectiveScrollPos");
            Scribe_Values.Look(ref missionScrollPos, "missionScrollPos");
            Scribe_Values.Look(ref openedOnce, "openedOnce");
            Scribe_Deep.Look(ref theme, "theme");
            Scribe_Collections.Look(ref ModFolder, "ModFolder");
            Scribe_Collections.Look(ref Missions, "Mission");
            Scribe_Collections.Look(ref FinishedMissions, "FinishedMissions");
        }

        public WorldComponent_Missions(World world) : base(world)
        {
        }

        public static WorldComponent_Missions MissionHandler
        {
            get
            {
                return Find.World.GetComponent<WorldComponent_Missions>();
            }
        }

        public string GetUniqueLoadID()
        {
            return this.ToString();
        }

        public override void FinalizeInit()
        {
            base.FinalizeInit();
        }

        public override void WorldComponentTick()
        {
            base.WorldComponentTick();
            if (Find.TickManager.TicksGame % GenTicks.TickRareInterval == 0)
            {
                foreach (ThingDef def in StationDefs)
                {
                    tempThingList.AddRange((Find.AnyPlayerHomeMap.listerThings.ThingsOfDef(def).Where(x => !tempThingList.Contains(x) && (x.TryGetComp<CompPowerTrader>()?.PowerOn ?? true))));
                }
            }
            foreach (MissionDef missionDef in DefDatabase<MissionDef>.AllDefsListForReading)
            {
                if (missionDef.CanStartNow && !missionDef.IncidentBound)
                {
                    if (!Missions.Any((Mission x) => x.def == missionDef))
                    {
                        AddNewMission(missionDef);
                    }
                }
            }
            for(int i = 0; i < Missions.Count; i++)
            {
                Mission mission = Missions[i];
                foreach (Objective objective in mission.Objectives)
                {
                    objective.Notify_Start();
                    if (objective.Finished)
                    {
                        objective.Notify_Finish();
                    }
                    if (objective.Failed)
                    {
                        objective.Notify_Fail();
                    }
                    if (objective.Active && !objective.Finished && !objective.Failed)
                    {
                        mission.TimePassed(objective, 1);
                    }
                }
                if (mission.def.IsFinished)
                {
                    if (!FinishedMissions.Contains(mission.def))
                    {
                        FinishedMissions.Add(mission.def);
                    }
                    if (mission.def.hideOnComplete || mission.def.repeatable)
                    {
                        Missions.Remove(mission);
                        mission.Dispose();
                    }
                }
            }
        }

        public void Notify_Seen(Mission mission)
        {
            if (!mission.seen)
            {
                Missions.Find((Mission x) => x == mission).seen = true;
            }
        }

        public void AddNewMission(MissionDef mission)
        {
            Mission newMission = new Mission(mission, this);
            Missions.Add(newMission);
        }

        public List<Objective> AllObjectives
        {
            get
            {
                List<Objective> list = new List<Objective>();
                foreach (Mission mission in Missions)
                {
                    list.AddRange(mission.Objectives);
                }
                return list;
            }
        }

        public List<ThingDef> StationDefs
        {
            get
            {
                List<ThingDef> list = new List<ThingDef>();
                foreach (Mission mission in Missions)
                {
                    if (mission != null)
                    {
                        foreach (Objective objective in mission.Objectives)
                        {
                            if (objective.Active)
                            {
                                foreach (ThingDef def in objective.def.stationDefs)
                                {
                                    if (!list.Contains(def))
                                    {
                                        list.Add(def);
                                    }
                                }
                                if (objective.def.objectiveType == ObjectiveType.Examine)
                                {
                                    foreach (ThingValue tv in objective.def.targets)
                                    {
                                        ThingDef def = tv.ThingDef;
                                        if (!list.Contains(def))
                                        {
                                            list.Add(def);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                return list;
            }
        }

        public List<Pawn> CapablePawnsTotal
        {
            get
            {
                List<Pawn> pawnList = new List<Pawn>();
                foreach (Mission mission in Missions)
                {
                    foreach (Objective objective in mission.Objectives)
                    {
                        if (objective.Active && !objective.def.IsManualJob)
                        {
                            pawnList.AddRange(Find.AnyPlayerHomeMap.mapPawns.AllPawns.Where(p => p.IsColonist && objective.def.skillRequirements.All((SkillRequirement x) => x.PawnSatisfies(p))).ToList());
                        }
                    }
                }
                return pawnList;
            }
        }
    }
}
