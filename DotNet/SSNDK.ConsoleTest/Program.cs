using System;
using static SSNDK.SSN.CSharp.StringExtensions;

namespace SSNDK.ConsoleTest
{
    class Program
    {
        static void Main(string[] args)
        {
            while (true)
            {
                Console.Write("Enter cpr>> ");
                var cpr = Console.ReadLine();
                try
                {
                    var (gender, birthDate) = cpr.GetPersonInfo(true, true);
                }
                catch (Exception error)
                {
                    Console.Error.WriteLine(error.Message);
                }
            }
        }
    }
}
