using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.Sound;
using RimWorld;

namespace MissionsAndObjectives
{
    [StaticConstructorOnStartup]
    public class MainTabWindow_MissionObjectives : MainTabWindow
    {
        public WorldComponent_Missions MissionHandler;

        private Mission selectedMission;

        private Objective selectedObjective;

        private bool tabFlag = true;

        //Visual components

        private int selectedImage = 0;

        private List<Texture2D> cachedImages = new List<Texture2D>();

        private Texture2D backgroundImage;


        public override Vector2 InitialSize
        {
            get
            {
                return new Vector2(1316f, 756f);
            }
        }

        public override void PreOpen()
        {
            base.PreOpen();
            if(MissionHandler == null)
            {
                MissionHandler = WorldComponent_Missions.MissionHandler;
            }
            foreach (ModContentPack mcp in LoadedModManager.RunningMods.Where(mcp => mcp.AllDefs.Any(def => def is MissionControlDef)))
            {
                if (mcp != null)
                {
                    if (!MissionHandler.ModFolder.Any(mcpw => mcpw.packName == mcp.Identifier))
                    {
                        ModContentPackWrapper mcpw = new ModContentPackWrapper(mcp.Identifier);
                        MissionHandler.ModFolder.Add(mcpw);
                    }
                }
            }
            if (MissionHandler.theme == null && !MissionHandler.openedOnce)
            {
                MissionHandler.theme = MissionHandler.ModFolder.Find(mcpw => mcpw.MCP.AllDefs.Contains(MCD.MainMissionControlDef));
            }
            if (selectedMission == null)
            {
                selectedMission = AvailableMissions.Where(m => !m.def.IsFinished).FirstOrDefault();
            }
            if (selectedObjective == null)
            {
                if (selectedMission != null && !selectedMission.Objectives.NullOrEmpty())
                {
                    selectedObjective = selectedMission.Objectives.Where(o => o.Active && !o.Finished).FirstOrDefault();
                }
            }
            MissionHandler.openedOnce = true;
        }

        public List<Mission> AvailableMissions
        {
            get
            {
                return MissionHandler.Missions;
            }
        }

        public void SetTheme(ModContentPack mcp)
        {
            if (mcp != null)
            {
                MissionHandler.theme = MissionHandler.ModFolder.Find(mcpw => mcpw.packName == mcp.Identifier);
            }
            else { MissionHandler.theme = null; }
        }

        public void SetBackground(ModContentPackWrapper themeHolder)
        {
            if (themeHolder != null)
            {
                backgroundImage = ContentFinder<Texture2D>.Get(themeHolder.MCD.bannerTex);
            }
            else { backgroundImage = null; }
        }

        public void SetDiaShow(ObjectiveDef objective)
        {
            cachedImages.Clear();
            foreach (string s in objective.images)
            {
                Texture2D text = ContentFinder<Texture2D>.Get(s, false);
                if (text != null)
                {
                    cachedImages.Add(text);
                }
            }
        }

        public void DoProgressBar(Rect rect, string label, float pct, Texture2D barMat)
        {
            Widgets.FillableBar(rect, pct, barMat, MissionMats.black, true);
            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(rect, label);
        }

        public List<Pawn> CapablePawns(ObjectiveDef objective)
        {
            return Find.AnyPlayerHomeMap.mapPawns.AllPawns.Where(p => p.IsColonist && objective.skillRequirements.All((SkillRequirement x) => x.PawnSatisfies(p))).ToList();
        }

        public void ResolveTargetLabel(ObjectiveDef def, out string label)
        {
            label = "";
            if (def.objectiveType == ObjectiveType.Destroy && !def.targets.NullOrEmpty())
            {
                label = def.targets.Find(t => t.ThingDef.BaseMaxHitPoints == def.targets.Max(t2 => t2.ThingDef.BaseMaxHitPoints)).ThingDef.LabelCap;
            }
            if(def.objectiveType == ObjectiveType.Hunt && !def.targets.NullOrEmpty())
            {
                label = def.targets.Find(tv => tv.PawnKindDef.RaceProps.baseHealthScale == def.targets.Max(tv2 => tv2.PawnKindDef.RaceProps.baseHealthScale)).PawnKindDef.LabelCap;
            }
        }

