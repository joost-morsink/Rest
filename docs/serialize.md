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