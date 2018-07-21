# Embeddings
Embeddings are objects that are added to a `RestValue` for some reason.
The reason could be that the embedded object might often be requested after the current request, or it might be that the embedded object occurs multiple times in the value.
In other words, these objects can be used as a potential performance gain.

## SerializationContext
`SerializationContext` is a class that helps to implement a consistent way of handling embeddings. 
It can keep track of Rest Values (containing embeddings) that have been encountered in the Scope, so embeddings can be looked up.
It also can keep track of objects that already are being serialized in a current parent-chain.
Both these aspects come in handy in different scenarios, as described in the next section.

## Implementation
The actual implementation and corresponding implications of embeddings can be quite different depending on the [Http converter](httpConv.md) used.
In the section below we will elaborate on the implementations of two converters:

* The regular Json converter
* The [Hal Json converter](hal.md)

For an example in the following sections we will use the following classes:

```csharp
class Person : IHasIdentity<Person> {
    IIdentity<Person> Id;
    string FirstName;
    string LastName;
    IIdentity<Country> Nationality;
}
class Country : IHasIdentity<Country>{
    IIdentity<Country Id; // Equals Code
    string Code;
    string Description;
}
```

### Hal
In HAL, each result has a separate property (called `_embedded`) to contain the embedded objects. 
Serializing these objects in that location is very easy, but because of that you can optimize serializing the value (and embeddings themselves as well) to refer to the embeddings, whenever an embedded object is encountered during serialization.

Say we want to serialize the following object:
```csharp
new Person { 
    Id = FreeIdentity<Person>.Create(1),
    FirstName = "Joost",
    LastName = "Morsink", 
    Nationality = FreeIdentity<Country>.Create("NL")
}
```

and we add an embedded object:

```csharp
new Country {
    Id = FreeIdentity<Country>.Create("NL"),
    Code = "NL",
    Description = "The Netherlands"
}
```

we want the following serialization:

```json
{
    "id": { "href": "/person/1" },
    "firstName": "Joost",
    "lastName": "Morsink",
    "nationality": { "href": "/country/NL" },
    "_links": {
        "self": { "href": "/person/1" }
    },
    "_embedded": {
        "country": {
            "id": { "href": "/country/NL" },
            "code": "NL",
            "description": "The Netherlands" 
         }
    }
}
```

### Json
In JSON there is no separate property for embeddings, so the trick there is to serialize the embedded object whenever a reference to an embedding is encountered during serialization.

There is a mechanism in place to restrict circular embeddings.

The same Rest value as in the HAL example should serialize as:

```json
{
    "id": { "href": "/person/1" },
    "firstName": "Joost",
    "lastName": "Morsink",
    "nationality":  {
        "id": { "href": "/country/NL" },
        "code": "NL",
        "description": "The Netherlands" 
    }
}
```

The _'embedded'_ object is literally embedded into the serialized result, although it was not part of the Value part of the object graph.
