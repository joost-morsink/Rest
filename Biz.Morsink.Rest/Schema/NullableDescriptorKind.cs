using System;
using System.Reflection;
using Biz.Morsink.Rest.Serialization;
using Ex = System.Linq.Expressions.Expression;
using Biz.Morsink.DataConvert;
namespace Biz.Morsink.Rest.Schema
{
    /// <summary>
    /// This class represents a type descriptor creator kind for nullable types.
    /// A nullable type is Nullable&lt;T&gt; for some T.
    /// </summary>
    public class NullableDescriptorKind : TypeDescriptorCreator.IKind
    {
        /// <summary>
        /// Singleton property.
        /// </summary>
        public static NullableDescriptorKind Instance { get; } = new NullableDescriptorKind();
        private NullableDescriptorKind() { }
        /// <summary>
        /// Gets a type descriptor for a nullable type.
        /// A nullable type is Nullable&lt;T&gt; for some T.
        /// This method returns null if the context does not represent a nullable tyoe.
        /// </summary>
        public TypeDescriptor GetDescriptor(TypeDescriptorCreator creator, TypeDescriptorCreator.Context context)
        {
            var inner = GetInner(context.Type);
            if (inner == null)
                return null;
            else
            {
                var t = creator.GetReferableDescriptor(context.WithType(inner));
                return t == null ? null : new TypeDescriptor.Union(t.ToString() + "?", new TypeDescriptor[] { t, TypeDescriptor.Null.Instance }, context.Type);
            }
        }
        private static Type GetInner(Type type)
        {
            var ti = type.GetTypeInfo();
            var ga = ti.GetGenericArguments();
            if (ga.Length == 1)
            {
                if (ti.GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    return ga[0];
                }
            }
            return null;
        }
        /// <summary>
        /// A nullable type is Nullable&lt;T&gt; for some T.
        /// </summary>
        public bool IsOfKind(Type type)
            => GetInner(type) != null;

        public Serializer<C>.IForType GetSerializer<C>(Serializer<C> serializer, Type type) where C : SerializationContext<C>
            => IsOfKind(type)
            ? (Serializer<C>.IForType)Activator.CreateInstance(typeof(SerializerImpl<,>).MakeGenericType(typeof(C), type), serializer)
            : null;
        private class SerializerImpl<C, T> : Serializer<C>.Typed<T>.Func
            where C : SerializationContext<C>
        {
            public SerializerImpl(Serializer<C> parent) : base(parent) { }
            protected override Func<C, SItem, T> MakeDeserializer()
            {
                var valueType = GetInner(typeof(T));
                var ctx = Ex.Parameter(typeof(C), "ctx");
                var input = Ex.Parameter(typeof(SItem), "input");
                var val = Ex.Parameter(typeof(SValue), "val");
                var cr = Ex.Parameter(typeof(ConversionResult<>).MakeGenericType(valueType), "cr");
                var block = Ex.Block(new[] { val },
                    Ex.Assign(val, Ex.Convert(input, typeof(SValue))),
                    Ex.Condition(
                        Ex.MakeBinary(System.Linq.Expressions.ExpressionType.Equal,
                            Ex.Property(val, nameof(SValue.Value)), Ex.Default(typeof(object))),
                        Ex.New(typeof(Nullable<>).MakeGenericType(valueType)),
                        Ex.Block(new[] { cr },
                            Ex.Assign(cr, Ex.Call(typeof(DataConverterExt), nameof(DataConverterExt.DoConversion), new[] { typeof(object), valueType },
                                Ex.Constant(Parent.Converter), Ex.Property(val, nameof(SValue.Value)))),
                            Ex.Condition(Ex.Property(cr, nameof(ConversionResult<object>.IsSuccessful)),
                                Ex.New(typeof(Nullable<>).MakeGenericType(valueType).GetConstructor(new[] { valueType }),
                                    Ex.Property(cr, nameof(ConversionResult<object>.Result))),
                                Ex.New(typeof(Nullable<>).MakeGenericType(valueType))))));
                var lambda = Ex.Lambda<Func<C, SItem, T>>(block, ctx, input);
                return lambda.Compile();
            }

            protected override Func<C, T, SItem> MakeSerializer()
            {
                var valueType = GetInner(typeof(T));
                var ctx = Ex.Parameter(typeof(C), "ctx");
                var input = Ex.Parameter(typeof(T), "input");
                var block = Ex.Condition(
                    Ex.Property(input, nameof(Nullable<int>.HasValue)),
                    Ex.Call(Ex.Constant(Parent), SERIALIZE, new[] { valueType },
                        ctx, Ex.Property(input, nameof(Nullable<int>.Value))),
                    Ex.New(typeof(SValue).GetConstructor(new[] { typeof(object) }), Ex.Default(typeof(object))));
                var lambda = Ex.Lambda<Func<C, T, SItem>>(block, ctx, input);
                return lambda.Compile();
            }
        }
    }
}
