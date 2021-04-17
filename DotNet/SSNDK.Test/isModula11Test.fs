module isModula11Test

open Xunit
open FsCheck
open FsCheck.Xunit
open SSNDK.SSN

[<Property>]
[<Trait("Category", "Unit")>]
let ``isModula11 works``(x: PositiveInt) = 
  (x.Get % 11 = 0) = (x.Get |> isModula11)