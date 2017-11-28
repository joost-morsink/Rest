# Collections
A collections is a composition of three distinct components based on two entity types.
The entity types are:

* The collection type.
* The entity type.

The components are:
* A Rest repository for the collection type, implementing Get and Post methods.
* A Rest repository for the element type, implementing Get, Put and Delete methods.
* A Source for the elements of the collection, implementing the `IRestResourceCollection<C, E>` interface.

## Repositories

### Collection repository
The collection repository is located at the root of the collection structure.
Its entity type may be derived from the `RestCollection` type, and if the `AbstractRestCollectionStructure` type is used, it is mandatory.
If the `RestCollection` type is used, all pagination links are provided by the `RestCollectionLinks` dynamic link provider.

### Element repository
This is a straightforward repository implementing Get, Put and Delete methods.
