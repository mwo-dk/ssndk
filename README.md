# ssndk

A minor package that helps working with danish social security numbers. The library is written in F#, but convenient C#-friendly extensions have been provided as well.

Simply install the package ssndk

Danish social security numbers are strings, that usually are represented as either ```DDMMYYNNNN``` or ```DDMMYY-NNNN```, where:

* ```DD``` is the day in the month of the date of birth of the person, which the number represents.
* ```MM``` is the month of the date of birth of the person, which the number represents.
* ```YY``` is the two-digit representation of the year of the daye of birth of the person, which the number represents. The century is determined according to [these rules (in danish)](https://www.cpr.dk/media/17534/personnummeret-i-cpr.pdf)
* ```NNNN``` is the four-digit control number. Even control numbers, represents females and odd numbers represent males. Besides that, the control number is utilized to determine the century in which the person is born, as well as computing the old modula 11 check.

## Usage

The package work on raw strings and exposes two functions:

1. ```validate: bool -> string -> ValidationResult```, performs simple validation of a given danish social security number. The arguments are:
* ```useModula11Check```, boolean flag telling whether to perform the modula 11 check, that was required in older social security numbers.
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
  | InvalidYear                        // The year is invalid
  | InvalidControl                     // The control number is invalid
  | InvalidYearAndControl              // The year and control numbers are invalid
  | InvalidYearAndControlCombination   // Essential unexpected error
  ```

2. ```getPersonInfo: bool -> bool -> string -> SSNResult```, validates and upon successfull validation extracts person info (gender and date of birth) of the person, which the number represents. The arguments are:
* ```useModula11Check```, boolean flag telling whether to perform the modula 11 check, that was required in older social security numbers.
* ```repairDateOfBirth```, boolean flag telling whether to repair the day in the month of the according to [this specification](https://www.cpr.dk/media/17535/erstatningspersonnummerets-opbygning.pdf)
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

Four extension methods (for string) have been provided for (namespace: ```SSNDKCS```):

1. ```Validate: string*Nullable<bool> -> ValidationResult```, utilizes ```validate``` above and returns a typed result, that fits well with C# pattern matching. The arguments are:
* ```ssn```, the social security number as a string.
* ```useModula11Check```, boolean flag telling whether to perform the modula 11 check, that was required in older social security numbers. This is optional and defaults to ```false```

  Using this method is in the line of:

  ```csharp
  using SSNDKCS;
  using static SSNDKCS.ErrorReason;

  switch (ssn.Validate(useModula11Check))
  {
      case ValidationOkResult ok:
        // Do whatever in case of success
        break;
      case ValidationErrorResult dang:
        var theReasonOfError = dang.Error;
        if (theResonOfError == Modula11CheckFail) // Do whatever...
        break;
      default:
        // Yikes
  }
  ```

2. ```GetPerson: string*Nullable<bool>*Nullable<bool> -> SSNResult```, utilizes ```getPersonInfo``` above and returns a typed result that fits well with C# pattern matching. The arguments are:
* ```ssn```, the social security number as a string.
* ```useModula11Check```, boolean flag telling whether to perform the modula 11 check, that was required in older social security numbers. This is optional and defaults to ```false```
* ```repairDateOfBirth```, boolean flag telling whether to repair the day in the month of the according to [this specification](https://www.cpr.dk/media/17535/erstatningspersonnummerets-opbygning.pdf). This is optional and defaults to ```true```

  Using this method is in the line of:

  ```csharp
  using SSNDKCS;
  using static SSNDKCS.ErrorReason;

  switch (ssn.GetPerson(useModula11Check, repair))
  {
      case SSNOkResult ok:
        var person = ok.Person;
        var gender = person.Gender;
        var dateOfBirth = person.DateOfBirth;
        // And on and on
      case SSNErrorResult error:var theReasonOfError = dang.Error;
        if (theResonOfError == Modula11CheckFail) // Do whatever...
        break;
      default:
        // Yikes
  }
  ```
3. ```ValidateAndThrow: string*Nullable<bool> -> ValidationOkResult```, utilizes ```validate``` above and returns a typed result, and throws an ```ArgumentException``` in case of error. The arguments are:
* ```ssn```, the social security number as a string.
* ```useModula11Check```, boolean flag telling whether to perform the modula 11 check, that was required in older social security numbers. This is optional and defaults to ```false```

  Using this method is in the line of:

  ```csharp
  using SSNDKCS;
  using static SSNDKCS.ErrorReason;

  try
  {
      var _ = ssn.ValidateAndThrow(useModula11Check);
  }
  catch (ArgumentException error)
  {
      // Log error or whatever
  }
  ```
4. ```GetPersonAndThrow: string*Nullable<bool>*Nullable<bool> -> SSNOkResult```, utilizes ```getPersonInfo``` above and returns a typed result that fits well with C# pattern matching. The arguments are:
* ```ssn```, the social security number as a string.
* ```useModula11Check```, boolean flag telling whether to perform the modula 11 check, that was required in older social security numbers. This is optional and defaults to ```false```
* ```repairDateOfBirth```, boolean flag telling whether to repair the day in the month of the according to [this specification](https://www.cpr.dk/media/17535/erstatningspersonnummerets-opbygning.pdf). This is optional and defaults to ```true```

Using this method is in the line of:

  ```csharp
  using SSNDKCS;
  using static SSNDKCS.ErrorReason;

  try
  {
      var result = ssn.GetPersonAndThrow(useModula11Check, repairDateOfBirth);
  }
  catch (ArgumentException error)
  {
      // Log error or whatever
  }
  ```