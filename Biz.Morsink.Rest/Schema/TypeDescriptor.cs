using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Biz.Morsink.Rest.Schema
{
    /// <summary>
    /// This abstract class represents a resource type.
    /// A resource type is a definition of an entity structure, which is serializable.
    /// </summary>
    public abstract class TypeDescriptor : IEquatable<TypeDescriptor>
    {
        /// <summary>
        /// This abstract class represents primitive types.
        /// A primitive type is not parameterized in a recursive way and often has a specific syntax.
        /// </summary>
        public abstract class Primitive : TypeDescriptor
        {
            /// <summary>
            /// This class represents the string type.
            /// </summary>
            public class String : Primitive
            {
                private readonly static int hashcode = typeof(String).GetHashCode();
                /// <summary>
                /// Singleton instance for String.
                /// </summary>
                public static String Instance { get; } = new String();

                public override bool Equals(TypeDescriptor other)
                    => other is String;
                public override int GetHashCode()
                    => hashcode;
            }
            /// <summary>
            /// This abstract class represents numeric types.
            /// </summary>
            public abstract class Numeric : Primitive
            {
                /// <summary>
                /// This class represents floating point numeric types.
                /// </summary>
                public class Float : Numeric
                {
                    private readonly static int hashcode = typeof(Float).GetHashCode();
                    /// <summary>
                    /// Singleton instance for Float.
                    /// </summary>
                    public static Float Instance { get; } = new Float();

                    public override bool Equals(TypeDescriptor other)
                        => other is Float;
                    public override int GetHashCode()
                        => hashcode;
                }
                /// <summary>
                /// This class represents integral numeric types.
                /// </summary>
                public class Integral : Numeric
                {
                    private readonly static int hashcode = typeof(Integral).GetHashCode();
                    /// <summary>
                    /// Singleton instance for Integral.
                    /// </summary>
                    public static Integral Instance { get; } = new Integral();
                    
                    public override bool Equals(TypeDescriptor other)
                        => other is Integral;
                    public override int GetHashCode()
                        => hashcode;
                }
            }
            /// <summary>
            /// This class represents DateTime values.
            /// </summary>
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
        /// <summary>
        /// This class represents enumerated collection types.
        /// It is parameterized by an elements type.
        /// </summary>
        public class Array : TypeDescriptor
        {
            private readonly static int hashcode = typeof(Array).GetHashCode();
            /// <summary>
            /// Constructor.
            /// </summary>
            public Array(TypeDescriptor elementType)
            {
                ElementType = elementType;
            }
            /// <summary>
            /// Gets the element type descriptor for this Array type.
            /// </summary>
            public TypeDescriptor ElementType { get; }

            public override bool Equals(TypeDescriptor other)
                => Equals(other as Array);
            public bool Equals(Array other)
                => other != null && ElementType.Equals(other.ElementType);
            public override int GetHashCode()
            {
                int b = ElementType.GetHashCode();
                return (b << 5) ^ (b >> 27 & 0x1f) ^ hashcode;
            }
        }
        /// <summary>
        /// This class represents records.
        /// A record is an unordered collection of (unique) key to value mappings.
        /// The Record type specifies for each 'property' (=key) the name, type and whether it is required in the record.
        /// </summary>
        public class Record : TypeDescriptor
        {
            private readonly static int hashcode = typeof(Record).GetHashCode();
            /// <summary>
            /// Constructor.
            /// </summary>
            /// <param name="properties">The property descriptors for all properties of the record.</param>
            public Record(IEnumerable<PropertyDescriptor<TypeDescriptor>> properties)
            {
                Properties = properties.ToDictionary(x => x.Name);
            }
   
            /// <summary>
            /// A dictionary containing mappings from property names to property descriptors.
            /// </summary>
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
        /// <summary>
        /// This class represents the null type, having only the value null as its member.
        /// </summary>
        public class Null : TypeDescriptor
        {
            private readonly static int hashcode = typeof(Null).GetHashCode();
            /// <summary>
            /// Singleton instance for Null.
            /// </summary>
            public static Null Instance { get; } = new Null();

            public override bool Equals(TypeDescriptor other)
                => other is Null;
            public override int GetHashCode()
                => hashcode;
        }
        /// <summary>
        /// This class represents a value as a type, having only the specified value as its member.
        /// </summary>
        public class Value : TypeDescriptor
        {
            private readonly static int hashcode = typeof(Value).GetHashCode();
            /// <summary>
            /// Constructor.
            /// </summary>
            /// <param name="baseType">The base type of the value.</param>
            /// <param name="value">The actual value.</param>
            public Value(TypeDescriptor baseType, object value)
            {
                BaseType = baseType;
                InnerValue = value;
            }
            /// <summary>
            /// Gets the type descriptor for the type value.
            /// </summary>
            public TypeDescriptor BaseType { get; }
            /// <summary>
            /// Gets the actual only member value fot this type.
            /// </summary>
            public object InnerValue { get; }

            public override bool Equals(TypeDescriptor other)
                => Equals(other as Value);
            public bool Equals(Value other)
                => BaseType.Equals(other.BaseType) && Equals(InnerValue, other.InnerValue);
        }
        /// <summary>
        /// This class represents union types.
        /// Union types are types that are at least one of their options. 
        /// (i.e. a value is a of a union type if there is an option of the union that the value is a member of as well.)
        /// </summary>
        public class Union : TypeDescriptor
        {
            private readonly static int hashcode = typeof(Union).GetHashCode();
            /// <summary>
            /// Constructor.
            /// </summary>
            /// <param name="options">A collection of the options for this union type.</param>
            public Union(IEnumerable<TypeDescriptor> options)
            {
                Options = options.ToArray();
            }
            /// <summary>
            /// The options for this union type.
            /// </summary>
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
