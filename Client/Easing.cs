using SFML.Graphics;

namespace Client
{
    // https://easings.net/
    public static class Easing
    {
        public static float ExpOut(float x)
        {
            return x >= 1f ? 1f : 1f - MathF.Pow(2f, -10f * x);
        }
    }
}
