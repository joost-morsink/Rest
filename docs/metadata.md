# Metadata
Metadata objects can be applied to Rest request and Rest response objects. 
They define a uniform way to communicate certain aspects of your services.
A lot of them map to HTTP headers and as such, need a component in the `IHttpRestRequestHandler` pipeline.

Request HTTP headers may be translated to Request metadata objects on the `RestRequest` and Response metadata objects on the `RestResponse` may be translated to response HTTP headers.

Metadata can be used on all types of requests and responses.
Actual support depends on handlers and repositories acting upon the metadata.

At the time of writing the following metadata types are available:
* `Capabilities` contains the capability names of the supported operations on a resource.
  This translates to the `Allow` response header.
* `CreatedResource` contains the address of a created resource.
  It can be used to indicate that a resource was created (so the HTTP status can be set to `201`), as well as the location of the created object (`Location` header).
* `Location` contains some address that relates to the response.
  It can be used for the HTTP `Location` header, but without setting the status to `201`.
* `ResponseCaching` contains caching information of the response.
* `VersionToken` contains information about a resource's version.
  In a request, it can be used to indicate a version is expected, or needs to be checked.
  In a response, it can carry the information of the current version.

Of course, any type of metadata can be modelled and used with this system, as it is a generic mechanism.
