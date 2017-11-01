# Architecture
The solution contains two main library projects:
* Biz.Morsink.Rest 
* Biz.Morsink.Rest.AspNetCore

The first is a library that supports creating RESTful interfaces, in a protocol-agnostic way.
The second library supports using this library with ASP.Net core 2.0.
We will discuss the architectures of both projects separately, as well as any extensibilty options.

## Rest {#Rest}

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
**At the time of writing, this component is in development.**
Metadata is propagated to the layer above, so it can be implemented per the protocol required.

#### Layered
This constraint is an external constraint and can be satisfied by proper infrastructural implementation.

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
HATEOAS as it is often abbreviated, is the constraint that the entire application can be navigated by hypermedia links.
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

Every address also optionally supports the OPTIONS capability. 
This type of request returns a value with TypeDescriptors for every aspect of every supported capability on the resource.

### Request Pipeline
The Rest Request pipeline is defined by a single method signature, found in an interface as well as a delegate:

```csharp
delegate ValueTask<RestResponse> RestRequestHandlerDelegate(RestRequest request);
interface IRestRequestHandler
{
    ValueTask<RestResponse> HandleRequest(RestRequest request);
}
```

A `RestRequest` contains all data needed to process the request. 
The return value indicates possible asynchrony through the `ValueTask<>` [functor](https://en.wikipedia.org/wiki/Functor).
The response side is a three layered value, each with its own aspects and containment of the layer below:
* `RestResponse` is the top layer, containing metadata for the response
* `RestResult` is effectively a [disjoint union type](https://en.wikipedia.org/wiki/Tagged_union) to allow indicating success and failure.
* `RestValue` represents the actual underlying value in a response, optionally containing links and embedded objects.

The main implementation of the `IRestRequestHandler` interface is the `CoreRestRequestHandler`.
This component tries to resolve the RestRequest to an instance of an `IRestRepository` through a `IServiceProvider` instance.

The `IRestRepository` supports capability discovery through the `GetCapabilities` method.
The capability descriptors returned are able to create a delegate to be called by the `CoreRestRequestHandler`.
The actual Rest operation implementing method can then take over an execute its logic.

The `IRestRequestHandlerBuilder` interface can be used to create a pipeline of partial handlers, to which the `CoreRestRequestHandler` can be the most inner handler.

### Capabilities
Rest capabilities can be specified and implemented freely.
But, since the most RESTful architectures have been implemented using HTTP, the main HTTP method have been specified as Rest capabilities. 
They are:

| Interface   | Method | Safe | Idempotent |
| ----------- | ------ | ---- | ---------- |
| IRestGet    | GET    | Yes  | Yes        |
| IRestPut    | PUT    | No   | Yes        |
| IRestPost   | POST   | No   | No         |
| IRestDelete | DELETE | No   | Yes        |

#### IRestGet
A Get is the operation to use when retrieving resources. 
It is a method with a resource address and optionally parameters, but no body.
A request can always be made multiple times without having any side effect on the service.
In other words, making the request multiple times has the same effect as not making it at all.

The response body type corresponds with the entity type specified by the address (identity of the resource).

#### IRestPut
A Put is the operation to use to update a resource with a new version using the resource's representation.
It is a method with a resource address an optionally parameters.
The body type corresponds with the entity specified by the address (identity of the resource).
The operation is idempotent, which means making the request multiple times has the same effect as making it once.

The response body type corresponds with the entity type specified by the address (identity of the resource).
The response itself should be a representation of the resource as it is after the operation succeeded.

#### IRestPost
A Post is the operation to use when adding new resources, or making a request that represents a 'call' of some sorts to a resource.
It is a method with a resource address an optionally parameters, it also has a body (the message to post).
The operation may have side effects.
Making the request multiple times may have undesired side-effects.

The response type can be any type.

#### IRestDelete
A Delete is the operation to use to delete a resource.
It is a method with a resource address and optionally parameters.
The operation is idempotent, which means making the request multiple times has the same effect as making it once.
This does not mean the response is the same on all requests (After deletion, the resource might 'not be found').

The response type can be any type.

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

## ASP.Net core {#ASP}
The Biz.Morsink.Rest.AspNetCore is a library that provides a binding between ASP.Net Core 2.0 and the Rest library.
It adds the following important concepts that are specific to ASP.Net Core:
* `IRestIdentityProvider`.
  Extends `IIdentityProvider` with specific methods for parsing paths and converting identity values to paths. 
* `IHttpRestRequestHandler`.
  Defines the pipeline interface for the translation of an `HttpRequest` to a `RestRequest`, as well as the translation of the `RestResponse` to the `HttpResponse`.
* `IHttpRestConverter`.
  Defines the way serialization formats need to be implemented.
  At the time of writing a JsonHttpConverter library takes care of a general JSON serialization format.

### Architectural constraints

#### Client-server
HTTP is a Client-server protocol.

#### Cacheable
Caching metadata is supported through HTTP headers.

#### Layered
As the implementation protocol is HTTP there is enough proof of layered architecture present on present-day internet.

#### Code on demand
Multiple options exists, but all of them should be implementable by extensions on this library.
The most obvious option is to allow javascript to be passed along to the client when a request is made for text/html. 
This is of course also the most platform independent way of doing so.

Extensions could however implement any type of code on demand distribution.

#### Uniform interface
##### Resource identification
HTTP uses paths to address resources. 
Translation between paths and identity values is made through a `IRestIdentityProvider`.

##### Resource manipulation through representations
Representation can be done in any serialization format, because HTTP is agnostic to the type of message that is sent.
Representation is handled by implementations of the `IHttpRestConverter` interface.
JSON and XML are popular serialization formats, and both have an implementation of that interface present in the solution. 
More specific usage of a serialization format could be implemented to support extra RESTful features.

##### Self-descriptive messages
Descriptiveness is dependent upon serialization format.
Both JSON and XML have a definition of schema, which can be used to make the messages self-descriptive. 
Using a HTTP header `Link` and the reltype 'describedby' a link to the schema definition for a message can be given.
A translation between `TypeDescriptor`s and schema's is an essential element of implementing a `IHttpRestConverter`. (See [HTTP Converters](./httpConv.md))

##### Hypermedia as the engine of application state
* The `Home` resource should map to the root path of the api: `/`.
* `Link`s should be paths, combined with method and optionally parameter and body schema information.

Links can be passed along with the resources using the standard `Link` HTTP header.

The OPTIONS capability can be accessed using the OPTIONS method, which not only returns all the allowed verbs on the resource, but also contains a body with schema information about all the capabilities.

The only _out of band_ information needed to discover the entire service is:
* Knowledge of the HTTP protocol (generic).
* Knowledge of some standard headers of the HTTP protocol.
* Knowledge of a supported serialization format.

### Dependency injection
The Rest library is setup with dependency injection in mind, but it does not explicitly use any specific technology.
ASP.Net Core has some machinery setup to deal with dependency injection, including using the IoC container of your choice. 
The Rest for ASP.Net core library only uses the generic interface of dependency injection and should be compatible with any IoC container a service implementor could choose.
Because the `Lazy<>` functor is not supported by the ASP.Net Core IoC container out of the box, a `IServiceProvider` reference may be used to break circular dependency chains.

### Request Pipeline
The library needs to hook into the ASP.Net request pipeline to handle HTTP requests.
This may be accomplished by using the `UseRest` And `AddRest` extension methods.

The HTTP request is inspected with the purpose of resolving to a `IHttpRestConverter` implementation, which is accompanied with the `HttpContext` and a starter `RestRequest` for further manipulation in a `IHttpRestRequestHandler`. 
This is another pipeline component in which everything about the HttpRequest that is needed in a Rest context is transformed into the RestRequest. 
The end of this pipeline is the start of the `IRestRequestHandler`.
After the `IRestRequestHandler` is done processing the message, the response is fed through the pipeline again, and in the end the response is written to the `HttpResponse` using the `IHttpRestConverter`.

### Extensibility
Extensibility is accomplished mainly through implementation of serialization formats (implementation of `IHttpRestConverter`).

