namespace Biz.Morsink.Rest.FSharp.Tryout
open System.Reflection
open Microsoft.FSharp.Reflection

type public Union = 
    | A of int
    | B of string
    | C of float

type public Record = { a: int; b: string; c: float }

module Tests =     
    type Regular() =
        member val Firstname = ""
        member val Lastname = "" with get, set

    type Case =
        | RegularPerson of person:Regular
        | CheckedPerson of person:Regular * cool:bool

    [<Struct>]
    type TaggedString = | TaggedString of string

    [<Struct>]
    type Abc = 
     | A of a:int
     | B of b:string
     | C of c:float

    let checkPerson = function
        | RegularPerson p when p.Firstname = "Joost" -> CheckedPerson (p,true)
        | RegularPerson p -> CheckedPerson(p,false)
        | x -> x

    let test2 =
        let ty = typeof<Case>
        let attr = ty.GetCustomAttributesData() |> Seq.find (fun a -> a.AttributeType.Name = "CompilationMappingAttribute") 
        let desc = attr.ConstructorArguments |> Seq.map (fun p -> sprintf "%A = %A " p.ArgumentType p.Value) |> List.ofSeq
        printf "%A" desc
        0

    let test1 () =
        let ty = typeof<Union>;
        printf "%A\n" (FSharpType.IsUnion ty)
        let uci, value= FSharpValue.GetUnionFields(B "Joost", ty)
        printf "%A of %A\n" uci.Name value
        printf "%A\n" (value.GetType())
    let test3 () =
        let ty = typeof<Union>;
        let cma = ty.GetCustomAttributes() |> Seq.find (fun a -> a.GetType().Name = "CompilationMappingAttribute")
        let sm = cma.GetType().GetProperty("SourceConstructFlags").GetValue(cma,null).ToString()
        printf "%s\n" sm
