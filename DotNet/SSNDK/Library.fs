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
| InvalidMonth                       // The given month is invalid
| InvalidYear                        // The year is invalid
| InvalidControl                     // The control number is invalid
| InvalidYearAndControl              // The year and control numbers are invalid
| InvalidYearAndControlCombination   // Essential unexpected error

/// <summary>
/// Represents the gender of a person
/// </summary>
type Gender =
| Male                               // Represents a male
| Female                             // Represents a female

[<AutoOpen>]
module internal Helpers =
  /// <summary>
  /// The space character
  /// </summary>
  let space = ' '

  /// <summary>
  /// Determines if a given character is a space
  /// </summary>
  /// <param name="x">The character to categorize</param>
  let isSpace x = x = space

  /// <summary>
  /// Represents a pair of cursors around a string, that needs to be trimmed
  /// </summary>
  type CursorPair =
  | Unknown
  | Known of int*int

  /// <summary>
  /// Determines the cursor pair (inclusive) around a string, that needs to be trimmed
  /// </summary>
  /// <param name="n"></param>
  /// <param name="cursors"></param>
  /// <param name="x"></param>
  let rec getRange n cursors x = 
    match x with
    | [] -> cursors
    | [x] ->
      match cursors with
      | Known (first, last) ->
        if x |> isSpace then cursors
        else Known (first, n)
      | _ -> Unknown
    | x::xs  ->
      match cursors with
      | Known (first, last) ->
        if x |> isSpace then getRange (n+1) cursors xs
        else getRange (n+1) (Known(first, n)) xs
      | _ -> 
        if x |> isSpace then getRange (n+1) Unknown xs
        else getRange (n+1) (Known(n, n)) xs

  /// <summary>
  /// Computes the range of valid character indices - instead of allocating new string when trimming
  /// </summary>
  /// <param name="x">The string to "trim"</param>
  let getIndices x =
    if String.IsNullOrWhiteSpace(x) then Unknown
    else x.ToCharArray() |> List.ofArray |> getRange 0 Unknown
  
  /// <summary>
  /// Determines whether all characters in the provided string <paramref name="x"/> given by the
  /// range <paramref name="first"/>;<paramref name="last"/> contains only characters
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
  /// Represents the outcome of the computation of year of birth
  /// </summary>
  type YearOfBirth =
  | YearOfBirthSuccess of int               // The year of birth is ok and is included
  | YearOfBirthError of ErrorReason  // The year of birth is wrong

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
    | false, true -> YearOfBirthError InvalidYear
    | true, false -> YearOfBirthError InvalidControl
    | false, false -> YearOfBirthError InvalidYearAndControl
    | true, true ->
      match yy, control with
      | x, y when x |> inRange 0 99 && y |> inRange 0 3999 -> YearOfBirthSuccess <| 1900 + x
      | x, y when x |> inRange 0 36 && y |> inRange 4000 4999 -> YearOfBirthSuccess <| 2000 + x
      | x, y when x |> inRange 37 99 && y |> inRange 4000 4999 -> YearOfBirthSuccess <| 1900 + x
      | x, y when x |> inRange 0 57 && y |> inRange 5000 8999 -> YearOfBirthSuccess <| 2000 + x
      | x, y when x |> inRange 58 99 && y |> inRange 5000 8999 -> YearOfBirthSuccess <| 1800 + x
      | x, y when x |> inRange 0 36 && y |> inRange 9000 9999 -> YearOfBirthSuccess <| 2000 + x
      | x, y when x |> inRange 37 99 && y |> inRange 9000 9999 -> YearOfBirthSuccess <| 1900 + x
      | _ -> YearOfBirthError InvalidYearAndControlCombination

  /// <summary>
  /// Determines the <see cref="Gender"/> of a person based on a given digit
  /// </summary>
  /// <param name="x">The digit to determine the <see cref="Gender"/> on</param>
  let getGender x = if (x |> toInt) % 2 = 0 then Female else Male

  /// <summary>
  /// Validates that trimmed string contains only digits where expected as well as a dash
  /// on the expected place - if expected
  /// </summary>
  /// <param name="first">First non-space positiion</param>
  /// <param name="last">Last non-space position</param>
  /// <param name="ssn">The ssn to be checked</param>
  let validateDigitsAndDash first last ssn =
    let length = last - first + 1
    match length with 
    | 10 -> ssn |> allInts first last, true
    | 11 -> (ssn |> allInts first (first + 5)) && ssn |> allInts (first + 7) last, ssn.[first + 6] = '-'
    | _ -> false, false

  /// <summary>
  /// Represents the outcome of a validation attempt
  /// </summary>
  type Validation =
  | ValidationSuccess of int*int*int*Gender     // dd, mm, yy, controlCode, gender
  | ValidationError of ErrorReason
  
  /// <summary>
  /// Checks whether a month is valid
  /// </summary>
  /// <param name="mm"></param>
  let isMonthValid mm = 1 <= mm && mm <= 12

  /// <summary>
  /// Repairs the day in a month according to rules - if <paramref name="repairDayInMonth"/> is set
  /// </summary>
  /// <param name="repairDayInMonth">Flag telling whether to repair</param>
  /// <param name="dd">The day to optionally repair</param>
  let repairDayInMonth' repairDayInMonth dd =
    if 61 <= dd && repairDayInMonth then dd - 60 else dd

  /// <summary>
  /// Validates whether a given day in given month is valid
  /// </summary>
  /// <param name="year">The year to check</param>
  /// <param name="mm">The month to check - assumed between 1 and 12. Validated before invokation</param>
  /// <param name="dd">The day to check</param>
  let isDayInMonthValid year mm dd = 
    let daysInMonth = DateTime.DaysInMonth(year, mm)
    1 <= dd && dd <= daysInMonth
    
  /// <summary>
  /// Fetches dd, mm, yy and cc of a trimmed ssn based on whether to repair or not
  /// </summary>
  /// <param name="first">First valid character position</param>
  /// <param name="repairDayInMonth">Flag telling whether to repair day in month</param>
  /// <param name="dash">Flag telling whether <paramref name="ssn"/> contains a dash</param>
  /// <param name="ssn">The ssn to check</param>
  let getDDMMYYCC first repairDayInMonth dash ssn =
    ssn |> getDD first |> repairDayInMonth' repairDayInMonth, ssn |> getMM first, ssn |> getYY first, ssn |> getControlCode first dash 

  /// <summary>
  /// Fetches dd, mm, birth year and gender based on whether to repair or not
  /// </summary>
  /// <param name="first">First valid character position</param>
  /// <param name="repairDayInMonth">Flag telling whether to repair day in month</param>
  /// <param name="dash">Flag telling whether <paramref name="ssn"/> contains a dash</param>
  /// <param name="ssn">The ssn to check</param>
  let getDDMMYYGender first repairDayInMonth dash ssn =
    let dd, mm, yy, controlCode = ssn |> getDDMMYYCC first repairDayInMonth dash
    if mm |> isMonthValid |> not then ValidationError InvalidMonth
      else 
        match getBirthYear yy controlCode with
        | YearOfBirthSuccess year ->
          if isDayInMonthValid year mm dd |> not then ValidationError InvalidDayInMonth
          else ValidationSuccess (dd, mm, year, if controlCode % 2 = 0 then Female else Male)
        | YearOfBirthError reason -> ValidationError reason

  /// <summary>
  /// Validates whether a given ssn is valid
  /// </summary>
  /// <param name="useModula11Check">Flag telling whether to perform the old modula 11 check</param>
  /// <param name="repairDayInMonth">Flag telling whether to repair the day in month</param>
  /// <param name="ssn">The ssn to be validated</param>
  let validate' useModula11Check repairDayInMonth ssn =
    match getIndices ssn with
    | Known (first, last) ->
      match ssn |> validateDigitsAndDash first last with
      | false, _ -> ValidationError NonDigitCharacters
      | _, false -> ValidationError NonDashCharacter
      | _ -> 
        let dash = last - first + 1 = 11
        match ssn |> getDDMMYYGender first repairDayInMonth dash with 
        | ValidationSuccess (dd, mm, yy, gender) ->
          if useModula11Check then 
            let indices = 
              if dash then [|for i in 0..9 do yield if i <= 5 then first + i else first + i + 1|]
              else [|first..first+9|]
            let ok = ssn |> sumOfProduct indices |> isModula11
            if ok then ValidationSuccess (dd, mm, yy, gender)
            else ValidationError Modula11CheckFail
          else ValidationSuccess (dd, mm, yy, gender)
        | ValidationError reason -> ValidationError reason
    | _ -> ValidationError NullEmptyOrWhiteSpace
    
