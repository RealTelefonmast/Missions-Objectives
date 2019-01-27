using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.Sound;
using RimWorld;
using RimWorld.Planet;
using Harmony;

namespace StoryFramework
{
    public enum TabMode
    {
        Missions,
        Themes,
        Settings
    }

    public class MainTabWindow_Missions : MainTabWindow
    {
        private StoryManager StoryManager = StoryManager.StoryHandler;
        private Dialog_ObjectiveInformation CurObjectiveInfo;
        private Vector2 descriptionScrollPos = Vector2.zero;
        private Vector2 missionInfoScrollPos = Vector2.zero;
        private Dictionary<ObjectiveDef, List<Pawn>> cachedPawns = new Dictionary<ObjectiveDef, List<Pawn>>();
        private MissionDef selectedLockedMission;
        private TabMode TabMode = TabMode.Missions;
        private bool showLocked = false;
        public const float Height = 630f;
        public const float LeftWidth = 250f;
        public const float YOffset = (720f - Height) / 2f;

        //Images
        private int currentImage = 0;
        private Dictionary<ObjectiveDef, List<Texture2D>> cachedImages = new Dictionary<ObjectiveDef, List<Texture2D>>();
        private Texture2D cachedBG = null;

        public Mission SelectedMission
        {
            get
            {
                return StoryManager.selectedMission;
            }
            set
            {
                StoryManager.selectedMission = value;
            }
        }

        public Objective SelectedObjective
        {
            get
            {
                return StoryManager.selectedObjective;
            }
            set
            {
                StoryManager.selectedObjective = value;
                if (value != null)
                {
                    TrySetDiaShow(value.def);
                }
            }
        }

        public List<Mission> AvailableMissions
        {
            get
            {
                return StoryManager.Missions;
            }
        }

        public List<Mission> MissionsForMod(ModContentPack mcp)
        {
            return AvailableMissions.Where(m => mcp.AllDefs.Contains(m.def) && m.Visible)?.ToList();
        }

        public void UpdateTheme(ModContentPack mcp)
        {
            if (mcp != null)
            {
                StoryManager.Theme = StoryManager.ModFolder.Find(mcpw => mcpw.identifier == mcp.Identifier);
                if (!StoryManager.Theme.SCD.backGroundPath.NullOrEmpty())
                {
                    if (!StoryManager.Theme.SCD.backGroundPath.NullOrEmpty())
                    {
                        cachedBG = ContentFinder<Texture2D>.Get(StoryManager.Theme.SCD.backGroundPath);
                    }
                    else { cachedBG = null; }
                }
            }
            else
            {
                StoryManager.Theme = null;
                cachedBG = null;
            }
        }

        public void SetActiveObjective()
        {           
            if (SelectedObjective == null || !SelectedMission.objectives.Contains(SelectedObjective))
            {
                SelectedObjective = SelectedMission.objectives.FirstOrDefault();
                for (int i = 0; i < SelectedMission.objectives.Count; i++)
                {
                    Objective objective = SelectedMission.objectives[i];
                    if (objective.CurrentState == MOState.Active)
                    {
                        SelectedObjective = objective;
                        return;
                    }
                }
            }
            return;
        }

        private void CachePawns(bool update)
        {
            foreach (Objective objective in StoryManager.GetObjectives)
            {
                if (!cachedPawns.ContainsKey(objective.def))
                {
                    cachedPawns.Add(objective.def, objective.CapablePawns);
                }
                else if (update)
                {
                    cachedPawns[objective.def] = objective.CapablePawns;
                }
            }
        }

        private void TrySetDiaShow(ObjectiveDef objective)
        {
            if (!cachedImages.TryGetValue(objective, out List<Texture2D> textures))
            {
                cachedImages.Clear();
                currentImage = 0;
                List<Texture2D> list = new List<Texture2D>();
                foreach (string s in objective.images)
                {
                    Texture2D image = ContentFinder<Texture2D>.Get(s, false);
                    if (image != null)
                    {
                        list.Add(image);
                    }
                }
                if (!list.NullOrEmpty())
                {
                    cachedImages.Add(objective, list);
                }
            }
        }

        protected override float Margin => 1f;

        public override Vector2 RequestedTabSize => new Vector2(1281f, 721f);

        public override void PreOpen()
        {
            base.PreOpen();
            CachePawns(false);
            UpdateTheme(StoryManager?.Theme?.MCP);
            if (SelectedObjective == null)
            {
                if (SelectedMission != null && !SelectedMission.objectives.NullOrEmpty())
                {
                    SelectedObjective = SelectedMission.objectives.Where(o => o.CurrentState == MOState.Active).FirstOrDefault();
                }
            }
        }

        public override void PostClose()
        {
            base.PostClose();
            if (CurObjectiveInfo != null)
            {
                CurObjectiveInfo.Close(false);
            }
            cachedPawns.Clear();
        }

        public override void DoWindowContents(Rect inRect)
        {
            if (cachedBG != null)
            {
                Widgets.DrawTextureFitted(inRect, cachedBG, 1f);
            }
            Rect MissionMenu = new Rect(0f, YOffset, LeftWidth, Height);
            Rect ObjectiveMenu = new Rect(LeftWidth, YOffset, inRect.width - MissionMenu.width, Height);

            DrawMissionMenu(MissionMenu.ContractedBy(10f));
            if (SelectedMission != null)
            {
                DrawObjectiveMenu(ObjectiveMenu.ContractedBy(10f));
            }
            if(selectedLockedMission != null && !selectedLockedMission.IsSeen)
            {
                DrawLockedMissionInfo(ObjectiveMenu.ContractedBy(10f));
            }
            Text.Font = GameFont.Small;
            Text.Anchor = 0;
        }

