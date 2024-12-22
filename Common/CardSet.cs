namespace Common
{
    public class CardSet
    {
        public enum CardType
        {
            Unknown = 0,
            // Resource
            Lumber = 1,
            Brick = 2,
            Wool = 3,
            Grain = 4,
            Ore = 5,
            // Development
            Knight = 6,
            RoadBuilding = 7,
            YearOfPlenty = 8,
            Monopoly = 9,
            VictoryPoint = 10
        };

        public static readonly CardType[] RESOURCE_CARD_TYPES = new CardType[]
        {
            CardType.Lumber,
            CardType.Brick,
            CardType.Wool,
            CardType.Grain,
            CardType.Ore
        };

        public static readonly CardType[] DEVELOPMENT_CARD_TYPES = new CardType[]
        {
            CardType.Knight,
            CardType.RoadBuilding,
            CardType.YearOfPlenty,
            CardType.Monopoly,
            CardType.VictoryPoint
        };

        // Card count indexed by type
        private uint[] _cards;

        public CardSet()
        {
            _cards = new uint[11];
        }

        public static CardSet CreateBank()
        {
            CardSet bank = new CardSet();

            bank.Add(CardType.Lumber, 19);
            bank.Add(CardType.Brick, 19);
            bank.Add(CardType.Wool, 19);
            bank.Add(CardType.Grain, 19);
            bank.Add(CardType.Ore, 19);

            bank.Add(CardType.Knight, 14);
            bank.Add(CardType.RoadBuilding, 2);
            bank.Add(CardType.YearOfPlenty, 2);
            bank.Add(CardType.Monopoly, 2);
            bank.Add(CardType.VictoryPoint, 5);

            return bank;
        }

        public static CardSet CreateSample()
        {
            CardSet bank = new CardSet();

            bank.Add(CardType.Lumber, 3);
            bank.Add(CardType.Brick, 3);
            bank.Add(CardType.Wool, 3);
            bank.Add(CardType.Grain, 3);
            bank.Add(CardType.Ore, 3);

            bank.Add(CardType.Knight, 2);
            bank.Add(CardType.RoadBuilding, 2);
            bank.Add(CardType.YearOfPlenty, 2);
            bank.Add(CardType.Monopoly, 2);
            bank.Add(CardType.VictoryPoint, 2);

            return bank;
        }

        public static string GetName(CardType type)
        {
            return type switch
            {
                CardType.Unknown => "Unknown",
                CardType.Lumber => "Lumber",
                CardType.Brick => "Brick",
                CardType.Wool => "Wool",
                CardType.Grain => "Grain",
                CardType.Ore => "Ore",
                CardType.Knight => "Knight",
                CardType.RoadBuilding => "Road Building",
                CardType.YearOfPlenty => "Year Of Plenty",
                CardType.Monopoly => "Monopoly",
                CardType.VictoryPoint => "Victory Point",
                _ => throw new InvalidOperationException(),
            };
        }

        public uint Get(CardType type)
        {
            return _cards[(int)type];
        }

        public uint GetResourceCardCount()
        {
            uint sum = 0;

            foreach(CardType type in RESOURCE_CARD_TYPES)
            {
                sum += _cards[(int)type];
            }

            return sum;
        }

        public uint GetDevelopmentCardCount()
        {
            uint sum = 0;

            foreach (CardType type in DEVELOPMENT_CARD_TYPES)
            {
                sum += _cards[(int)type];
            }

            return sum;
        }

        public void Add(CardType type, uint amount)
        {
            _cards[(int)type] += amount;
        }

        public void Remove(CardType type, uint amount)
        {
            if (!Contains(type, amount)) throw new InvalidOperationException("Removal amount exceeds available card count");

            _cards[(int)type] -= amount;
        }

        public bool Contains(CardType type, uint amount)
        {
            return _cards[(int)type] >= amount;
        }

        public void Clear()
        {
            _cards = new uint[11];
        }
    }
}
