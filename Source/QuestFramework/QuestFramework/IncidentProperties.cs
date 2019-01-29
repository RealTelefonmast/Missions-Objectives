using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace StoryFramework
{
    public enum IncidentCondition
    {
        Failed,
        Finished,
        Started
    }

    public class IncidentProperties : Editable
    {
        [Unsaved]
        private IncidentWorker workerInt;
        public IncidentType type = IncidentType.None;
        public Type workerClass;
        public IncidentCategoryDef category;
        public string letterLabel;
        public string letterDesc;
        public LetterDef letterDef;
        public RaidSettings raidSettings = new RaidSettings();
        public TaleDef tale;
        public int points = -1;
        public float pointMultiplier = 1f;
        public List<ResearchProjectDef> researchUnlocks = new List<ResearchProjectDef>();
        public PositionFilter positionFilter = new PositionFilter();
        public SpawnSettings spawnSettings = new SpawnSettings();

        public IncidentProperties()
        {
            DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "category", "ThreatSmall");
            DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "letterDef", "NeutralEvent");
        }

        public override IEnumerable<string> ConfigErrors()
        {
            return base.ConfigErrors();
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

        private Faction Faction
        {
            get
            {
                return Find.FactionManager.AllFactions.First(f => f.def == raidSettings.faction);
            }
        }

        private string PawnKinds
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                foreach (ThingValue tv in spawnSettings.spawnList)
                {
                    sb.AppendLine("   - " + tv.value + " (x" + tv.PawnKindDef.LabelCap + ")");
                }
                return sb.ToString();
            }
        }

        private IncidentParms IncidentParms(Map map, TargetInfo target)
        {
            IncidentParms parms = StorytellerUtility.DefaultParmsNow(category, map);
            parms.points = points >= 0 ? points : parms.points;
            parms.faction = Faction;
            parms.forced = true;
            parms.raidStrategy = raidSettings.strategy;
            parms.raidArrivalMode = raidSettings.arriveMode;
            parms.spawnCenter = positionFilter.FindCell(map, spawnSettings.spawnList);
            if(type == IncidentType.Reward)
            {
                if (spawnSettings.mode == SpawnMode.Target)
                {
                    parms.spawnCenter = target.Cell;
                }
            }
            if (type == IncidentType.Raid)
            {
                parms.points = spawnSettings.spawnList.Sum(tv => tv.PawnKindDef.combatPower);
                parms.raidArrivalMode.Worker.TryResolveRaidSpawnCenter(parms);
            }
            parms.points *= pointMultiplier;
            return parms;
        }

        public void Notify_Execute(Map map, TargetInfo target, ObjectiveDef def, IncidentCondition? condition)
        {
            target = target.Thing ?? map.PlayerPawnsForStoryteller.RandomElement();
            if (Worker != null)
            {
                Worker.def = DefDatabase<IncidentDef>.AllDefs.First(i => i.workerClass == Worker.GetType()) ?? new IncidentDef();
                if (Worker.def.tale == null)
                {
                    Worker.def.tale = tale;
                }
                Worker.TryExecute(IncidentParms(map, target));
                return;
            }
            TryExecute(map, IncidentParms(map, target), def, condition);
        }

        private void TryExecute(Map map, IncidentParms parms, ObjectiveDef def, IncidentCondition? condition)
        {
            LookTargets targets = new LookTargets();
            string label = "";
            string message = "";
            if(type == IncidentType.Research)
            {
                StringBuilder sb = new StringBuilder();
                foreach(ResearchProjectDef project in researchUnlocks)
                {
                    sb.Append("   - " + project.LabelCap + "\n");
                    Find.ResearchManager.FinishProject(project);
                }
                label = "ResearchIncident_SMO".Translate();
                message = "ResearchIncidentDesc_SMO".Translate(sb.ToString());
            }
            if (type == IncidentType.Reward)
            {
                List<Thing> things = SpawnThings(out List<List<Thing>> groups, ref targets);
                SpawnMode mode = spawnSettings.mode;
                if (mode == SpawnMode.Target)
                {
                    SpawnAround(parms.spawnCenter, map, things, ref targets, out bool p);
                }
                if (mode == SpawnMode.Stockpile)
                {
                    List<IntVec3> cells = new List<IntVec3>();
                    List<Zone> zones = map.zoneManager.AllZones;
                    for (int i = 0; i < zones.Count; i++)
                    {
                        Zone zone = zones[i];
                        if (zone is Zone_Stockpile)
                        {
                            cells.AddRange(zone.Cells.Where(c => c.GetFirstItem(map) == null));
                        }
                    }
                    if (cells.Count < things.Count)
                    {
                        IntVec3 cell = IntVec3.Invalid;
                        if (map.areaManager.Home?.ActiveCells.Count() > 0)
                        {
                            PositionFilter filter = new PositionFilter();
                            filter.homeArea = AreaCheck.Prefer;
                            filter.roofs = AreaCheck.Avoid;
                            cell = filter.FindCell(map, spawnSettings.spawnList);
                        }
                        cell = cell.IsValid ? cell : parms.spawnCenter;
                        DropPodUtility.DropThingGroupsNear(cell, map, groups, 140, false, true, true);                       
                    }
                    else
                    {
                        foreach (Thing thing in things)
                        {
                            IntVec3 cell = cells.RandomElement();
                            cells.Remove(cell);
                            targets.targets.Add(GenSpawn.Spawn(thing, cells.RandomElement(), map));
                        }
                    }
                }
                if (mode == SpawnMode.DropPod)
                {
                    SpawnDropPod(parms.spawnCenter, map, groups);
                }
                if (condition.Value == IncidentCondition.Started)
                {
                    label = "StartingItems_SMO".Translate();
                    message = "StartingItemsDesc_SMO".Translate("'" + def.LabelCap + "'");
                }
                if (condition.Value == IncidentCondition.Finished || condition == null)
                {
                    label = "Reward_SMO".Translate();
                    if (def != null)
                    {
                        message = "RewardDesc_SMO".Translate("'" + def.LabelCap + "'");
                    }
                }
            }
            if (type == IncidentType.Appear)
            {
                List<Thing> things = SpawnThings(out List<List<Thing>> groups, ref targets);
                SpawnAround(parms.spawnCenter, map, things, ref targets, out bool p);
                label = p ? "AppearPlural_SMO".Translate() : "Appear_SMO".Translate();
                message = p ? "AppearDescPlural_SMO".Translate() : "AppearDesc_SMO".Translate(targets.targets?[0].Thing.LabelCap);
            }
            if (type == IncidentType.Skyfaller)
            {
                int count = 0;
                var sortedList = spawnSettings.skyfallers.OrderBy(x => x.chance).ToArray();
                for(int i = 0; i < sortedList.Count(); i++)
                {
                    ThingSkyfaller skyfaller = sortedList[i];
                    if (Rand.Chance(skyfaller.chance) || (spawnSettings.singleChance && i == sortedList.Count() - 1))
                    {
                        List<IntVec3> pos = positionFilter.FindCells(map, skyfaller.amount, null, spawnSettings.skyfallers.ThingDefs());
                        for (int ii = 0; ii < skyfaller.amount; ii++)
                        {
                            count++;
                            Skyfaller skyfallerThing = SkyfallerMaker.SpawnSkyfaller(skyfaller.skyfallerDef, skyfaller.innerThing, pos[ii], map);
                            targets.targets.Add(skyfallerThing.innerContainer[0]);
                        }
                        if (spawnSettings.singleChance) { break; }
                    }
                }
                bool plural = count > 1;
                label = !plural ? "Skyfaller_SMO".Translate() : "SkyfallerPlural_SMO".Translate();
                message = !plural ? "SkyfallerDesc_SMO".Translate(targets.targets?[0].Thing.LabelCap) : "SkyfallerDescPlural_SMO".Translate();
            }
            if (type == IncidentType.Raid)
            {
                List<Pawn> raiders = new List<Pawn>();
                foreach (ThingValue tv in spawnSettings.spawnList)
                {
                    if (Rand.Chance(tv.chance))
                    {
                        for (int i = 0; i < tv.value; i++)
                        {
                            Pawn pawn = PawnGenerator.GeneratePawn(tv.PawnKindDef, parms.faction);
                            raiders.Add(pawn);
                            targets.targets.Add(pawn);
                        }
                    }
                }
                parms.raidArrivalMode.Worker.Arrive(raiders, parms);
                raidSettings.MakeLords(parms, map, raiders);
                Find.StoryWatcher.statsRecord.numRaidsEnemy++;
                label = "Raid_SMO".Translate();
                message = "RaidDesc_SMO".Translate(PawnKinds);
            }
            Find.LetterStack.ReceiveLetter(letterLabel ?? label, letterDesc ?? message, letterDef, targets, type == IncidentType.Raid ? Faction : null, null);
        }

        public void SpawnDropPod(IntVec3 root, Map map, List<List<Thing>> groups)
        {
            DropPodUtility.DropThingGroupsNear(root, map, groups, 140, false, true, true);
        }

        public void SpawnAround(IntVec3 root, Map map, List<Thing> things, ref LookTargets lastThing, out bool plural)
        {
            plural = things.Count > 1;
            if (spawnSettings.mode == SpawnMode.Scatter)
            {
                List<IntVec3> cells = positionFilter.FindCells(map, things.Count, spawnSettings.spawnList);
                for (int i = 0; i < things.Count; i++)
                {
                    GenSpawn.Spawn(things[i], cells[i], map);
                }
                return;
            }
            foreach (Thing thing in things)
            {
                GenPlace.TryPlaceThing(thing, root, map, ThingPlaceMode.Near);
                lastThing = thing;
            }
        }

        private List<Thing> SpawnThings(out List<List<Thing>> groups, ref LookTargets targets)
        {
            groups = new List<List<Thing>>();
            List<Thing> things = new List<Thing>();
            foreach (ThingValue tv in spawnSettings.spawnList)
            {
                if (Rand.Chance(tv.chance))
                {
                    List<Thing> thingList = new List<Thing>();
                    if (tv.IsPawnKindDef)
                    {
                        for (int i = 0; i < tv.value; i++)
                        {
                            Pawn pawn = PawnGenerator.GeneratePawn(tv.PawnKindDef, Faction.OfPlayer);
                            things.Add(pawn);
                            thingList.Add(pawn);
                            targets.targets.Add(pawn);
                        }
                    }
                    else
                    {
                        int totalStack = tv.value;
                        while (totalStack > 0)
                        {
                            ThingDef stuff = tv.ResolvedStuff;
                            Thing thing = ThingMaker.MakeThing(tv.ThingDef, stuff);
                            totalStack--;
                            if (thing.TryGetQuality(out QualityCategory qc))
                            {
                                thing.TryGetComp<CompQuality>().SetQuality(tv.QualityCategory, ArtGenerationContext.Outsider);
                            }
                            for (int i = 1; i < thing.def.stackLimit && totalStack > 0; i++)
                            {
                                totalStack--;
                                thing.stackCount++;
                            }
                            targets.targets.Add(thing);
                            things.Add(thing);
                            thingList.Add(thing);
                        }
                    }
                    groups.Add(thingList);
                }
            }
            return things;
        }
    }
}
