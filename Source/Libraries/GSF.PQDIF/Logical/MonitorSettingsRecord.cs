//******************************************************************************************************
//  MonitorSettingsRecord.cs - Gbtc
//
//  Copyright © 2012, Grid Protection Alliance.  All Rights Reserved.
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
//  05/03/2012 - Stephen C. Wills, Grid Protection Alliance
//       Generated original version of source code.
//  12/17/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using GSF.PQDIF.Physical;

namespace GSF.PQDIF.Logical
{
    /// <summary>
    /// Represents a monitor settings record in a PQDIF file.
    /// </summary>
    public class MonitorSettingsRecord
    {
        #region [ Members ]

        // Fields
        private readonly Record m_physicalRecord;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new instance of the <see cref="MonitorSettingsRecord"/> class.
        /// </summary>
        /// <param name="physicalRecord">The physical structure of the monitor settings record.</param>
        private MonitorSettingsRecord(Record physicalRecord)
        {
            m_physicalRecord = physicalRecord;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets nominal frequency.
        /// </summary>
        public double NominalFrequency
        {
            get
            {
                ScalarElement nominalFrequencyElement = m_physicalRecord.Body.Collection.GetScalarByTag(NominalFrequencyTag);

                if ((object)nominalFrequencyElement == null)
                    return 60.0D;

                return nominalFrequencyElement.GetReal8();
            }
        }

        #endregion

        #region [ Static ]

        // Static Fields

        /// <summary>
        /// Tag that identifies the time that these settings become effective.
        /// </summary>
        public static readonly Guid EffectiveTag = new Guid("62f28183-f9c4-11cf-9d89-0080c72e70a3");

        /// <summary>
        /// Tag that identifies the install time.
        /// </summary>
        public static readonly Guid TimeInstalledTag = new Guid("3d786f85-f76e-11cf-9d89-0080c72e70a3");

        /// <summary>
        /// Tag that identifies the flag which determines whether to apply calibration to the series.
        /// </summary>
        public static readonly Guid UseCalibrationTag = new Guid("62f28180-f9c4-11cf-9d89-0080c72e70a3");

        /// <summary>
        /// Tag that identifies the flag which determines whether to apply transducer adjustments to the series.
        /// </summary>
        public static readonly Guid UseTransducerTag = new Guid("62f28181-f9c4-11cf-9d89-0080c72e70a3");

        /// <summary>
        /// Tag that identifies the collection of channel settings.
        /// </summary>
        public static readonly Guid ChannelSettingsArrayTag = new Guid("62f28182-f9c4-11cf-9d89-0080c72e70a3");

        /// <summary>
        /// Tag that identifies one channel setting in the collection.
        /// </summary>
        public static readonly Guid OneChannelSettingTag = new Guid("3d786f9a-f76e-11cf-9d89-0080c72e70a3");

        /// <summary>
        /// Tag that identifies the nominal frequency.
        /// </summary>
        public static readonly Guid NominalFrequencyTag = new Guid("0fa118c3-cb4a-11d2-b30b-fe25cb9a1760");

        // Static Methods

        /// <summary>
        /// Creates a new monitor settings record from the given physical record
        /// if the physical record is of type monitor settings. Returns null if
        /// it is not.
        /// </summary>
        /// <param name="physicalRecord">The physical record used to create the monitor settings record.</param>
        /// <returns>The new monitor settings record, or null if the physical record does not define a monitor settings record.</returns>
        public static MonitorSettingsRecord CreateMonitorSettingsRecord(Record physicalRecord)
        {
            bool isValidMonitorSettingsRecord = physicalRecord.Header.TypeOfRecord == RecordType.MonitorSettings;
            return isValidMonitorSettingsRecord ?  new MonitorSettingsRecord(physicalRecord) : null;
        }

        #endregion
        
    }
}
