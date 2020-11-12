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
        /// Gets or sets flag that determines if action adapters are selected.
        /// </summary>
        [TypeConvertedValueExpression("Form.checkBoxActionAdapters.Checked")]
        [Description("Flag that determines if action adapters are selected.")]
        [UserScopedSetting]
        public bool SelectActionAdapters { get; set; }

        /// <summary>
        /// Gets or sets flag that determines if input adapters are selected.
        /// </summary>
        [TypeConvertedValueExpression("Form.checkBoxInputAdapters.Checked")]
        [Description("Flag that determines if input adapters are selected.")]
        [UserScopedSetting]
        public bool SelectInputAdapters { get; set; }

        /// <summary>
        /// Gets or sets flag that determines if output adapters are selected.
        /// </summary>
        [TypeConvertedValueExpression("Form.checkBoxOutputAdapters.Checked")]
        [UserScopedSetting]
        [Description("Flag that determines if output adapters are selected.")]
        public bool SelectOutputAdapters { get; set; }
    }
}
