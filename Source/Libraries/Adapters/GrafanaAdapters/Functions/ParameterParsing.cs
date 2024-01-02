//******************************************************************************************************
//  ParameterParsing.cs - Gbtc
//
//  Copyright © 2024, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may not use this
//  file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  01/01/2024 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************
// ReSharper disable PossibleMultipleEnumeration

using GrafanaAdapters.DataSources;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace GrafanaAdapters.Functions;

internal static class ParameterParsing
{
    public static (string[] parsedParameters, string updatedExpression) ParseParameters(this IGrafanaFunction function, string expression, GroupOperations groupOperation)
    {
        return TargetCache<(string[], string)>.GetOrAdd(expression, () =>
        {
            // Check if function defines any custom parameter parsing
            List<string> parsedParameters = function.ParseParameters(ref expression);

            if (parsedParameters is null)
            {
                parsedParameters = new List<string>();

                // Extract any required function parameters
                int requiredParameters = function.RequiredParameterCount;

                // Any slice operation adds one required parameter for time tolerance
                if (groupOperation == GroupOperations.Slice)
                    requiredParameters++;

                if (requiredParameters > 0)
                {
                    int index = 0;

                    for (int i = 0; i < requiredParameters && index > -1; i++)
                        index = expression.IndexOf(',', index + 1);

                    if (index > -1)
                        parsedParameters.AddRange(expression.Substring(0, index).Split(','));

                    if (parsedParameters.Count == requiredParameters)
                        expression = expression.Substring(index + 1).Trim();
                    else
                        throw new FormatException($"Expected {requiredParameters + 1} parameters, received {parsedParameters.Count + 1} in: {function.Name}({expression})");
                }

                // Extract any provided optional function parameters
                int optionalParameters = function.OptionalParameterCount;

                if (optionalParameters > 0)
                {
                    int index = expression.IndexOf(',');

                    if (index > -1 && !hasSubExpression(expression.Substring(0, index)))
                    {
                        int lastIndex = index;

                        for (int i = 1; i < optionalParameters && index > -1; i++)
                        {
                            index = expression.IndexOf(',', index + 1);

                            if (index > -1 && hasSubExpression(expression.Substring(lastIndex + 1, index - lastIndex - 1).Trim()))
                            {
                                index = lastIndex;
                                break;
                            }

                            lastIndex = index;
                        }

                        if (index > -1)
                        {
                            parsedParameters.AddRange(expression.Substring(0, index).Split(','));
                            expression = expression.Substring(index + 1).Trim();
                        }
                    }
                }
            }

            return (parsedParameters.ToArray(), expression);
        });

        static bool hasSubExpression(string target)
        {
            return target.StartsWith("FILTER", StringComparison.OrdinalIgnoreCase) || target.Contains("(");
        }
    }

    public static Parameters GenerateParameters<T>(this IGrafanaFunction<T> function, string[] parsedParameters, IEnumerable<T> dataSourceValues, string rootTarget, DataSet metadata, Dictionary<string, string> metadataMap) where T : struct, IDataSourceValue<T>
    {
        // Generate a list of value mutable parameters
        Parameters parameters = function.GetMutableParameters();
        int index = 0;

        Debug.Assert(parameters.Count == function.RequiredParameterCount + function.OptionalParameterCount + 1, $"Expected {function.RequiredParameterCount + function.OptionalParameterCount + 1} parameters, received {parameters.Count} in: {function.Name}({string.Join(",", parsedParameters)})");
        Debug.Assert(parsedParameters.Length <= parameters.Count - 1, $"Expected {parameters.Count - 1} parameters, received {parsedParameters.Length} in: {function.Name}({string.Join(",", parsedParameters)})");
        Debug.Assert(parsedParameters.Length == 0 || parsedParameters.All(parameter => !string.IsNullOrWhiteSpace(parameter)), $"Expected all parameters to be non-empty in: {function.Name}({string.Join(",", parsedParameters)})");

        parameters.ParsedCount = parsedParameters.Length;

        for (int i = 0; i < parameters.Count; i++)
        {
            IMutableParameter parameter = parameters[i];

            // Data -- last parameter is always a data source value
            if (i == parameters.Count - 1)
            {
                Debug.Assert(parameter is IParameter<IEnumerable<IDataSourceValue>>, $"Last parameter is not a data source value of type '{typeof(IEnumerable<IDataSourceValue>).Name}'.");

                // Replace last parameter with data source type specific parameter with associated values
                parameters[i] = new Parameter<IEnumerable<T>>(default(T).DataSourceValuesParameterDefinition)
                {
                    Value = dataSourceValues
                };

                break;
            }

            // Parameter
            if (index < parsedParameters.Length)
                parameter.ConvertParsedValue(parsedParameters[index++], rootTarget, dataSourceValues, metadata, metadataMap);
            else
                Debug.Fail($"Expected {function.RequiredParameterCount} parameters, received {index} in: {function.Name}({string.Join(",", parsedParameters)})");
        }

        return parameters;
    }

