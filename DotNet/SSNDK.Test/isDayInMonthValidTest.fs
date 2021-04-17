module isDayInMonthValidTest

open System
open Xunit
open FsCheck
open FsCheck.Xunit
open SSNDK.SSN

[<Property>]
[<Trait("Category", "Unit")>]
let ``isDayInMonthValid for negative values works``(year: PositiveInt, month: PositiveInt, day : NegativeInt) =
  let month' = (month.Get % 12) + 1
  isDayInMonthValid (year.Get) month' (day.Get) |> not

[<Property>]
[<Trait("Category", "Unit")>]
let ``isDayInMonthValid for zero values works``(year: PositiveInt, month: PositiveInt) =
  let month' = (month.Get % 12) + 1
  isDayInMonthValid (year.Get) month' 0 |> not

[<Property>]
[<Trait("Category", "Unit")>]
let ``isDayInMonthValid for too high values works``(year: PositiveInt, month: PositiveInt) =
  let month' = (month.Get % 12) + 1
  let daysInMonth = DateTime.DaysInMonth(year.Get, month')
  isDayInMonthValid (year.Get) month' (daysInMonth+1) |> not

[<Property>]
[<Trait("Category", "Unit")>]
let ``isDayInMonthValid works``(year: PositiveInt, month: PositiveInt, day: PositiveInt) =
  let month' = (month.Get % 12) + 1
  let daysInMonth = DateTime.DaysInMonth(year.Get, month')
  let day' = (day.Get % daysInMonth)+1
  isDayInMonthValid (year.Get) month' day'