namespace Biz.Morsink.Rest.FSharp.Tryout
open Biz.Morsink.Identity
open Biz.Morsink.Rest

/// <summary>
/// An F# union type for testing purposes.
/// Contains 3 cases with a named parameter and one parameterless case.
/// </summary>
type public Union = 
    | A of a:int
    | B of b:string
    | C of c:float
    | D 

/// <summary>
/// An F# union type for testing purposes.
/// This type is implemented as a ValueType (struct).
/// Contains 3 cases with a named parameter and one parameterless case.
/// </summary>    
[<Struct>]
type public UnionStruct =
    | A of a:int
    | B of b:string
    | C of c:float
    | D 
/// <summary>
/// A typical F# record type for testing purposes.
/// </summary>
type public Record = { a: int; b: string; c: float }

/// <summary>
/// An F# union type for testing purposes.
/// This type is implemented as a ValueType (struct).
/// Contains only a single case.
/// </summary>
[<Struct>]
type TaggedString = | TaggedString of string

/// <summary>
/// An F# record type for address data.
/// </summary>
type AddressData = { Street: string; HouseNumber: int; City:string }
/// <summary>
/// An F# union type for distinguishing different types of address.
/// </summary>
type Address = MailAddress of address:AddressData | HomeAddress of address:AddressData
/// <summary>
/// An F# record type for person data.
/// </summary>
type Person = { FirstName:string; LastName:string; Addresses : Address list }
    with 
        static member Create (firstName, lastName, addresses) = { FirstName=firstName; LastName=lastName; Addresses = addresses |> List.ofSeq  }
/// <summary>
/// A recursive F# union type for testing purposes.
/// </summary>
type Expression = 
    | Value of value:int
    | Mul of left:Expression * right:Expression
    | Add of left:Expression * right:Expression

/// <summary>
/// An Rest repository implementation in F# (for testing purposes).
/// </summary>
type FsPersonRepository() =
    [<RestGet>]
    member this.Get (id:IIdentity<Person>) = {
        FirstName = "Joost";
        LastName = "Morsink";
        Addresses = 
        [
            HomeAddress { Street="Mainstreet"; HouseNumber=1; City="Utrecht" };
            MailAddress { Street="PO box"; HouseNumber=1234; City="Utrecht" }
        ] }