/// <summary>
/// Represents the outcome of a validation
/// </summary>
type ValidationResult =
| Ok                                       // The validation succeeded
| Error of ErrorReason                     // The validation failed

/// https://www.cpr.dk/media/17535/erstatningspersonnummerets-opbygning.pdf

/// <summary>
/// Validates a given danish social security number
/// </summary>
/// <param name="useModula11Check">Flag telling whether to use modula 11 check or not</param>
/// <param name="repairDayInMonth">Flag telling whether to repair day in month part of the
/// birthday</param>
/// <param name="ssn">The ssn to be checked</param>
let validate useModula11Check repairDayInMonth ssn =
  match ssn |> validate' useModula11Check repairDayInMonth with
  | ValidationSuccess _ -> Ok
  | ValidationError reason -> Error reason

/// <summary>
/// Validates a given danish social security number in the good old boolean fashion
/// <param name="useModula11Check">Flag telling whether to use modula 11 check or not</param>
/// <param name="repairDayInMonth">Flag telling whether to repair day in month part of the
/// birthday</param>
/// <param name="ssn">The ssn to be checked</param>
let isValid useModula11Check repairDayInMonth ssn =
  match ssn |> validate' useModula11Check repairDayInMonth with 
  | ValidationSuccess _ -> true
  | _ -> false

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
/// <param name="repairDayInMonth">Tlag telling whether to repair day in month</param>
/// <param name="ssn">The SSN string</param>
let getPersonInfo useModula11Check repairDayInMonth ssn =
  match ssn |> validate' useModula11Check repairDayInMonth with
  | ValidationSuccess (dd, mm, year, gender) ->
    Ok {Gender = gender; DateOfBirth = DateTimeOffset(year, mm, dd, 0, 0, 0, TimeSpan.Zero)}
  | ValidationError reason -> Error reason