module getIndicesTest

open Xunit
open SSNDK.SSN
open TestHelpers

[<Fact>]
[<Trait("Category", "Unit")>]
let ``getIndices of null string works``() = 
  match getIndices null with
  | Unknown -> assertSuccess()
  | _ -> assertFail()

[<Fact>]
[<Trait("Category", "Unit")>]
let ``getIndices of empty string works``() = 
  match getIndices "" with
  | Unknown -> assertSuccess()
  | _ -> assertFail()

[<Fact>]
[<Trait("Category", "Unit")>]
let ``getIndices of white space string works``() = 
  match getIndices " " with
  | Unknown -> assertSuccess()
  | _ -> assertFail()

[<Fact>]
[<Trait("Category", "Unit")>]
let ``getIndices works 1``() = 
  match getIndices " joe " with
  | Known (first, last) -> 
    assertEqual 1 first
    assertEqual 3 last
  | _ -> assertFail()

[<Fact>]
[<Trait("Category", "Unit")>]
let ``getIndices works 2``() = 
  match getIndices " jo e " with
  | Known (first, last) -> 
    assertEqual 1 first
    assertEqual 4 last
  | _ -> assertFail()