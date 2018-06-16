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
        public WorldComponent_Missions Missions;

        public Vector2 scrollPosLeft = Vector2.zero;

        public Vector2 scrollPosObj = Vector2.zero;

        public Mission selectedMission;

        public ObjectiveDef selectedObjective;

        private int imgNum = 0;

        private Dictionary<ObjectiveDef, Vector2> skillScroll = new Dictionary<ObjectiveDef, Vector2>();

        public List<Texture2D> imageRow = new List<Texture2D>();

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
            this.Missions = WorldComponent_Missions.MissionHandler;
            if (selectedMission == null)
            {
                this.selectedMission = Missions.Missions.FirstOrDefault();
            }
        }

        public List<Mission> AvailableMissions
        {
            get
            {
                return Missions.Missions;
            }
        }

        public List<Pawn> CapablePawns(ObjectiveDef objective)
        {
            return Find.AnyPlayerHomeMap.mapPawns.AllPawns.Where(p => p.IsColonist && objective.skillRequirements.All((SkillRequirement x) => x.PawnSatisfies(p))).ToList();
        }

        public override void DoWindowContents(Rect inRect)
        {
            /*
            if ((this.selectedMission = this.Missions.Missions.FirstOrDefault((Mission x) => x.def.CanStartNow)) == null)
            {
                Text.Font = GameFont.Tiny;
                string s = "Thanks Mehni. Look what you made me do.";
                Vector2 v = Text.CalcSize(s);
                Rect rect = new Rect(new Vector2(0f, 0f), v);
                rect.center = inRect.center;
                Widgets.DrawMenuSection(rect);
                Widgets.Label(rect, s);
                return;
            }
            */
            Widgets.DrawTextureFitted(inRect, MissionMats.WorkBanner, 1f);

            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.MiddleLeft;

            float missionTabHeight = 35f;
            float yHeight = 630f;
            float yOffset = (inRect.height - yHeight) / 2f;
            Rect refRect = new Rect(0f, yOffset, inRect.width, yHeight);
            GUI.BeginGroup(refRect);
            float viewHeight = missionTabHeight * AvailableMissions.Count();

            Rect leftPart = new Rect(0f, 0f, 250f, refRect.height).ContractedBy(5f);
            Widgets.DrawMenuSection(leftPart);
            Rect viewRect = new Rect(0f, 0f, leftPart.width, viewHeight);
            GUI.BeginGroup(leftPart);
            Widgets.BeginScrollView(new Rect(0f, 0f, leftPart.width, leftPart.height), ref this.scrollPosLeft, viewRect, true);
            float modGroupYPos = 0;
            int ii = 0;
            for (int i = 0; i < LoadedModManager.RunningMods.Count(); i++)
            {
                ModContentPack mcp = LoadedModManager.RunningMods.ElementAt(i);
                if (mcp.AllDefs.Any(d => d is MissionDef && AvailableMissions.Any(m => m.def == d)))
                {               
                    ii++;
                    Def def = mcp.AllDefs.ToList().Find(d => d is MissionControlDef);
                    MissionControlDef MCD2 = def != null ? def as MissionControlDef : MCD.MainMissionControlDef;

                    Text.Font = GameFont.Tiny;
                    List<Mission> MissionList = this.AvailableMissions.Where(m => mcp.AllDefs.Contains(m.def) && !m.def.hideOnComplete).ToList();
                    float groupHeight = MissionList.Count * missionTabHeight;
                    string identifier = MCD2.label;
                    Vector2 identifierSize = Text.CalcSize(identifier);
                    modGroupYPos += identifierSize.y;
                    if (ii > 1)
                    {
                        modGroupYPos += 10f;
                    }
                    identifierSize.x += 3f;                   
                    Rect modGroup = new Rect(0f, modGroupYPos, leftPart.width, groupHeight + 10f).ContractedBy(5f);
                    Rect identifierRect = new Rect(new Vector2(5f, modGroup.y - identifierSize.y + 1f), identifierSize);
                    Widgets.DrawMenuSection(identifierRect);
                    Text.Anchor = TextAnchor.MiddleCenter;
                    Widgets.Label(identifierRect, identifier);
                    Text.Anchor = 0;
                    Text.Font = GameFont.Small;
                    if (Widgets.ButtonInvisible(new Rect(identifierRect.x, identifierRect.y, modGroup.width, identifierRect.height)))
                    {
                        ModContentPackWrapper mcpw = new ModContentPackWrapper(mcp.Identifier);
                        if (!Missions.ModFolder.Any(m => m.packName == mcp.Identifier))
                        {
                            Missions.ModFolder.Add(mcpw);
                        }
                        else
                        {
                            Missions.ModFolder.Find(m => m.packName == mcp.Identifier).toggled = !Missions.ModFolder.Find(m => m.packName == mcp.Identifier).toggled;
                        }
                    }
                    if (Missions.ModFolder.Find(mcpw => mcpw.packName == mcp.Identifier)?.toggled ?? true)
                    {                        
                        MissionUtils.DrawMenuSectionColor(modGroup, 1, MCD2.color, MCD2.borderColor);
                        float missionTabYPos = modGroup.yMin;
                        foreach (Mission mission in MissionList)
                        {                      
                            Rect rect4 = new Rect(modGroup.x, (float)missionTabYPos, modGroup.width, missionTabHeight);
                            this.DoMissionTab(rect4, mission, MCD2);
                            missionTabYPos += (int)missionTabHeight;
                        }
                        modGroupYPos += groupHeight;
                    }
                }
            }
            Widgets.EndScrollView();
            GUI.EndGroup();
            Rect rightPart = new Rect(250f, 0f, refRect.width - 250, refRect.height).ContractedBy(5f);
            Widgets.DrawMenuSection(rightPart);
            rightPart = rightPart.ContractedBy(10f);
            GUI.BeginGroup(rightPart);
            if (selectedMission != null)
            {
                DoObjectiveWindow(rightPart, selectedMission);
            }
            GUI.EndGroup();
            GUI.EndGroup();
            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.UpperLeft;
        }

        private void DoMissionTab(Rect rect, Mission mission, MissionControlDef MCD)
        {
            Missions.Notify_Seen(mission);

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
                this.selectedObjective = null;
            }
        }

        private void DoObjectiveTab(Rect rect, Objective objective, int curObj)
        {
            ObjectiveDef objectiveDef = objective.def;

            rect = rect.ContractedBy(1f);
            if (curObj % 2 == 0)
            {
                Widgets.DrawBoxSolid(rect, new ColorInt(50, 50, 50).ToColor);
            }
            GUI.BeginGroup(rect);
            WidgetRow widgetRow = new WidgetRow(5f, 0f, UIDirection.RightThenUp, 99999f, 4f);
            widgetRow.Label(objectiveDef.label, 200f);

            Rect CornerRect = new Rect(rect.xMax - 100f, 0f, 100f, 30f).ContractedBy(5f);

            bool flag = objectiveDef.workCost > 0;
            float pct = 0;
            string label = "";


            if (objectiveDef.workCost > 0)
            {
                pct = objective.ProgressPct;
                label = Mathf.RoundToInt(objective.GetProgress) + "/" + objectiveDef.workCost;

                DoProgressBar(CornerRect, label, pct, MissionMats.blue);
                CornerRect.y = 30f;
            }
            else if (objectiveDef.objectiveType == ObjectiveType.Construct || objectiveDef.objectiveType == ObjectiveType.Craft)
            {
                pct = (objective.Finished ? 1f : 0f);
                label = objective.Finished ? "1/1" : "0/1";
                DoProgressBar(CornerRect, label, pct, MissionMats.orange);
                CornerRect.y = 30f;
            }
            else if(objectiveDef.objectiveType == ObjectiveType.Destroy || objectiveDef.objectiveType == ObjectiveType.Hunt)
            {
                pct = ((float)objective.killTracker.GetCountKilled / (float)objectiveDef.killAmount);
                label = objective.killTracker.GetCountKilled.ToString() + "/" + objectiveDef.killAmount;
                DoProgressBar(CornerRect, label, pct, MissionMats.green);
                CornerRect.y = 30f;
            }
            else if(objectiveDef.objectiveType == ObjectiveType.Discover)
            {
                pct = ((float)objective.killTracker.GetCountDiscovered / (float)objectiveDef.targetThings.Count);
                label = objective.killTracker.GetCountDiscovered.ToString() + "/" + objectiveDef.targetThings.Count;
                DoProgressBar(CornerRect, label, pct, MissionMats.green);
                CornerRect.y = 30f;
            }
            if (objectiveDef.TimerTicks > 0)
            {
                float timer = objective.GetTimer;
                pct = timer / objectiveDef.TimerTicks;
                if (timer > GenDate.TicksPerYear)
                {
                    label = Math.Round(timer / GenDate.TicksPerYear, 1) + "y";
                }
                else if (timer > GenDate.TicksPerDay)
                {
                    label = Math.Round(timer / GenDate.TicksPerDay, 1) + "d";
                }
                else if (timer < GenDate.TicksPerDay)
                {
                    label = Math.Round(timer / GenDate.TicksPerHour, 1) + "h";
                }
                if (objective.Finished)
                {
                    label = "---";
                    pct = 0f;
                }
                DoProgressBar(CornerRect, label, pct, MissionMats.grey);
            }

            Rect skillRect = new Rect();
            if (objectiveDef.skillRequirements.Count > 0)
            {
                foreach (ObjectiveDef objectiveDef2 in selectedMission.def.objectives)
                {
                    if (!skillScroll.Keys.Contains(objectiveDef2))
                    {
                        skillScroll.Add(objectiveDef2, new Vector2());
                    }
                }
                List<Pawn> pawnList = CapablePawns(objectiveDef);
                skillRect = new Rect(rect.xMax - (CornerRect.width * 2f) - 10f, 0f, CornerRect.width + 5f, rect.height).ContractedBy(5f);
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

            Text.Anchor = 0;
            if ( (objectiveDef.objectiveType == ObjectiveType.Destroy || objectiveDef.objectiveType == ObjectiveType.Hunt) && (!objectiveDef.targetThings.NullOrEmpty() || !objectiveDef.targetPawns.NullOrEmpty()))
            {
                ResolveTargetLabel(objectiveDef, out string s);
                string s2 = "Targets".Translate() + ": " + s;
                Vector2 v2 = Text.CalcSize(s2);
                Rect rect2 = new Rect(5f, rect.height / 2, v2.x, v2.y);
                MissionUtils.DrawMenuSectionColor(rect2.ExpandedBy(1f), 1, new ColorInt(35, 35, 35), new ColorInt(85, 85, 85));
                Widgets.Label(rect2, s2);

                if (!objectiveDef.targetThings.NullOrEmpty())
                {
                    StringBuilder sb = new StringBuilder();
                    foreach (ThingDef def in objectiveDef.targetThings)
                    {
                        sb.AppendLine("    " + def.LabelCap);
                    }
                    TooltipHandler.TipRegion(rect2, "AllTargets".Translate(new object[] {
                    s,
                    sb
                }));
                }
                if (!objectiveDef.targetPawns.NullOrEmpty())
                {
                    StringBuilder sb = new StringBuilder();
                    foreach (PawnKindDef def in objectiveDef.targetPawns)
                    {
                        sb.AppendLine("    " + def.LabelCap);
                    }
                    TooltipHandler.TipRegion(rect2, "AllTargets".Translate(new object[] {
                    s,
                    sb
                }));
                }
            }
            if (!objectiveDef.stationDefs.NullOrEmpty())
            {
                string s = "StationNeeded".Translate() + ": " + objectiveDef.BestPotentialStationDef.LabelCap;
                Vector2 v2 = Text.CalcSize(s);
                Rect rect2 = new Rect(5f, rect.height / 2, v2.x, v2.y);
                MissionUtils.DrawMenuSectionColor(rect2.ExpandedBy(1f), 1, new ColorInt(35, 35, 35), new ColorInt(85, 85, 85));
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

            GUI.EndGroup();
            if (objective.Failed)
            {
                GUI.color = Color.red;
                Widgets.DrawHighlight(rect);
                GUI.color = Color.white;
            }
            if (Mouse.IsOver(rect) || this.selectedObjective == objectiveDef)
            {
                GUI.color = Color.yellow;
                Widgets.DrawHighlight(rect);
                GUI.color = Color.white;
            }

            skillRect.center = new Vector2(skillRect.center.x, skillRect.center.y + (curObj * rect.ExpandedBy(1f).height));
            if (Widgets.ButtonText(new Rect(rect.x, rect.y, rect.width, rect.height), "", false, true, Color.blue, true))
            {
                if (Mouse.IsOver(skillRect))
                {
                    Find.Selector.SelectedObjects.Clear();
                    if (!CapablePawns(objectiveDef).NullOrEmpty())
                    {
                        Find.Selector.Select(CapablePawns(objectiveDef).RandomElement());
                        this.Close();
                    }
                }
                SoundDefOf.Click.PlayOneShotOnCamera(null);
                this.selectedObjective = objectiveDef;
                imgNum = 0;
            }
        }

        public void ResolveTargetLabel(ObjectiveDef def, out string label)
        {
            label = "";
            if (!def.targetThings.NullOrEmpty())
            {
                label = def.targetThings.Find(t => t.BaseMaxHitPoints == def.targetThings.Max(t2 => t2.BaseMaxHitPoints)).LabelCap;
            }
            if (!def.targetPawns.NullOrEmpty())
            {
                label = def.targetPawns.Find(t => t.RaceProps.baseHealthScale == def.targetPawns.Max(t2 => t2.RaceProps.baseHealthScale)).LabelCap;
            }
        }

        public void DoProgressBar(Rect rect, string label, float pct, Texture2D barMat)
        {
            Widgets.FillableBar(rect, pct, barMat, MissionMats.black, true);
            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(rect, label);
        }

        public void DoObjectiveWindow(Rect rect, Mission mission)
        {
            if (selectedObjective == null)
            {
                selectedObjective = mission.def.objectives.FirstOrDefault();
            }
            Text.Font = GameFont.Small;
            Rect descRect = new Rect(0, 0, rect.width / 3, rect.height).ContractedBy(10f);
            Widgets.DrawMenuSection(descRect);
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(selectedMission.def.description);
            sb.AppendLine("");
            sb.AppendLine(selectedObjective?.description);

            Text.Anchor = TextAnchor.UpperLeft;
            Widgets.Label(descRect.ContractedBy(5f), sb.ToString());
            Text.Anchor = 0;

            Rect imageRect = new Rect(rect.width / 3, 0, (rect.width * 2) / 3, rect.height / 2).ContractedBy(10f);
            Rect imageSwapL = new Rect(imageRect.x, imageRect.y, 45f, imageRect.height).ContractedBy(5f);
            Rect imageSwapR = new Rect(imageRect.xMax - 45f, imageRect.y, 45f, imageRect.height).ContractedBy(5f);

            Widgets.DrawShadowAround(imageRect);
            Widgets.DrawBoxSolid(imageRect, new Color(0.14f, 0.14f, 0.14f));

            if (selectedObjective != null)
            {
                SetDiaShow(selectedObjective);
                if (imageRow.Count > 0 && imgNum <= (imageRow.Count - 1) && imageRow[imgNum] != null)
                {
                    Widgets.DrawTextureFitted(imageRect, imageRow[imgNum], 1f);
                }
            }
            if (Mouse.IsOver(imageSwapL))
            {
                if (imgNum > 0)
                {
                    GUI.color = Color.gray;
                    Widgets.DrawHighlight(imageSwapL);
                    if (Widgets.ButtonText(imageSwapL, "", false, true, Color.blue, true))
                    {
                        imgNum -= 1;
                    }
                }
            }
            if (Mouse.IsOver(imageSwapR))
            {
                if (imgNum < imageRow.Count - 1)
                {
                    GUI.color = Color.gray;
                    Widgets.DrawHighlight(imageSwapR);
                    if (Widgets.ButtonText(imageSwapR, "", false, true, Color.blue, true))
                    {
                        imgNum += 1;
                    }
                }
            }
            Rect objectiveMenu = new Rect(rect.width / 3f, rect.height / 2f, rect.width - rect.width / 3, rect.height / 2).ContractedBy(10f);
            float objectiveTabHeight = 60f;
            float viewHeight = objectiveTabHeight * mission.Objectives.Where(o => o.Active).Count();
            Widgets.DrawMenuSection(objectiveMenu);
            GUI.BeginGroup(objectiveMenu);
            Rect inRect = new Rect(0f, 0f, objectiveMenu.width, objectiveMenu.height).ContractedBy(1f);
            Rect viewRect = new Rect(0f, 0f, inRect.width, viewHeight);

            Widgets.BeginScrollView(inRect, ref this.scrollPosObj, viewRect, false);
            int objectiveTabYPos = 0;
            for (int i = 0; i < mission.Objectives.Count; i++)
            {
                Objective obj = mission.Objectives[i];
                if (obj.Active)
                {
                    Rect rect4 = new Rect(0f, (float)objectiveTabYPos, objectiveMenu.ContractedBy(1).width, objectiveTabHeight);
                    DoObjectiveTab(rect4, obj, i);
                    objectiveTabYPos += (int)objectiveTabHeight;
                }
            }
            Widgets.EndScrollView();
            GUI.EndGroup();

        }

        public void SetDiaShow(ObjectiveDef objective)
        {
            imageRow.Clear();
            foreach (string s in objective.images)
            {
                Texture2D text = ContentFinder<Texture2D>.Get(s, true);
                imageRow.Add(text);
            }
        }
    }
}