        public void DrawMissionMenu(Rect inRect)
        {
            //Draw Tabs
            Text.Font = GameFont.Small;
            Rect tabRect = new Rect(inRect.x + 10f, inRect.y - 20f, inRect.width - 30f, 20f);
            string missions = "Missions_SMO".Translate();
            string themes = "Themes_SMO".Translate();
            string settings = "Settings_SMO".Translate();
            Vector2 v1 = Text.CalcSize(missions);
            Vector2 v2 = Text.CalcSize(themes);
            Vector2 v3 = Text.CalcSize(settings);
            v1.x += 6;
            v2.x += 6;
            v3.x += 6;
            Rect missionTab = new Rect(new Vector2(tabRect.x, tabRect.y), v1);
            Rect themeTab = new Rect(new Vector2(missionTab.xMax, missionTab.y), v2);
            Rect settingsTab = new Rect(new Vector2(themeTab.xMax, themeTab.y), v3);

            Text.Anchor = TextAnchor.MiddleCenter;           
            StoryUtils.DrawMenuSectionColor(missionTab, 1, StoryMats.defaultFill, StoryMats.defaultBorder);
            Widgets.Label(missionTab, missions);
            StoryUtils.DrawMenuSectionColor(themeTab, 1, StoryMats.defaultFill, StoryMats.defaultBorder);
            Widgets.Label(themeTab, themes);
            StoryUtils.DrawMenuSectionColor(settingsTab, 1, StoryMats.defaultFill, StoryMats.defaultBorder);
            Widgets.Label(settingsTab, settings);
            GUI.color = new Color(0.8f, 0.8f, 0.8f);
            if (Widgets.ButtonInvisible(missionTab))
            {
                SoundDefOf.Click.PlayOneShotOnCamera(null);
                TabMode = TabMode.Missions;
            }
            if (Mouse.IsOver(missionTab) || TabMode == TabMode.Missions)
            {                
                Widgets.DrawBox(missionTab, 1);
            }
            if (Widgets.ButtonInvisible(themeTab))
            {
                SoundDefOf.Click.PlayOneShotOnCamera(null);
                TabMode = TabMode.Themes;
            }
            if (Mouse.IsOver(themeTab) || TabMode == TabMode.Themes)
            {
                Widgets.DrawBox(themeTab, 1);
            }
            if (Widgets.ButtonInvisible(settingsTab))
            {
                SoundDefOf.Click.PlayOneShotOnCamera(null);
                TabMode = TabMode.Settings;
            }
            if (Mouse.IsOver(settingsTab) || TabMode == TabMode.Settings)
            {
                Widgets.DrawBox(settingsTab, 1);
            }
            GUI.color = Color.white;

            StoryUtils.DrawMenuSectionColor(inRect, 1, new ColorInt(55, 55, 55), new ColorInt(135, 135, 135));
            Rect rect = inRect.ContractedBy(5f);

            float selectionHeight = 45f;
            float viewHeight = 0f;
            float selectionYPos = 0f;

            if (TabMode == TabMode.Missions)
            {
                //Mission Tab
                GUI.BeginGroup(rect);
                viewHeight = MissionTabHeight(selectionHeight);
                Rect outRect = new Rect(0f, 0f, rect.width, rect.height);
                Rect viewRect = new Rect(0f, 0f, rect.width, viewHeight);
                Widgets.BeginScrollView(outRect, ref StoryManager.missionScrollPos, viewRect, false);
                for (int i = 0; i < StoryManager.ModFolder.Count; i++)
                {
                    ModContentPackWrapper MCPW = StoryManager.ModFolder[i];
                    if (MCPW.SCD != SCD.MainStoryControlDef)
                    {
                        List<Mission> Missions = MissionsForMod(MCPW.MCP);
                        //Identifier
                        string appendix = MCPW.Toggled ? "-" : "+";
                        string identifier = MCPW.SCD.label + " " + appendix;
                        Vector2 identifierSize = Text.CalcSize(identifier);
                        identifierSize.x += 6f;
                        Rect identifierRect = new Rect(new Vector2(0f, selectionYPos), identifierSize);
                        selectionYPos += identifierRect.height;
                        Widgets.DrawMenuSection(identifierRect);
                        Text.Font = GameFont.Tiny;
                        Text.Anchor = TextAnchor.MiddleCenter;
                        Widgets.Label(identifierRect, identifier);
                        Text.Anchor = 0;
                        Text.Font = GameFont.Small;
                        if (Widgets.ButtonInvisible(identifierRect) && MCPW.HasActiveMissions)
                        {                          
                            MCPW.Toggle();
                            if (MCPW.Toggled)
                            {
                                SoundDefOf.TabOpen.PlayOneShotOnCamera(null);
                            }
                            else
                            {
                                SoundDefOf.TabClose.PlayOneShotOnCamera(null);
                            }
                        }
                        if (Mouse.IsOver(identifierRect))
                        {
                            GUI.color = new Color(0.8f, 0.8f, 0.8f);
                            Widgets.DrawBox(identifierRect, 1);
                            GUI.color = Color.white;
                        }
                        UIHighlighter.HighlightOpportunity(identifierRect, MCPW.MCP.Identifier + "-StoryHighlight");

                        if (MCPW.Toggled)
                        {
                            //Group
                            float lockedCount = 0;
                            string lockedLabel = "";
                            Vector2 lockedSize = Vector2.zero;
                            List<Def> cachedLocked = new List<Def>();
                            if (showLocked)
                            {
                                cachedLocked = MCPW.MCP.AllDefs.Where(d => d is MissionDef && !(d as MissionDef).HardLocked && (d as MissionDef).CurrentState == MOState.Inactive).ToList();
                                lockedCount = cachedLocked.Count();
                                lockedLabel = "Locked_SMO".Translate() + ":";
                                lockedSize = Text.CalcSize(lockedLabel);
                            }
                            bool lockedBool = lockedCount > 0;
                            float height = (lockedCount + Missions.Count) * selectionHeight + (lockedBool ? lockedSize.y : 0f);
                            Rect groupRect = new Rect(0f, selectionYPos, rect.width, height);
                            StoryUtils.DrawMenuSectionColor(groupRect, 1, MCPW.SCD.color, MCPW.SCD.borderColor);
                            float missionTabY = groupRect.y;
                            for(int ii = 0; ii < Missions.Count ;ii++)
                            {
                                Mission mission = Missions[ii];
                                Rect rect4 = new Rect(groupRect.x, missionTabY, groupRect.width, selectionHeight);
                                StoryControlDef scd = StoryManager.Theme != null ? StoryManager.Theme.SCD : SCD.MainStoryControlDef;
                                DrawMissionTab(rect4, mission, scd, ii);
                                missionTabY += selectionHeight;
                            }
                            if (showLocked && lockedBool)
                            {
                                float h = height - (Missions.Count * selectionHeight);
                                Rect boxRect = new Rect(groupRect.x, groupRect.y + (height - h), groupRect.width, h);
                                Widgets.DrawBoxSolid(boxRect.ContractedBy(2f), new Color(0.2f, 0.2f, 0.2f, 0.2f));
                                GUI.color = MCPW.SCD.borderColor.ToColor;
                                Widgets.DrawLineHorizontal(boxRect.x,boxRect.y,boxRect.width);
                                GUI.color = Color.white;
                                Widgets.Label(new Rect(new Vector2(groupRect.x + 5f, missionTabY), lockedSize), lockedLabel);
                                missionTabY += lockedSize.y;
                                foreach (MissionDef def in cachedLocked)
                                {
                                    Rect rect4 = new Rect(groupRect.x, missionTabY, groupRect.width, selectionHeight);
                                    DrawLockedMissionTab(rect4, def, MCPW.SCD);
                                    missionTabY += selectionHeight;
                                }
                            }
                            selectionYPos += height;
                        }

                    }
                    selectionYPos += 5f;
                }
                Widgets.EndScrollView();
                GUI.EndGroup();
            }
            else if(TabMode == TabMode.Themes)
            {
                //Theme Tab
                GUI.BeginGroup(rect);
                viewHeight = selectionHeight * StoryManager.ModFolder.Count;
                Rect outRect = new Rect(0f, 0f, rect.width, rect.height);
                Rect viewRect = new Rect(0f, 0f, rect.width, viewHeight);
                Widgets.BeginScrollView(outRect, ref StoryManager.missionScrollPos, viewRect, false);
                for (int i = 0; i < StoryManager.ModFolder.Count + 1; i++)
                {
                    Rect selection = new Rect(0f, selectionYPos, rect.width, selectionHeight).ContractedBy(5f);
                    Text.Anchor = TextAnchor.MiddleCenter;                   
                    if (i == 0)
                    {
                        Widgets.DrawMenuSection(selection);
                        Widgets.Label(selection, "None");
                        if (Mouse.IsOver(selection) || StoryManager.Theme == null)
                        {
                            GUI.color = new Color(0.8f, 0.8f, 0.8f);
                            Widgets.DrawBox(selection, 1);
                            GUI.color = Color.white;
                            if (Widgets.ButtonInvisible(selection))
                            {
                                UpdateTheme(null);
                                SoundDefOf.Click.PlayOneShotOnCamera(null);
                            }
                        }
                    }
                    else
                    {
                        ModContentPackWrapper mcp = StoryManager.ModFolder.ElementAt(i - 1);
                        if (mcp.SCD != SCD.MainStoryControlDef ? mcp.SCD.backGroundPath != SCD.MainStoryControlDef.backGroundPath : true)
                        {
                            Widgets.DrawMenuSection(selection);
                            Widgets.Label(selection, mcp.SCD.LabelCap);
                            if (Mouse.IsOver(selection) || StoryManager.Theme == mcp)
                            {
                                GUI.color = new Color(0.8f, 0.8f, 0.8f);
                                Widgets.DrawBox(selection, 1);
                                GUI.color = Color.white;
                                if (Widgets.ButtonInvisible(selection, true))
                                {
                                    UpdateTheme(mcp.MCP);
                                    SoundDefOf.Click.PlayOneShotOnCamera(null);
                                }
                            }
                        }
                    }
                    Text.Anchor = 0;
                    selectionYPos += selectionHeight;
                }
                Widgets.EndScrollView();
                GUI.EndGroup();
            }
            else
            {
                //Settings Tab
                Text.Anchor = TextAnchor.MiddleLeft;
                Listing_Standard listing = new Listing_Standard();
                listing.Begin(rect);
                listing.Gap(6f);
                listing.CheckboxLabeled("LockedSettings_SMO".Translate(), ref showLocked, "LockedSettingsToolTip_SMO".Translate());
                listing.GapLine(6f); 
                listing.CheckboxLabeled("ShowExtraInfo_SMO".Translate(), ref StoryManager.showExtraInfo, "ShowExtraInfoToolTip_SMO".Translate());
                listing.GapLine(6f);
                string str = "TimerAlertMinHours_SMO".Translate() + ": " + StoryManager.timerAlertMinHours + "h";
                listing.Label(str, -1, null);
                StoryManager.timerAlertMinHours = Mathf.Round(listing.Slider(StoryManager.timerAlertMinHours, 1f, 24f));
                listing.End();
                Text.Anchor = 0;
            }
        }

