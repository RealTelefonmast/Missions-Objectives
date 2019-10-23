using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StoryFramework
{
    public class Objective : StoryObject
    {
        public new ObjectiveDef def;

        private int workLeft = -1;

        public Objective(ObjectiveDef def) : base(def)
        {

        }

        public override void Finish()
        {
            base.Finish();

        }
    }
}
