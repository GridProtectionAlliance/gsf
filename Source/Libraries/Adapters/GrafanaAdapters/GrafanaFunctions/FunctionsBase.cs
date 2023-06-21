using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrafanaAdapters.GrafanaFunctions
{
    internal class FunctionsBase
    {
        public static List<IFunctionsModel> GrafanaFunctions { get; } = new List<IFunctionsModel>
        {
            new GrafanaFunction(
                "Add",
                "Add",
                "Adds a decimal number to DataSourceValue",
                new List<IParameter>
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
                },
                values =>
                {
                    foreach (object value in values)
                    {
                        Console.WriteLine(value);
                    }
                }
            ),
            new GrafanaFunction(
                "AbsoluteValue",
                "(AbsoluteValue|Abs)",
                "Returns absolute value of DataSourceValue",
                new List<IParameter>(),
                values =>
                {
                    foreach (object value in values)
                    {
                        Console.WriteLine(value);
                    }
                }
            )
        };
        
    }
}
