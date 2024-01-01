//******************************************************************************************************
//  Common.cs - Gbtc
//
//  Copyright © 2023, Grid Protection Alliance.  All Rights Reserved.
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
//  08/23/2023 - Timothy Liakh
//       Generated original version of source code.
//
//******************************************************************************************************
// ReSharper disable AccessToModifiedClosure
// ReSharper disable PossibleMultipleEnumeration

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using GrafanaAdapters.DataSources;

namespace GrafanaAdapters.FunctionParsing;

internal static class Common
{
    public const GroupOperations DefaultGroupOperations = GroupOperations.Standard | GroupOperations.Slice | GroupOperations.Set;

    public static IReadOnlyList<IParameter> GenerateParameters(IEnumerable<IParameter> parameters = default)
    {
        List<IParameter> parameterList = parameters?.ToList() ?? new List<IParameter>();

        parameterList.Add(new ParameterDefinition<IEnumerable<IDataSourceValue>>()
        {
            Default = Array.Empty<IDataSourceValue>(),
            Description = "Input Data Points",
            Required = true
        });

        return parameterList;
    }

    //public static readonly ParameterDefinition<IEnumerable<IDataSourceValue>> InputDataSourceValues = new()
    //{
    //    Default = Array.Empty<IDataSourceValue>(),
    //    Description = "Input Data Points",
    //    Required = true
    //};

    public static void InsertRequiredSliceParameter(this List<IParameter> parameters)
    {
        parameters.Insert(0, new ParameterDefinition<double>()
        {
            Default = 0.0333,
            Description = "A floating-point value that must be greater than or equal to zero that represents the desired time tolerance, in seconds, for the time slice",
            Required = true
        });
    }

    // Gets type for specified data source type name, assuming local namespace if needed.
    public static Type GetLocalType(string typeName)
    {
        if (typeName is null)
            throw new ArgumentNullException(nameof(typeName));

        if (!typeName.Contains('.'))
            typeName = $"{nameof(GrafanaAdapters)}.{typeName}";

        return Type.GetType(typeName);
    }

    public static int ParseInt(string parameter, bool includeZero = true)
    {
        parameter = parameter.Trim();

        if (!int.TryParse(parameter, out int value))
            throw new FormatException($"Could not parse '{parameter}' as an integer value.");

        if (includeZero)
        {
            if (value < 0)
                throw new ArgumentOutOfRangeException($"Value '{parameter}' is less than zero.");
        }
        else
        {
            if (value <= 0)
                throw new ArgumentOutOfRangeException($"Value '{parameter}' is less than or equal to zero.");
        }

        return value;
    }

    public static double ParseFloat(string parameter, bool validateGTEZero = true)
    {
        parameter = parameter.Trim();

        if (!double.TryParse(parameter, out double value))
            throw new FormatException($"Could not parse '{parameter}' as a floating-point value.");

        if (validateGTEZero && value < 0.0D)
            throw new ArgumentOutOfRangeException($"Value '{parameter}' is less than zero.");

        return value;
    }

    public static IEnumerable<T> GetDataSource<T>(this List<IParameter> parameters) where T : struct, IDataSourceValue<T>
    {
        return (parameters.LastOrDefault() as IParameter<IEnumerable<T>>)?.Value ?? throw new InvalidOperationException($"Last parameter is not a data source value of type '{typeof(IEnumerable<T>).Name}'.");
    }

    // We cache object factory functions by type so they are only created once
    private static readonly ConcurrentDictionary<Type, Func<IParameter, IParameter>> s_valueMutableParameterFactoryFunctions = new();

    // Since value mutable parameters need to be created at each function call and we only have the 'Type' property of the
    // IParameter interface to create the 'Parameter<T>' type, i.e., a specific type 'T' is not available for compiler, we
    // generate a fast object creation factory function. This is much faster than using the 'Activator.CreateInstance' which
    // incurs extra overhead because of type validations.
    public static Func<IParameter, IParameter> GetValueMutableParameterFactoryFunction(Type parameterType)
    {
        return s_valueMutableParameterFactoryFunctions.GetOrAdd(parameterType, _ =>
        {
            Type type = typeof(ValueMutableParameter<>).MakeGenericType(parameterType);
            Type[] parameterTypes = new[] { typeof(ParameterDefinition<>).MakeGenericType(parameterType) };
            ConstructorInfo constructor = type.GetConstructor(parameterTypes);

            Debug.Assert(constructor is not null, $"No constructor exists for type '{type.Name}' with 'ParameterDefinition<T>' parameter");

            // The following is markedly faster than using Activator.CreateInstance
            DynamicMethod method = new("ctor_type$" + type.Name, type, parameterTypes, type);
            ILGenerator generator = method.GetILGenerator();

            generator.Emit(OpCodes.Newobj, constructor);
            generator.Emit(OpCodes.Ret);

            return (Func<IParameter, IParameter>)method.CreateDelegate(typeof(Func<IParameter, IParameter>));
        });
    }
}