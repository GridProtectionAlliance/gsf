using System;
using System.Collections.Generic;
using System.Threading;
using GrafanaAdapters.DataSources;

namespace GrafanaAdapters.Functions.BuiltIn;

/// <summary>
/// Returns a single value that represents the mode of the values in the source series.
/// </summary>
/// <remarks>
/// Signature: Evaluate(sliceTolerance, evalExpression, filterExpression)
/// Returns: Single value per slice
/// Example 1: Evaluate(0.0333, { R* Sin(T* PI / 180)}, T=GPA_SHELBY-PA1:VH; R=GPA_SHELBY-PM1:V)
/// Example 2: Eval(0.0333, { (GPA_SHELBYPA2VH - GPA_SHELBYPA1VH) % 360 - 180}, GPA_SHELBY-PA1:VH; GPA_SHELBY-PA2:VH)
/// Example 3: eval(0.5, { (if (_v0 &gt; 62, _v2, if (_v0 &lt; 57, _v2, _v0)) + if (_v1 &gt; 62, _v2, if (_v1 &lt; 57, _v2, _v1))) / 2 }, FILTER TOP 3 ActiveMeasurements WHERE SignalType = 'FREQ')
/// Example 4: evaluate(0.0333, { if (abs(b - a) &gt; 180, if (sign(b - a) &lt; 0, b - a + 360, b - a - 360), b - a)}, a=PMU.009-PZR.AV:ANG; b=PMU.008-PZR.AV:ANG)
/// Variants: Evaluate, Eval
/// Execution: Deferred enumeration
/// </remarks>
public abstract class Evaluate<T> : GrafanaFunctionBase<T> where T : struct, IDataSourceValue<T>
{
    /// <inheritdoc />
    public override string Name => "Evaluate";

    /// <inheritdoc />
    public override string Description => "Evaluates an expression over a slice of values in one or more series.";

    /// <inheritdoc />
    public override string[] Aliases => new[] { "Eval" };

    /// <inheritdoc />
    public override GroupOperations SupportedGroupOperations => GroupOperations.Slice;

    /// <inheritdoc />
    public override GroupOperations PublishedGroupOperations => GroupOperations.Standard;

    /// <inheritdoc />
    public override ParameterDefinitions ParameterDefinitions => new List<IParameter>
    {
        // Slice tolerance added automatically by Grafana function handling
        new ParameterDefinition<string>
        {
            Name = "evalExpression",
            Default = "{ 0.0 }",
            Description = "A string representing expression to be evaluated. Parameter must always be expressed in braces, e.g., { expression }. Expression is strongly typed, but not case sensitive; expression is expected to return a value that can be evaluated as a floating-point number.",
            Required = true
        }
    };

    /// <inheritdoc />
    public override List<string> ParseParameters(ref string expression)
    {
        List<string> parsedParameters = new();

        int lastIndex = 0;

        for (int i = 0; i < 2; i++)
        {
            if (i == 1)
            {
                // Eval expression required to be in braces, i.e., { expression }, to delineate sub-expression that may have its own functions and parameters.
                int startBrace = expression.IndexOf('{', lastIndex);

                if (startBrace < 0)
                    break;

                startBrace++;

                int endBrace = expression.IndexOf('}', startBrace);

                if (endBrace < 0)
                    break;

                parsedParameters.Add(expression.Substring(startBrace, endBrace - startBrace));

                lastIndex = expression.IndexOf(',', endBrace);

                if (lastIndex < 0)
                    throw new FormatException($"Expected 3 parameters, received 2 in: Evaluate({expression})");

                lastIndex++;
            }
            else
            {
                int nextIndex = expression.IndexOf(',', lastIndex);

                if (nextIndex < 0)
                    break;

                parsedParameters.Add(expression.Substring(lastIndex, nextIndex - lastIndex));
                lastIndex = nextIndex + 1;
            }
        }

        if (parsedParameters.Count == 2)
            expression = expression.Substring(lastIndex).Trim();
        else
            throw new FormatException($"Expected 3 parameters, received {parsedParameters.Count + 1} in: Evaluate({expression})");

        return parsedParameters;
    }

    /// <inheritdoc />
    public override GroupOperations CheckSupportedGroupOperation(GroupOperations requestedOperation)
    {
        // Force standard group operation to be Slice - eval only supports slice operations
        if (requestedOperation is 0 or GroupOperations.Standard)
            requestedOperation = GroupOperations.Slice;

        return base.CheckSupportedGroupOperation(requestedOperation);
    }

    /// <inheritdoc />
    public class ComputeDataSourceValue : Evaluate<DataSourceValue>
    {
        /// <inheritdoc />
        public override IEnumerable<DataSourceValue> Compute(Parameters parameters, CancellationToken cancellationToken)
        {
            //// Get Values
            //DataSourceValueGroup<DataSourceValue> dataSourceValues = (DataSourceValueGroup<DataSourceValue>)(parameters[0] as IParameter<IDataSourceValueGroup>).Value;

            //// TODO: JRC - add eval functionality

            //return dataSourceValues;
            return null;
        }
    }

    /// <inheritdoc />
    public class ComputePhasorValue : Evaluate<PhasorValue>
    {
        /// <inheritdoc />
        public override IEnumerable<PhasorValue> Compute(Parameters parameters, CancellationToken cancellationToken)
        {
            //// Get Values
            //DataSourceValueGroup<PhasorValue> phasorValues = (DataSourceValueGroup<PhasorValue>)(parameters[0] as IParameter<IDataSourceValueGroup>).Value;

            //// TODO: JRC - add eval functionality

            //return phasorValues;
            return null;
        }
    }
}