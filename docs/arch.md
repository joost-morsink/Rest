# Architecture
The solution contains two main library projects:
* Biz.Morsink.Rest 
* Biz.Morsink.Rest.AspNetCore

The first is a library that supports creating RESTful interfaces, in a protocol-agnostic way.
The second library supports using this library with ASP.Net core 2.0.
We will discuss the architectures of both projects separately, as well as any extensibilty options.

## Rest

### Architectural constraints
Rest is an architectural style for services that have the following properties (ref: [Architectural constraints](https://en.wikipedia.org/wiki/Representational_state_transfer#Architectural_constraints))
* Client-Server (Request-Response)
* Stateless
* Cacheable
* Layered
* Code on demand
* Uniform interface
  * Resource identification
  * Resource manipulation through representations
  * Self-descriptive messages 
  * Hypermedia as the engine of application state

Before we can discuss these architectural styles we state the structure of a RestRequest in this library:

```csharp
class RestRequest
{
    string Capability { get; }
    IIdentity Address { get; } 
    Func<Type, object> BodyParser { get; }
    RestParameterCollection Parameters { get; }
    TypeKeyedDictionary Metadata { get; }
}
```

#### Client-Server
This library focusses on the server part of the RESTful service. 
The client is assumed to exist.

#### Stateless
The library does not address statelessness, but it is encouraged by not having any session related classes defined.
Also, the assumption of statelessness is made in the library, making it unfit for usage in stateful scenario's.
A consumer of the library could 'cheat' by implementing their own state system, but it may break the inner workings of the library.

#### Cacheable
Caching metadata can be added to every response, allowing for a caching middleware component to handle caching.
**At the time of writing, this component still needs to be developed**
Metadata is propagated to the layer above, so it can be implemented per the protocol required.

#### Layered
This constraint is an external constraint and can be satisfied by proper implementation.

#### Code on demand
This constraint is dependent on the specific protocol used to expose the Rest service and can be satisfied by the consuming library.

#### Uniform interface
##### Resource identification
The `Address` identifies a resource uniquely.
For this property, the Identity library is used to ease manipulation of these values.

##### Resource manipulation through representations
This constraint basically boils down to two distinct elements:
* Representation: Everything is an object in C#, so all entities/resources are represented by a (typed) object.
* Manipulation: This is just a type of `Capability`.

##### Self-descriptive messages
Responses that contain a resource, contain a typed object.
The type of the object is a description for the object, making the object self-descriptive.
Of course some translation needs to be made when the object itself is translated to a more specific format.

Internally the type of a message is converted to a `TypeDescriptor` instance that tries to describe the datatype in a formal way.
Obviously there may be inconsistencies between the C#/.Net type system and a formal mathematical one, but these will be mitigated on a case-by-case basis.

##### Hypermedia as the engine of application state
HATEOAS as it is often abbreviated is the constraint that the entire application can be navigated by hypermedia.
This means there must be a path to every resource in the service. 
This constraint is satisfied by two concepts:
* The `Home` resource. 
  This resource maps to the home-path or homepage of the service.
  A Home resource is mandatory for this library to be used in a RESTful context but does not need to contain much data as a type.
  However, it should produce links to all the first-level services.
* `Link`s.
  `Link`s are references of a certain type (`Reltype`) to resources (`Address`), paired with a operation (`Capability`).
  This only determines part of the Rest request, but the other information might be dynamic. 
  However, type information should be known about the capability, which can be retrieved using CapabilityDescriptors for the operation.

### Extensibility
Obviously this library needs to be used in-process, as it does not define any network functions. 
With the Biz.Morsink.Rest.AspNetCore library, the Rest functionality can be exposed over HTTP. 
However, due to the strict separation of HTTP and non-HTTP logic, we could provide a RESTful interface over other kinds of protocol by implementing a library for that protocol.

The actual RESTful service implementation is also an extensibility point of this library, allowing services to be protocol-agnostic as well.

Key extensibility points are:
* `IRestRequestHandler`, by consumption in the protocol-aware library.
* `IRestRepository`, by implementation in services libraries.
* `ILinkProvider` and `IDynamicLinkProvider`, by implementation in services libraries.
* `IAuthorizationProvider`, by implementation in a security library.

## ASP.Net core

