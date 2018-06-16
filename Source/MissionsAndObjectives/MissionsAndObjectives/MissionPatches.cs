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
        internal static class DesignatorBuild_Patch
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

        [HarmonyPatch(typeof(RecipeDef))]
        [HarmonyPatch("AvailableNow", 0)]
        internal static class RecipeDef_Patch
        {
            public static void Postfix(RecipeDef __instance, ref bool __result)
            {
                foreach(ThingCountClass count in __instance.products)
                {
                    ThingDef def = count.thingDef;
                    if(def is MissionThingDef && !DebugSettings.godMode && (def as MissionThingDef).objectivePrerequisites != null)
                    {
                        if ((def as MissionThingDef).objectivePrerequisites.Any((ObjectiveDef x) => x.IsFinished))
                        {
                            __result = true;
                            return;
                        }
                        __result = false;
                    }
                }
            }
        }

        [StaticConstructorOnStartup]
        [HarmonyPatch(typeof(Thing))]
        [HarmonyPatch("Print")]
        static class PrintAtPatch
        {
            private static Graphic Graphic;
            public static void Postfix(Thing __instance, SectionLayer layer)
            {
                Graphic = GraphicDatabase.Get(typeof(Graphic_Single), "UI/ObjectiveMarker", ShaderDatabase.MetaOverlay, __instance.def.size.ToVector2(), Color.white, Color.white);

                if (Missions.Missions.Any(m => m.Objectives.Any(o => !o.Finished && o.Active && o.def.targetThings.Contains(__instance.def))))
                {
                    Material Mat = Graphic.MatAt(Rot4.North, null);
                    Printer_Mesh.PrintMesh(layer, __instance.DrawPos, Graphic.MeshAt(Rot4.North), Mat);
                }
            }
        }

        [StaticConstructorOnStartup]
        [HarmonyPatch(typeof(Pawn))]
        [HarmonyPatch("DrawAt")]
        static class DrawAtPatch
        {
            private static Graphic Graphic;
            public static void Postfix(Pawn __instance)
            {
                if (__instance != null)
                {
                    if (Missions.Missions.Any(m => m.Objectives.Any(o => !o.Finished && o.Active && o.def.targetPawns.Contains(__instance.kindDef))))
                    {
                        Graphic = GraphicDatabase.Get(typeof(Graphic_Single), "UI/ObjectiveMarker", ShaderDatabase.MetaOverlay, __instance.Drawer.renderer.graphics.nakedGraphic.drawSize, Color.white, Color.white);
                        Material Mat = Graphic.MatAt(Rot4.North, null);
                        Graphics.DrawMesh(Graphic.MeshAt(Rot4.North), __instance.DrawPos + Altitudes.AltIncVect, Quaternion.identity, Mat, 0);
                    }
                }
            }
        }

        public static WorldComponent_Missions Missions
        {
            get
            {
                return Find.World.GetComponent<WorldComponent_Missions>(); 
            }
        }

        [HarmonyPatch(typeof(Thing))]
        [HarmonyPatch("SpawnSetup")]
        static class SpawnThingPatch
        {
            public static void Postfix(Thing __instance)
            {
                if (__instance.Map == Find.AnyPlayerHomeMap)
                {
                    Missions.Missions.ForEach(m => m.Objectives.Where(o => o.Active && !o.Finished).ToList().ForEach(o => o.killTracker.Discover(__instance.def)));
                }
            }
        }

        [HarmonyPatch(typeof(Thing))]
        [HarmonyPatch("Kill")]
        static class KillThingPatch
        {
            private static ThingDef temp;
            public static bool Prefix(Thing __instance)
            {
                temp = __instance.def;
                return true;
            }

            public static void Postfix(Thing __instance, DamageInfo dinfo)
            {
                if (dinfo.Instigator != null && (dinfo.Instigator as Pawn).IsColonist)
                {
                    Missions.Missions.ForEach(m => m.Objectives.Where(o => o.Active && !o.Finished).ToList().ForEach(o => o.killTracker.Destroy(temp)));
                }
            }
        }

        [HarmonyPatch(typeof(Pawn))]
        [HarmonyPatch("Kill")]
        static class KillPawnPatch
        {
            private static PawnKindDef temp;
            public static bool Prefix(Pawn __instance)
            {
                temp = __instance.kindDef;
                return true;
            }

            public static void Postfix(Pawn __instance, DamageInfo dinfo)
            {
                if (dinfo.Instigator != null && (dinfo.Instigator as Pawn).IsColonist)
                {
                    Missions.Missions.ForEach(m => m.Objectives.Where(o => o.Active && !o.Finished).ToList().ForEach(o => o.killTracker.Destroy(null, temp)));
                }
            }
        }
    }
}
