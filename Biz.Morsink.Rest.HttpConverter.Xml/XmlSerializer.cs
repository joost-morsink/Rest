using Biz.Morsink.DataConvert;
using Biz.Morsink.Rest.Schema;
using System;
using System.Collections;
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
        private static string StripName(string name)
        {
            if (name.Contains('`'))
                return name.Substring(0, name.IndexOf('`'));
            else
                return name;
        }
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
            if (item == null)
                return null;
            var serializer = GetSerializerForType(item.GetType());
            return serializer.Serialize(item);
        }
        public XElement Serialize<T>(T item)
        {
            if (item == null)
                return null;
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

            var res = (IForType)(repr == null
                ? typeof(IEnumerable).IsAssignableFrom(t)
                    ? Activator.CreateInstance(typeof(Typed<>.Collection).MakeGenericType(t), this)
                    : Activator.CreateInstance(typeof(Typed<>.Default).MakeGenericType(t), this)
                : Activator.CreateInstance(typeof(Typed<>.Represented).MakeGenericType(t), this, t, repr));

            return res;
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
            public class Collection : Typed<T>
            {
                private readonly Type basetype;
                private readonly Func<T, XElement> serializer;
                private readonly Func<XElement, T> deserializer;

                public Collection(XmlSerializer parent) : base(parent)
                {
                    basetype = typeof(T).GetGeneric(typeof(IEnumerable<>));
                    if (basetype == null)
                        throw new ArgumentException("Generic type is not a collection");
                    serializer = makeSerializer();
                }
                private Func<T, XElement> makeSerializer()
                {
                    var input = Ex.Parameter(typeof(T), "input");
                    var result = Ex.Parameter(typeof(XElement), "result");
                    var enumerator = Ex.Parameter(typeof(IEnumerator<>).MakeGenericType(basetype), "enumerator");
                    var item = Ex.Parameter(basetype, "item");
                    var start = Ex.Label("start");
                    var end = Ex.Label("end");
                    var block = Ex.Block(new[] { item,enumerator,result },
                        Ex.Assign(result, Ex.New(typeof(XElement).GetConstructor(new[] { typeof(XName) }), Ex.Constant((XName)"Array"))),
                        Ex.Assign(enumerator, Ex.Call(input, typeof(IEnumerable<>).MakeGenericType(basetype).GetMethod(nameof(IEnumerable<object>.GetEnumerator)))),
                        Ex.Label(start),
                        Ex.IfThen(Ex.Not(Ex.Call(Ex.Convert(enumerator, typeof(IEnumerator)), nameof(IEnumerator.MoveNext), Type.EmptyTypes)),
                            Ex.Goto(end, result)),
                        Ex.Assign(item, Ex.Property(enumerator, nameof(IEnumerator<object>.Current))),
                        Ex.Call(result, nameof(XElement.Add), Type.EmptyTypes,
                            Ex.Convert(Ex.Call(Ex.Constant(Parent), nameof(XmlSerializer.Serialize), Type.EmptyTypes,
                                Ex.Convert(item, typeof(object))),
                                typeof(object))),
                        Ex.Goto(start),
                        Ex.Label(end),
                        result);
                    var lambda = Ex.Lambda(block, input);
                    return (Func<T, XElement>)lambda.Compile();
                }
                public override T Deserialize(XElement e)
                {
                    throw new NotImplementedException();
                }

                public override XElement Serialize(T item)
                    => serializer(item);
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
                        Ex.Convert(Ex.Constant(StripName(typeof(T).Name)), typeof(XName)),
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
                private readonly Type originalType;

                public Represented(XmlSerializer parent, Type originalType, ITypeRepresentation representation) : base(parent)
                {
                    this.representation = representation;
                    this.originalType = originalType;
                }
                public override XElement Serialize(T item)
                {
                    var repr = representation.GetRepresentation(item);
                    var res = Parent.Serialize(repr);
                    return new XElement(StripName(originalType.Name), res.GetContent());
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
