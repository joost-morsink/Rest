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
It is a method with a resource address and optionally parameters, it also has a body (the message to post).
The operation may have side effects.
Making the request multiple times may have undesired side-effects.

The response type can be any type.

## Patch
A patch is an operation that supports processing instructions on a resource.
The concrete format of these instructions depend on the implementation.
It is a method with a resource address, parameters and also a body containing the instructions.
The operation may have side effects.
Making the request multiple times may have undesired side-effects, depending on the semantics of the instructions provided.

The response should contain the modified resource.

## Delete 
Delete is the operation to use to delete a resource.
It is a method with a resource address and optionally parameters.
The operation is idempotent, which means making the request multiple times has the same effect as making it once.
This does not mean the response is the same on all requests (After deletion, the resource might 'not be found').

The response type can be any type.

## Table
The constraints of all the capability types are:

| Capability | Request body type | Response body type |
| ---------- | ----------------- | ------------------ |
| GET        | Empty             | Same as entity     |
| POST       | Any               | Any                |
| PUT        | Same as entity    | Same as entity     |
| PATCH      | Any               | Same as entity     |
| DELETE     | Should be empty   | Any                |
 