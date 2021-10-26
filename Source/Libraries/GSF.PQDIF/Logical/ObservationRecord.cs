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
    #region [ Enumerations ]

    /// <summary>
    /// Type of trigger which caused the observation.
    /// </summary>
    public enum TriggerMethod : uint
    {
        /// <summary>
        /// No trigger.
        /// </summary>
        None = 0u,

        /// <summary>
        /// A specific channel (or channels) caused the trigger; should be
        /// used with tagChannelTriggerIdx to specify which channels.
        /// </summary>
        Channel = 1u,

        /// <summary>
        /// Periodic data trigger.
        /// </summary>
        Periodic = 2u,

        /// <summary>
        /// External system trigger.
        /// </summary>
        External = 3u,

        /// <summary>
        /// Periodic statistical data.
        /// </summary>
        PeriodicStats = 4u
    }

    #endregion

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
                CollectionElement collectionElement = m_physicalRecord.Body.Collection;
                VectorElement nameElement = collectionElement.GetVectorByTag(ObservationNameTag);
                return Encoding.ASCII.GetString(nameElement.GetValues()).Trim((char)0);
            }
            set
            {
                CollectionElement collectionElement = m_physicalRecord.Body.Collection;
                byte[] bytes = Encoding.ASCII.GetBytes(value + (char)0);
                collectionElement.AddOrUpdateVector(ObservationNameTag, PhysicalType.Char1, bytes);
            }
        }

        /// <summary>
        /// Gets the creation time of the observation record.
        /// </summary>
        public DateTime CreateTime
        {
            get
            {
                return m_physicalRecord.Body.Collection
                    .GetScalarByTag(TimeCreateTag)
                    .GetTimestamp();
            }
            set
            {
                CollectionElement collectionElement = m_physicalRecord.Body.Collection;
                ScalarElement timeCreateElement = collectionElement.GetOrAddScalar(TimeCreateTag);
                timeCreateElement.TypeOfValue = PhysicalType.Timestamp;
                timeCreateElement.SetTimestamp(value);
            }
        }

        /// <summary>
        /// Gets the starting time of the data in the observation record. This time should
        /// be used as the base time for all relative seconds recorded in the series instances.
        /// </summary>
        /// <remarks>
        /// The <see cref="ObservationRecord"/> contains two timestamp fields: <see cref="StartTime"/> and
        /// <see cref="TimeTriggered"/>. The StartTime is a required part of any Observation, whereas the 
        /// <see cref="TimeTriggered"/> is optional. 
        /// 
        /// The <see cref="StartTime"/> does not have to be the same as the trigger time and can therefore 
        /// be chosen more or less arbitrarily. For instance, you can choose to record the 
        /// <see cref="StartTime"/> as the timestamp of the first data point in the observation or the top 
        /// of an interval in which the data was captured. The trigger time field is more well defined, 
        /// essentially always representing the point in time at which the data source decided to capture 
        /// some data in the PQDIF file.
        /// </remarks>
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
                CollectionElement collectionElement = m_physicalRecord.Body.Collection;
                ScalarElement timeStartElement = collectionElement.GetOrAddScalar(TimeStartTag);
                timeStartElement.TypeOfValue = PhysicalType.Timestamp;
                timeStartElement.SetTimestamp(value);
            }
        }

        /// <summary>
        /// Gets or sets the type of trigger which caused the observation.
        /// </summary>
        public TriggerMethod TriggerMethod
        {
            get
            {
                return (TriggerMethod)m_physicalRecord.Body.Collection
                    .GetScalarByTag(TriggerMethodTag)
                    .GetUInt4();
            }
            set
            {
                CollectionElement collectionElement = m_physicalRecord.Body.Collection;
                ScalarElement triggerMethodElement = collectionElement.GetOrAddScalar(TriggerMethodTag);
                triggerMethodElement.TypeOfValue = PhysicalType.UnsignedInteger4;
                triggerMethodElement.SetUInt4((uint)value);
            }
        }


        /// <summary>
        /// Gets the time the observation was triggered. For more information regarding recording
        /// time in PQD files, see <see cref="StartTime"/>.
        /// </summary>
        public DateTime TimeTriggered
        {
            get
            {
                ScalarElement timeTriggeredElement = m_physicalRecord.Body.Collection
                    .GetScalarByTag(TimeTriggeredTag);

                if ((object)timeTriggeredElement == null)
                    return DateTime.MinValue;

                return timeTriggeredElement.GetTimestamp();
            }
            set
            {
                CollectionElement collectionElement = m_physicalRecord.Body.Collection;
                ScalarElement timeTriggeredElement = collectionElement.GetOrAddScalar(TimeTriggeredTag);
                timeTriggeredElement.TypeOfValue = PhysicalType.Timestamp;
                timeTriggeredElement.SetTimestamp(value);
            }
        }

        /// <summary>
        /// Gets or sets the Disturbance Category ID
        /// </summary>
        public Guid DisturbanceCategoryID
        {
            get
            {
                CollectionElement collectionElement = m_physicalRecord.Body.Collection;
                ScalarElement DisturbanceIDElement = collectionElement.GetScalarByTag(DisturbanceCategoryTag);

                if ((object)DisturbanceIDElement == null)
                    return DisturbanceCategory.None;

                return DisturbanceIDElement.GetGuid();
            }
            set
            {
                CollectionElement collectionElement = m_physicalRecord.Body.Collection;
                ScalarElement vendorIDElement = collectionElement.GetOrAddScalar(DisturbanceCategoryTag);
                vendorIDElement.TypeOfValue = PhysicalType.Guid;
                vendorIDElement.SetGuid(value);
            }
        }
    

        /// <summary>
        /// Gets or sets the index into <see cref="ChannelInstancesTag"/> collection within this record which initiated the observation.
        /// </summary>
        public uint[] ChannelTriggerIndex
        {
            get
            {
                VectorElement channelTriggerIndexElement = m_physicalRecord.Body.Collection
                    .GetVectorByTag(ChannelTriggerIndexTag);

                if ((object)channelTriggerIndexElement == null)
                    return new uint[0];

                return Enumerable.Range(0, channelTriggerIndexElement.Size)
                    .Select(index => channelTriggerIndexElement.GetUInt4(index))
                    .ToArray();
            }
            set
            {
                CollectionElement collectionElement = m_physicalRecord.Body.Collection;
                VectorElement channelTriggerIndexElement = collectionElement.GetOrAddVector(ChannelTriggerIndexTag);
                channelTriggerIndexElement.TypeOfValue = PhysicalType.UnsignedInteger4;
                channelTriggerIndexElement.Size = value.Length;

                for (int i = 0; i < value.Length; i++)
                    channelTriggerIndexElement.SetUInt4(i, value[i]);
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
        public ChannelInstance AddNewChannelInstance(ChannelDefinition channelDefinition)
        {
            CollectionElement channelInstanceElement = new CollectionElement() { TagOfElement = OneChannelInstanceTag };
            ChannelInstance channelInstance = new ChannelInstance(channelInstanceElement, this);

            channelInstance.ChannelDefinitionIndex = (uint)channelDefinition.DataSource.ChannelDefinitions.IndexOf(channelDefinition);
            channelInstanceElement.AddElement(new CollectionElement() { TagOfElement = ChannelInstance.SeriesInstancesTag });

            CollectionElement channelInstancesElement = m_physicalRecord.Body.Collection.GetCollectionByTag(ChannelInstancesTag);

            if ((object)channelInstancesElement == null)
            {
                channelInstancesElement = new CollectionElement() { TagOfElement = ChannelInstancesTag };
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
        /// Tag that identifies the time the observation was triggered.
        /// </summary>
        public static readonly Guid TimeTriggeredTag = new Guid("3d786f8e-f76e-11cf-9d89-0080c72e70a3");

        /// <summary>
        /// Tag that identifies the index into <see cref="ChannelInstancesTag"/> collection within this record. This specifies which channel(s) initiated the observation.
        /// </summary>
        public static readonly Guid ChannelTriggerIndexTag = new Guid("3d786f8f-f76e-11cf-9d89-0080c72e70a3");

        /// <summary>
        /// Tag that identifies the channel instances collection.
        /// </summary>
        public static readonly Guid ChannelInstancesTag = new Guid("3d786f91-f76e-11cf-9d89-0080c72e70a3");

        /// <summary>
        /// Tag that identifies a single channel instance in the collection.
        /// </summary>
        public static readonly Guid OneChannelInstanceTag = new Guid("3d786f92-f76e-11cf-9d89-0080c72e70a3");

        /// <summary>
        /// Tag that identifies the Disturbance Category.
        /// </summary>
        public static readonly Guid DisturbanceCategoryTag = new Guid("b48d8597-f5f5-11cf-9d89-0080c72e70a3");


        // Static Methods

        /// <summary>
        /// Creates a new observation record from scratch with the given data source and settings.
        /// </summary>
        /// <param name="dataSource">The data source record that defines the channels in this observation record.</param>
        /// <param name="settings">The monitor settings to be applied to this observation record.</param>
        /// <returns>The new observation record.</returns>
        public static ObservationRecord CreateObservationRecord(DataSourceRecord dataSource, MonitorSettingsRecord settings)
        {
            Guid recordTypeTag = Record.GetTypeAsTag(RecordType.Observation);
            Record physicalRecord = new Record(recordTypeTag);
            ObservationRecord observationRecord = new ObservationRecord(physicalRecord, dataSource, settings);

            DateTime now = DateTime.UtcNow;
            observationRecord.Name = now.ToString();
            observationRecord.CreateTime = now;
            observationRecord.StartTime = now;
            observationRecord.TriggerMethod = TriggerMethod.None;

            CollectionElement bodyElement = physicalRecord.Body.Collection;
            bodyElement.AddElement(new CollectionElement() { TagOfElement = ChannelInstancesTag });

            return observationRecord;
        }

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
