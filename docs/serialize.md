# Serialization
The core library contains helper classes for uniform serialization and deserialization of data.
An intermediate serialization format is defined by a class hierarchy, and an extensible serializer takes care of converting objects to and from this intermediate format.

## Class hierarchy
The bullet list below illustrates the class hierarchy underlying the intermediate serialization format, with data parameters within parentheses:

* `SItem`
  * `SObject`(`SProperty`*)
  * `SArray`(`SItem`*)
  * `SValue`(`object`)
* `SProperty`(`string`, `SItem`)

It is a fairly minimal structure, but it should be able to model any type.
However, it is not designed to have a one to one mapping with every concrete serialization format. 
There is a straightforward mapping to Json-based types, but constructing a mapping for XML requires making some choices.

## Serializer
A serializer is able to serialize and deserialize objects to and from the intermediate format (`SItem`), within the context of a serialization context `C : SerializationContext<C>`.
The serialization context may be used for keeping track of embeddings and entities in the current serialization chain.
It depends heavily on the `IHasIdentity` and `IIdentity` interfaces for identifying entities.
Circular references can be solved by using a correct implementation, but only for objects using these interfaces.

## HTTP Converters
Currently the Json and Hal+Json converters use the intermediate serialization format. 
Objects serialized using these converters should behave uniformly, unless the media type dictates otherwise. 
These serializers should also have the most advanced/up-to-date feature sets available.

The Xml converter has not been converted to use the intermediate format.
Because of the absence of a canonical mapping mentioned earlier, this might never be implemented. 
Support for some advanced features may therefore be unsupported for the Xml format.

The Html converter does not support deserialization and is able to use a quite straightforward serialization algorithm.
It generates tables for objects.

## Binary representation
The `SItem` family of classes can be serialized to a binary representation. 
The format is a _type-length-value_ format for primitive types and a _type-value_ format for structured types. 
Little endian encoding is used for primitive values.

| Type byte | Type            | Includes length | Encoding | Termination    |
| --------- | --------------- | --------------- | -------- | -------------- |
| 0x00      | Null            | No              |          | None           | 
| 0x01      | SObject         | No              |          | 0x02 type byte |
| 0x03      | SArray          | No              |          | 0x04 type byte |
| 0x10      | Short String    | ubyte           | UTF8     |                |
| 0x11      | String          | ushort          | UTF8     |                |
| 0x12      | Long String     | uint            | UTF8     |                |
| 0x20      | Blob            | uint            |          |                |
| 0x30      | Int             | ubyte           |          |                |
| 0x31      | Uint            | ubyte           |          |                | 
| 0x32      | Float           | ubyte           |          |                |
| 0x33      | Decimal         | ubyte (16)      |          |                | 
| 0x40      | DateTime        | ubyte (8)       |          |                |
| 0x41      | DateTimeOffset  | ubyte (16)      |          |                | 
