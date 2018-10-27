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
using Verse.AI;
using UnityEngine;

namespace StoryFramework
{
    [StaticConstructorOnStartup]
    public static class StoryPatches
    {
        static StoryPatches()
        {
            HarmonyInstance Story = HarmonyInstance.Create("com.storyframework.rimworld.mod");
            Story.PatchAll(Assembly.GetExecutingAssembly());
            foreach(Type type in typeof(JobDriver).AllSubclasses())
            {
                MethodInfo method = AccessTools.Method(type, "TryMakePreToilReservations");
                HarmonyMethod postfix = new HarmonyMethod(typeof(StoryPatches), "JobDriver_PostFix");
                Story.Patch(method, null, postfix);
            }           
        }

        public static void JobDriver_PostFix(JobDriver __instance, bool __result)
        {
            if (__result)
            {
                Action newAction = delegate
                {
                    StoryHandler.Notify_JobStarted(__instance.job.def, __instance.GetActor());
                };
                LongEventHandler.ExecuteWhenFinished(newAction);
            }
        }

        public static StoryManager StoryHandler
        {
            get
            {
                return StoryManager.StoryHandler;
            }
        }

        public static bool CanBeMade(Def thingDef, ref bool result)
        {
            if (!DebugSettings.godMode)
            {
                StoryThingDefExtension extension = thingDef.modExtensions.Find(m => m is StoryThingDefExtension) as StoryThingDefExtension;
                if (!extension.objectiveRequisites.NullOrEmpty())
                {
                    if (extension.OnStart)
                    {
                        if (extension.Any)
                        {
                            result = extension.objectiveRequisites.Any(o => StoryManager.StoryHandler.GetObjectiveState(o) == MOState.Active);
                        }
                        else
                        {
                            result = extension.objectiveRequisites.All(o => StoryManager.StoryHandler.GetObjectiveState(o) == MOState.Active);
                        }
                    }
                    else
                    {
                        if (extension.Any)
                        {
                            result = extension.objectiveRequisites.Any(o => StoryManager.StoryHandler.GetObjectiveState(o) == MOState.Finished);
                        }
                        else
                        {
                            result = extension.objectiveRequisites.All(o => StoryManager.StoryHandler.GetObjectiveState(o) == MOState.Finished);
                        }
                    }
                }
            }
            return result;
        }

        public static bool HasStoryExtension(this Def def)
        {
            if (!def.modExtensions.NullOrEmpty())
            {
                return def.modExtensions.Any(m => m is StoryThingDefExtension);
            }
            return false;
        }

        [HarmonyPatch(typeof(SettleUtility))]
        [HarmonyPatch("AddNewHome")]
        public static class SettlePatch
        {
            public static void Postfix(int tile, Faction faction)
            {
                StoryHandler.Notify_Explored(tile);
            }
        }

        [HarmonyPatch(typeof(SettlementUtility))]
        [HarmonyPatch("AttackNow")]
        public static class AttackPatch
        {
            public static void Postfix(Caravan caravan, SettlementBase settlement)
            {
                FactionDef def = settlement.Faction.def;
                StoryHandler.Notify_Interacted(def, TravelMode.Raid, 0);
            }
        }

        [HarmonyPatch(typeof(TradeDeal))]
        [HarmonyPatch("TryExecute")]
        public static class TradePatch
        {
            private static int profit = 0;

            public static bool Prefix(ref bool actuallyTraded)
            {
                profit = TradeSession.deal.SilverTradeable.CountHeldBy(Transactor.Colony);
                return true;
            }

            public static void Postfix(ref bool actuallyTraded)
            {
                if (actuallyTraded)
                {
                    profit -= TradeSession.deal.SilverTradeable.CountHeldBy(Transactor.Colony);
                    FactionDef def = TradeSession.trader.Faction.def;
                    StoryHandler.Notify_Interacted(def, TravelMode.Trade, -profit);
                }
            }
        }