        private float MissionTabHeight(float selection)
        {
            float height = 0f;
            foreach (ModContentPackWrapper MCPW in StoryManager.ModFolder)
            {
                height += 12f;
                if (MCPW.Toggled)
                {
                    height += MCPW.MCP.AllDefs.Where(d => d is MissionDef).Count() * selection;
                }
            }
            return height;
        }

        private void DrawLockedMissionTab(Rect rect, MissionDef def, StoryControlDef SCD)
        {
            rect = rect.ContractedBy(1f);
            if (Mouse.IsOver(rect) || selectedLockedMission == def)
            {
                Widgets.DrawHighlight(rect);
            }
            float offset = (rect.height - 24f) * 0.5f;
            WidgetRow tab = new WidgetRow(rect.x + offset, rect.y + offset);
            tab.Label(def.LabelCap);
            if (Widgets.ButtonInvisible(rect))
            {
                SoundDefOf.Click.PlayOneShotOnCamera(null);
                selectedLockedMission = def;
                SelectedMission = null;
            }
            bool mouseOver = Mouse.IsOver(rect);
            /*
            if (mouseOver || selectedLockedMission == def)
            {
                Color color = SCD.borderColor.ToColor;
                Color half = color;
                half.a = 0.5f;
                GUI.color = mouseOver ? half : color;
                Widgets.DrawBox(rect, 1);
                GUI.color = Color.white;
            }
            */
        }

