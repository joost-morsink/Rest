namespace Biz.Morsink.Rest.FSharp.Tryout
open Biz.Morsink.Identity
open Biz.Morsink.Rest

type public Union = 
    | A of a:int
    | B of b:string
    | C of c:float
    | D 
    
[<Struct>]
type public UnionStruct =
    | A of a:int
    | B of b:string
    | C of c:float
    | D 

type public Record = { a: int; b: string; c: float }

[<Struct>]
type TaggedString = | TaggedString of string

type AddressData = { Street: string; HouseNumber: int; City:string }
type Address = MailAddress of address:AddressData | HomeAddress of address:AddressData
type Person = { FirstName:string; LastName:string; Addresses : Address list }
    with 
        static member Create (firstName, lastName, addresses) = { FirstName=firstName; LastName=lastName; Addresses = addresses |> List.ofSeq  }
type Expression = 
    | Value of value:int
    | Mul of left:Expression * right:Expression
    | Add of left:Expression * right:Expression

type FsPersonRepository =
    [<RestGet>]
    member this.Get (id:IIdentity<Person>) = {
        FirstName = "Joost";
        LastName = "Morsink";
        Addresses = 
        [
            HomeAddress { Street="Mainstreet"; HouseNumber=1; City="Utrecht" };
            MailAddress { Street="PO box"; HouseNumber=1234; City="Utrecht" }
        ] }

//module Tests =     
//    type Regular() =
//        member val Firstname = ""
//        member val Lastname = "" with get, set

//    type Case =
//        | RegularPerson of person:Regular
//        | CheckedPerson of person:Regular * cool:bool

//    [<Struct>]
//    type TaggedString = | TaggedString of string

//    [<Struct>]
//    type Abc = 
//     | A of a:int
//     | B of b:string
//     | C of c:float

//    let checkPerson = function
//        | RegularPerson p when p.Firstname = "Joost" -> CheckedPerson (p,true)
//        | RegularPerson p -> CheckedPerson(p,false)
//        | x -> x

//    let test2 =
//        let ty = typeof<Case>
//        let attr = ty.GetCustomAttributesData() |> Seq.find (fun a -> a.AttributeType.Name = "CompilationMappingAttribute") 
//        let desc = attr.ConstructorArguments |> Seq.map (fun p -> sprintf "%A = %A " p.ArgumentType p.Value) |> List.ofSeq
//        printf "%A" desc
//        0

//    let test1 () =
//        let ty = typeof<Union>;
//        printf "%A\n" (FSharpType.IsUnion ty)
//        let uci, value= FSharpValue.GetUnionFields(B "Joost", ty)
//        printf "%A of %A\n" uci.Name value
//        printf "%A\n" (value.GetType())
//    let test3 () =
//        let ty = typeof<Union>;
//        let cma = ty.GetCustomAttributes() |> Seq.find (fun a -> a.GetType().Name = "CompilationMappingAttribute")
//        let sm = cma.GetType().GetProperty("SourceConstructFlags").GetValue(cma,null).ToString()
//        printf "%s\n" sm
