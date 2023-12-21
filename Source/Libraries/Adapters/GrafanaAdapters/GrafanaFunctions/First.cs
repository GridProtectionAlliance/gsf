using System;
using System.Collections.Generic;
using System.Linq;
using GrafanaAdapters.GrafanaFunctionsCore;

namespace GrafanaAdapters.GrafanaFunctions;

/// <summary>
/// Returns a series of N, or N% of total, values from the start of the source series.
/// N is either a positive integer value, representing a total, that is greater than zero - or - a floating point value,
/// suffixed with '%' representing a percentage, that must range from greater than 0 to less than or equal to 100 - defaults to 1.
/// N can either be constant value or a named target available from the expression. Any target values that fall between 0
/// and 1 will be treated as a percentage.
/// </summary>
/// <remarks>
/// Signature: <c>First([N|N% = 1], expression)</c><br/>
/// Returns: Series of values.<br/>
/// Example: <c>First(5%, FILTER ActiveMeasurements WHERE SignalType='FREQ')</c><br/>
/// Variants: First<br/>
/// Execution: Immediate in-memory array load.
/// </remarks>
public class First: GrafanaFunctionBase
{
    /// <inheritdoc />
    public override string Name => nameof(First);

    /// <inheritdoc />
    public override string Description => "Returns a series of N, or N% of total, values from the start of the source series.";

    /// <inheritdoc />
    public override List<IParameter> Parameters => new()
    {
        new Parameter<string>
        {
            Default = "1",
            Description = "A integer value or percent representing number of % of elements to take.",
            Required = true
        },

        InputDataPointValues
    };

    /// <summary>
    /// Used to convert value or percent to the number of points selected
    /// </summary>
    /// <param name="rawValue"></param>
    /// <param name="numberPoints"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    private int convertToValue(string rawValue, int numberPoints)
    {
        try
        {
            //Percent
            if (rawValue.EndsWith("%"))
            {
                double percent = Convert.ToDouble(rawValue.TrimEnd('%')) / 100;
                if (percent < 0 || percent > 1)
                {
                    throw new Exception($"Error {rawValue} out of bounds (0 - 100).");
                }
                return Convert.ToInt32(numberPoints * percent);
            }
            //Number
            else
            {
                double value = Convert.ToDouble(rawValue);

                //Between 0-1 so count as percent
                if (value > 0 && value < 1)
                    return Convert.ToInt32(numberPoints * value);

                return Convert.ToInt32(value);
            }

        }
        catch (Exception ex)
        {
            throw new Exception($"Error converting {rawValue} to {typeof(double)}.", ex);
        }
    }

    /// <inheritdoc />
    public override DataSourceValueGroup<DataSourceValue> Compute(List<IParameter> parameters)
    {
        // Get Values
        string rawValue = (parameters[0] as IParameter<string>).Value;
        DataSourceValueGroup<DataSourceValue> dataSourceValues = (DataSourceValueGroup<DataSourceValue>)(parameters[1] as IParameter<IDataSourceValueGroup>).Value;
        int numberRequested = convertToValue(rawValue, dataSourceValues.Source.Count());

        // Requested more than accessable
        if (numberRequested >= dataSourceValues.Source.Count())
            numberRequested = dataSourceValues.Source.Count();

        // Compute
        IEnumerable<DataSourceValue> transformedDataSourceValues = dataSourceValues.Source
            .OrderBy(dataSourceValue => dataSourceValue.Time)
            .Take(numberRequested);

        // Set Values
        dataSourceValues.Target = $"{Name}({dataSourceValues.Target})";
        dataSourceValues.Source = transformedDataSourceValues;

        return dataSourceValues;
    }

    /// <inheritdoc />
    public override DataSourceValueGroup<PhasorValue> ComputePhasor(List<IParameter> parameters)
    {
        // Get Values
        string rawValue = (parameters[0] as IParameter<string>).Value;
        DataSourceValueGroup<PhasorValue> phasorValues = (DataSourceValueGroup<PhasorValue>)(parameters[1] as IParameter<IDataSourceValueGroup>).Value;

        int numberRequested = convertToValue(rawValue, phasorValues.Source.Count());

        // Requested more than accessable
        if (numberRequested >= phasorValues.Source.Count())
            numberRequested = phasorValues.Source.Count();

        // Compute
        IEnumerable<PhasorValue> transformedPhasorValues = phasorValues.Source
            .OrderBy(phasorValue => phasorValue.Time)
            .Take(numberRequested);

        // Set Values
        string[] labels = phasorValues.Target.Split(';');
        phasorValues.Target = $"{Name}({labels[0]});{Name}({labels[1]})";
        phasorValues.Source = transformedPhasorValues;

        return phasorValues;
    }
}