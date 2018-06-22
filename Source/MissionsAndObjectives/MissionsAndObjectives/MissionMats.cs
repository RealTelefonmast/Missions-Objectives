using System;
using UnityEngine;
using Verse;
using RimWorld;

namespace MissionsAndObjectives
{
    [StaticConstructorOnStartup]
    public static class MissionMats
    {
        public static readonly Texture2D info = ContentFinder<Texture2D>.Get("UI/Icons/info", true);

        //Colors
        public static readonly Texture2D clear = SolidColorMaterials.NewSolidColorTexture(Color.clear);

        public static readonly Texture2D grey = SolidColorMaterials.NewSolidColorTexture(Color.grey);

        public static readonly Texture2D blue = SolidColorMaterials.NewSolidColorTexture(new ColorInt(40, 170, 225).ToColor);

        public static readonly Texture2D yellow = SolidColorMaterials.NewSolidColorTexture(new ColorInt(250, 240, 50).ToColor);

        public static readonly Texture2D orange = SolidColorMaterials.NewSolidColorTexture(new ColorInt(255, 155, 0).ToColor);

        public static readonly Texture2D red = SolidColorMaterials.NewSolidColorTexture(new ColorInt(190, 30, 45).ToColor);

        public static readonly Texture2D green = SolidColorMaterials.NewSolidColorTexture(new ColorInt(45, 180, 115).ToColor);

        public static readonly Texture2D white = SolidColorMaterials.NewSolidColorTexture(new ColorInt(255, 255, 255).ToColor);

        public static readonly Texture2D black = SolidColorMaterials.NewSolidColorTexture(new ColorInt(15, 10, 10).ToColor);
    }
}
