using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public readonly struct RollResult
    {
        private readonly byte _first;
        private readonly byte _second;

        public readonly byte First
        {
            get { return _first; }
            init
            {
                if (value >= 1 && value <= 6)
                {
                    _first = value;
                }
            }
        }
        public readonly byte Second
        {
            get { return _second; }
            init
            {
                if (value >= 1 && value <= 6)
                {
                    _second = value;
                }
            }
        }

        public readonly byte Total
        {
            get { return (byte)(First + Second); }
        }

        public static RollResult GetRandom()
        {
            return new RollResult()
            {
                First = (byte)(Utils.Random.Next(6) + 1),
                Second = (byte)(Utils.Random.Next(6) + 1)
            };
        }
    }
}
