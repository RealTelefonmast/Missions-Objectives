using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;

namespace StoryFramework
{
    public class StoryContentPack : IExposable
    {
        public string identifier;
        public bool Toggled = false;
        public List<MissionDef> MissionDefs = new List<MissionDef>();
        public List<ObjectiveDef> ObjectiveDefs = new List<ObjectiveDef>();
        public StoryDef story;
        public ThemePack ThemePack;
        private ModContentPack mcp;

        public void ExposeData()
        {
            Scribe_Defs.Look(ref story, "story");
            Scribe_Values.Look(ref identifier, "identifier");
            Scribe_Values.Look(ref Toggled, "toggled");
        }

        public StoryContentPack()
        { SetUp(); }

        public StoryContentPack(ModContentPack mcp)
        {
            identifier = mcp.Identifier;
            story = (StoryDef)mcp.AllDefs.First(d => d is StoryDef);
            SetUp();
        }

        public void SetUp()
        {
            MissionDefs.AddRange((List<MissionDef>)mcp.AllDefs.Where(d => d is MissionDef));
            ObjectiveDefs.AddRange((List<ObjectiveDef>)mcp.AllDefs.Where(d => d is ObjectiveDef));
            ThemePack = new ThemePack(story);
        }

        public ModContentPack MCP
        {
            get
            {
                if(mcp == null)
                {
                    mcp = LoadedModManager.RunningMods.ToList().Find(mcp => mcp.Identifier == identifier);
                }
                return mcp;
            }
        }

        public bool HasActiveMissions
        {
            get
            {
                return MissionDefs.Any(m => m.);
            }
        }

        public bool HasUnseenMissions
        {
            get
            {
                return MissionDefs.Any(m => m.);
            }
        }


        public void Toggle()
        {
            Toggled = !Toggled;
        }
    }
}
