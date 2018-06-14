﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI;
using UnityEngine;

namespace MissionsAndObjectives
{
    public class ObjectiveDef : Def
    {
        public List<ThingDef> targetThings = new List<ThingDef>();

        public List<ThingDef> stationDefs = new List<ThingDef>();

        public List<ThingDef> rewards = new List<ThingDef>();

        public List<PawnKindDef> targetPawns = new List<PawnKindDef>();

        public float distanceToTarget = 0;

        public RewardType rewardType = RewardType.None;

        public ObjectiveType objectiveType = ObjectiveType.None;

        public List<SkillRequirement> skillRequirements = new List<SkillRequirement>();

        public List<ObjectiveDef> objectiveRequisites = new List<ObjectiveDef>();

        public List<ObjectiveDef> dependantOn = new List<ObjectiveDef>();

        public List<IncidentProperties> incidentsOnCompletion = new List<IncidentProperties>();

        public List<IncidentProperties> incidentsOnFail = new List<IncidentProperties>();

        public int workCost = 0;

        public float timerDays = 0;

        public List<string> images = new List<string>();

        public override IEnumerable<string> ConfigErrors()
        {
            if (objectiveType == ObjectiveType.None)
            {
                Log.Error("Missing objective type for " + this.defName);
            }
            if (objectiveType == ObjectiveType.Wait && timerDays <= 0f)
            {
                yield return "Objective of type 'Wait' is missing a timer value.";
            }
            if (((objectiveType == ObjectiveType.Discover || objectiveType == ObjectiveType.Craft || objectiveType == ObjectiveType.Construct || objectiveType == ObjectiveType.Destroy) && targetThings.NullOrEmpty()) || (objectiveType == ObjectiveType.Hunt && targetPawns.NullOrEmpty()))
            {
                yield return "Objective is missing a target.";
            }
            if (objectiveType == ObjectiveType.Examine && workCost <= 0)
            {
                yield return "Objective of type 'Examine' is missing a work value.";
            }
            if (objectiveType == ObjectiveType.Examine && (targetThings.NullOrEmpty() || targetPawns.NullOrEmpty()))
            {
                yield return "Objective of type 'Examine' is missing a target.";
            }
            if (objectiveType == ObjectiveType.Research && workCost <= 0)
            {
                yield return "Objective of type 'Research' is missing a work value.";
            }
            if (objectiveType == ObjectiveType.Research && stationDefs.NullOrEmpty())
            {
                yield return "Objective of type 'Research' is missing at least one defined work station.";
            }
        }

        public bool IsFinished
        {
            get
            {
                return Find.World.GetComponent<WorldComponent_Missions>().Missions.Find(m => m.Objectives.Any(o => o.def == this)).Objectives.Find(o => o.def == this).Finished;
            }
        }

        public bool IsManualJob
        {
            get
            {
                if (objectiveType == ObjectiveType.Construct || objectiveType == ObjectiveType.Craft || objectiveType == ObjectiveType.Destroy || objectiveType == ObjectiveType.Wait)
                {
                    return true;
                }
                return false;
            }
        }

        public ThingDef BestPotentialStationDef
        {
            get
            {
                float num = 0f;
                foreach (ThingDef def in stationDefs)
                {
                    float num2 = def.statBases.Sum(s => s.value);
                    if (num2 > num)
                    {
                        num = num2;
                    }
                }
                return stationDefs.Find(x => x.statBases.Sum(s => s.value) == num);
            }
        }

        public StatDef EffectiveStat
        {
            get
            {
                switch (objectiveType)
                {
                    case ObjectiveType.Construct:
                        return StatDefOf.ConstructionSpeed;
                    case ObjectiveType.Craft:
                        return StatDefOf.WorkSpeedGlobal;
                    case ObjectiveType.Examine:
                        return StatDefOf.ResearchSpeed;
                    case ObjectiveType.Research:
                        return StatDefOf.ResearchSpeed;
                }
                return null;
            }
        }

        public int TimerTicks
        {
            get
            {
                return Mathf.RoundToInt(GenDate.TicksPerDay * timerDays);
            }
        }

        public StatDef EffectiveStatFactor
        {
            get
            {
                switch (objectiveType)
                {
                    case ObjectiveType.Craft:
                        return StatDefOf.WorkTableWorkSpeedFactor;
                    case ObjectiveType.Research:
                        return StatDefOf.ResearchSpeedFactor;
                }
                return null;
            }
        }
    }

    public enum ObjectiveType
    {
        Research,
        Discover,
        Destroy,
        Examine,
        Construct,
        Craft,
        Wait,
        Hunt,
        None
    }

    public enum RewardType
    {
        DropPods,
        SpawnOnObjective,
        None
    }
}
