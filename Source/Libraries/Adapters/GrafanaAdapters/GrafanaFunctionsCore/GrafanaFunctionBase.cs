//******************************************************************************************************
//  GrafanaFunctionBase.cs - Gbtc
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
//  11/19/2023 - Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace GrafanaAdapters.GrafanaFunctionsCore
{
    /// <summary>
    /// Represents the base functionality for any Grafana function.
    /// </summary>
    public abstract class GrafanaFunctionBase : IGrafanaFunction
    {
        private Regex m_regex;

        /// <inheritdoc />
        public abstract string Name { get; }

        /// <inheritdoc />
        public abstract string Description { get; }

        /// <inheritdoc />
        public virtual string[] Aliases => null;

        /// <inheritdoc />
        public virtual Regex Regex => m_regex ??= GetRegex();

        private Regex GetRegex()
        {
            if (Aliases is null || Aliases.Length == 0)
                return GenerateFunctionRegex(Name);

            string[] functionNames = new string[Aliases.Length + 1];

            functionNames[0] = Name;
            Array.Copy(Aliases, 0, functionNames, 1, Aliases.Length);

            return GenerateFunctionRegex(functionNames);
        }

        /// <inheritdoc />
        public virtual FunctionOperations SupportedFunctionOperations => DefaultFunctionOperations;

        /// <inheritdoc />
        public virtual FunctionOperations PublishedFunctionOperations => DefaultFunctionOperations;

        /// <inheritdoc />
        public abstract List<IParameter> Parameters { get; }

        /// <inheritdoc />
        public abstract DataSourceValueGroup<DataSourceValue> Compute(List<IParameter> parameters);

        /// <inheritdoc />
        public virtual DataSourceValueGroup<PhasorValue> ComputePhasor(List<IParameter> parameters)
        {
            return null;
        }

        /// <inheritdoc />
        public virtual DataSourceValueGroup<DataSourceValue> ComputeSlice(List<IParameter> parameters)
        {
            return Compute(parameters);
        }

        /// <inheritdoc />
        public virtual DataSourceValueGroup<PhasorValue> ComputePhasorSlice(List<IParameter> parameters)
        {
            return ComputePhasor(parameters);
        }

        /// <inheritdoc />
        public virtual DataSourceValueGroup<DataSourceValue> ComputeSet(List<IParameter> parameters)
        {
            return Compute(parameters);
        }

        /// <inheritdoc />
        public virtual DataSourceValueGroup<PhasorValue> ComputePhasorSet(List<IParameter> parameters)
        {
            return ComputePhasor(parameters);
        }
    }
}
