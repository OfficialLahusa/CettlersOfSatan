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
        public RollResult LastRoll;

        public bool MustRoll { get; set; }
        public bool[] AwaitedPlayerDiscards { get; set; }
        public bool MustDiscard
        {
            get { return AwaitedPlayerDiscards.Any(x => x); }
        }
        public bool MustMoveRobber { get; set; }

        public bool HasPlayedDevelopmentCard { get; set; }


        public TurnState(uint playerCount)
        {
            PlayerIndex = 0;

            TypeOfRound = RoundType.FirstInitial;
            RoundCounter = 0;
            LastRoll = RollResult.GetRandom();

            MustRoll = false;
            MustMoveRobber = false;
            AwaitedPlayerDiscards = new bool[playerCount];

            HasPlayedDevelopmentCard = false;
        }
    }
}
