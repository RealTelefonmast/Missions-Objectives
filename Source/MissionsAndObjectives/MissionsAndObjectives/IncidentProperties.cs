using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace MissionsAndObjectives
{
    public class IncidentProperties : Editable
    {
        [Unsaved]
        private IncidentWorker workerInt;

        public IncidentType type = IncidentType.None;

        public IncidentCategory category = IncidentCategory.ThreatSmall;

        public Type workerClass;

        public TaleDef tale;

        public float pointMultiplier = 1;

        public int pointsOverride = -1;

        public bool random = false;

        public List<MissionDef> missionUnlocks = new List<MissionDef>();

        public List<ThingDef> spawnList = new List<ThingDef>();

        public List<ThingWithSkyfaller> skyfallerSpawnList = new List<ThingWithSkyfaller>();

        public IncidentFilter filter;

        public override IEnumerable<string> ConfigErrors()
        {
            if (type == IncidentType.None)
            {
                Log.Error("Incident properties missing type");
            }
            if(type == IncidentType.Skyfaller && skyfallerSpawnList.NullOrEmpty())
            {
                yield return "incident properties use Skyfaller type, but have no skyaller def";
            }
            if ((type == IncidentType.RewardAtStockpile || type == IncidentType.RewardAtTarget) && spawnList.NullOrEmpty() && skyfallerSpawnList.Count > 0)
            {
                yield return "incident properties use "+ type + " type, but have no rewards defined or use the skyfaller list";
            }   
            if(type == IncidentType.UnlockMission && missionUnlocks.NullOrEmpty())
            {
                yield return "incident properties use UnlockMission type but have no missions to unlock";
            }
            if(type == IncidentType.Appear && spawnList.NullOrEmpty())
            {
                yield return "incident properties use Appear type but have no things in spawnList";
            }
            if (workerClass != null && type != IncidentType.CustomWorker)
            {
                yield return "workerClass active but type is set to " + type + " instead of 'CustomWorker'";
            }
            if (workerClass != null && spawnList.NullOrEmpty())
            {
                yield return "workerClass active with thingDef parameters, remove everything except the workerClass.";
            }
        }

        public IncidentProperties()
        {
        }

        public IncidentWorker Worker
        {
            get
            {
                if (this.workerInt == null)
                {
                    if (workerClass != null)
                    {
                        this.workerInt = (IncidentWorker)Activator.CreateInstance(this.workerClass);
                    }
                }
                return this.workerInt;
            }
        }

        private IncidentParms Parms(Map map)
        {
            IncidentParms parms = StorytellerUtility.DefaultParmsNow(Find.Storyteller.def, category, map);
            if (pointsOverride >= 0)
            {
                parms.points = pointsOverride;
            }
            parms.points *= pointMultiplier;
            return parms;
        }

        public void Notify_Execute(Map map)
        {
            if (Worker != null)
            {
                Worker.def = DefDatabase<IncidentDef>.AllDefs.ToList().Find(i => i.workerClass == Worker.GetType()) ?? new IncidentDef();
                if(Worker.def.tale == null)
                {
                    Worker.def.tale = tale;
                }
                Worker.TryExecute(Parms(map));
                return;
            }
            TryExecute(map);
        }

        private void TryExecute(Map map, Thing target = null)
        {
            if(type == IncidentType.UnlockMission)
            {
                foreach (MissionDef mission in missionUnlocks)
                {
                    WorldComponent_Missions.MissionHandler.AddNewMission(mission);
                }
                return;
            }
            if (type == IncidentType.Skyfaller)
            {
                if (!random)
                {
                    foreach (ThingWithSkyfaller skyThing in skyfallerSpawnList)
                    {
                        SkyfallerMaker.SpawnSkyfaller(skyThing.skyfaller, skyThing.def, SpawnPosition(filter, map), map);
                    }
                    return;
                }
                ThingWithSkyfaller skyThing2 = skyfallerSpawnList.RandomElement();
                SkyfallerMaker.SpawnSkyfaller(skyThing2.skyfaller, skyThing2.def, SpawnPosition(filter, map), map);
                return;
            }
            if (type == IncidentType.RewardAtTarget)
            {
                foreach (ThingDef def in spawnList)
                {
                    IntVec3 loc = target.Position;
                    Thing spawnThing = ThingMaker.MakeThing(def);
                    GenSpawn.Spawn(spawnThing, loc, map);
                }
                return;
            }
            if (type == IncidentType.RewardAtStockpile)
            {
                bool messageFlag = false;
                foreach (ThingDef def in spawnList)
                {
                    IntVec3 loc = map.zoneManager.AllZones.Find(zone => zone is Zone_Stockpile).Cells.Find(cell => cell.GetThingList(map).NullOrEmpty());
                    if (!loc.IsValid)
                    {
                        loc = map.areaManager.Home.ActiveCells.RandomElement();
                        if (!loc.IsValid)
                        {
                            Pawn pawn = map.mapPawns.FreeColonists.RandomElement();
                            loc = pawn.Position;
                            if (!messageFlag)
                            {
                                Messages.Message("RewardSpawnedAtPawn", pawn, MessageTypeDefOf.NeutralEvent);
                                messageFlag = true;
                            }
                        }
                    }
                    Thing spawnThing = ThingMaker.MakeThing(def);
                    GenSpawn.Spawn(spawnThing, loc, map);
                }
                return;
            }
            if (type == IncidentType.Appear)
            {
                foreach(ThingDef def in spawnList)
                {
                    Thing spawnThing = ThingMaker.MakeThing(def);
                    GenSpawn.Spawn(spawnThing, SpawnPosition(filter, map), map);
                }
                return;
            }
        }

        public IntVec3 SpawnPosition(IncidentFilter filter, Map map)
        {
            IntVec3 spawnPos = IntVec3.Invalid;
            List<IntVec3> AllCells = new List<IntVec3>();
            AllCells.AddRange(map.AllCells);

            if (!filter.terrainToAvoid.NullOrEmpty())
            {
                AllCells.RemoveAll(v => filter.terrainToAvoid.Contains(v.GetTerrain(map)));
            }
            if (!filter.spawnAt.NullOrEmpty())
            {
                AllCells.RemoveAll(v => v.GetThingList(map).Any(t => !filter.spawnAt.Contains(t.def)));
            }
            if (!filter.distanceFromThings.NullOrEmpty())
            {
                AllCells.RemoveAll(v => filter.distanceFromThings.All(dist => map.listerThings.ThingsOfDef(dist.ThingDef).Any(t => v.DistanceTo(t.Position) < dist.value)));
            }

            if (filter.avoidHome == AreaCheck.Avoid)
            {
                AllCells.RemoveAll(v => map.areaManager.Home[v]);
            }
            if (filter.avoidHome == AreaCheck.Prefer)
            {
                AllCells.RemoveAll(v => !map.areaManager.Home[v]);
            }
            if (filter.avoidRoofs == AreaCheck.Avoid)
            {
                AllCells.RemoveAll(v => v.Roofed(map));
            }
            if (filter.avoidRoofs == AreaCheck.Prefer)
            {
                AllCells.RemoveAll(v => !v.Roofed(map));
            }
            spawnPos = AllCells.RandomElement();
            return spawnPos;
        }
    }

    public enum IncidentType
    {
        UnlockMission,
        CustomWorker,
        Skyfaller,
        RewardAtTarget,
        RewardAtStockpile,
        Appear,
        None
    }
}
