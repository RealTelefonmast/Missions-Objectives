using System;
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
        public ObjectiveType objectiveType = ObjectiveType.None;

        public List<SkillRequirement> skillRequirements = new List<SkillRequirement>();

        public List<ObjectiveDef> objectiveRequisites = new List<ObjectiveDef>();

        public List<ThingValue> targets = new List<ThingValue>();

        public List<ThingDef> stationDefs = new List<ThingDef>();

        public List<ObjectiveDef> dependantOn = new List<ObjectiveDef>();

        public List<ObjectiveDef> failOn = new List<ObjectiveDef>();

        public List<IncidentProperties> incidentsOnCompletion = new List<IncidentProperties>();

        public List<IncidentProperties> incidentsOnFail = new List<IncidentProperties>();

        public bool hideOnComplete = false;

        public bool anyTarget = false;

        public int workAmount = -1;

        public float timerDays = -1;

        public List<string> images = new List<string>();

        public override IEnumerable<string> ConfigErrors()
        {
            if (objectiveType == ObjectiveType.None)
            {
                Log.Error("is missing an 'objectiveType'.");
            }
            if (objectiveType == ObjectiveType.Craft || objectiveType == ObjectiveType.Construct || objectiveType == ObjectiveType.Destroy || objectiveType == ObjectiveType.Hunt || objectiveType == ObjectiveType.Discover || objectiveType == ObjectiveType.Examine)
            {
                if (targets.NullOrEmpty())
                    yield return "Objective of type '" + objectiveType + "' has an empty 'targets' list.";
            }
            if(objectiveType == ObjectiveType.Hunt)
            {
                //Nothing so far
            }
            if (objectiveType == ObjectiveType.Research || objectiveType == ObjectiveType.Examine)
            {
                if (workAmount < 0)
                    yield return "Objective of type '" + objectiveType + "' has 'workAmount' below 0.";
            }
            if (objectiveType == ObjectiveType.Wait)
            {
                if (timerDays < 0)
                    yield return "Obejctive of type 'Wait' has no 'timerDays' set.";
            }
        }

        public bool IsFinished
        {
            get
            {
                Objective obj;
                if((obj = Find.World.GetComponent<WorldComponent_Missions>().Missions.Find(m => m.Objectives.Any(o => o.def == this))?.Objectives.Find(o => o.def == this)) != null)
                {
                    return obj.Finished;
                }
                return false;
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

    public enum ObjectiveType : byte
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
}