        [HarmonyPatch(typeof(Storyteller))]
        [HarmonyPatch("TryFire")]
        public static class IncidentPatch
        {
            public static void Postfix(FiringIncident fi, bool __result)
            {
                StoryHandler.Notify_IncidentFired(fi.def);
            }
        }

        [HarmonyPatch(typeof(Designator_Build))]
        [HarmonyPatch("Visible", MethodType.Getter)]
        internal static class DesignatorBuild_Patch
        {
            public static void Postfix(Designator_Build __instance, ref bool __result)
            {
                BuildableDef def = __instance.PlacingDef;
                if (def.HasStoryExtension())
                {
                    __result = CanBeMade(def, ref __result);
                }
            }
        }

        [HarmonyPatch(typeof(RecipeDef))]
        [HarmonyPatch("AvailableNow", MethodType.Getter)]
        internal static class RecipeDef_Patch
        {
            public static void Postfix(RecipeDef __instance, ref bool __result)
            {
                foreach (ThingDefCountClass count in __instance.products)
                {
                    ThingDef def = count.thingDef;
                    if (def.HasStoryExtension())
                    {
                        __result = CanBeMade(def, ref __result);
                    }
                }
            }
        }

        [HarmonyPatch(typeof(Command_SetPlantToGrow))]
        [HarmonyPatch("IsPlantAvailable")]
        internal static class Grow_Patch
        {
            public static void Postfix(ThingDef plantDef, ref bool __result)
            {
                if (plantDef.HasStoryExtension())
                {
                    __result = CanBeMade(plantDef, ref __result);
                }
            }
        }

        [HarmonyPatch(typeof(InteractionWorker_RecruitAttempt))]
        [HarmonyPatch("DoRecruit")]
        [HarmonyPatch(new Type[] { typeof(Pawn), typeof(Pawn), typeof(float), typeof(bool)})]
        static class RecruitPatch
        {
            private static PawnKindDef def;

            public static bool Prefix(Pawn recruiter, Pawn recruitee, float recruitChance, bool useAudiovisualEffects = true)
            {
                def = recruitee.kindDef;
                return true;
            }

            public static void Postfix(Pawn recruiter, Pawn recruitee, float recruitChance, bool useAudiovisualEffects = true)
            {
                StoryHandler.Missions.ForEach(m => m.objectives.Where(o => o.CurrentState == MOState.Active).Do(o => o.thingTracker?.ProcessTarget(def, recruitee.Position, recruitee.Map, ObjectiveType.Recruit, null, recruitee)));          
            }
        }

        [HarmonyPatch(typeof(Frame))]
        [HarmonyPatch("CompleteConstruction")]
        static class BuildPatch
        {
            public static void Postfix(Frame __instance)
            {
                if ((__instance.def.entityDefToBuild as TerrainDef) == null)
                {
                    StoryHandler.Missions.ForEach(m => m.objectives.Where(o => o.CurrentState == MOState.Active).Do(o => o.thingTracker?.ProcessTarget(__instance.def.entityDefToBuild as ThingDef, __instance.Position, __instance.Map, ObjectiveType.ConstructOrCraft)));
                }
            }
        }

        [HarmonyPatch(typeof(Blueprint_Install))]
        [HarmonyPatch("MakeSolidThing")]
        static class BuildPatch2
        {
            public static void Postfix(Thing __result)
            {
                if (__result != null)
                {
                    StoryHandler.Missions.ForEach(m => m.objectives.Where(o => o.CurrentState == MOState.Active).Do(o =>
                    {
                        o.thingTracker?.ProcessTarget(__result.def, __result.Position, __result.Map, ObjectiveType.ConstructOrCraft);
                    }
                    ));
                }
            }
        }

