# Configuration
Configuration is an integral part of ASP.Net Core, and as such, using the Rest library with ASP.Net Core needs some configuration as well.

We will base the example on the ExampleWebApp configuration at the time of writing. 
The actual implementation of the ExampleWebApp configuration could change at any time.

## Example config

```csharp
services.AddRest(bld => bld
    .AddDefaultServices()
    .AddDefaultIdentityProvider()
    .AddCache<RestMemoryCache>()
    .AddJobs()
    .AddJsonHttpConverter(jbld => jbld.Configure(opts => opts.ApplyCamelCaseNamingStrategy()))
    .AddStructure<PersonStructure.Structure>(ServiceLifetime.Singleton)
    .AddRepository<HomeRepository>()
    );
```

Let's examine step by step what is happening here.

### AddRest
An extension method `AddRest` on `IServiceCollection` wraps the `IServiceCollection` instance into an `IRestServicesBuilder` object. 
This object supports extension method specifically for usage with the Rest library.
Also it defines a number of minimal services needed to get the library running properly:
* `CoreRestRequestHandler` is the component that directs a generic `RestRequest` to a specific repository.
* `TypeDescriptorCreator` is a component that automatically creates `TypeDescriptor` instances for .Net types.
* A `SchemaRepository` that implements `IRestRepository<TypeDescriptor>`. 
  This is used satisfying the HATEOAS constraint for requests that need input.
* `HttpContextAccessor` is needed get ASP.Net Core authentication and authorization information into an `IUser` instance, so Rest security can function properly.
* `AspNetCoreUser` implements `IUser` to implement Rest security.

Some services are only registered if no service is registered in the lambda passed to the `AddRest` method:
* If no `IAuthorizationProvider` service is provided, a default instance allowing everything is provided to the `IServiceCollection`.
* If no `IRestIdentityProvider` service is provided, an exception is thrown.

The `IRestServicesBuilder` instances also supports adding components to the following pipelines:
* RestRequestHandler pipeline using the `UseRequestHandler` method.
* HttpRestRequestHandler pipeline using the `UseHttpRequestHandler` method.

### AddDefaultServices
This method calls the following methods to add some components to the pipelines that are almost always applicable to a Rest interface.
* `AddCaching`. Adds support for cache related HTTP headers.
* `AddLocationHeader`. Adds support for the Location HTTP header, for instance in responses to POST requests.
* `AddOptionsHandler`. Add support for the HTTP Options method.

### AddDefaultIdentityProvider
This method adds an `IRestIdentityProvider` that is able to use attributes to determine how to map repositories.
It also registers `IRestPathMapping` instances, for cases where attributes cannot be used.

It is also possible to derive from `RestIdentityProvider`, but then the implementor is responsible for setting up all the path mappings.

### AddCache
Adds an `IRestCache` instance to the services and a `CacheRequestHandler` to the pipeline. 

### AddJobs
Adds multiple components necessary to implement long running tasks, called [Jobs](jobs.md) to the Rest API.

It adds:
* `JobRepository` so job statuses can be retrieved.
* `JobResultRepository` for retrieval of job results.
* Path mappings for the two repositories.
* `RestJobRepresentation` to customize the `RestJob` instances for serialization.
* If no `IRestJobStore` was configured, a default instance for in-memory caching is added.

These components allow a smooth experience with asynchronous responses for clients.

### AddJsonHttpConverter
Every API needs at least one HTTP Converter, and this example uses the Json HTTP converter. 
It is configured by calling the `Configure` method.
The usage of the `ApplyCamelCaseNamingStrategy` causes the Pascal-cased C# property names to be converted to the more idiomatic Camel-cased JSON property names.

### AddStructure
Adds a [structure](struct.md) of multiple components to the `IRestServicesBuilder` instance.

In this case a `PersonStructure` which models a collection of `Person` objects.
It defines 5 calls in two repositories and matching Rest path mappings:

* `PersonStructure.CollectionRepository`
  * `GET` -> Search for `Person`s in the collection.
  * `POST` -> Add a `Person` to the collection.
* `PersonStructure.ItemRepository`
  * `GET` -> Gets a specific `Person` from the repository.
  * `PUT` -> Updates (or creates) a specific `Person` in the repository.
  * `DELETE` -> Deletes a specific `Person` from the repository.

### AddRepository
Adds a repository to the services collection.

In this case the `HomeRepository` mapped to the home resource (`/`).
This resource is most important for its links to important other repositories, and is needed to satisfy the HATEOAS constraint.

## Other methods
Other methods are available for configuration of the Rest component, such as:

* `AddPathMapping` to add path mappings for the `DefaultRestIdentityProvider`.
* `AddAttributedRepository` to add an [attributed Rest repository](attrRepo.md) to the services.
    
## F#
> **TODO**: A library allowing for easy configuration in F# should be built.
