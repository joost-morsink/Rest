# Attributed Rest Repositories
These are repositories that make heavy use of attributes on C# methods to indicate what type of Rest method is supported by the C# method.
It actually is more of a [structure](./struct.md), which registers the repository through the `AddAttributedRestRepository`. 
It also needs to register all the paths for the resource types exposed by the structure.

The registration method takes care of the rest, which is quite a bit:

* It registers the class itself as a service, because it is referred to by the actual runtime implementations.
  This makes it easier to control lifetime for the entire component.
* It uses reflection to query all the resource types used in the method signatures. 
  For each resource type it encounters it registers an `IRestRepository`.
* For each method attributed with a `RestAttribute`, it generates a capability on the registered `IRestRepository`.

For a method to be eligible for being a Rest method, the following criteria must be met:

* It must be attributed with a `RestAttribute` derived class (`RestGetAttribute`, `RestPostAttribute`, `RestPutAttribute`, `RestPatchAttribute`, `RestDeleteAttribute`)
* It must adhere to a certain form, depending on the attribute.

  * The return type must match either a `RestResponse<T>`, `RestResult<T>` or `RestValue<T>`, with `T` corresponding to the return type of the capability.
  * The return type may be wrapped in a `Task<>` or `ValueTask<>` type.
  * The first parameter **must** be of type `IIdentity<T>`.
  * A second optional parameter is mapped to the parameter type of the Rest request.
  * An optional third parameter (or second with a `RestBodyAttribute`) is mapped to the body type of the Rest request.
    A body parameter is mandatory for Post, Patch and Put requests.
  * Optionally parameters of `RestRequest` and `CancellationToken` may be added to the parameters of the method.

The type constraints for the Rest capability types can be found in the [capabilities chapter](./capabilities.md).
 