        private void DrawMissionTab(Rect rect, Mission mission, StoryControlDef SCD, int num)
        {
            mission.Notify_Seen();
            rect = rect.ContractedBy(1f);
            if (Mouse.IsOver(rect) || SelectedMission == mission)
            {
                Widgets.DrawHighlight(rect);
            }
            
            float offset = (rect.height - 24f) * 0.5f;
            WidgetRow tab = new WidgetRow(rect.x + offset, rect.y + offset);
            MOState state = mission.LatestState;
            if (mission.def.repeatable)
            {
                tab.Icon(ContentFinder<Texture2D>.Get(SCD.repeatableIconPath, false));
            }
            else if(state == MOState.Active)
            {
                tab.Icon(ContentFinder<Texture2D>.Get(SCD.activeIconPath, false));
            }
            else if(state == MOState.Finished)
            {
                tab.Icon(ContentFinder<Texture2D>.Get(SCD.finishedIconPath, false));
            }
            else if(state == MOState.Failed)
            {
                tab.Icon(ContentFinder<Texture2D>.Get(SCD.failedIconPath, false));
            }
            tab.Gap(5f);           
            string label = mission.def.LabelCap;
            Vector2 labelSize = Text.CalcSize(label);
            float width = rect.width - tab.FinalX;
            float labelHeight = Text.CalcHeight(label, width);
            offset = (rect.height - labelHeight) * 0.5f;
            Rect labelRect = new Rect(tab.FinalX, rect.y + offset, width, rect.height);
            Widgets.Label(labelRect, label);

            if (Widgets.ButtonInvisible(rect, true))
            {
                SoundDefOf.Click.PlayOneShotOnCamera(null);
                selectedLockedMission = null;
                this.SelectedMission = mission;
                SetActiveObjective();
            }
            /*
            bool mouseOver = Mouse.IsOver(rect);
            if (mouseOver || this.SelectedMission == mission)
            {
                Color color = SCD.borderColor.ToColor;
                Color half = color;
                half.a = 0.5f;
                GUI.color = mouseOver ? half : color;
                Widgets.DrawBox(rect, 1);
                GUI.color = Color.white;
            }
            */
        }

        public void DrawLockedMissionInfo(Rect rect)
        {
            Rect InfoRect = new Rect(rect.x, rect.y, rect.width / 3f, rect.height);
            StoryUtils.DrawMenuSectionColor(InfoRect, 1, new ColorInt(55, 55, 55), new ColorInt(135, 135, 135));
            float TotalHeight = InfoHeightFor(InfoRect.width, selectedLockedMission);
            Rect ViewRect = new Rect(InfoRect.x, InfoRect.y, InfoRect.width, TotalHeight);
            Widgets.BeginScrollView(InfoRect, ref missionInfoScrollPos, ViewRect, false);
            InfoRect = InfoRect.ContractedBy(5f);
            GUI.BeginGroup(InfoRect);
            //MainPart
            float CurY = 0f;
            Requisites requisites = selectedLockedMission.requisites;
            string MainInfo = requisites.anyList ? "MissionInfoMainAny_SMO".Translate("'" + selectedLockedMission.label + "'") : "MissionInfoMain_SMO".Translate("'" + selectedLockedMission.label + "'");
            float MainInfoHeight = Text.CalcHeight(MainInfo, InfoRect.width);
            Rect MainInfoRect = new Rect(0, 0, InfoRect.width, MainInfoHeight);
            Widgets.Label(MainInfoRect, MainInfo);
            CurY += MainInfoRect.height;

            GeneralInfo(new Rect(0f, CurY, InfoRect.width, 0f), ref CurY, requisites.researchProjects, requisites.anyResearch ? "MissionInfoResearchAny_SMO".Translate() : "MissionInfoResearch_SMO".Translate(), delegate (ResearchProjectDef def)
            {
                Find.MainTabsRoot.SetCurrentTab(MainButtonDefOf.Research, true);
                MainTabWindow_Research window = (Find.WindowStack.Windows.Where(w => w is MainTabWindow_Research).FirstOrDefault() as MainTabWindow_Research);
                Traverse.Create(window).Field("selectedProject").SetValue(def);
            });
            GeneralInfo(new Rect(0f, CurY, InfoRect.width, 0f), ref CurY, requisites.missions, requisites.anyMission ? "MissionInfoMissionsAny_SMO".Translate() : "MissionInfoMissions_SMO".Translate(), delegate (MissionDef def)
            {
                SelectedMission = StoryManager.GetMission(def);
                selectedLockedMission = null;
            });
            GeneralInfo(new Rect(0f, CurY, InfoRect.width, 0f), ref CurY, requisites.objectives, requisites.anyObjective ? "MissionInfoObjectivesAny_SMO".Translate() : "MissionInfoObjectives_SMO".Translate(), delegate (ObjectiveDef def)
            {
                SelectedMission = StoryManager.GetMission(def);
                SelectedObjective = SelectedMission.objectives.Find(o => o.def == def);
                selectedLockedMission = null;
            });
            GeneralInfo(new Rect(0f, CurY, InfoRect.width, 0f), ref CurY, requisites.things, requisites.anyThing ? "MissionInfoThingsAny_SMO".Translate() : "MissionInfoThings_SMO".Translate(), null, delegate (Rect selection, ThingValue tv)
            {
                ThingDef thingDef = tv.ThingDef;
                string stuffLabel = tv.Stuff != null ? tv.ResolvedStuff.LabelCap : "Any_SMO".Translate();
                string qualityLabel = !thingDef.CountAsResource ? tv.CustomQuality ? " (" + tv.QualityCategory.GetLabel() + ")" : "" : "";
                string fullLabel = "  - " + (tv.ResolvedStuff != null ? stuffLabel + " " : "") + thingDef.LabelCap + qualityLabel + " (x" + tv.value + ")";
                if(Text.CalcSize(fullLabel).x > selection.width)
                {
                    Widgets.DrawHighlightIfMouseover(selection);
                    TooltipHandler.TipRegion(selection, fullLabel);
                    fullLabel = fullLabel.Truncate(selection.width);
                }
                Widgets.Label(selection, fullLabel);
            });
            GeneralInfo(new Rect(0f, CurY, InfoRect.width, 0f), ref CurY, requisites.jobs, "MissionInfoJobs_SMO".Translate(),null, delegate(Rect selection, JobDef def)
            {
                string label = ("  ..." + def.reportString);
                if(Text.CalcSize(label).x > selection.width)
                {
                    Widgets.DrawHighlightIfMouseover(selection);
                    TooltipHandler.TipRegion(selection, label);
                    label = label.Truncate(selection.width);
                }
                Widgets.Label(selection, label);
            });
            GUI.EndGroup();
            Widgets.EndScrollView();
        }

