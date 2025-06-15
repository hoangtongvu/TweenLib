using Unity.Mathematics;

namespace TweenLib
{
    public enum EasingType
    {
        None,
        Linear,

        EaseInSine,
        EaseOutSine,
        EaseInOutSine,

        EaseInQuad,
        EaseOutQuad,
        EaseInOutQuad,

        EaseInCubic,
        EaseOutCubic,
        EaseInOutCubic,

        EaseInQuart,
        EaseOutQuart,
        EaseInOutQuart,

        EaseInQuint,
        EaseOutQuint,
        EaseInOutQuint,

        EaseInExpo,
        EaseOutExpo,
        EaseInOutExpo,

        EaseInCirc,
        EaseOutCirc,
        EaseInOutCirc,

        EaseInBack,
        EaseOutBack,
        EaseInOutBack,

        EaseInElastic,
        EaseOutElastic,
        EaseInOutElastic,

        EaseInBounce,
        EaseOutBounce,
        EaseInOutBounce,
    }

    // Credit goes to https://easings.net/
    // Credit goes to https://github.com/PhilSA/Trove/blob/main/com.trove.tweens/Runtime/EasingUtilities.cs
    public static class EasingUtilities
    {
        const float c1 = 1.70158f;
        const float c2 = c1 * 1.525f;
        const float c3 = c1 + 1f;
        const float c4 = (2f * math.PI) / 3f;
        const float c5 = (2f * math.PI) / 4.5f;
        const float n1 = 7.5625f;
        const float d1 = 2.75f;

        public static float CalculateEasing(float x, EasingType type)
        {
            return type switch
            {
                EasingType.None => 0f,
                EasingType.Linear => EaseLinear(x),
                EasingType.EaseInSine => EaseInSine(x),
                EasingType.EaseOutSine => EaseOutSine(x),
                EasingType.EaseInOutSine => EaseInOutSine(x),
                EasingType.EaseInQuad => EaseInQuad(x),
                EasingType.EaseOutQuad => EaseOutQuad(x),
                EasingType.EaseInOutQuad => EaseInOutQuad(x),
                EasingType.EaseInCubic => EaseInCubic(x),
                EasingType.EaseOutCubic => EaseOutCubic(x),
                EasingType.EaseInOutCubic => EaseInOutCubic(x),
                EasingType.EaseInQuart => EaseInQuart(x),
                EasingType.EaseOutQuart => EaseOutQuart(x),
                EasingType.EaseInOutQuart => EaseInOutQuart(x),
                EasingType.EaseInQuint => EaseInQuint(x),
                EasingType.EaseOutQuint => EaseOutQuint(x),
                EasingType.EaseInOutQuint => EaseInOutQuint(x),
                EasingType.EaseInExpo => EaseInExpo(x),
                EasingType.EaseOutExpo => EaseOutExpo(x),
                EasingType.EaseInOutExpo => EaseInOutExpo(x),
                EasingType.EaseInCirc => EaseInCirc(x),
                EasingType.EaseOutCirc => EaseOutCirc(x),
                EasingType.EaseInOutCirc => EaseInOutCirc(x),
                EasingType.EaseInBack => EaseInBack(x),
                EasingType.EaseOutBack => EaseOutBack(x),
                EasingType.EaseInOutBack => EaseInOutBack(x),
                EasingType.EaseInElastic => EaseInElastic(x),
                EasingType.EaseOutElastic => EaseOutElastic(x),
                EasingType.EaseInOutElastic => EaseInOutElastic(x),
                EasingType.EaseInBounce => EaseInBounce(x),
                EasingType.EaseOutBounce => EaseOutBounce(x),
                EasingType.EaseInOutBounce => EaseInOutBounce(x),
                _ => 0f,
            };
        }

        public static float EaseLinear(float x) => x;

        public static float EaseInSine(float x) => 1f - math.cos((x * math.PI) / 2f);
        public static float EaseOutSine(float x) => math.sin((x * math.PI) / 2f);
        public static float EaseInOutSine(float x) => -(math.cos(x * math.PI) - 1f) / 2f;

        public static float EaseInQuad(float x) => x * x;
        public static float EaseOutQuad(float x) => 1f - (1f - x) * (1f - x);
        public static float EaseInOutQuad(float x)
        {
            return math.select(1f - math.pow((-2f * x) + 2f, 2f) / 2f, 2f * x * x, x < 0.5f);
        }

