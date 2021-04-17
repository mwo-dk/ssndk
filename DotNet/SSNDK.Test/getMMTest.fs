module getMMTest

open System
open Xunit
open FsCheck.Xunit
open SSNDK.SSN
open TestHelpers

[<Property>]
[<Trait("Category", "Unit")>]
let ``getMM works``(x: DateTimeOffset) = 
  let s = x |> formatDate
  x.Month = (s |> getMM)