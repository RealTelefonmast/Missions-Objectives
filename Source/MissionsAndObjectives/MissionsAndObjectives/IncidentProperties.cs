using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace MissionsAndObjectives
{
    public class IncidentProperties : Editable
    {
        [Unsaved]
        private IncidentWorker workerInt;

        public IncidentType type = IncidentType.None;

        public Type workerClass;

        public float pointMultiplier = 1;

        public int pointsOverride = 0;

        public List<ThingDef> spawnList = new List<ThingDef>();

        public List<ThingDef> randomSpawnThing = new List<ThingDef>();

        public ThingDef spawnAtDef;

        public ThingDef spawnThing;

        public ThingDef skyfallerDef;

        public override IEnumerable<string> ConfigErrors()
        {
            if (type == IncidentType.None)
            {
                Log.Error("Incident properties missing type");
            }
            if (workerClass != null && type != IncidentType.CustomWorker)
            {
                yield return "workerClass active but type is set to " + type + " instead of 'CustomWorker'";
            }
            if (workerClass != null && (spawnList.NullOrEmpty() || randomSpawnThing.NullOrEmpty() || spawnThing != null))
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
            return StorytellerUtility.DefaultParmsNow(Find.Storyteller.def, IncidentCategory.ThreatSmall, map);
        }

        public void Notify_Execute(Map map = null)
        {
            if (Worker != null)
            {
                Worker.TryExecute(Parms(map));
                return;
            }
            TryExecute();
        }

        private void TryExecute()
        {
            foreach (ThingDef def in spawnList)
            {

                if (type == IncidentType.Skyfaller)
                {

                }
                if (type == IncidentType.RewardAtTarget)
                {

                }
                if (type == IncidentType.RewardAtStockpile)
                {

                }
                if (type == IncidentType.Appear)
                {
                }
            }
        }
    }

    public enum IncidentType
    {
        CustomWorker,
        Skyfaller,
        RewardAtTarget,
        RewardAtStockpile,
        Appear,
        None
    }
}
