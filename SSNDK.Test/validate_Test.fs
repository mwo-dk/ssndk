module validate_Test

open System
open Xunit
open FsCheck
open FsCheck.Xunit
open SSNDK.Helpers
open TestHelpers
open SSNDK

let getSSN (x: DateTimeOffset) dash (controlCode: NonNegativeInt) =
  let c = (controlCode.Get % 10000)
  if dash then (sprintf "%s-%04i" (x |> formatDate) c, c)
  else (sprintf "%s%04i" (x |> formatDate) c, c)
let getSSNWithNonDigit (x: DateTimeOffset) dash (controlCode: NonNegativeInt) =
  let c = (controlCode.Get % 10000)
  if dash then (sprintf "a%s-%03i" (x |> formatDate) c, c)
  else (sprintf "a%s%03i" (x |> formatDate) c, c)
let getSSNWithDashFailure (x: DateTimeOffset) (controlCode: NonNegativeInt) =
  let c = (controlCode.Get % 10000)
  (sprintf "%sx%04i" (x |> formatDate) c, c)

[<Property>]
[<Trait("Category", "Unit")>]
let ``validate' with non-digit works``(x: DateTimeOffset, dash: bool, controlCode: NonNegativeInt, useModula11Check: bool) = 
  let s, c = getSSNWithNonDigit x dash controlCode
  match s |> validate' useModula11Check with
  | Ok _ -> false
  | Error reason ->  reason = NonDigitCharacters

[<Property>]
[<Trait("Category", "Unit")>]
let ``validate' with dash failure works``(x: DateTimeOffset, controlCode: NonNegativeInt, useModula11Check: bool) = 
  let s, c = getSSNWithDashFailure x controlCode
  match s |> validate' useModula11Check with
  | Ok _ -> false
  | Error reason ->  reason = NonDashCharacter

[<Property>]
[<Trait("Category", "Unit")>]
let ``validate' works``(x: DateTimeOffset, dash: bool, controlCode: NonNegativeInt, useModula11Check: bool) = 
  let s, c = getSSN x dash controlCode
  match s |> validate' useModula11Check with
  | Ok (first, last, c') ->
    first = 0 && (if dash then last = 10 else last = 9) && c = c'
  | Error reason -> 
    if useModula11Check then
      let indices = if dash then [|for i in 0..9 do yield if i <= 5 then i else i+1|] else [|0..9|]
      let isModula11 = s |> sumOfProduct indices |> isModula11
      if isModula11 then false
      else reason = Modula11CheckFail
    else 
      false