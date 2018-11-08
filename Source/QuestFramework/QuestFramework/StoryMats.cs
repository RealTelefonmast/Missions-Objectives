using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace StoryFramework
{
    [StaticConstructorOnStartup]
    public static class StoryMats
    {
        public static readonly Texture2D info2 = ContentFinder<Texture2D>.Get("UI/Buttons/InfoButton", true);
        public static readonly Texture2D arrow = ContentFinder<Texture2D>.Get("UI/Arrow");

        //Colors
        public static ColorInt defaultFill = new ColorInt(45, 45, 45);
        public static ColorInt defaultBorder = new ColorInt(135, 135, 135);
        public static readonly Texture2D clear = SolidColorMaterials.NewSolidColorTexture(Color.clear);
        public static readonly Texture2D grey = SolidColorMaterials.NewSolidColorTexture(Color.grey);
        public static readonly Texture2D blue = SolidColorMaterials.NewSolidColorTexture(new ColorInt(40, 170, 225).ToColor);
        public static readonly Texture2D yellow = SolidColorMaterials.NewSolidColorTexture(new ColorInt(250, 240, 50).ToColor);
        public static readonly Texture2D orange = SolidColorMaterials.NewSolidColorTexture(new ColorInt(255, 155, 0).ToColor);
        public static readonly Texture2D red = SolidColorMaterials.NewSolidColorTexture(new ColorInt(190, 30, 45).ToColor);
        public static readonly Texture2D purple = SolidColorMaterials.NewSolidColorTexture(new ColorInt(171, 140, 192).ToColor);
        public static readonly Texture2D green = SolidColorMaterials.NewSolidColorTexture(new ColorInt(45, 180, 115).ToColor);
        public static readonly Texture2D white = SolidColorMaterials.NewSolidColorTexture(new ColorInt(255, 255, 255).ToColor);
        public static readonly Texture2D black = SolidColorMaterials.NewSolidColorTexture(new ColorInt(15, 10, 10).ToColor);
        
    }
}
