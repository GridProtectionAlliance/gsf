//******************************************************************************************************
//  InputAdapterSettingsBase.cs - Gbtc
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

namespace GSF.TimeSeries.Adapters
{
    /// <summary>
    /// Connection string settings for <see cref="InputAdapterBase"/>.
    /// </summary>
    public abstract class InputAdapterSettingsBase : AdapterSettingsBase
    {
        #region [ Members ]

        // Fields
        private string m_inputSignalExpression;
        private string m_outputSourceIDs;

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Redefined to hide this property as a connection string parameter.
        /// Input adapters do not have any input signals.
        /// </summary>
        public new string InputSignalExpression
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
        /// Gets or sets <see cref="MeasurementKey.Source"/> values used to filter output signals.
        /// </summary>
        /// <remarks>
        /// This allows an adapter to associate itself with entire collections of signals based on the source of the measurement keys.
        /// Set to <c>null</c> apply no filter.
        /// </remarks>
        [ConnectionStringParameter,
        DefaultValue(null),
        Description("Defines the source of the measurements produced by this adapter.")]
        public string OutputSourceIDs
        {
            get
            {
                return m_outputSourceIDs;
            }
            set
            {
                m_outputSourceIDs = value;
            }
        }

        #endregion
    }
}
