# HTTP converters
An Http converter is a component that takes on the responsibility to connect the ASP.Net Core pipeline to the Rest pipeline, by implementing the `IHttpRestConverter` interface.
The interface looks as follows:

```csharp
interface IHttpRestConverter
{
    bool Applies(HttpContext context);
    RestRequest ManipulateRequest(RestRequest req, HttpContext context);
    object ParseBody(Type t, byte[] body);
    Task SerializeResponse(RestResponse response, HttpContext context);
}
```

It is responsible for:
* Determining if it is applicable to an Http Request.
* Amending the RestRequest.
* Parsing the body.
* Serializing the response.

The instance of the handling converter is present in the `IHttpRestRequestHandler`, but not in the `IRestRequestHandler`.
Be sure to handle all serialization format specific aspects in the Http Rest pipeline.

## JsonHttpConverter
Included in the solution is the JsonHttpConverter, which implements a plain JSON serialization format for the Rest requests and responses.
It determines applicability on the Accept header, which should equal `application/json`.

