using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using Biz.Morsink.Rest.Serialization;

namespace Biz.Morsink.Rest.Schema
{
    /// <summary>
    /// A TypeDescriptorCreator.IKind implementation for ExpandoObject as a representative for dynamic objects.
    /// </summary>
    public class ExpandoObjectKind : TypeDescriptorCreator.IKind
    {
        /// <summary>
        /// Singleton instance.
        /// </summary>
        public static ExpandoObjectKind Instance { get; } = new ExpandoObjectKind();
        /// <summary>
        /// Constructor.
        /// </summary>
        private ExpandoObjectKind() { }

        public TypeDescriptor GetDescriptor(ITypeDescriptorCreator creator, TypeDescriptorCreator.Context context)
            => IsOfKind(context.Type)
                ? TypeDescriptor.MakeDictionary("ExpandoObject", TypeDescriptor.MakeAny())
                : null;

        public Serializer<C>.IForType GetSerializer<C>(Serializer<C> serializer, Type type) where C : SerializationContext<C>
            => IsOfKind(type)
                ? new SerializerImpl<C>(serializer)
                : null;

        /// <summary>
        /// Only ExpandoObject is of this kind.
        /// </summary>
        /// <param name="type">The type to check.</param>
        /// <returns>True if the specified type is ExpandoObject, false otherwise.</returns>
        public static bool IsOfKind(Type type)
            => type == typeof(ExpandoObject);
        bool TypeDescriptorCreator.IKind.IsOfKind(Type type)
            => IsOfKind(type);

        private class SerializerImpl<C> : Serializer<C>.Typed<ExpandoObject>.Simple
            where C : SerializationContext<C>
        {
            public SerializerImpl(Serializer<C> parent) : base(parent)
            {
            }

            public override ExpandoObject Deserialize(C context, SItem item)
            {
                var res = new ExpandoObject();
                var dict = (IDictionary<string, object>)res;
                if (item is SObject sobj)
                    foreach (var prop in sobj.Properties)
                        dict[prop.Name] = DeserializeItem(context, prop.Token);
                return res;
            }
            private object DeserializeItem(C context, SItem item)
            {
                switch (item)
                {
                    case SObject obj:
                        return Deserialize(context, obj);
                    case SValue val:
                        return val.Value;
                    case SArray arr:
                        return arr.Content.Select(element => DeserializeItem(context, element)).ToArray();
                    default:
                        throw new InvalidOperationException($"Unknown SItem type {item.GetType().FullName}");
                }
            }

            public override SItem Serialize(C context, ExpandoObject item)
            {
                var dict = (IDictionary<string, object>)item;
                return new SObject(dict.Select(kvp => new SProperty(kvp.Key, Parent.Serialize(context, kvp.Value))));
            }
        }
    }
}
