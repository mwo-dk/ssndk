module SSNDK.SSN

open System.Runtime.CompilerServices
open System
open System.Text.RegularExpressions
open SSNDK.ROP

[<assembly: InternalsVisibleTo("SSNDK.Test")>]
[<assembly: InternalsVisibleTo("SSNDK.CSharp.Test")>]
do()

let private expr = Regex(@"\w*(\d\d)(\d\d)(\d\d)\-?(\d\d\d\d)\w*", RegexOptions.Compiled)
let private matchSSN s = 
    let result = expr.Match(s)
    if result.Success && result.Groups.Count = 5 then
        let ddOk, dd = Int32.TryParse(result.Groups[0].Value)
        let mmOk, mm = Int32.TryParse(result.Groups[1].Value)
        let yyOk, yy = Int32.TryParse(result.Groups[2].Value)
        let controlOk, control = Int32.TryParse(result.Groups[3].Value)
        ddOk && mmOk && yyOk && controlOk, dd, mm, yy, control
    else false, -1, -1, -1, -1

/// <summary>
/// Represents the gender of a person
/// </summary>
type Gender =
| Male                               // Represents a male
| Female                             // Represents a female

[<AutoOpen>]
module internal Helpers =

  /// <summary>
  /// The character '0' represented as integer
  /// </summary>
  let private zeroAsInt = int('0')

  /// <summary>
  /// Converts a digit character to its corresponding integer value
  /// </summary>
  /// <param name="x">The character to convert</param>
  let inline private toInt x = int x - zeroAsInt

  /// <summary>
  /// The weights utilized for modula 11 checks
  /// </summary>
  let private weights = [|4;3;2;7;6;5;4;3;2;1|]

  /// <summary>
  /// Computes the sum of product (utilized to the modula 11 check) of the digits in
  /// the provided string - on the locations denoted by the given indices
  /// </summary>
  /// <param name="x">The string to work on</param>
  let internal sumOfProduct (x: char array) =
    [|0..9|] |> Array.fold (fun acc n -> acc + weights[n]*(toInt x[n])) 0
  
  /// <summary>
  /// Computes modula 11 of a given integer
  /// </summary>
  /// <param name="x">The number to compute the modula 11 on</param>
  let internal modula11Of x = x % 11

  /// <summary>
  /// Checkes whether the provided number is modula 11
  /// </summary>
  /// <param name="x">The number to check</param>
  let internal isModula11 x = 0 = (x |> modula11Of)

  /// <summary>
  /// Computes the year of birth according to the rules set up in
  /// https://www.cpr.dk/media/17534/personnummeret-i-cpr.pdf
  /// </summary>
  /// <param name="yy">The 'yy' part from the SSN</param>
  /// <param name="control">The control character</param>
  let internal getBirthYear yy control =
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
    match ssn |> matchSSN with
    | true, dd, mm, yy, control ->
        match control |> getBirthYear yy with
        | Success yy ->
            let dd = if 61 <= dd && repairDayInMonth then dd - 60 else dd
            let gender = if control % 2 = 0 then Female else Male
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
    | _ -> 
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
    let isValid (useModula11Check, repairDayInMonth, ssn) =
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
    let validate (useModula11Check, repairDayInMonth, ssn) =
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
    let getPersonInfo (useModula11Check, repairDayInMonth, ssn) =
        let toEnumGender gender =
            match gender with
            | Male -> Gender.Male
            | Female -> Gender.Female
        let struct (dd, mm, year, gender) = validate (useModula11Check, repairDayInMonth, ssn)
        struct (gender |> toEnumGender, DateTimeOffset(year, mm, dd, 0, 0, 0, TimeSpan.Zero))

    [<Extension>]
    type StringExtensions =
        [<Extension>]
        static member inline IsValid(x, useModula11Check, repairDayInMonth) =
            isValid (useModula11Check, repairDayInMonth, x)

        [<Extension>]
        static member inline Validate(x, useModula11Check, repairDayInMonth) =
            validate (useModula11Check, repairDayInMonth, x)

        [<Extension>]
        static member inline GetPersonInfo(x, useModula11Check, repairDayInMonth) =
            getPersonInfo (useModula11Check, repairDayInMonth, x)