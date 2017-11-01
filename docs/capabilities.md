# Capabilities

## Get
A Get is the operation to use when retrieving resources. 
It is a method with a resource address and optionally parameters, but no body.
A request can always be made multiple times without having any side effect on the service.
In other words, making the request multiple times has the same effect as not making it at all.

The response body type corresponds with the entity type specified by the address (identity of the resource).

## Put
A Put is the operation to use to update a resource with a new version using the resource's representation.
It is a method with a resource address an optionally parameters.
The body type corresponds with the entity specified by the address (identity of the resource).
The operation is idempotent, which means making the request multiple times has the same effect as making it once.

The response body type corresponds with the entity type specified by the address (identity of the resource).
The response itself should be a representation of the resource as it is after the operation succeeded.

## Post
A Post is the operation to use when adding new resources, or making a request that represents a 'call' of some sorts to a resource.
It is a method with a resource address an optionally parameters, it also has a body (the message to post).
The operation may have side effects.
Making the request multiple times may have undesired side-effects.

The response type can be any type.

## Delete 
Delete is the operation to use to delete a resource.
It is a method with a resource address and optionally parameters.
The operation is idempotent, which means making the request multiple times has the same effect as making it once.
This does not mean the response is the same on all requests (After deletion, the resource might 'not be found').

The response type can be any type.