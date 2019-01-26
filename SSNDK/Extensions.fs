namespace SSNDKCS

open System
open SSNDK
open System.Runtime.CompilerServices
open System.Runtime.InteropServices

/// <summary>
/// Represents an error reason
/// </summary>
type ErrorReason =
| NullEmptyOrWhiteSpace = 1              // The value was null, empty or white space
| NonDigitCharacters = 2                 // The value contained non-digit characters, where characters were expected
| NonDashCharacter = 3                   // The value contained a non-dash character where a dash was expected
| Modula11CheckFail = 4                  // The modula 11 check failed
| InvalidLength = 5                      // The trimmed range has invalid length
| InvalidDayInMonth = 6                  // The given day in the month is invalid
| InvalidYear = 7                        // The year is invalid
| InvalidControl = 8                     // The control number is invalid
| InvalidYearAndControl = 9              // The year and control numbers are invalid
| InvalidYearAndControlCombination = 10  // Essential unexpected error

/// <summary>
/// Represents the gender of a person
/// </summary>
type Gender = 
| Male = 1                               // Represents a male
| Female = 2                             // Represents a female

[<AutoOpen>]
module internal Helpers = 
  /// <summary>
  /// Converts argument from union case to enum
  /// </summary>
  /// <param name="x">The union case to convert</param>
  let toError x =
    match x with 
    | NullEmptyOrWhiteSpace -> ErrorReason.NullEmptyOrWhiteSpace
    | NonDigitCharacters -> ErrorReason.NonDigitCharacters
    | NonDashCharacter -> ErrorReason.NonDashCharacter
    | Modula11CheckFail -> ErrorReason.Modula11CheckFail
    | InvalidLength -> ErrorReason.InvalidLength
    | InvalidDayInMonth -> ErrorReason.InvalidDayInMonth
    | InvalidYear -> ErrorReason.InvalidYear
    | InvalidControl -> ErrorReason.InvalidControl
    | InvalidYearAndControl -> ErrorReason.InvalidYearAndControl
    | InvalidYearAndControlCombination -> ErrorReason.InvalidYearAndControlCombination
  
  /// <summary>
  /// Converts the argument from union case to enum
  /// </summary>
  /// <param name="gender"></param>
  let toGender gender =
    match gender with
    | Male -> Gender.Male
    | Female -> Gender.Female

  /// <summary>
  /// Converts a union case to an <see cref="ArgumentException"/>
  /// </summary>
  /// <param name="reason">The error reason</param>
  let toException reason =
    match reason with
    | NullEmptyOrWhiteSpace -> ArgumentNullException("x") :> ArgumentException
    | NonDigitCharacters -> ArgumentException("The argument contains non digit characters where digits are expected", "x")
    | NonDashCharacter -> ArgumentException("The argument contains a non dash character where a dash was expected", "x")
    | Modula11CheckFail -> ArgumentException("The modula 11 check failed", "x")
    | InvalidLength -> ArgumentException("The lenght of the trimmed argument is wrong. Only 10 and 11 are accepted", "x")
    | InvalidDayInMonth -> ArgumentException("The day in the given month is invalid", "x")
    | InvalidYear -> ArgumentException("The year is invalid", "x")
    | InvalidControl -> ArgumentException("The control number is invalid", "x")
    | InvalidYearAndControl -> ArgumentException("The year and control numbers are invalid", "x")
    | InvalidYearAndControlCombination -> ArgumentException("Argument is invalid", "x")

/// <summary>
/// Base class for the outcome of a validation
/// </summary>
[<AbstractClass>]
type ValidationResult(isOk: bool) =
  member val IsOk = isOk
  member val IsError = isOk |> not
/// <summary>
/// Represents a successfull validation
/// </summary>
[<Sealed>]
type ValidationOkResult() =
  inherit ValidationResult(true)
/// <summary>
/// Represents a failed validation
/// </summary>
[<Sealed>]
type ValidationErrorResult(reason: ErrorReason) =
  inherit ValidationResult(false)
  member this.Error = reason
  
