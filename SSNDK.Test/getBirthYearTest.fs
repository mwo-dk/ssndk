module getBirthYearTest

open Xunit
open FsCheck
open FsCheck.Xunit
open SSNDK.Helpers
open TestHelpers
open SSNDK

[<Property>]
[<Trait("Category", "Unit")>]
let ``getBirthYear works``(yy: NonNegativeInt, controlCode: NonNegativeInt) = 
  let yy' = (yy.Get % 100)
  let c = (controlCode.Get % 10000)

  let inRange a b x = a <= x && x <= b
  match getBirthYear yy' c with
  | YearOfBirth.Ok year -> 
    match yy', c with 
    | _, c when c |> inRange 0 3999 -> year = 1900 + yy'
    | yy'', c when yy'' |> inRange 0 36 && c |> inRange 4000 4999 -> year = 2000 + yy'
    | yy'', c when yy'' |> inRange 37 99 && c |> inRange 4000 4999 -> year = 1900 + yy'
    | yy'', c when yy'' |> inRange 0 57 && c |> inRange 5000 5899 -> year = 2000 + yy'
    | yy'', c when yy'' |> inRange 58 99 && c |> inRange 5000 8999 -> year = 1800 + yy'
    | yy'', c when yy'' |> inRange 0 36 && c |> inRange 9000 9999 -> year = 2000 + yy'
    | yy'', c when yy'' |> inRange 37 99 && c |> inRange 9000 9999 -> year = 1900 + yy'
  | YearOfBirth.Error _ -> false