using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Biz.Morsink.Rest.FSharp;
using Biz.Morsink.Rest.Serialization;

namespace Biz.Morsink.Rest.Schema
{
    public class StandardTypeDescriptorCreator : ITypeDescriptorCreator { 
        private ConcurrentDictionary<Type, TypeDescriptor> descriptors;
        private ConcurrentDictionary<string, TypeDescriptor> byString;

        private readonly TypeDescriptorCreator.IKindPipeline kindPipeline;
        private readonly IEnumerable<ITypeRepresentation> representations;
        /// <summary>
        /// Gets a collection of all the registered types.
        /// </summary>
        public ICollection<Type> RegisteredTypes => descriptors.Keys;
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="representations">A collection of type representations.</param>
        /// <param name="kindPipeline">
        /// A pipeline of different creator kinds. The default pipeline is:
        /// <list type="number">
        /// <item>NullableDescriptorKind</item>
        /// <item>DictionaryDescriptorKind</item>
        /// <item>ArrayDescriptorKind</item>
        /// <item>FSharpUnionDescriptorKind</item>
        /// <item>UnionDescriptorKind</item>
        /// <item>RecordDescriptorKind</item>
        /// <item>UnitDescriptorKind</item>
        /// </list>
        /// </param>
        public StandardTypeDescriptorCreator(IEnumerable<ITypeRepresentation> representations = null, TypeDescriptorCreator.IKindPipeline kindPipeline = null)
        {
            this.representations = representations ?? Enumerable.Empty<ITypeRepresentation>();
            var d = new ConcurrentDictionary<Type, TypeDescriptor>();

            d[typeof(string)] = TypeDescriptor.Primitive.String.Instance;
            d[typeof(long)] = TypeDescriptor.Primitive.Numeric.Integral.Instance;
            d[typeof(int)] = TypeDescriptor.Primitive.Numeric.Integral.Instance;
            d[typeof(short)] = TypeDescriptor.Primitive.Numeric.Integral.Instance;
            d[typeof(sbyte)] = TypeDescriptor.Primitive.Numeric.Integral.Instance;
            d[typeof(ulong)] = TypeDescriptor.Primitive.Numeric.Integral.Instance;
            d[typeof(uint)] = TypeDescriptor.Primitive.Numeric.Integral.Instance;
            d[typeof(ushort)] = TypeDescriptor.Primitive.Numeric.Integral.Instance;
            d[typeof(byte)] = TypeDescriptor.Primitive.Numeric.Integral.Instance;

            d[typeof(decimal)] = TypeDescriptor.Primitive.Numeric.Float.Instance;
            d[typeof(float)] = TypeDescriptor.Primitive.Numeric.Float.Instance;
            d[typeof(double)] = TypeDescriptor.Primitive.Numeric.Float.Instance;

            d[typeof(bool)] = TypeDescriptor.Primitive.Boolean.Instance;

            d[typeof(DateTime)] = TypeDescriptor.Primitive.DateTime.Instance;

            d[typeof(object)] = TypeDescriptor.Any.Instance;

            descriptors = d;
            byString = new ConcurrentDictionary<string, TypeDescriptor>(descriptors.Select(e => new KeyValuePair<string, TypeDescriptor>(e.Key.ToString(), e.Value)));

            this.kindPipeline = kindPipeline ?? TypeDescriptorCreator.CreateKindPipeline(new TypeDescriptorCreator.IKind[] {
                new RepresentableDescriptorKind(representations),
                NullableDescriptorKind.Instance,
                DictionaryDescriptorKind.Instance,
                SemanticStructKind.Instance,
                FSharpUnionDescriptorKind.Instance,
                ArrayDescriptorKind.Instance,
                UnionRepresentationDescriptorKind.Instance,
                UnionDescriptorKind.Instance,
                RecordDescriptorKind.Instance,
                UnitDescriptorKind.Instance,
            });
        }

        /// <summary>
        /// Gets a TypeDescriptor for this type.
        /// </summary>
        /// <param name="type">The type to get a TypeDescriptor for.</param>
        /// <returns>A TypeDescriptor for the type.</returns>
        public TypeDescriptor GetDescriptor(Type type)
            => type == null ? null : GetDescriptor(new TypeDescriptorCreator.Context(type));

        /// <summary>
        /// Creates a TypeDescriptor and makes it 'Referable' if it is not a primitive descriptor.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public TypeDescriptor GetReferableDescriptor(TypeDescriptorCreator.Context context)
        {
            var desc = GetDescriptor(context);
            if (TypeDescriptorCreator.IsPrimitiveTypeDescriptor(desc))
                return desc;
            else
                return TypeDescriptor.Referable.Create(GetTypeName(context.Type), desc);
        }
        /// <summary>
        /// Get a TypeDescriptor for a given context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>A TypeDescriptor.</returns>
        public TypeDescriptor GetDescriptor(TypeDescriptorCreator.Context context)
        {
            if (context.Enclosing.Contains(context.Type))
                return new TypeDescriptor.Reference(GetTypeName(context.Type));
            return descriptors.GetOrAdd(context.Type, ty =>
            {
                //ty = representations.Where(rep => rep.IsRepresentable(ty)).Select(rep => rep.GetRepresentationType(ty)).FirstOrDefault() ?? ty;
                var ctx = context.WithType(ty).PushEnclosing(context.Type);
                var desc = kindPipeline.GetDescriptor(this, ctx);
                byString.AddOrUpdate(GetTypeName(context.Type), desc, (_, __) => desc);
                return desc;
            });
        }
        /// <summary>
        /// Gets a TypeDescriptor with a specified name.
        /// </summary>
        /// <param name="name">The name of the TypeDescriptor.</param>
        /// <returns></returns>
        public TypeDescriptor GetDescriptorByName(string name)
            => byString.TryGetValue(name, out var res) ? res : null;
        /// <summary>
        /// Gets the 'name' for a Type.
        /// The name is used as a key to lookup TypeDescriptors.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>The name for a type.</returns>
        public string GetTypeName(Type type)
            => type.ToString().Replace('+', '.');

        public Serializer<C>.IForType CreateSerializer<C>(Serializer<C> serializer, Type t)
            where C : SerializationContext<C>
        {
            var specificSerializer = kindPipeline.GetSerializer(serializer, t);
            if (specificSerializer == null)
                throw new InvalidOperationException("Cannot create serializer for type.");
            return specificSerializer;
        }
    }
}
