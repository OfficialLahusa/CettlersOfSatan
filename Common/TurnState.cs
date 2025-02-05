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
            SecondInitial,
            MatchEnded
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


        public TurnState()
        {
            PlayerIndex = 0;

            TypeOfRound = RoundType.FirstInitial;
            RoundCounter = 0;

            MustRoll = false;
            MustMoveRobber = false;
            AwaitedDiscards = 0;

            HasPlayedDevelopmentCard = false;
        }
    }
}
