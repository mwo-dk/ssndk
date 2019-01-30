module toIntTest

open System
open Xunit
open FsCheck.Xunit
open SSNDK.Helpers
open TestHelpers

[<Fact>]
[<Trait("Category", "Unit")>]
let ``zeroAsInt is 48``() = assertEqual 48 zeroAsInt

[<Fact>]
[<Trait("Category", "Unit")>]
let ``integer characters are converted correctly``() =
  (['0'..'9'], [0..9])||> List.fold2 (fun acc c x -> acc && (toInt c) = x) true

[<Property>]
[<Trait("Category", "Unit")>]
let ``toInt works``(x: char) =
  let isInt = Char.IsDigit(x)
  if isInt then (int(x) - zeroAsInt) = (toInt x)
  else true