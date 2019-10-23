using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using RimWorld;
using Verse;

namespace StoryFramework
{
    public class ThemePack
    {
        public Texture2D ActiveIcon;
        public Texture2D FinishedIcon;
        public Texture2D FailedIcon;
        public Texture2D RepeatableIcon;
        public Texture2D BackGround;
        public Texture2D ObjectiveMarker;

        public ThemePack(StoryDef story)
        {
            ActiveIcon = ContentFinder<Texture2D>.Get(story.activeIconPath, false);
            FinishedIcon = ContentFinder<Texture2D>.Get(story.finishedIconPath, false);
            FailedIcon = ContentFinder<Texture2D>.Get(story.failedIconPath, false);
            RepeatableIcon = ContentFinder<Texture2D>.Get(story.repeatableIconPath, false);
            BackGround = ContentFinder<Texture2D>.Get(story.backGroundPath, false);
            ObjectiveMarker = ContentFinder<Texture2D>.Get(story.objectiveMarkerPath, false);
        }
    }
}
