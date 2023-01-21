//******************************************************************************************************
//  SubscriptionInfo.cs - Gbtc
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
//  04/25/2012 - Stephen C. Wills
//       Generated original version of source code.
//  12/20/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;

namespace GSF.TimeSeries.Transport
{
    /// <summary>
    /// Configuration object for data subscriptions.
    /// </summary>
    public abstract class SubscriptionInfo
    {
        #region [ Members ]

        // Fields

        private bool m_useCompactMeasurementFormat;

        private int m_dataChannelLocalPort;

        private double m_lagTime;
        private double m_leadTime;

        private int m_processingInterval;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new instance of the <see cref="SubscriptionInfo"/> class.
        /// </summary>
        protected SubscriptionInfo()
        {
            m_useCompactMeasurementFormat = true;
            m_dataChannelLocalPort = 9500;
            m_lagTime = 10.0;
            m_leadTime = 5.0;
            m_processingInterval = -1;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the filter expression used to define which
        /// measurements are being requested by the subscriber.
        /// </summary>
        public virtual string FilterExpression { get; set; }

        /// <summary>
        /// Gets or sets the flag that determines whether to use the
        /// compact measurement format or the full measurement format
        /// for transmitting measurements to the subscriber.
        /// </summary>
        public virtual bool UseCompactMeasurementFormat
        {
            get => m_useCompactMeasurementFormat;
            set => m_useCompactMeasurementFormat = value;
        }

        /// <summary>
        /// Gets or sets the flag that determines whether the subscriber
        /// is requesting its data over a separate UDP data channel.
        /// </summary>
        public virtual bool UdpDataChannel { get; set; }

        /// <summary>
        /// Gets or sets the port number that the UDP data channel binds to.
        /// This value is only used when the subscriber requests a separate
        /// UDP data channel.
        /// </summary>
        public virtual int DataChannelLocalPort
        {
            get => m_dataChannelLocalPort;
            set => m_dataChannelLocalPort = value;
        }

        /// <summary>
        /// Gets or sets the allowed past time deviation
        /// tolerance in seconds (can be sub-second).
        /// </summary>
        public virtual double LagTime
        {
            get => m_lagTime;
            set => m_lagTime = value;
        }

        /// <summary>
        /// Gets or sets the allowed future time deviation
        /// tolerance, in seconds (can be sub-second).
        /// </summary>
        public virtual double LeadTime
        {
            get => m_leadTime;
            set => m_leadTime = value;
        }

        /// <summary>
        /// Gets or sets the flag that determines whether the server's
        /// local clock is used as real-time. If false, the timestamps
        /// of the measurements will be used as real-time.
        /// </summary>
        public virtual bool UseLocalClockAsRealTime { get; set; }

        /// <summary>
        /// Gets or sets the flag that determines whether measurement timestamps use
        /// millisecond resolution. If false, they will use <see cref="Ticks"/> resolution.
        /// </summary>
        /// <remarks>
        /// This flag determines the size of the timestamps transmitted as part of
        /// the compact measurement format when the server is using base time offsets.
        /// </remarks>
        public virtual bool UseMillisecondResolution { get; set; }

        /// <summary>
        /// Gets or sets the flag that determines whether to request that measurements
        /// sent to the subscriber should be filtered by the publisher prior to sending them.
        /// </summary>
        public virtual bool RequestNaNValueFilter { get; set; }

        /// <summary>
        /// Gets or sets the start time of the requested
        /// temporal session for streaming historic data.
        /// </summary>
        /// <remarks>
        /// <para>
        /// When the <see cref="StartTime"/> or <see cref="StopTime"/> temporal processing constraints are defined (i.e., not <c>null</c>), this
        /// specifies the start and stop time over which the subscriber session will process data. Passing in <c>null</c> for the <see cref="StartTime"/>
        /// and <see cref="StopTime"/> specifies the subscriber session will process data in standard, i.e., real-time, operation.
        /// </para>
        /// 
        /// <para>
        /// Both the <see cref="StartTime"/> and <see cref="StopTime"/> parameters can be specified in one of the
        /// following formats:
        /// <list type="table">
        ///     <listheader>
        ///         <term>Time Format</term>
        ///         <description>Format Description</description>
        ///     </listheader>
        ///     <item>
        ///         <term>12-30-2000 23:59:59.033</term>
        ///         <description>Absolute date and time.</description>
        ///     </item>
        ///     <item>
        ///         <term>*</term>
        ///         <description>Evaluates to <see cref="DateTime.UtcNow"/>.</description>
        ///     </item>
        ///     <item>
        ///         <term>*-20s</term>
        ///         <description>Evaluates to 20 seconds before <see cref="DateTime.UtcNow"/>.</description>
        ///     </item>
        ///     <item>
        ///         <term>*-10m</term>
        ///         <description>Evaluates to 10 minutes before <see cref="DateTime.UtcNow"/>.</description>
        ///     </item>
        ///     <item>
        ///         <term>*-1h</term>
        ///         <description>Evaluates to 1 hour before <see cref="DateTime.UtcNow"/>.</description>
        ///     </item>
        ///     <item>
        ///         <term>*-1d</term>
        ///         <description>Evaluates to 1 day before <see cref="DateTime.UtcNow"/>.</description>
        ///     </item>
        /// </list>
        /// </para>
        /// </remarks>
        public virtual string StartTime { get; set; }

        /// <summary>
        /// Gets or sets the stop time of the requested
        /// temporal session for streaming historic data.
        /// </summary>
        /// <remarks>
        /// <para>
        /// When the <see cref="StartTime"/> or <see cref="StopTime"/> temporal processing constraints are defined (i.e., not <c>null</c>), this
        /// specifies the start and stop time over which the subscriber session will process data. Passing in <c>null</c> for the <see cref="StartTime"/>
        /// and <see cref="StopTime"/> specifies the subscriber session will process data in standard, i.e., real-time, operation.
        /// </para>
        /// 
        /// <para>
        /// Both the <see cref="StartTime"/> and <see cref="StopTime"/> parameters can be specified in one of the
        /// following formats:
        /// <list type="table">
        ///     <listheader>
        ///         <term>Time Format</term>
        ///         <description>Format Description</description>
        ///     </listheader>
        ///     <item>
        ///         <term>12-30-2000 23:59:59.033</term>
        ///         <description>Absolute date and time.</description>
        ///     </item>
        ///     <item>
        ///         <term>*</term>
        ///         <description>Evaluates to <see cref="DateTime.UtcNow"/>.</description>
        ///     </item>
        ///     <item>
        ///         <term>*-20s</term>
        ///         <description>Evaluates to 20 seconds before <see cref="DateTime.UtcNow"/>.</description>
        ///     </item>
        ///     <item>
        ///         <term>*-10m</term>
        ///         <description>Evaluates to 10 minutes before <see cref="DateTime.UtcNow"/>.</description>
        ///     </item>
        ///     <item>
        ///         <term>*-1h</term>
        ///         <description>Evaluates to 1 hour before <see cref="DateTime.UtcNow"/>.</description>
        ///     </item>
        ///     <item>
        ///         <term>*-1d</term>
        ///         <description>Evaluates to 1 day before <see cref="DateTime.UtcNow"/>.</description>
        ///     </item>
        /// </list>
        /// </para>
        /// </remarks>
        public virtual string StopTime { get; set; }

        /// <summary>
        /// Gets or sets the additional constraint parameters
        /// supplied to temporal adapters in a temporal session.
        /// </summary>
        public virtual string ConstraintParameters { get; set; }

        /// <summary>
        /// Gets or sets the processing interval requested by the subscriber.
        /// A value of <c>-1</c> indicates the default processing interval.
        /// A value of <c>0</c> indicates data will be processed as fast as
        /// possible.
        /// </summary>
        /// <remarks>
        /// With the exception of the values of -1 and 0, the <see cref="ProcessingInterval"/> value specifies the desired historical playback data
        /// processing interval in milliseconds. This is basically a delay, or timer interval, over which to process data. Setting this value to -1 means
        /// to use the default processing interval while setting the value to 0 means to process data as fast as possible.
        /// </remarks>
        public virtual int ProcessingInterval
        {
            get => m_processingInterval;
            set => m_processingInterval = value;
        }

        /// <summary>
        /// Gets or sets the additional connection string parameters to
        /// be applied to the connection string sent to the publisher
        /// during subscription.
        /// </summary>
        public virtual string ExtraConnectionStringParameters { get; set; }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Creates a shallow copy of this
        /// <see cref="SubscriptionInfo"/> object.
        /// </summary>
        /// <returns>The copy of this object.</returns>
        public virtual SubscriptionInfo Copy()
        {
            return (SubscriptionInfo)MemberwiseClone();
        }

        #endregion
    }