        private void GeneralInfo<T>(Rect rect, ref float yPos, List<T> list,string mainLabel , Action<T> action = null, Action<Rect,T> action2 = null)
        {
            if(list.Count == 0 || selectedLockedMission == null)
            {
                return;
            }
            AddGap(rect, ref yPos);
            List<T> column1 = new List<T>(),
                    column2 = new List<T>();
            for (int i = 0; i < list.Count; i++)
            {
                if (i % 2 == 0)
                {
                    column1.Add(list[i]);
                }
                else
                {
                    column2.Add(list[i]);
                }
            }
            string main = "".Translate();
            float height = Text.CalcHeight(main, rect.width),
                  height2 = Math.Max(column1.Count, column2.Count) * 22f;
            rect = new Rect(0f, yPos, rect.width, height + height2);
            Rect Listing = new Rect(0f, yPos + height, rect.width, height2);
            Widgets.DrawBoxSolid(rect, new ColorInt(33, 33, 33).ToColor);
            Vector2 mainSize = Text.CalcSize(mainLabel);
            Widgets.Label(rect, " " + mainLabel);
            Texture2D checkBox = selectedLockedMission.requisites.StatusForType(list.FirstOrDefault()) ? Widgets.CheckboxOnTex : Widgets.CheckboxOffTex;
            Widgets.DrawTextureFitted(new Rect(rect.xMax - 20f, rect.y + 2f, 18f, 18f), checkBox, 1f);
            float begin = Listing.y;
            float leftPos = begin;
            float rightPos = begin;
            GUI.color = new Color(0.20f, 0.20f, 0.20f);
            Widgets.DrawLineVertical(Listing.RightHalf().x, Listing.y + 3f, height2 - 6f);
            GUI.color = Color.white;
            for (int i = 0; i < list.Count; i++)
            {
                bool left = i % 2 == 0;
                Rect half = left ? Listing.LeftHalf() : Listing.RightHalf();
                float pos = left ? leftPos : rightPos;
                string label = ("  - " + (list[i] as Def)?.LabelCap ?? ""),
                       labelTrun = label.Truncate(half.width);
                Rect selection = new Rect(half.x, pos, half.width, 22f);
                if ((!(list[i] as ResearchProjectDef)?.IsFinished ?? false) || ((list[i] as MissionDef)?.CurrentState == MOState.Active) || (list[i] as ObjectiveDef)?.CurrentState == MOState.Active || (list[i] as ThingValue)?.ThingDef != null)
                {
                    if (Text.CalcSize(label).x > selection.width)
                    {
                        Widgets.DrawHighlightIfMouseover(selection);
                        TooltipHandler.TipRegion(selection, label);
                    }
                    if (action2 != null)
                    {
                        action2(selection, list[i]);
                    }
                    else
                    if (Widgets.ButtonText(selection, labelTrun, false, true))
                    {
                        SoundDefOf.Click.PlayOneShotOnCamera(null);
                        action(list[i]);
                    }
                }
                else
                {
                    if (action2 != null)
                    {
                        action2(selection, list[i]);
                    }
                    else
                    {                     
                        if (Text.CalcSize(label).x > selection.width)
                        {
                            Widgets.DrawHighlightIfMouseover(selection);
                            TooltipHandler.TipRegion(selection, label);
                        }
                        Widgets.Label(selection, labelTrun);
                    }

                }
                if (left) { leftPos += 22f; } else { rightPos += 22f; }
            }
            yPos += rect.height;
        }

        private void AddGap(Rect rect, ref float YPos)
        {
            /*
            GUI.color = new Color(0.35f, 0.35f, 0.35f);
            Widgets.DrawLineHorizontal(0f, YPos + (12f * 0.5f), rect.width);
            GUI.color = Color.white;
            */
            YPos += 12f;
        }

        private float InfoHeightFor(float width, MissionDef def)
        {
            Requisites req = def.requisites;
            float total = 0f;
            total += Text.CalcHeight("MissionInfoMain_SMO".Translate("'" + selectedLockedMission.label + "'"), width);
            total += 12f;
            total += ReqHeight(req.researchProjects);
            total += ReqHeight(req.missions);
            total += ReqHeight(req.objectives);
            total += ReqHeight(req.jobs);
            total += ReqHeight(req.things.AllThingDefs());
            return total;
        }

        private float ReqHeight<T>(List<T> defs)
        {
            float total = 0f;
            if (defs.Count > 0)
            {
                total += (float)Math.Round(((float)(defs.Count + 1) * 0.5f), 0, MidpointRounding.AwayFromZero) * Text.CalcSize((defs.FirstOrDefault() as Def)?.LabelCap).y;
                total += 34f;
            }
            return total;
        }

