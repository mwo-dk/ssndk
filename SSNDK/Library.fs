module SSNDK

open System.Runtime.CompilerServices
open System

[<assembly: InternalsVisibleTo("SSNDK.Test")>]
[<assembly: InternalsVisibleTo("SSNDK.CSharp.Test")>]
do()

/// <summary>
/// Represents an error reason
/// </summary>
type ErrorReason =
| NullEmptyOrWhiteSpace              // The value was null, empty or white space
| NonDigitCharacters                 // The value contained non-digit characters, where characters were expected
| NonDashCharacter                   // The value contained a non-dash character where a dash was expected
| Modula11CheckFail                  // The modula 11 check failed
| InvalidLength                      // The trimmed range has invalid length
| InvalidDayInMonth                  // The given day in the month is invalid
| InvalidYear                        // The year is invalid
| InvalidControl                     // The control number is invalid
| InvalidYearAndControl              // The year and control numbers are invalid
| InvalidYearAndControlCombination   // Essential unexpected error

/// <summary>
/// Represents the outcome of the computation of year of birth
/// </summary>
type YearOfBirth =
| Ok of int                          // The year of birth is ok and is included
| Error of ErrorReason               // The year of birth is wrong

/// <summary>
/// Represents the gender of a person
/// </summary>
type Gender =
| Male                               // Represents a male
| Female                             // Represents a female

[<AutoOpen>]
module internal Helpers =
  /// <summary>
  /// Computes the range of valid character indices - instead of allocating new string when trimming
  /// </summary>
  /// <param name="x">The string to "trim"</param>
  let getIndices x =
    if (String.IsNullOrWhiteSpace(x)) then (None, None)
    else 
      let length = x.Length
      let mutable first : int option = None
      let mutable last : int option = None
      {0..length-1} |> Seq.iter 
        (fun n -> 
          if ' ' <> x.[n] then 
            if first.IsNone then first <- Some n
            last <- Some n
          )
      (first, last)
  
  /// <summary>
  /// Determines whether all characters in the provided string <paramref name="x"/> given by the
  /// range [<paramref name="first"/>;<paramref name="last"/>] contains only characters
  /// </summary>
  /// <param name="first">The index of the first character</param>
  /// <param name="last">The index of the last character</param>
  /// <param name="x">The string to check</param>
  let allInts first last (x: string) =
    {first..last} |> Seq.fold (fun acc n -> acc && Char.IsDigit(x.[n])) true

  /// <summary>
  /// The character '0' represented as integer
  /// </summary>
  let zeroAsInt = int('0')

  /// <summary>
  /// Converts a digit character to its corresponding integer value
  /// </summary>
  /// <param name="x">The character to convert</param>
  let toInt x = int x - zeroAsInt

  /// <summary>
  /// The weights utilized for modula 11 checks
  /// </summary>
  let weights = [|4;3;2;7;6;5;4;3;2;1|]

  /// <summary>
  /// Computes the sum of product (utilized to the modula 11 check) of the digits in
  /// the provided string - on the locations denoted by the given indices
  /// </summary>
  /// <param name="indices">The indices, where the digits are located</param>
  /// <param name="x">The string to work on</param>
  let sumOfProduct (indices: int array) (x: string) =
    [|0..9|] |> Array.fold (fun acc n -> acc + weights.[n]*(toInt x.[indices.[n]])) 0
  
  /// <summary>
  /// Computes modula 11 of a given integer
  /// </summary>
  /// <param name="x">The number to compute the modula 11 on</param>
  let modula11Of x = x % 11

  /// <summary>
  /// Checkes whether the provided number is modula 11
  /// </summary>
  /// <param name="x">The number to check</param>
  let isModula11 x = 0 = (x |> modula11Of)
  
  /// <summary>
  /// Extracts the raw 'dd' (day in month) of the birthday part of the string
  /// </summary>
  /// <param name="first">First non-space character</param>
  /// <param name="x">The source string</param>
  let getDD first (x: string) = 10 * (toInt x.[first]) + (toInt x.[first + 1])
  /// <summary>
  /// Extracts the raw 'mm' (month) of the birthday part of the string
  /// </summary>
  /// <param name="first">First non-space character</param>
  /// <param name="x">The source string</param>
  let getMM first (x: string) = 10 * (toInt x.[first + 2]) + (toInt x.[first + 3])
  /// <summary>
  /// Extracts the raw 'y' (year) of the birthday part of the string
  /// </summary>
  /// <param name="first">First non-space character</param>
  /// <param name="x">The source string</param>
  let getYY first (x: string) = 10 * (toInt x.[first + 4]) + (toInt x.[first + 5])
  /// <summary>
  /// Extracts the control value of the string
  /// </summary>
  /// <param name="first">First non-space character</param>
  /// <param name="hasDash">Flag telling whether there is a dash in the string
  /// <param name="x">The source string</param>
  let getControlCode first hasDash (x: string) =
    let offSet = if hasDash then 7 else 6
    [|first + offSet..first + offSet+3|] |> Array.fold (fun acc n -> 10*acc + (toInt x.[n])) 0
    
  /// <summary>
  /// Computes the year of birth according to the rules set up in
  /// https://www.cpr.dk/media/17534/personnummeret-i-cpr.pdf
  /// </summary>
  /// <param name="yy">The 'yy' part from the SSN</param>
  /// <param name="control">The control character</param>
  let getBirthYear yy control =
    let inRange a b x = a <= x && x <= b
    let yearValid x = x |> inRange 0 99
    let controlValid x = x |> inRange 0 9999

    match yy |> yearValid, control |> controlValid with
    | false, true -> Error InvalidYear
    | true, false -> Error InvalidControl
    | false, false -> Error InvalidYearAndControl
    | true, true ->
      match yy, control with
      | x, y when x |> inRange 0 99 && y |> inRange 0 3999 -> Ok <| 1900 + x
      | x, y when x |> inRange 0 36 && y |> inRange 4000 4999 -> Ok <| 2000 + x
      | x, y when x |> inRange 37 99 && y |> inRange 4000 4999 -> Ok <| 1900 + x
      | x, y when x |> inRange 0 57 && y |> inRange 5000 8999 -> Ok <| 2000 + x
      | x, y when x |> inRange 58 99 && y |> inRange 5000 8999 -> Ok <| 1800 + x
      | x, y when x |> inRange 0 36 && y |> inRange 9000 9999 -> Ok <| 2000 + x
      | x, y when x |> inRange 37 99 && y |> inRange 9000 9999 -> Ok <| 1900 + x
      | _ -> Error InvalidYearAndControlCombination

  /// <summary>
  /// Determines the <see cref="Gender"/> of a person based on a given digit
  /// </summary>
  /// <param name="x">The digit to determine the <see cref="Gender"/> on</param>
  let getGender x = if (x |> toInt) % 2 = 0 then Female else Male

  /// <summary>
  /// Represents the outcome of a validation attempt
  /// </summary>
  type ValidationResult =
  | Ok of int*int*int     // first non-space character position, last ditto and control number
  | Error of ErrorReason

  let validate' useModula11Check ssn =
    match getIndices ssn with
    | Some first, Some last ->
      let length = last-first+1
      match length with 
      | 10 ->
        if ssn |> allInts first last |> not then Error NonDigitCharacters
        else
          if useModula11Check &&
            ssn |> sumOfProduct [|first .. first + 9|] |> 
              isModula11 |> not then Error Modula11CheckFail
          else
            Ok (first, last, ssn |> getControlCode first false)
      | 11 ->
        if ssn |> allInts first (first + 5) |> not || 
          ssn |> allInts (first + 7) last |> not then Error NonDigitCharacters
        else if ssn.[first + 6] <> '-' then Error NonDashCharacter
        else 
          if useModula11Check &&
            ssn |> sumOfProduct [|for n in first .. first + 9 do yield if n - first <= 5 then n else n + 1|] |> 
              isModula11 |> not then Error Modula11CheckFail
          else 
            Ok (first, last, ssn |> getControlCode first true)
      | _ -> Error InvalidLength
    | _ -> Error NullEmptyOrWhiteSpace
    
