//******************************************************************************************************
//  Settings.cs - Gbtc
//
//  Copyright © 2017, Grid Protection Alliance.  All Rights Reserved.
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
//  03/26/2020 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.ComponentModel;
using System.Configuration;
using ExpressionEvaluator;
using GSF.ComponentModel;
using GSF.Configuration;

namespace UpdateTagNames
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
        [TypeConvertedValueExpression("Form.textBoxConfigFile.Text")]
        [Description("Target config file name.")]
        [UserScopedSetting]
        public string TargetConfigFile { get; set; }

        /// <summary>
        /// Gets or sets port for historian GEP connection.
        /// </summary>
        [TypeConvertedValueExpression("Form.textBoxExpression.Text")]
        [Description("Point tag name expression.")]
        [UserScopedSetting]
        public string Expression { get; set; }

        /// <summary>
        /// Gets or sets name of historian instance to access.
        /// </summary>
        [TypeConvertedValueExpression("Form.checkBoxSetPortNumber.Checked")]
        [Description("Flag that determines if STTP port number should be set.")]
        [UserScopedSetting]
        public bool SetPortNumber { get; set; }

        /// <summary>
        /// Gets or sets frame-rate, in frames per second, used to estimate total data for timespan.
        /// </summary>
        [TypeConvertedValueExpression("Form.maskedTextBoxPortNumber.Text")]
        [Description("Target STTP port number.")]
        [UserScopedSetting]
        public int PortNumber { get; set; }
    }
}
