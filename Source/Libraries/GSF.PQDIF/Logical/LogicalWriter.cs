//******************************************************************************************************
//  LogicalWriter.cs - Gbtc
//
//  Copyright © 2015, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  09/14/2015 - Stephen C. Wills
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GSF.PQDIF.Physical;

namespace GSF.PQDIF.Logical
{
    /// <summary>
    /// Represents an exception
    /// </summary>
    public sealed class MissingTag
    {
        #region [ Members ]

        // Fields
        private RecordType m_recordType;
        private string m_tagName;
        private Guid m_tag;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new instance of the <see cref="MissingTag"/> class.
        /// </summary>
        /// <param name="recordType">The type of record the tag is missing from.</param>
        /// <param name="tagName">The name of the tag.</param>
        /// <param name="tag">The globally unique identifier for the tag.</param>
        public MissingTag(RecordType recordType, string tagName, Guid tag)
        {
            m_recordType = recordType;
            m_tagName = tagName;
            m_tag = tag;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets the type of the record in which
        /// the tag was identified as missing.
        /// </summary>
        public RecordType RecordType
        {
            get
            {
                return m_recordType;
            }
        }

        /// <summary>
        /// Gets the name of the missing tag.
        /// </summary>
        public string TagName
        {
            get
            {
                return m_tagName;
            }
        }

        /// <summary>
        /// Gets the globally unique identifier for the given tag.
        /// </summary>
        public Guid Tag
        {
            get
            {
                return m_tag;
            }
        }

        #endregion
    }

    /// <summary>
    /// Represents a writer used to create files using the PQDIF file format.
    /// </summary>
    public sealed class LogicalWriter : IDisposable
    {
        #region [ Members ]

        // Fields
        private PhysicalWriter m_physicalWriter;
        private ContainerRecord m_containerRecord;
        private DataSourceRecord m_currentDataSource;
        private MonitorSettingsRecord m_currentMonitorSettings;
        private List<MissingTag> m_missingTags;

        private bool m_disposed;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new instance of the <see cref="LogicalWriter"/> class.
        /// </summary>
        /// <param name="filePath">The path to the file where the PQDIF data is to be written.</param>
        public LogicalWriter(string filePath)
        {
            m_physicalWriter = new PhysicalWriter(filePath);
            m_missingTags = new List<MissingTag>();
        }

