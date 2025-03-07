using Microsoft.VisualBasic;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows.Markup;

namespace Common
{
    public enum ResourceCardType : byte
    {
        Unknown = 0,
        Lumber = 1,
        Brick = 2,
        Wool = 3,
        Grain = 4,
        Ore = 5
    };
    public enum DevelopmentCardType : byte
    {
        Unknown = 0,
        Knight = 1,
        RoadBuilding = 2,
        YearOfPlenty = 3,
        Monopoly = 4,
        VictoryPoint = 5
    };

    public class CardSet<T> where T : Enum
    {
        // Card count indexed by type
        private uint[] _cards;

        public static readonly IReadOnlyList<T> Values;

        static CardSet() {
            Values = (IReadOnlyList<T>)Enum.GetValues(typeof(T));
        }

        public CardSet()
        {
            _cards = new uint[Enum.GetValues(typeof(T)).Length];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ToInt(T val)
        {
            // Performance comparison https://stackoverflow.com/questions/16960555
            //return Convert.ToInt32(Convert.ChangeType(val, val.GetTypeCode()));
            //return (byte)(ValueType)val;
            //return Unsafe.As<T, byte>(ref val);
            return val.GetHashCode();
        }

        public T Draw(bool consume = true)
        {
            // Weighted random draw
            uint totalWeight = Count();

            // No cards in set
            if(totalWeight == 0)
            {
                throw new InvalidOperationException();
            }

            uint draw = (uint)Utils.Random.Next((int)totalWeight);

            foreach(T type in Values)
            {
                uint typeWeight = _cards[ToInt(type)];

                if (draw < typeWeight)
                {
                    // Remove drawn card from stock
                    if(consume)
                    {
                        _cards[ToInt(type)] = typeWeight - 1;
                    }

                    return type;
                }

                draw -= typeWeight;
            }

            throw new InvalidOperationException();
        }

        public uint Get(T type)
        {
            return _cards[ToInt(type)];
        }

        public uint Count()
        {
            uint sum = 0;

            foreach(uint amount in _cards)
            {
                sum += amount;
            }

            return sum;
        }

        public void Add(T type, uint amount)
        {
            _cards[ToInt(type)] += amount;
        }

        public void Remove(T type, uint amount)
        {
            if (!Contains(type, amount)) throw new InvalidOperationException("Removal amount exceeds available card count");

            _cards[ToInt(type)] -= amount;
        }

        public bool Contains(T type, uint amount = 1)
        {
            return _cards[ToInt(type)] >= amount;
        }

        public bool Contains(CardSet<T> subset)
        {
            foreach(T cardType in Values)
            {
                if (subset.Get(cardType) > Get(cardType)) return false;
            }

            return true;
        }

        public void Add(CardSet<T> subset)
        {
            foreach (T cardType in Values)
            {
                _cards[ToInt(cardType)] += subset.Get(cardType);
            }
        }

        public void Remove(CardSet<T> subset)
        {
            if (!Contains(subset)) throw new InvalidOperationException("Removal amount exceeds available card count");

            foreach (T cardType in Values)
            {
                _cards[ToInt(cardType)] -= subset.Get(cardType);
            }
        }

        public override bool Equals(object? obj)
        {
            return obj is CardSet<T> set
                && _cards.SequenceEqual(set._cards);
        }

        public override int GetHashCode()
        {
            HashCode hash = new HashCode();

            foreach(uint cardAmount in _cards)
            {
                hash.Add(cardAmount);
            }

            return hash.ToHashCode();
        }
    }

    public static class CardTypeExtensions
    {
        public static string GetName(this ResourceCardType type)
        {
            return type switch
            {
                ResourceCardType.Unknown => "Unknown",
                ResourceCardType.Lumber => "Lumber",
                ResourceCardType.Brick => "Brick",
                ResourceCardType.Wool => "Wool",
                ResourceCardType.Grain => "Grain",
                ResourceCardType.Ore => "Ore",
                _ => throw new InvalidOperationException(),
            };
        }

        public static string GetName(this DevelopmentCardType type)
        {
            return type switch
            {
                DevelopmentCardType.Unknown => "Unknown",
                DevelopmentCardType.Knight => "Knight",
                DevelopmentCardType.RoadBuilding => "Road Building",
                DevelopmentCardType.YearOfPlenty => "Year Of Plenty",
                DevelopmentCardType.Monopoly => "Monopoly",
                DevelopmentCardType.VictoryPoint => "Victory Point",
                _ => throw new InvalidOperationException(),
            };
        }

        public static string GetAbbreviation(this ResourceCardType type)
        {
            return type switch
            {
                ResourceCardType.Unknown => "?",
                ResourceCardType.Lumber => "L",
                ResourceCardType.Brick => "B",
                ResourceCardType.Wool => "W",
                ResourceCardType.Grain => "G",
                ResourceCardType.Ore => "O",
                _ => throw new InvalidOperationException()
            };
        }

        public static string GetAbbreviation(this DevelopmentCardType type)
        {
            return type switch
            {
                DevelopmentCardType.Unknown => "?",
                DevelopmentCardType.Knight => "K",
                DevelopmentCardType.RoadBuilding => "R",
                DevelopmentCardType.YearOfPlenty => "Y",
                DevelopmentCardType.Monopoly => "M",
                DevelopmentCardType.VictoryPoint => "V",
                _ => throw new InvalidOperationException()
            };
        }
    }
}
