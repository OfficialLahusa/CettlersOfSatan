using Common;
using Microsoft.VisualBasic;
using SFML.Graphics;
using static Common.Direction;
using static Common.Tile;

namespace Client
{
    public static class TextureAtlas
    {
        public static readonly Texture Texture;

        public enum Sprite : byte
        {
            None = 255,
            Lumber = 0,
            Brick = 1,
            Wool = 2,
            Grain = 3,
            Ore = 4,
            Development = 5,
            QuestionMark = 6,

            Settlement = 8,
            City = 9,
            Road = 10,
            Desert = 11,
            Port = 12,
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

        public static Sprite GetSprite(TileType type)
        {
            return type switch
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

        public static Sprite GetSprite(Port.TradeType type)
        {
            return type switch
            {
                Port.TradeType.Generic => Sprite.QuestionMark,
                Port.TradeType.Lumber => Sprite.Lumber,
                Port.TradeType.Brick => Sprite.Brick,
                Port.TradeType.Wool => Sprite.Wool,
                Port.TradeType.Grain => Sprite.Grain,
                Port.TradeType.Ore => Sprite.Ore,
                _ => throw new InvalidOperationException(),
            };
        }

        public static Sprite GetSprite(ResourceCardType type)
        {
            return type switch
            {
                ResourceCardType.Unknown => Sprite.QuestionMark,
                ResourceCardType.Lumber => Sprite.Lumber,
                ResourceCardType.Brick => Sprite.Brick,
                ResourceCardType.Wool => Sprite.Wool,
                ResourceCardType.Grain => Sprite.Grain,
                ResourceCardType.Ore => Sprite.Ore,
                _ => throw new InvalidOperationException()
            };
        }

        public static Sprite GetSprite(DevelopmentCardType type)
        {
            return type switch
            {
                DevelopmentCardType.Unknown => Sprite.QuestionMark,
                DevelopmentCardType.Knight => Sprite.Knight,
                DevelopmentCardType.RoadBuilding => Sprite.Road,
                DevelopmentCardType.YearOfPlenty => Sprite.YearOfPlenty,
                DevelopmentCardType.Monopoly => Sprite.Monopoly,
                DevelopmentCardType.VictoryPoint => Sprite.VictoryPoint,
                _ => throw new InvalidOperationException()
            };
        }
    }
}
