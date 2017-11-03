# Type Descriptors
Type descriptors are objects that describe types using a type system.
The type system will be explained here, as well as patterns and transformation techniques for C# objects.

Type descriptors are used to construct schemas for concrete serialization types.

## Formal definition
A TypeDescriptor is a disjoint union type, and can be any its subtypes. 
The constructor is private and can only be accessed by nested classes. 
All abstract nested classes have private constructors, and all non-abstract classes are sealed.
This way we can be certain that the set of known subtypes is a complete set.

The subtype hierarchy is as follows:
* TypeDescriptor
  * Primitive
    * Number
      * Float
      * Integral
    * String
    * Boolean
    * DateTime
  * Array
  * Record
  * Null
  * Value
  * Union
  * Intersection
  * Reference

### Primitives
A primitive value is just that: a primitive value.
It is either a number (float or integer), string, boolean or date-time.

### Array
An array is 0 or more elements of a certain type (with its own TypeDescriptor).
For instance, an array of integers is Array(Integral), and an array of strings is Array(String).

### Record
This is where it gets a bit more interesting. 
A record is a name and a collection of key-value mappings, called properties.
Each property has the following 'properties':
* Name (key of the property)
* Value (For each key a TypeDescriptor)
* Required (A boolean indicating whether the _presence_ of the property is required)

Note that the presence of properties is not the same as nullability.

### Null
This type only contains the null value, and other types explicitly do not allow null values.
The only way to construct a nullable type is to make a union of the type with this type.
Union types are explained in the Union section.

### Value
This type matches only one specific value for some underlying type.
This type can also be used with the Union type to construct enumerations.

### Union
A union type is a name and a specification that a value may belong to any of the underlying options. 
The constraint is that the value should match _at least one_ option, so the options do not have to be mutually exclusive.

Examples of unions:
* Direction: 

  `Union(Direction,[Value("North",String), Value("South",String), Value("West",String), Value("East",String)])`

* Nullable string: 

  `Union(NullableString, [String,Null])`

* Array of nullable numbers: 

  `Array(Union(NullableNumber, [Float,Integral,Null]))`

### Intersection
An intersection is the dual of a Union.
It is a name combined with multiple parts, to which the value _must all_ match. 
It is useful to model type inheritance by specifying the base class separately from the part that is derivation-specific.

### Reference
A reference is a reference to a TypeDescriptor by name.
It is dependent on external data (TypeDescriptor repository of some sorts) being present that knows about the referenced type.

