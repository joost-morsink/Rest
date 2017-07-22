# Rest server
These libraries support developing Rest services (Richardson maturity model level 3), in a protocol-agnostic way, on top of which HTTP is supported.
It tries to adhere to the original meaning of the word 'Rest', contrary to what is found on the web in abundance, something which may be called Web Api's.
Additionally the Rest services can be served over an HTTP connection by using ASP.Net Core.

## Architecture
Two main components form the backbone of a Rest API:
* A Rest component that specifies all the interfaces and helper classes needed to implement a Rest service.
* An ASP.Net Core component that exposes a Rest service implementation over HTTP.

A Rest service is responsible for answering a `RestRequest` with a `RestResponse`. 
The HTTP component is responsible for translating an `HttpRequest` to a `RestRequest`, routing that request to the proper Rest service, and translating the `RestResponse` back to a `HttpResponse`. 

## Identity
This library uses the Biz.Morsink.Identity library for identifying entities. 
Every resource in the Rest server corresponds to a type and a identity value.
There is a decoupling between the Rest pipeline and the actual serving of content over HTTP.

## Aspects
The following aspects come into play when designing a Rest repository:
* Identity (covered by the dependency).
* Home resource
* Operation (e.g. HTTP methods).
  Operation maps to operation interfaces.
* Entities and objects versus resources.
* Schema and metadata.
* Content-Type and transformation.
* Serialization/Deserialization.
  Should be configurable per content type. 
* Result type.
  * Asynchrony.
  * Lazy evaluation.
  * Status and failure (e.g. HTTP status codes/exceptions).
  * Result class.
  * Links.
  * Embedded objects.
* HATEOAS.
  * In order to be fully Rest compliant everything must be navigable by content retrieved from the Rest Server.
  * This includes manipulation methods.
* Security.
  * Based on access checks for identity values/paths.
* Navigation.
  * LinkProviders.
* Paging.
* Versioning & Deprecation.
* Localization.

### Home resource
The home resource, located at `"/"`, is the entry point for the API and provides links to all the resources that can be queried by the client.
* Either the API is fully public, so it does not need to know *who* is accessing the API.
* Or the API is fully private, so it will reply with a 401 whenever it is accessed without credentials.
  The 401 should contain a link to a login.
* Or the API is partially private.
  A 200 response should be given on accessing the home resource. 
  It should contain only links that are available to anonymous users and a link to the login, if there are no credentials supplied.
  Whenever a protected resource is accessed, a 401 response should delegate to the login at that time.

### Operation interfaces
At least two ways of implementing operation interfaces are possible:
* Have a specific interface per operation with the resource as a generic parameter (`IGet<T>`, `IPut<T>`, etc.).
* Have an interface implementation per resource `IRepository<T>` which allows for capability discovery.
  Capability discovery should expose the same interface per operation interfaces.

The precise specification of the operation interfaces depends on the specification of the `Result` type.

An operation interface should not be equal to an HTTP method, but there should be a mapping.
When dealing with entities, often there is a preference for specifying 'CRUD' operations. 
However, CRUD deals too much with the implementation of the backend layer, whereas the HTTP methods deal more with what kind of guarantees are tied to the different methods.
For this reason, we at least need to specify the following interfaces:
* IRestGet<T>
* IRestPut<T>
* IRestPost<T>
* IRestDelete<T>
These interfaces should have the same constraints as the corresponding HTTP methods:

| Interface   | Method | Safe | Idempotent |
| ----------- | ------ | ---- | ---------- |
| IRestGet    | GET    | Yes  | Yes        |
| IRestPut    | PUT    | No   | Yes        |
| IRestPost   | POST   | No   | No         |
| IRestDelete | DELETE | No   | Yes        |

### Entities, objects, resources

> **Definition:** A resource is anything that can be addressed by a URI.

> **Definition:** An entity is a resource that adheres to a certain model.

> **Definition:** An object is an instance of an entity.

Resources can either be entities or non-entities.
An entity can be represented by an abstract model and can be serialized into a representational form.
Non-entities often already have a representational form (JPG image, HTML page, PDF document, etc.).

An object is an instance of an entity model and can be used to convert to and from the representational form. 
The Rest component is responsible for handling these objects, whereas the server component is responsible for converting it to and from the representational form.
Non-entities are handled by the server component only.
Of course, it is also possible to let ASP.Net Core handle static files (non-entities) using the UseStaticFiles() extension method.

