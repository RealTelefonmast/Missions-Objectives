using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI.Group;
using Verse.AI;
using System.Text;

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

        public string raidFaction = "Pirate";

        public PawnsArriveMode arriveMode = PawnsArriveMode.EdgeWalkIn;

        public float pointMultiplier = 1;

        public int pointsOverride = -1;

        public bool random = false;

        public List<MissionDef> missionUnlocks = new List<MissionDef>();

        public List<ThingValue> spawnList = new List<ThingValue>();

        public List<ThingWithSkyfaller> skyfallerSpawnList = new List<ThingWithSkyfaller>();

        public IncidentFilter filter = new IncidentFilter();

        public override IEnumerable<string> ConfigErrors()
        {
            if (type == IncidentType.None)
            {
                Log.Error("Incident properties missing type");
            }
            if (type == IncidentType.UnlockMission && missionUnlocks.NullOrEmpty())
            {
                yield return "Incident properties uses 'UnlockMission' type but 'missionUnlocks' is empty.";
            }
            if(type == IncidentType.Skyfaller && skyfallerSpawnList.NullOrEmpty())
            {
                yield return "Incident properties uses 'Skyfaller' type but 'skyfallerSpawnList' is empty.";
            }
            if((type == IncidentType.Reward || type == IncidentType.RewardAtTarget || type == IncidentType.RewardDropPod || type == IncidentType.Appear) && spawnList.NullOrEmpty())
            {
                yield return "Incident properties uses '" + type + "' type but 'spawnList' is empty.";
            }
            if (workerClass != null && type != IncidentType.CustomWorker)
            {
                yield return "Incident properties uses active 'workerClass' but type is not 'CustomWorker'.";
            }
            if(type == IncidentType.Raid)
            {
                if(Faction == null)
                {
                    yield return "Incident properties of 'Raid' type cannot find a usable Faction with def named '" + raidFaction + "'.";
                }
                if (spawnList.NullOrEmpty())
                {
                    yield return "Incident properties uses '" + type + "' type but 'spawnList' is empty.";
                }
                else
                {
                    foreach (ThingValue tv in spawnList)
                    {
                        if (tv.PawnKindDef == null)
                        {
                            yield return "Incident properties uses 'Raid' type but 'spawnList' contains '" + tv.defName + "' which has no corresponding PawnKindDef.";
                        }
                    }
                }
            }
        }

        public string PawnKinds
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                foreach(ThingValue tv in spawnList)
                {
                    sb.AppendLine("    " + tv.value + "x " + tv.PawnKindDef.LabelCap);
                }
                return sb.ToString();
            }
        }

        public string ThingDefs
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                foreach (ThingValue tv in spawnList)
                {
                    sb.AppendLine("    " + tv.value + "x " + tv.ThingDef.LabelCap);
                }
                return sb.ToString();
            }
        }

        public IncidentProperties()
        {
        }

        public Faction Faction
        {
            get
            {
                return Find.FactionManager.AllFactions.ToList().Find(f => f.def.defName == raidFaction);
            }
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
            parms.faction = Faction;
            if (type == IncidentType.Raid)
            {
                parms.points = spawnList.Sum(tv => tv.PawnKindDef.combatPower);
                if (parms.raidStrategy != null)
                {
                    return parms;
                }
                parms.raidStrategy = (from d in DefDatabase<RaidStrategyDef>.AllDefs
                where d.Worker.CanUseWith(parms)
                select d).RandomElementByWeight((RaidStrategyDef d) => d.Worker.SelectionChance(map));
            }
            return parms;
        }

        public void Notify_Execute(Map map, TargetInfo target)
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
            TryExecute(map, target);
        }

        private void TryExecute(Map map, TargetInfo target)
        {
            TargetInfo lastThing = null;
            if (type == IncidentType.UnlockMission)
            {
                if (!random)
                {
                    foreach (MissionDef mission in missionUnlocks)
                    {
                        WorldComponent_Missions.MissionHandler.AddNewMission(mission);
                    }
                    Messages.Message("NewMission".Translate(), MessageTypeDefOf.NeutralEvent);
                }
                else
                {
                    MissionDef randomMission = missionUnlocks.RandomElement();
                    WorldComponent_Missions.MissionHandler.AddNewMission(randomMission);
                    Messages.Message("NewMissions".Translate(), MessageTypeDefOf.NeutralEvent);
                }
                return;
            }
            if(type == IncidentType.Skyfaller)
            {
                if (!random)
                {
                    foreach(ThingWithSkyfaller skyThing in skyfallerSpawnList)
                    {
                        lastThing = SkyfallerMaker.SpawnSkyfaller(skyThing.skyfaller, skyThing.def, SpawnPosition(filter, map), map);
                    }
                }
                else
                {
                    ThingWithSkyfaller randomSkyThing = skyfallerSpawnList.RandomElement();
                    lastThing = SkyfallerMaker.SpawnSkyfaller(randomSkyThing.skyfaller, randomSkyThing.def, SpawnPosition(filter, map), map);
                }
                Messages.Message("Skyfaller".Translate(), lastThing, MessageTypeDefOf.NeutralEvent);
                return;
            }
            if (type == IncidentType.Appear)
            {
                IntVec3 center = SpawnPosition(filter, map, spawnList);
                SpawnAround(center, map, ref lastThing, "ThingsAppeared".Translate());
            }
            if (type == IncidentType.Reward)
            {
                Zone zone = map.zoneManager.AllZones.Find(z => !(z is Zone_Growing) && z.Cells.RandomElement().GetRoom(map).CellCount >= spawnList.Sum(tv => tv.value));
                if(zone != null)
                {
                    IntVec3 center = zone.Cells.RandomElement();
                    SpawnAround(center, map, ref lastThing, "RewardsAppeared".Translate());
                }
            }
            if(type == IncidentType.RewardAtTarget)
            {
                IntVec3 center = target.Cell;
                SpawnAround(center, map, ref lastThing, "RewardsAppeared".Translate());
            }
            if(type == IncidentType.RewardDropPod)
            {
                IntVec3 center = SpawnPosition(filter, map, spawnList);
                List<List<Thing>> groups = new List<List<Thing>>();                
                if (!random)
                {
                    foreach (ThingValue thingValue in spawnList)
                    {
                        List<Thing> thingList = new List<Thing>();
                        for (int i = 0; i < thingValue.value; i++)
                        {
                            thingList.Add(ThingMaker.MakeThing(thingValue.ThingDef));
                        }
                        groups.Add(thingList);
                    }
                }
                else
                {
                    ThingValue thingValue = spawnList.RandomElement();
                    List<Thing> thingList = new List<Thing>();
                    for (int i = 0; i < thingValue.value; i++)
                    {
                        thingList.Add(ThingMaker.MakeThing(thingValue.ThingDef));
                    }
                    groups.Add(thingList);
                }
                DropPodUtility.DropThingGroupsNear(center, map, groups, 140, false, true, true, true);
                Find.LetterStack.ReceiveLetter("RewardDropPod".Translate(), "RewardDropPodDesc".Translate( new object[] {
                    ThingDefs
                }), LetterDefOf.PositiveEvent, new TargetInfo(center, map), null);
            }
            if (type == IncidentType.Raid)
            {
                IntVec3 center = DropCellFinder.FindRaidDropCenterDistant(map);
                List<Pawn> pawnList = new List<Pawn>();
                foreach (ThingValue tv in spawnList)
                {
                    for (int i = 0; i < tv.value; i++)
                    {
                        Pawn pawn = PawnGenerator.GeneratePawn(tv.PawnKindDef, Faction);
                        pawnList.Add(pawn);
                    }
                }
                if (arriveMode == PawnsArriveMode.CenterDrop || arriveMode == PawnsArriveMode.EdgeDrop)
                {
                    DropPodUtility.DropThingsNear(center, map, pawnList.Cast<Thing>(), 140, false, true, true, false);
                    lastThing = new TargetInfo(center, map, false);
                }
                else
                {
                    foreach (Pawn pawn in pawnList)
                    {
                        IntVec3 loc = CellFinder.RandomClosewalkCellNear(center, map, 8, null);
                        GenSpawn.Spawn(pawn, loc, map, Rot4.Random, false);
                        lastThing = pawn;
                    }
                }
                IncidentParms parms = Parms(map);
                Lord lord = LordMaker.MakeNewLord(Faction, parms.raidStrategy.Worker.MakeLordJob(parms, map), map, pawnList);
                AvoidGridMaker.RegenerateAvoidGridsFor(Faction, map);
                Find.TickManager.slower.SignalForceNormalSpeedShort();
                Find.StoryWatcher.statsRecord.numRaidsEnemy++;
                Find.LetterStack.ReceiveLetter("RaidCustom".Translate(), "RaidCustomDesc".Translate(new object[] {
                    PawnKinds
                }), LetterDefOf.ThreatBig, lastThing, null);
            }
        }

        public void DropPodReward()
        {

        }

        private void SpawnAround(IntVec3 center, Map map, ref TargetInfo lastThing, string message)
        {
            List<Thing> thingList = new List<Thing>();
            if (!random)
            {
                foreach (ThingValue thingValue in spawnList)
                {
                    for (int i = 0; i < thingValue.value; i++)
                    {
                        thingList.Add(ThingMaker.MakeThing(thingValue.ThingDef));
                    }
                }
            }
            else
            {
                ThingValue thingValue = spawnList.RandomElement();
                for (int i = 0; i < thingValue.value; i++)
                {
                    thingList.Add(ThingMaker.MakeThing(thingValue.ThingDef));
                }
            }
            foreach (Thing thing in thingList)
            {
                GenPlace.TryPlaceThing(thing, center, map, ThingPlaceMode.Near);
                lastThing = thing;
            }
            Messages.Message(message, lastThing, MessageTypeDefOf.NeutralEvent);
            return;
        }

        public IntVec3 SpawnPosition(IncidentFilter filter, Map map, List<ThingValue> list = null)
        {
            float minRadius = 0f;
            int numCells = 0;
            foreach(ThingValue tv in list)
            {
                numCells += (int)Math.Round((double)(tv.value / tv.ThingDef.stackLimit), 0, MidpointRounding.AwayFromZero);
            }
            //minRadius = GenRadial.RadiusOfNumCells(numCells);

            IntVec3 spawnPos = IntVec3.Invalid;
            List<IntVec3> AllCells = new List<IntVec3>();
            AllCells.AddRange(map.AllCells.Where(c => c.IsValid && !c.Fogged(map)));

            if(numCells > 0f)
            {
                AllCells.RemoveAll(v => v.GetRoom(map)?.CellCount < numCells);
            }
            if (filter != null)
            {
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
            }
            spawnPos = AllCells.RandomElement();
            return spawnPos;
        }
    }

    public enum IncidentType
    {       
        CustomWorker,
        UnlockMission,
        RewardDropPod,
        RewardAtTarget,
        Reward,
        Skyfaller,
        Appear,
        Raid,
        None
    }
}
