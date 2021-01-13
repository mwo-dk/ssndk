using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace ssndk.CSharp
{
    /// <summary>
    /// Represents an error reason
    /// </summary>
    public enum ErrorReason
    {
        /// <summary>
        /// No error occurred
        /// </summary>
        Ok = 0,
        /// <summary>
        /// The value was null, empty or white space
        /// </summary>
        NullEmptyOrWhiteSpace = 1,
        /// <summary>
        /// The value contained non-digit characters, where characters were expected
        /// </summary>
        NonDigitCharacters = 2,
        /// <summary>
        /// The value contained a non-dash character where a dash was expected
        /// </summary>
        NonDashCharacter = 3,
        /// <summary>
        /// The modula 11 check failed
        /// </summary>
        Modula11CheckFail = 4,
        /// <summary>
        /// The trimmed range has invalid length
        /// </summary>
        InvalidLength = 5,
        /// <summary>
        /// The given day in the month is invalid
        /// </summary>
        InvalidDayInMonth = 6,
        /// <summary>
        /// The given month is invalid
        /// </summary>
        InvalidMonth = 7,
        /// <summary>
        /// The year is invalid
        /// </summary>
        InvalidYear = 8,
        /// <summary>
        /// The control number is invalid
        /// </summary>
        InvalidControl = 9,
        /// <summary>
        /// The year and control numbers are invalid
        /// </summary>
        InvalidYearAndControl = 10,
        /// <summary>
        /// Essential unexpected error
        /// </summary>
        InvalidYearAndControlCombination = 11
    }

    /// <summary>
    /// Represents the gender of a person
    /// </summary>
    public enum Gender
    {
        /// <summary>
        /// Represents a male
        /// </summary>
        Male = 1,
        /// <summary>
        /// Represents a female
        /// </summary>
        Female = 2
    }

    /// <summary>
    /// Represents the language of the error texts produced
    /// </summary>
    public enum ErrorTextLanguage
    {
        /// <summary>
        /// English
        /// </summary>
        English = 1,
        /// <summary>
        /// Danish
        /// </summary>
        Danish = 2
    }

    /// <summary>
    /// Extensions for danish SSN's
    /// </summary>
    public static class Extensions
    {
        private static readonly (SSNDK.ErrorReason, ErrorReason)[] ErrorReasonMap = new[]
        {
            (SSNDK.ErrorReason.NullEmptyOrWhiteSpace, ErrorReason.NullEmptyOrWhiteSpace),
            (SSNDK.ErrorReason.NonDigitCharacters, ErrorReason.NonDigitCharacters),
            (SSNDK.ErrorReason.NonDashCharacter, ErrorReason.NonDashCharacter),
            (SSNDK.ErrorReason.Modula11CheckFail, ErrorReason.Modula11CheckFail),
            (SSNDK.ErrorReason.InvalidLength, ErrorReason.InvalidLength),
            (SSNDK.ErrorReason.InvalidDayInMonth, ErrorReason.InvalidDayInMonth),
            (SSNDK.ErrorReason.InvalidMonth, ErrorReason.InvalidMonth),
            (SSNDK.ErrorReason.InvalidYear, ErrorReason.InvalidYear),
            (SSNDK.ErrorReason.InvalidControl, ErrorReason.InvalidControl),
            (SSNDK.ErrorReason.InvalidYearAndControl, ErrorReason.InvalidYearAndControl),
            (SSNDK.ErrorReason.InvalidYearAndControlCombination, ErrorReason.InvalidYearAndControlCombination),
        };

        /// <summary>
        /// Maps <paramref name="error"/> to an <see cref="ErrorReason"/>
        /// </summary>
        /// <param name="error">The <see cref="SSNDK.ErrorReason"/> to map</param>
        /// <returns>The resulting <see cref="ErrorReason"/></returns>
        public static ErrorReason ToError(this SSNDK.ErrorReason error)
        {
            if (error is null)
                throw new ArgumentNullException(nameof(error));

            foreach (var (x, y) in ErrorReasonMap)
                if (error == x)
                    return y;
            return ErrorReason.Ok;
        }

        private static readonly (SSNDK.Gender, Gender)[] GenderMap = new[]
        {
            (SSNDK.Gender.Male, Gender.Male),
            (SSNDK.Gender.Female, Gender.Female)
        };

        /// <summary>
        /// Maps <paramref name="gender"/> to a <see cref="Gender"/>
        /// </summary>
        /// <param name="gender">The <see cref="SSNDK.Gender"/> to map</param>
        /// <returns>The resulting <see cref="Gender"/></returns>
        public static Gender ToGender(this SSNDK.Gender gender)
        {
            if (gender is null)
                throw new ArgumentNullException(nameof(gender));

            foreach (var (x, y) in GenderMap)
                if (gender == x)
                    return y;

            throw new ArgumentOutOfRangeException(nameof(gender));
        }

        private static readonly (SSNDK.ErrorReason, string)[] EnglishErrorTexts = new[]
        {
            (SSNDK.ErrorReason.NullEmptyOrWhiteSpace, "The argument is null, empty or white-space"),
            (SSNDK.ErrorReason.NonDigitCharacters, "The argument contains non digit characters where digits are expected"),
            (SSNDK.ErrorReason.NonDashCharacter, "The argument contains a non dash character where a dash was expected"),
            (SSNDK.ErrorReason.Modula11CheckFail, "The modula 11 check failed"),
            (SSNDK.ErrorReason.InvalidLength, "The lenght of the trimmed argument is wrong. Only 10 and 11 are accepted"),
            (SSNDK.ErrorReason.InvalidDayInMonth, "The day in the given month is invalid"),
            (SSNDK.ErrorReason.InvalidMonth, "The month is invalid"),
            (SSNDK.ErrorReason.InvalidYear, "The year is invalid"),
            (SSNDK.ErrorReason.InvalidControl, "The control number is invalid"),
            (SSNDK.ErrorReason.InvalidYearAndControl, "The year and control numbers are invalid"),
            (SSNDK.ErrorReason.InvalidYearAndControlCombination, "Argument is invalid"),
        };

        /// <summary>
        /// Maps <paramref name="error"/> to an english error test
        /// </summary>
        /// <param name="error">The <see cref="SSNDK.ErrorReason"/> to map</param>
        /// <returns>The resulting english error text</returns>
        public static string ToEnglishErrorText(this SSNDK.ErrorReason error)
        {
            if (error is null)
                throw new ArgumentNullException(nameof(error));

            foreach (var (x, y) in EnglishErrorTexts)
                if (error == x)
                    return y;

            throw new ArgumentOutOfRangeException(nameof(error));
        }

        private static readonly (SSNDK.ErrorReason, string)[] DanishErrorTexts = new[]
        {
            (SSNDK.ErrorReason.NullEmptyOrWhiteSpace, "Argumentet er enten null, tomt eller indeholder \"white-space\""),
            (SSNDK.ErrorReason.NonDigitCharacters, "Argumentet indeholder bogstaver, der ikke er cifre, hvor disse er forventede"),
            (SSNDK.ErrorReason.NonDashCharacter, "Argumentet indeholder et bogstav, der ikke er en binde-streg, hvor dette er forventet"),
            (SSNDK.ErrorReason.Modula11CheckFail, "Modula-11 tjekket fejlede"),
            (SSNDK.ErrorReason.InvalidLength, "Længden af det \"trimmede\" argument er forkert. Efter at mellemrum er fjernet i starten og slutningen, skal længden af det tilbageværende, være enten 10 eller 11 bogstaver"),
            (SSNDK.ErrorReason.InvalidDayInMonth, "Dagen i månedet er ikke validt"),
            (SSNDK.ErrorReason.InvalidMonth, "Måneden er ikke valid"),
            (SSNDK.ErrorReason.InvalidYear, "Året er ikke validt"),
            (SSNDK.ErrorReason.InvalidControl, "Kontrol-nummeret er ikke validt"),
            (SSNDK.ErrorReason.InvalidYearAndControl, "Kombinationen af år og kontrol-nummer er ikke valid"),
            (SSNDK.ErrorReason.InvalidYearAndControlCombination, "Argumentet er ikke validt"),
        };

        /// <summary>
        /// Maps <paramref name="error"/> to an danish error test
        /// </summary>
        /// <param name="error">The <see cref="SSNDK.ErrorReason"/> to map</param>
        /// <returns>The resulting danish error text</returns>
        public static string ToDanishErrorText(this SSNDK.ErrorReason error)
        {
            if (error is null)
                throw new ArgumentNullException(nameof(error));

            foreach (var (x, y) in DanishErrorTexts)
                if (error == x)
                    return y;

            throw new ArgumentOutOfRangeException(nameof(error));
        }

        private static ErrorTextLanguage DefaultErrorTextLanguage = ErrorTextLanguage.English;
        /// <summary>
        /// The error text language
        /// </summary>
        public static ErrorTextLanguage ErrorTextLanguage = DefaultErrorTextLanguage;

        /// <summary>
        /// Maps a given <see cref="SSNDK.ErrorReason"/> to a human readable error
        /// </summary>
        /// <param name="error">The <see cref="SSNDK.ErrorReason"/> to map</param>
        /// <returns>The resulting error text</returns>
        public static string ToErrorText(this SSNDK.ErrorReason error) =>
            ErrorTextLanguage switch
            {
                ErrorTextLanguage.English => error.ToEnglishErrorText(),
                ErrorTextLanguage.Danish => error.ToDanishErrorText(),
                _ => throw new ArgumentOutOfRangeException(nameof(error))
            };

        private static (SSNDK.ErrorReason, Func<SSNDK.ErrorReason, ArgumentException>)[] ErrorToExceptionGeneratorMappers = new (SSNDK.ErrorReason, Func<SSNDK.ErrorReason, ArgumentException>)[]
        {
            (SSNDK.ErrorReason.NullEmptyOrWhiteSpace, _ => new ArgumentNullException()),
            (SSNDK.ErrorReason.NonDigitCharacters, _ => new ArgumentException()),
            (SSNDK.ErrorReason.NonDashCharacter, _ => new ArgumentNullException()),
            (SSNDK.ErrorReason.Modula11CheckFail, _ => new ArgumentNullException()),
            (SSNDK.ErrorReason.InvalidLength, _ => new ArgumentNullException()),
            (SSNDK.ErrorReason.InvalidDayInMonth, _ => new ArgumentNullException()),
            (SSNDK.ErrorReason.InvalidMonth, _ => new ArgumentNullException()),
            (SSNDK.ErrorReason.InvalidYear, _ => new ArgumentNullException()),
            (SSNDK.ErrorReason.InvalidControl, _ => new ArgumentNullException()),
            (SSNDK.ErrorReason.InvalidYearAndControl, _ => new ArgumentNullException()),
            (SSNDK.ErrorReason.InvalidYearAndControlCombination, _ => new ArgumentNullException()),
        };

        /// <summary>
        /// Maps a given <see cref="SSNDK.ErrorReason"/> to an <see cref="ArgumentException"/>
        /// </summary>
        /// <param name="error">The <see cref="SSNDK.ErrorReason"/> to map</param>
        /// <returns>The resulting <see cref="ArgumentException"/></returns>
        public static ArgumentException ToException(this SSNDK.ErrorReason error)
        {
            if (error == SSNDK.ErrorReason.NullEmptyOrWhiteSpace)
                return new ArgumentNullException();
            return new ArgumentException(error.ToErrorText());
        }

        /// <summary>
        /// Validates the provided <paramref name="ssn"/>
        /// </summary>
        /// <param name="ssn">The SSN to validate</param>
        /// <param name="useModula11Check">Flag telling whether to use modula-11 check</param>
        /// <param name="repairDayInMonth">Flag telling whether to repair the day in the month</param>
        /// <returns>Result of the validation</returns>
        public static (bool Success, ErrorReason Error) IsValid(this string ssn, bool useModula11Check = false, bool repairDayInMonth = true)
        {
            var result = SSNDK.validate(useModula11Check, repairDayInMonth, ssn);
            if (result == SSNDK.ValidationResult.Ok) return (true, default);
            else
            {
                var innerError = (result as SSNDK.ValidationResult.Error).Item;
                return (false, innerError.ToError());
            }
        }

        /// <summary>
        /// Validates the provided <paramref name="ssn"/>
        /// </summary>
        /// <param name="ssn">The SSN to validate</param>
        /// <param name="useModula11Check">Flag telling whether to use modula-11 check</param>
        /// <param name="repairDayInMonth">Flag telling whether to repair the day in the month</param>
        /// <param name="language">The language in which to format error texts</param>
        /// <returns>Result of the validation</returns>
        public static (bool Success, string ErrorMessage) IsValidEx(this string ssn, bool useModula11Check = false, bool repairDayInMonth = true, ErrorTextLanguage language = ErrorTextLanguage.English)
        {
            var result = SSNDK.validate(useModula11Check, repairDayInMonth, ssn);
            if (result == SSNDK.ValidationResult.Ok) return (true, default);
            else
            {
                var innerError = (result as SSNDK.ValidationResult.Error).Item;
                return (false, language == ErrorTextLanguage.English ? innerError.ToEnglishErrorText() : innerError.ToDanishErrorText());
            }
        }

        /// <summary>
        /// Validates the provided <paramref name="ssn"/>
        /// </summary>
        /// <param name="ssn">The SSN to validate</param>
        /// <param name="useModula11Check">Flag telling whether to use modula-11 check</param>
        /// <param name="repairDayInMonth">Flag telling whether to repair the day in the month</param>
        public static void ValidateAndThrowOnError(this string ssn, bool useModula11Check = false, bool repairDayInMonth = true)
        {
            var result = SSNDK.validate(useModula11Check, repairDayInMonth, ssn);
            if (result != SSNDK.ValidationResult.Ok)
            {
                var innerError = (result as SSNDK.ValidationResult.Error).Item;
                throw innerError.ToException();
            }    
        }

        /// <summary>
        /// Retrieves <see cref="Gender"/> and date of birth from the provided <paramref name="ssn"/>
        /// </summary>
        /// <param name="ssn">The SSN</param>
        /// <param name="useModula11Check">Flag telling whether to use modula-11 check</param>
        /// <param name="repairDayInMonth">Flag telling whether to repair the day in the month</param>
        /// <returns></returns>
        public static (bool Success, ErrorReason Error, Gender Gender, DateTimeOffset DateOfBirth) GetPerson(this string ssn, bool useModula11Check = false, bool repairDayInMonth = true)
        {
            var result = SSNDK.getPersonInfo(useModula11Check, repairDayInMonth, ssn);
            if (result is SSNDK.SSNResult.Ok)
            {
                var person = (result as SSNDK.SSNResult.Ok).Item;
                return (true, ErrorReason.Ok, person.Gender.ToGender(), person.DateOfBirth);
            }
            else
            {
                var innerError = (result as SSNDK.SSNResult.Error).Item;
                return (false, innerError.ToError(), default, default);
            }
        }

        /// <summary>
        /// Retrieves <see cref="Gender"/> and date of birth from the provided <paramref name="ssn"/>
        /// </summary>
        /// <param name="ssn">The SSN</param>
        /// <param name="useModula11Check">Flag telling whether to use modula-11 check</param>
        /// <param name="repairDayInMonth">Flag telling whether to repair the day in the month</param>
        /// <returns></returns>
        public static (bool Success, string ErrorMessage, Gender Gender, DateTimeOffset DateOfBirth) GetPersonEx(this string ssn, bool useModula11Check = false, bool repairDayInMonth = true)
        {
            var result = SSNDK.getPersonInfo(useModula11Check, repairDayInMonth, ssn);
            if (result is SSNDK.SSNResult.Ok)
            {
                var person = (result as SSNDK.SSNResult.Ok).Item;
                return (true, default, person.Gender.ToGender(), person.DateOfBirth);
            }
            else
            {
                var innerError = (result as SSNDK.SSNResult.Error).Item;
                return (false, innerError.ToErrorText(), default, default);
            }
        }
    }
}
