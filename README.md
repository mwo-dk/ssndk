# ssndk

A minor package that helps working with danish social security numbers. The library is written in F#, but convenient C#-friendly extensions have been provided as well.

For .Net utilization, simply install the package ssndk from [nuget.org](https://www.nuget.org/packages/SSNDK). Haskell source code is also available - perhabs a package will be deployed on [hackage](https://hackage.haskell.org/)

Danish social security numbers are strings, that usually are represented as either ```DDMMYYNNNN``` or ```DDMMYY-NNNN```, where:

* ```DD``` is the day in the month of the date of birth of the person, which the number represents.
* ```MM``` is the month of the date of birth of the person, which the number represents.
* ```YY``` is the two-digit representation of the year of the daye of birth of the person, which the number represents. The century is determined according to [these rules (in danish)](https://www.cpr.dk/media/17534/personnummeret-i-cpr.pdf)
* ```NNNN``` is the four-digit control number. Even control numbers, represents females and odd numbers represent males. Besides that, the control number is utilized to determine the century in which the person is born, as well as computing the old modula 11 check.

## Usage

The package work on raw strings and exposes two functions:

1. ```validate: bool -> string -> ValidationResult```, performs simple validation of a given danish social security number. The arguments are:
* ```useModula11Check```, boolean flag telling whether to perform the modula 11 check, that was required in older social security numbers.
* ```repairDayInMonth```, boolean flag telling whether to repair the day in the month of the according to [this specification](https://www.cpr.dk/media/17535/erstatningspersonnummerets-opbygning.pdf)
* ```ssn```, the social security number as a string.

  The outcome of the validation is represented by ```validationResult```
  ```fsharp
  /// <summary>
  /// Represents the outcome of a validation
  /// </summary>
  type ValidationResult =
  | Ok                                       // The validation succeeded
  | Error of ErrorReason                     // The validation failed
  ```

  where ```ErrorReason``` is

  ```fsharp
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
  ```
2. ```isValid: bool -> bool -> string -> bool```, simple yes/no validation of ssn
* ```useModula11Check```, boolean flag telling whether to perform the modula 11 check, that was required in older social security numbers.
* ```repairDayInMonth```, boolean flag telling whether to repair the day in the month of the according to [this specification](https://www.cpr.dk/media/17535/erstatningspersonnummerets-opbygning.pdf)
* ```ssn```, the social security number as a string.

3. ```getPersonInfo: bool -> bool -> string -> SSNResult```, validates and upon successfull validation extracts person info (gender and date of birth) of the person, which the number represents. The arguments are:
* ```useModula11Check```, boolean flag telling whether to perform the modula 11 check, that was required in older social security numbers.
* ```repairDayInMonth```, boolean flag telling whether to repair the day in the month of the according to [this specification](https://www.cpr.dk/media/17535/erstatningspersonnummerets-opbygning.pdf)
* ```ssn```, the social security number as a string.

  The outcome is represented by ```SSNResult```
  ```fsharp
  /// <summary>
  /// Represents the gender of a person
  /// </summary>
  type Gender =
  | Male                               // Represents a male
  | Female                             // Represents a female

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
  ```
## Usage C#

Besides the core F# library a group of extension methods have been provided for C# convenience. They are grouped as:

* Extension methods for strings of social security numbers. There is basically two functionalities: simple yes/no validation and then methods to extract gender and date of birth. They come in various flavors depending on preferred usage pattern:
  1. Return a ```System.ValueTuple``` denoting success and or error reasons and results.
  2. Invoke, get result (if not just basic validation) and then throw an ```ArgumentException``` in case the validation fails.

  These methods all take at least two boolean parameters, marking whether to perform the old modula 11 check as well as to repair the day in month of the birth date - see links below.
* Extension method to format the error returned in either Danish or English.
* Extension methods to handle default languages.

### Extension methods for strings of Social Security Numbers

Six extension methods (for string) have been provided for (namespace: ```SSNDKCS```):

1. ```IsValid: string*Nullable<bool>*Nullable<bool> -> bool*ErrorReason```, utilizes ```validate``` above and returns a ```System.ValueTuple```, that fits well with C# [tuple deconstruction](https://docs.microsoft.com/en-us/dotnet/csharp/deconstruct). The arguments are:
* ```ssn```, the social security number as a string.
* ```useModula11Check```, boolean flag telling whether to perform the modula 11 check, that was required in older social security numbers. This is optional and defaults to ```false```
* ```repairDayInMonth```, boolean flag telling whether to repair the day in the month of the according to [this specification](https://www.cpr.dk/media/17535/erstatningspersonnummerets-opbygning.pdf). This is optional and defaults to ```true```

  Using this method is in the line of:

  ```csharp
  using SSNDKCS;

  // If your care little about the error-reason, then is.
  var (success, _) = ssn.IsValid(useModula11Check, repairDayInMonth);
  // Handle the boolean outcome of success, or...

  // If you need the error reason, then...
  var (success, reason) = ssn.IsValid(useModula11Check, repairDayInMonth);
  // Handle errors if success if false.
  ```
  
  The error returned above is one of:

  ```csharp
  public enum ErrorReason
  {
    Ok,
    NullEmptyOrWhiteSpace,
    NonDigitCharacters,
    NonDashCharacter,
    Modula11CheckFail,
    InvalidLength,
    InvalidDayInMonth,
    InvalidMonth,
    InvalidYear,
    InvalidControl,
    InvalidYearAndControl,
    InvalidYearAndControlCombination
  }
  ```
  As can be seen, it matches the similar union case in the F# implementation with an extre ```Ok``` (== 0) value, which is retuerned in case ```success``` is true.

2. ```IsValidEx: string*Nullable<bool>*Nullable<bool>*Nullable<ErrorTextLanguage> -> bool*string```, utilizes ```validate``` above and returns a ```System.ValueTuple```, that fits well with C# [tuple deconstruction](https://docs.microsoft.com/en-us/dotnet/csharp/deconstruct). The arguments are:
* ```ssn```, the social security number as a string.
* ```useModula11Check```, boolean flag telling whether to perform the modula 11 check, that was required in older social security numbers. This is optional and defaults to ```false```
* ```repairDayInMonth```, boolean flag telling whether to repair the day in the month of the according to [this specification](https://www.cpr.dk/media/17535/erstatningspersonnummerets-opbygning.pdf). This is optional and defaults to ```true```
* ```language```, flag telling which language to convert error messages to. If not provided, nor set via the static method
SSNDKCS.LanguageSettings.SetDefaultErrorLanguage, then it defaults to english

  Using this method is in the line of:

  ```csharp
  using SSNDKCS;

  // If your care little about the error-reason, then is.
  var (success, _) = ssn.IsValid(useModula11Check, repairDayInMonth);
  // Handle the boolean outcome of success, or...

  var (success, errorMessage) = ssn.IsValid(useModula11Check, repairDayInMonth);
  // Log message if success is false - or somilar
  ```

  The error text language is one of:

  ```csharp
  public enum ErrorTextLanguage
  {
    English = 1,
    Danish
  }
  ```

3. ```ValidateAndThrowOnError: string*Nullable<bool>*Nullable<bool>*Nullable<ErrorTextLanguage> -> unit```, utilizes ```validate``` above, and throws an ```ArgumentException``` in case of validation failure. The arguments are:
* ```ssn```, the social security number as a string.
* ```useModula11Check```, boolean flag telling whether to perform the modula 11 check, that was required in older social security numbers. This is optional and defaults to ```false```
* ```repairDayInMonth```, boolean flag telling whether to repair the day in the month of the according to [this specification](https://www.cpr.dk/media/17535/erstatningspersonnummerets-opbygning.pdf). This is optional and defaults to ```true```
* ```language```, flag telling which language to convert error messages to. If not provided, nor set via the static method
SSNDKCS.LanguageSettings.SetDefaultErrorLanguage, then it defaults to english

  Using this method is in the line of:

  ```csharp
  using SSNDKCS;

  try
  {
      ssn.ValidateAndThrowOnError(useModula11Check, repairDayInMonth);
  }
  catch (ArgumentException error)
  {
      // Log error or whatever
  }
  ```

4. ```GetPerson: string*Nullable<bool>*Nullable<bool> -> bool*ErrorReason*Gender*DateTimeOffset```, utilizes ```getPersonInfo``` above and returns a ```System.ValueTuple```, that fits well with C# [tuple deconstruction](https://docs.microsoft.com/en-us/dotnet/csharp/deconstruct). The arguments are:
* ```ssn```, the social security number as a string.
* ```useModula11Check```, boolean flag telling whether to perform the modula 11 check, that was required in older social security numbers. This is optional and defaults to ```false```
* ```repairDayInMonth```, boolean flag telling whether to repair the day in the month of the according to [this specification](https://www.cpr.dk/media/17535/erstatningspersonnummerets-opbygning.pdf). This is optional and defaults to ```true```

  Using this method is in the line of:

  ```csharp
  using SSNDKCS;
  using static SSNDKCS.ErrorReason;

  var (success, error, gender, dateOfBirth) = ssn.GetPerson(useModula11Check, repairDayInMonth);
  if (success)
    // Handle success case, error is Ok and gender and dateOfBirth have sensible values
  else
    // Inspect error and act accordingly
  ```

  ```gender``` is of type ```Gender``` is a C# enumeration, matchin the F# union case above:

  ```csharp
  public enum Gender
  {
	  Male = 1,
	  Female = 2
  }
  ```

5. ```GetPersonEx: string*Nullable<bool>*Nullable<bool> -> bool*string*Gender*DateTimeOffset```, utilizes ```getPersonInfo``` above and returns a ```System.ValueTuple```, that fits well with C# [tuple deconstruction](https://docs.microsoft.com/en-us/dotnet/csharp/deconstruct). The arguments are:
* ```ssn```, the social security number as a string.
* ```useModula11Check```, boolean flag telling whether to perform the modula 11 check, that was required in older social security numbers. This is optional and defaults to ```false```
* ```repairDayInMonth```, boolean flag telling whether to repair the day in the month of the according to [this specification](https://www.cpr.dk/media/17535/erstatningspersonnummerets-opbygning.pdf). This is optional and defaults to ```true```
```language```, flag telling which language to convert error messages to. If not provided, nor set via the static method
SSNDKCS.LanguageSettings.SetDefaultErrorLanguage, then it defaults to english

  Using this method is in the line of:

   ```csharp
  using SSNDKCS;

  // If your care little about the error-reason, then is.
  var (success, errorMessage, gender, dateOfBirth) = ssn.GetPersonEx(useModula11Check);
  if (success)
    // Use that person
  else 
    // Don't use it. Log the error, or similar.
  ```

6. ```GetPersonAndThrowOnError: string*Nullable<bool>*Nullable<bool>*Nullable<ErrorTextLanguage> -> Gender*DateTimeOffset```, utilizes ```getPersonInfo``` above and returns a ```System.ValueTuple```, that fits well with C# [tuple deconstruction](https://docs.microsoft.com/en-us/dotnet/csharp/deconstruct). The arguments are:
* ```ssn```, the social security number as a string.
* ```useModula11Check```, boolean flag telling whether to perform the modula 11 check, that was required in older social security numbers. This is optional and defaults to ```false```
* ```repairDateOfBirth```, boolean flag telling whether to repair the day in the month of the according to [this specification](https://www.cpr.dk/media/17535/erstatningspersonnummerets-opbygning.pdf). This is optional and defaults to ```true```
```language```, flag telling which language to convert error messages to. If not provided, nor set via the static method
SSNDKCS.LanguageSettings.SetDefaultErrorLanguage, then it defaults to english

  Using this method is in the line of:

    ```csharp
    using SSNDKCS;

    try
    {
        var (gender, dateOfBirth) = ssn.GetPersonAndThrowOnError(useModula11Check, repairDateOfBirth);
    }
    catch (ArgumentException error)
    {
        // Log error or whatever
    }
    ```
### Extension method for formatting errors

A single extension method (for ErrorReason) have been provided for (namespace: ```SSNDKCS```):
1. ```ToText: ErrorReason -> string```, converts an ```ErrorReason``` to a ```string```. The arguments are:
* ```language```, flag telling which language to convert error messages to. If not provided, nor set via the static method
SSNDKCS.LanguageSettings.SetDefaultErrorLanguage, then it defaults to english

  Using this method is in the line of:

  ```csharp
  using SSNDKCS;

  // Gimme that reason then (ie from the field after calling Validate or others returning an ErrorReason).
  var errorText = ssn.ToText(reason);
  ```

### Extension methods to setup default lanaguage formatting

Besides this a static class to 'ambient' setup of the error reporting language has been provided. The name is ```LanguageSettings``` (namespace: ```SSNDKCS```). The two methods are:
1. ```GetDefaultErrorLanguage: unit -> ErrorTextLanguage```, gets the default error reporting language. sss
2. ```SetDefaultErrorLanguage: ErrorTextLanguage -> unit```, sets the default error reporting language. The argument is:
* ```language```, flag telling which language to convert error messages to by default in the above mentioned extension methods for ```string``` and ```ErrorReason```

  Using these methods is in the line of:

  ```csharp
  using SSNDKCS;
  using static SSNDKCS.LanguageSettings;

  var thatDefaultLanguageIsSetTo = GetDefaultErrorLanguage()
  /// .... and on and on

  SetDefaultErrorLanguage(ErrorTextLanguage.Danish)
  ```

  Mind that, in non-typesafe-languges (ie. wrt enums) you can provide invalid values (like: ``` (ErrorTextLanguage)666```), and consecutively enjoy that things fail. We do not bother - if you do, all error messages will format to: ```"Hnnng? What language is that?"```, and like other features such as nullability, non-immutability, no discriminated unions etc, we pass on your freedom to enjoy. So: enjoy.