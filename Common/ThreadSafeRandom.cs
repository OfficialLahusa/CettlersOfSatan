using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class ThreadSafeRandom
    {
        private readonly object _randomLock = new object();
        private Random _random;

        public ThreadSafeRandom(int seed)
        {
            _random = new Random(seed);
        }

        public ThreadSafeRandom()
            : this(Guid.NewGuid().GetHashCode())
        { }

        public int Next(int min, int max)
        {
            lock (_randomLock) return _random.Next(min, max);
        }

        public int Next(int max)
        {
            lock (_randomLock) return _random.Next(max);
        }

        public int Next()
        {
            lock (_randomLock) return _random.Next();
        }
    }
}
