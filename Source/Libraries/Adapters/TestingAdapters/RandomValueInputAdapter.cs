//******************************************************************************************************
//  RandomValueInputAdapter.cs - Gbtc
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
//  09/16/2009 - James R. Carroll
//       Generated original version of source code.
//  09/25/2009 - Pinal C. Patel
//       Modified PublishRandomPoints() to use PrecisionTimer instead of DateTime for higher resolution.
//  12/13/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading;
using GSF;
using GSF.TimeSeries;
using GSF.TimeSeries.Adapters;

namespace TestingAdapters
{
    /// <summary>
    /// Represents a class used to stream in random values for input measurements.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Example connection string using manually defined measurements:<br/>
    /// <c>outputMeasurements={P3:1345,60.0,1.0;P3:1346;P3:1347}</c><br/>
    /// When defined manually outputMeasurements are defined as "ArchiveSource:PointID,Adder,Multiplier",
    /// the adder and multiplier are optional defaulting to 0.0 and 1.0 respectively.
    /// <br/>
    /// </para>
    /// <para>
    /// Example connection string using measurements defined in a <see cref="AdapterBase.DataSource"/> table:<br/>
    /// <c>outputMeasurements={FILTER ActiveMeasurements WHERE SignalType IN ('IPHA','VPHA') AND Phase='+'}</c><br/>
    /// <br/>
    /// Basic filtering syntax is as follows:<br/>
    /// <br/>
    ///     {FILTER &lt;TableName&gt; WHERE &lt;Expression&gt; [ORDER BY &lt;SortField&gt;]}<br/>
    /// <br/>
    /// Source tables are expected to have at least the following fields:<br/>
    /// <list type="table">
    ///     <listheader>
    ///         <term>Name</term>
    ///         <term>Type</term>
    ///         <description>Description.</description>
    ///     </listheader>
    ///     <item>
    ///         <term>ID</term>
    ///         <term>NVARCHAR</term>
    ///         <description>Measurement key formatted as: ArchiveSource:PointID.</description>
    ///     </item>
    ///     <item>
    ///         <term>PointTag</term>
    ///         <term>NVARCHAR</term>
    ///         <description>Point tag of measurement.</description>
    ///     </item>
    ///     <item>
    ///         <term>Adder</term>
    ///         <term>FLOAT</term>
    ///         <description>Adder to apply to value, if any (default to 0.0).</description>
    ///     </item>
    ///     <item>
    ///         <term>Multiplier</term>
    ///         <term>FLOAT</term>
    ///         <description>Multipler to apply to value, if any (default to 1.0).</description>
    ///     </item>
    /// </list>
    /// </para>
    /// <para>
    /// Note that the random value produced for the points will be a number between 0 to 1, use the Adder and Multipler
    /// to narrow down the range for your point. For example, to produce random frequency values between 59.95 and 60.05
    /// you would use the following point definition:<br/>
    /// <c>outputMeasurements={LocalDevArchive:2,59.95,0.1}</c>
    /// </para>
    /// </remarks>
    [Description("Random Values: Streams random values for input measurements")]
    public class RandomValueInputAdapter : InputAdapterBase
    {
        #region [ Members ]

        /// <summary>
        /// Specifies the default value for the <see cref="DefaultPointsToSend"/> property.
        /// </summary>
        public const int DefaultPointsToSend = 5;

        /// <summary>
        /// Specifies the default value for the <see cref="InterpointDelay"/> property.
        /// </summary>
        public const int DefaultInterpointDelay = 33;

