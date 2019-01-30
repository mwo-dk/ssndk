using System;
using System.Linq;
using Xunit;
using FsCheck;
using FsCheck.Xunit;
using SSNDKCS;
using static SSNDKCSharp.Test.TestHelpers;

namespace SSNDKCSharp.Test
{
    public class IsValidTest
    {
        [Property]
        [Trait("Category", "Unit")]
        public Property IsValid_with_non_digits_work(DateTimeOffset x,
            bool dash, NonNegativeInt controlCode, bool useModula11Check)
        {
            var sut = GetSSNWithNoneDigits(x, dash, controlCode);

            return sut.IsValid(useModula11Check).ToProperty();
        }

        [Property]
        [Trait("Category", "Unit")]
        public Property IsValid_with_dash_failure_work(DateTimeOffset x,
            NonNegativeInt controlCode, bool useModula11Check)
        {
            var sut = GetSSNWithDashFailure(x, controlCode);

            return (!sut.IsValid(useModula11Check)).ToProperty();
        }
    }
}
