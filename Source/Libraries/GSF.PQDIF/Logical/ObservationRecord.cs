//******************************************************************************************************
//  ObservationRecord.cs - Gbtc
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
using System.Text;
using GSF.PQDIF.Physical;

namespace GSF.PQDIF.Logical
{
    /// <summary>
    /// Represents an observation record in a PQDIF file.
    /// </summary>
    public class ObservationRecord
    {
        #region [ Members ]

        // Fields
        private readonly Record m_physicalRecord;
        private readonly DataSourceRecord m_dataSource;
        private readonly MonitorSettingsRecord m_settings;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new instance of the <see cref="ObservationRecord"/> class.
        /// </summary>
        /// <param name="physicalRecord">The physical structure of the observation record.</param>
        /// <param name="dataSource">The data source record that defines the channels in this observation record.</param>
        /// <param name="settings">The monitor settings to be applied to this observation record.</param>
        private ObservationRecord(Record physicalRecord, DataSourceRecord dataSource, MonitorSettingsRecord settings)
        {
            m_physicalRecord = physicalRecord;
            m_dataSource = dataSource;
            m_settings = settings;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets the physical structure of the observation record.
        /// </summary>
        public Record PhysicalRecord
        {
            get
            {
                return m_physicalRecord;
            }
        }

        /// <summary>
        /// Gets the data source record that defines
        /// the channels in this observation record.
        /// </summary>
        public DataSourceRecord DataSource
        {
            get
            {
                return m_dataSource;
            }
        }

        /// <summary>
        /// Gets the monitor settings record that defines the
        /// settings to be applied to this observation record.
        /// </summary>
        public MonitorSettingsRecord Settings
        {
            get
            {
                return m_settings;
            }
        }

        /// <summary>
        /// Gets the name of the observation record.
        /// </summary>
        public string Name
        {
            get
            {
                VectorElement nameElement = m_physicalRecord.Body.Collection.GetVectorByTag(ObservationNameTag);
                return Encoding.ASCII.GetString(nameElement.GetValues()).Trim((char)0);
            }
            set
            {
                byte[] bytes = Encoding.ASCII.GetBytes(value + (char)0);
                VectorElement nameElement = m_physicalRecord.Body.Collection.GetVectorByTag(ObservationNameTag);

                if ((object)nameElement == null)
                {
                    nameElement = new VectorElement()
                    {
                        TagOfElement = ObservationNameTag,
                        TypeOfValue = PhysicalType.Char1
                    };

                    m_physicalRecord.Body.Collection.AddElement(nameElement);
                }

                nameElement.Size = bytes.Length;
                nameElement.SetValues(bytes, 0);
            }
        }

        /// <summary>
        /// Gets the starting time of the data in the observation record.
        /// </summary>
        public DateTime StartTime
        {
            get
            {
                return m_physicalRecord.Body.Collection
                    .GetScalarByTag(TimeStartTag)
                    .GetTimestamp();
            }
            set
            {
                ScalarElement timeStartElement = m_physicalRecord.Body.Collection.GetScalarByTag(TimeStartTag);

                if ((object)timeStartElement == null)
                {
                    timeStartElement = new ScalarElement()
                    {
                        TagOfElement = TimeStartTag,
                        TypeOfValue = PhysicalType.Timestamp
                    };

                    m_physicalRecord.Body.Collection.AddElement(timeStartElement);
                }

                timeStartElement.SetTimestamp(value);
            }
        }

        /// <summary>
        /// Gets the channel instances in this observation record.
        /// </summary>
        public IList<ChannelInstance> ChannelInstances
        {
            get
            {
                return m_physicalRecord.Body.Collection
                    .GetCollectionByTag(ChannelInstancesTag)
                    .GetElementsByTag(OneChannelInstanceTag)
                    .Cast<CollectionElement>()
                    .Select(collection => new ChannelInstance(collection, this))
                    .ToList();
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Adds a new channel instance to the collection
        /// of channel instances in this observation record.
        /// </summary>
        public ChannelInstance AddNewChannelInstance()
        {
            CollectionElement channelInstancesElement = m_physicalRecord.Body.Collection.GetCollectionByTag(ChannelInstancesTag);
            CollectionElement channelInstanceElement = new CollectionElement() { TagOfElement = OneChannelInstanceTag };
            ChannelInstance channelInstance = new ChannelInstance(channelInstanceElement, this);

            if ((object)channelInstancesElement == null)
            {
                channelInstancesElement = new CollectionElement()
                {
                    TagOfElement = OneChannelInstanceTag
                };

                m_physicalRecord.Body.Collection.AddElement(channelInstancesElement);
            }

            channelInstancesElement.AddElement(channelInstanceElement);

            return channelInstance;
        }

        /// <summary>
        /// Removes the given channel instance from the collection of channel instances.
        /// </summary>
        /// <param name="channelInstance">The channel instance to be removed.</param>
        public void Remove(ChannelInstance channelInstance)
        {
            CollectionElement channelInstancesElement = m_physicalRecord.Body.Collection.GetCollectionByTag(ChannelInstancesTag);
            List<CollectionElement> channelInstanceElements;
            ChannelInstance instance;

            if ((object)channelInstancesElement == null)
                return;

            channelInstanceElements = channelInstancesElement.GetElementsByTag(OneChannelInstanceTag).Cast<CollectionElement>().ToList();

            foreach (CollectionElement channelSettingElement in channelInstanceElements)
            {
                instance = new ChannelInstance(channelSettingElement, this);

                if (Equals(channelInstance, instance))
                    channelInstancesElement.RemoveElement(channelSettingElement);
            }
        }

        #endregion

        #region [ Static ]

        // Static Fields

        /// <summary>
        /// Tag that identifies the name of the observation record.
        /// </summary>
        public static readonly Guid ObservationNameTag = new Guid("3d786f8a-f76e-11cf-9d89-0080c72e70a3");

        /// <summary>
        /// Tag that identifies the time that the observation record was created.
        /// </summary>
        public static readonly Guid TimeCreateTag = new Guid("3d786f8b-f76e-11cf-9d89-0080c72e70a3");

        /// <summary>
        /// Tag that identifies the start time of the data in the observation record.
        /// </summary>
        public static readonly Guid TimeStartTag = new Guid("3d786f8c-f76e-11cf-9d89-0080c72e70a3");

        /// <summary>
        /// Tag that identifies the type of trigger that caused the observation.
        /// </summary>
        public static readonly Guid TriggerMethodTag = new Guid("3d786f8d-f76e-11cf-9d89-0080c72e70a3");

        /// <summary>
        /// Tag that identifies the channel instances collection.
        /// </summary>
        public static readonly Guid ChannelInstancesTag = new Guid("3d786f91-f76e-11cf-9d89-0080c72e70a3");

        /// <summary>
        /// Tag that identifies a single channel instance in the collection.
        /// </summary>
        public static readonly Guid OneChannelInstanceTag = new Guid("3d786f92-f76e-11cf-9d89-0080c72e70a3");

        // Static Methods

        /// <summary>
        /// Creates a new observation record from the given physical record
        /// if the physical record is of type observation. Returns null if
        /// it is not.
        /// </summary>
        /// <param name="physicalRecord">The physical record used to create the observation record.</param>
        /// <param name="dataSource">The data source record that defines the channels in this observation record.</param>
        /// <param name="settings">The monitor settings to be applied to this observation record.</param>
        /// <returns>The new observation record, or null if the physical record does not define a observation record.</returns>
        public static ObservationRecord CreateObservationRecord(Record physicalRecord, DataSourceRecord dataSource, MonitorSettingsRecord settings)
        {
            bool isValidObservationRecord = physicalRecord.Header.TypeOfRecord == RecordType.Observation;
            return isValidObservationRecord ? new ObservationRecord(physicalRecord, dataSource, settings) : null;
        }

        #endregion
    }
}
