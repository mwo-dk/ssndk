module repairDayInMonth_Test

open Xunit
open FsCheck
open FsCheck.Xunit
open SSNDK.Helpers

[<Property>]
[<Trait("Category", "Unit")>]
let ``repairDayInMonth' works``(dd: PositiveInt, repair: bool) =
  match dd.Get |> repairDayInMonth' repair with 
  | dd' when 61 <= dd' ->
    repair |> not || dd' = dd.Get
  | dd' -> dd' = dd.Get