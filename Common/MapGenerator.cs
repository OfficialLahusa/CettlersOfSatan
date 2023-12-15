using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class MapGenerator
    {
        public static HexMap<Tile> GenerateRandomClassic()
        {
            HexMap<Tile> map = new HexMap<Tile>(7, 7, Tile.Empty);

            List<TileType> types = new List<TileType>(){
                TileType.Brick, TileType.Brick, TileType.Brick,
                TileType.Lumber, TileType.Lumber, TileType.Lumber, TileType.Lumber,
                TileType.Ore, TileType.Ore, TileType.Ore,
                TileType.Grain, TileType.Grain, TileType.Grain, TileType.Grain,
                TileType.Wool, TileType.Wool, TileType.Wool, TileType.Wool,
                TileType.Desert
            };
            List<int> numbers = new List<int>()
            {
                2, 3, 3, 4, 4, 5, 5, 6, 6, 8, 8, 9, 9, 10, 10, 11, 11, 12
            };
            Utils.Shuffle(types);
            Utils.Shuffle(numbers);

            for (int y = 0; y < map.Height; y++)
            {
                for (int x = 0; x < map.Width; x++)
                {
                    int dist = HexMap<Tile>.Distance(x, y, 3, 3);
                    if (dist < 3)
                    {
                        TileType type = types[0];
                        types.RemoveAt(0);

                        int? number = null;
                        if (type != TileType.Desert)
                        {
                            number = numbers[0];
                            numbers.RemoveAt(0);
                        }

                        map.SetTile(x, y, new Tile(type, number));
                    }
                    else if (dist < 4) map.SetTile(x, y, new Tile(TileType.Water, null));
                }
            }

            return map;
        }
    }
}
