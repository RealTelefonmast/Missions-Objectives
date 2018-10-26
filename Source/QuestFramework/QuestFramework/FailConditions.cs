using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StoryFramework
{
    public class FailConditions
    {
        // If any of these are finished, the objective automatically fails
        public List<ObjectiveDef> objectives = new List<ObjectiveDef>();
        public List<MissionDef> missions = new List<MissionDef>();

        public bool Failed
        {
            get
            {

                return false;
            }
        }
    }
}