### Schema and metadata
All entities should link to a schema, definining the structure of that entity.

### Content-Type and serialization
This is purely an HTTP aspect. 
The Rest service delivers a `RestResponse` which must be translated into a `HttpResponse`.
Part of this transformation is determining in what format the reponse needs to be serialized.

Determination of the Content-Type is affected by the following factors:
* Constraints of the resource.
  Non-entities may have a fixed Content-Type.
* Capability of the HTTP server.
  The server has support for different Content-Types based on registered plugins.
* `HttpRequest`'s Accept header.
* Request path/query string parameter hints. 
  This could be necessary when the Accept header cannot be set.

When the Content-Type is selected, the corresponding serialization can be used if necessary.
The serialization strategy is responsible for populating the `HttpResponse` with all the information in the `RestResponse` that is appropriate for the format.
This includes dealing with different Rest aspects and the format's support for them.

### Result type
The `RestResponse<T>` class is an abstraction over a subset of HTTP responses.
It should support everything that is needed for Rest.

#### Asynchrony
**TODO**

#### Lazy evaluation
The actual result of a `RestResponse` might not be needed to fullfil the `HttpRequest`, therefore the evaluation of the requested resource should be delayed until it is necessary.
This constraint will probably result in a `Lazy<T>`-typed `Result` property.

#### Status and failure
**TODO**


#### Result class
**TODO** How does this differ from the `RestResponse`? Is this actually needed?

#### Links and navigation
The result should be able to contain links to navigation targets.
There needs to be a Provider mechanism to provide these links and the actual links need to be validated against security )(and other) constraints.
The links can then be contained in a separate collection in the result/response.

#### Embedded objects
It should be possible to embed related objects directly into the response. 
Because the serialization format may not support this, it should also be lazy loaded.

#### HATEOAS
This aspect encompasses the following aspects:
* Linked schema information (Schema type)
  * Deciding on the type system (union types).
* Specification of Rest methods/interfaces.
* Specification of the way in which parameters are passed on the HTTP level. (Query string, HTTP Headers or Request Body)

#### Security 
A security interface needs to be defined that can serve as a backend for the implementation of link validation on Rest calls and also for the implementation of a ASP.Net Core pipeline component.

#### Paging
Paging can be made Restful by including navigation links.
Possible class hierarchy:
* PagedResult
* SearchResult
* ...

#### Versioning and deprecation
**TODO**
* URL?
* Header?
* Content-Type?
* Hyperlinking to different versions?

#### Localization
**TODO**

## Examples of requests

### Simple GET
1. A GET Request is made for `"/api/Person/Joost"`.
2. The ASP RestServer has a PathIdentityProvider translating it to an `Person("Joost")` identity.
3. The Rest repository is queried for `IGet<Person>`.
4. The `IGet<Person>` is passed the `Person("Joost")` value and returns a corresponding `Result<Person>` object.
5. The ASP RestServer inspects the object and determines the best available serialization format by inspecting the Accept header.
6. The serializer is called to serialize the `Person` object.

### Search
1. A GET Request is made for `"api/Person?search=Joost"`
2. The ASP RestServer translates this to a `PersonCollection(["search" => "Joost"])` identity.
3. The ASP RestServer translates the parameters into a `PersonCollectionParameters` object.
4. The Rest repository is queried for `IGet<PersonCollection>`. The PersonCollectionParameters type is implicit because the Identity library hides underlying types.
5. The `IGet<PersonCollection>` is passed the identity value and retrieves a `PersonCollection` which contains paging details. 
   These details include links to other pages and a total count.
6. The ASP RestServer inspects the object and determines the best available serialization format by inspecting the Accept header.
7. The serializer is called to serialize the `PersonCollection` object.

The `PersonCollection` type derives from some abstract type that supports paging.

### Post
How does a client know what to POST?
1. Client navigates to the Home resource `"/"`.
2. The server's return value contains a link to the person collection `"/api/Person"`.
3. Retrieving `"/api/Person"` yields a response containing a link to the metadata (schema) for the Person collection.
  This metadata contains:
  * schema information for `PersonCollection`.
  * a reference to the `Person` schema. 
  * a reference to the `PersonCollectionParameters` schema.
  * an operation `GET` that takes a `PersonCollectionParameters` on the query string.
  * an operation `POST` that takes a `Person` in the body.
4. Now the client knows what schema to use to construct a request to post to `"/api/Person"`.


