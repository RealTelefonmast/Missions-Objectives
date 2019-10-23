using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using Verse;
using System;
using System.Text.RegularExpressions;
using RimWorld;

namespace StoryFramework
{
    public static class StoryUtils
    {
        //Randomizers
        public static float Range(FloatRange range)
        {
            return Range(range.min, range.max);
        }

        public static float Range(float min, float max)
        {
            Rand.PushState();
            if (max <= min)
            {
                return min;
            }
            float result = Rand.Value * (max - min) + min;
            Rand.PopState();
            return result;
        }

        public static int RangeInclusive(int min, int max)
        {
            if (max <= min)
            {
                return min;
            }
            return Range(min, max + 1);
        }

        public static int Range(IntRange range)
        {
            return Range(range.min, range.max);
        }

        public static int Range(int min, int max)
        {
            Rand.PushState();
            if (max <= min)
            {
                return min;
            }
            int result = min + Mathf.Abs(Rand.Int % (max - min));
            Rand.PopState();
            return result;
        }

        public static float Value
        {
            get
            {
                Rand.PushState();
                float value = Rand.Value;
                Rand.PopState();
                return value;
            }
        }

        public static bool Chance(float f)
        {
            Rand.PushState();
            bool result = Rand.Chance(f);
            Rand.PopState();
            return result;
        }

        //
    }
}