        public override void DoWindowContents(Rect inRect)
        {
            SetBackground(MissionHandler.theme);
            if (backgroundImage != null)
            {
                Widgets.DrawTextureFitted(inRect, backgroundImage, 1f);
            }
            //Set default values
            float generalHeight = 630f;
            float leftPartWidth = 250f;
            float yOffSet = (inRect.height - generalHeight) / 2f;

            Rect leftPart = new Rect(0f, yOffSet, leftPartWidth, generalHeight).ContractedBy(5f);
            Rect rightPart = new Rect(leftPartWidth, yOffSet, inRect.width - leftPartWidth, generalHeight).ContractedBy(5f);

            DoLefPart(leftPart);
            if (selectedMission != null)
            {
                DoRightPart(rightPart, selectedMission);
            }
            Text.Anchor = 0;
        }

        public void DoLefPart(Rect rect)
        {
            //AddTabs
            Rect tabRect = new Rect(rect.x + 5f, rect.y - 20f, rect.width - 30f, 20f);
            string missions = "Missions".Translate();
            string themes = "Themes".Translate();
            Vector2 v1 = Text.CalcSize(missions);
            Vector2 v2 = Text.CalcSize(themes);
            v1.x += 6f;
            v2.x += 6f;
            Rect missionTab = new Rect(new Vector2(tabRect.x, tabRect.y), v1);
            Rect themeTab = new Rect(new Vector2(missionTab.xMax, missionTab.y), v2);

            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.DrawMenuSection(missionTab);
            Widgets.Label(missionTab, missions);
            if (Widgets.ButtonInvisible(missionTab))
            {
                tabFlag = true;
            }
            Widgets.DrawMenuSection(themeTab);
            Widgets.Label(themeTab, themes);
            if (Widgets.ButtonInvisible(themeTab))
            {
                tabFlag = false;
            }
            Text.Anchor = TextAnchor.MiddleLeft;

            Widgets.DrawMenuSection(rect);
            //Default Values
            float selectionHeight = 45f;
            float viewHeight = 0f;
            float selectionYPos = 0f;
            
            if (tabFlag)
            {
                //Do Mission Tab
                GUI.BeginGroup(rect);
                viewHeight = selectionHeight * AvailableMissions.Count();
                Rect viewRect = new Rect(0f, 0f, rect.width, viewHeight);
                Widgets.BeginScrollView(new Rect(0f, 0f, rect.width, rect.height), ref MissionHandler.missionScrollPos, viewRect, true);
                int ii = 0;
                for (int i = 0; i < MissionHandler.ModFolder.Count; i++)
                {
                    ModContentPackWrapper mcp = MissionHandler.ModFolder.ElementAt(i);
                    if (mcp.MCD != MCD.MainMissionControlDef)
                    {
                        ii++;
                        List<Mission> MissionList = this.AvailableMissions.Where(m => mcp.MCP.AllDefs.Contains(m.def) && !m.def.hideOnComplete).ToList();
                        float groupHeight = MissionList.Count * selectionHeight;

                        //Identifier
                        string identifier = mcp.MCD.label;
                        Vector2 identifierSize = Text.CalcSize(identifier);
                        identifierSize.x += 4f;
                        selectionYPos += identifierSize.y;
                        if (ii > 1)
                        {
                            selectionYPos += 10f;
                        }
                        Rect modGroup = new Rect(0f, selectionYPos, rect.width, groupHeight + 10f).ContractedBy(5f);
                        Rect identifierRect = new Rect(new Vector2(5f, modGroup.y - identifierSize.y + 1f), identifierSize);
                        Widgets.DrawMenuSection(new Rect(identifierRect.x, identifierRect.y, identifierRect.width + 15f, identifierRect.height));
                        Rect appendix = new Rect(identifierRect.xMax, identifierRect.y, 8f, identifierRect.height);
                        Text.Font = GameFont.Tiny;
                        Text.Anchor = TextAnchor.MiddleCenter;
                        if (mcp.toggled)
                        {
                            Widgets.Label(appendix, "-");
                        }
                        else { Widgets.Label(appendix, "+"); }                                       
                        Widgets.Label(identifierRect, identifier);
                        Text.Anchor = 0;
                        Text.Font = GameFont.Small;

                        if (Widgets.ButtonInvisible(new Rect(identifierRect.x, identifierRect.y, modGroup.width, identifierRect.height)))
                        {
                            MissionHandler.ModFolder.Find(m => m == mcp).Toggle();
                        }
                        if (mcp.toggled)
                        {
                            MissionUtils.DrawMenuSectionColor(modGroup, 1, mcp.MCD.color, mcp.MCD.borderColor);
                            float missionTabYPos = modGroup.yMin;
                            foreach (Mission mission in MissionList)
                            {
                                Rect rect4 = new Rect(modGroup.x, (float)missionTabYPos, modGroup.width, selectionHeight);
                                this.DoMissionTab(rect4, mission, mcp.MCD);
                                missionTabYPos += (int)selectionHeight;
                            }
                            selectionYPos += groupHeight;
                        }
                    }
                }
                Widgets.EndScrollView();
                GUI.EndGroup();
                Widgets.DrawHighlight(missionTab);
            }
            else
            {
                //Do Theme Tab
                GUI.BeginGroup(rect);
                viewHeight = selectionHeight * MissionHandler.ModFolder.Count;
                Rect viewRect = new Rect(0f, 0f, rect.width, viewHeight);
                Widgets.BeginScrollView(new Rect(0f, 0f, rect.width, rect.height), ref MissionHandler.missionScrollPos, viewRect, true);
                for(int i = 0; i < MissionHandler.ModFolder.Count+1; i++)
                {
                    Rect selection = new Rect(0f, selectionYPos, rect.width, selectionHeight).ContractedBy(5f);                    
                    Text.Anchor = TextAnchor.MiddleCenter;
                    if (i == 0)
                    {
                        Widgets.DrawMenuSection(selection);
                        Widgets.Label(selection, "None");
                        if (Mouse.IsOver(selection))
                        {
                            Widgets.DrawHighlight(selection);
                            if (Widgets.ButtonInvisible(selection))
                            {
                                SetTheme(null);
                            }
                        }
                    }
                    else
                    {
                        ModContentPackWrapper mcp = MissionHandler.ModFolder.ElementAt(i - 1);
                        if (mcp.MCD != MCD.MainMissionControlDef ? mcp.MCD.bannerTex != MCD.MainMissionControlDef.bannerTex : true)
                        {
                            //Widgets.DrawTextureFitted(selection.ContractedBy(1f), null, 1f);
                            Widgets.DrawMenuSection(selection);
                            Widgets.Label(selection, mcp.MCD.LabelCap);
                            if (Mouse.IsOver(selection))
                            {
                                Widgets.DrawHighlight(selection);
                                if (Widgets.ButtonInvisible(selection))
                                {
                                    SetTheme(mcp.MCP);
                                }
                            }
                        }
                    }
                    Text.Anchor = 0;
                    selectionYPos += selectionHeight;
                }
                Widgets.EndScrollView();
                GUI.EndGroup();
                Widgets.DrawHighlight(themeTab);
            }
        }

