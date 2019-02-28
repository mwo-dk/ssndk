namespace SSNDKCS

open System
open SSNDK
open System.Runtime.CompilerServices
open System.Runtime.InteropServices

/// <summary>
/// Represents an error reason
/// </summary>
type ErrorReason =
| Ok = 0                                 // No error occurred
| NullEmptyOrWhiteSpace = 1              // The value was null, empty or white space
| NonDigitCharacters = 2                 // The value contained non-digit characters, where characters were expected
| NonDashCharacter = 3                   // The value contained a non-dash character where a dash was expected
| Modula11CheckFail = 4                  // The modula 11 check failed
| InvalidLength = 5                      // The trimmed range has invalid length
| InvalidDayInMonth = 6                  // The given day in the month is invalid
| InvalidMonth = 7                       // The given month is invalid
| InvalidYear = 8                        // The year is invalid
| InvalidControl = 9                     // The control number is invalid
| InvalidYearAndControl = 10             // The year and control numbers are invalid
| InvalidYearAndControlCombination = 11  // Essential unexpected error

/// <summary>
/// Represents the gender of a person
/// </summary>
type Gender = 
| Male = 1                               // Represents a male
| Female = 2                             // Represents a female

/// <summary>
/// Represents the language of the error texts produced
/// </summary>
type ErrorTextLanguage =
| English = 1                            // Error text is formatted in english
| Danish = 2                             // Error text is formatted in danish

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
    | InvalidMonth -> ErrorReason.InvalidMonth
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
  /// Formats a given error reason to plain english
  /// </summary>
  /// <param name="reason">The error reason to format</param>
  let toErrorEnglishText reason =
    match reason with
    | NullEmptyOrWhiteSpace -> "The argument is null, empty or white-space"
    | NonDigitCharacters -> "The argument contains non digit characters where digits are expected"
    | NonDashCharacter -> "The argument contains a non dash character where a dash was expected"
    | Modula11CheckFail -> "The modula 11 check failed"
    | InvalidLength -> "The lenght of the trimmed argument is wrong. Only 10 and 11 are accepted"
    | InvalidDayInMonth -> "The day in the given month is invalid"
    | InvalidMonth -> "The month is invalid"
    | InvalidYear -> "The year is invalid"
    | InvalidControl -> "The control number is invalid"
    | InvalidYearAndControl -> "The year and control numbers are invalid"
    | InvalidYearAndControlCombination -> "Argument is invalid"
  
  /// <summary>
  /// Formats a given error reason to plain danish
  /// </summary>
  /// <param name="reason">The error reason to format</param>
  let toErrorDanishText reason =
    match reason with
    | NullEmptyOrWhiteSpace -> "Argumentet er enten null, tomt eller indeholder \"white-space\""
    | NonDigitCharacters -> "Argumentet indeholder bogstaver, der ikke er cifre, hvor disse er forventede"
    | NonDashCharacter -> "Argumentet indeholder et bogstav, der ikke er en binde-streg, hvor dette er forventet "
    | Modula11CheckFail -> "Modula-11 tjekket fejlede"
    | InvalidLength -> "Længden af det \"trimmede\" argument er forkert. Efter at mellemrum er fjernet i starten og slutningen, skal længden af det tilbageværende, være enten 10 eller 11 bogstaver"
    | InvalidDayInMonth -> "Dagen i månedet er ikke validt"
    | InvalidMonth -> "Måneden er ikke validt"
    | InvalidYear -> "Året er ikke validt"
    | InvalidControl -> "Kontrol-nummeret er ikke validt"
    | InvalidYearAndControl -> "Kombinationen af år og kontrol-nummer er ikke valid"
    | InvalidYearAndControlCombination -> "Argumentet er ikke validt"

  let mutable defaultErrorLanguage = ErrorTextLanguage.English
  /// <summary>
  /// Sets the default error reporting language
  /// </summary>
  /// <param name="language">The preferrend error reporting language</param>
  let setDefaultErrorLanguage language = defaultErrorLanguage <- language
  /// <summary>
  /// Gets the default error reporting language
  /// </summary>
  let getDefaultErrorLanguage() = defaultErrorLanguage

  let toErrorText language reason =
    match language with 
    | ErrorTextLanguage.English -> reason |> toErrorEnglishText
    | ErrorTextLanguage.Danish -> reason |> toErrorDanishText
    | _ -> "Hnnng? What language is that?"

  /// <summary>
  /// Converts a union case to an <see cref="ArgumentException"/>
  /// </summary>
  /// <param name="reason">The error reason</param>
  let toException reason language =
    match reason with
    | NullEmptyOrWhiteSpace -> ArgumentNullException("x") :> ArgumentException
    | NonDigitCharacters -> ArgumentException(reason |> toErrorText language, "x")
    | NonDashCharacter -> ArgumentException(reason |> toErrorText language, "x")
    | Modula11CheckFail -> ArgumentException(reason |> toErrorText language, "x")
    | InvalidLength -> ArgumentException(reason |> toErrorText language, "x")
    | InvalidDayInMonth -> ArgumentException(reason |> toErrorText language, "x")
    | InvalidMonth -> ArgumentException(reason |> toErrorText language, "x")
    | InvalidYear -> ArgumentException(reason |> toErrorText language, "x")
    | InvalidControl -> ArgumentException(reason |> toErrorText language, "x")
    | InvalidYearAndControl -> ArgumentException(reason |> toErrorText language, "x")
    | InvalidYearAndControlCombination -> ArgumentException(reason |> toErrorText language, "x")
  
