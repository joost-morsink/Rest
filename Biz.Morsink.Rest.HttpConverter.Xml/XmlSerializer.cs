using Biz.Morsink.DataConvert;
using Biz.Morsink.Rest.Schema;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Linq;
using Ex = System.Linq.Expressions.Expression;

namespace Biz.Morsink.Rest.HttpConverter.Xml
{
    public class XmlSerializer
    {
        private readonly ConcurrentDictionary<Type, IForType> serializers;
        private readonly TypeDescriptorCreator typeDescriptorCreator;
        private readonly IDataConverter converter;
        private readonly IReadOnlyList<ITypeRepresentation> representations;

        public XmlSerializer(TypeDescriptorCreator typeDescriptorCreator, IDataConverter converter, IEnumerable<ITypeRepresentation> representations)
        {
            serializers = new ConcurrentDictionary<Type, IForType>();
            this.typeDescriptorCreator = typeDescriptorCreator;
            this.converter = converter;
            this.representations = representations.ToArray();
            InitializeDefaultSerializers();
        }
        private void InitializeDefaultSerializers()
        {
            serializers[typeof(string)] = new Typed<string>.Simple(this, converter);

            serializers[typeof(long)] = new Typed<long>.Simple(this, converter);
            serializers[typeof(int)] = new Typed<int>.Simple(this, converter);
            serializers[typeof(short)] = new Typed<short>.Simple(this, converter);
            serializers[typeof(sbyte)] = new Typed<sbyte>.Simple(this, converter);
            serializers[typeof(ulong)] = new Typed<ulong>.Simple(this, converter);
            serializers[typeof(uint)] = new Typed<uint>.Simple(this, converter);
            serializers[typeof(ushort)] = new Typed<ushort>.Simple(this, converter);
            serializers[typeof(byte)] = new Typed<byte>.Simple(this, converter);

            serializers[typeof(decimal)] = new Typed<decimal>.Simple(this, converter);
            serializers[typeof(float)] = new Typed<float>.Simple(this, converter);
            serializers[typeof(double)] = new Typed<double>.Simple(this, converter);

            serializers[typeof(bool)] = new Typed<bool>.Simple(this, converter);

            serializers[typeof(DateTime)] = new Typed<DateTime>.Simple(this, converter);

        }

        public XElement Serialize(object item)
        {
            var serializer = GetSerializerForType(item.GetType());
            return serializer.Serialize(item);
        }
        public XElement Serialize<T>(T item)
        {
            var serializer = GetSerializerForType<T>();
            return serializer.Serialize(item);
        }
        public object Deserialize(XElement element, Type type)
        {
            var serializer = GetSerializerForType(type);
            return serializer.Deserialize(element);
        }
        public T Deserialize<T>(XElement element)
        {
            var serializer = GetSerializerForType<T>();
            return serializer.Deserialize(element);
        }

        public IForType GetSerializerForType(Type type)
            => serializers.GetOrAdd(type, get);
        public Typed<T> GetSerializerForType<T>()
            => (Typed<T>)GetSerializerForType(typeof(T));

        private IForType get(Type t)
        {
            var repr = representations.FirstOrDefault(r => r.IsRepresentable(t));

            return (IForType)(repr == null
                ? Activator.CreateInstance(typeof(Typed<>.Default).MakeGenericType(t), this)
                : Activator.CreateInstance(typeof(Typed<>.Represented).MakeGenericType(t), this, repr));
        }

        #region Helper types
        public interface IForType
        {
            Type ForType { get; }
            XElement Serialize(object item);
            object Deserialize(XElement element);
        }
        public abstract class Typed<T> : IForType
        {
            protected XmlSerializer Parent { get; }
            public Typed(XmlSerializer parent)
            {
                Parent = parent;
            }
            public abstract XElement Serialize(T item);
            public abstract T Deserialize(XElement e);

            Type IForType.ForType => typeof(T);
            XElement IForType.Serialize(object item) => Serialize((T)item);
            object IForType.Deserialize(XElement element) => Deserialize(element);
            [Obsolete]
            public class DotNetDefault : Typed<T>
            {
                private System.Xml.Serialization.XmlSerializer serializer;
                public DotNetDefault(XmlSerializer parent) : base(parent)
                {
                    serializer = new System.Xml.Serialization.XmlSerializer(typeof(T));
                }

                public override XElement Serialize(T item)
                {
                    var root = new XElement("root");
                    using (var wri = root.CreateWriter())
                        serializer.Serialize(wri, item);
                    return root.Elements().First();
                }
                public override T Deserialize(XElement e)
                {
                    using (var rdr = e.CreateReader())
                        return (T)serializer.Deserialize(rdr);
                }
            }
            public class Simple : Typed<T>
            {
                private readonly IDataConverter converter;

                public Simple(XmlSerializer parent, IDataConverter converter) : base(parent)
                {
                    this.converter = converter;
                }

                public override T Deserialize(XElement e) => converter.Convert(e.Value).TryTo(out T res) ? res : default(T);
                public override XElement Serialize(T item) => new XElement("simple", converter.Convert(item).To<string>());

            }

            public class Default : Typed<T>
            {
                private Func<T, XElement> serializer;
                private Func<XElement, T> deserializer;

                public Default(XmlSerializer parent) : base(parent)
                {
                    serializer = makeSerializer();
                }

                public override T Deserialize(XElement e)
                {
                    throw new NotImplementedException();
                }

                public override XElement Serialize(T item)
                    => serializer(item);

                private Func<T, XElement> makeSerializer()
                {
                    var input = Ex.Parameter(typeof(T), "input");
                    var props = typeof(T).GetTypeInfo().Iterate(x => x.BaseType?.GetTypeInfo())
                        .TakeWhile(x => x != null)
                        .SelectMany(x => x.DeclaredProperties)
                        .Where(p => p.CanRead && p.GetMethod.IsPublic && !p.GetMethod.IsStatic)
                        .GroupBy(x => x.Name)
                        .Select(x => x.First())
                        .ToArray();
                    var block = Ex.New(typeof(XElement).GetConstructor(new[] { typeof(XName), typeof(object[]) }),
                        Ex.Convert(Ex.Constant(Parent.typeDescriptorCreator.GetTypeName(typeof(T))), typeof(XName)),
                        Ex.NewArrayInit(typeof(object),
                            props.Select(prop =>
                                Ex.New(typeof(XElement).GetConstructor(new[] { typeof(XName), typeof(object) }),
                                Ex.Convert(Ex.Constant(prop.Name), typeof(XName)),
                                Ex.Call(typeof(Utils), nameof(Utils.GetContent), Type.EmptyTypes,
                                    Ex.Call(Ex.Constant(Parent), nameof(XmlSerializer.Serialize), Type.EmptyTypes,
                                        Ex.Convert(Ex.Property(input, prop), typeof(object))))))));
                    var lambda = Ex.Lambda(block, input);
                    return (Func<T, XElement>)lambda.Compile();
                }
            }
            public class Represented : Typed<T>
            {
                private readonly ITypeRepresentation representation;

                public Represented(XmlSerializer parent, ITypeRepresentation representation) : base(parent)
                {
                    this.representation = representation;
                }
                public override XElement Serialize(T item)
                {
                    var repr = representation.GetRepresentation(item);
                    return Parent.Serialize(repr);
                }
                public override T Deserialize(XElement e)
                {
                    var repr = Parent.Deserialize(e, representation.GetRepresentationType(typeof(T)));
                    return (T)representation.GetRepresentable(repr);
                }
            }
        }
        #endregion
    }
}
