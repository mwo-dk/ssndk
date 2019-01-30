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
  if dash then sprintf "%s-%04i" (x |> formatDate) c, c
  else (sprintf "%s%04i" (x |> formatDate) c, c)
let getSSNWithNonDigit (x: DateTimeOffset) dash (controlCode: NonNegativeInt) =
  let c = (controlCode.Get % 10000)
  if dash then (sprintf "a%s-%03i" (x |> formatDate) c, c)
  else (sprintf "a%s%03i" (x |> formatDate) c, c)
let getSSNWithDashFailure (x: DateTimeOffset) (controlCode: NonNegativeInt) =
  let c = (controlCode.Get % 10000)
  (sprintf "%sx%04i" (x |> formatDate) c, c)
let getSSNWithDashFailure' (dd: NonNegativeInt) (mm: NonNegativeInt) (yy: NonNegativeInt) (controlCode: NonNegativeInt) =
  let dd' = (dd.Get % 32)
  let mm' = (mm.Get % 87) + 13
  let yy' = (yy.Get % 100)
  let c = (controlCode.Get % 10000)
  (sprintf "%02i%02i%02i-%04i" dd' mm' yy' c, c)

[<Property>]
[<Trait("Category", "Unit")>]
let ``validate' with non-digit works``(x: DateTimeOffset, dash: bool, controlCode: NonNegativeInt, useModula11Check: bool, repair: bool) = 
  let s, c = getSSNWithNonDigit x dash controlCode
  match s |> validate' useModula11Check repair with
  | ValidationSuccess _ -> false
  | ValidationError reason ->  reason = NonDigitCharacters

[<Property>]
[<Trait("Category", "Unit")>]
let ``validate' with dash failure works``(x: DateTimeOffset, controlCode: NonNegativeInt, useModula11Check: bool, repair: bool) = 
  let s, c = getSSNWithDashFailure x controlCode
  match s |> validate' useModula11Check repair with
  | ValidationSuccess _ -> false
  | ValidationError reason ->  reason = NonDashCharacter

[<Property>]
[<Trait("Category", "Unit")>]
let ``validate' with month failure works``(dd: NonNegativeInt, mm: NonNegativeInt, yy: NonNegativeInt, controlCode: NonNegativeInt, useModula11Check: bool, repair: bool) = 
  let s, c = getSSNWithDashFailure' dd mm yy controlCode
  match s |> validate' useModula11Check repair with
  | ValidationSuccess _ -> false
  | ValidationError reason ->  reason = InvalidMonth

[<Property>]
[<Trait("Category", "Unit")>]
let ``validate' works``(x: DateTimeOffset, dash: bool, controlCode: NonNegativeInt, useModula11Check: bool, repair: bool) = 
  let s, c = getSSN x dash controlCode
  match s |> validate' useModula11Check repair with
  | ValidationSuccess (dd, mm, yy, gender) ->
    dd = x.Day && mm = x.Month && (yy % 100) = (x.Year % 100) && 
      if c % 2 = 0 then gender = Female else gender = Male
  | ValidationError reason -> 
    if useModula11Check then
      let indices = if dash then [|for i in 0..9 do yield if i <= 5 then i else i+1|] else [|0..9|]
      let isModula11 = s |> sumOfProduct indices |> isModula11
      if isModula11 then true
      else reason = Modula11CheckFail
    else 
      false