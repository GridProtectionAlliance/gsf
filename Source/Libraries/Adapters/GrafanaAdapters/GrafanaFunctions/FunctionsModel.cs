using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;


namespace GrafanaAdapters.GrafanaFunctions
{
    internal class GrafanaFunctions
    {
        public static List<IFunctionsModel> FunctionList { get; } = new List<IFunctionsModel>
        {
            new Add(),
            new AbsoluteValue()
        };
    }

    internal class Add : IFunctionsModel
    {
        private const string Expression = @"^{0}\s*\(\s*(?<Expression>.+)\s*\)";
        public Regex Regex { get; } = new Regex(string.Format(Expression, "Add"), RegexOptions.Compiled | RegexOptions.IgnoreCase);
        public string Name { get; } = "Add";
        public string Description { get; } = "Adds a decimal number to DataSourceValue";
        public List<IParameter> Parameters { get; }

        public Add()
        {
            // Initialize the list of parameters
            Parameters = new List<IParameter>
            {
                new Parameter<decimal>
                {
                    Default = typeof(decimal),
                    Description = "Decimal number to add",
                    Required = true,
                },
                new Parameter<DataSourceValue>
                {
                    Default = typeof(DataSourceValue),
                    Description = "Series",
                    Required = true
                }
            };
        }

        public void Compute(List<object> values)
        {
            foreach (object value in values)
            {
                Console.WriteLine(value);
            }
        }
    }

    internal class AbsoluteValue : IFunctionsModel
    {
        private const string Expression = @"^{0}\s*\(\s*(?<Expression>.+)\s*\)";
        public Regex Regex { get; } = new Regex(string.Format(Expression, "(AbsoluteValue|Abs)"), RegexOptions.Compiled | RegexOptions.IgnoreCase);
        public string Name { get; } = "AbsoluteValue";
        public string Description { get; } = "Returns absolute value of DataSourceValue";
        public List<IParameter> Parameters { get; }

        public AbsoluteValue()
        {
            Parameters = new List<IParameter> {};
        }

        public void Compute(List<object> values)
        {
            foreach (object value in values)
            {
                Console.WriteLine(value);
            }
        }
    }

    internal class Parameter<T> : IParameter
    {
        public Type Default { get; set; }
        public string Description { get; set; }
        public bool Required { get; set; }
        public Type Type { get; } = typeof(T);
    }
}