        public void DrawObjectiveMenu(Rect inRect)
        {
            StoryUtils.DrawMenuSectionColor(inRect, 1, new ColorInt(55, 55, 55), new ColorInt(135, 135, 135));
            //Widgets.DrawMenuSection(inRect);
            if (SelectedObjective == null)
            {
                SelectedObjective = SelectedMission.objectives.FirstOrDefault();
            }
            GUI.BeginGroup(inRect);
            float third = inRect.width / 3f;

            //Rects
            bool missionTimer = SelectedMission.HasTimer;
            string timerText = "MissionTimer_SMO".Translate() + ": ";
            float timerHeight = Text.CalcSize(timerText).y + 20f;
            float descriptionHeight = missionTimer ? inRect.height - timerHeight : inRect.height;
            Rect DescriptionRect = new Rect(0f, 0f, third, descriptionHeight);
            Rect MissionExtrasRect = new Rect(DescriptionRect.x, DescriptionRect.yMax, third, timerHeight);
            Rect InnerRect = new Rect(DescriptionRect.xMax, 0f, third * 2f, inRect.height);
            Rect ImageRect = InnerRect.TopHalf().ContractedBy(5f);
            Rect ObjectiveRect = InnerRect.BottomHalf();

            //Description
            Widgets.DrawMenuSection(DescriptionRect.ContractedBy(5f));
            StringBuilder description = new StringBuilder();
            description.AppendLine(SelectedMission.def.description?.ToString() + "\n");
            description.AppendLine(SelectedObjective?.def.description?.ToString());
            Text.Anchor = TextAnchor.UpperLeft;
            Widgets.LabelScrollable(DescriptionRect.ContractedBy(10f), description.ToString(), ref descriptionScrollPos);
            //Widgets.Label(DescriptionRect.ContractedBy(10f), description.ToString());
            Text.Anchor = 0;

            Rect timerRect = MissionExtrasRect.ContractedBy(5f);

            if (missionTimer)
            {
                Widgets.DrawMenuSection(timerRect);
                Rect timer = MissionExtrasRect.RightHalf();
                Widgets.Label(timer.ContractedBy(10f), timerText);
                Rect BarRect = new Rect(timerRect.xMax - 96f, timerRect.ContractedBy(6f).y, 90f, timerRect.ContractedBy(6f).height);
                float pct = (float)SelectedMission.GetTimer / (float)SelectedMission.def.timer.GetTotalTime;
                DrawProgressBar(BarRect, StoryUtils.GetTimerText(SelectedMission.GetTimer, SelectedMission.LatestState), pct, StoryMats.grey);
            }


            //
            if (SelectedObjective == null) { return; }

            //Image Slide
            Vector2 buttonSize = new Vector2(45f, ImageRect.height);
            Rect ImageButtonLeft = new Rect(new Vector2(ImageRect.x, ImageRect.y), buttonSize).ContractedBy(5f);
            Rect ImageButtonRight = new Rect(new Vector2(ImageRect.xMax - 45f, ImageRect.y), buttonSize).ContractedBy(5f);
            Widgets.DrawShadowAround(ImageRect);
            Widgets.DrawBoxSolid(ImageRect, new Color(0.14f, 0.14f, 0.14f));

            if (SelectedObjective != null)
            {
                if (cachedImages.TryGetValue(SelectedObjective.def, out List<Texture2D> list) && list[currentImage] != null)
                {
                    Widgets.DrawTextureFitted(ImageRect, list[currentImage], 1f);
                }
            }
            GUI.color = new Color(1f, 1f, 1f, 0.5f);
            if (Mouse.IsOver(ImageRect) && cachedImages.TryGetValue(SelectedObjective?.def, out List<Texture2D> texts))
            {
                string imageCount = (currentImage + 1) + "/" + texts.Count;
                Vector2 size = Text.CalcSize(imageCount);
                Rect imageCountRect = new Rect(new Vector2(0f, 0f), size);
                imageCountRect.center = new Vector2(ImageRect.center.x, ImageRect.yMax - (size.y * 0.5f));
                Widgets.Label(imageCountRect, imageCount);
                if (currentImage > 0)
                {
                    UI.RotateAroundPivot(180f, ImageButtonLeft.center);
                    Widgets.DrawTextureFitted(ImageButtonLeft, StoryMats.arrow, 1f);
                    UI.RotateAroundPivot(180f, ImageButtonLeft.center);
                    if (Widgets.ButtonInvisible(ImageButtonLeft, true))
                    {
                        SoundDefOf.TabClose.PlayOneShotOnCamera(null);
                        currentImage -= 1;
                    }
                }
                if (currentImage < texts.Count - 1)
                {
                    Widgets.DrawTextureFitted(ImageButtonRight, StoryMats.arrow, 1f);
                    if (Widgets.ButtonInvisible(ImageButtonRight, true))
                    {
                        SoundDefOf.TabOpen.PlayOneShotOnCamera(null);
                        currentImage += 1;
                    }
                }
            }
            GUI.color = Color.white;

            //Objectives
            Widgets.DrawMenuSection(ObjectiveRect.ContractedBy(5f));
            Rect ObjectiveContracted = ObjectiveRect.ContractedBy(6f);
            GUI.BeginGroup(ObjectiveContracted);
            float tabHeight = 60f;
            float viewHeight = SelectedMission.objectives.Where(o => o.Active).Count() * tabHeight;
            Rect outRect = new Rect(0f, 0f, ObjectiveContracted.width, ObjectiveContracted.height);
            Rect viewRect = new Rect(0f, 0f, ObjectiveContracted.width, viewHeight);
            Widgets.BeginScrollView(outRect, ref StoryManager.objectiveScrollPos, viewRect, false);
            float yPos = 0f;
            for (int i = 0; i < SelectedMission.objectives.Count; i++)
            {
                Objective objective = SelectedMission.objectives[i];
                if (objective.Active || DebugSettings.godMode)
                {
                    Rect Objective = new Rect(0f, yPos, ObjectiveContracted.width, tabHeight);
                    DrawObjectiveTab(Objective, objective, i);
                    yPos += tabHeight;
                }
            }
            Widgets.EndScrollView();
            GUI.EndGroup();
            GUI.EndGroup();
        }

