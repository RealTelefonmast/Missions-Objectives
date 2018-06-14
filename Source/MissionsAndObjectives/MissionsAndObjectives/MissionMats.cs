using System;
using UnityEngine;
using Verse;
using RimWorld;

namespace MissionsAndObjectives
{
    [StaticConstructorOnStartup]
    public static class MissionMats
    {
        public static readonly Texture2D WorkBanner = ContentFinder<Texture2D>.Get(MCD.MainMissionControlDef.bannerTexture, true);

        //Colors
        public static readonly Texture2D clear = SolidColorMaterials.NewSolidColorTexture(Color.clear);

        public static readonly Texture2D grey = SolidColorMaterials.NewSolidColorTexture(Color.grey);

        public static readonly Texture2D blue = SolidColorMaterials.NewSolidColorTexture(new ColorInt(38, 169, 224).ToColor);

        public static readonly Texture2D yellow = SolidColorMaterials.NewSolidColorTexture(new ColorInt(249, 236, 49).ToColor);

        public static readonly Texture2D red = SolidColorMaterials.NewSolidColorTexture(new ColorInt(190, 30, 45).ToColor);

        public static readonly Texture2D green = SolidColorMaterials.NewSolidColorTexture(new ColorInt(41, 180, 115).ToColor);

        public static readonly Texture2D white = SolidColorMaterials.NewSolidColorTexture(new ColorInt(255, 255, 255).ToColor);

        public static readonly Texture2D black = SolidColorMaterials.NewSolidColorTexture(new ColorInt(15, 11, 12).ToColor);
    }
}
