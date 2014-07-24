//******************************************************************************************************
//  ChannelInstance.cs - Gbtc
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
    /// Represents a channel instance in a PQDIF file. A channel instance
    /// resides in an <see cref="ObservationRecord"/>, and is defined by
    /// a <see cref="ChannelDefinition"/> inside the observation record's
    /// <see cref="DataSourceRecord"/>.
    /// </summary>
    public class ChannelInstance
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
        /// Gets the index of the <see cref="ChannelDefinition"/>
        /// which defines the channel instance.
        /// </summary>
        public uint ChannelDefinitionIndex
        {
            get
            {
                return m_physicalStructure
                    .GetScalarByTag(ChannelDefinitionIndexTag)
                    .GetUInt4();
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
                VectorElement moduleNameVector = m_physicalStructure.GetVectorByTag(ChannelTriggerModuleNameTag);

                if ((object)moduleNameVector == null)
                    return null;

                return Encoding.ASCII.GetString(moduleNameVector.GetValues()).Trim((char)0);
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
                VectorElement deviceNameVector = m_physicalStructure.GetVectorByTag(CrossTriggerDeviceNameTag);

                if ((object)deviceNameVector == null)
                    return null;

                return Encoding.ASCII.GetString(deviceNameVector.GetValues()).Trim((char)0);
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

        #region [ Static ]

        // Static Fields

        /// <summary>
        /// Tag that identifies the channel definition index.
        /// </summary>
        public static readonly Guid ChannelDefinitionIndexTag = new Guid("b48d858f-f5f5-11cf-9d89-0080c72e70a3");

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