    // We cache object factory functions by type so they are only created once
    private static readonly ConcurrentDictionary<Type, Func<IParameter, IMutableParameter>> s_mutableParameterFactories = new();

    // Since value mutable parameters need to be created at each function call and we only have the 'Type' property of the IParameter
    // interface to create the 'Parameter<T>' type, i.e., a specific type 'T' is not available for compiler, we generate a fast object
    // creation factory function. This is faster than using the 'Activator.CreateInstance', even after dictionary cache lookup, because
    // the IL is highly specific to task and does not incur overhead of type validations. Note that on .NET Core the performance of
    // 'Activator.CreateInstance' is at least three times slower than the generated IL, as compared to at least four times slower on
    // .NET Framework -- with this the dictionary lookup eats into performance gains more for .NET Core environments.
    // https://andrewlock.net/benchmarking-4-reflection-methods-for-calling-a-constructor-in-dotnet/
    public static Func<IParameter, IMutableParameter> GetMutableParameterFactory(Type parameterType)
    {
        // This function creates generated IL given a specific type creating a function that operates similar to
        // the following where type 'T' would be known at compile-time:
        //
        //    private IMutableParameter CreateMutableParameter<T>(IParameter parameter) =>
        //        return new Parameter<T>((ParameterDefinition<T>)parameter);
        //
        // Here is the equivalent of the generated IL using the slower 'Activator.CreateInstance':
        //
        //    private static IMutableParameter CreateMutableParameter(IParameter parameter) => 
        //        (IMutableParameter)Activator.CreateInstance(typeof(Parameter<>).MakeGenericType(parameter.Type), parameter);
        //
        return s_mutableParameterFactories.GetOrAdd(parameterType, _ =>
        {
            // Create specific types for the generic parameter definition and value mutable parameter
            Type parameterDefinitionType = typeof(ParameterDefinition<>).MakeGenericType(parameterType);
            Type mutableParameterType = typeof(Parameter<>).MakeGenericType(parameterType);
            ConstructorInfo constructor = mutableParameterType.GetConstructor(new[] { parameterDefinitionType });

            Debug.Assert(constructor is not null, $"No constructor exists for type '{mutableParameterType.Name}' with 'ParameterDefinition<T>' parameter");

            // Generate a dynamic method that will construct a new instance of the value mutable parameter type, this
            // will be a new constructor for the Parameter<T> type that takes a single IParameter argument
            DynamicMethod method = new("ctor_type$" + mutableParameterType.Name, mutableParameterType, new[] { typeof(IParameter) }, mutableParameterType);
            ILGenerator generator = method.GetILGenerator();

            // Load the first method argument (which is an IParameter) onto the stack
            generator.Emit(OpCodes.Ldarg_0);

            // Since IParameter references a ParameterDefinition<T>, which is a struct, we need to use Unbox_Any
            // which pops and unboxes item on the stack into the specified type, pushing result onto the stack
            generator.Emit(OpCodes.Unbox_Any, parameterDefinitionType);

            // Call the Parameter<T> constructor - single ParameterDefinition<T> parameter is ready on the stack
            generator.Emit(OpCodes.Newobj, constructor);

            // Return the new object - since Parameter<T> implements IMutableParameter, CLR handles the cast
            generator.Emit(OpCodes.Ret);

            return (Func<IParameter, IMutableParameter>)method.CreateDelegate(typeof(Func<IParameter, IMutableParameter>));
        });
    }
}