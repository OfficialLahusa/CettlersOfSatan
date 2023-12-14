using SFML.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    public static class Util
    {
        public static Color Vec3ToColor(System.Numerics.Vector3 color)
        {
            return new Color((byte)(255 * color.X), (byte)(255 * color.Y), (byte)(255 * color.Z));
        }
    }
}
