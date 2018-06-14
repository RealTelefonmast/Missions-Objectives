using Harmony;
using System;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using RimWorld.BaseGen;
using Verse;
using Verse.AI.Group;
using UnityEngine;

namespace MissionsAndObjectives
{
    [StaticConstructorOnStartup]
    public static class MissionPatches
    {
        static MissionPatches()
        {
            HarmonyInstance MissionsAndObjectives = HarmonyInstance.Create("com.missionsandobjectives.rimworld.mod");
            MissionsAndObjectives.PatchAll(Assembly.GetExecutingAssembly());
        }

        [HarmonyPatch(typeof(Designator_Build))]
        [HarmonyPatch("Visible", 0)]
        internal static class Harmony_Designator_Build_Patch
        {
            public static void Postfix(Designator_Build __instance, ref bool __result)
            {
                MissionThingDef thingDef;
                if ((thingDef = (__instance.PlacingDef as MissionThingDef)) != null && !DebugSettings.godMode && thingDef.objectivePrerequisites != null)
                {
                    if (thingDef.objectivePrerequisites.Any((ObjectiveDef x) => x.IsFinished))
                    {
                        __result = true;
                        return;
                    }
                    __result = false;
                }
            }
        }
    }

}
