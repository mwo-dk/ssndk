module getDDMMYYCCTest

open System
open Xunit
open FsCheck
open FsCheck.Xunit
open SSNDK.SSN

let getSSN dd mm yy dash (controlCode: NonNegativeInt) =
  let c = (controlCode.Get % 10000)
  if dash then (sprintf "%02i%02i%02i-%04i" dd mm yy c), c
  else (sprintf "%02i%02i%02i%04i" dd mm yy c), c

//[<Property>]
//[<Trait("Category", "Unit")>]
//let ``getDDMMYYCC works``(dd: PositiveInt, mm: PositiveInt, yy: PositiveInt, dash: bool, controlCode: NonNegativeInt, repair: bool) = 
//  let yy' = yy.Get
//  let mm' = (mm.Get % 12) + 1
//  let daysInMonth = DateTime.DaysInMonth(yy', mm')
//  let dd' = 1 + (dd.Get % daysInMonth)
//  let dd'' = if repair then dd' + 60 else dd'
//  let ssn, c = getSSN dd'' mm' yy' dash controlCode
//  match ssn |> getIndices with
//  | Known (first, _) ->
//    let dd, mm, yy, cc = ssn |> getDDMMYYCC first repair dash 
//    dd = dd' && mm = mm' && yy = yy' && cc = c
//  | _ -> false