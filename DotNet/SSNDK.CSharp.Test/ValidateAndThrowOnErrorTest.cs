using System;
using System.Linq;
using Xunit;
using FsCheck;
using FsCheck.Xunit;
using SSNDKCS;
using static SSNDKCSharp.Test.TestHelpers;

namespace SSNDKCSharp.Test
{
    public class ValidateAndThrowOnErrorTest
    {
        [Property]
        [Trait("Category", "Unit")]
        public Property ValidateAndThrow_with_non_digits_work(DateTimeOffset x,
            bool dash, NonNegativeInt controlCode, bool useModula11Check)
        {
            var sut = GetSSNWithNoneDigits(x, dash, controlCode);

            try
            {
                sut.ValidateAndThrowOnError(useModula11Check, false, ErrorTextLanguage.English);
            }
            catch (ArgumentException error)
            {
                return (error.Message.StartsWith("The argument contains non digit characters where digits are expected")).ToProperty();
            }
            catch
            {
            }

            return false.ToProperty();
        }

        [Property]
        [Trait("Category", "Unit")]
        public Property ValidateAndThrow_with_dash_failure_work(DateTimeOffset x,
            NonNegativeInt controlCode, bool useModula11Check)
        {
            var sut = GetSSNWithDashFailure(x, controlCode);

            try
            {
                sut.ValidateAndThrowOnError(useModula11Check, false, ErrorTextLanguage.English);
            }
            catch (ArgumentException error)
            {
                return (error.Message.StartsWith("The argument contains a non dash character where a dash was expected")).ToProperty();
            }
            catch
            {
            }

            return false.ToProperty();
        }

        [Property]
        [Trait("Category", "Unit")]
        public Property ValidateAndThrow_work(DateTimeOffset x,
            bool dash, NonNegativeInt controlCode, bool useModula11Check)
        {
            var sut = GetSSN(x, dash, controlCode);

            try
            {
                sut.ValidateAndThrowOnError(useModula11Check, false, ErrorTextLanguage.English);
            }
            catch (ArgumentException error)
            {
                if (useModula11Check)
                {
                    var indices = Enumerable.Range(0, 10)
                        .Select(n => n < 6 ? n : (dash ? n + 1 : n))
                        .ToArray();
                    var isModula11 = SSNDK.Helpers.sumOfProduct(indices, sut) % 11 == 0;
                    if (isModula11) return false.ToProperty();
                    else return (error.Message.StartsWith("The modula 11 check failed")).ToProperty();
                }
                return false.ToProperty();
            }
            catch
            {
                return false.ToProperty();
            }

            return true.ToProperty();
        }
    }
}
