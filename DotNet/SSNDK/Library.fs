module SSNDK.SSN

open System.Runtime.CompilerServices
open System
open SSNDK.ROP

[<assembly: InternalsVisibleTo("SSNDK.Test")>]
[<assembly: InternalsVisibleTo("SSNDK.CSharp.Test")>]
do()

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
  let inline isSpace x = x = space

  /// <summary>
  /// Represents a pair of cursors around a string, that needs to be trimmed
  /// </summary>
  [<Struct>]
  type CursorPair =
  | Unknown
  | Known of struct (int*int)

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
      | Known struct (first, _) ->
        if x |> isSpace then cursors
        else Known struct (first, n)
      | _ -> Unknown
    | x::xs  ->
      match cursors with
      | Known (first, last) ->
        if x |> isSpace then getRange (n+1) cursors xs
        else getRange (n+1) (Known struct (first, n)) xs
      | _ -> 
        if x |> isSpace then getRange (n+1) Unknown xs
        else getRange (n+1) (Known struct (n, n)) xs

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
  /// <param name="x">The string to work on</param>
  let sumOfProduct (x: char array) =
    [|0..9|] |> Array.fold (fun acc n -> acc + weights.[n]*(toInt x.[n])) 0
  
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
  /// Reads a two-digit number
  /// </summary>
  /// <param name="offSet">The offset in the string</param>
  /// <param name="x">The source string</param>
  let getTwoDigitNumber offSet (x:string) = 10*(toInt x.[offSet]) + (toInt x.[offSet + 1])

  /// <summary>
  /// Extracts the raw 'dd' (day in month) of the birthday part of the string
  /// </summary>
  let getDD = getTwoDigitNumber 0

  /// <summary>
  /// Extracts the raw 'mm' (month) of the birthday part of the string
  /// </summary>
  let getMM = getTwoDigitNumber 2

  /// <summary>
  /// Extracts the raw 'y' (year) of the birthday part of the string
  /// </summary>
  let getYY = getTwoDigitNumber 4

  /// <summary>
  /// Extracts the control value of the string
  /// </summary>
  /// <param name="hasDash">Flag telling whether there is a dash in the string</param>
  /// <param name="x">The source string</param>
  let getControlCode hasDash (x: string) =
    let offSet = if hasDash then 7 else 6
    [|offSet..offSet+3|] |> Array.fold (fun acc n -> 10*acc + (toInt x.[n])) 0  

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
    | false, false -> Exception($"Invalid year ({yy}) and control number ({control})") |> fail
    | false, true -> Exception($"Invalid year ({yy})") |> fail
    | true, false -> Exception($"Invalid control number ({control})") |> fail
    | true, true ->
        match yy, control with
        | x, y when x |> inRange 0 99 && y |> inRange 0 3999 -> succeed <| 1900 + x
        | x, y when x |> inRange 0 36 && y |> inRange 4000 4999 -> succeed <| 2000 + x
        | x, y when x |> inRange 37 99 && y |> inRange 4000 4999 -> succeed <| 1900 + x
        | x, y when x |> inRange 0 57 && y |> inRange 5000 8999 -> succeed <| 2000 + x
        | x, y when x |> inRange 58 99 && y |> inRange 5000 8999 -> succeed <| 1800 + x
        | x, y when x |> inRange 0 36 && y |> inRange 9000 9999 -> succeed <| 2000 + x
        | x, y when x |> inRange 37 99 && y |> inRange 9000 9999 -> succeed <| 1900 + x
        | _ -> Exception($"Invalid year ({yy}) and control number ({control})") |> fail
      

  /// <summary>
  /// Determines the <see cref="Gender"/> of a person based on a given digit
  /// </summary>
  /// <param name="x">The digit to determine the <see cref="Gender"/> on</param>
  let getGender x = if (x |> toInt) % 2 = 0 then Female else Male
    
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
  let repairDayInMonth' repairDayInMonth dd = if 61 <= dd && repairDayInMonth then dd - 60 else dd

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
  /// <param name="repairDayInMonth">Flag telling whether to repair day in month</param>
  /// <param name="dash">Flag telling whether <paramref name="ssn"/> contains a dash</param>
  /// <param name="ssn">The ssn to check</param>
  let getDDMMYYCC repairDayInMonth dash ssn =
    ssn |> getDD |> repairDayInMonth' repairDayInMonth, ssn |> getMM, ssn |> getYY, ssn |> getControlCode dash 

  /// <summary>
  /// Fetches dd, mm, birth year and gender based on whether to repair or not
  /// </summary>
  /// <param name="repairDayInMonth">Flag telling whether to repair day in month</param>
  /// <param name="dash">Flag telling whether <paramref name="ssn"/> contains a dash</param>
  /// <param name="ssn">The ssn to check</param>
  let getDDMMYYGender repairDayInMonth dash ssn =
    let dd, mm, yy, controlCode = ssn |> getDDMMYYCC repairDayInMonth dash
    if mm |> isMonthValid |> not then Exception($"Invalid month ({mm})") |> fail
    else 
        match getBirthYear yy controlCode with
        | Success year ->
            if isDayInMonthValid year mm dd |> not then Exception($"Invalid day ({dd}) in month ({mm}) in the year {year}") |> fail
            else (dd, mm, year, if controlCode % 2 = 0 then Female else Male) |> succeed
        | Failure error -> error |> fail

/// https://www.cpr.dk/media/17535/erstatningspersonnummerets-opbygning.pdf

