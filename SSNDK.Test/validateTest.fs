module validateTest

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

[<Property>]
[<Trait("Category", "Unit")>]
let ``validate works``(x: DateTimeOffset, dash: bool, controlCode: NonNegativeInt, useModula11Check: bool) = 
  let s, c = getSSN x dash controlCode
  match s |> validate' useModula11Check with
  | Ok _ ->
    SSNDK.ValidationResult.Ok = (s |> validate useModula11Check)
  | Error reason -> 
    SSNDK.ValidationResult.Error reason = (s |> validate useModula11Check)