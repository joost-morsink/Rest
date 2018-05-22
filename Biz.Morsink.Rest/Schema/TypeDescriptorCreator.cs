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
        public interface IKind
        {
            TypeDescriptor GetDescriptor(TypeDescriptorCreator creator, Context context);
        }
        public interface IKindPipeline : IKind { }
        public static IKindPipeline CreateKindPipeline(IEnumerable<IKind> kinds)
            => new MultipleKinds(kinds);
        public class Context
        {
            public Context(Type type, Type cutoff=null, ImmutableStack<Type> enclosing =null)
            {
                Type = type;
                Cutoff = cutoff;
                Enclosing = enclosing ?? ImmutableStack<Type>.Empty;
            }
            public Type Type { get; }
            public Type Cutoff { get; }
            public ImmutableStack<Type> Enclosing { get; }

            public Context WithType(Type type)
                => new Context(type, Cutoff, Enclosing);
            public Context WithCutoff(Type cutoff)
                => new Context(Type, cutoff, Enclosing);
            public Context WithEnclosing(ImmutableStack<Type> enclosing)
                => new Context(Type, Cutoff, enclosing);
            public Context PushEnclosing(Type type)
                => new Context(Type, Cutoff, Enclosing.Push(type));
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
        public TypeDescriptor GetReferableDescriptor(Context context)
        {
            var desc = GetDescriptor(context);
            if (IsPrimitiveTypeDescriptor(desc))
                return desc;
            else
                return TypeDescriptor.Referable.Create(GetTypeName(context.Type), desc);
        }
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