        public void DoRightPart(Rect rect, Mission mission)
        {
            Widgets.DrawMenuSection(rect);
            //Setup Rects
            Rect innerRect = rect.ContractedBy(10f);
            Rect descriptionRect = new Rect(innerRect.x, innerRect.y, innerRect.width / 3f, innerRect.height);
            Rect splitRect = new Rect(descriptionRect.xMax, descriptionRect.y, (innerRect.width / 3f) * 2f, innerRect.height);
            descriptionRect = descriptionRect.ContractedBy(5f);
            Rect imageRect = splitRect.TopHalf().ContractedBy(5f);
            Rect objectiveRect = splitRect.BottomHalf().ContractedBy(5f);

            //Description
            Widgets.DrawMenuSection(descriptionRect);
            StringBuilder description = new StringBuilder();
            description.AppendLine(selectedMission.def.description + "\n");
            description.AppendLine(selectedObjective?.def.description);
            Text.Anchor = TextAnchor.UpperLeft;
            Widgets.Label(descriptionRect.ContractedBy(5f), description.ToString());
            Text.Anchor = 0;

            //Image View
            Rect imageSwapL = new Rect(imageRect.x, imageRect.y, 45f, imageRect.height).ContractedBy(5f);
            Rect imageSwapR = new Rect(imageRect.xMax - 45f, imageRect.y, 45f, imageRect.height).ContractedBy(5f);
            Widgets.DrawShadowAround(imageRect);
            Widgets.DrawBoxSolid(imageRect, new Color(0.14f, 0.14f, 0.14f));

            if (selectedObjective != null)
            {
                SetDiaShow(selectedObjective.def);
                if (cachedImages.Count > 0 && selectedImage <= (cachedImages.Count - 1) && cachedImages[selectedImage] != null)
                {
                    Widgets.DrawTextureFitted(imageRect, cachedImages[selectedImage], 1f);
                }
            }
            if (Mouse.IsOver(imageSwapL))
            {
                if (selectedImage > 0)
                {
                    GUI.color = Color.gray;
                    Widgets.DrawHighlight(imageSwapL);
                    if (Widgets.ButtonText(imageSwapL, "", false, true, Color.blue, true))
                    {
                        selectedImage -= 1;
                    }
                }
            }
            if (Mouse.IsOver(imageSwapR))
            {
                if (selectedImage < cachedImages.Count - 1)
                {
                    GUI.color = Color.gray;
                    Widgets.DrawHighlight(imageSwapR);
                    if (Widgets.ButtonText(imageSwapR, "", false, true, Color.blue, true))
                    {
                        selectedImage += 1;
                    }
                }
            }

            //Objective View
            Widgets.DrawMenuSection(objectiveRect);
            float objectiveTabHeight = 60f;
            float viewHeight = objectiveTabHeight * mission.Objectives.Where(o => o.Active).Count();
            GUI.BeginGroup(objectiveRect);
            Rect inRect = new Rect(0f, 0f, objectiveRect.width, objectiveRect.height).ContractedBy(1f);
            Rect viewRect = new Rect(0f, 0f, inRect.width, viewHeight);

            Widgets.BeginScrollView(inRect, ref MissionHandler.objectiveScrollPos, viewRect, false);
            int objectiveTabYPos = 0;
            for (int i = 0; i < mission.Objectives.Count; i++)
            {
                Objective obj = mission.Objectives[i];
                if (obj.Active)
                {
                    Rect objectiveSelection = new Rect(0f, (float)objectiveTabYPos, objectiveRect.ContractedBy(1).width, objectiveTabHeight);
                    DoObjectiveTab(objectiveSelection, obj, i);
                    objectiveTabYPos += (int)objectiveTabHeight;
                }
            }
            Widgets.EndScrollView();
            GUI.EndGroup();
        }

