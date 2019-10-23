using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StoryFramework
{
    /* Missions contain objectives. The main part of the story framework.
     * Missions usually finish when all objectives are finished
     * Special cases include: Multiple Objective Paths -
     *
     * 
     */

    public class Mission : StoryObject
    {
        public new MissionDef def;
        public List<Objective> Objectives = new List<Objective>();

        public Mission(MissionDef def) : base(def)
        {
            foreach(ObjectiveDef objective in def.objectives)
            {
                Objectives.Add(new Objective(objective));
            }
        }

        public override void Finish()
        {
            base.Finish();
            Objectives.ForEach(o => o.Finish());
            
        }

        public override bool Finished
        {
            get
            {
                return Objectives.All(o => o.CurrentState == StoryState.Finished);
            }
        }
    }
}
