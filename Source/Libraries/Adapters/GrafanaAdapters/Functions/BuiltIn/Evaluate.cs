using Ciloci.Flee;
using GrafanaAdapters.DataSources;
using GrafanaAdapters.DataSources.BuiltIn;
using GrafanaAdapters.Metadata;
using GSF;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading;

namespace GrafanaAdapters.Functions.BuiltIn;

/// <summary>
/// Returns a single value that represents the evaluation of an expression over a slice of the values in the source series.
/// The <c>sliceTolerance</c> parameter is a floating-point value that must be greater than or equal to 0.001 that represents
/// the desired time tolerance, in seconds, for the time slice. The <c>evalExpression</c> parameter must always be expressed
/// in braces, e.g., <c>{ expression }</c>; expression is strongly typed, but not case sensitive; expression is expected to
/// return a value that can be evaluated as a floating-point number. Aliases of target tag names are used as variable names
/// in the <c>evalExpression</c>  when defined. If no alias is defined, all non-valid characters will be removed from target
/// tag name, for example, variable name for tag <c>PMU.032-PZR_CI:ANG</c> would be <c>PMU032PZR_CIANG</c>. All targets are
/// also available as index suffixed variables named <c>_v</c>, for example, first and second target values are available as
/// <c>_v0</c> and <c>_v1</c>. The <c>Evaluate</c> function is always evaluated as a slice, any specified group operation
/// prefix will be ignored. Default system types available to expressions are <c>System.Math</c> and <c>System.DateTime</c>.
/// See <see href="https://www.codeproject.com/Articles/19768/Flee-Fast-Lightweight-Expression-Evaluator">details</see> on
/// valid expressions. Use the <c>Imports</c> command to define more types for <c>evalExpression</c>.
/// </summary>
/// <remarks>
/// Signature: <c>Evaluate(sliceTolerance, evalExpression, filterExpression)</c><br/>
/// Returns: Single value per slice<br/>
/// Example 1: <c>Evaluate(0.0333, { R* Sin(T* PI / 180)}, T=GPA_SHELBY-PA1:VH; R=GPA_SHELBY-PM1:V)</c><br/>
/// Example 2: <c>Eval(0.0333, { (GPA_SHELBYPA2VH - GPA_SHELBYPA1VH) % 360 - 180}, GPA_SHELBY-PA1:VH; GPA_SHELBY-PA2:VH)</c><br/>
/// Example 3: <c>eval(0.5, { (if (_v0 &gt; 62, _v2, if (_v0 &lt; 57, _v2, _v0)) + if (_v1 &gt; 62, _v2, if (_v1 &lt; 57, _v2, _v1))) / 2 }, FILTER TOP 3 ActiveMeasurements WHERE SignalType = 'FREQ')</c><br/>
/// Example 4: <c>evaluate(0.0333, { if (abs(b - a) &gt; 180, if (sign(b - a) &lt; 0, b - a + 360, b - a - 360), b - a)}, a=PMU.009-PZR.AV:ANG; b=PMU.008-PZR.AV:ANG)</c><br/>
/// Variants: Evaluate, Eval<br/>
/// Execution: Deferred enumeration
/// <para>
/// The following special command-level parameter is available to the <c>Evaluate</c> function: <c>Imports={expr}</c><br/>
/// This command adds custom .NET type imports that can be used with the <c>Evaluate</c> function. <c>expr</c>defines a
/// key-value pair definition of assembly name, i.e., <c>AssemblyName</c> = DLL filename without suffix, and type name, i.e.,
/// <c>TypeName</c> = fully qualified case-sensitive type name, to be imported. Key-value pairs are separated with commas and
/// multiple imports are by separated semi-colons. <c>expr</c> must be surrounded by braces. Example:
/// <c>; imports={AssemblyName=mscorlib, TypeName=System.TimeSpan; AssemblyName=MyCode, TypeName=MyCode.MyClass}</c>
/// </para>
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
    public override ReturnType ReturnType => ReturnType.Scalar;

    /// <inheritdoc />
    public override GroupOperations AllowedGroupOperations => GroupOperations.Slice;

    /// <inheritdoc />
    public override GroupOperations PublishedGroupOperations => GroupOperations.None;

    /// <inheritdoc />
    public override ParameterDefinitions ParameterDefinitions => new List<IParameter>
    {
        // Required slice tolerance parameter added automatically by Grafana function handling
        new ParameterDefinition<string>
        {
            Name = "evalExpression",
            Default = "{ 0.0 }",
            Description = "A string representing expression to be evaluated. Parameter must always be expressed in braces, e.g., { expression }. Expression is strongly typed, but not case sensitive; expression is expected to return a value that can be evaluated as a floating-point number.",
            Required = true
        },
        // The 'imports' parameter is not provided as a user-provided parameter expression to the eval function since
        // its signature is already complex enough -- so any needed imports are defined as a command-level parameter.
        // Here we define an internal parameter to hold any defined command-level imports, but the value is added
        // manually to the parameters list, see 'ParseParameters' override below.
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
        // Force group operation to be Slice as Evaluate only supports slice operations. This ignores
        // any requested group operation instead of throwing an exception based on what is allowed.
        return GroupOperations.Slice;
    }

    /// <inheritdoc />
    public override async IAsyncEnumerable<T> ComputeSliceAsync(Parameters parameters, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        string expression = parameters.Value<string>(0);

        // Build and cache an expression context
        ExpressionContext context = TargetCache<ExpressionContext>.GetOrAdd(expression, () =>
        {
            ExpressionContext expressionContext = new();

            // Add default imports
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
                    throw new SyntaxErrorException($"Unable to load import type from assembly for \"{typeDef}\": {ex.Message}", ex);
                }
            }

            return expressionContext;
        });

        static string getCleanIdentifier(string target) =>
            Regex.Replace(target, @"[^A-Z0-9_]", "", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        // Data values in this array will be for current slice, one value for each target series
        T[] dataValues = await GetDataSourceValues(parameters).ToArrayAsync(cancellationToken).ConfigureAwait(false);

        if (dataValues.Length == 0)
            yield break;

        lock (context)
        {
            // Clear existing variables - missing values will be exposed as NaN
            foreach (string target in context.Variables.Keys)
                context.Variables[target] = double.NaN;

            List<string> targets = new();
            T sourceValue = default;
            int index = 0;

            // Load each target as variable name with its current slice value
            foreach (T dataValue in dataValues)
            {
                // First non-zero time value will be used as time for the slice
                if (sourceValue.Time == 0.0D && dataValue.Time > 0.0D)
                    sourceValue = dataValue;

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

            if (sourceValue.Time == 0.0D)
                yield break;

            // Compile and cache the expression (only compiled once per expression)
            IDynamicExpression dynamicExpression = TargetCache<IDynamicExpression>.GetOrAdd(expression, () =>
            {
                try
                {
                    return context.CompileDynamic(expression);
                }
                catch (Exception ex)
                {
                    throw new SyntaxErrorException($"Failed to compile expression \"{expression}\" for evaluation: {ex.Message}", ex);
                }
            });

            // Return evaluated expression
            yield return sourceValue with
            {
                Value = Convert.ToDouble(dynamicExpression.Evaluate()),
                Target = $"{string.Join("; ", targets.Take(4))}{(targets.Count > 4 ? "; ..." : "")}"
            };
        }
    }

    /// <inheritdoc />
    public override IAsyncEnumerable<T> ComputeAsync(Parameters parameters, CancellationToken cancellationToken)
    {
        // 'ComputeAsync' is abstract, so we must override, however, it should never be called for 'Evaluate' function since
        // 'CheckAllowedGroupOperation' forces group operation to be 'Slice', 'ComputeSliceAsync' should be called instead:
        Debug.Fail("Unexpected Operation: ComputeAsync should never be called for Evaluate function, ComputeSliceAsync should be called instead.");
        return null;
    }

    /// <inheritdoc />
    // 'Evaluate' function has special parameter parsing requirements, so we override the default implementation:
    public override (List<string>, string) ParseParameters(QueryParameters queryParameters, string queryExpression)
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

                if (lastIndex < 0) // No filter expression provided
                    throw new SyntaxErrorException($"Expected 3 parameters, received 2 in: Evaluate({queryExpression})");

                lastIndex++;
            }
            else
            {
                // First parameter is slice tolerance
                int nextIndex = queryExpression.IndexOf(',', lastIndex);

                if (nextIndex < 0)
                    break;

                parsedParameters.Add(queryExpression.Substring(lastIndex, nextIndex - lastIndex));
                lastIndex = nextIndex + 1;
            }
        }

        if (parsedParameters.Count == 2)
        {
            // Separate remaining query expression from required parameters, in this case, this is the filter expression
            queryExpression = queryExpression.Substring(lastIndex).Trim();
        }
        else
        {
            // Offset counts for expected filter expression in error message for better user feedback
            throw new SyntaxErrorException($"Expected 3 parameters, received {parsedParameters.Count + 1} in: Evaluate({queryExpression})");
        }

        // Add command-level type imports to parsed parameters, this becomes an internal parameter
        parsedParameters.Add(queryParameters.Imports);

        // Return parsed parameters and remaining query expression
        return (parsedParameters, queryExpression);
    }

    /// <inheritdoc />
    public override string FormatTargetName(GroupOperations groupOperation, string targetName, string[] parsedParameters)
    {
        // Format eval expression parameter to be in braces, i.e., { expression },
        // this way UI representation of formatted parameters will match user input
        string[] parameters = new string[3];
        Array.Copy(parsedParameters, parameters, 3);
        parameters[1] = $"{{ {parameters[1]} }}";

        // Hide slice prefix on UI since eval function is always slice
        return base.FormatTargetName(GroupOperations.None, targetName, parameters);
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