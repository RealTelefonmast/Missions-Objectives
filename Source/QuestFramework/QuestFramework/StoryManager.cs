using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using RimWorld.Planet;
using Verse;
using UnityEngine;

namespace StoryFramework
{
    public class StoryManager : WorldComponent
    {
        public List<Mission> Missions = new List<Mission>();
        public List<MissionDef> LockedMissions = new List<MissionDef>();
        public Objective selectedObjective;
        public Mission selectedMission;
        public ModContentPackWrapper Theme;
        public List<ModContentPackWrapper> ModFolder = new List<ModContentPackWrapper>();
        public List<ObjectiveStation> AllStations = new List<ObjectiveStation>();
        public List<Thing> TempStations = new List<Thing>();
        public Vector2 missionScrollPos = Vector2.zero;
        public Vector2 objectiveScrollPos = Vector2.zero;

        public static StoryManager StoryHandler
        {
            get
            {
                return Find.World.GetComponent<StoryManager>();
            }
        }

        public StoryManager(World world) : base(world)
        {
        }

        public override void FinalizeInit()
        {
            CheckForNewMods();
            UpdateMissionList();
        }

        public override void ExposeData()
        {
            Scribe_Collections.Look(ref Missions, "Missions", LookMode.Deep);
            Scribe_Collections.Look(ref LockedMissions, "LockedMission", LookMode.Def);
            Scribe_References.Look(ref selectedObjective, "selectedObjective");
            Scribe_References.Look(ref selectedMission, "selectedMission");
            Scribe_Deep.Look(ref Theme, "Theme");
            Scribe_Collections.Look(ref ModFolder, "ModFolder", LookMode.Deep);
            base.ExposeData();
        }

        public override void WorldComponentTick()
        {
            base.WorldComponentTick();
            if (Find.TickManager.TicksGame % GenTicks.TickRareInterval == 0)
            {
                UpdateMissionList();
                UpdateStations();
            }
            foreach (Mission mission in Missions)
            {
                mission.MissionTick();
            }
        }

        public void Notify_NewMission(Mission mission)
        {
            List<Thing> PoweredThings = AllStations.Stations();
            foreach (Thing thing in PoweredThings)
            {
                foreach(Objective objective in mission.objectives)
                {
                    if(objective.CurrentState != MOState.Finished && objective.def.type == ObjectiveType.Research && (objective.def.targetSettings?.targets.Any(tv => tv.ThingDef == thing.def) ?? false))
                    {
                        ObjectiveStation station = StoryHandler.AllStations.Station(null, thing);
                        if(station != null)
                        {
                            station.AddObjective(objective);
                        }                     
                    }
                }
            }
        }

        public void Notify_Explored(int tile)
        {
            IEnumerable<Objective> objectives = GetObjectives.Where(o => o.CurrentState == MOState.Active && o.def.type == ObjectiveType.Travel);
            foreach (Objective objective in objectives)
            {
                objective.travelTracker.TryExplore(tile);
            }
        }

        public void Notify_Interacted(FactionDef def, TravelMode mode, int profit)
        {
            IEnumerable<Objective> objectives = GetObjectives.Where(o => o.CurrentState == MOState.Active && o.def.type == ObjectiveType.Travel);
            foreach (Objective objective in objectives)
            {
                objective.travelTracker.Notify_Interacted(def, mode, profit);
            }
        }

        public void Notify_IncidentFired(IncidentDef def)
        {
            UpdateMissionList(null, def);
            IEnumerable<Objective> objectives = GetObjectives.Where(o => o.CurrentState == MOState.Inactive && (o.def.requisites?.incidents.Contains(def) ?? false));
            foreach (Objective objective in objectives)
            {
                if (objective.parentMission.def.chronological)
                {
                    return;
                }
                if (objective.def.requisites?.IsFulfilled(null, def) ?? true)
                {
                    objective.Notify_Start();
                }
            }
        }

        public void Notify_JobStarted(JobDef def, Pawn worker = null)
        {
            UpdateMissionList(def);
            IEnumerable<Objective> objectives = GetObjectives.Where(o => o.CurrentState == MOState.Inactive && (o.def.requisites?.jobs.Contains(def) ?? false));
            foreach (Objective objective in objectives)
            {
                if (objective.parentMission.def.chronological)
                {
                    return;
                }
                if (objective.def.requisites?.IsFulfilled(def) ?? true)
                {
                    objective.Notify_Start();
                    objective.lastTarget = worker;
                }
            }
        }

