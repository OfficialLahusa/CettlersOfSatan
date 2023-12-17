using SFML.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using static Common.Tile;
using static System.Net.Mime.MediaTypeNames;

namespace Client
{
    public static class TextureAtlas
    {
        public static readonly Texture Texture;

        public enum Sprite
        {
            None = -1,
            Lumber = 0,
            Brick = 1,
            Wool = 2,
            Grain = 3,
            Ore = 4,
            Development = 5,

            House = 8,
            City = 9,
            Road = 10,
            Desert = 11,
            Harbor = 12,
            Robber = 13,
            ThumbsUp = 14,
            ThumbsDown = 15,

            Knight = 16,
            Monopoly = 17,
            YearOfPlenty = 18,
            VictoryPoint = 19,
            ArmySize = 20,
            Bank = 21,
            Checkmark = 22,
            Crossmark = 23,

            DiceOne = 24,
            DiceTwo = 25,
            DiceThree = 26,
            DiceFour = 27,
            DiceFive = 28,
            DiceSix = 29,
            Hourglass = 30,
            Skip = 31
        }

        static TextureAtlas()
        {
            Texture = new Texture(@"..\..\..\res\atlas.png");
        }

        public static IntRect GetTextureRect(this Sprite sprite)
        {
            return (sprite != Sprite.None) ? new IntRect(((int)sprite % 8) * 512, ((int)sprite / 8) * 512, 512, 512) : new IntRect(0, 0, 0, 0);
        }

        public static Sprite GetSprite(TileType tileType)
        {
            return tileType switch
            {
                TileType.Lumber => Sprite.Lumber,
                TileType.Brick => Sprite.Brick,
                TileType.Wool => Sprite.Wool,
                TileType.Grain => Sprite.Grain,
                TileType.Ore => Sprite.Ore,
                TileType.Desert => Sprite.Desert,
                _ => Sprite.None
            };
        }

        
    }
}
