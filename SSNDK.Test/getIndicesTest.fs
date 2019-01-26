module getIndicesTest

open Xunit
open SSNDK.Helpers
open TestHelpers

[<Fact>]
[<Trait("Category", "Unit")>]
let ``getIndices of null string works``() = 
  let (first, last) = getIndices null
  assertIsNone first
  assertIsNone last

[<Fact>]
[<Trait("Category", "Unit")>]
let ``getIndices of empty string works``() = 
  let (first, last) = getIndices ""
  assertIsNone first
  assertIsNone last

[<Fact>]
[<Trait("Category", "Unit")>]
let ``getIndices of white space string works``() = 
  let (first, last) = getIndices ""
  assertIsNone first
  assertIsNone last

[<Fact>]
[<Trait("Category", "Unit")>]
let ``getIndices works 1``() = 
  let (first, last) = getIndices " joe "
  match first, last with
  | Some 1, Some 3 -> assertSuccess()
  | _, _ -> assertFail()

[<Fact>]
[<Trait("Category", "Unit")>]
let ``getIndices works 2``() = 
  let (first, last) = getIndices " jo e "
  match first, last with
  | Some 1, Some 4 -> assertSuccess()
  | _, _ -> assertFail()