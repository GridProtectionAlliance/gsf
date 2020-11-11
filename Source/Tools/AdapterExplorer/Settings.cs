//******************************************************************************************************
//  Settings.cs - Gbtc
//
//  Copyright © 2020, Grid Protection Alliance.  All Rights Reserved.
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
//  11/06/2020 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System.ComponentModel;
using System.Configuration;
using ExpressionEvaluator;
using GSF.ComponentModel;
using GSF.Configuration;

namespace AdapterExplorer
{
    /// <summary>
    /// Defines settings for the DataExtractor application.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Default value expressions in this class reference the primary form instance, as a result,
    /// instances of this class should only be created from the primary UI thread or otherwise
    /// use <see cref="System.Windows.Forms.Form.Invoke(Delegate)"/>.
    /// </para>
    /// <para>
    /// In order for properties of this class decorated with <see cref="TypeConvertedValueExpressionAttribute"/>
    /// to have access to form element values, the elements should be declared with "public" access.
    /// </para>
    /// </remarks>
    public sealed class Settings : CategorizedSettingsBase<Settings>
    {
        /// <summary>
        /// Creates a new <see cref="Settings"/> instance.
        /// </summary>
        /// <param name="typeRegistry">
        /// Type registry to use when parsing <see cref="TypeConvertedValueExpressionAttribute"/> instances,
        /// or <c>null</c> to use <see cref="ValueExpressionParser.DefaultTypeRegistry"/>.
        /// </param>
        public Settings(TypeRegistry typeRegistry) : base("systemSettings", typeRegistry)
        {
        }

        /// <summary>
        /// Gets or sets host address for historian connection.
        /// </summary>
        //[TypeConvertedValueExpression("Form.textBoxHistorianHostAddress.Text")]
        [Description("Host address for historian connection.")]
        [UserScopedSetting]
        public string HostAddress { get; set; }

        /// <summary>
        /// Gets or sets port for historian GEP connection.
        /// </summary>
        //[TypeConvertedValueExpression("Form.maskedTextBoxHistorianPort.Text")]
        [Description("Meta-data port for historian connection.")]
        [UserScopedSetting]
        public int Port { get; set; }

        /// <summary>
        /// Gets or sets meta-data retriever timeout, in milliseconds.
        /// </summary>
        [UserScopedSetting]
        [Description("Meta-data retriever timeout, in milliseconds.")]
        public int MetadataTimeout { get; set; }
    }
}
