# Rest server
This library supports developing level 3 Rest services.
Additionally the Rest services can be served over an HTTP connection by using ASP.Net Core.

## Identity
This library uses the Biz.Morsink.Identity library for identifying entities. 
Every resource in the Rest server corresponds to a type and a identity value.
There is a decoupling between the Rest pipeline and the actual serving of content over HTTP.

## Aspects
The following aspects come into play when designing a Rest repository:
* Identity (covered by the dependency).
* Operation (e.g. HTTP methods).
  Operation maps to operation interfaces.
* Entities and objects.
* Serialization/Deserialization.
  Should be configurable per content type.
* Result type.
  * Asynchrony.
  * Lazy evaluation.
  * Status and failure (e.g. HTTP status codes/exceptions).
  * Result type.
  * Links.
  * Embedded objects.
* In order to be fully Rest compliant everything must be navigable by content retrieved from the Rest Server.
  * This includes manipulation methods.
* Security.
  * Based on access checks for identity values/paths.
* Navigation.
  * LinkProviders.

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


