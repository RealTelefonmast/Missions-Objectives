using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;

namespace StoryFramework
{
    public class StoryIncidentDef : IncidentDef
    {
        public Requisites requisites = new Requisites();
        public IncidentProperties incident;
        public List<IncidentProperties> incidents = new List<IncidentProperties>();
    }
}
