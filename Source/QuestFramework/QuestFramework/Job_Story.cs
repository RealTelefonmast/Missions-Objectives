using Verse;
using Verse.AI;

namespace StoryFramework
{
    public class Job_Story : Job, IExposable
    {
        public Objective objective;

        public Job_Story() { }

        public Job_Story(JobDef def, LocalTargetInfo targetA, Objective objective)
        {
            this.def = def;
            this.targetA = targetA;
            this.objective = objective;            
            this.loadID = Find.UniqueIDsManager.GetNextJobID();
        }

        public new void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref objective, "objective");
        }
    }
}
