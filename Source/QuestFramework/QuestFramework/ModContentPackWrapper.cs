using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using UnityEngine;

namespace StoryFramework
{
    public class ModContentPackWrapper : IExposable
    {
        public string identifier;
        private bool toggled = false;
        public StoryControlDef SCD;

        public ModContentPackWrapper(){}

        public ModContentPackWrapper(string name)
        {
            identifier = name;
            if(SCD == null)
            {
                SCD = MCP.AllDefs.Where(d => d is StoryControlDef).FirstOrDefault() as StoryControlDef;
            }
        }

        public ModContentPack MCP
        {
            get
            {
                return LoadedModManager.RunningMods.ToList().Find(mcp => mcp.Identifier == identifier);
            }
        }

        public bool HasActiveMissions
        {
            get
            {
                return MCP.AllDefs.Any(d => d is MissionDef && !(d as MissionDef).HardLocked);
                //return MCP.AllDefs.Any(d => d is MissionDef && (d as MissionDef).CurrentState == MOState.Active);
            }
        }

        public bool HasUnseenMissions
        {
            get
            {
                return MCP.AllDefs.Any(d => d is MissionDef && !(d as MissionDef).IsSeen);
            }
        }

        public bool Toggled
        {
            get
            {
                return toggled;
            }
        }

        public void Toggle()
        {
            toggled = !toggled;
        }

        public void ExposeData()
        {
            Scribe_Defs.Look(ref SCD, "SCD");
            Scribe_Values.Look(ref identifier, "identifier");
            Scribe_Values.Look(ref toggled, "toggled");
        }
    }
}
