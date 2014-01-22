//******************************************************************************************************
//  AdapterSettingsBase.cs - Gbtc
//
//  Copyright © 2014, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the Eclipse Public License -v 1.0 (the "License"); you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://www.opensource.org/licenses/eclipse-1.0.php
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  01/20/2014 - Stephen C. Wills
//       Generated original version of source code.
//
//******************************************************************************************************

using System.ComponentModel;
using GSF.Configuration;

namespace GSF.TimeSeries.Adapters
{
    /// <summary>
    /// Connection string settings for <see cref="AdapterBase"/>.
    /// </summary>
    public abstract class AdapterSettingsBase
    {
        #region [ Members ]

        // Fields
        private string m_inputSignalExpression;
        private string m_outputSignalExpression;

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the primary keys of the input signals the adapter expects.
        /// Can be one of a filter expression, measurement key, point tag, or Guid.
        /// </summary>
        [ConnectionStringParameter,
        SettingName("InputSignalIDs"),
        DefaultValue(null),
        Description("Defines primary keys of input signals the adapter expects; can be one of a filter expression, measurement key, point tag or Guid."),
        CustomConfigurationEditor("GSF.TimeSeries.UI.WPF.dll", "GSF.TimeSeries.UI.Editors.MeasurementEditor")]
        public string InputSignalExpression
        {
            get
            {
                return m_inputSignalExpression;
            }
            set
            {
                m_inputSignalExpression = value;
            }
        }

        /// <summary>
        /// Gets or sets the primary keys of the output signals the adapter will produce.
        /// Can be one of a filter expression, measurement key, point tag, or Guid.
        /// </summary>
        [ConnectionStringParameter,
        SettingName("OutputSignalIDs"),
        DefaultValue(null),
        Description("Defines primary keys of output signals the adapter will produce; can be one of a filter expression, measurement key, point tag or Guid."),
        CustomConfigurationEditor("GSF.TimeSeries.UI.WPF.dll", "GSF.TimeSeries.UI.Editors.MeasurementEditor")]
        public string OutputSignalExpression
        {
            get
            {
                return m_outputSignalExpression;
            }
            set
            {
                m_outputSignalExpression = value;
            }
        }

        #endregion
    }
}
