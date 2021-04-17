module getYYTest

open System
open Xunit
open FsCheck.Xunit
open SSNDK.SSN
open TestHelpers

[<Property>]
[<Trait("Category", "Unit")>]
let ``getYY works``(x: DateTimeOffset) = 
  let s = x |> formatDate
  (x.Year % 100) = (s |> getYY)