        // Fields
        private int m_pointsToSend;
        private int m_interpointDelay;
        private Thread m_publishThread;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="RandomValueInputAdapter"/>.
        /// </summary>
        public RandomValueInputAdapter()
        {
            m_pointsToSend = DefaultPointsToSend;
            m_interpointDelay = DefaultInterpointDelay;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets flag that determines if the data input connects asynchronously.
        /// </summary>
        protected override bool UseAsyncConnect
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Gets the flag indicating if this adapter supports temporal processing.
        /// </summary>
        public override bool SupportsTemporalProcessing
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Gets or sets the name of this <see cref="RandomValueInputAdapter"/>.
        /// </summary>
        public override string Name
        {
            get
            {
                return "Random point generator defined to send " + m_pointsToSend + " points...";
            }
        }

        /// <summary>
        /// Gets or sets number of test points to send.
        /// </summary>
        [ConnectionStringParameter,
        Description("Define the number of frames to send into the concentrator."),
        DefaultValue(5)]
        public int PointsToSend
        {
            get
            {
                return m_pointsToSend;
            }
            set
            {
                m_pointsToSend = value;
            }
        }

        /// <summary>
        /// Gets or sets number of milliseconds between points.
        /// </summary>
        [ConnectionStringParameter,
        Description("Define the interval of time, in milliseconds, between sending frames into the concentrator."),
        DefaultValue(33)]
        public int InterpointDelay
        {
            get
            {
                return m_interpointDelay;
            }
            set
            {
                m_interpointDelay = value;
            }
        }

        /// <summary>
        /// Returns the detailed status of the data input source.
        /// </summary>
        public override string Status
        {
            get
            {
                StringBuilder status = new StringBuilder();

                status.AppendFormat("  Number of points to send: {0}\r\n", PointsToSend);
                status.AppendFormat("         Inter-point delay: {0}ms\r\n", InterpointDelay);
                status.Append(base.Status);

                return status.ToString();
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Intializes <see cref="RandomValueInputAdapter"/>.
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();

            Dictionary<string, string> settings = Settings;
            string setting;

            if (settings.TryGetValue("pointsToSend", out setting))
                m_pointsToSend = int.Parse(setting);
            else
                m_pointsToSend = DefaultPointsToSend;

            if (settings.TryGetValue("interpointDelay", out setting))
                m_interpointDelay = int.Parse(setting);
            else
                m_interpointDelay = DefaultInterpointDelay;
        }

        /// <summary>
        /// Gets a short one-line status of this <see cref="RandomValueInputAdapter"/>.
        /// </summary>
        /// <param name="maxLength">Maximum number of available characters for display.</param>
        /// <returns>A short one-line summary of the current status of this <see cref="AdapterBase"/>.</returns>
        public override string GetShortStatus(int maxLength)
        {
            return ("Total sent measurements " + ProcessedMeasurements.ToString("N0")).CenterText(maxLength);
        }

        /// <summary>
        /// Attempts to connect to data input source.
        /// </summary>
        protected override void AttemptConnection()
        {
            m_publishThread = new Thread(PublishRandomPoints);
            m_publishThread.Start();
        }

        /// <summary>
        /// Attempts to disconnect from data input source.
        /// </summary>
        protected override void AttemptDisconnection()
        {
            if (m_publishThread != null)
                m_publishThread.Abort();

            m_publishThread = null;
        }

        private void PublishRandomPoints()
        {
            Random randomNumber = new Random();
            Ticks timestamp;

            for (int i = 0; i < m_pointsToSend; i++)
            {
                ICollection<IMeasurement> outputMeasurementClones = new List<IMeasurement>();
                timestamp = DateTime.UtcNow.Ticks;

                for (int j = 0; j < OutputMeasurements.Length; j++)
                {
                    OutputMeasurements[j].Timestamp = timestamp;
                    OutputMeasurements[j].Value = randomNumber.NextDouble();
                    outputMeasurementClones.Add(Measurement.Clone(OutputMeasurements[j]));
                }

                // Publish next set of measurements to consumer...
                this.OnNewMeasurements(outputMeasurementClones);

                // Sleep until next desired publication...
                Thread.Sleep(m_interpointDelay);
            }
        }

        #endregion
    }
}
