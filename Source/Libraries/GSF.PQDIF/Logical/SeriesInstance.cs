//******************************************************************************************************
//  SeriesInstance.cs - Gbtc
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
using GSF.PQDIF.Physical;

namespace GSF.PQDIF.Logical
{
    /// <summary>
    /// Represents an instance of a series in a PQDIF file. A series
    /// instance resides in a <see cref="ChannelInstance"/> and is
    /// defined by a <see cref="SeriesDefinition"/>.
    /// </summary>
    public class SeriesInstance
    {
        #region [ Members ]

        // Fields
        private readonly CollectionElement m_physicalStructure;
        private readonly ChannelInstance m_channel;
        private readonly SeriesDefinition m_definition;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new instance of the <see cref="SeriesInstance"/> class.
        /// </summary>
        /// <param name="physicalStructure">The physical structure of the series instance.</param>
        /// <param name="channel">The channel instance that this series instance resides in.</param>
        /// <param name="definition">The series definition that defines this series instance.</param>
        public SeriesInstance(CollectionElement physicalStructure, ChannelInstance channel, SeriesDefinition definition)
        {
            m_physicalStructure = physicalStructure;
            m_channel = channel;
            m_definition = definition;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets the channel instance in which the series instance resides.
        /// </summary>
        public ChannelInstance Channel
        {
            get
            {
                return m_channel;
            }
        }

        /// <summary>
        /// Gets the series definition that defines the series.
        /// </summary>
        public SeriesDefinition Definition
        {
            get
            {
                return m_definition;
            }
        }

        /// <summary>
        /// Gets the value by which to scale the values in
        /// order to restore the original data values.
        /// </summary>
        public ScalarElement SeriesScale
        {
            get
            {
                return m_physicalStructure.GetScalarByTag(SeriesScaleTag);
            }
        }

        /// <summary>
        /// Gets the value added to the values in order
        /// to restore the original data values.
        /// </summary>
        public ScalarElement SeriesOffset
        {
            get
            {
                return m_physicalStructure.GetScalarByTag(SeriesOffsetTag);
            }
        }

        /// <summary>
        /// Gets the values contained in this series instance.
        /// </summary>
        public VectorElement SeriesValues
        {
            get
            {
                SeriesInstance seriesShareSeries = SeriesShareSeries;

                return ((object)seriesShareSeries != null)
                    ? seriesShareSeries.SeriesValues
                    : m_physicalStructure.GetVectorByTag(SeriesValuesTag);
            }
        }

        /// <summary>
        /// Gets the original data values, after expanding
        /// sequences and scale and offset modifications.
        /// </summary>
        public IList<object> OriginalValues
        {
            get
            {
                return GetOriginalValues();
            }
        }

        /// <summary>
        /// Gets the index of the channel that owns the series to be shared.
        /// </summary>
        public uint? SeriesShareChannelIndex
        {
            get
            {
                ScalarElement seriesShareChannelIndexScalar = m_physicalStructure
                    .GetScalarByTag(SeriesShareChannelIndexTag);

                return ((object)seriesShareChannelIndexScalar != null)
                    ? seriesShareChannelIndexScalar.GetUInt4()
                    : (uint?)null;
            }
        }

        /// <summary>
        /// Gets the index of the series to be shared.
        /// </summary>
        public uint? SeriesShareSeriesIndex
        {
            get
            {
                ScalarElement seriesShareSeriesIndexScalar = m_physicalStructure
                    .GetScalarByTag(SeriesShareSeriesIndexTag);

                return ((object)seriesShareSeriesIndexScalar != null)
                    ? seriesShareSeriesIndexScalar.GetUInt4()
                    : (uint?)null;
            }
        }

        /// <summary>
        /// Gets the channel that owns the series to be shared.
        /// </summary>
        public ChannelInstance SeriesShareChannel
        {
            get
            {
                uint? seriesShareChannelIndex = SeriesShareChannelIndex;

                return ((object)seriesShareChannelIndex != null)
                    ? m_channel.ObservationRecord.ChannelInstances[(int)seriesShareChannelIndex]
                    : null;
            }
        }

        /// <summary>
        /// Gets the series to be shared.
        /// </summary>
        public SeriesInstance SeriesShareSeries
        {
            get
            {
                uint? seriesShareSeriesIndex = SeriesShareSeriesIndex;
                ChannelInstance seriesShareChannel = SeriesShareChannel;
                SeriesInstance seriesShareSeries = null;

                if (((object)seriesShareSeriesIndex != null) && ((object)seriesShareChannel != null))
                    seriesShareSeries = seriesShareChannel.SeriesInstances[(int)seriesShareSeriesIndex];

                return seriesShareSeries;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Gets the original data values by expanding
        /// sequences and applying scale and offset.
        /// </summary>
        /// <returns>A list of the original data values.</returns>
        private IList<object> GetOriginalValues()
        {
            IList<object> values = new List<object>();
            VectorElement valuesVector = SeriesValues;
            StorageMethods storageMethods = Definition.StorageMethodID;

            bool incremented = (storageMethods & StorageMethods.Increment) != 0;
            dynamic start, count, increment;

            bool scaled = (storageMethods & StorageMethods.Scaled) != 0;
            dynamic offset = ((object)SeriesOffset != null) ? SeriesOffset.Get() : 0;
            dynamic scale = ((object)SeriesScale != null) ? SeriesScale.Get() : 1;
            dynamic value;

            if (!scaled)
            {
                offset = 0;
                scale = 1;
            }

            if (incremented)
            {
                start = valuesVector.Get(0);
                count = valuesVector.Get(1);
                increment = valuesVector.Get(2);

                for (int i = 0; i < count; i++)
                    values.Add((object)(start + (i * increment)));
            }
            else
            {
                for (int i = 0; i < valuesVector.Size; i++)
                    values.Add(valuesVector.Get(i));
            }

            for (int i = 0; i < values.Count; i++)
            {
                value = values[i];
                values[i] = offset + (value * scale);
            }

            return values;
        }

        #endregion

        #region [ Static ]

        // Static Fields

        /// <summary>
        /// Tag that identifies the scale value to apply to the series.
        /// </summary>
        public static readonly Guid SeriesScaleTag = new Guid("3d786f96-f76e-11cf-9d89-0080c72e70a3");

        /// <summary>
        /// Tag that identifies the offset value to apply to the series.
        /// </summary>
        public static readonly Guid SeriesOffsetTag = new Guid("3d786f97-f76e-11cf-9d89-0080c72e70a3");

        /// <summary>
        /// Tag that identifies the values contained in the series.
        /// </summary>
        public static readonly Guid SeriesValuesTag = new Guid("3d786f99-f76e-11cf-9d89-0080c72e70a3");

        /// <summary>
        /// Tag that identifies the index of the channel that owns the series to be shared.
        /// </summary>
        public static readonly Guid SeriesShareChannelIndexTag = new Guid("8973861f-f1c3-11cf-9d89-0080c72e70a3");

        /// <summary>
        /// Tag that identifies the index of the series to be shared.
        /// </summary>
        public static readonly Guid SeriesShareSeriesIndexTag = new Guid("89738620-f1c3-11cf-9d89-0080c72e70a3");

        #endregion
    }
}
