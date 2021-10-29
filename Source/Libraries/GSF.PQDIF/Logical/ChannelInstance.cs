//******************************************************************************************************
//  ChannelInstance.cs - Gbtc
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
//  05/04/2012 - Stephen C. Wills, Grid Protection Alliance
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
    /// Represents a channel instance in a PQDIF file. See IEEE 1159.3-2019 section 5.4.3
    /// for details. A channel instance resides in an <see cref="ObservationRecord"/>, and 
    /// is defined by a <see cref="ChannelDefinition"/> inside the observation record's
    /// <see cref="DataSourceRecord"/>.
    /// </summary>
    public class ChannelInstance : IEquatable<ChannelInstance>
    {
        #region [ Members ]

        // Fields
        private readonly CollectionElement m_physicalStructure;
        private readonly ObservationRecord m_observationRecord;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new instance of the <see cref="ChannelInstance"/> class.
        /// </summary>
        /// <param name="physicalStructure">The collection element which is the physical structure of the channel instance.</param>
        /// <param name="observationRecord">The observation record in which the channel instance resides.</param>
        public ChannelInstance(CollectionElement physicalStructure, ObservationRecord observationRecord)
        {
            m_physicalStructure = physicalStructure;
            m_observationRecord = observationRecord;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets the physical structure of the channel instance.
        /// </summary>
        public CollectionElement PhysicalStructure
        {
            get
            {
                return m_physicalStructure;
            }
        }

        /// <summary>
        /// Gets the observation record in which the channel instance resides.
        /// </summary>
        public ObservationRecord ObservationRecord
        {
            get
            {
                return m_observationRecord;
            }
        }

        /// <summary>
        /// Gets the channel definition which defines the channel instance.
        /// </summary>
        public ChannelDefinition Definition
        {
            get
            {
                return m_observationRecord.DataSource.ChannelDefinitions[(int)ChannelDefinitionIndex];
            }
        }

        /// <summary>
        /// Gets the channel setting which defines the instrument settings for the channel.
        /// </summary>
        public ChannelSetting Setting
        {
            get
            {
                MonitorSettingsRecord monitorSettings;
                IList<ChannelSetting> channelSettings;

                monitorSettings = m_observationRecord.Settings;

                if ((object)monitorSettings == null)
                    return null;

                channelSettings = monitorSettings.ChannelSettings;

                if ((object)channelSettings == null)
                    return null;

                return channelSettings.FirstOrDefault(channelSetting => channelSetting.ChannelDefinitionIndex == ChannelDefinitionIndex);
            }
        }

        /// <summary>
        /// Gets the index of the <see cref="ChannelDefinition"/>
        /// which defines the channel instance.
        /// </summary>
        public uint ChannelDefinitionIndex
        {
            get
            {
                return m_physicalStructure
                    .GetScalarByTag(ChannelDefinition.ChannelDefinitionIndexTag)
                    .GetUInt4();
            }
            set
            {
                ScalarElement channelDefinitionIndexElement = m_physicalStructure.GetOrAddScalar(ChannelDefinition.ChannelDefinitionIndexTag);
                channelDefinitionIndexElement.TypeOfValue = PhysicalType.UnsignedInteger4;
                channelDefinitionIndexElement.SetUInt4(value);
            }
        }

        /// <summary>
        /// Gets the identifier for the harmonic or
        /// interharmonic group represented by this channel.
        /// </summary>
        public short ChannelGroupID
        {
            get
            {
                ScalarElement channelGroupIDElement = m_physicalStructure.GetScalarByTag(ChannelGroupIDTag);

                if ((object)channelGroupIDElement == null)
                    return 0;

                return channelGroupIDElement.GetInt2();
            }
            set
            {
                ScalarElement channelGroupIDElement = m_physicalStructure.GetOrAddScalar(ChannelGroupIDTag);
                channelGroupIDElement.TypeOfValue = PhysicalType.Integer2;
                channelGroupIDElement.SetInt2(value);
            }
        }

        /// <summary>
        /// Gets the name of the of a device specific code or hardware
        /// module, algorithm, or rule not necessarily channel based
        /// that cause this channel to be recorded.
        /// </summary>
        public string TriggerModuleName
        {
            get
            {
                VectorElement moduleNameElement = m_physicalStructure.GetVectorByTag(ChannelTriggerModuleNameTag);

                if ((object)moduleNameElement == null)
                    return null;

                return Encoding.ASCII.GetString(moduleNameElement.GetValues()).Trim((char)0);
            }
            set
            {
                byte[] bytes = Encoding.ASCII.GetBytes(value + (char)0);
                m_physicalStructure.AddOrUpdateVector(ChannelTriggerModuleNameTag, PhysicalType.Char1, bytes);
            }
        }

        /// <summary>
        /// Gets the name of the device involved in
        /// an external cross trigger scenario.
        /// </summary>
        public string CrossTriggerDeviceName
        {
            get
            {
                VectorElement deviceNameElement = m_physicalStructure.GetVectorByTag(CrossTriggerDeviceNameTag);

                if ((object)deviceNameElement == null)
                    return null;

                return Encoding.ASCII.GetString(deviceNameElement.GetValues()).Trim((char)0);
            }
            set
            {
                byte[] bytes = Encoding.ASCII.GetBytes(value + (char)0);
                m_physicalStructure.AddOrUpdateVector(CrossTriggerDeviceNameTag, PhysicalType.Char1, bytes);
            }
        }

        /// <summary>
        /// Gets the series instances contained in this channel.
        /// </summary>
        public IList<SeriesInstance> SeriesInstances
        {
            get
            {
                return m_physicalStructure
                    .GetCollectionByTag(SeriesInstancesTag)
                    .GetElementsByTag(OneSeriesInstanceTag)
                    .Cast<CollectionElement>()
                    .Zip(Definition.SeriesDefinitions, (collection, seriesDefinition) => new SeriesInstance(collection, this, seriesDefinition))
                    .ToList();
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Adds a new series instance to the collection
        /// of series instances in this channel instance.
        /// </summary>
        public SeriesInstance AddNewSeriesInstance()
        {
            if (Definition.SeriesDefinitions.Count <= SeriesInstances.Count)
                throw new InvalidOperationException("Cannot create a series instance without a corresponding series definition.");

            CollectionElement seriesInstanceElement = new CollectionElement() { TagOfElement = OneSeriesInstanceTag };
            SeriesDefinition seriesDefinition = Definition.SeriesDefinitions[SeriesInstances.Count];
            SeriesInstance seriesInstance = new SeriesInstance(seriesInstanceElement, this, seriesDefinition);
            seriesInstanceElement.AddOrUpdateVector(SeriesInstance.SeriesValuesTag, PhysicalType.UnsignedInteger1, new byte[0]);

            CollectionElement seriesInstancesElement = m_physicalStructure.GetCollectionByTag(SeriesInstancesTag);

            if ((object)seriesInstancesElement == null)
            {
                seriesInstancesElement = new CollectionElement() { TagOfElement = SeriesInstancesTag };
                m_physicalStructure.AddElement(seriesInstancesElement);
            }

            seriesInstancesElement.AddElement(seriesInstanceElement);

            return seriesInstance;
        }

        /// <summary>
        /// Removes the given series instance from the collection of series instances.
        /// </summary>
        /// <param name="seriesInstance">The series instance to be removed.</param>
        public void Remove(SeriesInstance seriesInstance)
        {
            CollectionElement seriesInstancesElement;
            List<CollectionElement> seriesInstanceElements;
            IList<SeriesDefinition> seriesDefinitions;
            SeriesInstance instance;

            seriesInstancesElement = m_physicalStructure.GetCollectionByTag(SeriesInstancesTag);

            if ((object)seriesInstancesElement == null)
                return;

            seriesDefinitions = Definition.SeriesDefinitions;
            seriesInstanceElements = seriesInstancesElement.GetElementsByTag(OneSeriesInstanceTag).Cast<CollectionElement>().ToList();

            for (int i = seriesInstanceElements.Count; i >= 0; i--)
            {
                instance = new SeriesInstance(seriesInstanceElements[i], this, seriesDefinitions[i]);

                if (Equals(seriesInstance, instance))
                    seriesInstancesElement.RemoveElement(seriesInstancesElement);
            }
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.</returns>
        public bool Equals(ChannelInstance other)
        {
            if ((object)other == null)
                return false;

            return ReferenceEquals(m_physicalStructure, other.m_physicalStructure);
        }

        /// <summary>
        /// Determines whether the specified <see cref="ChannelInstance"/> is equal to the current <see cref="ChannelInstance"/>.
        /// </summary>
        /// <param name="obj">The object to compare with the current object. </param>
        /// <returns>true if the specified object  is equal to the current object; otherwise, false.</returns>
        /// <filterpriority>2</filterpriority>
        public override bool Equals(object obj)
        {
            return Equals(obj as ChannelInstance);
        }

        /// <summary>
        /// Serves as a hash function for a particular type. 
        /// </summary>
        /// <returns>A hash code for the current <see cref="ChannelInstance"/>.</returns>
        /// <filterpriority>2</filterpriority>
        public override int GetHashCode()
        {
            return m_physicalStructure.GetHashCode();
        }

        #endregion

        #region [ Static ]

        // Static Fields

        /// <summary>
        /// Tag that identifies the channel group ID.
        /// </summary>
        public static readonly Guid ChannelGroupIDTag = new Guid("f90de218-e67b-4cf1-a295-b021a2d46767");

        /// <summary>
        /// Tag that identifies the series instances collection.
        /// </summary>
        public static readonly Guid SeriesInstancesTag = new Guid("3d786f93-f76e-11cf-9d89-0080c72e70a3");

        /// <summary>
        /// Tag that identifies a single series instance in the collection.
        /// </summary>
        public static readonly Guid OneSeriesInstanceTag = new Guid("3d786f94-f76e-11cf-9d89-0080c72e70a3");

        /// <summary>
        /// Tag that identifies the channel trigger module name.
        /// </summary>
        public static readonly Guid ChannelTriggerModuleNameTag = new Guid("0fa118c6-cb4a-11cf-9d89-0080c72e70a3");

        /// <summary>
        /// Tag that identifies the cross trigger device name.
        /// </summary>
        public static readonly Guid CrossTriggerDeviceNameTag = new Guid("0fa118c5-cb4a-11cf-9d89-0080c72e70a3");

        #endregion
    }
}
