module getDDTest

open System
open Xunit
open FsCheck.Xunit
open SSNDK.SSN
open TestHelpers

[<Property>]
[<Trait("Category", "Unit")>]
let ``getDD works``(x: DateTimeOffset) = 
  let s = x |> formatDate
  x.Day = getDD s