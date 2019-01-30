module validateDigitsAndDashTest

open Xunit
open SSNDK.Helpers
open TestHelpers

[<Fact>]
[<Trait("Category", "Unit")>]
let ``validateDigitsAndDash for short strings works``() =
  let testData = "444"
  match testData |> getIndices with
  | Known (first, last) -> 
    match testData |> validateDigitsAndDash first last with
    | false, false -> assertSuccess()
    | _ -> assertFail()
  | _ -> assertFail()

[<Fact>]
[<Trait("Category", "Unit")>]
let ``validateDigitsAndDash for long strings works``() =
  let testData = "444444444444444444"
  match testData |> getIndices with
  | Known (first, last) -> 
    match testData |> validateDigitsAndDash first last with
    | false, false -> assertSuccess()
    | _ -> assertFail()
  | _ -> assertFail()

[<Fact>]
[<Trait("Category", "Unit")>]
let ``validateDigitsAndDash with non digits and no dash works``() =
  let testData = "a444444444"
  match testData |> getIndices with
  | Known (first, last) -> 
    match testData |> validateDigitsAndDash first last with
    | false, true -> assertSuccess()
    | _ -> assertFail()
  | _ -> assertFail()

[<Fact>]
[<Trait("Category", "Unit")>]
let ``validateDigitsAndDash with non digits and dash works``() =
  let testData = "a44444-4444"
  match testData |> getIndices with
  | Known (first, last) -> 
    match testData |> validateDigitsAndDash first last with
    | false, true -> assertSuccess()
    | _ -> assertFail()
  | _ -> assertFail()

[<Fact>]
[<Trait("Category", "Unit")>]
let ``validateDigitsAndDash with non digits and dash failure works``() =
  let testData = "a44444x4444"
  match testData |> getIndices with
  | Known (first, last) -> 
    match testData |> validateDigitsAndDash first last with
    | false, false -> assertSuccess()
    | _ -> assertFail()
  | _ -> assertFail()

[<Fact>]
[<Trait("Category", "Unit")>]
let ``validateDigitsAndDash with digits and no dash works``() =
  let testData = "4444444444"
  match testData |> getIndices with
  | Known (first, last) -> 
    match testData |> validateDigitsAndDash first last with
    | true, true -> assertSuccess()
    | _ -> assertFail()
  | _ -> assertFail()

[<Fact>]
[<Trait("Category", "Unit")>]
let ``validateDigitsAndDash with digits and dash works``() =
  let testData = "444444-4444"
  match testData |> getIndices with
  | Known (first, last) -> 
    match testData |> validateDigitsAndDash first last with
    | true, true -> assertSuccess()
    | _ -> assertFail()
  | _ -> assertFail()

[<Fact>]
[<Trait("Category", "Unit")>]
let ``validateDigitsAndDash with digits and dash failure works``() =
  let testData = "444444x4444"
  match testData |> getIndices with
  | Known (first, last) -> 
    match testData |> validateDigitsAndDash first last with
    | true, false -> assertSuccess()
    | _ -> assertFail()
  | _ -> assertFail()