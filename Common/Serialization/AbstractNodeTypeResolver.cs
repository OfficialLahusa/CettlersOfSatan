using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NodeDeserializers;

namespace Common.Serialization
{
    public class AbstractNodeNodeTypeResolver : INodeDeserializer
    {
        private readonly INodeDeserializer original;
        private readonly ITypeDiscriminator[] typeDiscriminators;

        public AbstractNodeNodeTypeResolver(INodeDeserializer original, params ITypeDiscriminator[] discriminators)
        {
            if (original is not ObjectNodeDeserializer)
            {
                throw new ArgumentException($"{nameof(AbstractNodeNodeTypeResolver)} requires the original resolver to be a {nameof(ObjectNodeDeserializer)}");
            }

            this.original = original;
            typeDiscriminators = discriminators;
        }

        public bool Deserialize(IParser reader, Type expectedType, Func<IParser, Type, object?> nestedObjectDeserializer, out object? value, ObjectDeserializer rootDeserializer)
        {
            // we're essentially "in front of" the normal ObjectNodeDeserializer.
            // We could let it check if the current event is a mapping, but we also need to know.
            if (!reader.Accept<MappingStart>(out var mapping))
            {
                value = null;
                return false;
            }

            // can any of the registered discrimaintors deal with the abstract type?
            var supportedTypes = typeDiscriminators.Where(t => t.BaseType == expectedType);
            if (!supportedTypes.Any())
            {
                // no? then not a node/type we want to deal with
                return original.Deserialize(reader, expectedType, nestedObjectDeserializer, out value, rootDeserializer);
            }

            // now buffer all the nodes in this mapping.
            // it'd be better if we did not have to do this, but YamlDotNet does not support non-streaming access.
            // See:  https://github.com/aaubry/YamlDotNet/issues/343
            // WARNING: This has the potential to be quite slow and add a lot of memory usage, especially for large documents.
            // It's better, if you use this at all, to use it on leaf mappings
            var start = reader.Current.Start;
            Type actualType;
            ParsingEventBuffer buffer;
            try
            {
                buffer = new ParsingEventBuffer(ReadNestedMapping(reader));

                // use the discriminators to tell us what type it is really expecting by letting it inspect the parsing events
                actualType = CheckWithDiscriminators(expectedType, supportedTypes, buffer);
            }
            catch (Exception exception)
            {
                throw new YamlException(start, reader.Current.End, "Failed when resolving abstract type", exception);
            }

            // now continue by re-emitting parsing events
            buffer.Reset();
            return original.Deserialize(buffer, actualType, nestedObjectDeserializer, out value, rootDeserializer);
        }

        private static Type CheckWithDiscriminators(Type expectedType, IEnumerable<ITypeDiscriminator> supportedTypes, ParsingEventBuffer buffer)
        {
            foreach (var discriminator in supportedTypes)
            {
                buffer.Reset();
                if (discriminator.TryResolve(buffer, out var actualType))
                {
                    CheckReturnedType(discriminator.BaseType, actualType);
                    return actualType;
                }
            }

            throw new Exception($"None of the registered type discriminators could supply a child class for {expectedType}");
        }

        private static LinkedList<ParsingEvent> ReadNestedMapping(IParser reader)
        {
            var result = new LinkedList<ParsingEvent>();
            result.AddLast(reader.Consume<MappingStart>());
            var depth = 0;
            do
            {
                var next = reader.Consume<ParsingEvent>();
                depth += next.NestingIncrease;
                result.AddLast(next);
            } while (depth >= 0);

            return result;
        }

        private static void CheckReturnedType(Type baseType, Type candidateType)
        {
            if (candidateType is null)
            {
                throw new NullReferenceException($"The type resolver for AbstractNodeNodeTypeResolver returned null. It must return a valid sub-type of {baseType}.");
            }
            else if (candidateType.GetType() == baseType)
            {
                throw new InvalidOperationException($"The type resolver for AbstractNodeNodeTypeResolver returned the abstract type. It must return a valid sub-type of {baseType}.");
            }
            else if (!baseType.IsAssignableFrom(candidateType))
            {
                throw new InvalidOperationException($"The type resolver for AbstractNodeNodeTypeResolver returned a type ({candidateType}) that is not a valid sub type of {baseType}");
            }
        }
    }
}
