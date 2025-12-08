using Common.Actions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Serialization;
using Action = Common.Actions.Action;

namespace Common.Serialization
{
    public class SaveFileSerializer
    {
        public static readonly IDeserializer Deserializer;
        public static readonly ISerializer Serializer;
        static SaveFileSerializer()
        {
            List<Type> actionTypes = [
                typeof(BuyDevelopmentCardAction),
                typeof(CityAction),
                typeof(DiscardAction),
                typeof(EndTurnAction),
                typeof(FirstInitialRoadAction),
                typeof(FirstInitialSettlementAction),
                typeof(FourToOneTradeAction),
                typeof(KnightAction),
                typeof(MonopolyAction),
                typeof(RoadAction),
                typeof(RoadBuildingAction),
                typeof(RobberAction),
                typeof(RollAction),
                typeof(SecondInitialRoadAction),
                typeof(SecondInitialSettlementAction),
                typeof(SettlementAction),
                typeof(ThreeToOneTradeAction),
                typeof(TwoToOneTradeAction),
                typeof(YearOfPlentyAction)
            ];
            Dictionary<string, Type> actionTypeMapping = new Dictionary<string, Type>(StringComparer.Ordinal);
            foreach (Type actionType in actionTypes)
            {
                actionTypeMapping[actionType.Name] = actionType;
            }

            Deserializer = new DeserializerBuilder()
                    .WithTypeConverter(new AdjacencyMatrix.Converter())
                    .WithTypeConverter(new CardSet<ResourceCardType>.Converter())
                    .WithTypeConverter(new CardSet<DevelopmentCardType>.Converter())
                    .WithTypeDiscriminatingNodeDeserializer(options =>
                    {
                        options.AddKeyValueTypeDiscriminator<Action>("ActionType", actionTypeMapping);
                    })
                    .EnablePrivateConstructors()
                    .Build();

            Serializer = new SerializerBuilder()
                    .WithTypeConverter(new AdjacencyMatrix.Converter())
                    .WithTypeConverter(new CardSet<ResourceCardType>.Converter())
                    .WithTypeConverter(new CardSet<DevelopmentCardType>.Converter())
                    .EnablePrivateConstructors()
                    .Build();
        }
    }
}
