using System;
using Xunit;
using FsCheck;
using FsCheck.Xunit;
using SSNDKCS;
using static SSNDKCSharp.Test.TestHelpers;

namespace SSNDKCSharp.Test
{
    public class GetPersonAndThrowTest
    {
        [Property]
        [Trait("Category", "Unit")]
        public Property GetPersonAndThrow_works(DateTimeOffset x,
            bool dash, NonNegativeInt controlCode, bool useModula11Check, bool repair)
        {
            var c = controlCode.Get % 10000;
            var sut = GetSSN(x, dash, repair, c);

            try
            {
                var ok = sut.GetPersonAndThrow(useModula11Check, repair);

                var p = IsYearOk(ok.Person.DateOfBirth.Year, x.Year, c);

                var isValid = SSNDK.ValidationResult.Ok == SSNDK.validate(useModula11Check, sut);
                var c_ = sut[sut.Length - 1] - '0';
                var genderOk = ok.Person.Gender == (c_ % 2 == 0 ? Gender.Female : Gender.Male);
                var dayOk = ok.Person.DateOfBirth.Day == x.Day;
                var monthOk = ok.Person.DateOfBirth.Month == x.Month;
                var yearOk = IsYearOk(ok.Person.DateOfBirth.Year, x.Year, c);
                return (isValid && genderOk && dayOk && monthOk && yearOk).ToProperty();
            }
            catch
            {
            }

            return true.ToProperty();
        }

        #region Utility
        private static string GetSSN(DateTimeOffset x,
            bool dash, bool unrepair, int controlCode)
        {
            var dd = unrepair ? x.Day + 60 : x.Day;
            var mm = x.Month;
            var yy = x.Year % 100;

            var datePart = $"{dd.ToString("00")}{mm.ToString("00")}{yy.ToString("00")}";
            return dash ? $"{datePart}-{controlCode.ToString("0000")}" :
                $"{datePart}{controlCode.ToString("0000")}";
        }
        private static bool IsYearOk(int x, int y, int controlCode)
        {
            var birthYear = GetBirthYear(y % 100, controlCode);
            if (birthYear == -1)
                return false;
            return x == birthYear;
        }
        #endregion
    }
}