        private void DoObjectiveTab(Rect rect, Objective objective, int num)
        {
            ObjectiveDef objectiveDef = objective.def;
            rect = rect.ContractedBy(1f);
            if(num % 2 == 0)
            {
                Widgets.DrawBoxSolid(rect, new ColorInt(50, 50, 50).ToColor);
            }
            //Rect Setup
            Rect inRect = rect.ContractedBy(5f);          
            //Label
            string objectiveLabel = objectiveDef.LabelCap;
            Vector2 labelVec = Text.CalcSize(objectiveLabel);
            Rect labelRect = new Rect(new Vector2(inRect.x, inRect.y), labelVec);
            Widgets.Label(labelRect, objectiveLabel);

            //Station - Targets
            if ((objectiveDef.objectiveType == ObjectiveType.Destroy || objectiveDef.objectiveType == ObjectiveType.Hunt) && !objectiveDef.targets.NullOrEmpty())
            {
                bool typeFlag = objectiveDef.objectiveType == ObjectiveType.Destroy;
                ResolveTargetLabel(objectiveDef, out string s);
                string s2 = "Targets".Translate() + ": " + s;
                Vector2 v2 = Text.CalcSize(s2);
                v2.x += 4f;
                Rect rect2 = new Rect(new Vector2(inRect.x, inRect.yMax - v2.y), v2);
                MissionUtils.DrawMenuSectionColor(rect2, 1, new ColorInt(35, 35, 35), new ColorInt(85, 85, 85));
                Widgets.Label(rect2, s2);

                if (!objectiveDef.targets.NullOrEmpty())
                {
                    StringBuilder sb = new StringBuilder();
                    foreach (ThingValue tv in objectiveDef.targets)
                    {
                        if (typeFlag)
                        {sb.AppendLine("    " + tv.ThingDef.LabelCap + ": " + objective.thingTracker.destroyedThings[tv.ThingDef] + "/" + tv.value);}
                        else
                        {sb.AppendLine("    " + tv.PawnKindDef.LabelCap + ": " + objective.thingTracker.killedThings[tv.PawnKindDef] + "/" + tv.value);}
                    }
                    if (objectiveDef.targets.Count > 1)
                    {
                        TooltipHandler.TipRegion(rect2, "AllTargets".Translate(new object[] {
                    s,
                    sb
                }));
                    }
                }
            }
            if (!objectiveDef.stationDefs.NullOrEmpty())
            {
                string s = "StationNeeded".Translate() + ": " + objectiveDef.BestPotentialStationDef.LabelCap;
                Vector2 v2 = Text.CalcSize(s);
                v2.x += 4f;
                Rect rect2 = new Rect(new Vector2(inRect.x, inRect.yMax - v2.y), v2);
                MissionUtils.DrawMenuSectionColor(rect2, 1, new ColorInt(35, 35, 35), new ColorInt(85, 85, 85));
                Widgets.Label(rect2, s);

                if (objectiveDef.stationDefs.Count > 1)
                {
                    StringBuilder sb = new StringBuilder();
                    foreach (ThingDef def in objectiveDef.stationDefs)
                    {
                        sb.AppendLine("    " + def.LabelCap);
                    }
                    TooltipHandler.TipRegion(rect2, "PotentialStations".Translate(new object[] {
                    objectiveDef.BestPotentialStationDef.LabelCap,
                    sb
                }));
                }
            }

            //Bars
            Vector2 size = new Vector2(90f, 20f);
            Rect BarRect = new Rect(new Vector2(inRect.xMax - size.x, inRect.y), size);
            Rect BotBarRect = new Rect(new Vector2(inRect.xMax - size.x, inRect.yMax - size.y), size);
            float pct = 0f;
            float pctAny = 0f;
            string label = "";
            string labelAny = "";

            if (objectiveDef.workAmount > 0)
            {
                pct = objective.ProgressPct;
                label = Mathf.RoundToInt(objective.GetProgress) + "/" + objectiveDef.workAmount;

                DoProgressBar(BarRect, label, pct, MissionMats.blue);
                BarRect = BotBarRect;
            }
            else if (objectiveDef.objectiveType == ObjectiveType.Construct || objectiveDef.objectiveType == ObjectiveType.Craft)
            {
                if (objectiveDef.anyTarget)
                {
                    ThingDef maxDef = objective.thingTracker.madeThings.ToList().Find(mt => mt.Value == objective.thingTracker.madeThings.Values.Max()).Key;
                    pctAny = (float)objective.thingTracker.madeThings[maxDef] / (float)objectiveDef.targets.Find(tv => tv.ThingDef == maxDef).value;
                    labelAny = (float)objective.thingTracker.madeThings[maxDef] + "/" + (float)objectiveDef.targets.Find(tv => tv.ThingDef == maxDef).value;
                }
                else
                {
                    pct = objective.thingTracker.GetCountMade / (float)objectiveDef.targets.Sum(tv => tv.value);
                    label = (float)objective.thingTracker.GetCountMade + "/" + (float)objectiveDef.targets.Sum(tv => tv.value);
                }
                DoProgressBar(BarRect, objectiveDef.anyTarget ? labelAny : label, objectiveDef.anyTarget ? pctAny : pct, MissionMats.orange);

                StringBuilder sb = new StringBuilder();
                foreach (ThingValue tv in objectiveDef.targets)
                {
                    sb.AppendLine("    " + tv.ThingDef.LabelCap + ": " + objective.thingTracker.GetCountMadeFor(tv.ThingDef) + "/" + tv.value);
                }
                if (objectiveDef.targets.Count > 1)
                {
                    string specific = "MakeTargets".Translate(new object[] { sb });
                    string any = "MakeTargetsAny".Translate(new object[] { sb });
                    TooltipHandler.TipRegion(BarRect, objectiveDef.anyTarget ? any : specific);
                }
                BarRect = BotBarRect;
            }
            else if (objectiveDef.objectiveType == ObjectiveType.Destroy || objectiveDef.objectiveType == ObjectiveType.Hunt)
            {
                //True means ThingDef - False mean PawnKindDef
                bool typeFlag = objectiveDef.objectiveType == ObjectiveType.Destroy;
                if (objectiveDef.anyTarget)
                {
                    if (typeFlag)
                    {
                        ThingDef maxDef = objective.thingTracker.destroyedThings.ToList().Find(dt => dt.Value == objective.thingTracker.destroyedThings.Values.Max()).Key;
                        pctAny = (float)objective.thingTracker.destroyedThings[maxDef] / (float)objectiveDef.targets.Find(tv => tv.ThingDef == maxDef).value;
                        labelAny = (float)objective.thingTracker.destroyedThings[maxDef] + "/" + (float)objectiveDef.targets.Find(tv => tv.ThingDef == maxDef).value;
                    }
                    else
                    {
                        PawnKindDef maxDef = objective.thingTracker.killedThings.ToList().Find(dt => dt.Value == objective.thingTracker.killedThings.Values.Max()).Key;
                        pctAny = (float)objective.thingTracker.killedThings[maxDef] / (float)objectiveDef.targets.Find(tv => tv.PawnKindDef == maxDef).value;
                        labelAny = (float)objective.thingTracker.killedThings[maxDef] + "/" + (float)objectiveDef.targets.Find(tv => tv.PawnKindDef == maxDef).value;
                    }
                }
                else
                {
                    pct = ((float)objective.thingTracker.GetSumKilledDestroyed / (float)objectiveDef.targets.Sum(tv => tv.value));
                    label = objective.thingTracker.GetSumKilledDestroyed + "/" + objectiveDef.targets.Sum(tv => tv.value);
                }
                DoProgressBar(BarRect, objectiveDef.anyTarget ? labelAny : label, objectiveDef.anyTarget ? pctAny : pct, MissionMats.green);
                BarRect = BotBarRect;
            }
            else if (objectiveDef.objectiveType == ObjectiveType.Discover)
            {
                pct = ((float)objective.thingTracker.GetCountDiscovered / (float)objectiveDef.targets.Sum(tv => tv.value));
                label = objective.thingTracker.GetCountDiscovered + "/" + objectiveDef.targets.Sum(tv => tv.value);
                DoProgressBar(BarRect, label, pct, MissionMats.green);
                BarRect = BotBarRect;
            }
            if (objectiveDef.TimerTicks > 0)
            {
                float timer = objective.GetTimer;
                pct = timer / objectiveDef.TimerTicks;
                label = objective.GetTimerText;
                if (objective.Finished)
                {
                    label = "---";
                    pct = 0f;
                }
                DoProgressBar(BarRect, label, pct, MissionMats.grey);
            }

            //SkillReq
            Rect skillRect = new Rect();
            if (objectiveDef.skillRequirements.Count > 0)
            {
                skillRect = new Rect(inRect.xMax - 2 * BarRect.width - 5f, inRect.y, BarRect.width, inRect.height);
                List<Pawn> pawnList = CapablePawns(objectiveDef);
                Widgets.DrawMenuSection(skillRect);

                Text.Anchor = TextAnchor.UpperCenter;
                Widgets.Label(skillRect, "Pawns: ");
                Text.Anchor = TextAnchor.LowerCenter;
                Widgets.Label(skillRect, "" + pawnList.Count);
                Text.Anchor = 0;

                StringBuilder skills = new StringBuilder();
                foreach (SkillRequirement skill in objectiveDef.skillRequirements)
                {
                    skills.AppendLine("    " + skill.skill.LabelCap + ": " + skill.minLevel);
                }
                StringBuilder pawns = new StringBuilder();
                foreach (Pawn p in pawnList)
                {
                    pawns.AppendLine("    " + p.LabelCap);
                }
                TooltipHandler.TipRegion(skillRect, "ObjectiveSkills".Translate(new object[] {
                    skills,
                    pawnList.Count > 0 ? "ObjectivePawns".Translate(new object[]{
                        pawns
                    }) : ""
                }));
            }

            //End Parts
            if (objective.Failed)
            {
                GUI.color = Color.red;
                Widgets.DrawHighlight(rect);
                GUI.color = Color.white;
            }
            if (Mouse.IsOver(rect) || this.selectedObjective == objective)
            {
                GUI.color = Color.yellow;
                Widgets.DrawHighlight(rect);
                GUI.color = Color.white;
            }

            Rect skillSelect = new Rect(new Vector2(inRect.xMax - (skillRect.width * 2) - 5, inRect.y), skillRect.size);
            if (Widgets.ButtonText(new Rect(rect.x, rect.y, rect.width, rect.height), "", false, true, Color.blue, true))
            {
                if (Mouse.IsOver(skillSelect))
                {
                    Find.Selector.SelectedObjects.Clear();
                    if (!CapablePawns(objectiveDef).NullOrEmpty())
                    {
                        Find.Selector.Select(CapablePawns(objectiveDef).RandomElement());
                        this.Close();
                    }
                }
                SoundDefOf.Click.PlayOneShotOnCamera(null);
                this.selectedObjective = objective;
                selectedImage = 0;
            }
        }

