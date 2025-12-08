using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;

namespace Common.Serialization
{
    public class ParsingEventBuffer : IParser
    {
        private readonly LinkedList<ParsingEvent> buffer;

        private LinkedListNode<ParsingEvent> current;

        public ParsingEventBuffer(LinkedList<ParsingEvent> events)
        {
            buffer = events;
            current = events.First;
        }

        public ParsingEvent Current => current?.Value;

        public bool MoveNext()
        {
            current = current.Next;
            return current is not null;
        }

        public void Reset()
        {
            current = buffer.First;
        }
    }
}
