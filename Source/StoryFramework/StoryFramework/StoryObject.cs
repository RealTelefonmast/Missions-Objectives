using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace StoryFramework
{
    public abstract class StoryObject : IExposable, ILoadReferenceable
    {
        public StoryObjectDef def;
        public StoryTimer timer;

        public StoryObject(StoryObjectDef def)
        {
            this.def = def;
            Setup(def);
        }

        public virtual void Setup(StoryObjectDef def)
        {
            if(def.timer != null)
            {
                timer = new StoryTimer(def.timer);
            }
        }

        public virtual void ExposeData()
        {
        }

        public string GetUniqueLoadID()
        {
            return def.defName;
        }

        public virtual void Tick()
        {
            timer.Tick();
        }

        public virtual void Start() { }
        public virtual void Finish()
        {
            timer.Terminate();
        }
        public virtual void Fail() { }

        public virtual bool Active => true;
        public virtual bool Finished => false;
        public virtual bool Failed => false;
        public virtual bool Cancelled => false;

        public StoryState CurrentState
        {
            get
            {
                if (Failed)
                    return StoryState.Failed;
                if (Cancelled)
                    return StoryState.Cancelled;
                if (Finished)
                    return StoryState.Finished;
                return StoryState.Active;
            }
        }
    }
}
