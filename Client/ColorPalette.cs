using Common;
using SFML.Graphics;
using SFML.System;
using System.Numerics;
using System.Runtime.InteropServices;
using static Common.Direction;
using static Common.Tile;
using Tile = Common.Tile;

namespace Client
{
    public static class ColorPalette
    {
        public static readonly Color WaterTileBlue = new Color(0x2d, 0x64, 0x9d);
        public static readonly Color LumberTileGreen = new Color(0x44, 0x92, 0x47);
        public static readonly Color BrickTileRed = new Color(0xd1, 0x70, 0x40);
        public static readonly Color WoolTileGreen = new Color(0x96, 0xb1, 0x41);
        public static readonly Color GrainTileYellow = new Color(0xe9, 0xbb, 0x4e);
        public static readonly Color OreTileGray = new Color(0xa5, 0xaa, 0xa7);
        public static readonly Color DesertTileBeige = new Color(0xd6, 0xcf, 0x9d);

        public static readonly Color TileBorderBeige = new Color(0xd5, 0xbe, 0x84);

        public static readonly Color DevelopmentCardPurple = new Color(0x88, 0x49, 0xa0);
        public static readonly Color DevelopmentCardDarkPurple = new Color(0x44, 0x25, 0x50);

        public static readonly Color PlayerFourOrange = new Color(255, 165, 0);

        public static Color GetPlayerColor(int idx)
        {
            return idx switch
            {
                0 => Color.Blue,
                1 => Color.Red,
                2 => Color.Green,
                3 => PlayerFourOrange,
                _ => throw new ArgumentOutOfRangeException("Invalid player index provided")
            };
        }

        public static Color GetTileColor(TileType type)
        {
            return type switch
            {
                // Playable tiles
                TileType.Water => WaterTileBlue,
                TileType.Lumber => LumberTileGreen,
                TileType.Brick => BrickTileRed,
                TileType.Wool => WoolTileGreen,
                TileType.Grain => GrainTileYellow,
                TileType.Ore => OreTileGray,
                TileType.Desert => DesertTileBeige,

                // non-playable => transparent
                _ => Color.Transparent
            };
        }

        public static Color GetTileIconColor(TileType type)
        {
            Color color = GetTileColor(type);

            // Grain/Brick: Increase primary intensity
            if (type == TileType.Grain || type == TileType.Brick)
            {
                byte max = Math.Max(color.R, Math.Max(color.G, color.B));
                float fac = 255.0f / max * ((type == TileType.Grain) ? 0.8f : 0.7f);
                color *= new Color((byte)(color.R * fac), (byte)(color.G * fac), (byte)(color.B * fac));
            }
            // Wool: Static light gray/white shade
            else if (type == TileType.Wool)
            {
                color = new Color(230, 230, 230);
            }
            // Ore/Lumber/Desert: Reduce color intensity by 50%
            else
            {
                color *= new Color(128, 128, 128);
            }

            return color;
        }

        public static Color GetPortIconColor(Port.TradeType type)
        {
            return GetTileIconColor(type switch
            {
                Port.TradeType.Generic => TileType.Wool,
                Port.TradeType.Lumber => TileType.Lumber,
                Port.TradeType.Brick => TileType.Brick,
                Port.TradeType.Wool => TileType.Wool,
                Port.TradeType.Grain => TileType.Grain,
                Port.TradeType.Ore => TileType.Ore,
                _ => throw new InvalidOperationException(),
            });
        }

        public static Color? GetTileBorderColor(Tile tile)
        {
            return tile.Type switch
            {
                // transparent for water/non-playable
                TileType.Water or TileType.NonPlayable => null,

                // For land tiles
                _ => TileBorderBeige
            };
        }

        public static Color GetCardColor(ResourceCardType type)
        {
            return type switch
            {
                ResourceCardType.Unknown => DevelopmentCardPurple,
                ResourceCardType.Lumber => GetTileColor(TileType.Lumber),
                ResourceCardType.Brick => GetTileColor(TileType.Brick),
                ResourceCardType.Wool => GetTileColor(TileType.Wool),
                ResourceCardType.Grain => GetTileColor(TileType.Grain),
                ResourceCardType.Ore => GetTileColor(TileType.Ore),
                _ => throw new InvalidOperationException()
            };
        }

        public static Color GetCardColor(DevelopmentCardType type)
        {
            return DevelopmentCardPurple;
        }

        public static Color GetCardIconColor(ResourceCardType type)
        {
            return type switch
            {
                ResourceCardType.Unknown => DevelopmentCardDarkPurple,
                ResourceCardType.Lumber => GetTileIconColor(TileType.Lumber),
                ResourceCardType.Brick => GetTileIconColor(TileType.Brick),
                ResourceCardType.Wool => GetTileIconColor(TileType.Wool),
                ResourceCardType.Grain => GetTileIconColor(TileType.Grain),
                ResourceCardType.Ore => GetTileIconColor(TileType.Ore),
                _ => throw new InvalidOperationException()
            };
        }

        public static Color GetCardIconColor(DevelopmentCardType type)
        {
            return DevelopmentCardDarkPurple;
        }

        public static Color Vec3ToColor(Vector3 color)
        {
            return new Color((byte)(255 * color.X), (byte)(255 * color.Y), (byte)(255 * color.Z));
        }

        public static Vector4 ColorToVec4(Color color)
        {
            return new Vector4(color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f);
        }
    }
}
