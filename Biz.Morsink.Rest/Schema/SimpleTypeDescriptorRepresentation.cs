using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest.Schema
{
    public class SimpleTypeDescriptorRepresentation : SimpleTypeRepresentation<TypeDescriptor, SimpleTypeDescriptorRepresentation.Representation>
    {
        public static SimpleTypeDescriptorRepresentation Instance { get; } = new SimpleTypeDescriptorRepresentation();
        private SimpleTypeDescriptorRepresentation() : base() { }

        public override TypeDescriptor GetRepresentable(Representation representation)
        {
            throw new NotSupportedException();
        }

        public override Representation GetRepresentation(TypeDescriptor item)
            => Visitor.Instance.Visit(item);

        private class Visitor : TypeDescriptorVisitor<Representation>
        {
            public static Visitor Instance { get; } = new Visitor();
            private Visitor() { }
            protected override Representation VisitAny(TypeDescriptor.Any a)
                => Representation.Basic.Any;

            protected override Representation VisitArray(TypeDescriptor.Array a, Representation inner)
                => new Representation.Array(inner);

            protected override Representation VisitBoolean(TypeDescriptor.Primitive.Boolean b)
                => Representation.Basic.Boolean;

            protected override Representation VisitDateTime(TypeDescriptor.Primitive.DateTime dt)
                => Representation.Basic.DateTime;

            protected override Representation VisitDictionary(TypeDescriptor.Dictionary d, Representation valueType)
                => new Representation.Dictionary(valueType);

            protected override Representation VisitFloat(TypeDescriptor.Primitive.Numeric.Float f)
                => Representation.Basic.Float;

            protected override Representation VisitIntegral(TypeDescriptor.Primitive.Numeric.Integral i)
                => Representation.Basic.Integral;

            protected override Representation VisitIntersection(TypeDescriptor.Intersection i, Representation[] parts)
                => new Representation.Intersection(parts);

            protected override Representation VisitNull(TypeDescriptor.Null n)
                => Representation.Basic.Null;

            protected override Representation VisitRecord(TypeDescriptor.Record r, PropertyDescriptor<Representation>[] props)
                => new Representation.Record(props);

            protected override Representation VisitReferable(TypeDescriptor.Referable r, Representation expandedDescriptor)
                => expandedDescriptor;

            protected override Representation VisitReference(TypeDescriptor.Reference r)
                => new Representation.Reference(r.RefName);

            protected override Representation VisitString(TypeDescriptor.Primitive.String s)
                => Representation.Basic.String;

            protected override Representation VisitUnion(TypeDescriptor.Union u, Representation[] options)
                => new Representation.Union(options);

            protected override Representation VisitValue(TypeDescriptor.Value v, Representation inner)
                => new Representation.Value(inner, v.InnerValue);
        }

        public abstract class Representation
        {
            public sealed class Basic : Representation
            {
                public static Basic String => new Basic(nameof(String));
                public static Basic Integral => new Basic(nameof(Integral));
                public static Basic Float => new Basic(nameof(Float));
                public static Basic DateTime => new Basic(nameof(DateTime));
                public static Basic Boolean => new Basic(nameof(Boolean));
                public static Basic Null => new Basic(nameof(Null));
                public static Basic Any => new Basic(nameof(Any));

                private Basic(string type)
                {
                    Type = type;
                }

                public string Type { get; }
            }
            public sealed class Record : Representation
            {
                public Record(PropertyDescriptor<Representation>[] properties)
                {
                    Properties = properties;
                }

                public PropertyDescriptor<Representation>[] Properties { get; }
            }
            public sealed class Intersection : Representation
            {
                public Intersection(Representation[] parts)
                {
                    Parts = parts;
                }

                public Representation[] Parts { get; }
            }
            public sealed class Union : Representation
            {
                public Union(Representation[] options)
                {
                    Options = options;
                }

                public Representation[] Options { get; }
            }
            public sealed class Array : Representation
            {
                public Array(Representation inner)
                {
                    Inner = inner;
                }

                public Representation Inner { get; }
            }
            public sealed class Dictionary : Representation
            {
                public Dictionary(Representation values)
                {
                    Values = values;
                }

                public Representation Values { get; }
            }
            public sealed class Reference : Representation
            {
                public Reference(string @ref)
                {
                    Ref = @ref;
                }

                public string Ref { get; }
            }

            public sealed class Value : Representation
            {
                public Value(Representation baseType, object innerValue)
                {
                    BaseType = baseType;
                    InnerValue = innerValue;
                }

                public Representation BaseType { get; }
                public object InnerValue { get; }
            }
        }
    }
}
