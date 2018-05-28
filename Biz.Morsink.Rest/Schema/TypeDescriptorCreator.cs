using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Biz.Morsink.Rest.Utils;
using Biz.Morsink.Rest.FSharp;

namespace Biz.Morsink.Rest.Schema
{
    /// <summary>
    /// A class that helps construct TypeDescriptor objects for CLR types.
    /// </summary>
    public class TypeDescriptorCreator
    {
        private class MultipleKinds : IKindPipeline
        {
            private IKind[] kinds;

            public MultipleKinds(IEnumerable<IKind> kinds)
            {
                this.kinds = kinds.ToArray();
            }

            public TypeDescriptor GetDescriptor(TypeDescriptorCreator creator, Context context)
            {
                TypeDescriptor result = null;
                for (int i = 0; i < kinds.Length && result == null; i++)
                    result = kinds[i].GetDescriptor(creator, context);
                return result;
            }
        }
        /// <summary>
        /// This interface specifies the contract for a single kind of TypeDescriptor creator.
        /// The kinds are processed in an IKindPipeline in order, which can be injected.
        /// </summary>
        public interface IKind
        {
            /// <summary>
            /// Gets a TypeDescriptor for some type in some context.
            /// </summary>
            /// <param name="creator">The TypeDescriptorCreator instance that is processing the request.</param>
            /// <param name="context">The type context for generating a descriptor.</param>
            /// <returns>
            /// If the context matches this kind it should return a TypeDescriptor for the context. 
            /// If the context does not match, this method should return null.
            /// </returns>
            TypeDescriptor GetDescriptor(TypeDescriptorCreator creator, Context context);
        }
        /// <summary>
        /// The interface for a kind pipeline.
        /// </summary>
        public interface IKindPipeline : IKind { }
        /// <summary>
        /// Creates a kind pipeline, based on an oredered sequence of kinds.
        /// The resulting pipeline will try each kind in order.
        /// </summary>
        /// <param name="kinds">The kinds to try.</param>
        /// <returns>A kind pipeline.</returns>
        public static IKindPipeline CreateKindPipeline(IEnumerable<IKind> kinds)
            => new MultipleKinds(kinds);
        /// <summary>
        /// This class represents the context for the request of a TypeDescriptor.
        /// This class is immutable.
        /// </summary>
        public class Context
        {
            /// <summary>
            /// Constructor.
            /// </summary>
            /// <param name="type">The type a descriptor is being requested for.</param>
            /// <param name="cutoff">The type to function as a cutoff for reflection.</param>
            /// <param name="enclosing">A collection of enclosing type descriptor definitions.</param>
            public Context(Type type, Type cutoff = null, ImmutableStack<Type> enclosing = null)
            {
                Type = type;
                Cutoff = cutoff;
                Enclosing = enclosing ?? ImmutableStack<Type>.Empty;
            }
            /// <summary>
            /// Contains the type a descriptor is neing requested for.
            /// </summary>
            public Type Type { get; }
            /// <summary>
            /// Contains the (base) type to funcvtion as a cutoff for reflection.
            /// This allows descriptors to be constructed per inheritance step.
            /// </summary>
            public Type Cutoff { get; }
            /// <summary>
            /// Contains a collection of enclosing type descriptor definitions.
            /// This prevents recursive definitions to result in infinite type descriptors or stack overflows.
            /// </summary>
            public ImmutableStack<Type> Enclosing { get; }

            /// <summary>
            /// Creates a new Context with a different Type.
            /// </summary>
            /// <param name="type">The new Type.</param>
            /// <returns>A new context.</returns>
            public Context WithType(Type type)
                => new Context(type, Cutoff, Enclosing);
            /// <summary>
            /// Creates a new Context with a different Cutoff type.
            /// </summary>
            /// <param name="cutoff">The new cutoff type.</param>
            /// <returns>A new context.</returns>
            public Context WithCutoff(Type cutoff)
                => new Context(Type, cutoff, Enclosing);
            /// <summary>
            /// Creates a new Context with a different Enclosing types stack.
            /// </summary>
            /// <param name="enclosing">A new enclosing types stack.</param>
            /// <returns>A new collection.</returns>
            public Context WithEnclosing(ImmutableStack<Type> enclosing)
                => new Context(Type, Cutoff, enclosing);
            /// <summary>
            /// Creates a new Context by pushing a new type on the Enclosing stack.
            /// </summary>
            /// <param name="type">The type to push on the stack.</param>
            /// <returns>A new context.</returns>
            public Context PushEnclosing(Type type)
                => new Context(Type, Cutoff, Enclosing.Push(type));
            /// <summary>
            /// Creates a new Context by popping a type off the Enclosing stack.
            /// </summary>
            /// <returns>A new context.</returns>
            public Context PopEnclosing()
                => new Context(Type, Cutoff, Enclosing.Pop());
        }
        private ConcurrentDictionary<Type, TypeDescriptor> descriptors;
        private ConcurrentDictionary<string, TypeDescriptor> byString;
        private readonly IKindPipeline kindPipeline;
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
        public TypeDescriptorCreator(IEnumerable<ITypeRepresentation> representations = null, IKindPipeline kindPipeline = null)
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

            this.kindPipeline = kindPipeline ?? CreateKindPipeline(new IKind[] {
                NullableDescriptorKind.Instance,
                DictionaryDescriptorKind.Instance,
                ArrayDescriptorKind.Instance,
                FSharpUnionDescriptorKind.Instance,
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
            => type == null ? null : GetDescriptor(new Context(type));
        private static bool IsPrimitiveTypeDescriptor(TypeDescriptor desc)
        {
            if (desc is TypeDescriptor.Primitive || desc is TypeDescriptor.Null || desc is TypeDescriptor.Referable
                || desc is TypeDescriptor.Reference || desc is TypeDescriptor.Value)
                return true;
            else if (desc is TypeDescriptor.Union u)
                return u.Options.All(IsPrimitiveTypeDescriptor);
            else if (desc is TypeDescriptor.Intersection i)
                return i.Parts.All(IsPrimitiveTypeDescriptor);
            else if (desc is TypeDescriptor.Array a)
                return IsPrimitiveTypeDescriptor(a.ElementType);
            else
                return false;
        }
        /// <summary>
        /// Creates a TypeDescriptor and makes it 'Referable' if it is not a primitive descriptor.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public TypeDescriptor GetReferableDescriptor(Context context)
        {
            var desc = GetDescriptor(context);
            if (IsPrimitiveTypeDescriptor(desc))
                return desc;
            else
                return TypeDescriptor.Referable.Create(GetTypeName(context.Type), desc);
        }
        /// <summary>
        /// Get a TypeDescriptor for a given context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>A TypeDescriptor.</returns>
        public TypeDescriptor GetDescriptor(Context context)
        {
            if (context.Enclosing.Contains(context.Type))
                return new TypeDescriptor.Reference(GetTypeName(context.Type));
            return descriptors.GetOrAdd(context.Type, ty =>
            {
                ty = representations.Where(rep => rep.IsRepresentable(ty)).Select(rep => rep.GetRepresentationType(ty)).FirstOrDefault() ?? ty;
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

    }
}