        private void DrawObjectiveTab(Rect TabRect, Objective objective, int num)
        {
            if (num % 2 == 0)
            {
                Widgets.DrawBoxSolid(TabRect, new ColorInt(50, 50, 50).ToColor);
            }
            //Setup
            ObjectiveDef Def = objective.def;
            TabRect = TabRect.ContractedBy(5f);
            GUI.BeginGroup(TabRect);
            //Label
            string Label = Def.LabelCap;
            Vector2 LabelSize = Text.CalcSize(Label);
            Rect LabelRect = new Rect(new Vector2(0f, 0f), LabelSize);
            Widgets.Label(LabelRect, Label);

            //Target
            string TargetLabel = ResolveTargetLabel(Def);
            Vector2 TargetSize = Text.CalcSize(TargetLabel);
            Rect TargetRect = new Rect(new Vector2(0f, TabRect.height - TargetSize.y), TargetSize);
            Rect InfoCardRect = new Rect(TargetLabel.NullOrEmpty() ? -5f : TargetRect.xMax, TargetRect.y, TargetRect.height, TargetRect.height);
            if (!TargetLabel.NullOrEmpty())
            {
                bool MouseOver = Mouse.IsOver(TargetRect);
                GUI.color = MouseOver ? new Color(0.8f, 0.8f, 0.8f) : new Color(0.6f, 0.6f, 0.6f);
                Widgets.Label(TargetRect, TargetLabel);
                GUI.color = Color.white;
            }
            Rect InfoCardAreaRect = new Rect(TargetRect.x, TargetRect.y, TargetRect.width + InfoCardRect.width, TargetRect.height);
            TooltipHandler.TipRegion(InfoCardAreaRect, "InfoCard_SMO".Translate());
            if (Widgets.ButtonInvisible(TargetRect) || Widgets.ButtonImage(InfoCardRect, StoryMats.info2, GUI.color))
            {
                Find.WindowStack.Add(CurObjectiveInfo = new Dialog_ObjectiveInformation(objective));
            }
            UIHighlighter.HighlightOpportunity(InfoCardRect, "InfoCard");

            //ProgressBar
            Vector2 size = new Vector2(90f, 20f);
            Rect BarRect = new Rect();
            Rect BotBarRect = new Rect();
            ResolveBarInputs(objective, out float pct, out string label, out Texture2D material);
            if (material != null && objective.def.type != ObjectiveType.Wait)
            {
                BarRect = new Rect(new Vector2(TabRect.xMax - (size.x + 5f), 0f), size);
                DrawProgressBar(BarRect, label, pct, material);
                if (objective.thingTracker?.ResolveButtonInput(BarRect) ?? false)
                {
                    TooltipHandler.TipRegion(BarRect, "BarInput_SMO".Translate());
                }
            }
            if (objective.def.timer.GetTotalTime > 0)
            {
                BotBarRect = new Rect(new Vector2(TabRect.xMax - (size.x + 5f), TabRect.height - (size.y + 5f)), size);
                float timer = objective.GetTimer;
                pct = timer / objective.def.timer.GetTotalTime;
                label = StoryUtils.GetTimerText(objective.GetTimer, objective.CurrentState);
                if (objective.CurrentState == MOState.Finished)
                { pct = 0f; }
                DrawProgressBar(BotBarRect, label, pct, StoryMats.grey);
            }

            //SkillRequirements
            Rect SkillRequirementRect = new Rect();
            if(objective.def.skillRequirements.Count > 0)
            {
                bool check = BarRect.width + BotBarRect.width > 0f;
                SkillRequirementRect = new Rect(TabRect.xMax - (10f + (check ? 180f : 90f)), 0f, 90f, TabRect.height);
                bool MouseOver = Mouse.IsOver(SkillRequirementRect);
                GUI.color = MouseOver ? new Color(0.8f, 0.8f, 0.8f) : new Color(0.6f, 0.6f, 0.6f);
                Text.Anchor = TextAnchor.UpperCenter;
                Text.Font = GameFont.Medium;
                int count = 0;
                if (cachedPawns.TryGetValue(objective.def, out List<Pawn> pawns))
                {
                    if (!pawns.NullOrEmpty())
                    {
                        count = pawns.Count;
                    }
                }
                Widgets.Label(SkillRequirementRect, count.ToString());
                Text.Font = GameFont.Tiny;
                Text.Anchor = TextAnchor.LowerCenter;
                Widgets.Label(SkillRequirementRect, count != 1 ? "SkillReqPlural_SMO".Translate() : "SkillReq_SMO".Translate());
                Text.Font = GameFont.Small;
                Text.Anchor = 0;
                GUI.color = Color.white;

                StringBuilder sb = new StringBuilder();
                if (!pawns.NullOrEmpty())
                {
                    foreach (Pawn pawn in pawns)
                    {
                        sb.AppendLine("       - " + pawn.LabelCap);
                    }
                }
                StringBuilder sb2 = new StringBuilder();
                foreach (SkillRequirement sr in objective.def.skillRequirements)
                {
                    sb2.AppendLine("       - " + sr.Summary);
                }
                TooltipHandler.TipRegion(SkillRequirementRect, pawns.NullOrEmpty() ? "PawnListEmpty_SMO".Translate(sb2) : "PawnList_SMO".Translate(sb));
            }            
            GUI.EndGroup();
            SkillRequirementRect.x += 5f;
            SkillRequirementRect.y += 5f + (TabRect.height * num);
            bool mouseOnSkill = Mouse.IsOver(SkillRequirementRect);
            if (mouseOnSkill)
            {
                if (cachedPawns.TryGetValue(objective.def, out List<Pawn> pawns))
                {
                    foreach (Pawn pawn in pawns)
                    {
                        if (pawn != null)
                        {
                            TargetHighlighter.Highlight(pawn, false, true, false);
                        }
                    }
                }
            }
            if (Widgets.ButtonInvisible(TabRect, true))
            {
                if (mouseOnSkill)
                {
                    Find.Selector.SelectedObjects.Clear();
                    if (cachedPawns.TryGetValue(objective.def, out List<Pawn> pawns))
                    {
                        if (!pawns.NullOrEmpty())
                        {
                            CameraJumper.TryJumpAndSelect(pawns.RandomElement());
                            this.Close();
                        }
                    }
                }
                SoundDefOf.Click.PlayOneShotOnCamera(null);
                SelectedObjective = objective;
            }
            TabRect = TabRect.ExpandedBy(5f);
            if (objective.CurrentState == MOState.Failed)
            {
                GUI.color = Color.red;
                Widgets.DrawHighlight(TabRect);
                GUI.color = Color.white;
            }
            bool mouseOver = Mouse.IsOver(TabRect);
            if (mouseOver || this.SelectedObjective == objective)
            {
                GUI.color = mouseOver ? new Color(0.3f, 0.3f, 0.3f) : new Color(0.5f, 0.5f, 0.5f);
                Widgets.DrawBox(TabRect, 1);
                GUI.color = Color.white;
            }
        }

