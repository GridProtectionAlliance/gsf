//******************************************************************************************************
//  MonitorSettingsRecord.cs - Gbtc
//
//  Copyright © 2012, Grid Protection Alliance.  All Rights Reserved.
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
//  05/03/2012 - Stephen C. Wills, Grid Protection Alliance
//       Generated original version of source code.
//  12/17/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
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
        /// Gets the physical record of the monitor settings record.
        /// </summary>
        public Record PhysicalRecord
        {
            get
            {
                return m_physicalRecord;
            }
        }

        /// <summary>
        /// Gets or sets the date time at which these settings become effective.
        /// </summary>
        public DateTime Effective
        {
            get
            {
                return m_physicalRecord.Body.Collection.GetScalarByTag(EffectiveTag).GetTimestamp();
            }
            set
            {
                ScalarElement effectiveElement = m_physicalRecord.Body.Collection.GetScalarByTag(EffectiveTag);

                if ((object)effectiveElement == null)
                {
                    effectiveElement = new ScalarElement()
                    {
                        TagOfElement = EffectiveTag,
                        TypeOfValue = PhysicalType.Timestamp
                    };

                    m_physicalRecord.Body.Collection.AddElement(effectiveElement);
                }

                effectiveElement.SetTimestamp(value);
            }
        }

        /// <summary>
        /// Gets or sets the time at which the settings were installed.
        /// </summary>
        public DateTime TimeInstalled
        {
            get
            {
                return m_physicalRecord.Body.Collection.GetScalarByTag(TimeInstalledTag).GetTimestamp();
            }
            set
            {
                ScalarElement timeInstalledElement = m_physicalRecord.Body.Collection.GetScalarByTag(TimeInstalledTag);

                if ((object)timeInstalledElement == null)
                {
                    timeInstalledElement = new ScalarElement()
                    {
                        TagOfElement = TimeInstalledTag,
                        TypeOfValue = PhysicalType.Timestamp
                    };

                    m_physicalRecord.Body.Collection.AddElement(timeInstalledElement);
                }

                timeInstalledElement.SetTimestamp(value);
            }
        }

        /// <summary>
        /// Gets or sets the flag that determines whether the
        /// calibration settings need to be applied before using
        /// the values recorded by this monitor.
        /// </summary>
        public bool UseCalibration
        {
            get
            {
                return m_physicalRecord.Body.Collection.GetScalarByTag(UseCalibrationTag).GetBool4();
            }
            set
            {
                ScalarElement useCalibrationElement = m_physicalRecord.Body.Collection.GetScalarByTag(UseCalibrationTag);

                if ((object)useCalibrationElement == null)
                {
                    useCalibrationElement = new ScalarElement()
                    {
                        TagOfElement = UseCalibrationTag,
                        TypeOfValue = PhysicalType.Boolean4
                    };

                    m_physicalRecord.Body.Collection.AddElement(useCalibrationElement);
                }

                useCalibrationElement.SetBool4(value);
            }
        }

        /// <summary>
        /// Gets or sets the flag that determines whether the
        /// transducer ratio needs to be applied before using
        /// the values recorded by this monitor.
        /// </summary>
        public bool UseTransducer
        {
            get
            {
                return m_physicalRecord.Body.Collection.GetScalarByTag(UseTransducerTag).GetBool4();
            }
            set
            {
                ScalarElement useTransducerElement = m_physicalRecord.Body.Collection.GetScalarByTag(UseTransducerTag);

                if ((object)useTransducerElement == null)
                {
                    useTransducerElement = new ScalarElement()
                    {
                        TagOfElement = UseTransducerTag,
                        TypeOfValue = PhysicalType.Boolean4
                    };

                    m_physicalRecord.Body.Collection.AddElement(useTransducerElement);
                }

                useTransducerElement.SetBool4(value);
            }
        }

        /// <summary>
        /// Gets or sets the settings for the channels defined in the data source.
        /// </summary>
        public IList<ChannelSetting> ChannelSettings
        {
            get
            {
                CollectionElement channelSettingsArray = m_physicalRecord.Body.Collection.GetCollectionByTag(ChannelSettingsArrayTag);

                if ((object)channelSettingsArray == null)
                    return null;

                return channelSettingsArray
                    .GetElementsByTag(OneChannelSettingTag)
                    .Cast<CollectionElement>()
                    .Select(collection => new ChannelSetting(collection, this))
                    .ToList();
            }
        }

        /// <summary>
        /// Gets or sets nominal frequency.
        /// </summary>
        public double NominalFrequency
        {
            get
            {
                ScalarElement nominalFrequencyElement = m_physicalRecord.Body.Collection.GetScalarByTag(NominalFrequencyTag);

                if ((object)nominalFrequencyElement == null)
                    return DefaultNominalFrequency;

                return nominalFrequencyElement.GetReal8();
            }
            set
            {
                ScalarElement nominalFrequencyElement = m_physicalRecord.Body.Collection.GetScalarByTag(NominalFrequencyTag);

                if ((object)nominalFrequencyElement == null)
                {
                    nominalFrequencyElement = new ScalarElement()
                    {
                        TagOfElement = NominalFrequencyTag,
                        TypeOfValue = PhysicalType.Real8
                    };

                    m_physicalRecord.Body.Collection.AddElement(nominalFrequencyElement);
                }

                nominalFrequencyElement.SetReal8(value);
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Adds a new channel setting to the collection
        /// of channel settings in this monitor settings record.
        /// </summary>
        public ChannelSetting AddNewChannelSetting()
        {
            CollectionElement channelSettingsElement = m_physicalRecord.Body.Collection.GetCollectionByTag(ChannelSettingsArrayTag);
            CollectionElement channelSettingElement = new CollectionElement() { TagOfElement = OneChannelSettingTag };
            ChannelSetting channelSetting = new ChannelSetting(channelSettingElement, this);

            if ((object)channelSettingsElement == null)
            {
                channelSettingsElement = new CollectionElement()
                {
                    TagOfElement = OneChannelSettingTag
                };

                m_physicalRecord.Body.Collection.AddElement(channelSettingsElement);
            }

            channelSettingsElement.AddElement(channelSettingElement);

            return channelSetting;
        }

        /// <summary>
        /// Removes the given channel setting from the collection of channel settings.
        /// </summary>
        /// <param name="channelSetting">The channel setting to be removed.</param>
        public void Remove(ChannelSetting channelSetting)
        {
            CollectionElement channelSettingsElement = m_physicalRecord.Body.Collection.GetCollectionByTag(ChannelSettingsArrayTag);
            List<CollectionElement> channelSettingElements;
            ChannelSetting setting;

            if ((object)channelSettingsElement == null)
                return;

            channelSettingElements = channelSettingsElement.GetElementsByTag(OneChannelSettingTag).Cast<CollectionElement>().ToList();

            foreach (CollectionElement channelSettingElement in channelSettingElements)
            {
                setting = new ChannelSetting(channelSettingElement, this);

                if (Equals(channelSetting, setting))
                    channelSettingsElement.RemoveElement(channelSettingElement);
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

        // Static Properties

        /// <summary>
        /// Gets or sets the default value for the <see cref="NominalFrequency"/>
        /// property when the value is not defined in the PQDIF file.
        /// </summary>
        public static double DefaultNominalFrequency { get; set; } = 60.0D;

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
