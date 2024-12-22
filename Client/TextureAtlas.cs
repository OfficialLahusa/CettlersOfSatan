using Common;
using SFML.Graphics;
using static Common.CardSet;
using static Common.Direction;
using static Common.Tile;

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

        public static Sprite GetSprite(CardType type)
        {
            return type switch
            {
                CardType.Unknown => Sprite.QuestionMark,

                // Resource
                CardType.Lumber => Sprite.Lumber,
                CardType.Brick => Sprite.Brick,
                CardType.Wool => Sprite.Wool,
                CardType.Grain => Sprite.Grain,
                CardType.Ore => Sprite.Ore,

                // Development
                CardType.Knight => Sprite.Knight,
                CardType.RoadBuilding => Sprite.Road,
                CardType.YearOfPlenty => Sprite.YearOfPlenty,
                CardType.Monopoly => Sprite.Monopoly,
                CardType.VictoryPoint => Sprite.VictoryPoint,

                _ => throw new InvalidOperationException()
            };
        }
    }
}