        private void ResolveBarInputs(Objective objective, out float pct, out string label, out Texture2D material)
        {
            ObjectiveDef def = objective.def;
            StringBuilder stringBuilder = new StringBuilder();
            pct = 0f;
            label = "";
            material = null;
            float l = 0,
                  r = 0;
            bool any = def.targetSettings?.any ?? false;
            if (def.type == ObjectiveType.Travel)
            {
                TravelSettings settings = def.travelSettings;
                if (settings.mode == TravelMode.Explore || settings.mode == TravelMode.Reach)
                {
                    return;
                }
                pct = (float)(l = objective.travelTracker.CurrentCount) /(float)(r = settings.factionSettings.ValueForMode(settings.mode));
                label = l + "/" + r;
            }
            if (def.targetSettings != null)
            {
                foreach (ThingValue thingValue in def.targetSettings.targets)
                {
                    stringBuilder.AppendLine("       " + (thingValue.IsPawnKindDef ? thingValue.PawnKindDef.LabelCap : thingValue.ThingDef.LabelCap));
                }
                if (def.type == ObjectiveType.Research)
                {
                    pct = objective.GetWorkPct;
                    label = Mathf.RoundToInt(objective.GetWorkDone) + "/" + def.workAmount;
                }
                else
                if (def.type == ObjectiveType.ConstructOrCraft || def.type == ObjectiveType.Own || def.type == ObjectiveType.MapCheck || def.type == ObjectiveType.Recruit || def.type == ObjectiveType.Destroy || def.type == ObjectiveType.Kill)
                {
                    //ThingValue maxValue = objective.thingTracker.TargetsDone.ToList().Find(tv => tv.Value == objective.thingTracker.TargetsDone.Values.Max()).Key;
                    int maxInt = ResolveBarAnyMax(objective, out int maxValue);
                    pct = any ? (maxInt > 0 ? (l = (float)maxValue) / (r = (float)maxInt) : 0f) :
                                (l = (float)objective.thingTracker.GetTotalCount) / (r = (float)objective.thingTracker.GetTotalNeededCount);
                    label = l + "/" + r;
                }
            }
            if (def.customSettings.progressBarColor != null)
            {
                material = SolidColorMaterials.NewSolidColorTexture(def.customSettings.progressBarColor.ToColor);
                return;
            }
            switch (def.type)
            {
                case ObjectiveType.Custom:
                    material = StoryMats.grey;              
                    break;               
                case ObjectiveType.Research:
                    material = StoryMats.blue;
                    break;
                case ObjectiveType.ConstructOrCraft:
                    material = StoryMats.orange;
                    break;
                case ObjectiveType.Destroy:
                case ObjectiveType.Kill:
                    material = StoryMats.red;
                    break;
                case ObjectiveType.MapCheck:
                case ObjectiveType.Recruit:
                case ObjectiveType.Own:
                    material = StoryMats.green;
                    break;
                case ObjectiveType.Travel:
                    material = StoryMats.purple;
                    break;
            }
        }

        public void DrawProgressBar(Rect rect, string label, float pct, Texture2D material)
        {
            Widgets.FillableBar(rect, pct, material, StoryMats.black, true);
            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(rect, label);
            Text.Anchor = 0;
        }

        private int ResolveBarAnyMax(Objective objective, out int max)
        {
            TargetSettings settings = objective.def.targetSettings;
            ThingTracker tracker = objective.thingTracker;
            int num = 0;
            max = 0;
            ThingValue tv = tracker.TargetsDone.Where(k => k.Value == tracker.TargetsDone.Max(k2 => k2.Value)).FirstOrDefault().Key;

            int pawnMin = settings.pawnSettings?.minAmount ?? 0;
            int thingMin = settings.thingSettings?.minAmount ?? 0;
            int targetsMin = settings.targets.Find(t => t == tv).value;
            float pawns = 0f;
            if (pawnMin > 0)
            {
                pawns = (float)tracker.GetPawnCount / (float)pawnMin;
            }
            float things = 0f;
            if (thingMin > 0)
            {
               things = (float)tracker.GetThingCount / (float)thingMin;
            }
            float targets = 0f;
            if (targetsMin > 0)
            {
                targets = (float)tracker.GetTargetCount / (float)targetsMin;
            }
            float maxpct = Mathf.Max(pawns, things, targets);
            if (maxpct == pawns && pawnMin > 0)
            {
                num = pawnMin;
                max = tracker.GetPawnCount;
            }
            if(maxpct == things && thingMin > 0)
            {
                num = thingMin;
                max = tracker.GetThingCount;
            }
            if (maxpct == targets && targetsMin > 0)
            {
                num = targetsMin;
                max = tracker.TargetsDone[tv];
            }
            return num;
        }

        private string ResolveTargetLabel(ObjectiveDef def)
        {
            string label = def.customSettings?.shortLabel;
            List<ThingValue> targets = def.targetSettings?.targets;
            bool any = def.targetSettings?.any ?? false || def.type == ObjectiveType.Research;
            bool multi = targets?.Count > 1 && !any && def.type != ObjectiveType.Research;
            bool travel = def.type == ObjectiveType.Travel;
            int count = targets.Count;
            string pre = ("Req" + def.type.ToString() + (travel ? def.travelSettings.mode.ToString() : "") + (multi ? "Plural" : "") + "_SMO");
            if (pre.CanTranslate())
            {
                if (label.NullOrEmpty())
                {label += pre.Translate(); }
                if (!targets.NullOrEmpty())
                {
                    switch (def.type)
                    {
                        case ObjectiveType.Research:
                            label += ": " + def.BestPotentialStation.LabelCap + (count > 1 ? " [+" + (count - 1) + "]" : "");
                            break;
                        case ObjectiveType.ConstructOrCraft:
                            break;
                        case ObjectiveType.Own:
                            break;
                        case ObjectiveType.Recruit:
                            break;
                        case ObjectiveType.Destroy:
                            break;
                        case ObjectiveType.Kill:
                            break;
                    }
                }
            }
            return label;
        }
    }
}