        [HarmonyPatch(typeof(GenRecipe))]
        [HarmonyPatch("MakeRecipeProducts")]
        static class CraftPatch
        {
            public static void Postfix(RecipeDef recipeDef, Pawn worker, IEnumerable<Thing> __result)
            {
                foreach(Thing thing in __result)
                {
                    StoryHandler.Missions.ForEach(m => m.objectives.Where(o => o.CurrentState == MOState.Active).Do(o =>
                    {
                        o.thingTracker?.ProcessTarget(thing.def, worker.Position, worker.Map, ObjectiveType.ConstructOrCraft, thing);
                    }
                    ));
                }
            }
        }

        [HarmonyPatch(typeof(Thing))]
        [HarmonyPatch("SpawnSetup")]
        static class SpawnThingPatch
        {
            public static void Postfix(Thing __instance, bool respawningAfterLoad)
            {
                if (__instance.Map.IsPlayerHome && !respawningAfterLoad && __instance.def.mote == null)
                {
                    if(__instance is Pawn pawn)
                    {
                        StoryHandler.Missions.ForEach(m => m.objectives.Where(o => o.CurrentState == MOState.Active).Do(o =>
                        {
                            o.thingTracker?.ProcessTarget(pawn.kindDef, pawn.Position, pawn.Map, ObjectiveType.PawnCheck, null, pawn);
                        }
                    ));
                    }
                    Dictionary<ObjectiveDef, List<ThingDef>> stations = StoryHandler.StationDefs();
                    foreach (ObjectiveDef objective in stations.Keys)
                    {
                        if (stations[objective].Contains(__instance.def))
                        {
                            bool active = __instance.TryGetComp<CompPowerTrader>()?.PowerOn ?? true;
                            ObjectiveStation station = StoryHandler.AllStations.Station(null, __instance);
                            if (station != null)
                            {
                                if (!station.objectives.Contains(objective))
                                {
                                    station.objectives.Add(objective);
                                }
                                return;
                            }
                            StoryHandler.AllStations.Add(new ObjectiveStation(__instance, objective, active));
                        }
                    }
                }
            }
        }

        [HarmonyPatch(typeof(Thing))]
        [HarmonyPatch("Kill")]
        static class KillThingPatch
        {            
            private static ThingDef temp;
            private static IntVec3 tempPos;
            private static Map tempMap;
            public static bool Prefix(Thing __instance)
            {
                tempMap = __instance.Map;
                tempPos = __instance.Position;
                temp = __instance.def;
                return true;
            }

            public static void Postfix(Thing __instance, DamageInfo? dinfo)
            {
                DamageInfo dinfo2 = new DamageInfo();
                if (dinfo.HasValue)
                { dinfo2 = dinfo.Value; }
                if (dinfo2.Instigator != null && dinfo2.Instigator is Pawn)
                {
                    if ((dinfo2.Instigator as Pawn).IsColonist)
                    {
                        StoryHandler.Missions.ForEach(m => m.objectives.Where(o => o.CurrentState == MOState.Active).Do(o => o.thingTracker?.ProcessTarget(temp, tempPos, tempMap, ObjectiveType.Destroy)));
                    }
                }
            }
        }

        [HarmonyPatch(typeof(Pawn))]
        [HarmonyPatch("Kill")]
        static class KillPawnPatch
        {
            private static PawnKindDef temp;
            private static IntVec3 tempPos;
            private static Map tempMap;
            public static bool Prefix(Pawn __instance)
            {
                tempMap = __instance.Map;
                tempPos = __instance.Position;
                temp = __instance.kindDef;
                return true;
            }

            public static void Postfix(Pawn __instance, DamageInfo? dinfo)
            {
                DamageInfo dinfo2 = new DamageInfo();
                if (dinfo.HasValue)
                { dinfo2 = dinfo.Value; }
                if (dinfo2.Instigator != null && dinfo2.Instigator.Faction == Faction.OfPlayerSilentFail)
                {
                    StoryHandler.Missions.ForEach(m => m.objectives.Where(o => o.CurrentState == MOState.Active).Do(o => o.thingTracker?.ProcessTarget(temp, tempPos, tempMap, ObjectiveType.Kill, null, __instance)));
                }
            }
        }
    }
}
