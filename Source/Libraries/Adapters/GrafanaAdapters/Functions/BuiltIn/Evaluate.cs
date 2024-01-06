using Ciloci.Flee;
using GrafanaAdapters.DataSources;
using GrafanaAdapters.Model.Annotations;
using GSF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace GrafanaAdapters.Functions.BuiltIn;

/// <summary>
/// Returns a single value that represents the evaluation of an expression over a slice of the values in the source series.
/// </summary>
/// <remarks>
/// Signature: <c>Evaluate(sliceTolerance, evalExpression, filterExpression)</c>
/// Returns: Single value per slice
/// Example 1: <c>Evaluate(0.0333, { R* Sin(T* PI / 180)}, T=GPA_SHELBY-PA1:VH; R=GPA_SHELBY-PM1:V)</c>
/// Example 2: <c>Eval(0.0333, { (GPA_SHELBYPA2VH - GPA_SHELBYPA1VH) % 360 - 180}, GPA_SHELBY-PA1:VH; GPA_SHELBY-PA2:VH)</c>
/// Example 3: <c>eval(0.5, { (if (_v0 &gt; 62, _v2, if (_v0 &lt; 57, _v2, _v0)) + if (_v1 &gt; 62, _v2, if (_v1 &lt; 57, _v2, _v1))) / 2 }, FILTER TOP 3 ActiveMeasurements WHERE SignalType = 'FREQ')</c>
/// Example 4: <c>evaluate(0.0333, { if (abs(b - a) &gt; 180, if (sign(b - a) &lt; 0, b - a + 360, b - a - 360), b - a)}, a=PMU.009-PZR.AV:ANG; b=PMU.008-PZR.AV:ANG)</c>
/// Variants: Evaluate, Eval
/// Execution: Deferred enumeration
/// </remarks>
public abstract class Evaluate<T> : GrafanaFunctionBase<T> where T : struct, IDataSourceValue<T>
{
    /// <inheritdoc />
    public override string Name => nameof(Evaluate<T>);

    /// <inheritdoc />
    public override string Description => "Evaluates an expression over a slice of values in one or more series.";

    /// <inheritdoc />
    public override string[] Aliases => new[] { "Eval" };

    /// <inheritdoc />
    public override GroupOperations AllowedGroupOperations => GroupOperations.Slice;

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
        },
        new ParameterDefinition<string>
        {
            Name = "imports",
            Default = "",
            Description = "Internal parameter used to pass command-level imports to the eval expression.",
            Required = false,
            Internal = true
        }
    };

    /// <inheritdoc />
    public override GroupOperations CheckAllowedGroupOperation(GroupOperations requestedOperation)
    {
        // Force standard group operation to be Slice - eval only supports slice operations
        if (requestedOperation is 0 or GroupOperations.Standard)
            requestedOperation = GroupOperations.Slice;

        return base.CheckAllowedGroupOperation(requestedOperation);
    }

    /// <inheritdoc />
    public override IEnumerable<T> Compute(Parameters parameters)
    {
        string expression = parameters.Value<string>(0);

        // Get the cached expression context
        ExpressionContext context = TargetCache<ExpressionContext>.GetOrAdd(expression, () =>
        {
            ExpressionContext expressionContext = new();

            expressionContext.Imports.AddType(typeof(Math));
            expressionContext.Imports.AddType(typeof(DateTime));

            // Load any custom imports
            string imports = parameters.Value<string>(1);

            if (string.IsNullOrWhiteSpace(imports))
                return expressionContext;

            foreach (string typeDef in imports.Split(';'))
            {
                try
                {
                    Dictionary<string, string> parsedTypeDef = typeDef.ParseKeyValuePairs(',');
                    string assemblyName = parsedTypeDef["assemblyName"];
                    string typeName = parsedTypeDef["typeName"];
                    Assembly assembly = Assembly.Load(new AssemblyName(assemblyName));
                    Type type = assembly.GetType(typeName);
                    expressionContext.Imports.AddType(type);
                }
                catch (Exception ex)
                {
                    throw new TypeLoadException($"Unable to load import type from assembly for \"{typeDef}\": {ex.Message}", ex);
                }
            }

            return expressionContext;
        });

        static string getCleanIdentifier(string target) =>
            Regex.Replace(target, @"[^A-Z0-9_]", "", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        lock (context)
        {
            // Clear existing variables - missing values will be exposed as NaN
            foreach (string target in context.Variables.Keys)
                context.Variables[target] = double.NaN;

            List<string> targets = new();
            T lastValue = default;
            int index = 0;

            // Load each target as variable name with its current slice value
            foreach (T dataValue in GetDataSourceValues(parameters))
            {
                lastValue = dataValue;

                // Get alias or clean target name for use as expression variable name
                string target = dataValue.Target.SplitAlias(out string alias);

                if (string.IsNullOrWhiteSpace(alias))
                {
                    targets.Add(target);
                    target = getCleanIdentifier(target);
                }
                else
                {
                    targets.Add($"{alias}={target}");
                    target = alias;
                }

                context.Variables[target] = dataValue.Value;
                context.Variables[$"_v{index++}"] = dataValue.Value;
            }

            // Compile the expression if it has not been compiled already
            IDynamicExpression dynamicExpression = TargetCache<IDynamicExpression>.GetOrAdd(expression, () => context.CompileDynamic(expression));

            // Return evaluated expression
            yield return lastValue with
            {
                Value = Convert.ToDouble(dynamicExpression.Evaluate()), 
                Target = $"{string.Join("; ", targets.Take(4))}{(targets.Count > 4 ? "; ..." : "")}"
            };
        }
    }

    /// <inheritdoc />
    public override List<string> ParseParameters(QueryParameters queryParameters, ref string queryExpression)
    {
        List<string> parsedParameters = new();

        int lastIndex = 0;

        for (int i = 0; i < 2; i++)
        {
            if (i == 1)
            {
                // Eval expression required to be in braces, i.e., { expression }, to delineate sub-expression that may have its own functions and parameters.
                int startBrace = queryExpression.IndexOf('{', lastIndex);

                if (startBrace < 0)
                    break;

                startBrace++;

                int endBrace = queryExpression.IndexOf('}', startBrace);

                if (endBrace < 0)
                    break;

                parsedParameters.Add(queryExpression.Substring(startBrace, endBrace - startBrace));

                lastIndex = queryExpression.IndexOf(',', endBrace);

                if (lastIndex < 0)
                    throw new FormatException($"Expected 3 parameters, received 2 in: Evaluate({queryExpression})");

                lastIndex++;
            }
            else
            {
                int nextIndex = queryExpression.IndexOf(',', lastIndex);

                if (nextIndex < 0)
                    break;


                parsedParameters.Add(queryExpression.Substring(lastIndex, nextIndex - lastIndex));
                lastIndex = nextIndex + 1;
            }
        }

        if (parsedParameters.Count == 2)
            queryExpression = queryExpression.Substring(lastIndex).Trim();
        else
            throw new FormatException($"Expected 3 parameters, received {parsedParameters.Count + 1} in: Evaluate({queryExpression})");

        // Add command-level imports to parsed parameters
        parsedParameters.Add(queryParameters.Imports);

        return parsedParameters;
    }

    /// <inheritdoc />
    public class ComputeDataSourceValue : Evaluate<DataSourceValue>
    {
    }

    /// <inheritdoc />
    public class ComputePhasorValue : Evaluate<PhasorValue>
    {
        // Operating on magnitude only
    }
}