/// <summary>
/// Represents the outcome of a validation
/// </summary>
type ValidationResult =
| Ok                                       // The validation succeeded
| Error of ErrorReason                     // The validation failed

/// https://www.cpr.dk/media/17535/erstatningspersonnummerets-opbygning.pdf
let validate useModula11Check ssn =
  match ssn |> validate' useModula11Check with
  | Helpers.Ok _ -> Ok
  | Helpers.Error reason -> Error reason

/// <summary>
/// Represents the result of a valid validation result
/// </summary>
type PersonInfo = {
  Gender: Gender;                          // The gender of the person
  DateOfBirth: DateTimeOffset              // The day of birth of the person
}

/// <summary>
/// Represents the outcome of extracting info about the person behind the SSN
/// </summary>
type SSNResult = 
| Ok of PersonInfo                         // The extraction succeeded
| Error of ErrorReason                     // The extraction failed

/// <summary>
/// Computes the <see cref="SSNResult"/> 
/// </summary>
/// <param name="useModula11Check">Flag telling whether to utilize the modula 11 check</param>
/// <param name="repairDateOfBirth">Tlag telling whether to repair day in month</param>
/// <param name="ssn">The SSN string</param>
let getPersonInfo useModula11Check repairDateOfBirth ssn =
  match ssn |> validate' useModula11Check with
  | Helpers.Ok (first, last, controlCode) ->
    let (dd, mm, yy) = (ssn |> getDD first, ssn |> getMM first, ssn |> getYY first)
    match getBirthYear yy controlCode with
    | YearOfBirth.Ok year ->
      if dd < 0 || 99 < dd then Error InvalidDayInMonth
      else
        if 31 < dd && repairDateOfBirth |> not then Error InvalidDayInMonth
        else 
          let dd' = if dd <= 31 then dd else dd-60
          let daysInMonth = DateTime.DaysInMonth(year, mm)
          if daysInMonth < dd' then Error InvalidDayInMonth
          else 
            let gender = ssn.[last] |> getGender
            Ok {Gender = gender; DateOfBirth = DateTimeOffset(year, mm, dd', 0, 0, 0, TimeSpan.Zero)}
    | YearOfBirth.Error reason -> Error reason 
  | Helpers.Error reason -> Error reason