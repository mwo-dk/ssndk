module getDDTest

open System
open Xunit
open FsCheck.Xunit
open SSNDK.Helpers
open TestHelpers

[<Property>]
[<Trait("Category", "Unit")>]
let ``getDD works``(x: DateTimeOffset) = 
  let s = x |> formatDate
  x.Day = (s |> getDD 0)