/// <summary>
/// Convenience extension for C# usage
/// </summary>
[<Extension>]
type StringExtensions() =
  /// <summary>
  /// Validates an SSN. Designed for C# tuple deconstruction
  /// </summary>
  /// <param name="useModula11Check">Flag telling whether to utilize modula 11 check. Defaults to fals</param>  
  /// <param name="repairDayInMonth">Flag telling whether to repair the day in the month. Defaults to true</param>
  [<Extension>]
  static member IsValid(ssn, [<Optional; DefaultParameterValue(false)>] ?useModula11Check, [<Optional; DefaultParameterValue(true)>] ?repairDayInMonth)  =
    let modula11Check = defaultArg useModula11Check false
    let repair = defaultArg repairDayInMonth true
    match ssn |> validate modula11Check repair with
    | SSNDK.ValidationResult.Ok -> struct (true, ErrorReason.Ok)
    | SSNDK.ValidationResult.Error reason -> struct (false, reason |> toError)

  /// <summary>
  /// Validates an SSN. Designed for C# tuple deconstruction
  /// </summary>
  /// <param name="useModula11Check">Flag telling whether to utilize modula 11 check. Defaults to fals</param>  
  /// <param name="repairDayInMonth">Flag telling whether to repair the day in the month. Defaults to true</param>
  /// <param name="language">Flag telling in which language errors should be reported. Defaults to English< if not set by SetDefaultErrorLanguage/param>>
  [<Extension>]
  static member IsValidEx(ssn, [<Optional; DefaultParameterValue(false)>] ?useModula11Check, [<Optional; DefaultParameterValue(true)>] ?repairDayInMonth, [<Optional; DefaultParameterValue(ErrorTextLanguage.English)>] ?language)  =
    let modula11Check = defaultArg useModula11Check false
    let repair = defaultArg repairDayInMonth true
    let lang = defaultArg language (getDefaultErrorLanguage())
    match ssn |> validate modula11Check repair with
    | SSNDK.ValidationResult.Ok -> struct (true, null)
    | SSNDK.ValidationResult.Error reason -> (false, reason |> (toErrorText lang))

  /// <summary>
  /// Validates an SSN. Throws an <see cref="ArgumentException"/> upon failure
  /// </summary>
  /// <param name="useModula11Check">Flag telling whether to utilize modula 11 cheeck. Defaults to false</param>
  /// <param name="repairDayInMonth">Flag telling whether to repair the day in the month. Defaults to true</param>
  /// <param name="language">Flag telling in which language errors should be reported. Defaults to English< if not set by SetDefaultErrorLanguage/param>
  [<Extension>]
  static member ValidateAndThrowOnError(ssn, [<Optional; DefaultParameterValue(false)>] ?useModula11Check, [<Optional; DefaultParameterValue(true)>] ?repairDayInMonth, [<Optional; DefaultParameterValue(ErrorTextLanguage.English)>] ?language)  =
    let modula11Check = defaultArg useModula11Check false
    let repair = defaultArg repairDayInMonth true
    let lang = defaultArg language (getDefaultErrorLanguage())
    match ssn |> validate modula11Check repair with
    | SSNDK.ValidationResult.Ok -> ()
    | SSNDK.ValidationResult.Error reason -> lang |> (toException reason) |> raise

  /// <summary>
  /// Extracts the person behind an SSN. Designed for C# pattern-match-like utilization
  /// </summary>
  /// <param name="useModula11Check">Flag telling whether to utilize modula 11 cheeck. Defaults to false</param>
  /// <param name="repairDayInMonth">Flag telling whether to repair the day in the month. Defaults to true</param>
  [<Extension>]
  static member GetPerson(ssn, [<Optional; DefaultParameterValue(false)>] ?useModula11Check, [<Optional; DefaultParameterValue(true)>] ?repairDayInMonth) =
    let modula11Check = defaultArg useModula11Check false
    let repair = defaultArg repairDayInMonth true
    match ssn |> getPersonInfo modula11Check  repair with
    | SSNResult.Ok person -> 
      struct (true, ErrorReason.Ok, person.Gender |> toGender, person.DateOfBirth)
    | SSNResult.Error reason -> 
      struct (false, reason |> toError, Unchecked.defaultof<Gender>, Unchecked.defaultof<DateTimeOffset>)

  /// <summary>
  /// Extracts the person behind an SSN. Designed for C# tuple deconstruction
  /// </summary>
  /// <param name="useModula11Check">Flag telling whether to utilize modula 11 cheeck. Defaults to false</param>
  /// <param name="repairDayInMonth">Flag telling whether to repair the day in the month. Defaults to true</param>
  [<Extension>]
  static member GetPersonEx(ssn, [<Optional; DefaultParameterValue(false)>] ?useModula11Check, [<Optional; DefaultParameterValue(true)>] ?repairDayInMonth, [<Optional; DefaultParameterValue(ErrorTextLanguage.English)>] ?language) =
    let modula11Check = defaultArg useModula11Check false
    let repair = defaultArg repairDayInMonth true
    let lang = defaultArg language (getDefaultErrorLanguage())
    match ssn |> getPersonInfo modula11Check  repair with
    | SSNResult.Ok person -> struct (true, null, person.Gender |> toGender, person.DateOfBirth)
    | SSNResult.Error reason -> 
      struct (false, reason |> (toErrorText lang), Unchecked.defaultof<Gender>, Unchecked.defaultof<DateTimeOffset>)

  /// <summary>
  /// Extracts the person behind an SSN. Throws an <see cref="ArgumentException"/> upon failure
  /// </summary>
  /// <param name="useModula11Check">Flag telling whether to utilize modula 11 cheeck. Defaults to false</param>
  /// <param name="repairDayInMonth">Flag telling whether to repair the day in the month. Defaults to true</>
  /// <param name="language">Flag telling in which language errors should be reported. Defaults to English< if not set by SetDefaultErrorLanguage/param>
  [<Extension>]
  static member GetPersonAndThrowOnError(ssn, [<Optional; DefaultParameterValue(false)>] ?useModula11Check, [<Optional; DefaultParameterValue(true)>] ?repairDayInMonth, [<Optional; DefaultParameterValue(ErrorTextLanguage.English)>] ?language) =
    let modula11Check = defaultArg useModula11Check false
    let repair = defaultArg repairDayInMonth true
    let lang = defaultArg language (getDefaultErrorLanguage())
    match ssn |> getPersonInfo modula11Check  repair with
    | SSNResult.Ok person -> 
      struct (person.Gender |> toGender, person.DateOfBirth)
    | SSNResult.Error reason -> lang |> (toException reason) |> raise

[<Extension>]
type ErrorReasonExtensions() =
  /// <summary>
  /// Converts an ErrorReason to a text.
  /// </summary>
  /// <param name="reason">The ErrorReason to convert</param>
  /// <param name="language">Flag telling in which language errors should be reported. Defaults to English< if not set by SetDefaultErrorLanguage/param>>
  [<Extension>]
  static member ToText(reason, [<Optional; DefaultParameterValue(ErrorTextLanguage.English)>] ?language) =
    let lang = defaultArg language (getDefaultErrorLanguage())
    toErrorText lang reason

/// <summary>
/// Helper (static) class to handle default error reporting language
/// </summary>
[<AbstractClass; Sealed>]
type LanguageSettings() =
  /// <summary>
  /// Gets default error language
  /// </summary>
  static member GetdefaultErrorLanguage() = getDefaultErrorLanguage()
  /// <summary>
  /// Sets default error language
  /// </summary>
  /// <param name="language">The error language to utilize in error reporting (and exceptions)</param>
  static member SetDefaultErrorLanguage(language) =
    setDefaultErrorLanguage language