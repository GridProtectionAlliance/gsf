using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GSF;
using GSF.Parsing;
using System.IO;

namespace StringExtensionsTest
{
    class Program
    {
        static void Main(string[] args)
        {
            string csvInput = File.ReadAllText("test2.csv");
            string[] csvOutput = StringParser.ParseStandardCSV(csvInput);
            string upperGreekLetters = "ΑΒΓΔΕΖΗΘΙΚΛΜΝΞΟΠΡΣΤΥΦΧΨΩ";
            string upperGreekLettersToLower = upperGreekLetters.ToLower();
            string lowerGreekLetters = "αβγδεζηθικλμνξοπρστυφχψω";
            string lowerGreekLettersToUpper = lowerGreekLetters.ToUpper();
            Console.WriteLine("Uppercase Greek Letters: " + upperGreekLetters);
            Console.WriteLine("Uppercase Greek Letters To Lower: " + upperGreekLettersToLower);
            Console.WriteLine("Lowercase Greek Letters: " + lowerGreekLetters);
            Console.WriteLine("Uppercase Greek Letters To Upper: " + lowerGreekLettersToUpper);

            string[] quoteUnwrapTests = { "quote \"unwrap\" test", "tab \t quotes \t rock", "\"quote unwrap test\"", "quote unwrap test", "\"quote unwrap test\"", "quote \t\t\tunwrap\t\t\t test" };
            char[] quotes = { '"', '\t', '"', '"', '\'', '\t' };

            for (int i = 0; i < quoteUnwrapTests.Length; i++)
            {
                quoteUnwrapTests[i] = quoteUnwrapTests[i].quoteUnwrap(quotes[i]);
                Console.WriteLine(quoteUnwrapTests[i]);
            }
            Console.ReadLine();
        }
    }
}
