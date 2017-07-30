using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Biz.Morsink.Rest.Schema
{
    public abstract class TypeDescriptor : IEquatable<TypeDescriptor>
    {
        public abstract class Primitive : TypeDescriptor
        {
            public class String : Primitive
            {
                private readonly static int hashcode = typeof(String).GetHashCode();
                public static String Instance { get; } = new String();

                public override bool Equals(TypeDescriptor other)
                    => other is String;
                public override int GetHashCode()
                    => hashcode;
            }
            public abstract class Numeric : Primitive
            {
                public class Float : Numeric
                {
                    private readonly static int hashcode = typeof(Float).GetHashCode();
                    public static Float Instance { get; } = new Float();

                    public override bool Equals(TypeDescriptor other)
                        => other is Float;
                    public override int GetHashCode()
                        => hashcode;
                }
                public class Integral : Numeric
                {
                    private readonly static int hashcode = typeof(Integral).GetHashCode();
                    public static Integral Instance { get; } = new Integral();
                    
                    public override bool Equals(TypeDescriptor other)
                        => other is Integral;
                    public override int GetHashCode()
                        => hashcode;
                }
            }
            public class DateTime : Primitive
            {
                private readonly static int hashcode = typeof(DateTime).GetHashCode();
                public static DateTime Instance { get; } = new DateTime();

                public override bool Equals(TypeDescriptor other)
                    => other is DateTime;
                public override int GetHashCode()
                    => hashcode;
            }
        }
        public class Array : TypeDescriptor
        {
            private readonly static int hashcode = typeof(Array).GetHashCode();
            public Array(TypeDescriptor baseType)
            {
                BaseType = baseType;
            }

            public TypeDescriptor BaseType { get; }

            public override bool Equals(TypeDescriptor other)
                => Equals(other as Array);
            public bool Equals(Array other)
                => other != null && BaseType.Equals(other.BaseType);
            public override int GetHashCode()
            {
                int b = BaseType.GetHashCode();
                return (b << 5) ^ (b >> 27 & 0x1f) ^ hashcode;
            }
        }
        public class Record : TypeDescriptor
        {
            private readonly static int hashcode = typeof(Record).GetHashCode();
            public Record(IEnumerable<PropertyDescriptor<TypeDescriptor>> properties)
            {
                Properties = properties.ToDictionary(x => x.Name);
            }
   
            public IReadOnlyDictionary<string, PropertyDescriptor<TypeDescriptor>> Properties { get; private set; }

            public override bool Equals(TypeDescriptor other)
                => Equals(other as Record);
            public bool Equals(Record other)
                => other != null
                && Properties.Values.OrderBy(p => p.Name).SequenceEqual(other.Properties.Values.OrderBy(p => p.Name));
            public override int GetHashCode()
            {
                var res = hashcode;
                foreach (var p in Properties.Values.OrderBy(p => p.Name))
                    res = (res << 7) ^ (res >> 25 & 0x7f) ^ p.GetHashCode();
                return res;
            }
        }
        public class Null : TypeDescriptor
        {
            private readonly static int hashcode = typeof(Null).GetHashCode();
            public static Null Instance { get; }

            public override bool Equals(TypeDescriptor other)
                => other is Null;
            public override int GetHashCode()
                => hashcode;
        }
        public class Value : TypeDescriptor
        {
            private readonly static int hashcode = typeof(Value).GetHashCode();
            public Value(TypeDescriptor baseType, object value)
            {
                BaseType = baseType;
                InnerValue = value;
            }

            public TypeDescriptor BaseType { get; }
            public object InnerValue { get; }

            public override bool Equals(TypeDescriptor other)
                => Equals(other as Value);
            public bool Equals(Value other)
                => BaseType.Equals(other.BaseType) && Equals(InnerValue, other.InnerValue);
        }
        public class Union : TypeDescriptor
        {
            private readonly static int hashcode = typeof(Union).GetHashCode();
            public Union(IEnumerable<TypeDescriptor> options)
            {
                Options = options.ToArray();
            }

            public IReadOnlyCollection<TypeDescriptor> Options { get; }

            public override bool Equals(TypeDescriptor other)
                => Equals(other as Union);
            public bool Equals(Union other)
                => Options.Count == other.Options.Count &&  Options.OrderBy(o => o.GetHashCode()).SequenceEqual(other.Options.OrderBy(o => o.GetHashCode()));
            public override int GetHashCode()
            {
                var res = hashcode;
                foreach(var o in  Options.OrderBy(x => x.GetHashCode()))
                    res = (res << 11) ^ (res >> 21 & 0x7ff) ^ o.GetHashCode();
                return res;
            }
        }

        public abstract bool Equals(TypeDescriptor other);
        public override bool Equals(object obj)
            => Equals(obj as TypeDescriptor);
        public static bool operator == (TypeDescriptor x, TypeDescriptor y)
            => ReferenceEquals(x,y) || !ReferenceEquals(x,null) && x.Equals(y);
        public static bool operator !=(TypeDescriptor x, TypeDescriptor y)
            => !ReferenceEquals(x, y) && (ReferenceEquals(x,null) || !x.Equals(y));
        public override int GetHashCode()
            => base.GetHashCode();
    }
}
