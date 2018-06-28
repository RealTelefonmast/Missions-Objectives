using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace MissionsAndObjectives
{
    public class MissionThingDef : ThingDef
    {
        public List<ObjectiveDef> objectivePrerequisites;

        public List<ObjectiveDef> unlockOnObjective;
    }
}
