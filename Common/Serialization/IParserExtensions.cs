using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;

namespace Common.Serialization
{
    public static class IParserExtensions
    {
        public static bool TryFindMappingEntry(this ParsingEventBuffer parser, Func<Scalar, bool> selector, out Scalar key, out ParsingEvent value)
        {
            parser.Consume<MappingStart>();
            do
            {
                // so we only want to check keys in this mapping, don't descend
                switch (parser.Current)
                {
                    case Scalar scalar:
                        // we've found a scalar, check if it's value matches one
                        // of our  predicate
                        var keyMatched = selector(scalar);

                        // move head so we can read or skip value
                        parser.MoveNext();

                        // read the value of the mapping key
                        if (keyMatched)
                        {
                            // success
                            value = parser.Current;
                            key = scalar;
                            return true;
                        }

                        // skip the value
                        parser.SkipThisAndNestedEvents();

                        break;
                    case MappingStart or SequenceStart:
                        parser.SkipThisAndNestedEvents();
                        break;
                    default:
                        // do nothing, skip to next node
                        parser.MoveNext();
                        break;
                }
            } while (parser.Current is not null);

            key = null;
            value = null;
            return false;
        }
    }
}
