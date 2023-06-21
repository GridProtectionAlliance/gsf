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
                        Default = 0,
                        Description = "Decimal number to add",
                        Required = true,
                    }
                },
                (parameters, dataSourceValues) =>
                {
                    double value = double.Parse(parameters[0]);


                    IEnumerable<DataSourceValue> transformedDataSourceValues = dataSourceValues.Select(dataValue =>
                    new DataSourceValue
                    {
                        Value = value + dataValue.Value,
                        Time = dataValue.Time,
                        Target = dataValue.Target
                    });

                    return transformedDataSourceValues;
                }
            ),

            new GrafanaFunction(
                "AbsoluteValue",
                "(AbsoluteValue|Abs)",
                "Returns absolute value of DataSourceValue",
                new List<IParameter>(),
                (parameters, dataSourceValues) =>
                {
                    IEnumerable<DataSourceValue> transformedDataSourceValues = dataSourceValues.Select(dataValue =>
                    new DataSourceValue
                    {
                        Value = Math.Abs(dataValue.Value),
                        Time = dataValue.Time,
                        Target = dataValue.Target
                    });

                    return transformedDataSourceValues;
                }
            )
        };
        
    }
}
