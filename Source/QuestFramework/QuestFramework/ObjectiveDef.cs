using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace StoryFramework
{
    public class ObjectiveDef : Def
    {
        public ObjectiveType type = ObjectiveType.None;
        public Type customClass = typeof(Objective);
        public CustomObjectiveSettings customSettings = new CustomObjectiveSettings();
        public Requisites requisites;
        public List<SkillRequirement> skillRequirements = new List<SkillRequirement>();
        public TargetSettings targetSettings;
        public TravelSettings travelSettings;
        //public FailConditions failConditions = new FailConditions();
        public TimerSettings timer = new TimerSettings();
        public IncidentProperties result;
        public List<IncidentProperties> incidentsOnStart = new List<IncidentProperties>();
        public List<IncidentProperties> incidentsOnCompletion = new List<IncidentProperties>();
        public List<IncidentProperties> incidentsOnFail = new List<IncidentProperties>();
        public List<string> images = new List<string>();
        //public bool useMarker = true;
        public bool hideOnComplete = false;
        public int workAmount = -1;

        public override IEnumerable<string> ConfigErrors()
        {
            if (ParentDef.repeatable)
            {
                if(timer.GetTotalTime > 0)
                {
                    yield return "A timer cannot be set in a repeatable mission.";
                }
            }
            if (type == ObjectiveType.None)
            {
                Log.Error(defName + " is missing an ObjectiveType (defined as <type> </type>).");
            }
            if(type == ObjectiveType.Custom)
            {
                if(customClass == null)
                {
                    yield return "Missing customClass.";
                }
            }
            if (type == ObjectiveType.Wait)
            {
                if (timer.GetTotalTime == 0)
                {
                    yield return "Objective of Type 'Wait' has no timer set up!";
                }
            }
            if (!targetSettings?.targets.NullOrEmpty() ?? false)
            {
                targetSettings.CheckForDuplicates(this);
            }
            if (type == ObjectiveType.Destroy || type == ObjectiveType.Kill || type == ObjectiveType.ConstructOrCraft || type == ObjectiveType.Own)
            {
                if (targetSettings == null)
                {
                    yield return "TargetSettings not set up!";
                }
                else if(type != ObjectiveType.Kill && type != ObjectiveType.Recruit && (targetSettings?.targets.Any(tv => tv.IsPawnKindDef) ?? false))
                {
                    yield return "Target list contains pawn although type is " + type;
                }
            }
            if(type == ObjectiveType.PawnCheck)
            {
                if (targetSettings?.pawnSettings == null)
                {
                    yield return "PawnSettings in targetSettings are empty.";
                }
            }
            if (type == ObjectiveType.Research)
            {
                if (targetSettings?.targets.NullOrEmpty() ?? true)
                {
                    yield return "Empty target list! Targets in this list will be used as stations for the research.";
                }
                if (workAmount <= 0)
                {
                    yield return "Missing workAmount.";
                }
            }
            if (type == ObjectiveType.Travel)
            {
                if (travelSettings == null)
                {
                    yield return "Type 'Travel' requires TravelSettings to be set up!";
                }
            }
        }

        public MissionDef ParentDef
        {
            get
            {
                return DefDatabase<MissionDef>.AllDefsListForReading.Find(m => m.objectives.Contains(this));
            }
        }

        public bool IsFinished
        {
            get
            {
                return CurrentState == MOState.Finished;
            }
        }

        public MOState CurrentState
        {
            get
            {
                return StoryManager.StoryHandler.GetObjectiveState(this);
            }
        }
       
        public bool IsManualJob
        {
            get
            {
                if(type == ObjectiveType.ConstructOrCraft || type == ObjectiveType.Destroy || type == ObjectiveType.Kill || type == ObjectiveType.Research)
                {
                    return true;
                }
                return false;
            }
        }

        public ThingDef BestPotentialStation
        {
            get
            {
                float num = 0f;
                for(int i = 0; i < targetSettings?.targets.Count; i++)
                {
                    ThingDef def = targetSettings.targets[i].ThingDef;
                    float num2 = def.statBases.Sum(s => s.value);
                    if(num2 > num)
                    {
                        num = num2;
                    }
                }
                return targetSettings?.targets.Find(tv => tv.ThingDef.statBases.Sum(s => s.value) == num).ThingDef;
            }
        }

        public StatDef EffectiveStat
        {
            get
            {
                switch (type)
                {
                    case ObjectiveType.ConstructOrCraft:
                        return StatDefOf.ConstructionSpeed;
                    case ObjectiveType.Research:
                        return StatDefOf.ResearchSpeed;
                }
                return null;
            }
        }
    }
}
