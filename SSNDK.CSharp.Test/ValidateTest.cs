using System;
using System.Linq;
using Xunit;
using FsCheck;
using FsCheck.Xunit;
using SSNDKCS;
using static SSNDKCSharp.Test.TestHelpers;

namespace SSNDKCSharp.Test
{
    public class ValidateTest
    {
        [Property]
        [Trait("Category", "Unit")]
        public Property Validate_with_non_digits_work(DateTimeOffset x,
            bool dash, NonNegativeInt controlCode, bool useModula11Check)
        {
            var sut = GetSSNWithNoneDigits(x, dash, controlCode);

            switch (sut.Validate(useModula11Check))
            {
                case ValidationErrorResult r:
                    return (r.Error == ErrorReason.NonDigitCharacters).ToProperty();
                default:
                    return false.ToProperty();
            }
        }

        [Property]
        [Trait("Category", "Unit")]
        public Property Validate_with_dash_failure_work(DateTimeOffset x,
            NonNegativeInt controlCode, bool useModula11Check)
        {
            var sut = GetSSNWithDashFailure(x, controlCode);

            switch (sut.Validate(useModula11Check))
            {
                case ValidationErrorResult r:
                    return (r.Error == ErrorReason.NonDashCharacter).ToProperty();
                default:
                    return false.ToProperty();
            }
        }

        [Property]
        [Trait("Category", "Unit")]
        public Property Validate_work(DateTimeOffset x,
            bool dash, NonNegativeInt controlCode, bool useModula11Check)
        {
            var sut = GetSSN(x, dash, controlCode);

            switch (sut.Validate(useModula11Check))
            {
                case ValidationOkResult ok:
                    return true.ToProperty();
                case ValidationErrorResult r:
                    if (useModula11Check)
                    {
                        var indices = Enumerable.Range(0, 10)
                            .Select(n => n < 6 ? n : (dash ? n + 1 : n))
                            .ToArray();
                        var isModula11 = SSNDK.Helpers.sumOfProduct(indices, sut) % 11 == 0;
                        if (isModula11) return false.ToProperty();
                        else return (r.Error == ErrorReason.Modula11CheckFail).ToProperty();
                    }
                    return false.ToProperty();
                default:
                    return false.ToProperty();
            }
        }
    }
}
