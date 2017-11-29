# Structures
A structure is an abstract concept encompassing a set of interacting components that all need to be registered in an `IServiceCollection`.
By registering a structure in the service collection, the underlying set is actually registered in the way the component developer wishes.
It is purely a method of convenience, and all components may also be registered individually by the Startup class.

A structure Is responsible for registering:
* The structure container type.
  Often this container type is used to resolve 'contained' service types.
* Repositories.
* Path mappings.
* Link providers and dynamic link providers.
* Any other services needed for correct operation of the structure.

Registration is done by calling an `AddStructure<S>` method, either the overload taking an instance, or relying on dependency injection.
The structure is not supposed to be registered itself, because that would require all kinds of dependencies being available at registration time. 
In many cases it will need to register some container component for other services (like repositories).

## Examples

### BlogRepository
At the time of writing, the ExampleWebApp has an implementation of repositories for the `Blog` and `BlogCollection` entities with an `AttributedRestRepository`. 
The structure is available as the `BlogRepository.Structure` class. 
It registers the `BlogRepository` as the `BlogRepository` service (as self)  and the collection and item repositories through the `AddAttributedRestRepository` extension method.
It also registers path mappings for both entity types:

```csharp
serviceCollection.AddAttributedRestRepository<BlogRepository>()
    .AddRestPathMapping<Blog>("/blog/*")
    .AddRestPathMapping<BlogCollection>("/blog?*");
```

For more information about attributed Rest repositories see the chapter about [attributes Rest repositories](attrRepo.md).

### PersonStructure
At the time of writing, the ExampleWebApp also has an implementation of repositories for the `Person` and `PersonCollection` types.
The structure derives from `AbstractRestCollectionStructure`, inheriting an `AbstractStructure` nested class.
This abstract structure already has a default registration implementation for collection based repository structures.
The class still needs to configure some aspects:

```csharp
string BasePath => "/person";
Type WildcardType => typeof(PersonCollection.Parameters);
```

Based on the `BasePath` it registers the collection and item repositories to the correct Rest paths and the `WildcardType` is used for determining parameters for searches in the collection repository.
Everything else is handled generically.

Within the `AbstractRestCollectionStructure.AbstractStructure` there is still the possibility to override the `RegisterComponents` method, when necessary.