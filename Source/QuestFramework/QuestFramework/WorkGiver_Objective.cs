using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI;

namespace StoryFramework
{
    public class WorkGiver_Objective : WorkGiver_Scanner
    {
        private Objective objective;

        private StoryManager Story
        {
            get
            {
                return StoryManager.StoryHandler;
            }
        }

        public override bool Prioritized => true;

        public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForGroup(ThingRequestGroup.Nothing);

        public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
        {
            if (Story.AllStations.Count > 0)
            {
                return Story.ActiveStations;
            }
            return null;
        }

        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            List<ObjectiveDef> objectives = Story.AllStations.StationObjectives(t);
            foreach(ObjectiveDef objective in objectives)
            {
                if (objective.CanBeDoneBy(pawn, t))
                {
                    this.objective = Story.GetObjective(objective);
                    return true;
                }
            }
            return false;
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            if (pawn.CanReserveAndReach(t, PathEndMode.Touch, Danger.Some))
            {
                return new Job_Story(SCD.DoMissionObjective, t, objective);
            }
            return null;
        }
    }
}