    /// <summary>
    /// Configuration object for synchronized data subscriptions.
    /// </summary>
    public sealed class SynchronizedSubscriptionInfo : SubscriptionInfo
    {
        #region [ Members ]

        // Fields

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new instance of the <see cref="SynchronizedSubscriptionInfo"/> class.
        /// </summary>
        /// <param name="remotelySynchronized">Flag that determines whether the subscription defined by this object is remotely synchronized or locally synchronized.</param>
        /// <param name="framesPerSecond">Frame rate of the subscription in frames per second.</param>
        public SynchronizedSubscriptionInfo(bool remotelySynchronized, int framesPerSecond)
        {
            RemotelySynchronized = remotelySynchronized;

            FramesPerSecond = framesPerSecond;
            DownsamplingMethod = DownsamplingMethod.LastReceived;
            AllowPreemptivePublishing = true;

            AllowSortsByArrival = true;
            TimeResolution = Ticks.PerMillisecond;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the flag that determines whether this subscription
        /// is remotely synchronized or locally synchronized.
        /// </summary>
        public bool RemotelySynchronized { get; set; }

        /// <summary>
        /// Gets or sets the frame rate of the subscription in frames per second.
        /// </summary>
        public int FramesPerSecond { get; set; }

        /// <summary>
        /// Gets or sets the down-sampling method used when the frame rate of
        /// the subscription is lower than the frame rate of the measurement's
        /// source.
        /// </summary>
        public DownsamplingMethod DownsamplingMethod { get; set; }

        /// <summary>
        /// Gets or sets the flag that determines whether frames should be published
        /// as soon as the data is available. If false, frames will be published
        /// when their timestamp expires <c>(realTime > timestamp + lagTime)</c>.
        /// </summary>
        public bool AllowPreemptivePublishing { get; set; }

        /// <summary>
        /// Gets or sets the flag that determines whether to allow measurement
        /// sorting based on the measurement's time of arrival, if its timestamp
        /// is unreasonable.
        /// </summary>
        public bool AllowSortsByArrival { get; set; }

        /// <summary>
        /// Gets or sets the flag that determines whether to
        /// ignore bad timestamps when sorting measurements.
        /// </summary>
        public bool IgnoreBadTimestamps { get; set; }

        /// <summary>
        /// Gets or sets the maximum time resolution, in ticks, to use when sorting measurements by timestamps into their proper destination frame.
        /// </summary>
        /// <remarks>
        /// <list type="table">
        ///     <listheader>
        ///         <term>Desired maximum resolution</term>
        ///         <description>Value to assign</description>
        ///     </listheader>
        ///     <item>
        ///         <term>Seconds</term>
        ///         <description><see cref="Ticks"/>.<see cref="Ticks.PerSecond"/></description>
        ///     </item>
        ///     <item>
        ///         <term>Milliseconds</term>
        ///         <description><see cref="Ticks"/>.<see cref="Ticks.PerMillisecond"/></description>
        ///     </item>
        ///     <item>
        ///         <term>Microseconds</term>
        ///         <description><see cref="Ticks"/>.<see cref="Ticks.PerMicrosecond"/></description>
        ///     </item>
        ///     <item>
        ///         <term>100-Nanoseconds</term>
        ///         <description>0</description>
        ///     </item>
        /// </list>
        /// Assigning values less than zero will be set to zero since minimum possible concentrator resolution is one tick (100-nanoseconds). Assigning
        /// values values greater than <see cref="Ticks"/>.<see cref="Ticks.PerSecond"/> will be set to <see cref="Ticks"/>.<see cref="Ticks.PerSecond"/>
        /// since maximum possible concentrator resolution is one second (i.e., 1 frame per second).
        /// </remarks>
        public long TimeResolution { get; set; }

        #endregion
    }

    /// <summary>
    /// Configuration object for unsynchronized data subscriptions.
    /// </summary>
    public sealed class UnsynchronizedSubscriptionInfo : SubscriptionInfo
    {
        #region [ Members ]

        // Fields

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new instance of the <see cref="UnsynchronizedSubscriptionInfo"/> class.
        /// </summary>
        /// <param name="throttled">The flag that determines whether to request that the subscription be throttled.</param>
        public UnsynchronizedSubscriptionInfo(bool throttled)
        {
            Throttled = throttled;
            PublishInterval = -1;
            IncludeTime = true;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the flag that determines whether
        /// to request that the subscription be throttled.
        /// </summary>
        public bool Throttled { get; set; }

        /// <summary>
        /// Gets or sets the interval at which data should be
        /// published when using a throttled subscription.
        /// </summary>
        public double PublishInterval { get; set; }

        /// <summary>
        /// Gets or sets the flag that determines whether timestamps are
        /// included in the data sent from the publisher. This value is
        /// ignored if the data is remotely synchronized.
        /// </summary>
        public bool IncludeTime { get; set; }

        #endregion
    }
}
