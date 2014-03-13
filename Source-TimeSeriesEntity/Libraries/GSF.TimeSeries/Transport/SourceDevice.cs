//******************************************************************************************************
//  SourceDevice.cs - Gbtc
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
//  02/13/2014 - Stephen C. Wills
//       Generated original version of source code.
//
//******************************************************************************************************

namespace GSF.TimeSeries.Transport
{
    /// <summary>
    /// Represents a device which acts as a source for
    /// measurements provided to a <see cref="DataSubscriber"/>.
    /// </summary>
    public class SourceDevice : IDevice
    {
        #region [ Members ]

        // Fields
        private long m_dataQualityErrors;
        private long m_timeQualityErrors;
        private long m_deviceErrors;
        private long m_measurementsReceived;
        private long m_measurementsExpected;

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets total data quality errors of this <see cref="SourceDevice"/>.
        /// </summary>
        public long DataQualityErrors
        {
            get
            {
                return m_dataQualityErrors;
            }
            set
            {
                m_dataQualityErrors = value;
            }
        }

        /// <summary>
        /// Gets or sets total time quality errors of this <see cref="SourceDevice"/>.
        /// </summary>
        public long TimeQualityErrors
        {
            get
            {
                return m_timeQualityErrors;
            }
            set
            {
                m_timeQualityErrors = value;
            }
        }

        /// <summary>
        /// Gets or sets total device errors of this <see cref="SourceDevice"/>.
        /// </summary>
        public long DeviceErrors
        {
            get
            {
                return m_deviceErrors;
            }
            set
            {
                m_deviceErrors = value;
            }
        }

        /// <summary>
        /// Gets or sets total measurements received for this <see cref="SourceDevice"/>.
        /// </summary>
        public long MeasurementsReceived
        {
            get
            {
                return m_measurementsReceived;
            }
            set
            {
                m_measurementsReceived = value;
            }
        }

        /// <summary>
        /// Gets or sets total measurements expected to have been received for this <see cref="SourceDevice"/>.
        /// </summary>
        public long MeasurementsExpected
        {
            get
            {
                return m_measurementsExpected;
            }
            set
            {
                m_measurementsExpected = value;
            }
        }

        #endregion
    }
}
