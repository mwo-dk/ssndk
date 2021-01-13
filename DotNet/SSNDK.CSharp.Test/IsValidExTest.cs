using System;
using System.Linq;
using Xunit;
using FsCheck;
using FsCheck.Xunit;
using static SSNDKCSharp.Test.TestHelpers;
using ssndk.CSharp;

namespace SSNDKCSharp.Test
{
    public class IsValidExTest
    {
        [Property]
        [Trait("Category", "Unit")]
        public Property IsValidEx_with_non_digits_work(DateTimeOffset x,
            bool dash, NonNegativeInt controlCode, bool useModula11Check)
        {
            var sut = GetSSNWithNoneDigits(x, dash, controlCode);

            var (success, error) = sut.IsValidEx(useModula11Check, false, ErrorTextLanguage.English);

            var result = !success && 
                error == "The argument contains non digit characters where digits are expected";
            return result.ToProperty();
        }

        [Property]
        [Trait("Category", "Unit")]
        public Property IsValidEx_with_dash_failure_work(DateTimeOffset x,
            NonNegativeInt controlCode, bool useModula11Check)
        {
            var sut = GetSSNWithDashFailure(x, controlCode);

            var (success, error) = sut.IsValidEx(useModula11Check, false, ErrorTextLanguage.English);

            var result = !success && error == "The argument contains a non dash character where a dash was expected";
            return result.ToProperty();
        }
    }
}
