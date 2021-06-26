//******************************************************************************************************
//  FacileActionAdapterBase.cs - Gbtc
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
//  09/02/2010 - J. Ritchie Carroll
//       Generated original version of source code.
//  12/02/2010 - J. Ritchie Carroll
//       Added an immediate measurement tracking option for incoming data.
//  12/20/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using GSF.Diagnostics;

namespace GSF.TimeSeries.Adapters
{
    /// <summary>
    /// Represents the base class for simple, non-time-aligned, action adapters.
    /// </summary>
    /// <remarks>
    /// This base class acts on incoming measurements, in a non-time-aligned fashion, for general processing. If derived
    /// class needs time-aligned data for processing, the <see cref="ActionAdapterBase"/> class should be used instead.
    /// Derived classes are expected call <see cref="OnNewMeasurements"/> for any new measurements that may get created.
    /// </remarks>
    public abstract class FacileActionAdapterBase : AdapterBase, IActionAdapter
    {
        #region [ Members ]

        // Constants

        /// <summary>
        /// Default value for the <see cref="RespectInputDemands"/> property.
        /// </summary>
        public const bool DefaultRespectInputDemands = false;

        /// <summary>
        /// Default value for the <see cref="RespectOutputDemands"/> property.
        /// </summary>
        public const bool DefaultRespectOutputDemands = true;

        /// <summary>
        /// Default value for the <see cref="FramesPerSecond"/> property.
        /// </summary>
        public const int DefaultFramesPerSecond = 0;

        /// <summary>
        /// Default value for the <see cref="LagTime"/> property.
        /// </summary>
        public const double DefaultLagTime = 10.0D;

        /// <summary>
        /// Default value for the <see cref="LeadTime"/> property.
        /// </summary>
        public const double DefaultLeadTime = 5.0D;

        /// <summary>
        /// Default value for the <see cref="TrackLatestMeasurements"/> property.
        /// </summary>
        public const bool DefaultTrackLatestMeasurements = false;

        /// <summary>
        /// Default value for the <see cref="UseLocalClockAsRealTime"/> property.
        /// </summary>
        public const bool DefaultUseLocalClockAsRealTime = true;

        /// <summary>
        /// Default value for the <see cref="FallBackOnLocalClock"/> property.
        /// </summary>
        public const bool DefaultFallBackOnLocalClock = false;

        // Events

        /// <summary>
        /// Provides new measurements from action adapter.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T}.Argument"/> is a collection of new measurements for host to process.
        /// </remarks>
        public event EventHandler<EventArgs<ICollection<IMeasurement>>> NewMeasurements;

        /// <summary>
        /// This event is raised by derived class, if needed, to track current number of unpublished seconds of data in the queue.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T}.Argument"/> is the total number of unpublished seconds of data.
        /// </remarks>
        public event EventHandler<EventArgs<int>> UnpublishedSamples;

        /// <summary>
        /// This event is raised if there are any measurements being discarded during the sorting process.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T}.Argument"/> is the enumeration of <see cref="IMeasurement"/> values that are being discarded during the sorting process.
        /// </remarks>
        public event EventHandler<EventArgs<IEnumerable<IMeasurement>>> DiscardingMeasurements;

