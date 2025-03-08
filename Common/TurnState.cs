﻿using System;
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
            AwaitedPlayerDiscards = new bool[playerCount];
            MustMoveRobber = false;

            HasPlayedDevelopmentCard = false;
        }

        /// <summary>
        /// Deep copy constructor
        /// </summary>
        /// <param name="copy">Instance to copy</param>
        public TurnState(TurnState copy)
        {
            PlayerIndex = copy.PlayerIndex;

            TypeOfRound = copy.TypeOfRound;
            RoundCounter = copy.RoundCounter;
            LastRoll = copy.LastRoll;

            MustRoll = copy.MustRoll;
            AwaitedPlayerDiscards = (bool[])copy.AwaitedPlayerDiscards.Clone();
            MustMoveRobber = copy.MustMoveRobber;

            HasPlayedDevelopmentCard = copy.HasPlayedDevelopmentCard;
        }

        public override bool Equals(object? obj)
        {
            return obj is TurnState state 
                && PlayerIndex == state.PlayerIndex 
                && TypeOfRound == state.TypeOfRound 
                && RoundCounter == state.RoundCounter 
                && LastRoll.Equals(state.LastRoll) 
                && MustRoll == state.MustRoll 
                && AwaitedPlayerDiscards.SequenceEqual(state.AwaitedPlayerDiscards) 
                && MustMoveRobber == state.MustMoveRobber 
                && HasPlayedDevelopmentCard == state.HasPlayedDevelopmentCard;
        }

        public override int GetHashCode()
        {
            HashCode hash = new HashCode();

            hash.Add(PlayerIndex);
            hash.Add(TypeOfRound);
            hash.Add(RoundCounter);
            hash.Add(LastRoll);
            hash.Add(MustRoll);
            
            foreach (bool val in AwaitedPlayerDiscards)
            {
                hash.Add(val);
            }

            hash.Add(MustMoveRobber);
            hash.Add(HasPlayedDevelopmentCard);

            return hash.ToHashCode();
        }
    }
}