        public void CheckForNewMods()
        {
            foreach (ModContentPack mcp in LoadedModManager.RunningMods.Where(mcp => mcp.AllDefs.Any(def => def is StoryControlDef)))
            {
                if (mcp != null)
                {
                    if (!ModFolder.Any(mcpw => mcpw.identifier == mcp.Identifier))
                    {
                        ModContentPackWrapper mcpw = new ModContentPackWrapper(mcp.Identifier);
                        ModFolder.Add(mcpw);
                    }
                }
            }
        }

        public IEnumerable<Thing> ActiveStations
        {
            get
            {
                return AllStations.FindAll(s => s.active == true).Stations();
            }
        }

        public List<Objective> GetObjectives
        {
            get
            {
                List<Objective> Objectives = new List<Objective>();
                foreach (Mission mission in Missions)
                {
                    Objectives.AddRange(mission.objectives.Where(o => o.Active));
                }
                return Objectives;
            }
        }

        public Mission GetMission(MissionDef def)
        {
            return Missions.Find(m => m.def == def);
        }

        public Mission GetMission(ObjectiveDef def)
        {
            return Missions.Find(m => m.objectives.Any(o => o.def == def));
        }

        public Mission ActivateMission(MissionDef missionDef)
        {
            if (!Missions.Any(m => m.def == missionDef))
            {
                Mission mission = new Mission(missionDef);
                Missions.Add(mission);
                return mission;
            }
            return Missions.Find(m => m.def == missionDef);
        }

        public void UpdateMissionList(JobDef def = null, IncidentDef incident = null)
        {
            foreach(MissionDef missionDef in DefDatabase<MissionDef>.AllDefsListForReading)
            {
                if (!LockedMissions.Contains(missionDef))
                {
                    if (missionDef.CanStartNow(def, incident))
                    {
                        ActivateMission(missionDef);
                    }
                }
            }
        }

        private void UpdateStations()
        {
            //Check power for stations
            List<Thing> PoweredThings = AllStations.Stations();
            foreach (Thing thing in PoweredThings)
            {
                CompPowerTrader compPower = thing.TryGetComp<CompPowerTrader>();
                ObjectiveStation station = AllStations.Station(null, thing);
                station.active = compPower?.PowerOn ?? true;
            }
            AllStations.RemoveAll(t => t.station.DestroyedOrNull());
        }

        public bool GetMissionSeen(MissionDef def)
        {
            return Missions.Find(m => m.def == def)?.Seen ?? false;
        }

        public MOState GetMissionState (MissionDef def)
        {
            if (Missions.Any(m => m.def == def))
            {
                return Missions.Find(m => m.def == def).LatestState;
            }
            return MOState.Inactive;
        }

        public MOState GetObjectiveState(ObjectiveDef def)
        {
            Objective objective = null;
            Missions.Find(m => (objective = m.objectives.Find(o => o.def == def)) != null);
            return objective?.CurrentState ?? MOState.Inactive;
        }

        public Dictionary<Objective, List<ThingDef>> StationDefs()
        {
            Dictionary<Objective, List<ThingDef>> defs = new Dictionary<Objective, List<ThingDef>>();
            foreach (Mission mission in Missions)
            {
                foreach (Objective objective in mission.objectives)
                {
                    ObjectiveType type = objective.def.type;
                    if (objective.CurrentState == MOState.Active && type == ObjectiveType.Research || (type == ObjectiveType.Custom && objective.def.customSettings.usesStation))
                    {
                        List<ThingDef> stations = objective.def.targetSettings?.targets.AllThingDefs();
                        defs.Add(objective, stations);
                    }
                }
            }
            return defs;
        }

        public List<Pawn> CapablePawnsTotal
        {
            get
            {
                List<Pawn> pawns = new List<Pawn>();
                foreach(Mission mission in Missions)
                {
                    foreach(Objective objective in mission.objectives)
                    {
                        if(objective.CurrentState == MOState.Active && !objective.def.IsManualJob)
                        {
                            pawns.AddRange(objective.CapablePawns);
                        }
                    }
                }
                return pawns;
            }
        }

    }
}
