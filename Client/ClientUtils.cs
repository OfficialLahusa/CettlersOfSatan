﻿using SFML.Graphics;
using SFML.System;
using System.Numerics;

namespace Client
{
    public static class ClientUtils
    {
        public static Vector2f RoundVec2f(Vector2f value)
        {
            return new Vector2f((float)Math.Round(value.X), (float)Math.Round(value.Y));
        }

        public static Vector2f EulerAngleToVec2f(float angle)
        {
            return new Vector2f(MathF.Cos(angle / 180.0f * MathF.PI), MathF.Sin(angle / 180.0f * MathF.PI));
        }
    }
}
