# Versioning
When an API needs to change, we often assign it a new version number.
If the change is not a breaking change, the new API can just replace the old API.
In case of a breaking change, we cannot replace the old API.

There are a few things we must do to help consumers transition to the new version:

* The old version needs to keep working, alongside the new version.
* Consumers need to be informed about the change.

When implementing a new version alongside an old version, we have the following options:

* Implement the new version at a different endpoint.
  For instance `person/v1/1` versus `person/v2/1`.
  This option can easily be implemented by adding a new repository to the project.
* Make use of an HTTP header:
  * Use content negotiation with the `Accept` header
  * Use a custom header

## HTTP header
In case we use an HTTP header for version discrimination, we end up in a situation that the same URI, and thus the same Rest Identity, point to different types and thus different Identities.




