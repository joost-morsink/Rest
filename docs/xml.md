# XML converter
The `Biz.Morsink.Rest.HttpConverter.Xml` project implements (currently under construction) an xml serialization format for the Rest library and ASP.Net Core.

## Schema generation
Type descriptors are a very mathematical way of defining types (data schemas).
JSON Schema is a very mathematical schema language, so the translation is quite straightforward.
XML Schema (XSD) however is a bit trickier, because it is a document schema.

Because TypeDescriptors work by intersecting constraints upon the Anything type, and XML Schema works by specifying possibilities upon the Nothing type, there is an incompatibility.
It is not even possible to define intersections in XSD.

Generating XML schemas is considered a best-effort. 
The section below deals with the choices in determining what is considered 'best'.

### Arrays
Arrays consist of possibly zero or multiple items, so the output for such a value should be typed as a fragment.
If an array is assigned to a property of a record, the Array has the property name as element name, and the item elements can be named either statically (like 'add'), or after the type of the item.

### Records
'Records' constrain the 'Anything' type to values of record shape, with every property specified in the record constraining the property in the value. 
This means if we do not have a property defined in a record, it could be present and may be any value.
For the XML Schema of records this means we have the following possibilities:

* We can use an XSD `sequence`, forcing an element order not part of the `TypeDescriptor`.
  * We can add an XSD `any` with `maxOccurs=unbounded`, to allow unconstrained extra properties. 
    They should be both at the beginning and the end.
  * We could also specify _some_ element order, but not enforce the ordering constraint.
* We can use an XSD `all`, solving the element order problem (not relevant), but we have no means to contain unconstrained properties.

It seems best to use the `sequence` with the `any` and not enforce the ordering constraint.

### Nulls
Nulls need to be integrated into the declaration site of the types it 'unions' with.
Nulls should use the XML Schema Instance nil construct.

### Intersections
As intersections are not supported in XML Schema, the contents of the sub-schemas need to be reiterated. 
In the case a sub-schema is a reference, the referred type needs to be available in the context.
If intersection is used for object oriented extension, XML Schema extension might be an option. 
Although detecting the extension structure might prove to be difficult, because the OOP information is forgotten when translating an extended type to a `TypeDescriptor`.

### References
Recursive `TypeDescriptor`s need references to keep the schema finite. 
These references are self-references and the referred type is therefore automatically within the context.
Arbitrary references should be resolved using imports or includes, although this cannot be done when the definitions of these references are needed at the referral site.



