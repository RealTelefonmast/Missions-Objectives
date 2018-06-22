using UnityEngine;
using System.Linq;
using Verse;

namespace MissionsAndObjectives
{
    [StaticConstructorOnStartup]
    public static class MissionUtils
    {
        public static string GetModNameFromMission(MissionDef def)
        {
           return LoadedModManager.RunningMods.Where(mcp => mcp.AllDefs.Contains(def)).RandomElement().Identifier;
        }

        public static void DrawMenuSectionColor(Rect rect, int thiccness, ColorInt colorBG, ColorInt colorBorder)
        {
            GUI.color = colorBG.ToColor;
            GUI.DrawTexture(rect, BaseContent.WhiteTex);
            GUI.color = colorBorder.ToColor;
            Widgets.DrawBox(rect, thiccness);
            GUI.color = Color.white;
        }
    }
}
