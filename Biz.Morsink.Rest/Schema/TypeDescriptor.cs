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
        public static TypeDescriptor MakeString() => Primitive.String.Instance;
        public static TypeDescriptor MakeFloat() => Primitive.Numeric.Float.Instance;
        public static TypeDescriptor MakeIntegral() => Primitive.Numeric.Integral.Instance;
        public static TypeDescriptor MakeDateTime() => Primitive.DateTime.Instance;
        public static TypeDescriptor MakeBoolean() => Primitive.Boolean.Instance;
        public static TypeDescriptor MakeNull() => Null.Instance;
        public static TypeDescriptor MakeArray(TypeDescriptor elementType) => new Array(elementType);
        public static TypeDescriptor MakeRecord(string name, IEnumerable<PropertyDescriptor<TypeDescriptor>> properties, Type associatedType) => new Record(name, properties, associatedType);
        public static TypeDescriptor MakeDictionary(string name, TypeDescriptor valueType) => new Dictionary(name, valueType);
        public static TypeDescriptor MakeValue(TypeDescriptor baseType, object innerValue) => new Value(baseType, innerValue);
        public static TypeDescriptor MakeEmpty() => MakeRecord("", Enumerable.Empty<PropertyDescriptor<TypeDescriptor>>(), null);
        public static TypeDescriptor MakeUnion(string name, IEnumerable<TypeDescriptor> options, Type associatedType)
            => options.Any()
                ? options.Skip(1).Any()
                    ? new Union(name, options, associatedType)
                    : options.First()
                : MakeEmpty();
        public static TypeDescriptor MakeIntersection(string name, IEnumerable<TypeDescriptor> parts, Type associatedType)
            => parts.Any()
                ? parts.Skip(1).Any()
                    ? new Intersection(name, parts, associatedType)
                    : parts.First()
                : MakeEmpty();
        public static TypeDescriptor MakeAny() => Any.Instance;
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="name">The name for the TypeDescriptor.</param>
        public TypeDescriptor(string name, Type associatedType = null)
        {
            Name = name;
            this.associatedType = associatedType;
        }
        private readonly Type associatedType;
        /// <summary>
        /// Gets the name of the TypeDescriptor.
        /// </summary>
        public string Name { get; }
        /// <summary>
        /// Gets the associated type of the TypeDescriptor.
        /// This value might be null!
        /// </summary>
        public Type GetAssociatedType() => associatedType;

        /// <summary>
        /// This class represents any value.
        /// </summary>
        public class Any : TypeDescriptor
        {
            private static readonly int hashcode = typeof(Any).GetHashCode();
            private Any() : base("Any", typeof(object)) { }
            public static Any Instance { get; } = new Any();
            public override bool Equals(TypeDescriptor other)
                => other is Any;
            public override int GetHashCode()
                => hashcode;

        }
        /// <summary>
        /// This abstract class represents primitive types.
        /// A primitive type is not parameterized in a recursive way and often has a specific syntax.
        /// </summary>
        public abstract class Primitive : TypeDescriptor
        {
            public Primitive(string name, Type associatedType) : base(name, associatedType) { }
            /// <summary>
            /// This class represents the string type.
            /// </summary>
            public class String : Primitive
            {
                public String() : base(nameof(String), typeof(String)) { }
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
                public Numeric(string name, Type associatedType) : base(name, associatedType) { }
                /// <summary>
                /// This class represents floating point numeric types.
                /// </summary>
                public class Float : Numeric
                {
                    public Float() : this(null) { }
                    public Float(string name) : base(name ?? nameof(Float), typeof(double)) { }
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
                    public Integral() : this(null) { }
                    public Integral(string name) : base(name ?? nameof(Integral), typeof(long)) { }
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
                public DateTime() : this(null) { }
                public DateTime(string name) : base(name ?? nameof(DateTime), typeof(DateTime)) { }
                private readonly static int hashcode = typeof(DateTime).GetHashCode();
                public static DateTime Instance { get; } = new DateTime();

                public override bool Equals(TypeDescriptor other)
                    => other is DateTime;
                public override int GetHashCode()
                    => hashcode;
            }
            /// <summary>
            /// This class represents Boolean values
            /// </summary>
            public class Boolean : Primitive
            {
                public Boolean() : this(null) { }
                public Boolean(string name) : base(name ?? nameof(Boolean), typeof(bool)) { }
                private readonly static int hashcode = typeof(Boolean).GetHashCode();
                public static Boolean Instance { get; } = new Boolean();
                public override bool Equals(TypeDescriptor other)
                    => other is Boolean;
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
                : base(string.Concat(nameof(Array), "<", elementType.Name, ">"),
                      elementType.GetAssociatedType() == null ? null : typeof(IEnumerable<>).MakeGenericType(elementType.GetAssociatedType()))
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
            public Record(string name, IEnumerable<PropertyDescriptor<TypeDescriptor>> properties, Type associatedType) : base(name, associatedType)
            {
                Properties = properties.ToDictionary(x => x.Name);
            }

            /// <summary>
            /// A dictionary containing mappings from property names to property descriptors.
            /// </summary>
            public IReadOnlyDictionary<string, PropertyDescriptor<TypeDescriptor>> Properties { get; }

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
        /// This class represents a dictionary type with free keys.
        /// The valuetype can be restricted by a TypeDescriptor.
        /// </summary>
        public class Dictionary : TypeDescriptor
        {
            private readonly static int hashcode = typeof(Dictionary).GetHashCode();
            /// <summary>
            /// Constructor.
            /// </summary>
            /// <param name="name">he name for the type descriptor.T</param>
            /// <param name="valueType">A type descriptor for the values in the dictionary.</param>
            public Dictionary(string name, TypeDescriptor valueType)
                : base(name, valueType?.GetAssociatedType() == null ? null : (typeof(IDictionary<,>).MakeGenericType(typeof(string), valueType?.GetAssociatedType())))
            {
                ValueType = valueType;
            }
            /// <summary>
            /// A type descriptor for the dictionary's values.
            /// </summary>
            public TypeDescriptor ValueType { get; }

            public override bool Equals(TypeDescriptor other)
                => Equals(other as Dictionary);
            public bool Equals(Dictionary other)
                => other != null
                && ValueType.Equals(other.ValueType);
            public override int GetHashCode()
                => hashcode ^ ValueType.GetHashCode();
        }
        /// <summary>
        /// This class represents the null type, having only the value null as its member.
        /// </summary>
        public class Null : TypeDescriptor
        {
            public Null() : base(nameof(Null)) { }
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
            /// <param name="innerValue">The actual value.</param>
            public Value(TypeDescriptor baseType, object innerValue) : base(string.Concat(baseType.Name, "=", innerValue.ToString()), baseType.GetAssociatedType())
            {
                BaseType = baseType;
                InnerValue = innerValue;
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
                => other != null && BaseType.Equals(other.BaseType) && Equals(InnerValue, other.InnerValue);
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
            public Union(string name, IEnumerable<TypeDescriptor> options, Type associatedType) : base(name, associatedType)
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
                => other != null && Options.Count == other.Options.Count && Options.OrderBy(o => o.GetHashCode()).SequenceEqual(other.Options.OrderBy(o => o.GetHashCode()));
            public override int GetHashCode()
            {
                var res = hashcode;
                foreach (var o in Options.OrderBy(x => x.GetHashCode()))
                    res = (res << 11) ^ (res >> 21 & 0x7ff) ^ o.GetHashCode();
                return res;
            }
        }
        /// <summary>
        /// This class represents intersection types.
        /// Intersection types implement all of their parts.
        /// </summary>
        public class Intersection : TypeDescriptor
        {
            private readonly static int hashcode = typeof(Intersection).GetHashCode();
            /// <summary>
            /// Constructor.
            /// </summary>
            /// <param name="parts">A collection of the parts for this intersection type.</param>
            public Intersection(string name, IEnumerable<TypeDescriptor> parts, Type associatedType) : base(name, associatedType)
            {
                Parts = parts.ToArray();
            }
            /// <summary>
            /// The parts for this intersection type.
            /// </summary>
            public IReadOnlyCollection<TypeDescriptor> Parts { get; }

            public override bool Equals(TypeDescriptor other)
                => Equals(other as Intersection);
            public bool Equals(Intersection other)
                => other != null && Parts.Count == other.Parts.Count && Parts.OrderBy(o => o.GetHashCode()).SequenceEqual(other.Parts.OrderBy(o => o.GetHashCode()));
            public override int GetHashCode()
            {
                var res = hashcode;
                foreach (var o in Parts.OrderBy(x => x.GetHashCode()))
                    res = (res << 11) ^ (res >> 21 & 0x7ff) ^ o.GetHashCode();
                return res;
            }
        }
        /// <summary>
        /// This class represents a reference to a TypeDescriptor by name.
        /// </summary>
        public class Reference : TypeDescriptor
        {
            private static readonly int hashcode = typeof(Reference).GetHashCode();
            /// <summary>
            /// Constructor.
            /// </summary>
            /// <param name="refname">The name of the reference.</param>
            public Reference(string refname) : base("&" + refname, null)
            {
                RefName = refname;
            }
            /// <summary>
            /// Gets the name of the TypeDescriptor referenced by this TypeDescriptor.
            /// </summary>
            public string RefName { get; }

            public bool Equals(Reference other)
                => other != null && RefName == other.RefName;
            public bool Equals(Referable other)
                => other != null && RefName == other.RefName;
            public override bool Equals(TypeDescriptor other)
                => Equals(other as Reference) || Equals(other as Referable);
            public override int GetHashCode()
                => hashcode ^ RefName.GetHashCode();
        }
        /// <summary>
        /// This class represents a reference to a TypeDescriptor by name, and lazily embeds the TypeDescriptor as well.
        /// </summary>
        public class Referable : TypeDescriptor
        {
            private static readonly int hashcode = typeof(Referable).GetHashCode();
            /// <summary>
            /// Constructor.
            /// </summary>
            /// <param name="refname">The name of the reference.</param>
            /// <param name="expandedDescriptor">The expanded type descriptor for the reference.</param>
            public Referable(string refname, Lazy<TypeDescriptor> expandedDescriptor, Type associatedType) : base("&+" + refname, associatedType)
            {
                RefName = refname;
                this.expandedDescriptor = expandedDescriptor;
            }
            /// <summary>
            /// Creates a new Referable based on an already expanded descriptor.
            /// </summary>
            /// <param name="refname"></param>
            /// <param name="expandedDescriptor"></param>
            /// <returns>A new referable TypeDescriptor.</returns>
            public static Referable Create(string refname, TypeDescriptor expandedDescriptor)
                => new Referable(refname, new Lazy<TypeDescriptor>(() => expandedDescriptor), expandedDescriptor?.GetAssociatedType());

            private readonly Lazy<TypeDescriptor> expandedDescriptor;
            /// <summary>
            /// Gets the name of the TypeDescriptor referenced by this TypeDescriptor.
            /// </summary>
            public string RefName { get; }
            /// <summary>
            /// Gets the actual TypeDescriptor.
            /// </summary>
            public TypeDescriptor ExpandedDescriptor => expandedDescriptor.Value;

            public bool Equals(Reference other)
                => other != null && RefName == other.RefName;
            public bool Equals(Referable other)
                => other != null && RefName == other.RefName;
            public override bool Equals(TypeDescriptor other)
                => Equals(other as Referable) || Equals(other as Reference);
            public override int GetHashCode()
                => hashcode ^ RefName.GetHashCode();
        }

        public abstract bool Equals(TypeDescriptor other);
        public override bool Equals(object obj)
            => Equals(obj as TypeDescriptor);
        public static bool operator ==(TypeDescriptor x, TypeDescriptor y)
            => ReferenceEquals(x, y) || !ReferenceEquals(x, null) && x.Equals(y);
        public static bool operator !=(TypeDescriptor x, TypeDescriptor y)
            => !ReferenceEquals(x, y) && (ReferenceEquals(x, null) || !x.Equals(y));
        public override int GetHashCode()
            => base.GetHashCode();
    }
}
