using SFML.Graphics;
using SFML.System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    public static class ClientUtils
    {
        public static Color Vec3ToColor(System.Numerics.Vector3 color)
        {
            return new Color((byte)(255 * color.X), (byte)(255 * color.Y), (byte)(255 * color.Z));
        }

        public static Vector2f RoundVec2f(Vector2f value)
        {
            return new Vector2f((float)Math.Round(value.X), (float)Math.Round(value.Y));
        }
    }
}
