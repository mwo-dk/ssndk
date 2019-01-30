module isMonthValidTest

open Xunit
open FsCheck
open FsCheck.Xunit
open SSNDK.Helpers
open TestHelpers

[<Property>]
[<Trait("Category", "Unit")>]
let ``isMonthValid for negative values values works``(mm: NegativeInt) =
  isMonthValid mm.Get |> not

[<Fact>]
[<Trait("Category", "Unit")>]
let ``isMonthValid for too negative zero works``() =
  isMonthValid 0 |> assertFalse

[<Property>]
[<Trait("Category", "Unit")>]
let ``isMonthValid for too high values values works``(mm: PositiveInt) =
  isMonthValid (12 + mm.Get) |> not