/// <summary>
/// Validates a given danish social security number
/// </summary>
/// <param name="useModula11Check">Flag telling whether to use modula 11 check or not</param>
/// <param name="repairDayInMonth">Flag telling whether to repair day in month part of the
/// birthday</param>
/// <param name="ssn">The ssn to be checked</param>
[<CompiledName("Validate")>]
let validate useModula11Check repairDayInMonth ssn =
  let isBaseContentOk =
      if (System.String.IsNullOrWhiteSpace(ssn)) then false
      else
          let length = ssn.Length
          if length <> 10 && length <> 11 then false
          else
              if length = 10 then {0..9} |> Seq.fold (fun agg n -> agg && Char.IsDigit(ssn.[n])) true
              else {0..10} |> Seq.fold (fun agg n -> agg && (if n = 6 then ssn.[n] = '-' else Char.IsDigit(ssn.[n]))) true
  if isBaseContentOk then
      match ssn |> getDDMMYYGender repairDayInMonth (ssn.Length = 11) with
      | Success (yy, mm, dd, gender) ->
          if useModula11Check then
              let digits = 
                  {0..ssn.Length-1} |>
                  Seq.filter (fun n -> Char.IsDigit(ssn.[n])) |>
                  Seq.map (fun n -> ssn.[n]) |>
                  Seq.toArray
              let ok = digits |> sumOfProduct |> isModula11
              if ok then (dd, mm, yy, gender) |> succeed
              else Exception("Modula 11 check failed") |> fail
          else (dd, mm, yy, gender) |> succeed
      | Failure error -> error |> fail

  else 
      ArgumentException("Argument must be either 10 digits straight or 10 digits with a dash on the seventh place", nameof ssn) :> Exception |> Failure

/// <summary>
/// Validates a given danish social security number in the good old boolean fashion
/// </summary>
/// <param name="useModula11Check">Flag telling whether to use modula 11 check or not</param>
/// <param name="repairDayInMonth">Flag telling whether to repair day in month part of the birthday</param>
/// <param name="ssn">The ssn to be checked</param>
[<CompiledName("IsValid")>]
let isValid useModula11Check repairDayInMonth ssn =
  match ssn |> validate useModula11Check repairDayInMonth with 
  | Success _ -> true
  | _ -> false

/// <summary>
/// Represents the result of a valid validation result
/// </summary>
type PersonInfo = {
  Gender: Gender;                          // The gender of the person
  DateOfBirth: DateTimeOffset              // The day of birth of the person
}

/// <summary>
/// Computes the <see cref="SSNResult"/> 
/// </summary>
/// <param name="useModula11Check">Flag telling whether to utilize the modula 11 check</param>
/// <param name="repairDayInMonth">Tlag telling whether to repair day in month</param>
/// <param name="ssn">The SSN string</param>
[<CompiledName("GetPersonInfo")>]
let getPersonInfo useModula11Check repairDayInMonth ssn =
  match ssn |> validate useModula11Check repairDayInMonth with
  | Success (dd, mm, year, gender) ->
    {Gender = gender; DateOfBirth = DateTimeOffset(year, mm, dd, 0, 0, 0, TimeSpan.Zero)} |> succeed
  | Failure reason -> reason |> fail

module CSharp =
    
    /// <summary>
    /// Checks whether a given danish social security number is valid
    /// </summary>
    /// <param name="useModula11Check">Flag telling whether to use modula 11 check or not</param>
    /// <param name="repairDayInMonth">Flag telling whether to repair day in month part of the
    /// birthday</param>
    /// <param name="ssn">The ssn to be checked</param>
    [<CompiledName("IsValid")>]
    let isValid useModula11Check repairDayInMonth ssn =
        match validate useModula11Check repairDayInMonth ssn with
        | Success _ -> true
        | _ -> false

    /// <summary>
    /// Validates a given danish social security number
    /// </summary>
    /// <param name="useModula11Check">Flag telling whether to use modula 11 check or not</param>
    /// <param name="repairDayInMonth">Flag telling whether to repair day in month part of the
    /// birthday</param>
    /// <param name="ssn">The ssn to be checked</param>
    [<CompiledName("Validate")>]
    let validate useModula11Check repairDayInMonth ssn =
        match validate useModula11Check repairDayInMonth ssn with
        | Success (dd, mm, year, gender) -> struct (dd, mm, year, gender)
        | Failure error -> raise error

    /// <summary>
    /// Represents the gender of a person
    /// </summary>
    type Gender =
    | Male   = 0                            // Represents a male
    | Female = 1                            // Represents a female

    /// <summary>
    /// Computes the <see cref="SSNResult"/> 
    /// </summary>
    /// <param name="useModula11Check">Flag telling whether to utilize the modula 11 check</param>
    /// <param name="repairDayInMonth">Tlag telling whether to repair day in month</param>
    /// <param name="ssn">The SSN string</param>
    [<CompiledName("GetPersonInfo")>]
    let getPersonInfo useModula11Check repairDayInMonth ssn =
        let toEnumGender gender =
            match gender with
            | Male -> Gender.Male
            | Female -> Gender.Female
        let struct (dd, mm, year, gender) = ssn |> validate useModula11Check repairDayInMonth
        struct {| Gender = gender |> toEnumGender; DateOfBirth = DateTimeOffset(year, mm, dd, 0, 0, 0, TimeSpan.Zero) |}

    [<Extension>]
    type StringExtensions =
        [<Extension>]
        static member inline IsValid(x, useModula11Check, repairDayInMonth) =
            x |> isValid useModula11Check repairDayInMonth

        [<Extension>]
        static member inline Validate(x, useModula11Check, repairDayInMonth) =
            x |> validate useModula11Check repairDayInMonth

        [<Extension>]
        static member inline GetPersonInfo(x, useModula11Check, repairDayInMonth) =
            x |> getPersonInfo useModula11Check repairDayInMonth