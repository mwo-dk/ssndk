module allIntsTest

open Xunit
open SSNDK.Helpers
open TestHelpers

[<Fact>]
[<Trait("Category", "Unit")>]
let ``allInts of string with non-digits works``() = 
  let testData = " 000hatussa666 "
  let (first, last) = getIndices testData
  match first, last with
  | Some first, Some last -> assertFalse <| (allInts first last testData)
  | _ -> assertFail()

[<Fact>]
[<Trait("Category", "Unit")>]
let ``allInts of string with digits works``() = 
  let testData = " 0004353453666 "
  let (first, last) = getIndices testData
  match first, last with
  | Some first, Some last -> assertTrue <| (allInts first last testData)
  | _ -> assertFail()