module modula11OfTest

open Xunit
open FsCheck
open FsCheck.Xunit
open SSNDK.SSN

[<Property>]
[<Trait("Category", "Unit")>]
let ``modula11Of works``(x: PositiveInt) = 
  (x.Get % 11) = (x.Get |> modula11Of)