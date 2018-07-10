# Hal converter
The `Biz.Morsink.Rest.HttpConverter.HalJson` project implements a HAL json format for the Rest library and ASP.Net Core.

The HAL format is documented informally [here](http://stateless.co/hal_specification.html).
It aims to be a Rest level 3 specification for GETs only. 

The HAL converter does not expose any type or schema information. Everything should be discovered while navigating the API.

## Serialization
Serialization and deserialization work by constructing JToken objects. 
This means the Json is parsed first, and then converted to an object. Or a Json syntax tree is constructed first, before writing it to some output stream.
This enables more dynamics within the (de-)serialization process needed for certain aspects of Hal.

Threaded through the (de-)serialization process is a `HalContext` class to keep track of some elements needed for (de-)serialization.

### HalContext
The `HalContext` object threaded through the (de-)serialization process keeps track of _embedded_ objects in lexical `RestValue` scopes. 
When serialization encounters an object with the same `Identity` as one in the embeddings, the object is only referred to.