        /// <summary>
        /// Creates a new instance of the <see cref="LogicalWriter"/> class.
        /// </summary>
        /// <param name="stream">The stream to write the PQDIF data to.</param>
        /// <param name="leaveOpen">Indicates whether to leave the stream open when disposing of the writer.</param>
        public LogicalWriter(Stream stream, bool leaveOpen = false)
        {
            m_physicalWriter = new PhysicalWriter(stream, leaveOpen);
            m_missingTags = new List<MissingTag>();
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets a collection of required tags identified as
        /// missing from the records written to this PQDIF file.
        /// </summary>
        public IReadOnlyCollection<MissingTag> MissingTags
        {
            get
            {
                return m_missingTags.AsReadOnly();
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Writes the given container record to the file stream.
        /// </summary>
        /// <param name="containerRecord">The container record to be written to the PQDIF file.</param>
        public void Write(ContainerRecord containerRecord)
        {
            if ((object)containerRecord == null)
                throw new ArgumentNullException("containerRecord");

            if (m_disposed)
                throw new ObjectDisposedException(GetType().Name);

            if ((object)m_containerRecord != null)
                throw new InvalidOperationException("Only one container record can be written to a PQDIF file.");

            ValidateContainerRecord(containerRecord);

            m_containerRecord = containerRecord;
            m_physicalWriter.WriteRecord(containerRecord.PhysicalRecord);
            m_physicalWriter.CompressionAlgorithm = m_containerRecord.CompressionAlgorithm;
            m_physicalWriter.CompressionStyle = m_containerRecord.CompressionStyle;
        }

        /// <summary>
        /// Writes the given observation record, as well as its data source
        /// and monitor settings records if necessary, to the file stream.
        /// </summary>
        /// <param name="observationRecord">The observation record to be written to the PQDIF file.</param>
        /// <param name="lastRecord">Indicates whether this observation record is the last record in the file.</param>
        public void Write(ObservationRecord observationRecord, bool lastRecord = false)
        {
            if ((object)observationRecord == null)
                throw new ArgumentNullException("observationRecord");

            if (m_disposed)
                throw new ObjectDisposedException(GetType().Name);

            if ((object)m_containerRecord == null)
                throw new InvalidOperationException("Container record must be the first record in a PQDIF file.");

            if ((object)observationRecord.DataSource == null)
                throw new InvalidOperationException("An observation record must have a data source.");

            if (!ReferenceEquals(m_currentDataSource, observationRecord.DataSource))
                ValidateDataSourceRecord(observationRecord.DataSource);

            if ((object)observationRecord.Settings != null && !ReferenceEquals(m_currentMonitorSettings, observationRecord.Settings))
                ValidateMonitorSettingsRecord(observationRecord.Settings);

            ValidateObservationRecord(observationRecord);

            if (!ReferenceEquals(m_currentDataSource, observationRecord.DataSource))
            {
                m_currentMonitorSettings = null;
                m_currentDataSource = observationRecord.DataSource;
                m_physicalWriter.WriteRecord(observationRecord.DataSource.PhysicalRecord);
            }

            if ((object)observationRecord.Settings != null && !ReferenceEquals(m_currentMonitorSettings, observationRecord.Settings))
            {
                m_currentMonitorSettings = observationRecord.Settings;
                m_physicalWriter.WriteRecord(observationRecord.Settings.PhysicalRecord);
            }

            m_physicalWriter.WriteRecord(observationRecord.PhysicalRecord, lastRecord);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="LogicalWriter"/> object and optionally releases the managed resources.
        /// </summary>
        public void Dispose()
        {
            if (!m_disposed)
            {
                try
                {
                    m_physicalWriter.Dispose();
                }
                finally
                {
                    m_disposed = true;
                }
            }
        }

        private void ValidateContainerRecord(ContainerRecord containerRecord)
        {
            if (!containerRecord.PhysicalRecord.Body.Collection.GetElementsByTag(ContainerRecord.VersionInfoTag).Any())
                m_missingTags.Add(new MissingTag(RecordType.Container, "VersionInfoTag", ContainerRecord.VersionInfoTag));

            if (!containerRecord.PhysicalRecord.Body.Collection.GetElementsByTag(ContainerRecord.FileNameTag).Any())
                m_missingTags.Add(new MissingTag(RecordType.Container, "FileNameTag", ContainerRecord.FileNameTag));

            if (!containerRecord.PhysicalRecord.Body.Collection.GetElementsByTag(ContainerRecord.VersionInfoTag).Any())
                m_missingTags.Add(new MissingTag(RecordType.Container, "CreationTag", ContainerRecord.CreationTag));
        }

        private void ValidateDataSourceRecord(DataSourceRecord dataSourceRecord)
        {
            if (!dataSourceRecord.PhysicalRecord.Body.Collection.GetElementsByTag(DataSourceRecord.DataSourceTypeIDTag).Any())
                m_missingTags.Add(new MissingTag(RecordType.DataSource, "DataSourceTypeTag", DataSourceRecord.DataSourceTypeIDTag));

            if (!dataSourceRecord.PhysicalRecord.Body.Collection.GetElementsByTag(DataSourceRecord.DataSourceNameTag).Any())
                m_missingTags.Add(new MissingTag(RecordType.DataSource, "DataSourceNameTag", DataSourceRecord.DataSourceNameTag));

            if (!dataSourceRecord.PhysicalRecord.Body.Collection.GetElementsByTag(DataSourceRecord.EffectiveTag).Any())
                m_missingTags.Add(new MissingTag(RecordType.DataSource, "EffectiveTag", DataSourceRecord.EffectiveTag));

            if (!dataSourceRecord.PhysicalRecord.Body.Collection.GetElementsByTag(DataSourceRecord.ChannelDefinitionsTag).Any())
                m_missingTags.Add(new MissingTag(RecordType.DataSource, "ChannelDefinitionsTag", DataSourceRecord.ChannelDefinitionsTag));

            if (dataSourceRecord.ChannelDefinitions.Count == 0)
                m_missingTags.Add(new MissingTag(RecordType.DataSource, "OneChannelDefinitionTag", DataSourceRecord.OneChannelDefinitionTag));

            foreach (ChannelDefinition channelDefinition in dataSourceRecord.ChannelDefinitions)
            {
                if (!channelDefinition.PhysicalStructure.GetElementsByTag(ChannelDefinition.PhaseIDTag).Any())
                    m_missingTags.Add(new MissingTag(RecordType.DataSource, "PhaseIDTag", ChannelDefinition.PhaseIDTag));

                if (!channelDefinition.PhysicalStructure.GetElementsByTag(ChannelDefinition.QuantityTypeIDTag).Any())
                    m_missingTags.Add(new MissingTag(RecordType.DataSource, "QuantityTypeIDTag", ChannelDefinition.QuantityTypeIDTag));

                if (!channelDefinition.PhysicalStructure.GetElementsByTag(ChannelDefinition.QuantityMeasuredIDTag).Any())
                    m_missingTags.Add(new MissingTag(RecordType.DataSource, "QuantityMeasuredIDTag", ChannelDefinition.QuantityMeasuredIDTag));

                if (!channelDefinition.PhysicalStructure.GetElementsByTag(ChannelDefinition.SeriesDefinitionsTag).Any())
                    m_missingTags.Add(new MissingTag(RecordType.DataSource, "SeriesDefinitionsTag", ChannelDefinition.SeriesDefinitionsTag));

                if (channelDefinition.SeriesDefinitions.Count == 0)
                    m_missingTags.Add(new MissingTag(RecordType.DataSource, "OneSeriesDefinitionTag", ChannelDefinition.OneSeriesDefinitionTag));

                foreach (SeriesDefinition seriesDefinition in channelDefinition.SeriesDefinitions)
                {
                    if (!seriesDefinition.PhysicalStructure.GetElementsByTag(SeriesDefinition.ValueTypeIDTag).Any())
                        m_missingTags.Add(new MissingTag(RecordType.DataSource, "ValueTypeIDTag", SeriesDefinition.ValueTypeIDTag));

                    if (!seriesDefinition.PhysicalStructure.GetElementsByTag(SeriesDefinition.QuantityUnitsIDTag).Any())
                        m_missingTags.Add(new MissingTag(RecordType.DataSource, "QuantityUnitsIDTag", SeriesDefinition.QuantityUnitsIDTag));

                    if (!seriesDefinition.PhysicalStructure.GetElementsByTag(SeriesDefinition.QuantityCharacteristicIDTag).Any())
                        m_missingTags.Add(new MissingTag(RecordType.DataSource, "QuantityCharacteristicIDTag", SeriesDefinition.QuantityCharacteristicIDTag));

                    if (!seriesDefinition.PhysicalStructure.GetElementsByTag(SeriesDefinition.StorageMethodIDTag).Any())
                        m_missingTags.Add(new MissingTag(RecordType.DataSource, "StorageMethodIDTag", SeriesDefinition.StorageMethodIDTag));
                }
            }
        }

        private void ValidateMonitorSettingsRecord(MonitorSettingsRecord monitorSettingsRecord)
        {
            if (!monitorSettingsRecord.PhysicalRecord.Body.Collection.GetElementsByTag(MonitorSettingsRecord.EffectiveTag).Any())
                m_missingTags.Add(new MissingTag(RecordType.MonitorSettings, "EffectiveTag", MonitorSettingsRecord.EffectiveTag));

            if (!monitorSettingsRecord.PhysicalRecord.Body.Collection.GetElementsByTag(MonitorSettingsRecord.TimeInstalledTag).Any())
                m_missingTags.Add(new MissingTag(RecordType.MonitorSettings, "TimeInstalledTag", MonitorSettingsRecord.TimeInstalledTag));

            if (!monitorSettingsRecord.PhysicalRecord.Body.Collection.GetElementsByTag(MonitorSettingsRecord.UseCalibrationTag).Any())
                m_missingTags.Add(new MissingTag(RecordType.MonitorSettings, "UseCalibrationTag", MonitorSettingsRecord.UseCalibrationTag));

            if (!monitorSettingsRecord.PhysicalRecord.Body.Collection.GetElementsByTag(MonitorSettingsRecord.UseTransducerTag).Any())
                m_missingTags.Add(new MissingTag(RecordType.MonitorSettings, "UseTransducerTag", MonitorSettingsRecord.UseTransducerTag));

            if (!monitorSettingsRecord.PhysicalRecord.Body.Collection.GetElementsByTag(MonitorSettingsRecord.ChannelSettingsArrayTag).Any())
                m_missingTags.Add(new MissingTag(RecordType.MonitorSettings, "ChannelSettingsArrayTag", MonitorSettingsRecord.ChannelSettingsArrayTag));

            if (monitorSettingsRecord.ChannelSettings.Count == 0)
                m_missingTags.Add(new MissingTag(RecordType.MonitorSettings, "OneChannelSettingTag", MonitorSettingsRecord.OneChannelSettingTag));

            foreach (ChannelSetting channelSetting in monitorSettingsRecord.ChannelSettings)
            {
                if (!channelSetting.PhysicalStructure.GetElementsByTag(ChannelDefinition.ChannelDefinitionIndexTag).Any())
                    m_missingTags.Add(new MissingTag(RecordType.MonitorSettings, "ChannelDefinitionIndexTag", ChannelDefinition.ChannelDefinitionIndexTag));
            }
        }

        private void ValidateObservationRecord(ObservationRecord observationRecord)
        {
            if (!observationRecord.PhysicalRecord.Body.Collection.GetElementsByTag(ObservationRecord.ObservationNameTag).Any())
                m_missingTags.Add(new MissingTag(RecordType.Observation, "ObservationNameTag", ObservationRecord.ObservationNameTag));

            if (!observationRecord.PhysicalRecord.Body.Collection.GetElementsByTag(ObservationRecord.TimeCreateTag).Any())
                m_missingTags.Add(new MissingTag(RecordType.Observation, "TimeCreateTag", ObservationRecord.TimeCreateTag));

            if (!observationRecord.PhysicalRecord.Body.Collection.GetElementsByTag(ObservationRecord.TimeStartTag).Any())
                m_missingTags.Add(new MissingTag(RecordType.Observation, "TimeStartTag", ObservationRecord.TimeStartTag));

            if (!observationRecord.PhysicalRecord.Body.Collection.GetElementsByTag(ObservationRecord.TriggerMethodTag).Any())
                m_missingTags.Add(new MissingTag(RecordType.Observation, "TriggerMethodTag", ObservationRecord.TriggerMethodTag));

            if (!observationRecord.PhysicalRecord.Body.Collection.GetElementsByTag(ObservationRecord.ChannelInstancesTag).Any())
                m_missingTags.Add(new MissingTag(RecordType.Observation, "ChannelInstancesTag", ObservationRecord.ChannelInstancesTag));

            if (observationRecord.ChannelInstances.Count == 0)
                m_missingTags.Add(new MissingTag(RecordType.Observation, "OneChannelInstanceTag", ObservationRecord.OneChannelInstanceTag));

            foreach (ChannelInstance channelInstance in observationRecord.ChannelInstances)
            {
                if (!channelInstance.PhysicalStructure.GetElementsByTag(ChannelDefinition.ChannelDefinitionIndexTag).Any())
                    m_missingTags.Add(new MissingTag(RecordType.Observation, "ChannelDefinitionIndexTag", ChannelDefinition.ChannelDefinitionIndexTag));

                if (!channelInstance.PhysicalStructure.GetElementsByTag(ChannelInstance.SeriesInstancesTag).Any())
                    m_missingTags.Add(new MissingTag(RecordType.Observation, "SeriesInstancesTag", ChannelInstance.SeriesInstancesTag));

                if (channelInstance.SeriesInstances.Count == 0)
                    m_missingTags.Add(new MissingTag(RecordType.Observation, "OneSeriesInstanceTag", ChannelInstance.OneSeriesInstanceTag));

                foreach (SeriesInstance seriesInstance in channelInstance.SeriesInstances)
                {
                    if (!seriesInstance.PhysicalStructure.GetElementsByTag(SeriesInstance.SeriesValuesTag).Any())
                        m_missingTags.Add(new MissingTag(RecordType.Observation, "SeriesValuesTag", SeriesInstance.SeriesValuesTag));
                }
            }
        }

        #endregion
    }
}
