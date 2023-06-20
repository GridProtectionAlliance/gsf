using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GrafanaAdapters.GrafanaFunctions
{
    internal class Functions
    {
        public static List<IFunctionsModel> FunctionList { get; } = new List<IFunctionsModel>
        {
            new Add(),
            new AbsoluteValue()
        };

        public static DataSourceValue ParseFunction(string expression)
        {
            (IFunctionsModel function, string parameterValue) = MatchFunction(expression);

            // Base Case
            if (function == null)
            {
                return new DataSourceValue();
            }

            string[] parsedParameters = parameterValue?.Split(',');

            Console.WriteLine(function.Name);
            Console.WriteLine(parameterValue);

            // Recursive call to parse the nested function
            DataSourceValue nestedResult = ParseFunction(parsedParameters?.LastOrDefault() ?? "");

            // Perform actions on the nested result as needed

            // Return the result to the previous call
            return nestedResult;
        }



        public static (IFunctionsModel function, string parameterValue) MatchFunction(string expression)
        {
            foreach (IFunctionsModel function in FunctionList)
            {
                // Check if the expression matches the current function's regex
                if (!function.Regex.IsMatch(expression))
                    continue;

                // Get the matched groups from the regex
                GroupCollection groups = function.Regex.Match(expression).Groups;
                string parameterValue = groups[groups.Count - 1].Value;

                return (function, parameterValue);
            }

            // No match found
            return (null, null);
        }







    }
}
