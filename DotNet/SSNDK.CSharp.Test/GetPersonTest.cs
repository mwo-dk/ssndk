using System;
using Xunit;
using FsCheck;
using FsCheck.Xunit;
using static SSNDKCSharp.Test.TestHelpers;
using ssndk.CSharp;

namespace SSNDKCSharp.Test
{
    public class GetPersonTest
    {
        [Property]
        [Trait("Category", "Unit")]
        public Property GetPerson_works(DateTimeOffset x,
            bool dash, NonNegativeInt controlCode, bool useModula11Check, bool repair)
        {
            var c = controlCode.Get % 10000;
            var sut = GetSSN(x, dash, repair, c);

            var (success, error, gender, dateOfBirth) = sut.GetPerson(useModula11Check, repair);

            if (success)
            {
                var p = IsYearOk(dateOfBirth.Year, x.Year, c);

                var isValid = SSNDK.ValidationResult.Ok == SSNDK.validate(useModula11Check, repair, sut);
                var c_ = sut[sut.Length - 1] - '0';
                var genderOk = gender == (c_ % 2 == 0 ? Gender.Female : Gender.Male);
                var dayOk = dateOfBirth.Day == x.Day;
                var monthOk = dateOfBirth.Month == x.Month;
                var yearOk = IsYearOk(dateOfBirth.Year, x.Year, c);
                return (isValid && genderOk && dayOk && monthOk && yearOk).ToProperty();
            }
            else
            {
                ErrorReason error_;
                (success, error_) = sut.IsValid(useModula11Check);
                if (success) return false.ToProperty();
                else return (error == error_).ToProperty();
            }
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
