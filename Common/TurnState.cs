using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class TurnState
    {
        public int PlayerIndex { get; set; }

        public enum RoundType : byte
        {
            Normal,
            FirstInitial,
            SecondInitial
        }

        public RoundType TypeOfRound { get; set; }
        public int RoundCounter;

        public bool MustRoll { get; set; }
        public int AwaitedDiscards { get; set; }
        public bool MustDiscard
        {
            get { return AwaitedDiscards > 0; }
        }
        public bool MustMoveRobber { get; set; }

        public bool HasPlayedDevelopmentCard { get; set; }


        public TurnState(int playerIdx, int roundCounter = 0)
        {
            PlayerIndex = playerIdx;

            TypeOfRound = RoundType.Normal;
            RoundCounter = roundCounter;

            MustRoll = true;
            MustMoveRobber = false;
            AwaitedDiscards = 0;

            HasPlayedDevelopmentCard = false;
        }
    }
}
