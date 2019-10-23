using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace StoryFramework
{
    public class Objective_Research : Objective
    {
        public Thing ResearchStation;

        public Objective_Research(ObjectiveDef def) : base(def)
        {
        }

        private void UpdateResearchStation()
        {

        }
    }
}
