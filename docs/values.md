# Rest Values
Rest values are included in RestResponses in a three layered design. 

* `RestValue`s are embedded in 
* `RestResult`s are embedded in
* `RestResponse`s, which are often return asynchronously (embedded in a `ValueTask`).

## RestValue
At the core lies the `RestValue`.
It contains three important properties:
* `Value` is the actual value that the response carries.
* `Links` are links that apply to the `Value` being returned.
* `Embeddings` are optionally embedded objects, that might alternatively be retrieved in a separate request. 
  They can either be requested for by parameters, or added by the repository as a predictive measure.
  Either way, the repository implementor is responsible for setting this property.

`RestValue<T>` is an immutable struct that contains the above-mentioned properties. 
The `Value` property is of type T.

It also implements the non-generic `IRestValue` interface, and the interfaces `IHasRestValue` and `IHasRestValue<T>` are used to indicate objects containing `IRestValue`s and `RestValue<T>`s respectively.

## RestResult
`RestResult`s indicate status of the response.
There are four main categories of results:

* **Successful** results carry the data that was requested.
* **Failure** results of different types map to different error situations.
  Depending on the failure type, data about the failure is contained in the result.
* **Redirection** results indicate that the client needs to look elsewhere.
* **Pending** results indicate longer running processes, that are still pending completion.

The types that carry a Rest value are adorned with the `IHasRestValue<>` interface.

A `RestResult<T>` is also immutable, but a class.
It implements the non-generic `IRestResult` interface.
Its nested and derived classes `Success`, `Failure`, `Redirect` and `Pending` correspond to the categories of results mentioned above.
They implement the `IRestSuccess<T>`, `IRestFailure`, `IRestRedirect` and `IRestPending` interfaces respectively.

## RestResponse
`RestResponse`s contain a `RestResult` and optional metadata (see [Metadata](metadata.md))

`RestResponse` is an abstract base class for `RestResponse<T>`.
This derived class carries more type information, but all the data is accessible through the base class, although in a non-generic way.
