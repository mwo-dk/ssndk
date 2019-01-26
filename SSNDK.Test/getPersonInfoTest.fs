module getPersonInfoTest

open System
open Xunit
open FsCheck
open FsCheck.Xunit
open SSNDK.Helpers
open SSNDK

let getSSN (x: DateTimeOffset) dash unrepair (controlCode: NonNegativeInt) =
  let c = (controlCode.Get % 10000)
  let formatDate() =
    let dd = if unrepair then x.Day + 60 else x.Day
    let mm = x.Month
    let yy = (x.Year % 100)
    sprintf "%02i%02i%02i" dd mm yy
  if dash then (sprintf "%s-%04i" (formatDate()) c, c)
  else (sprintf "%s%04i" (formatDate()) c, c)

[<Property>]
[<Trait("Category", "Unit")>]
let ``getPersonInfo works``(x: DateTimeOffset, dash: bool, controlCode: NonNegativeInt, useModula11Check: bool, repairDay: bool) = 
  let s, c = getSSN x dash repairDay controlCode
  let yearOk x y =
    match getBirthYear (y%100) c with
    | YearOfBirth.Ok y -> x = y
    | _ -> false
  match s |> getPersonInfo useModula11Check repairDay with
  | SSNDK.SSNResult.Ok p ->
    let isValid = SSNDK.ValidationResult.Ok = (s |> validate useModula11Check) 
    let genderOk = p.Gender = getGender (s.[s.Length - 1])
    let dayOk = p.DateOfBirth.Day = x.Day 
    let monthOk = p.DateOfBirth.Month = x.Month
    let yearOk = yearOk (p.DateOfBirth.Year) (x.Year)
    isValid && genderOk && dayOk && monthOk && yearOk
  | SSNDK.SSNResult.Error reason -> 
    SSNDK.ValidationResult.Error reason = (s |> validate useModula11Check)