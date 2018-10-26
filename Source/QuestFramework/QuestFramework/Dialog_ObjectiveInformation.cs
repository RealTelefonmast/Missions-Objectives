using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace StoryFramework
{
    public class Dialog_ObjectiveInformation : Window
    {
        private Objective objective;
        public Vector2 MainScrollPos = Vector2.zero;
        public Vector2 TargetPos = Vector2.zero;
        public Vector2 RewardCompletedPos = Vector2.zero;
        public Vector2 RewardFailedPos = Vector2.zero;
        private const float gap = 10f;

        public Dialog_ObjectiveInformation(){}

        public Dialog_ObjectiveInformation(Objective objective)
        {
            soundAppear = SoundDefOf.InfoCard_Open;
            soundClose = SoundDefOf.InfoCard_Close;
            this.objective = objective;
            closeOnClickedOutside = true;
        }

        public override Vector2 InitialSize => new Vector2(500f, 800f);

        public override void DoWindowContents(Rect inRect)
        {
            float CurrentY = 0f;
            //Title
            Text.Font = GameFont.Medium;
            string Title = objective.def.LabelCap;
            Vector2 TitleSize = Text.CalcSize(Title);
            Rect TitleRect = new Rect(new Vector2(0f, CurrentY), TitleSize);
            Widgets.Label(TitleRect, Title);
            Text.Font = GameFont.Small;
            CurrentY += TitleRect.height;
            
            Rect MainInfoBody = new Rect(0f, CurrentY, inRect.width, inRect.height - CurrentY);
            Widgets.DrawMenuSection(MainInfoBody);
            Rect MainInfoSub = MainInfoBody.ContractedBy(5f);
            CurrentY = 0f;
            GUI.BeginGroup(MainInfoSub);

            float tenth = MainInfoSub.width * 0.1f;
            //Objective Type - State - Work - Time
            string TypeLabel = "OType_SMO".Translate() + ": " + (objective.def.type.ToString() + "_Label").Translate();
            Vector2 TypeLabelSize = Text.CalcSize(TypeLabel);
            Rect TypeLabelRect = new Rect(new Vector2(5f, CurrentY), TypeLabelSize);

            string StateLabel = "OState_SMO".Translate() + ": " + (objective.CurrentState.ToString() + "_SMO").Translate();
            Vector2 StateLabelSize = Text.CalcSize(StateLabel);
            Rect StateLabelRect = new Rect(new Vector2(tenth * 5f, CurrentY), StateLabelSize);
            CurrentY += StateLabelRect.height + 5f;

            bool workExists = objective.def.workAmount > 0;
            string WorkAmount = "WorkAmount_SMO".Translate() + ": " + (workExists ? Math.Round(objective.GetWorkDone, 0) + "/" + objective.def.workAmount : "N/A");
            Vector2 WorkAmountSize = Text.CalcSize(WorkAmount);
            string TimerLabel = "Timer_SMO".Translate() + ": " + StoryUtils.GetTimerText(objective.GetTimer, objective.CurrentState);
            Vector2 TimerLabelSize = Text.CalcSize(TimerLabel);
            Rect WorkAmountRect = new Rect(new Vector2(5f, CurrentY), WorkAmountSize);
            Rect TimerLabelRect = new Rect(new Vector2(tenth * 5f, CurrentY), TimerLabelSize);
            CurrentY += WorkAmountRect.height;

            Rect TSWTRect = new Rect(0f, TypeLabelRect.yMin, MainInfoSub.width, TypeLabelRect.height + WorkAmountRect.height + 5f);
            Widgets.DrawBoxSolid(TSWTRect, new ColorInt(33, 33, 33).ToColor);
            Widgets.Label(TypeLabelRect, TypeLabel);
            Widgets.Label(StateLabelRect, StateLabel);
            Widgets.Label(WorkAmountRect, WorkAmount);
            Widgets.Label(TimerLabelRect, TimerLabel);
            AddGap(MainInfoSub, ref CurrentY);

            //TravelSettings
            TravelSettings travelSettings = objective.def.travelSettings;
            float width = MainInfoSub.width - 10f;
            if (travelSettings != null)
            {
                TravelMode mode = travelSettings.mode;
                string ModeLabel = (mode.ToString() + "_SMO").Translate();
                float ModeLabelHeight = Text.CalcHeight(ModeLabel, width);
                Rect ModeLabelRect = new Rect(5f, CurrentY, width, ModeLabelHeight);
                Rect BoxRect = new Rect();
                List<string> factions = new List<string>();
                List<string> factionCounts = new List<string>();

                if (!travelSettings.factionSettings.factions.NullOrEmpty())
                {
                    foreach (FactionDef def in travelSettings.factionSettings.factions)
                    {
                        string label = def.LabelCap;
                        factions.Add(label);
                    }
                    foreach (FactionDef def in travelSettings.factionSettings.factions)
                    {
                        string label = def.LabelCap;
                        factionCounts.Add(label + ": " + objective.travelTracker.CountFor(def) + "/" + travelSettings.factionSettings.Value(mode));
                    }
                }

                if (mode == TravelMode.Reach || mode == TravelMode.Explore)
                {
                    float height = 4f * 25f;
                    BoxRect = new Rect(0f, CurrentY, MainInfoSub.width, height);
                    Widgets.DrawBoxSolid(BoxRect, new ColorInt(33, 33, 33).ToColor);
                    CurrentY += ModeLabelHeight + 5f;
                    var listing = new Listing_Standard(GameFont.Small);
                    listing.Begin(new Rect(5f, CurrentY, MainInfoSub.width - 10f, height));
                    listing.ColumnWidth = (MainInfoSub.width - 10f) * 0.5f;
                    //left side
                    string Distance = "TravelDistance_SMO".Translate(travelSettings.minDistance);
                    listing.Label(Distance, Text.CalcHeight(Distance, listing.ColumnWidth));
                    string Biome = "TileBiome_SMO".Translate() + ": " + (travelSettings.tileSettings.biome?.LabelCap ?? "N/A");
                    listing.Label(Biome, Text.CalcHeight(Biome, listing.ColumnWidth));
                    string River = "TileA_SMO".Translate() + " " + (travelSettings.tileSettings.river?.label ?? "N/A");
                    listing.Label(River, Text.CalcHeight(River, listing.ColumnWidth));
                    //right side
                    listing.NewColumn();
                    string Destination = "TravelDestination_SMO".Translate() + ": " + (travelSettings.destination?.label ?? "N/A");
                    listing.Label(Destination, Text.CalcHeight(Destination, listing.ColumnWidth));
                    string Road = "TileA_SMO".Translate() + " " + travelSettings.tileSettings.road?.label;
                    listing.Label(Road, Text.CalcHeight(Road, listing.ColumnWidth));
                    string Hilliness = "Tile_SMO".Translate() + " " + (travelSettings.tileSettings.hilliness.HasValue ? travelSettings.tileSettings.hilliness.Value.GetLabel() : "N/A");
                    listing.Label(Hilliness, Text.CalcHeight(Hilliness, listing.ColumnWidth));
                    listing.End();
                    CurrentY += (height - 23f);
                    Widgets.Label(ModeLabelRect, ModeLabel);
                }
                else if (mode == TravelMode.Trade)
                {
                    CurrentY += ModeLabelHeight;
                    Rect ValueLabelRect = new Rect();
                    string ValueLabel = "";
                    int silver = travelSettings.minSilver;
                    if (silver != 0)
                    {
                        ValueLabel = silver > 0 ? "TradeBuy_SMO".Translate(silver) : "TradeSell_SMO".Translate(-silver);
                        Vector2 ValueLabelSize = Text.CalcSize(ValueLabel);
                        ValueLabelRect = new Rect(new Vector2(5f, CurrentY), ValueLabelSize);
                        CurrentY += ValueLabelSize.y;
                    }
                    BoxRect = new Rect(0f, ModeLabelRect.yMin, MainInfoSub.width, ModeLabelHeight + ValueLabelRect.height);
                    Widgets.DrawBoxSolid(BoxRect, new ColorInt(33, 33, 33).ToColor);
                    Widgets.Label(ModeLabelRect, ModeLabel);
                    Widgets.Label(ValueLabelRect, ValueLabel);
                    TryMakeTextList(MainInfoSub, ref CurrentY, "", factionCounts, false);
                }
                else
                {
                    TryMakeTextList(MainInfoSub, ref CurrentY, ModeLabel, factionCounts, false);
                }
                AddGap(MainInfoSub, ref CurrentY);
            }
            //ThingSettings 
            ThingSettings thingSettings = objective.def.targetSettings?.thingSettings;
            if (thingSettings != null)
            {
                string ThingSettingLabel = ResolveThingSettings();
                float ThingSettingLabelHeight = Text.CalcHeight(ThingSettingLabel, width);
                Rect ThingSettingLabelRect = new Rect(5f, CurrentY, width, ThingSettingLabelHeight);
                CurrentY += ThingSettingLabelRect.height;

                string stuff = thingSettings.stuff?.LabelCap ?? "AnyStuff_SMO".Translate();
                string StuffLabel = "ThingSettingsStuff_SMO".Translate(stuff);
                Vector2 StuffLabelSize = Text.CalcSize(StuffLabel);
                Rect StuffLabelRect = new Rect(new Vector2(5f, CurrentY), StuffLabelSize);

                string qual = thingSettings.minQuality?.GetLabel() ?? "Any_SMO".Translate();
                string QualityLabel = "ThingSettingsQuality_SMO".Translate(qual);
                Vector2 QualityLabelSize = Text.CalcSize(QualityLabel);
                Rect QualityLabelRect = new Rect(new Vector2((width) * 0.5f, CurrentY), QualityLabelSize);
                CurrentY += QualityLabelRect.height;
                Rect TSRect = new Rect(0f, ThingSettingLabelRect.yMin, width, ThingSettingLabelHeight + QualityLabelSize.y);
                Widgets.DrawBoxSolid(TSRect, new ColorInt(33, 33, 33).ToColor);
                Widgets.Label(ThingSettingLabelRect, ThingSettingLabel);
                Widgets.Label(StuffLabelRect, StuffLabel);
                Widgets.Label(QualityLabelRect, QualityLabel);

                AddGap(MainInfoSub, ref CurrentY);
            }

            //PawnSettings
            PawnSettings pawnSettings = objective.def.targetSettings?.pawnSettings;
            if(pawnSettings != null)
            {
                bool humanlike = pawnSettings.def?.race.Humanlike ?? false;
                string PawnDefLabel = ResolvePawnSettings((pawnSettings.def?.LabelCap ?? "AnyPawn_SMO".Translate()) + " (x" + pawnSettings.minAmount + ")");
                float PawnDefLabelHeight = Text.CalcHeight(PawnDefLabel, width);
                Rect PawnDefLabelRect = new Rect(5f, CurrentY, width, PawnDefLabelHeight);
                CurrentY += PawnDefLabelHeight;

                string PawnKindDefLabel = "PawnSettingsKind_SMO".Translate(pawnSettings.kindDef?.LabelCap ?? "N/A");
                Vector2 PawnKindDefLabelSize = Text.CalcSize(PawnKindDefLabel);
                Rect PawnKindDefLabelRect = new Rect(new Vector2 (5f,CurrentY), PawnKindDefLabelSize);

                string FactionLabel = "PawnSettingsFaction_SMO".Translate(pawnSettings.factionDef?.LabelCap ?? "N/A");
                Vector2 FactionLabelSize = Text.CalcSize(FactionLabel);
                Rect FactionLabelRect = new Rect(new Vector2(MainInfoSub.center.x,CurrentY),FactionLabelSize);
                CurrentY += PawnKindDefLabelSize.y;

                string GenderLabel = "PawnSettingsGender_SMO".Translate(pawnSettings.gender?.GetLabel(!humanlike) ?? "N/A");
                Vector2 GenderLabelSize = Text.CalcSize(GenderLabel);
                Rect GenderLabelRect = new Rect(new Vector2(5f,CurrentY), GenderLabelSize);
                CurrentY += GenderLabelSize.y;

                Rect PawnSettingRect = new Rect(0f, PawnDefLabelRect.yMin, width, PawnDefLabelHeight + PawnKindDefLabelSize.y + GenderLabelSize.y);
                Widgets.DrawBoxSolid(PawnSettingRect, new ColorInt(33, 33, 33).ToColor);
                Widgets.Label(PawnDefLabelRect, PawnDefLabel);
                Widgets.Label(PawnKindDefLabelRect, PawnKindDefLabel);
                Widgets.Label(FactionLabelRect, FactionLabel);
                Widgets.Label(GenderLabelRect, GenderLabel);

                AddGap(MainInfoSub, ref CurrentY);
            }

            //Stations
            if (NeedsStation)
            {
                List<string> stations = new List<string>();
                foreach(ThingValue tv in objective.def.targetSettings?.targets)
                {
                    stations.Add(tv.ThingDef.LabelCap);
                }
                TryMakeTextList(MainInfoSub, ref CurrentY, "Stations_SMO".Translate(objective.def.BestPotentialStation.LabelCap), stations, true);
            }
            if (!objective.def.skillRequirements.NullOrEmpty())
            {
                List<string> skills = new List<string>();
                foreach (SkillRequirement sr in objective.def.skillRequirements)
                {
                    skills.Add(sr.Summary);
                }
                TryMakeTextList(MainInfoSub, ref CurrentY, "SkillRequirementsInfo_SMO".Translate(), skills, true);
            }

            float RestHeight = MainInfoBody.ContractedBy(5f).height - CurrentY;
            float ContainerSplit = NeedsStation ? RestHeight * 0.5f : RestHeight / 3f;
            //Targets
            if (objective.def.targetSettings != null && objective.def.targetSettings.targets.Count > 0 && !NeedsStation)
            {
                float TotalHeight = objective.def.targetSettings.targets.Count * 34f;
                float ContainerHeight = TotalHeight > ContainerSplit ? ContainerSplit : TotalHeight;
                Rect ContainerRect = new Rect(0f, CurrentY, MainInfoSub.width, ContainerHeight);
                string TargetDesc = ResolveTargetText();
                if (!TargetDesc.NullOrEmpty())
                {
                    float TargetDescHeight = Text.CalcHeight(TargetDesc, MainInfoSub.width - 5f);
                    Rect BoxRect = new Rect(0f, CurrentY, MainInfoSub.width, TargetDescHeight + 10f);
                    Rect TargetDescRect = BoxRect.ContractedBy(5f);
                    ContainerRect.y += BoxRect.height + 5f;
                    if (TotalHeight > ContainerSplit)
                    {
                        ContainerRect.height -= BoxRect.height + 5f;
                    }
                    Widgets.DrawBoxSolid(BoxRect, new ColorInt(33, 33, 33).ToColor);
                    Widgets.Label(TargetDescRect, TargetDesc);
                }
                StoryUtils.DrawMenuSectionColor(ContainerRect, 1, new ColorInt(40, 40, 40), StoryMats.defaultBorder);
                GUI.BeginGroup(ContainerRect.ContractedBy(1));
                Widgets.BeginScrollView(new Rect(0f, 0f, ContainerRect.width, ContainerRect.height), ref TargetPos, new Rect(0f, 0f, ContainerRect.width, TotalHeight), false);
                float CurY = 5f;
                List<ThingValue> targets = objective.def.targetSettings.targets;
                for (int i = 0; i < targets.Count; i++)
                {
                    ThingValue thingValue = targets[i];
                    ThingTracker tracker = objective.thingTracker;
                    if (i % 2 == 0)
                    {
                        GUI.color = new Color(1f, 1f, 1f, 0.5f);
                        Widgets.DrawHighlight(new Rect(0f, CurY - 5f, ContainerRect.width, 34f));
                        GUI.color = Color.white;
                    }
                    string label = tracker.WorksWithPawns ? thingValue.PawnKindDef.LabelCap : thingValue.ThingDef.LabelCap;
                    WidgetRow itemRow = new WidgetRow(5f, CurY);
                    WidgetRow infoRect = new WidgetRow(ContainerRect.width * 0.5f, CurY);
                    itemRow.Gap(10f);
                    GUI.color = ResolveColor(thingValue);
                    itemRow.Icon(thingValue.ThingDef.uiIcon);
                    GUI.color = Color.white;
                    itemRow.Label(label);
                    if (tracker.TargetsDone.TryGetValue(thingValue, out int value))
                    {
                        infoRect.Label(value.ToString() + "/" + thingValue.value);
                    }
                    CurY += 34f;
                }
                CurrentY += ContainerRect.height;
                Widgets.EndScrollView();
                GUI.EndGroup();
            }

            //Rewards
            float RewardContainerHeight = ContainerSplit - 46f;
            List<IncidentProperties> props = new List<IncidentProperties>(objective.def.incidentsOnCompletion);
            props.Add(objective.def.result);
            if (!props.NullOrEmpty() && props.Any(p => !p?.spawnSettings?.spawnList.NullOrEmpty() ?? false))
            {
                float TotalHeight = props.Sum(s => s.spawnSettings.spawnList.Count) * 34f;
                float height = TotalHeight > ContainerSplit ? ContainerSplit : TotalHeight;
                Rect CompletedRewardsRect = new Rect(0f, CurrentY, MainInfoSub.width, height);
                CurrentY += CompletedRewardsRect.height;
                DrawRewardContainer(CompletedRewardsRect, "CompletedRewards_SMO".Translate(), ref RewardCompletedPos, props);
            }
            props = objective.def.incidentsOnFail;
            if (!props.NullOrEmpty() && props.Any(p => !p?.spawnSettings?.spawnList.NullOrEmpty() ?? false))
            {
                float total = props.Sum(s => s.spawnSettings.spawnList.Count) * 34f;
                float height = total > ContainerSplit ? ContainerSplit : total;
                Rect FailedRewardsRect = new Rect(0f, CurrentY, MainInfoSub.width, height);
                CurrentY += FailedRewardsRect.height;
                DrawRewardContainer(FailedRewardsRect, "FailedRewards_SMO".Translate(), ref RewardFailedPos, props);
            }
            GUI.EndGroup();
        }

        private bool NeedsStation
        {
            get
            {
                ObjectiveType OType = objective.def.type;
                return OType == ObjectiveType.Research;
            }
        }

        private void DrawRewardContainer(Rect rect, string label, ref Vector2 pos, List<IncidentProperties> properties)
        {
            Vector2 LabelSize = Text.CalcSize(label);
            Rect LabelRect = new Rect(new Vector2(5f, rect.y), LabelSize);
            Widgets.Label(LabelRect, label);

            rect = new Rect(rect.x, rect.y + 22f, rect.width, rect.height);
            StoryUtils.DrawMenuSectionColor(new Rect(rect.x, rect.y + 24f, rect.width, rect.height), 1, new ColorInt(40, 40, 40), StoryMats.defaultBorder);

            List<ThingValue> things = ThingsFromIncidents(properties);
            float TotalHeight = things.Count * 34f;
            Rect UpperTab = new Rect(rect.x, rect.y, rect.width, 24f);
            StoryUtils.DrawMenuSectionColor(UpperTab, 1, new ColorInt(40, 40, 40), StoryMats.defaultBorder);
            float tenth = UpperTab.width * 0.1f;

            float itemPos = UpperTab.x += 5f;
            Widgets.Label(UpperTab, "Item_SMO".Translate());
            float ChancePos = UpperTab.x += tenth * 8.5f;
            Widgets.Label(UpperTab, "Chance_SMO".Translate());
            Rect ContainerRect = new Rect(rect.x, UpperTab.yMax, rect.width, rect.height+2f).ContractedBy(1f);
            GUI.BeginGroup(ContainerRect);
            Widgets.BeginScrollView(new Rect(0f, 0f, ContainerRect.width, ContainerRect.height), ref pos, new Rect(0f, 0f, ContainerRect.width, TotalHeight), false);
            float CurY = 5f;
            for (int i = 0; i < things.Count; i++)
            {
                if (i % 2 == 0)
                {
                    GUI.color = new Color(1f, 1f, 1f, 0.5f);
                    Widgets.DrawHighlight(new Rect(0f, CurY - 5f, ContainerRect.width, 34f));
                    GUI.color = Color.white;
                }
                ThingValue thingValue = things[i];
                ThingDef thingDef = thingValue.ThingDef;
                WidgetRow itemRow = new WidgetRow(itemPos, CurY);
                GUI.color = ResolveColor(thingValue);
                itemRow.Icon(thingValue.ThingDef.uiIcon);
                GUI.color = Color.white;
                string qualityLabel = !thingDef.CountAsResource ? " (" + thingValue.QualityCategory.GetLabel() + ")" : "";
                float LabelY = itemRow.Label((thingValue.ResolvedStuff == null ? "" : thingValue.ResolvedStuff.LabelCap + " ") + thingDef.LabelCap + qualityLabel + " (x" + thingValue.value + ")").y;
                itemRow.Gap(ChancePos - itemRow.FinalX);
                itemRow.Label(thingValue.chance.ToStringPercent());
                CurY += 34f;
            }
            Widgets.EndScrollView();
            GUI.EndGroup();
        }

        private void TryMakeTextList(Rect rect, ref float curY, string main, List<string> list, bool addGap)
        {
            if (addGap)
            {
                AddGap(rect, ref curY);
            }
            string column1 = "",
                   column2 = "";
            int s1 = 0,
                s2 = 0;
            for (int i = 0; i < list.Count; i++)
            {
                if (i % 2 == 0)
                {
                    column1 += "    - " + list[i] + (list.ElementAtOrDefault(i+2) == null ? "" : "\n");
                    s1++;
                }
                else
                {
                    column2 +=  "- " + list[i] + (list.ElementAtOrDefault(i + 2) == null ? "" : "\n");
                    s2++;
                }
            }
            float height  = !main.NullOrEmpty() ? Text.CalcHeight(main, rect.width - 10f) : 0f,
                  height2 = Text.CalcHeight(s1 == Math.Max(s1, s2) ? column1 : column2, rect.width - 10f);
            rect = new Rect(0f, curY, rect.width, height + height2);
            Rect lower = new Rect(0f, curY + height, rect.width, height2);
            Widgets.DrawBoxSolid(rect, new ColorInt(33, 33, 33).ToColor);
            Widgets.Label(new Rect(5f, rect.y, rect.width, rect.height), main);
            Widgets.Label(lower.LeftHalf(), column1);
            Widgets.Label(lower.RightHalf(), column2);
            curY += rect.height;
        }

        private void AddGap(Rect rect, ref float YPos)
        {
            GUI.color = new Color(0.35f, 0.35f, 0.35f);
            Widgets.DrawLineHorizontal(0f, YPos + (gap * 0.5f), rect.width);
            GUI.color = Color.white;
            YPos += gap;
        }
      
        private Color ResolveColor(ThingValue thingValue)
        {          
            if (thingValue.ResolvedStuff != null)
            {
                return thingValue.ResolvedStuff.stuffProps.color;
            }
            if (thingValue.IsPawnKindDef)
            {
                PawnKindDef pdef = thingValue.PawnKindDef;
                if (pdef.lifeStages.Last().bodyGraphicData.color != Color.white)
                {
                    return thingValue.ThingDef.race.leatherDef.graphicData.color;
                }
            }
            return Color.white;
        }

        private string ResolvePawnSettings(params NamedArgument[] args)
        {
            string translate = "PawnSettings" + objective.def.type + "_SMO";
            return translate.Translate(args);
        }

        private string ResolveThingSettings(params NamedArgument[] args)
        {
            string translate = "ThingSettings" + objective.def.type + "_SMO";
            return translate.Translate(args);
        }

        private string ResolveTargetText()
        {
            bool custom = !objective.def.customSettings.targetLabel.NullOrEmpty();
            string any = objective.def.targetSettings?.any ?? false ? "Any" : "";
            string translate = objective.def.type.ToString() + any + "_SMO";
            return custom ? objective.def.customSettings.targetLabel : translate.Translate();
        }

        private float MiddleOffset(float RectWidth, float SizeWidth)
        {
            return (RectWidth * 0.5f) - (SizeWidth * 0.5f);
        }

        private List<ThingValue> ThingsFromIncidents(List<IncidentProperties> props)
        {
            List<ThingValue> Things = new List<ThingValue>();
            foreach (IncidentProperties prop in props)
            {
                Things.AddRange(prop?.spawnSettings?.spawnList);
            }
            return Things;
        }
    }
}
