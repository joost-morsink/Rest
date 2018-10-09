using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Biz.Morsink.Identity.PathProvider;
using Biz.Morsink.Rest.Serialization;
using Ex = System.Linq.Expressions.Expression;
namespace Biz.Morsink.Rest.Schema
{
    public class UnionRepresentationDescriptorKind : TypeDescriptorCreator.IKind
    {
        public static UnionRepresentationDescriptorKind Instance { get; } = new UnionRepresentationDescriptorKind();
        public TypeDescriptor GetDescriptor(ITypeDescriptorCreator creator, TypeDescriptorCreator.Context context)
        {
            if (!IsOfKind(context.Type))
                return null;
            var typeParams = GetTypes(context.Type);
            return TypeDescriptor.MakeUnion(context.Type.Name, typeParams.Select(tp => creator.GetDescriptor(tp)), context.Type);
        }

        bool TypeDescriptorCreator.IKind.IsOfKind(Type type) => IsOfKind(type);

        public static bool IsOfKind(Type type)
            => typeof(UnionRepresentation).IsAssignableFrom(type);

        public static IReadOnlyList<Type> GetTypes(Type type)
            => (IReadOnlyList<Type>)type.GetMethod(nameof(UnionRepresentation<object, object>.GetTypeParameters)).Invoke(null, null);

        public Serializer<C>.IForType GetSerializer<C>(Serializer<C> serializer, Type type) where C : SerializationContext<C>
            => IsOfKind(type)
            ? (Serializer<C>.IForType)Activator.CreateInstance(typeof(SerializerImpl<,>).MakeGenericType(typeof(C), type), serializer)
            : null;
        private class SerializerImpl<C, T> : Serializer<C>.Typed<T>.Func
            where C : SerializationContext<C>
        {
            public SerializerImpl(Serializer<C> parent) : base(parent)
            {

            }

            private static int Score(TypeDescriptor td, SItem item)
            {
                var score = 0;
                if (item is SObject sobj)
                {
                    var props = (td as TypeDescriptor.Record)?.Properties;
                    if (props != null)
                    {
                        var propDict = props.ToDictionary(p => p.Key, p => p.Value, CaseInsensitiveEqualityComparer.Instance);
                        var req = new HashSet<string>(props.Where(p => p.Value.Required).Select(p => p.Key), CaseInsensitiveEqualityComparer.Instance);
                        foreach (var prop in sobj.Properties)
                        {
                            if (propDict.TryGetValue(prop.Name, out var desc))
                            {
                                if (req.Count > 0 && desc.Required)
                                    req.Remove(desc.Name);
                                score += 10;
                            }
                        }
                        if (req.Count == 0)
                            score *= 10;
                        return score;
                    }
                }
                return 1;
            }
            private static Type Best((Type,TypeDescriptor)[] options, SItem item)
            {
                int bestScore = int.MinValue;
                Type best = null;
                for(int i=0; i< options.Length; i++)
                {
                    var score = Score(options[i].Item2, item);
                    if (score > bestScore)
                    {
                        bestScore = score;
                        best = options[i].Item1;
                    }
                }
                return best;
            }
            protected override Func<C, SItem, T> MakeDeserializer()
            {
                var tds = UnionRepresentation.GetTypeParameters(typeof(T)).Select(t => (type:t,desc:Parent.TypeDescriptorCreator.GetDescriptor(t))).ToArray();
                var builder = UnionRepresentation.FromOptions(tds.Select(t => t.type).ToArray());
                var score = typeof(SerializerImpl<C, T>).GetMethod(nameof(Score), BindingFlags.Static | BindingFlags.NonPublic);
                var input = Ex.Parameter(typeof(SItem), "input");
                var ctx = Ex.Parameter(typeof(C), "ctx");
                var best = Ex.Parameter(typeof(Type), "best");

                var block = Ex.Block(new[] { best },
                    Ex.Assign(best, 
                        Ex.Call(typeof(SerializerImpl<C, T>).GetMethod(nameof(Best), BindingFlags.Static | BindingFlags.NonPublic),
                            Ex.Constant(tds),
                            input)),
                    Ex.Convert(
                        Ex.Call(Ex.Constant(builder), nameof(UnionRepresentation.RepresentationCreator.Create), Type.EmptyTypes,
                            Ex.Call(Ex.Constant(Parent), DESERIALIZE, Type.EmptyTypes,
                                ctx, best, input)), 
                        typeof(T)));
                var lambda = Ex.Lambda<Func<C, SItem, T>>(block, ctx, input);
                return lambda.Compile();
            }

            protected override Func<C, T, SItem> MakeSerializer()
            {
                var input = Ex.Parameter(typeof(T), "input");
                var ctx = Ex.Parameter(typeof(C), "ctx");
                var block = Ex.Call(Ex.Constant(Parent), SERIALIZE, Type.EmptyTypes,
                    ctx,
                    Ex.Call(input, nameof(UnionRepresentation.GetItemType), Type.EmptyTypes),
                    Ex.Call(input, nameof(UnionRepresentation.GetItem), Type.EmptyTypes));
                var lambda = Ex.Lambda<Func<C, T, SItem>>(block, ctx, input);
                return lambda.Compile();
            }
        }
    }
}