        public static float EaseInCubic(float x) => x * x * x;
        public static float EaseOutCubic(float x) => 1f - math.pow(1f - x, 3f);
        public static float EaseInOutCubic(float x)
        {
            return math.select(1f - math.pow((-2f * x) + 2f, 3f) / 2f, 4f * x * x * x, x < 0.5f);
        }

        public static float EaseInQuart(float x) => x * x * x * x;
        public static float EaseOutQuart(float x) => 1f - math.pow(1f - x, 4f);
        public static float EaseInOutQuart(float x)
        {
            return math.select(1f - math.pow((-2f * x) + 2f, 4f) / 2f, 8f * x * x * x * x, x < 0.5f);
        }

        public static float EaseInQuint(float x) => x * x * x * x * x;
        public static float EaseOutQuint(float x) => 1f - math.pow(1f - x, 5f);
        public static float EaseInOutQuint(float x)
        {
            return math.select(1f - math.pow((-2f * x) + 2f, 5f) / 2f, 16f * x * x * x * x * x, x < 0.5f);
        }

        public static float EaseInExpo(float x)
        {
            return math.select(math.pow(2f, (10f * x) - 10f), 0f,x == 0f);
        }
        public static float EaseOutExpo(float x)
        {
            return math.select(1f - math.pow(2f, -10f * x), 1f, x == 1f);
        }
        public static float EaseInOutExpo(float x)
        {
            return math.select(math.select(math.select((2f - math.pow(2f, (-20f * x) + 10f)) / 2, math.pow(2f, (20f * x) - 10f) / 2f, x < 0.5f), 1f, x == 1f), 0f, x == 0f);
        }

        public static float EaseInCirc(float x)
        {
            return 1f - math.sqrt(1f - math.pow(x, 2f));
        }
        public static float EaseOutCirc(float x)
        {
            return math.sqrt(1f - math.pow(x - 1f, 2f));
        }
        public static float EaseInOutCirc(float x)
        {
            return math.select((math.sqrt(1f - math.pow((-2f * x) + 2f, 2f)) + 1f) / 2f, (1f - math.sqrt(1f - math.pow(2f * x, 2f))) / 2f, x < 0.5f);
        }

        public static float EaseInBack(float x)
        {
            return (c3 * x * x * x) - (c1 * x * x);
        }
        public static float EaseOutBack(float x)
        {
            return 1f + (c3 * math.pow(x - 1f, 3f)) + (c1 * math.pow(x - 1f, 2f));
        }
        public static float EaseInOutBack(float x)
        {
            return math.select((math.pow((2f * x) - 2f, 2f) * ((c2 + 1f) * ((x * 2f) - 2f) + c2) + 2f) / 2f, (math.pow(2f * x, 2f) * (((c2 + 1f) * 2f * x) - c2)) / 2f, x < 0.5f);
        }

        public static float EaseInElastic(float x)
        {
            return math.select(math.select(-math.pow(2f, (10f * x) - 10f) * math.sin(((x * 10f) - 10.75f) * c4), 1f, x == 1f), 0f, x == 0f);
        }
        public static float EaseOutElastic(float x)
        {
            return math.select(math.select(math.pow(2f, -10f * x) * math.sin(((x * 10f) - 0.75f) * c4) + 1f, 1f, x == 1f), 0f, x == 0f);
        }
        public static float EaseInOutElastic(float x)
        {
            return math.select(math.select(math.select(((math.pow(2f, (-20f * x) + 10f) * math.sin(((20f * x) - 11.125f) * c5)) / 2f) + 1f, -((math.pow(2f, (20f * x) - 10f) * math.sin(((20f * x) - 11.125f) * c5)) / 2f), x < 0.5f), 1f, x == 1f), 0f, x == 0f);
        }

        public static float EaseInBounce(float x)
        {
            return 1f - EaseOutBounce(1f - x);
        }
        public static float EaseOutBounce(float x)
        {
            if (x < (1f / d1))
            {
                return n1 * x * x;
            }
            else if (x < (2f / d1))
            {
                return (n1 * (x -= (1.5f / d1)) * x) + 0.75f;
            }
            else if (x < (2.5f / d1))
            {
                return (n1 * (x -= (2.25f / d1)) * x) + 0.9375f;
            }
            else
            {
                return (n1 * (x -= (2.625f / d1)) * x) + 0.984375f;
            }
        }
        public static float EaseInOutBounce(float x)
        {
            return math.select((1f + EaseOutBounce((2f * x) - 1f)) / 2f, (1f - EaseOutBounce(1f - (2f * x))) / 2f, x < 0.5f);
        }
    }
}