        // Fields
        private List<string> m_inputSourceIDs;
        private List<string> m_outputSourceIDs;
        private readonly ImmediateMeasurements m_latestMeasurements; // Absolute latest received measurement values
        private long m_realTimeTicks;                                // Timestamp of real-time or the most recently received measurement

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new instance of the <see cref="FacileActionAdapterBase"/> class.
        /// </summary>
        protected FacileActionAdapterBase()
        {
            m_latestMeasurements = new ImmediateMeasurements { RealTimeFunction = () => RealTime };
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets primary keys of input measurements the <see cref="FacileActionAdapterBase"/> expects, if any.
        /// </summary>
        [ConnectionStringParameter]
        [DefaultValue(null)]
        [Description("Defines primary keys of input measurements the adapter expects; can be one of a filter expression, measurement key, point tag or Guid.")]
        [CustomConfigurationEditor("GSF.TimeSeries.UI.WPF.dll", "GSF.TimeSeries.UI.Editors.MeasurementEditor")]
        public override MeasurementKey[] InputMeasurementKeys
        {
            get => base.InputMeasurementKeys;
            set
            {
                base.InputMeasurementKeys = value;

                // Clear measurement cache when updating input measurement keys
                if (TrackLatestMeasurements)
                    LatestMeasurements.ClearMeasurementCache();
            }
        }

        /// <summary>
        /// Gets or sets <see cref="MeasurementKey.Source"/> values used to filter input measurement keys.
        /// </summary>
        /// <remarks>
        /// This allows an adapter to associate itself with entire collections of measurements based on the source of the measurement keys.
        /// Set to <c>null</c> apply no filter.
        /// </remarks>
        public virtual string[] InputSourceIDs
        {
            get => m_inputSourceIDs?.ToArray();
            set
            {
                if (value is null)
                {
                    m_inputSourceIDs = null;
                }
                else
                {
                    m_inputSourceIDs = new List<string>(value);
                    m_inputSourceIDs.Sort();
                }

                // Filter measurements to list of specified source IDs
                LoadInputSourceIDs(this);
            }
        }

        /// <summary>
        /// Gets or sets <see cref="MeasurementKey.Source"/> values used to filter output measurements.
        /// </summary>
        /// <remarks>
        /// This allows an adapter to associate itself with entire collections of measurements based on the source of the measurement keys.
        /// Set to <c>null</c> apply no filter.
        /// </remarks>
        public virtual string[] OutputSourceIDs
        {
            get => m_outputSourceIDs?.ToArray();
            set
            {
                if (value is null)
                {
                    m_outputSourceIDs = null;
                }
                else
                {
                    m_outputSourceIDs = new List<string>(value);
                    m_outputSourceIDs.Sort();
                }

                // Filter measurements to list of specified source IDs
                LoadOutputSourceIDs(this);
            }
        }

        /// <summary>
        /// Gets or sets input measurement keys that are requested by other adapters based on what adapter says it can provide.
        /// </summary>
        public virtual MeasurementKey[] RequestedInputMeasurementKeys { get; set; }

        /// <summary>
        /// Gets or sets output measurement keys that are requested by other adapters based on what adapter says it can provide.
        /// </summary>
        public virtual MeasurementKey[] RequestedOutputMeasurementKeys { get; set; }

        /// <summary>
        /// Gets or sets flag indicating if action adapter should respect auto-start requests based on input demands.
        /// </summary>
        /// <remarks>
        /// Action adapters are in the curious position of being able to both consume and produce points, as such the user needs to be able to control how their
        /// adapter will behave concerning routing demands when the adapter is setup to connect on demand. In the case of respecting auto-start input demands,
        /// as an example, this would be <c>false</c> for an action adapter that calculated measurement, but <c>true</c> for an action adapter used to archive inputs.
        /// </remarks>
        public virtual bool RespectInputDemands { get; set; } = DefaultRespectInputDemands;

        /// <summary>
        /// Gets or sets flag indicating if action adapter should respect auto-start requests based on output demands.
        /// </summary>
        /// <remarks>
        /// Action adapters are in the curious position of being able to both consume and produce points, as such the user needs to be able to control how their
        /// adapter will behave concerning routing demands when the adapter is setup to connect on demand. In the case of respecting auto-start output demands,
        /// as an example, this would be <c>true</c> for an action adapter that calculated measurement, but <c>false</c> for an action adapter used to archive inputs.
        /// </remarks>
        public virtual bool RespectOutputDemands { get; set; } = DefaultRespectOutputDemands;

        /// <summary>
        /// Gets or sets the frames per second to be used by the <see cref="FacileActionAdapterBase"/>.
        /// </summary>
        /// <remarks>
        /// This value is only tracked in the <see cref="FacileActionAdapterBase"/>, derived class will determine its use.
        /// </remarks>
        [ConnectionStringParameter]
        [DefaultValue(DefaultFramesPerSecond)]
        [Description("Defines the number of frames per second expected by the adapter.")]
        public virtual int FramesPerSecond { get; set; } = DefaultFramesPerSecond;

        /// <summary>
        /// Gets or sets the allowed past time deviation tolerance, in seconds (can be sub-second).
        /// </summary>
        /// <remarks>
        /// <para>Defines the time sensitivity to past measurement timestamps.</para>
        /// <para>The number of seconds allowed before assuming a measurement timestamp is too old.</para>
        /// <para>This becomes the amount of delay introduced by the concentrator to allow time for data to flow into the system.</para>
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException">LagTime must be greater than zero, but it can be less than one.</exception>
        [ConnectionStringParameter]
        [DefaultValue(DefaultLagTime)]
        [Description("Defines the allowed past time deviation tolerance, in seconds (can be sub-second).")]
        public double LagTime
        {
            get => LatestMeasurements.LagTime;
            set => LatestMeasurements.LagTime = value;
        }

        /// <summary>
        /// Gets or sets the allowed future time deviation tolerance, in seconds (can be sub-second).
        /// </summary>
        /// <remarks>
        /// <para>Defines the time sensitivity to future measurement timestamps.</para>
        /// <para>The number of seconds allowed before assuming a measurement timestamp is too advanced.</para>
        /// <para>This becomes the tolerated +/- accuracy of the local clock to real-time.</para>
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException">LeadTime must be greater than zero, but it can be less than one.</exception>
        [ConnectionStringParameter]
        [DefaultValue(DefaultLeadTime)]
        [Description("Defines the allowed future time deviation tolerance, in seconds (can be sub-second).")]
        public double LeadTime
        {
            get => LatestMeasurements.LeadTime;
            set => LatestMeasurements.LeadTime = value;
        }

        /// <summary>
        /// Gets or sets flag to start tracking the absolute latest received measurement values.
        /// </summary>
        /// <remarks>
        /// Latest received measurement value will be available via the <see cref="LatestMeasurements"/> property.
        /// </remarks>
        public virtual bool TrackLatestMeasurements { get; set; } = DefaultTrackLatestMeasurements;

        /// <summary>
        /// Gets reference to the collection of absolute latest received measurement values.
        /// </summary>
        public virtual ImmediateMeasurements LatestMeasurements => m_latestMeasurements;

        /// <summary>
        /// Gets or sets flag that determines whether or not to use the local clock time as real time.
        /// </summary>
        /// <remarks>
        /// Use your local system clock as real time only if the time is locally GPS-synchronized,
        /// or if the measurement values being sorted were not measured relative to a GPS-synchronized clock.
        /// Turn this off if the class is intended to process historical data.
        /// </remarks>
        [ConnectionStringParameter]
        [DefaultValue(DefaultUseLocalClockAsRealTime)]
        [Description("Defines flag that determines whether or not to use the local clock time as real time.")]
        public virtual bool UseLocalClockAsRealTime { get; set; } = DefaultUseLocalClockAsRealTime;

        /// <summary>
        /// Gest or sets flag that determines whether to fall back on local clock time as real time when time is unreasonable.
        /// </summary>
        /// <remarks>
        /// This property is only applicable when <see cref="UseLocalClockAsRealTime"/> is <c>false</c>.
        /// </remarks>
        [ConnectionStringParameter]
        [DefaultValue(DefaultFallBackOnLocalClock)]
        [Description("Defines flag that determines whether to fall back on local clock time as real time when time is unreasonable. Only applicable when UseLocalClockAsRealTime is false.")]
        public virtual bool FallBackOnLocalClock { get; set; } = DefaultFallBackOnLocalClock;

        /// <summary>
        /// Gets the most accurate time value that is available. If <see cref="UseLocalClockAsRealTime"/> = <c>true</c>, then
        /// this function will return <see cref="DateTime.UtcNow"/>. Otherwise, this function will return the timestamp of the
        /// most recent measurement.
        /// </summary>
        public Ticks RealTime
        {
            get
            {
                // When using local clock as real-time, assume this is the best value we have for real time.
                if (UseLocalClockAsRealTime || !TrackLatestMeasurements)
                    return DateTime.UtcNow.Ticks;

                // Assume latest measurement timestamp is the best value we have for real-time.
                return m_realTimeTicks;
            }
        }

        /// <summary>
        /// Returns the detailed status of the data input source.
        /// </summary>
        /// <remarks>
        /// Derived classes should extend status with implementation specific information.
        /// </remarks>
        public override string Status
        {
            get
            {
                StringBuilder status = new StringBuilder();

                status.Append(base.Status);
                status.AppendLine($"        Defined frame rate: {FramesPerSecond} frames/sec");
                status.AppendLine($"      Measurement tracking: {(TrackLatestMeasurements ? "Enabled" : "Disabled")}");
                status.AppendLine($"  Respecting input demands: {RespectInputDemands}");
                status.AppendLine($" Respecting output demands: {RespectOutputDemands}");
                status.AppendLine($"  Local clock is real time: {UseLocalClockAsRealTime}");
                status.AppendLine($"  Fall back on local clock: {(UseLocalClockAsRealTime ? "N/A" : $"{FallBackOnLocalClock}")}");
                status.AppendLine($"   Current real time value: {RealTime:yyyy-MM-dd HH:mm:ss.fff}");

                return status.ToString();
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Initializes <see cref="FacileActionAdapterBase"/>.
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();

            Dictionary<string, string> settings = Settings;

            if (settings.TryGetValue(nameof(FramesPerSecond), out string setting))
                FramesPerSecond = int.Parse(setting);

            if (settings.TryGetValue(nameof(UseLocalClockAsRealTime), out setting))
                UseLocalClockAsRealTime = setting.ParseBoolean();

            if (settings.TryGetValue(nameof(FallBackOnLocalClock), out setting))
                FallBackOnLocalClock = setting.ParseBoolean();

            if (settings.TryGetValue(nameof(TrackLatestMeasurements), out setting))
                TrackLatestMeasurements = setting.ParseBoolean();

            if (TrackLatestMeasurements)
            {
                LatestMeasurements.LagTime = settings.TryGetValue(nameof(LagTime), out setting) ? double.Parse(setting) : DefaultLagTime;
                LatestMeasurements.LeadTime = settings.TryGetValue(nameof(LeadTime), out setting) ? double.Parse(setting) : DefaultLeadTime;
            }

            if (settings.TryGetValue(nameof(RespectInputDemands), out setting))
                RespectInputDemands = setting.ParseBoolean();

            if (settings.TryGetValue(nameof(RespectOutputDemands), out setting))
                RespectOutputDemands = setting.ParseBoolean();

            if (settings.TryGetValue(nameof(InputSourceIDs), out setting))
                InputSourceIDs = setting.Split(',');

            if (settings.TryGetValue(nameof(OutputSourceIDs), out setting))
                OutputSourceIDs = setting.Split(',');
        }

        /// <summary>
        /// Queues a single measurement for processing.
        /// </summary>
        /// <param name="measurement">Measurement to queue for processing.</param>
        public virtual void QueueMeasurementForProcessing(IMeasurement measurement) => 
            QueueMeasurementsForProcessing(new[] { measurement });

        /// <summary>
        /// Queues a collection of measurements for processing.
        /// </summary>
        /// <param name="measurements">Measurements to queue for processing.</param>
        public virtual void QueueMeasurementsForProcessing(IEnumerable<IMeasurement> measurements)
        {
            if (!TrackLatestMeasurements)
                return;

            // If enabled, facile adapter will track the absolute latest measurement values.
            bool useLocalClockAsRealTime = UseLocalClockAsRealTime;

            foreach (IMeasurement measurement in measurements)
            {
                m_latestMeasurements.UpdateMeasurementValue(measurement);

                // Track latest timestamp as real-time, if requested.
                if (!useLocalClockAsRealTime && measurement.Timestamp > m_realTimeTicks)
                {
                    if (measurement.Timestamp.UtcTimeIsValid(LagTime, LeadTime))
                        m_realTimeTicks = measurement.Timestamp;
                    else if (FallBackOnLocalClock)
                        m_realTimeTicks = DateTime.UtcNow.Ticks;
                }
            }
        }

        /// <summary>
        /// Raises the <see cref="NewMeasurements"/> event.
        /// </summary>
        protected virtual void OnNewMeasurements(ICollection<IMeasurement> measurements)
        {
            try
            {
                NewMeasurements?.Invoke(this, new EventArgs<ICollection<IMeasurement>>(measurements));
                IncrementProcessedMeasurements(measurements.Count);
            }
            catch (Exception ex)
            {
                // We protect our code from consumer thrown exceptions
                OnProcessException(MessageLevel.Info, new InvalidOperationException($"Exception in consumer handler for NewMeasurements event: {ex.Message}", ex), "ConsumerEventException");
            }
        }

        /// <summary>
        /// Raises the <see cref="UnpublishedSamples"/> event.
        /// </summary>
        /// <param name="seconds">Total number of unpublished seconds of data.</param>
        protected virtual void OnUnpublishedSamples(int seconds)
        {
            try
            {
                UnpublishedSamples?.Invoke(this, new EventArgs<int>(seconds));
            }
            catch (Exception ex)
            {
                // We protect our code from consumer thrown exceptions
                OnProcessException(MessageLevel.Info, new InvalidOperationException($"Exception in consumer handler for UnpublishedSamples event: {ex.Message}", ex), "ConsumerEventException");
            }
        }

        /// <summary>
        /// Raises the <see cref="DiscardingMeasurements"/> event.
        /// </summary>
        /// <param name="measurements">Enumeration of <see cref="IMeasurement"/> values being discarded.</param>
        protected virtual void OnDiscardingMeasurements(IEnumerable<IMeasurement> measurements)
        {
            try
            {
                DiscardingMeasurements?.Invoke(this, new EventArgs<IEnumerable<IMeasurement>>(measurements));
            }
            catch (Exception ex)
            {
                // We protect our code from consumer thrown exceptions
                OnProcessException(MessageLevel.Info, new InvalidOperationException($"Exception in consumer handler for DiscardingMeasurements event: {ex.Message}", ex), "ConsumerEventException");
            }
        }

        #endregion
    }
}