# Type Descriptors
Type descriptors are objects that describe types using a type system.
The type system will be explained here, as well as patterns and transformation techniques for C# objects.

Type descriptors are used to construct schemas for concrete serialization types.

## Formal definition
A TypeDescriptor is a disjoint union type, and can be any of its subtypes. 
The constructor is private and can only be accessed by nested classes. 
All abstract nested classes have private constructors, and all non-abstract classes are sealed.
This way we can be certain that the set of known subtypes is a complete set.

The subtype hierarchy is as follows:
* TypeDescriptor
  * Any
  * Primitive
    * Number
      * Float
      * Integral
    * String
    * Boolean
    * DateTime
  * Array
  * Record
  * Dictionary
  * Null
  * Value
  * Union
  * Intersection
  * Reference
  * Referable

TypeDescriptors constrain other types in some way, ultimately constraining a 'Anything' type containing all possible values. 
The absence of a descriptor means any value (Anything) and the presence of multiple descriptors should be interpreted as an intersection of those types.
At the other end of the type spectrum there is 'Nothing', a type that is not inhabited by any value.

### Any
An any could be anything. 
It is a type descriptor that represents the absence of any constraints.

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

Note that the presence of properties is not the same as nullability of their values.

### Dictionary
A dictionary is a collection of key-value mappings. 
The key type should always be a string type, but the value type is a parameter to the dictionary type.

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

### Referable
A referable is a lazily evaluated TypeDescriptor, with a reference name attached. 
Consumers can choose whether to evaluate the actual TypeDescriptor or to use the reference name.

