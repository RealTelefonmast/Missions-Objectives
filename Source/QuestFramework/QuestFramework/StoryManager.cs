using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using RimWorld.Planet;
using Verse;
using UnityEngine;
using Harmony;

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
        private Vector2 extraInfoScrollPos = Vector2.zero;
        public bool showExtraInfo = true;
        public float timerAlertMinHours = 5f;

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
            Scribe_Collections.Look(ref AllStations, "AllStations", LookMode.Deep);
            Scribe_Collections.Look(ref ModFolder, "ModFolder", LookMode.Deep);
            Scribe_References.Look(ref selectedObjective, "selectedObjective");
            Scribe_References.Look(ref selectedMission, "selectedMission");
            Scribe_Deep.Look(ref Theme, "Theme");
            Scribe_Values.Look(ref showExtraInfo, "showExtraInfo");
            Scribe_Values.Look(ref timerAlertMinHours, "timerAlertMinHours");
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

        public void MissionInfoOnGUI()
        {
            if (Find.WindowStack.IsOpen<Screen_Credits>() || !(selectedMission?.def.failConditions != null || selectedObjective?.def.failConditions != null))
            {
                return;
            }
            Text.Font = GameFont.Small;
            AlertsReadout alerts = (Find.UIRoot as UIRoot_Play).alerts;
            LearningReadout readout = Find.Tutor?.learningReadout;
            float totalHeight = Math.Min(ContentHeight(), ((float)UI.screenHeight / 2f)); //+ 8f;
            float heightLearn = 8f;
            if (Prefs.AdaptiveTrainingEnabled && readout != null)
            {
                float value = Traverse.Create(readout).Field("contentHeight").GetValue<float>();
                if (value > 0)
                {
                    heightLearn += value + 19f;
                }
            }
            float heightAlert = 0f;
            if(alerts != null)
            {                               
                float alertY = Find.LetterStack.LastTopY - Traverse.Create(alerts).Field("activeAlerts").GetValue<List<Alert>>().Count * 28f;
                float diff = (totalHeight + heightLearn) - alertY;
                if (diff > 0)
                {
                    heightAlert = Math.Abs(diff) + 5f;
                }
            }
            float maxHeight = totalHeight - heightAlert;
            Rect Main = new Rect((float)UI.screenWidth - 208f, heightLearn, 200f, maxHeight);
            Find.WindowStack.ImmediateWindow(1582828288, Main, WindowLayer.GameUI, delegate
            {
                Widgets.DrawHighlight(new Rect(0f, 0f, Main.width, Main.height));
                Rect rect = new Rect(3f, 0f, Main.width, Main.height);              
                Rect view = new Rect(0f, 0f, Main.width - 4f, totalHeight);
                DrawCustomScrollBar(new Rect(0f, 0f, rect.width, Main.height), totalHeight);
                Listing_Standard listing = new Listing_Standard();
                listing.verticalSpacing = 2f;
                listing.Begin(view);
                Text.Font = GameFont.Tiny;
                Widgets.BeginScrollView(rect, ref extraInfoScrollPos, view, false);
                bool flag = false;
                if (selectedMission != null && selectedMission.def.failConditions != null)
                {
                    bool failed = selectedMission.LatestState == MOState.Failed;
                    listing.Label(failed ? "MissionFailed_SMO".Translate("'" + selectedMission.def.LabelCap + "'") : "Mission_SMO".Translate("'" + selectedMission.def.LabelCap + "'") + "... ");
                    if (!failed)
                    {
                        FailconReadout(selectedMission, selectedMission.def.failConditions, ref listing);
                    }
                    flag = true;
                }           
                if (selectedObjective != null && selectedObjective.def.failConditions != null)
                {
                    if (flag)
                    {
                        listing.GapLine(6);
                    }
                    bool failed = selectedObjective.CurrentState == MOState.Failed;
                    listing.Label(failed ? "ObjectiveFailed_SMO".Translate("'" + selectedObjective.def.LabelCap + "'") : "Objective_SMO".Translate("'" + selectedObjective.def.LabelCap + "'") + "... ");
                    if (!failed)
                    {
                        FailconReadout(selectedObjective, selectedObjective.def.failConditions, ref listing);
                    }
                }
                Widgets.EndScrollView();
                listing.End();
            }, false, false, 0f);
        }

        private void DrawCustomScrollBar(Rect rect, float viewHeight)
        {
            float pct = rect.height / viewHeight;
            float height = rect.height * pct;
            Rect bar = new Rect(rect.xMax - 3f, rect.y, 3f, rect.height);
            Widgets.DrawBoxSolid(bar, new Color(0.25f, 0.25f, 0.25f, 0.75f));
            Vector2 size = new Vector2(3f, height);
            float scrollHeight = (rect.height - height);
            float scrollPosArea = viewHeight - rect.height;
            float yOffset = scrollHeight * (1f - ((scrollPosArea - extraInfoScrollPos.y) / scrollPosArea));
            Vector2 pos = new Vector2(bar.x, yOffset);
            Rect block = new Rect(pos, size);
            Widgets.DrawBoxSolid(block, Color.white);
        }

        private void FailconReadout<T>(T t, FailConditions conditions, ref Listing_Standard listing)
        {
            FailTracker failTracker = t is Mission ? (t as Mission).failTracker : (t as Objective).failTracker;
            bool whenFinished = conditions.whenFinished;
            if (!conditions.missions.NullOrEmpty())
            {
                string missions = whenFinished ? "FailCon_MissionDone".Translate() : "FailCon_MissionFail".Translate();
                listing.IconLabel(StoryMats.warning, (missions + ":"), new Vector2(9f, 18f));
                foreach (MissionDef mdef in conditions.missions)
                {
                    listing.Label("    - " + mdef.LabelCap);
                }
            }
            if (!conditions.objectives.NullOrEmpty())
            {
                string objectives = whenFinished ? "FailCon_ObjectiveDone".Translate() : "FailCon_ObjectiveFail".Translate();
                listing.IconLabel(StoryMats.warning, (objectives + ":"), new Vector2(9f, 18f));
                foreach (ObjectiveDef oDef in conditions.objectives)
                {
                    listing.Label("    - " + oDef.LabelCap);
                }
            }
            if (!conditions.targetSettings.targets.NullOrEmpty())
            {
                string targets = conditions.targetSettings.any ? "FailCon_TargetsAny".Translate() : "FailCon_Targets".Translate();
                listing.IconLabel(StoryMats.warning, (targets + ":"), new Vector2(9f, 18f));
                foreach (ThingValue tv in conditions.targetSettings.targets)
                {
                    listing.Label("    - " + tv.ThingDef.LabelCap + " - " + failTracker.TargetsLost[tv] + "/" + tv.value);
                }
            }
            if(conditions.targetSettings.minColonistsToLose > 0)
            {
                listing.IconLabel(StoryMats.warning, "FailCon_DeadColonists".Translate() + ": " + failTracker.lostColonists + "/" + conditions.targetSettings.minColonistsToLose, new Vector2(9f, 18f));
            }
        }

        private float ContentHeight()
        {
            float value = 0f;
            List<FailConditions> cond = new List<FailConditions>();
            if (selectedMission?.def.failConditions != null)
            {
                value += Text.CalcHeight("Mission_SMO".Translate() + ": " + selectedMission.def.LabelCap, 196f) + 2f;
                if (selectedMission.LatestState == MOState.Active)
                {
                    cond.Add(selectedMission?.def.failConditions);
                }
            }          
            if (selectedObjective?.def.failConditions != null)
            {
                value += Text.CalcHeight("Objective_SMO".Translate() + ": " + selectedObjective.def.LabelCap, 196f) + 2f;
                if (selectedObjective.CurrentState == MOState.Active)
                {
                    cond.Add(selectedObjective.def.failConditions);
                }
            }
            for (int i = 0; i < cond.Count; i++)
            {
                FailConditions condition = cond[i];
                bool whenFinished = condition.whenFinished;
                if (!condition.objectives.NullOrEmpty())
                {
                    string objectives = whenFinished ? "FailCon_ObjectiveDone".Translate() : "FailCon_ObjectiveFail".Translate();
                    value += Text.CalcHeight(objectives + ":", 196f - 9f);
                    value += (condition.objectives.Count * 12f) + 2f;
                }
                if (!condition.missions.NullOrEmpty())
                {
                    string missions = whenFinished ? "FailCon_MissionDone".Translate() : "FailCon_MissionFail".Translate();
                    value += Text.CalcHeight(missions + ":", 196f - 9f);
                    value += (condition.missions.Count * 12f) + 2f;
                }
                if ((condition.targetSettings?.targets.Count ?? 0) > 0)
                {
                    string targets = condition.targetSettings.any ? "FailCon_TargetsAny".Translate() : "FailCon_Targets".Translate();
                    value += Text.CalcHeight(targets + ":", 196f - 9f);
                    value += (condition.targetSettings.targets.Count) * 12f + 2f;
                }
                if (condition.targetSettings?.minColonistsToLose > 0)
                {
                    value += 12f + 2f;
                }
            }
            return value;
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
                        ModFolder.Add(new ModContentPackWrapper(mcp.Identifier));
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

        public Objective GetObjective(ObjectiveDef def)
        {
            Objective objective = null;
            Missions.Find(m => (objective = m.objectives.Find(o => o.def == def)) != null);
            return objective;
        }

        public MOState GetObjectiveState(ObjectiveDef def)
        {
            Objective objective = null;
            Missions.Find(m => (objective = m.objectives.Find(o => o.def == def)) != null);
            return objective?.CurrentState ?? MOState.Inactive;
        }

        public Dictionary<ObjectiveDef, List<ThingDef>> StationDefs()
        {
            Dictionary<ObjectiveDef, List<ThingDef>> defs = new Dictionary<ObjectiveDef, List<ThingDef>>();
            List<ObjectiveDef> objectives = DefDatabase<ObjectiveDef>.AllDefsListForReading.FindAll(o => o.type == ObjectiveType.Research || (o.type == ObjectiveType.Custom && o.customSettings.usesStation));
            foreach(ObjectiveDef objective in objectives)
            {
                List<ThingDef> stations = objective.targetSettings?.targets.AllThingDefs();
                defs.Add(objective, stations);
            }
            /*
            foreach (Mission mission in Missions)
            {
                foreach (Objective objective in mission.objectives)
                {
                    ObjectiveType type = objective.def.type;
                    if (objective.CurrentState == MOState.Active && ( type == ObjectiveType.Research || (type == ObjectiveType.Custom && objective.def.customSettings.usesStation)))
                    {
                        List<ThingDef> stations = objective.def.targetSettings?.targets.AllThingDefs();
                        defs.Add(objective, stations);
                    }
                }
            }
            */
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
