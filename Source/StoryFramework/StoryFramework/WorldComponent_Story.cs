using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;
using RimWorld.Planet;

namespace StoryFramework
{
    public class WorldComponent_Story : WorldComponent
    {
        public List<Mission> ActiveMissions = new List<Mission>();
        public List<MissionDef> LockedMissions = new List<MissionDef>();
        public List<StoryContentPack> Stories = new List<StoryContentPack>();
        public ThemePack Theme;

        public static Mission SelectedMission;
        public static Objective SelectedObjective;

        public Vector2 missionScrollPos = Vector2.zero;
        public Vector2 objectiveScrollPos = Vector2.zero;
        public Vector2 extraInfoScrollPos = Vector2.zero;

        public static WorldComponent_Story Story => Find.World.GetComponent<WorldComponent_Story>();

        //On World Gen
        public void SetupScenario(ScenPart_Story scene)
        {
            LockedMissions = scene.lockedMissions;
            foreach(var start in scene.MissionsToStart)
            {
                var m = StartMission(start);
                if (scene.finishedMissions.Contains(start))
                    m.Finish();
            }
        }

        public WorldComponent_Story(World world) : base(world)
        {
        }

        public override void ExposeData()
        {
            base.ExposeData();
        }

        public override void WorldComponentTick()
        {
            base.WorldComponentTick();
        }

        public Mission StartMission(MissionDef def)
        {
            if(!ActiveMissions.Any(m => m.def == def))
            {
                Mission mission = new Mission(def);
                ActiveMissions.Add(mission);
                return mission;
            }
            return ActiveMissions.Find(m => m.def == def);
        }
    }
}
