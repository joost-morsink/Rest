# HTTP converters
An Http converter is a component that takes on the responsibility to connect the ASP.Net Core pipeline to the Rest pipeline, by implementing the `IHttpRestConverter` interface.
The interface has the following (partial) definition:

```csharp
interface IHttpRestConverter
{
    NegotiationScore AppliesToRequestScore(HttpContext context);
    NegotiationScore AppliesToResponseScore(HttpContext context, RestRequest request, RestResponse response)
    RestRequest ManipulateRequest(RestRequest req, HttpContext context);
    object ParseBody(Type t, byte[] body);
    Task SerializeResponse(RestResponse response, HttpContext context);
}
```

It is responsible for:
* Determining if it is applicable to an Http Request.
* Determining if it is applicable to produced Http Responses.
* Amending RestRequests.
* Parsing request bodies.
* Serializing responses.

The instance of the handling converter is present in the `IHttpRestRequestHandler`, but not in the `IRestRequestHandler`.
Be sure to handle all serialization format specific aspects in the Http Rest pipeline.

## AbstractHttpConverter
`AbstractHttpConverter` is a base class for Http converters that helps in implementation.
It provides default behavior on some of the interface methods:

* `ManipulateRequest` adds the body parser to the Rest Request based on a single lazy read of the Request Body stream.
* `SerializeResponse` defines a template for converting a RestResponse to an HttpResponse.
  It needs some new members to fill in the gaps:
    * `ApplyGeneralHeaders` sets general headers, such as Content-Type.
    * `ApplyHeaders` sets headers that may depend on the value being serialized.  (Schema location and links go here)
    * `WriteValue` actually serializes the value to some Stream. (Usually the response stream)

It also provides many useful (protected) methods to help implement the gaps.

## JsonHttpConverter
Included in the solution is the `JsonHttpConverter`, which implements a plain JSON serialization format for the Rest requests and responses.
It determines applicability on the Accept header, which should equal `application/json`.
It also serializes values using the `application/json` Content-Type header.

The converter makes use of the helper methods mentioned in the `AbstractHttpConverter` section:

```csharp
protected override void ApplyGeneralHeaders(...)
{
    httpResponse.ContentType = "application/json";
}
protected override void ApplyHeaders(...)
{
    UseSchemaLocationHeader(httpResponse, value);
    UseLinkHeaders(httpResponse, value);
}
```

Although plain JSON is not considered to be RESTful, the `JsonHttpConverter` is able to satisfy the HATEOAS constraint by using HTTP-headers and JSON Schemas for `TypeDescriptor`s, as can be seen in the code fragment above.
The component can also be configured (using `LinkLocation`) to use a json property for the link collection.

## XmlHttpConverter
Also included is the `XmlHttpConverter`, which implements a plain XML serialization format for the Rest requests and responses.
The Accept header should be `application/xml` if this component should be used. 
This is also reflected in the Content-Type header.

Plain XML is just as RESTful as plain JSON, so the `XmlHttpConverter` also satisfies the HATEOAS constraint by using HTTP-headers, and XML schemas.

More information about this converter can be found in the [chapter about XML](xml.md).

## HalJsonHttpConverter
A separate project in the solution from the regular JSON implementation is the HAL implementation for JSON. 
It does not carry any type or schema information and is only supposed to be used for read-only scenarios.
The Accept header should be `application/hal+json` for responses to be formatted using HAL.

Strictly speaking this component implements HATEOAS in the serialization format, although it does not specify how to deal with dynamic navigation options in a hyperlinked fashion.
This eliminates the possibility for search forms using the HAL approach.

More information about the HAL converter can be found in the [chapter about HAL](hal.md).
