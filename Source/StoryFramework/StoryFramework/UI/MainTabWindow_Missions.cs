using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;

namespace StoryFramework
{
    public class MainTabWindow_Missions : MainTabWindow
    {
        //Settings
        private WorldComponent_Story Story = WorldComponent_Story.Story;

        //Cache TODO: Cache your shit
        private float cachedMainMenuHeight = -1;

        private Dictionary<StoryDef, List<Mission>> MissionPerStory;

        //Images
        private int curImage = 0;
        private Dictionary<ObjectiveDef, List<Texture2D>> cachedImages = new Dictionary<ObjectiveDef, List<Texture2D>>();
        private Texture2D cachedBG;

        private const float Height = 630;
        private const float MissionMenuWidth = 250;
        private const float BorderSize = 10;
        private const float HorizontalMargin = 0;
        private const float VerticalMargin = 40;
        private const float TabHeight = 20f;

        protected override float Margin => 1f;
        private float TotalMargin => Margin + BorderSize;
        public override Vector2 RequestedTabSize => new Vector2(1280, 720f) + new Vector2(TotalMargin * 2 , TotalMargin * 2);

        public override Vector2 InitialSize { get; }

        public Mission SelMission
        {
            get => WorldComponent_Story.SelectedMission;
            set => WorldComponent_Story.SelectedMission = value;
        }

        public Objective SelObjective
        {
            get => WorldComponent_Story.SelectedObjective;
            set
            {
                WorldComponent_Story.SelectedObjective = value;
                if (value != null)
                    SetDiaShowFor(value.def);
            }
        }

        private void SetDiaShowFor(ObjectiveDef def)
        {
            if (cachedImages.TryGetValue(def, out List<Texture2D> images)) return;
            var textures = new List<Texture2D>();
            foreach(string path in def.images)
            {
                var texture = ContentFinder<Texture2D>.Get(path, false);
                if(texture != null)
                    textures.Add(texture);
            }
            if(!textures.NullOrEmpty())
                cachedImages.Add(def, textures);
        }

        public override void DoWindowContents(Rect inRect)
        {
            Rect MissionMenuRect = new Rect();
            Rect ObjectiveViewRect = new Rect();

            DrawMissionMenu(MissionMenuRect);
            DrawObjectiveView(ObjectiveViewRect);
        }

        public void DrawMissionMenu(Rect rect)
        {
            Widgets.DrawMenuSection(rect);
            Rect tabRect = new Rect(rect.x + 10, rect.y - TabHeight, rect.width - 20f, TabHeight);

            Rect menuContent = rect.ContractedBy(5f);
            float selectionHeight = 45f;
            float selectionYPos = 0f;
            GUI.BeginGroup(rect);
            if(cachedMainMenuHeight < 0)
                cachedMainMenuHeight = Story.Stories.Where(s => s.Toggled).SelectMany(s => s.MCP.AllDefs.Where(d => d is MissionDef)).Count() * selectionHeight;
            Rect outRect = new Rect(0f, 0f, rect.width, rect.height);
            Rect viewRect = new Rect(0f, 0f, rect.width, cachedMainMenuHeight);
            Widgets.BeginScrollView(outRect, ref Story.missionScrollPos, viewRect, false);
            for (int i = 0; i < Story.Stories.Count; i++)
            {
                StoryContentPack story = Story.Stories[i];
                if (story.story != StoryDef.Named("MainStoryDef"))
                {
                    List<Mission> missions = Story.ActiveMissions.Where(m => story.MCP.AllDefs.Contains(m.def) && m.Visible)?.ToList()
                }
            }
            Widgets.EndScrollView();
            GUI.EndGroup();

        }

        public void DrawObjectiveView(Rect rect)
        {

        }
    }
}