## Schema
`TypeDescriptors` are generic representations of types. 
As such, they can be translated into format specific schemas.
For instance, JSON has a [JSON Schema standard](http://json-schema.org/) and the `Biz.Morsink.Rest.HttpConverter.Json` project defines a `JsonSchemaTypeDescriptorVisitor` that is able to translate any `TypeDescriptor` into a JSON schema.
Each serialization format should be able to convert `TypeDescriptor`s to schema, and the visitor pattern is there to help with the implementation.

### JSON
JSON schema is a very compatible schema type, because of the way it is set up.
An empty schema accepts _any_ JSON, and every element added to the schema adds a constraint.
This allows for a very clean mathematical specification of datatype constraints.

JSON as a format closely resembles data types defined in other languages (like .Net).
This is also a very good reason why its schema is very compatible to `TypeDescriptor` instances.

### XML
XML on the other side has many ways of specifying schemas, the most popular being XSD (Xml schema definition).
An empty schema accepts _no_ XML, and everything added to the schema adds either more possibilities or more constraints to the datatype.
This means specifying XML structure using XSD is not very mathematical in nature.

XML is a very powerful _document_ format, when used for data only a small subset is needed.
There is no best, or canonical, way of determining how a `TypeDescriptor` should be converted to an XSD, or how an object of the type described should be serialized to XML.
Of course it is possible to define a few ways of doing it.

## Canonical type transformation
Types are generally transformed to `TypeDescriptor`s by a generic algorithm, called the canonical type transformation. 
In cases where the canonical type transformation is not adequate, an override mechanism called 'Representations' can be used (see below).
A `TypeDescriptorCreator` instance can be used to 

### Primitives
All primitive types have a canonical type descriptors, shown in the table below:

| Primitive type | TypeDescriptor                       |
| -------------- | -------------------------------------|
| string         | String                               |
| sbyte          | Integral                             |
| byte           | Integral                             |
| short          | Integral                             |
| ushort         | Integral                             |
| int            | Integral                             |
| uint           | Integral                             |
| long           | Integral                             |
| ulong          | Integral                             |
| decimal        | Float                                |
| float          | Float                                |
| double         | Float                                |
| bool           | Boolean                              |
| DateTime       | DateTime                             |
| Object         | Record (with 0 properties)           |

These descriptors are fixed and cannot be changed. 
Please note that a decimal is also a floating point number that just happens to be exact in a base 10 system.
The Float `TypeDescriptor` is not supposed to be interpreted as having accuracy or inaccuracy.

### Type forms
If a registration of a certain type is not yet present, the creator must create a descriptor, and it does so by checking whether types fit a particular form.
At the moment the following forms are checked:

* Nullability
* Dictionaries
* Sequential collections
* Disjunct union types
* Records
* Units

#### Nullability
The nullability check is a check whether the type is of the form `Nullable<T>` for some type `T`.
The type constraint on `Nullable<T>`make `T` a struct type.
The result is a union of the `Null` type and the type descriptor for `T`.

#### Dictionaries
Dictionaries are key-value mapping collections.
They implement `IDictionary<string,V>` for some value type `V`. 
The first implementation of `IDictionary<string, V>` is used to describe the item type.
Instances of the non-generic `IDictionary` can also be used and is treated as an `IDictionary<string,object>`.

#### Sequential collections
Sequential collections include types that implement `IEnumerable`, but don't implement `IDictionary<K,V>` for any `K` and `V`.
Sequential collections are described as the `Array` type.
The first implementation of `IEnumerable<T>` is used to describe the item type. 
If such an implementation is not found, `object` and as such an any type descriptor is used as the item type.

#### Disjunct union types
A disjunct union type is an abstract class containing (nested) derived classes. 
The type descriptor generated for these types is a `Union` over all the derived public classes.
When the base class contains relevant state, an `Intersection` of the base class and the `Union` of the cases is returned. 

F# union types with more than one case should almost be covered by this form. 
Although it must be noted that these types contain no public constructors, only static construction methods. 
There is also the issue of the `Tag` property on the base class, which does not contain properly formatted data.

F# union types with a single case are just some decoration over an existing type.
They are isomorphic to the inner type of the case of the union and, ideally, they should be treated as such.

F# union types are annotated with a `CompilationMapping(SourceConstructFlags.SumType)`.
Based on this criterion a specialized form should be created.

> **TODO**: A specialized F# union type form should be designed to handle these types.
> HttpConverters should implement proper serializers and deserializers to adhere to the generated descriptors.

#### Records
There are two kinds of record forms supported by the `TypeDescriptorCreator` mechanism.

First of all, regular DTO's are supported.
These should have a parameterless constructor and properties with getters and setters.
For each property there will be a property in the `Record`.

Second, immutable DTO's are supported.
These should have only readonly properties, and a constructor with parameters.
The parameter names should match the property names (case-insensitive, allowing idiomatic casing) and the parameter types should also match the property types.
Each property will result in a property in the `Record`, just as with regular DTO's.

#### Unit
If a type contains a parameterless constructor and does not contain any properties, the type is considered to be isomorphic to the unit type.
Unit types are described as a `Record` with 0 properties, basically type without constraints.

## Representations
Sometimes a type is not the best candidate for a serialization format.
In that case you can _represent_ the type by another type.
We say the _representable_ type is represented by the _representation_ type.

A very good example is the `Identity<T>` types. 
They contain a lot of information that is very useful in an in-process context.
However, it is not information that you would want to be serialized. 
Every type of `T` in `Identity<T>` must be known by the Rest Identity Provider, allowing the identity value to be converted into a URL.
This URL is part of the representation type for identity values. (`Href` property)

The implementation of type representations is made through the `ITypeRepresentation` interface, which is defined as follows:

```csharp
interface ITypeRepresentation
{
    bool IsRepresentable(Type type);
    bool IsRepresentation(Type type);
    Type GetRepresentationType(Type type);
    Type GetRepresentableType(Type type);
    object GetRepresentation(object obj);
    object GetRepresentable(object rep);
}
```

The type representation implementation supports querying representation eligibility through the `IsRepresentable` and `IsRepresentation` methods. 
It supports querying the representable or representation types responding to each other through the `GetRepresentationType` and `GetRepresentableType`, because the information is needed for serialization and de-serialization.
The actual conversion is implemented in the `GetRepresentation` and `GetRepresentable` methods.

The type representations are injected into the Http converters at runtime, and queried when necessary.
