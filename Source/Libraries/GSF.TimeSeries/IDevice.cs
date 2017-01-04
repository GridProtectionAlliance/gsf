//******************************************************************************************************
//  IDevice.cs - Gbtc
//
//  Copyright © 2014, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://www.opensource.org/licenses/MIT
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

namespace GSF.TimeSeries
{
    /// <summary>
    /// Represents a device that acts as a source of <see cref="IMeasurement"/>s.
    /// </summary>
    public interface IDevice
    {
        /// <summary>
        /// Gets or sets total data quality errors of this <see cref="IDevice"/>.
        /// </summary>
        long DataQualityErrors
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets total time quality errors of this <see cref="IDevice"/>.
        /// </summary>
        long TimeQualityErrors
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets total device errors of this <see cref="IDevice"/>.
        /// </summary>
        long DeviceErrors
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets total measurements received for this <see cref="IDevice"/>.
        /// </summary>
        long MeasurementsReceived
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets total measurements expected to have been received for this <see cref="IDevice"/>.
        /// </summary>
        long MeasurementsExpected
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the number of measurements recevied while this <see cref="IDevice"/> was reporting errors.
        /// </summary>
        long MeasurementsWithError
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the number of measurements (per frame) defined for this <see cref="IDevice"/>.
        /// </summary>
        long MeasurementsDefined
        {
            get;
            set;
        }
    }
}