/// <summary>
/// Represents the person behind an SSN 
/// </summary>
[<Struct>]
type PersonInfo(gender: Gender, dateOfBirth: DateTimeOffset) =
  member this.Gender = gender
  member this.DateOfBirth = dateOfBirth
    
/// <summary>
/// Base class for the result of extracting the person behind an SSN
/// </summary>
[<AbstractClass>]
type SSNResult(isOk: bool) =
  member val IsOk = isOk
  member val IsError = isOk |> not
/// <summary>
/// Represents a successfull extraction of the person behind an SSN
/// </summary>
[<Sealed>]
type SSNOkResult(person: PersonInfo) =
  inherit SSNResult(true)
  member this.Person = person
/// <summary>
/// Represents a failed extraction of the person behind an SSN
/// </summary>
[<Sealed>]
type SSNErrorResult(reason: ErrorReason) =
  inherit SSNResult(false)
  member this.Error = reason
  
/// <summary>
/// Convenience extension for C# usage
/// </summary>
[<Extension>]
type StringExtensions() =
  /// <summary>
  /// Validates an SSN. Designed for C# pattern-match-like utilzation
  /// </summary>
  /// <param name="useModula11Check">Flag telling whether to utilize modula 11 check. Defaults to fals</param>
  [<Extension>]
  static member Validate(ssn, [<Optional; DefaultParameterValue(false)>] ?useModula11Check)  =
    let modula11Check = defaultArg useModula11Check false
    match ssn |> validate modula11Check with
    | SSNDK.ValidationResult.Ok -> ValidationOkResult() :> ValidationResult
    | SSNDK.ValidationResult.Error reason -> ValidationErrorResult(reason |> toError) :> ValidationResult
  /// <summary>
  /// Extracts the person behind an SSN. Designed for C# pattern-match-like utilization
  /// </summary>
  /// <param name="useModula11Check">Flag telling whether to utilize modula 11 cheeck. Defaults to false</param>
  /// <param name="repairBirthDate">Flag telling whether to repair the day in the month. Defauls to true</param>
  [<Extension>]
  static member GetPerson(ssn, [<Optional; DefaultParameterValue(false)>] ?useModula11Check, [<Optional; DefaultParameterValue(true)>] ?repairBirthDate) =
    let modula11Check = defaultArg useModula11Check false
    let repair = defaultArg repairBirthDate true
    match ssn |> getPersonInfo modula11Check  repair with
    | SSNResult.Ok person -> SSNOkResult(PersonInfo(person.Gender |> toGender, person.DateOfBirth)) :> SSNResult
    | SSNResult.Error reason -> SSNErrorResult(reason |> toError) :> SSNResult
  /// <summary>
  /// Validates an SSN. Throws an <see cref="ArgumentException"/> upon failure
  /// </summary>
  /// <param name="useModula11Check">Flag telling 
  [<Extension>]
  static member ValidateAndThrow(ssn, [<Optional; DefaultParameterValue(false)>] ?useModula11Check)  =
    let modula11Check = defaultArg useModula11Check false
    match ssn |> validate modula11Check with
    | SSNDK.ValidationResult.Ok -> ValidationOkResult()
    | SSNDK.ValidationResult.Error reason -> reason |> toException |> raise
  /// <summary>
  /// Extracts the person behind an SSN. Throws an <see cref="ArgumentException"/> upon failure
  /// </summary>
  /// <param name="useModula11Check">Flag telling whether to utilize modula 11 cheeck. Defaults to false</param>
  /// <param name="repairBirthDate">Flag telling whether to repair the day in the month. Defauls to true</
  [<Extension>]
  static member GetPersonAndThrow(ssn, [<Optional; DefaultParameterValue(false)>] ?useModula11Check, [<Optional; DefaultParameterValue(true)>] ?repairBirthDate) =
    let modula11Check = defaultArg useModula11Check false
    let repair = defaultArg repairBirthDate true
    match ssn |> getPersonInfo modula11Check  repair with
    | SSNResult.Ok person -> SSNOkResult(PersonInfo(person.Gender |> toGender, person.DateOfBirth))
    | SSNResult.Error reason -> reason |> toException |> raise