        private void DoMissionTab(Rect rect, Mission mission, MissionControlDef MCD)
        {
            MissionHandler.Notify_Seen(mission);
            rect = rect.ContractedBy(3f);
            if (Mouse.IsOver(rect) || this.selectedMission == mission)
            {
                GUI.color = Color.yellow;
                Widgets.DrawHighlight(rect);
            }
            GUI.color = Color.white;
            WidgetRow widgetRow = new WidgetRow(rect.x, rect.y + (rect.height - 24f) / 2, UIDirection.RightThenUp, 99999f, 1f);
            if (mission != null && !mission.def.IsFinished)
            {
                widgetRow.Icon(ContentFinder<Texture2D>.Get(MCD.boxActive, false), null);
            }
            else if (mission.failed)
            {
                widgetRow.Icon(ContentFinder<Texture2D>.Get(MCD.boxFailed, false), null);
            }
            else if (mission.def.IsFinished)
            {
                widgetRow.Icon(ContentFinder<Texture2D>.Get(MCD.boxFinished, false), null);
            }
            widgetRow.Gap(5f);
            widgetRow.Label(mission.def.label, 200f);
            if (Widgets.ButtonText(rect, "", false, true, Color.blue, true))
            {
                SoundDefOf.Click.PlayOneShotOnCamera(null);
                this.selectedMission = mission;
                this.selectedObjective = selectedMission.Objectives.FirstOrDefault();
            }
        }
    }
}
