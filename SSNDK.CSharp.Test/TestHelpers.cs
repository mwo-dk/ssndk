using FsCheck;
using System;
using System.Collections.Generic;
using System.Text;

namespace SSNDKCSharp.Test
{
    internal static class TestHelpers
    {
        internal static string GetSSN(DateTimeOffset x,
            bool dash, NonNegativeInt controlCode)
        {
            var (dd, mm, yy) = x.GetDate();
            var c = controlCode.Get % 10000;
            return dash ? $"{x.FormatDayOfBirth()}-{c.ToString("0000")}" :
                $"{x.FormatDayOfBirth()}{c.ToString("0000")}";
        }

        internal static string GetSSNWithNoneDigits(DateTimeOffset x,
            bool dash, NonNegativeInt controlCode)
        {
            var correct = GetSSN(x, dash, controlCode);
            return $"a{correct.Substring(1)}";
        }

        internal static string GetSSNWithDashFailure(DateTimeOffset x,
            NonNegativeInt controlCode)
        {
            var correct = GetSSN(x, true, controlCode);
            return correct.Replace('-', 'x');
        }

        internal static int GetBirthYear(int yy, int controlCode)
        {
            bool InRange(int x, int a, int b) =>
                a <= x && x <= b;
            var isYearValid = InRange(yy, 0, 99);
            if (!isYearValid) return -1;
            var isControlCodeValid = InRange(controlCode, 0, 9999);
            if (!isControlCodeValid) return -1;

            if (InRange(yy, 0, 99) && InRange(controlCode, 0, 3999))
                return 1900 + yy;
            if (InRange(yy, 0, 36) && InRange(controlCode, 4000, 4999))
                return 2000 + yy;
            if (InRange(yy, 37, 99) && InRange(controlCode, 4000, 4999))
                return 1900 + yy;
            if (InRange(yy, 0, 57) && InRange(controlCode, 5000, 8999))
                return 2000 + yy;
            if (InRange(yy, 58, 99) && InRange(controlCode, 5000, 8999))
                return 1800 + yy;
            if (InRange(yy, 0, 36) && InRange(controlCode, 9000, 9999))
                return 1900 + yy;
            if (InRange(yy, 37, 99) && InRange(controlCode, 9000, 9999))
                return 1900 + yy;

            return -1;
        }

        private static string FormatDayOfBirth(this DateTimeOffset x)
        {
            var (dd, mm, yy) = x.GetDate();
            return $"{dd.ToString("00")}{mm.ToString("00")}{yy.ToString("00")}";
        }
        private static (int DD, int MM, int YY) GetDate(this DateTimeOffset x) =>
            (x.Day, x.Month, x.Year % 100);
    }
}
