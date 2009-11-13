//*******************************************************************************************************
//  ConcentratorBase.cs - Gbtc
//
//  Tennessee Valley Authority, 2009
//  No copyright is claimed pursuant to 17 USC § 105.  All Other Rights Reserved.
//
//  This software is made freely available under the TVA Open Source Agreement (see below).
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  11/12/2004 - J. Ritchie Carroll
//       Generated initial version of source for Super Phasor Data Concentrator.
//  02/23/2006 - J. Ritchie Carroll
//       Abstracted classes for general use, and added to TVA code library.
//  04/23/2007 - J. Ritchie Carroll
//       Migrated concentrator to use a base class model instead of using delegates.
//  08/01/2007 - J. Ritchie Carroll
//       Completed extensive threading optimizations to ensure performance.
//  08/27/2007 - Darrell Zuercher
//       Edited code comments.
//  11/02/2007 - J. Ritchie Carroll
//       Changed code to use new FrameQueue class instead of KeyedProcessQueue to
//       allow more finite control of locking to reduce thread contention.
//  02/19/2008 - J. Ritchie Carroll
//       Added code to detect and avoid redundant calls to Dispose().
//  08/22/2008 - J. Ritchie Carroll, Jian Zuo
//       Replaced timing code using PrecisionTimer.
//       Added code to evenly distribute integer based millisecond wait times across a second.
//  09/16/2008 - J. Ritchie Carroll
//       Converted to C#.
//  08/05/2009 - Josh L. Patterson
//       Edited Comments.
//  09/14/2009 - Stephen C. Wills
//       Added new header and license agreement.
//
//*******************************************************************************************************

#region [ TVA Open Source Agreement ]
/*

 THIS OPEN SOURCE AGREEMENT ("AGREEMENT") DEFINES THE RIGHTS OF USE,REPRODUCTION, DISTRIBUTION,
 MODIFICATION AND REDISTRIBUTION OF CERTAIN COMPUTER SOFTWARE ORIGINALLY RELEASED BY THE
 TENNESSEE VALLEY AUTHORITY, A CORPORATE AGENCY AND INSTRUMENTALITY OF THE UNITED STATES GOVERNMENT
 ("GOVERNMENT AGENCY"). GOVERNMENT AGENCY IS AN INTENDED THIRD-PARTY BENEFICIARY OF ALL SUBSEQUENT
 DISTRIBUTIONS OR REDISTRIBUTIONS OF THE SUBJECT SOFTWARE. ANYONE WHO USES, REPRODUCES, DISTRIBUTES,
 MODIFIES OR REDISTRIBUTES THE SUBJECT SOFTWARE, AS DEFINED HEREIN, OR ANY PART THEREOF, IS, BY THAT
 ACTION, ACCEPTING IN FULL THE RESPONSIBILITIES AND OBLIGATIONS CONTAINED IN THIS AGREEMENT.

 Original Software Designation: openPDC
 Original Software Title: The TVA Open Source Phasor Data Concentrator
 User Registration Requested. Please Visit https://naspi.tva.com/Registration/
 Point of Contact for Original Software: J. Ritchie Carroll <mailto:jrcarrol@tva.gov>

 1. DEFINITIONS

 A. "Contributor" means Government Agency, as the developer of the Original Software, and any entity
 that makes a Modification.

 B. "Covered Patents" mean patent claims licensable by a Contributor that are necessarily infringed by
 the use or sale of its Modification alone or when combined with the Subject Software.

 C. "Display" means the showing of a copy of the Subject Software, either directly or by means of an
 image, or any other device.

 D. "Distribution" means conveyance or transfer of the Subject Software, regardless of means, to
 another.

 E. "Larger Work" means computer software that combines Subject Software, or portions thereof, with
 software separate from the Subject Software that is not governed by the terms of this Agreement.

 F. "Modification" means any alteration of, including addition to or deletion from, the substance or
 structure of either the Original Software or Subject Software, and includes derivative works, as that
 term is defined in the Copyright Statute, 17 USC § 101. However, the act of including Subject Software
 as part of a Larger Work does not in and of itself constitute a Modification.

 G. "Original Software" means the computer software first released under this Agreement by Government
 Agency entitled openPDC, including source code, object code and accompanying documentation, if any.

 H. "Recipient" means anyone who acquires the Subject Software under this Agreement, including all
 Contributors.

 I. "Redistribution" means Distribution of the Subject Software after a Modification has been made.

 J. "Reproduction" means the making of a counterpart, image or copy of the Subject Software.

 K. "Sale" means the exchange of the Subject Software for money or equivalent value.

 L. "Subject Software" means the Original Software, Modifications, or any respective parts thereof.

 M. "Use" means the application or employment of the Subject Software for any purpose.

 2. GRANT OF RIGHTS

 A. Under Non-Patent Rights: Subject to the terms and conditions of this Agreement, each Contributor,
 with respect to its own contribution to the Subject Software, hereby grants to each Recipient a
 non-exclusive, world-wide, royalty-free license to engage in the following activities pertaining to
 the Subject Software:

 1. Use

 2. Distribution

 3. Reproduction

 4. Modification

 5. Redistribution

 6. Display

 B. Under Patent Rights: Subject to the terms and conditions of this Agreement, each Contributor, with
 respect to its own contribution to the Subject Software, hereby grants to each Recipient under Covered
 Patents a non-exclusive, world-wide, royalty-free license to engage in the following activities
 pertaining to the Subject Software:

 1. Use

 2. Distribution

 3. Reproduction

 4. Sale

 5. Offer for Sale

 C. The rights granted under Paragraph B. also apply to the combination of a Contributor's Modification
 and the Subject Software if, at the time the Modification is added by the Contributor, the addition of
 such Modification causes the combination to be covered by the Covered Patents. It does not apply to
 any other combinations that include a Modification. 

 D. The rights granted in Paragraphs A. and B. allow the Recipient to sublicense those same rights.
 Such sublicense must be under the same terms and conditions of this Agreement.

 3. OBLIGATIONS OF RECIPIENT

 A. Distribution or Redistribution of the Subject Software must be made under this Agreement except for
 additions covered under paragraph 3H. 

 1. Whenever a Recipient distributes or redistributes the Subject Software, a copy of this Agreement
 must be included with each copy of the Subject Software; and

 2. If Recipient distributes or redistributes the Subject Software in any form other than source code,
 Recipient must also make the source code freely available, and must provide with each copy of the
 Subject Software information on how to obtain the source code in a reasonable manner on or through a
 medium customarily used for software exchange.

 B. Each Recipient must ensure that the following copyright notice appears prominently in the Subject
 Software:

          No copyright is claimed pursuant to 17 USC § 105.  All Other Rights Reserved.

 C. Each Contributor must characterize its alteration of the Subject Software as a Modification and
 must identify itself as the originator of its Modification in a manner that reasonably allows
 subsequent Recipients to identify the originator of the Modification. In fulfillment of these
 requirements, Contributor must include a file (e.g., a change log file) that describes the alterations
 made and the date of the alterations, identifies Contributor as originator of the alterations, and
 consents to characterization of the alterations as a Modification, for example, by including a
 statement that the Modification is derived, directly or indirectly, from Original Software provided by
 Government Agency. Once consent is granted, it may not thereafter be revoked.

 D. A Contributor may add its own copyright notice to the Subject Software. Once a copyright notice has
 been added to the Subject Software, a Recipient may not remove it without the express permission of
 the Contributor who added the notice.

 E. A Recipient may not make any representation in the Subject Software or in any promotional,
 advertising or other material that may be construed as an endorsement by Government Agency or by any
 prior Recipient of any product or service provided by Recipient, or that may seek to obtain commercial
 advantage by the fact of Government Agency's or a prior Recipient's participation in this Agreement.

 F. In an effort to track usage and maintain accurate records of the Subject Software, each Recipient,
 upon receipt of the Subject Software, is requested to register with Government Agency by visiting the
 following website: https://naspi.tva.com/Registration/. Recipient's name and personal information
 shall be used for statistical purposes only. Once a Recipient makes a Modification available, it is
 requested that the Recipient inform Government Agency at the web site provided above how to access the
 Modification.

 G. Each Contributor represents that that its Modification does not violate any existing agreements,
 regulations, statutes or rules, and further that Contributor has sufficient rights to grant the rights
 conveyed by this Agreement.

 H. A Recipient may choose to offer, and to charge a fee for, warranty, support, indemnity and/or
 liability obligations to one or more other Recipients of the Subject Software. A Recipient may do so,
 however, only on its own behalf and not on behalf of Government Agency or any other Recipient. Such a
 Recipient must make it absolutely clear that any such warranty, support, indemnity and/or liability
 obligation is offered by that Recipient alone. Further, such Recipient agrees to indemnify Government
 Agency and every other Recipient for any liability incurred by them as a result of warranty, support,
 indemnity and/or liability offered by such Recipient.

 I. A Recipient may create a Larger Work by combining Subject Software with separate software not
 governed by the terms of this agreement and distribute the Larger Work as a single product. In such
 case, the Recipient must make sure Subject Software, or portions thereof, included in the Larger Work
 is subject to this Agreement.

 J. Notwithstanding any provisions contained herein, Recipient is hereby put on notice that export of
 any goods or technical data from the United States may require some form of export license from the
 U.S. Government. Failure to obtain necessary export licenses may result in criminal liability under
 U.S. laws. Government Agency neither represents that a license shall not be required nor that, if
 required, it shall be issued. Nothing granted herein provides any such export license.

 4. DISCLAIMER OF WARRANTIES AND LIABILITIES; WAIVER AND INDEMNIFICATION

 A. No Warranty: THE SUBJECT SOFTWARE IS PROVIDED "AS IS" WITHOUT ANY WARRANTY OF ANY KIND, EITHER
 EXPRESSED, IMPLIED, OR STATUTORY, INCLUDING, BUT NOT LIMITED TO, ANY WARRANTY THAT THE SUBJECT
 SOFTWARE WILL CONFORM TO SPECIFICATIONS, ANY IMPLIED WARRANTIES OF MERCHANTABILITY, FITNESS FOR A
 PARTICULAR PURPOSE, OR FREEDOM FROM INFRINGEMENT, ANY WARRANTY THAT THE SUBJECT SOFTWARE WILL BE ERROR
 FREE, OR ANY WARRANTY THAT DOCUMENTATION, IF PROVIDED, WILL CONFORM TO THE SUBJECT SOFTWARE. THIS
 AGREEMENT DOES NOT, IN ANY MANNER, CONSTITUTE AN ENDORSEMENT BY GOVERNMENT AGENCY OR ANY PRIOR
 RECIPIENT OF ANY RESULTS, RESULTING DESIGNS, HARDWARE, SOFTWARE PRODUCTS OR ANY OTHER APPLICATIONS
 RESULTING FROM USE OF THE SUBJECT SOFTWARE. FURTHER, GOVERNMENT AGENCY DISCLAIMS ALL WARRANTIES AND
 LIABILITIES REGARDING THIRD-PARTY SOFTWARE, IF PRESENT IN THE ORIGINAL SOFTWARE, AND DISTRIBUTES IT
 "AS IS."

 B. Waiver and Indemnity: RECIPIENT AGREES TO WAIVE ANY AND ALL CLAIMS AGAINST GOVERNMENT AGENCY, ITS
 AGENTS, EMPLOYEES, CONTRACTORS AND SUBCONTRACTORS, AS WELL AS ANY PRIOR RECIPIENT. IF RECIPIENT'S USE
 OF THE SUBJECT SOFTWARE RESULTS IN ANY LIABILITIES, DEMANDS, DAMAGES, EXPENSES OR LOSSES ARISING FROM
 SUCH USE, INCLUDING ANY DAMAGES FROM PRODUCTS BASED ON, OR RESULTING FROM, RECIPIENT'S USE OF THE
 SUBJECT SOFTWARE, RECIPIENT SHALL INDEMNIFY AND HOLD HARMLESS  GOVERNMENT AGENCY, ITS AGENTS,
 EMPLOYEES, CONTRACTORS AND SUBCONTRACTORS, AS WELL AS ANY PRIOR RECIPIENT, TO THE EXTENT PERMITTED BY
 LAW.  THE FOREGOING RELEASE AND INDEMNIFICATION SHALL APPLY EVEN IF THE LIABILITIES, DEMANDS, DAMAGES,
 EXPENSES OR LOSSES ARE CAUSED, OCCASIONED, OR CONTRIBUTED TO BY THE NEGLIGENCE, SOLE OR CONCURRENT, OF
 GOVERNMENT AGENCY OR ANY PRIOR RECIPIENT.  RECIPIENT'S SOLE REMEDY FOR ANY SUCH MATTER SHALL BE THE
 IMMEDIATE, UNILATERAL TERMINATION OF THIS AGREEMENT.

 5. GENERAL TERMS

 A. Termination: This Agreement and the rights granted hereunder will terminate automatically if a
 Recipient fails to comply with these terms and conditions, and fails to cure such noncompliance within
 thirty (30) days of becoming aware of such noncompliance. Upon termination, a Recipient agrees to
 immediately cease use and distribution of the Subject Software. All sublicenses to the Subject
 Software properly granted by the breaching Recipient shall survive any such termination of this
 Agreement.

 B. Severability: If any provision of this Agreement is invalid or unenforceable under applicable law,
 it shall not affect the validity or enforceability of the remainder of the terms of this Agreement.

 C. Applicable Law: This Agreement shall be subject to United States federal law only for all purposes,
 including, but not limited to, determining the validity of this Agreement, the meaning of its
 provisions and the rights, obligations and remedies of the parties.

 D. Entire Understanding: This Agreement constitutes the entire understanding and agreement of the
 parties relating to release of the Subject Software and may not be superseded, modified or amended
 except by further written agreement duly executed by the parties.

 E. Binding Authority: By accepting and using the Subject Software under this Agreement, a Recipient
 affirms its authority to bind the Recipient to all terms and conditions of this Agreement and that
 Recipient hereby agrees to all terms and conditions herein.

 F. Point of Contact: Any Recipient contact with Government Agency is to be directed to the designated
 representative as follows: J. Ritchie Carroll <mailto:jrcarrol@tva.gov>.

*/
#endregion

// Define this constant to enable use of high-resolution time, undefine to use system time function
#define UseHighResolutionTime

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using TVA.Units;

namespace TVA.Measurements
{
    /// <summary>
    /// Measurement concentrator base class.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This class synchronizes (i.e., sorts by timestamp) real-time measurements.
    /// </para>
    /// <para>
    /// Note that your lag time should be defined as it relates to the rate at which data data is coming
    /// into the concentrator. Make sure you allow enough time for transmission of data over the network
    /// allowing any needed time for possible network congestion.  Lead time should be defined as your
    /// confidence in the accuracy of your local clock (e.g., if you set lead time to 2, this means you
    /// trust that your local clock is within plus or minus 2 seconds of real-time.)
    /// </para>
    /// </remarks>
    [CLSCompliant(false)]
    public abstract class ConcentratorBase : IDisposable
    {        
        #region [ Members ]

        // Events

        /// <summary>
        /// This event is raised every second allowing consumer to track current number of unpublished seconds of data in the queue.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T}.Argument"/> is the total number of unpublished seconds of data.
        /// </remarks>
        public event EventHandler<EventArgs<int>> UnpublishedSamples;

        /// <summary>
        /// This event is raised if there is an exception encountered while attempting to process a frame in the sample queue.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <see cref="EventArgs{T}.Argument"/> is the <see cref="Exception"/> encountered while parsing the data stream.
        /// </para>
        /// <para>
        /// Processing will not stop for any exceptions thrown by the user function, but any captured exceptions will be exposed through this event.
        /// </para>
        /// </remarks>
        public event EventHandler<EventArgs<Exception>> ProcessException;

        /// <summary>
        /// Raised, for the benefit of dependent classes, when lag time is updated.
        /// </summary> 
        internal event Action<double> LagTimeUpdated;

        /// <summary>
        /// Raised, for the benefit of dependent classes, when lead time is updated.
        /// </summary> 
        internal event Action<double> LeadTimeUpdated;

        // Fields
        private FrameQueue m_frameQueue;                    // Queue of frames to be published
        private PrecisionTimer m_publicationTimer;          // High precision timer used for frame processing
        private System.Timers.Timer m_monitorTimer;         // Sample monitor - tracks total number of unpublished frames
        private int m_framesPerSecond;                      // Frames per second
        private decimal m_ticksPerFrame;                    // Frame rate - we use a 64-bit scaled integer to avoid round-off errors in calculations
        private int[] m_framePeriods;                       // Evenly distributed waiting times, in whole milliseconds, per frame
        private int m_lastFramePeriod;                      // Tracks last frame period
        private int m_frameIndex;                           // Determines current frame index
        private double m_lagTime;                           // Allowed past time deviation tolerance, in seconds
        private double m_leadTime;                          // Allowed future time deviation tolerance, in seconds
        private long m_timeResolution;                      // Maximum sorting resolution in ticks
        private Ticks m_lagTicks;                           // Current lag time calculated in ticks
        private bool m_enabled;                             // Enabled state of concentrator
        private long m_startTime;                           // Start time of concentrator
        private long m_stopTime;                            // Stop time of concentrator
        private long m_realTimeTicks;                       // Timstamp of real-time or the most recently received measurement
        private bool m_allowSortsByArrival;                 // Determines whether or not to sort incoming measurements with a bad timestamp by arrival
        private bool m_useLocalClockAsRealTime;             // Determines whether or not to use local system clock as "real-time"
        private long m_totalMeasurements;                   // Total number of measurements ever requested for sorting
        private long m_measurementsSortedByArrival;         // Total number of measurements that were sorted by arrival
        private long m_discardedMeasurements;               // Total number of discarded measurements
        private long m_publishedMeasurements;               // Total number of published measurements
        private long m_missedSortsByTimeout;                // Total number of unsorted measurements due to timeout waiting for lock
        private long m_publishedFrames;                     // Total number of published frames
        private Ticks m_totalPublishTime;                   // Total cumulative frame user function publication time (in ticks) - used to calculate average
        private bool m_trackLatestMeasurements;             // Determines whether or not to track latest measurements
        private ImmediateMeasurements m_latestMeasurements; // Absolute latest received measurement values
        private IMeasurement m_lastDiscardedMeasurement;    // Last measurement that was discarded by the concentrator
        private bool m_disposed;                            // Disposed flag detects redundant calls to dispose method

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="ConcentratorBase"/>.
        /// </summary>
        /// <remarks>
        /// Concentration will not begin until consumer "Starts" concentrator (i.e., calling <see cref="ConcentratorBase.Start"/> method or setting
        /// <c><see cref="ConcentratorBase.Enabled"/> = true</c>).
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException">Specified argument is outside of allowed value range (see remarks).</exception>
        protected ConcentratorBase()
        {
#if UseHighResolutionTime
            m_realTimeTicks = PrecisionTimer.UtcNow.Ticks;
#else
            m_realTime = DateTime.UtcNow.Ticks;
#endif
            m_allowSortsByArrival = true;
            m_latestMeasurements = new ImmediateMeasurements(this);

            // Create a new queue for managing real-time frames
            m_frameQueue = new FrameQueue(this);

            // Set minimum timer resolution to one millisecond to improve timer accuracy
            PrecisionTimer.SetMinimumTimerResolution(1);

            // Create high precision timer used for frame processing
            m_publicationTimer = new PrecisionTimer();
            m_publicationTimer.AutoReset = true;
            m_publicationTimer.Tick += PublishFrames;

            // This timer monitors the total number of unpublished samples every second. This is a useful statistic
            // to monitor: if total number of unpublished samples exceed lag time, measurement concentration could
            // be falling behind.
            m_monitorTimer = new System.Timers.Timer();
            m_monitorTimer.Interval = 1000;
            m_monitorTimer.AutoReset = true;
            m_monitorTimer.Elapsed += MonitorUnpublishedSamples;
        }

        /// <summary>
        /// Creates a new <see cref="ConcentratorBase"/> from specified parameters.
        /// </summary>
        /// <param name="framesPerSecond">Number of frames to publish per second.</param>
        /// <param name="lagTime">Past time deviation tolerance, in seconds - this becomes the amount of time to wait before publishing begins.</param>
        /// <param name="leadTime">Future time deviation tolerance, in seconds - this becomes the tolerated +/- accuracy of the local clock to real-time.</param>
        /// <remarks>
        /// <para>
        /// <paramref name="framesPerSecond"/> must be greater then 0.
        /// </para>
        /// <para>
        /// <paramref name="lagTime"/> must be greater than zero, but can be specified in sub-second intervals (e.g., set to .25 for a quarter-second lag time).
        /// Note that this defines time sensitivity to past timestamps.
        /// </para>
        /// <para>
        /// <paramref name="leadTime"/> must be greater than zero, but can be specified in sub-second intervals (e.g., set to .5 for a half-second lead time).
        /// Note that this defines time sensitivity to future timestamps.
        /// </para>
        /// <para>
        /// Concentration will not begin until consumer "Starts" concentrator (i.e., calling <see cref="ConcentratorBase.Start"/> method or setting
        /// <c><see cref="ConcentratorBase.Enabled"/> = true</c>).
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException">Specified argument is outside of allowed value range (see remarks).</exception>
        protected ConcentratorBase(int framesPerSecond, double lagTime, double leadTime)
            : this()
        {
            this.FramesPerSecond = framesPerSecond;
            this.LagTime = lagTime;
            this.LeadTime = leadTime;
        }

        /// <summary>
        /// Releases the unmanaged resources before the <see cref="ConcentratorBase"/> object is reclaimed by <see cref="GC"/>.
        /// </summary>
        ~ConcentratorBase()
        {
            // We implement finalizer for this class to ensure sample queue shuts down in an orderly fashion.
            Dispose(false);
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the allowed past time deviation tolerance, in seconds (can be subsecond).
        /// </summary>
        /// <remarks>
        /// <para>Defines the time sensitivity to past measurement timestamps.</para>
        /// <para>The number of seconds allowed before assuming a measurement timestamp is too old.</para>
        /// <para>This becomes the amount of delay introduced by the concentrator to allow time for data to flow into the system.</para>
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException">LagTime must be greater than zero, but it can be less than one.</exception>
        public double LagTime
        {
            get
            {
                return m_lagTime;
            }
            set
            {
                if (value <= 0)
                    throw new ArgumentOutOfRangeException("value", "LagTime must be greater than zero, but it can be less than one");

                m_lagTime = value;
                m_lagTicks = (long)(m_lagTime * Ticks.PerSecond);

                if (LagTimeUpdated != null)
                    LagTimeUpdated(m_lagTime);
            }
        }

        /// <summary>
        /// Gets defined past time deviation tolerance, in ticks.
        /// </summary>
        public Ticks LagTicks
        {
            get
            {
                return m_lagTicks;
            }
        }

        /// <summary>
        /// Gets or sets the allowed future time deviation tolerance, in seconds (can be subsecond).
        /// </summary>
        /// <remarks>
        /// <para>Defines the time sensitivity to future measurement timestamps.</para>
        /// <para>The number of seconds allowed before assuming a measurement timestamp is too advanced.</para>
        /// <para>This becomes the tolerated +/- accuracy of the local clock to real-time.</para>
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException">LeadTime must be greater than zero, but it can be less than one.</exception>
        public double LeadTime
        {
            get
            {
                return m_leadTime;
            }
            set
            {
                if (value <= 0)
                    throw new ArgumentOutOfRangeException("value", "LeadTime must be greater than zero, but it can be less than one");

                m_leadTime = value;

                if (LeadTimeUpdated != null)
                    LeadTimeUpdated(m_leadTime);
            }
        }

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
        ///         <description><see cref="Ticks.PerSecond"/></description>
        ///     </item>
        ///     <item>
        ///         <term>Milliseconds</term>
        ///         <description><see cref="Ticks.PerMillisecond"/></description>
        ///     </item>
        ///     <item>
        ///         <term>Microseconds</term>
        ///         <description><see cref="Ticks.PerMicrosecond"/></description>
        ///     </item>
        ///     <item>
        ///         <term>100-Nanoseconds</term>
        ///         <description>0</description>
        ///     </item>
        /// </list>
        /// Assigning values less than zero will be set to zero since minimum possible concentrator resolution is one tick (100-nanoseconds).
        /// Assigning values values greater than <see cref="Ticks.PerSecond"/> will be set to <see cref="Ticks.PerSecond"/> since maximum
        /// possible concentrator resolution is one second (i.e., 1 frame per second).
        /// </remarks>
        public long TimeResolution
        {
            get
            {
                return m_timeResolution;
            }
            set
            {
                if (value < 0)
                    m_timeResolution = 0;
                if (value > Ticks.PerSecond)
                    m_timeResolution = Ticks.PerSecond;
                else
                    m_timeResolution = value;

                // Assign desired time resolution to frame queue
                m_frameQueue.TimeResolution = m_timeResolution;
            }
        }

        /// <summary>
        /// Gets or sets flag to start tracking the absolute latest received measurement values.
        /// </summary>
        /// <remarks>
        /// Lastest received measurement value will be available via the <see cref="LatestMeasurements"/> property.
        /// Note that enabling this option will slightly increase the required sorting time.
        /// </remarks>
        public bool TrackLatestMeasurements
        {
            get
            {
                return m_trackLatestMeasurements;
            }
            set
            {
                m_trackLatestMeasurements = value;
            }
        }

        /// <summary>
        /// Gets reference to the collection of absolute latest received measurement values.
        /// </summary>
        public ImmediateMeasurements LatestMeasurements
        {
            get
            {
                return m_latestMeasurements;
            }
        }

        /// <summary>
        /// Gets reference to the last published <see cref="IFrame"/>.
        /// </summary>
        public IFrame LastFrame
        {
            get
            {
                return m_frameQueue.Last;
            }
        }

        /// <summary>
        /// Gets or sets the number of frames per second.
        /// </summary>
        /// <remarks>
        /// Valid frame rates for a <see cref="ConcentratorBase"/> are greater than 0 frames per second.
        /// </remarks>
        public int FramesPerSecond
        {
            get
            {
                return m_framesPerSecond;
            }
            set
            {
                if (value < 1)
                    throw new ArgumentOutOfRangeException("value", "Frames per second must be greater than 0");

                m_framesPerSecond = value;
                m_ticksPerFrame = (decimal)Ticks.PerSecond / (decimal)m_framesPerSecond;

                if (m_frameQueue != null)
                    m_frameQueue.TicksPerFrame = m_ticksPerFrame;

                // Calculate new wait time periods for new number of frames per second
                int[] framePeriods = new int[m_framesPerSecond];

                for (int frameIndex = 0; frameIndex < m_framesPerSecond; frameIndex++)
                {
                    framePeriods[frameIndex] = CalcWaitTimeForFrameIndex(m_framesPerSecond, frameIndex);
                }

                Interlocked.Exchange(ref m_framePeriods, framePeriods);
            }
        }

        /// <summary>
        /// Gets the number of ticks per frame.
        /// </summary>
        public decimal TicksPerFrame
        {
            get
            {
                return m_ticksPerFrame;
            }
        }

        /// <summary>
        /// Gets or sets the current enabled state of concentrator.
        /// </summary>
        /// <returns>Current enabled state of concentrator</returns>
        /// <remarks>
        /// Concentrator must be started by calling <see cref="ConcentratorBase.Start"/> method or setting
        /// <c><see cref="ConcentratorBase.Enabled"/> = true</c>) before concentration will begin.
        /// </remarks>
        public virtual bool Enabled
        {
            get
            {
                return m_enabled;
            }
            set
            {
                if (value)
                    Start();
                else
                    Stop();
            }
        }
        /// <summary>
        /// Gets the total amount of time, in seconds, that the concentrator has been active.
        /// </summary>
        public virtual Time RunTime
        {
            get
            {
                Ticks processingTime = 0;

                if (m_startTime > 0)
                {
                    if (m_stopTime > 0)
                    {
                        processingTime = m_stopTime - m_startTime;
                    }
                    else
                    {
#if UseHighResolutionTime
                        processingTime = PrecisionTimer.UtcNow.Ticks - m_startTime;
#else
                        processingTime = DateTime.UtcNow.Ticks - m_startTime;
#endif
                    }
                }

                if (processingTime < 0) processingTime = 0;

                return processingTime.ToSeconds();
            }
        }

        /// <summary>
        /// Gets or sets flag that determines whether or not to allow incoming measurements with bad timestamps
        /// to be sorted by arrival time.
        /// </summary>
        /// <remarks>
        /// Value defaults to <c>true</c>, so any incoming measurement with a bad timestamp quality will be sorted
        /// according to its arrival time. Setting the property to <c>false</c> will cause all measurements with a
        /// bad timestamp quality to be discarded.
        /// </remarks>
        public bool AllowSortsByArrival
        {
            get
            {
                return m_allowSortsByArrival;
            }
            set
            {
                m_allowSortsByArrival = value;
            }
        }

        /// <summary>
        /// Gets or sets flag that determines whether or not to use the local clock time as real time.
        /// </summary>
        /// <remarks>
        /// Use your local system clock as real time only if the time is locally GPS-synchronized,
        /// or if the measurement values being sorted were not measured relative to a GPS-synchronized clock.
        /// </remarks>
        public bool UseLocalClockAsRealTime
        {
            get
            {
                return m_useLocalClockAsRealTime;
            }
            set
            {
                m_useLocalClockAsRealTime = value;
            }
        }

        /// <summary>
        /// Gets the the most accurate time value that is available. If <see cref="UseLocalClockAsRealTime"/> = <c>true</c>, then
        /// this function will return <see cref="DateTime.UtcNow"/>. Otherwise, this function will return the timestamp of the
        /// most recent measurement, or <see cref="DateTime.UtcNow"/> if no measurement timestamps are within time deviation
        /// tolerances as specified by the <see cref="LeadTime"/> value.
        /// </summary>
        /// <remarks>
        /// Because the measurements being received by remote devices are often measured relative to GPS time, these timestamps
        /// are typically more accurate than the local clock. As a result, we can use the latest received timestamp as the best
        /// local time measurement we have (ignoring transmission delays); but, even these times can be incorrect so we still have
        /// to apply reasonability checks to these times. To do this, we use the local system time and the <see cref="LeadTime"/>
        /// value to validate the latest measured timestamp. If the newest received measurement timestamp gets too old or creeps
        /// too far into the future (both validated + and - against defined lead time property value), we will fall back on local
        /// system time. Note that this creates a dependency on a fairly accurate local clock - the smaller the lead time deviation
        /// tolerance, the better the needed local clock acuracy. For example, a lead time deviation tolerance of a few seconds
        /// might only require keeping the local clock synchronized to an NTP time source; but, a sub-second tolerance would
        /// require that the local clock be very close to GPS time.
        /// </remarks>
        public Ticks RealTime
        {
            get
            {
                if (m_useLocalClockAsRealTime)
                {
                    // Assumes local system clock is the best value we have for real time.
#if UseHighResolutionTime
                    return PrecisionTimer.UtcNow.Ticks;
#else
                    return DateTime.UtcNow.Ticks;
#endif
                }
                else
                {
                    // If the current value for real-time is outside of the time deviation tolerance of the local
                    // clock, then we set latest measurement time (i.e., real-time) to be the current local clock
                    // time. Since the lead time typically defines the tolerated accuracy of the local clock to
                    // real-time we will use this value as the + and - timestamp tolerance to validate if the
                    // measurement time is reasonable.
#if UseHighResolutionTime
                    long currentTimeTicks = PrecisionTimer.UtcNow.Ticks;
#else
                    long currentTimeTicks = DateTime.UtcNow.Ticks;
#endif
                    long currentRealTimeTicks = m_realTimeTicks;
                    double distance = (currentTimeTicks - currentRealTimeTicks) / (double)Ticks.PerSecond;

                    if (distance > m_leadTime || distance < -m_leadTime)
                    {
                        // Set real time ticks to current ticks (as long as another thread hasn't changed it
                        // already), the interlocked compare exchange avoids an expensive synclock to update real
                        // time ticks.
                        Interlocked.CompareExchange(ref m_realTimeTicks, currentTimeTicks, currentRealTimeTicks);
                    }

                    // Assume lastest measurement timestamp is the best value we have for real-time.
                    return m_realTimeTicks;
                }
            }
        }

        /// <summary>
        /// Gets the total number of measurements that have ever been requested for sorting.
        /// </summary>
        public long TotalMeasurements
        {
            get
            {
                return m_totalMeasurements;
            }
        }

        /// <summary>
        /// Gets the total number of measurements that have been discarded because of old timestamps
        /// (i.e., measurements that were outside the time deviation tolerance from base time, past or future).
        /// </summary>
        public long DiscardedMeasurements
        {
            get
            {
                return m_discardedMeasurements;
            }
        }

        /// <summary>
        /// Gets a reference the last <see cref="IMeasurement"/> that was discarded by the concentrator.
        /// </summary>
        public IMeasurement LastDiscardedMeasurement
        {
            get
            {
                return m_lastDiscardedMeasurement;
            }
        }

        /// <summary>
        /// Gets the total number of published measurements.
        /// </summary>
        public long PublishedMeasurements
        {
            get
            {
                return m_publishedMeasurements;
            }
        }

        /// <summary>
        /// Gets the total number of published frames.
        /// </summary>
        public long PublishedFrames
        {
            get
            {
                return m_publishedFrames;
            }
        }

        /// <summary>
        /// Gets the total number of measurements that were sorted by arrival because the measurement reported a bad timestamp quality.
        /// </summary>
        public long MeasurementsSortedByArrival
        {
            get
            {
                return m_measurementsSortedByArrival;
            }
        }

        /// <summary>
        /// Gets the total number of seconds frames have spent in the publication process since concentrator started.
        /// </summary>
        public Time TotalPublicationTime
        {
            get
            {
                return m_totalPublishTime.ToSeconds();
            }
        }

        /// <summary>
        /// Gets the average required frame publication time, in seconds.
        /// </summary>
        /// <remarks>
        /// If user publication function, <see cref="ConcentratorBase.PublishFrame"/>, consistently exceeds available publishing time
        /// (i.e., <c>1 / <see cref="ConcentratorBase.FramesPerSecond"/></c>), concentration will fall behind.
        /// </remarks>
        public Time AveratePublicationTimePerFrame
        {
            get
            {
                return TotalPublicationTime / m_publishedFrames;
            }
        }

        /// <summary>
        /// Gets current detailed state and status of concentrator for display purposes.
        /// </summary>
        public virtual string Status
        {
            get
            {
                StringBuilder status = new StringBuilder();
                IFrame lastFrame = m_frameQueue.Last;
#if UseHighResolutionTime
                DateTime currentTime = PrecisionTimer.UtcNow;
#else
                DateTime currentTime = DateTime.UtcNow;
#endif

                status.AppendFormat("     Data concentration is: {0}", Enabled ? "Enabled" : "Disabled");
                status.AppendLine();
                status.AppendFormat("    Total process run time: {0}", RunTime.ToString());
                status.AppendLine();
                status.AppendFormat("    Measurement wait delay: {0} seconds (lag time)", m_lagTime);
                status.AppendLine();
                status.AppendFormat("     Local clock tolerance: {0} seconds (lead time)", m_leadTime);
                status.AppendLine();
                status.AppendFormat("   Maximum time resolution: {0} ticks", m_timeResolution);
                status.AppendLine();
                status.AppendFormat("    Local clock time (UTC): {0}", currentTime.ToString("dd-MMM-yyyy HH:mm:ss.fff"));
                status.AppendLine();
                status.AppendFormat("  Using clock as real-time: {0}", m_useLocalClockAsRealTime);
                status.AppendLine();
                if (!m_useLocalClockAsRealTime)
                {
                    status.Append("      Local clock accuracy: ");
#if UseHighResolutionTime
                    status.Append(SecondsFromRealTime(PrecisionTimer.UtcNow.Ticks).ToString("0.0000"));
#else
                    status.Append(SecondsFromRealTime(DateTime.UtcNow.Ticks).ToString("0.0000"));
#endif
                    status.Append(" second deviation from latest time");
                    status.AppendLine();
                }
                status.AppendFormat(" Allowing sorts by arrival: {0}", m_allowSortsByArrival);
                status.AppendLine();
                status.AppendFormat("        Total measurements: {0}", m_totalMeasurements);
                status.AppendLine();
                status.AppendFormat("    Published measurements: {0}", m_publishedMeasurements);
                status.AppendLine();
                status.AppendFormat("    Discarded measurements: {0}", m_discardedMeasurements);
                status.AppendLine();
                status.Append("Last discarded measurement: ");
                if (m_lastDiscardedMeasurement == null)
                {
                    status.Append("<none>");
                }
                else
                {
                    status.Append(Measurement.ToString(m_lastDiscardedMeasurement));
                    status.Append(" - ");
                    status.Append(((DateTime)m_lastDiscardedMeasurement.Timestamp).ToString("dd-MMM-yyyy HH:mm:ss.fff"));
                }
                status.AppendLine();
                status.AppendFormat("    Total sorts by arrival: {0}", m_measurementsSortedByArrival);
                status.AppendLine();
                status.AppendFormat("   Missed sorts by timeout: {0}", m_missedSortsByTimeout);
                status.AppendLine();
                status.AppendFormat("  Average publication time: {0} milliseconds", (AveratePublicationTimePerFrame / SI.Milli).ToString("0.0000"));
                status.AppendLine();
                status.AppendFormat(" User function utilization: {0} of available time used", ((decimal)1.0 - (m_ticksPerFrame - (decimal)AveratePublicationTimePerFrame.ToTicks()) / m_ticksPerFrame).ToString("##0.0000%"));
                status.AppendLine();
                status.AppendFormat("Published measurement loss: {0}", (m_discardedMeasurements / (double)m_totalMeasurements).ToString("##0.0000%"));
                status.AppendLine();
                status.AppendFormat("      Loss due to timeouts: {0}", (m_missedSortsByTimeout / (double)m_totalMeasurements).ToString("##0.0000%"));
                status.AppendLine();
                status.AppendFormat(" Measurement time accuracy: {0}", (1.0D - m_measurementsSortedByArrival / (double)m_totalMeasurements).ToString("##0.0000%"));
                status.AppendLine();
                status.AppendFormat("    Total published frames: {0}", m_publishedFrames);
                status.AppendLine();
                status.AppendFormat("        Defined frame rate: {0} frames/sec, {1} ticks/frame", m_framesPerSecond, m_ticksPerFrame.ToString("0.00"));
                status.AppendLine();
                status.AppendFormat("    Actual mean frame rate: {0} frames/sec", (m_publishedFrames / (RunTime - m_lagTime)).ToString("0.00"));
                status.AppendLine();
                status.AppendFormat("        Queued frame count: {0}", m_frameQueue.Count);
                status.AppendLine();
                status.Append("      Last published frame: ");

                if (lastFrame == null)
                {
                    status.Append("<none>");
                }
                else
                {
                    status.Append(((DateTime)lastFrame.Timestamp).ToString("dd-MMM-yyyy HH:mm:ss.fff"));
                    status.AppendLine();
                    status.Append("   Last sorted measurement: ");
                    status.Append(Measurement.ToString(lastFrame.LastSortedMeasurement));
                }
                status.AppendLine();

                return status.ToString();
            }
        }

        /// <summary>
        /// Gets a reference to the <see cref="FrameQueue"/>.
        /// </summary>
        protected FrameQueue FrameQueue
        {
            get
            {
                return m_frameQueue;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Releases all the resources used by the <see cref="ConcentratorBase"/> object.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="ConcentratorBase"/> object and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!m_disposed)
            {
                try
                {
                    if (disposing)
                    {
                        if (m_publicationTimer != null)
                        {
                            m_publicationTimer.Tick -= PublishFrames;
                            m_publicationTimer.Dispose();
                        }
                        m_publicationTimer = null;

                        if (m_frameQueue != null)
                        {
                            m_frameQueue.Dispose();
                        }
                        m_frameQueue = null;

                        if (m_monitorTimer != null)
                        {
                            m_monitorTimer.Elapsed -= MonitorUnpublishedSamples;
                            m_monitorTimer.Dispose();
                        }
                        m_monitorTimer = null;

                        if (m_latestMeasurements != null)
                        {
                            m_latestMeasurements.Dispose();
                        }
                        m_latestMeasurements = null;

                        m_lastDiscardedMeasurement = null;

                        // Clear minimum timer resolution.
                        PrecisionTimer.ClearMinimumTimerResolution(1);
                    }
                }
                finally
                {
                    m_disposed = true;  // Prevent duplicate dispose.
                }
            }
        }

        /// <summary>
        /// Starts the concentrator, if it is not already running.
        /// </summary>
        /// <remarks>
        /// Concentrator must be started by calling <see cref="ConcentratorBase.Start"/> method or setting
        /// <c><see cref="ConcentratorBase.Enabled"/> = true</c>) before concentration will begin.
        /// </remarks>
        public virtual void Start()
        {
            if (!m_enabled)
            {
                // Reset statistics
                m_totalMeasurements = 0;
                m_measurementsSortedByArrival = 0;
                m_discardedMeasurements = 0;
                m_publishedMeasurements = 0;
                m_missedSortsByTimeout = 0;
                m_publishedFrames = 0;
                m_totalPublishTime = 0;
                m_stopTime = 0;
#if UseHighResolutionTime
                m_startTime = PrecisionTimer.UtcNow.Ticks;
#else
                m_startTime = DateTime.UtcNow.Ticks;
#endif
                m_frameQueue.Clear();

                // Start real-time frame publication
                m_frameIndex = 0;
                m_lastFramePeriod = m_framePeriods[m_frameIndex];
                m_publicationTimer.Period = m_lastFramePeriod;
                m_publicationTimer.Start();
                m_monitorTimer.Start();
            }

            m_enabled = true;
        }

        /// <summary>
        /// Stops the concentrator.
        /// </summary>
        public virtual void Stop()
        {
            if (m_enabled)
            {
                m_enabled = false;

                m_publicationTimer.Stop();
                m_monitorTimer.Stop();
                m_frameQueue.Clear();

#if UseHighResolutionTime
                m_stopTime = PrecisionTimer.UtcNow.Ticks;
#else
                m_stopTime = DateTime.UtcNow.Ticks;
#endif
            }
        }

        /// <summary>
        /// Returns the deviation, in seconds, that the given number of ticks is from real-time (i.e., <see cref="ConcentratorBase.RealTime"/>).
        /// </summary>
        /// <param name="timestamp">Timestamp to calculate distance from real-time.</param>
        /// <returns>A <see cref="Double"/> value indicating the deviation, in seconds, from real-time.</returns>
        public double SecondsFromRealTime(Ticks timestamp)
        {
            return (RealTime - timestamp).ToSeconds();
        }

        /// <summary>
        /// Returns the deviation, in milliseconds, that the given number of ticks is from real-time (i.e., <see cref="ConcentratorBase.RealTime"/>).
        /// </summary>
        /// <param name="timestamp">Timestamp to calculate distance from real-time.</param>
        /// <returns>A <see cref="Double"/> value indicating the deviation in milliseconds.</returns>
        public double MillisecondsFromRealTime(Ticks timestamp)
        {
            return (RealTime - timestamp).ToMilliseconds();
        }

        /// <summary>
        /// Sorts the <see cref="IMeasurement"/> placing the data point in its proper <see cref="IFrame"/>.
        /// </summary>
        /// <param name="measurement"><see cref="IMeasurement"/> to sort.</param>
        public virtual void SortMeasurement(IMeasurement measurement)
        {
            SortMeasurements(new IMeasurement[] { measurement });
        }

        /// <summary>
        /// Sorts each <see cref="IMeasurement"/> placing each data point in its proper <see cref="IFrame"/>.
        /// </summary>
        /// <param name="measurements">Collection of <see cref="IMeasurement"/>'s to sort.</param>
        public virtual void SortMeasurements(IEnumerable<IMeasurement> measurements)
        {
            if (!m_enabled) return;

            // This function is called continually with new measurements to handle "time-alignment" (i.e., sorting)
            // of these new values. Many threads will be waiting for frames of time aligned data so make sure any
            // work to be done here is executed as efficiently as possible.

            // Note that breaking up this function into several parts might help with readability and make it
            // easier to maintain, but to reduce function calls (and hence save time) the decision was made to
            // put the code into one larger more complex function...

            IFrame frame = null;
            Ticks timestamp = 0, lastTimestamp = 0;
            double distance;
            bool discardMeasurement;

            // Track the total number of measurements ever requested for sorting.
            Interlocked.Add(ref m_totalMeasurements, measurements.Count());

            // Measurements usually come in groups. This function processes all available measurements in the
            // collection here directly as an optimization which avoids the overhead of a function call for
            // each measurement.
            foreach (IMeasurement measurement in measurements)
            {
                // Reset flag for next measurement.
                discardMeasurement = false;

                // Check for a bad measurement timestamp.
                if (!measurement.TimestampQualityIsGood)
                {
                    if (m_allowSortsByArrival)
                    {
                        // Device reports measurement timestamp as bad; this typically means that the GPS timestamp of the
                        // source device is not accurate. If the concentrator is set to allow sorts by arrival then it is
                        // assumed that our local real time value is better than the device measurement, so we sort the
                        // measurement by arrival time.
                        timestamp = RealTime;
                        Interlocked.Increment(ref m_measurementsSortedByArrival);
                    }
                    else
                    {
                        // If sorting by arrival time is not allowed, measurements with bad timestamps are discarded.
                        discardMeasurement = true;
                    }
                }
                else
                {
                    // Timestamp quality is good, get ticks for this measurement.
                    timestamp = measurement.Timestamp;
                }

                if (!discardMeasurement)
                {
                    //
                    // *** Sort the measurement into proper frame ***
                    //

                    // Get the destination frame for the measurement. Note that groups of parsed measurements will
                    // typically be coming in from the same source and will have the same ticks. If we have already
                    // found the destination frame for the same ticks, then there is no need to lookup frame again.
                    if (frame == null || timestamp != lastTimestamp)
                    {
                        // Badly time-aligned measurements, or those coming in at a higher sample rate, may fall
                        // outside available frame buckets. To check for this, the difference between the measurement
                        // timestamp and real-time in seconds is calculated and validated between lag and lead times.
                        distance = SecondsFromRealTime(timestamp);

                        if (distance > m_lagTime || distance < -m_leadTime)
                        {
                            // This data has come in late or has a future timestamp.  For old timestamps, we're not
                            // going to create a frame for data that will never be processed.  For future dates we
                            // must assume that the clock from source device must be advanced and out-of-sync with
                            // real-time - either way this data will be discarded.
                            frame = null;
                        }
                        else
                        {
                            // Get a frame for this measurement
                            frame = m_frameQueue.GetFrame(timestamp);
                            lastTimestamp = timestamp;
                        }
                    }

                    if (frame == null)
                    {
                        // Measurement is discarded if no bucket (i.e., destination frame) was found for it.
                        discardMeasurement = true;
                        lastTimestamp = 0;
                    }
                    else
                    {
                        // Assign new measurement to its frame using user customizable function.
                        if (AssignMeasurementToFrame(frame, measurement))
                        {
                            frame.LastSortedMeasurement = measurement;
                        }
                        else
                        {
                            // Track the total number of measurements that failed to sort because the
                            // system ran out of time.
                            Interlocked.Increment(ref m_missedSortsByTimeout);

                            // Count this as a discarded measurement if it was never assigned to the frame.
                            discardMeasurement = true;
                        }

                        // If enabled, concentrator will track the absolute latest measurement values.
                        if (m_trackLatestMeasurements)
                            m_latestMeasurements.UpdateMeasurementValue(measurement);
                    }
                }

                if (discardMeasurement)
                {
                    // This measurement was marked to be discarded.
                    Interlocked.Increment(ref m_discardedMeasurements);
                    m_lastDiscardedMeasurement = measurement;
                }
                else
                {
                    //
                    // *** Manage "real-time" ticks ***
                    //

                    if (!m_useLocalClockAsRealTime)
                    {
                        // Algorithm:
                        //      If the measurement time is newer than the current real time value and within the
                        //      specified time deviation tolerance of the local clock time, then the measurement
                        //      timestamp is set as real time.
                        long realTimeTicks = m_realTimeTicks;

                        if (timestamp > m_realTimeTicks)
                        {
                            // Apply a resonability check to this value using the local clock. Since the lead time
                            // typically defines the tolerated accuracy of the local clock to real time, this value
                            // is used as the + and - timestamp tolerance to validate if the time is reasonable.
#if UseHighResolutionTime
                            long currentTimeTicks = PrecisionTimer.UtcNow.Ticks;
#else
                            long currentTimeTicks = DateTime.UtcNow.Ticks;
#endif
                            if (timestamp.TimeIsValid(currentTimeTicks, m_leadTime, m_leadTime))
                            {
                                // The new time measurement looks good, so this function assumes the time is
                                // "real time" so long as another thread has not changed the real time value
                                // already. Using the interlocked compare exchange method introduces the
                                // possibility that we may have had newer ticks than another thread that just
                                // updated real-time ticks, but if so the deviation will not be much since ticks
                                // were greater than current real-time ticks in all threads that got to this
                                // point. Besides, newer measurements are always coming in anyway and the compare
                                // exchange method saves a call to a monitor lock thereby reducing contention.
                                Interlocked.CompareExchange(ref m_realTimeTicks, timestamp, realTimeTicks);
                            }
                            else
                            {
                                // Measurement ticks were outside of time deviation tolerances so we'll also check to make
                                // sure current real-time ticks are within these tolerances as well
                                distance = (currentTimeTicks - m_realTimeTicks) / (double)Ticks.PerSecond;

                                if (distance > m_leadTime || distance < -m_leadTime)
                                {
                                    // New time measurement was invalid as was current real-time value so we have no choice but to
                                    // assume the current time as "real-time", so we set real time ticks to current ticks so long
                                    // as another thread hasn't changed it already
                                    Interlocked.CompareExchange(ref m_realTimeTicks, currentTimeTicks, realTimeTicks);
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Publish <see cref="IFrame"/> of time-aligned collection of <see cref="IMeasurement"/> values that arrived within the
        /// concentrator's defined <see cref="ConcentratorBase.LagTime"/>.
        /// </summary>
        /// <param name="frame"><see cref="IFrame"/> of measurements with the same timestamp that arrived within <see cref="ConcentratorBase.LagTime"/> that are ready for processing.</param>
        /// <param name="index">Index of <see cref="IFrame"/> within a second ranging from zero to <c><see cref="ConcentratorBase.FramesPerSecond"/> - 1</c>.</param>
        /// <remarks>
        /// If user implemented publication function consistently exceeds available publishing time (i.e., <c>1 / <see cref="ConcentratorBase.FramesPerSecond"/></c> seconds),
        /// concentration will fall behind. A small amount of this time is required by the <see cref="ConcentratorBase"/> for processing overhead, so actual total time
        /// available for user function process will always be slightly less than <c>1 / <see cref="ConcentratorBase.FramesPerSecond"/></c> seconds.
        /// </remarks>
        protected abstract void PublishFrame(IFrame frame, int index);

        /// <summary>
        /// Creates a new <see cref="IFrame"/> for the given <paramref name="timestamp"/>.
        /// </summary>
        /// <param name="timestamp">Timestamp for new <see cref="IFrame"/> in <see cref="Ticks"/>.</param>
        /// <returns>New <see cref="IFrame"/> at given <paramref name="timestamp"/>.</returns>
        /// <remarks>
        /// Derived classes can override this method to create a new custom <see cref="IFrame"/>. Default
        /// behavior creates a basic <see cref="Frame"/> to hold synchronized measurements.
        /// </remarks>
        protected internal virtual IFrame CreateNewFrame(Ticks timestamp)
        {
            return new Frame(timestamp);
        }

        /// <summary>
        /// Assigns <see cref="IMeasurement"/> to its associated <see cref="IFrame"/>.
        /// </summary>
        /// <returns>True if <see cref="IMeasurement"/> was successfully assigned to its <see cref="IFrame"/>.</returns>
        /// <remarks>
        /// <para>
        /// Derived classes can choose to override this method to handle custom assignment of a <see cref="IMeasurement"/> to
        /// its <see cref="IFrame"/>. Default behavior simply assigns measurement to frame's keyed measurement dictionary.
        /// </para>
        /// <example>
        /// If overridden user must perform their own synchronization as needed, for example:
        /// <code>
        /// lock (frame.Measurements)
        /// {
        ///     if (!frame.Published)
        ///     {
        ///         frame.Measurements[measurement.Key] = measurement;
        ///         return true;
        ///     }
        ///     
        ///     return false;
        /// }
        /// </code>
        /// </example>
        /// <para>
        /// Note that the <see cref="IFrame.Measurements"/> dictionary is used internally to synchrnonize assignment
        /// of the <see cref="IFrame.Published"/> flag. If your custom <see cref="IFrame"/> makes use of the
        /// <see cref="IFrame.Measurements"/> dictionary you must implement a locking scheme similar to the sample
        /// code to prevent changes to the measurement dictionary during frame publication.
        /// </para>
        /// </remarks>
        /// <param name="frame">The <see cref="IFrame"/> that is used.</param>
        /// <param name="measurement">The type of <see cref="IMeasurement"/> to use."/></param>
        protected virtual bool AssignMeasurementToFrame(IFrame frame, IMeasurement measurement)
        {
            IDictionary<MeasurementKey, IMeasurement> measurements = frame.Measurements;

            lock (measurements)
            {
                if (!frame.Published)
                {
                    measurements[measurement.Key] = measurement;
                    return true;
                }

                return false;
            }
        }

        /// <summary>
        /// Raises the <see cref="ProcessException"/> event.
        /// </summary>
        /// <param name="ex">Exception to send to <see cref="ProcessException"/> event.</param>
        /// <remarks>
        /// Allows derived classes to raise a processing exception.
        /// </remarks>
        protected void OnProcessException(Exception ex)
        {
            if (ProcessException != null)
                ProcessException(this, new EventArgs<Exception>(ex));
        }

        // Member variables being updated in this method are only updated here so we don't worry about atomic operations on
        // these variables. This method is the PrecisionTimer's "Tick" event delegate handler.
        private void PublishFrames(object sender, EventArgs e)
        {
            IFrame frame;
            Ticks timestamp, distance;
            int frameIndex, period;
            decimal timeOffset = (m_timeResolution > 1 ? m_timeResolution / 2 : 1);

            // First things first, prepare timer period for next call...
            m_frameIndex++;

            if (m_frameIndex >= m_framesPerSecond)
                m_frameIndex = 0;

            // Get the frame period for this frame index
            period = m_framePeriods[m_frameIndex];

            // We only update timer period if it has changed since last call. Note that this is necessary since
            // timer periods are defined as integers but actual period is typically uneven (e.g., 33.333 ms)
            if (m_lastFramePeriod != period)
                m_publicationTimer.Period = period;

            m_lastFramePeriod = period;

            // Keep publishing frames so long as they are ready for publication. This handles case where
            // system may be falling behind because user function is taking too long - exit when no
            // other frames are available to process

            while (true)
            {
                try
                {
                    // Get top frame
                    frame = m_frameQueue.Head;

                    if (frame == null)
                    {
                        // No frame ready to publish, exit
                        break;
                    }
                    else
                    {
                        // Get ticks for this frame
                        timestamp = frame.Timestamp;

                        // See if any lagtime needs to pass before we begin publishing,
                        // distance is calculated in ticks
                        distance = m_lagTicks - (RealTime - timestamp);

                        // Exit if it's not time to publish
                        if (distance > 0)
                            break;

                        // Mark start time for publication
#if UseHighResolutionTime
                        distance = PrecisionTimer.UtcNow.Ticks;
#else
                        distance = DateTime.UtcNow.Ticks;
#endif

                        // Calculate index of this frame within its second - note that we have to calculate this
                        // value instead of using m_frameIndex since it is is possible for multiple frames to be
                        // published within one frame period if the system is stressed
                        frameIndex = (int)(((decimal)timestamp.DistanceBeyondSecond() + timeOffset) / m_ticksPerFrame);

                        // Mark the frame as published to prevent any further sorting into this frame
                        lock (frame.Measurements)
                        {
                            // Setting this flag is in a critcal section to ensure that
                            // sorting into this frame has ceased prior to publication...
                            frame.Published = true;
                        }

                        try
                        {
                            // Publish the current frame (i.e., call user implemented publication function)
                            PublishFrame(frame, frameIndex);
                        }
                        finally
                        {
                            // Remove the frame from the queue whether it is successfully published or not
                            m_frameQueue.Pop();

                            // Update publication statistics
                            m_publishedFrames++;
                            m_publishedMeasurements += frame.PublishedMeasurements;

                            // Track total publication time
#if UseHighResolutionTime
                            m_totalPublishTime += PrecisionTimer.UtcNow.Ticks - distance;
#else
                            m_totalPublishTime += DateTime.UtcNow.Ticks - distance;
#endif
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Not stopping for exceptions - but we'll let user know there are issues...
                    OnProcessException(ex);
                    break;
                }
            }
        }

        // Exposes the number of unpublished seconds of data in the queue (note that first second of data will always be "publishing").
        private void MonitorUnpublishedSamples(object sender, System.Timers.ElapsedEventArgs e)
        {
            int secondsOfData = (m_frameQueue.Count / m_framesPerSecond) - 1;

            if (secondsOfData < 0)
                secondsOfData = 0;

            if (UnpublishedSamples != null)
                UnpublishedSamples(this, new EventArgs<int>(secondsOfData));
        }

        // Wait times are not necessarily perfectly even (e.g., at 30 samples per second wait time per frame is 33.333... milliseconds)
        // so we use this function to evenly distribute integer based millisecond wait times across a second.
        private static int CalcWaitTimeForFrameIndex(int framesPerSecond, int frameIndex)
        {
            // Jian Zuo...
            int millisecondsWaitTime;
            int frameRate;
            int deficit;

            frameRate = (int)(Math.Round(1000.0D / framesPerSecond));
            deficit = 1000 - frameRate * framesPerSecond;

            if (deficit == 0)
            {
                millisecondsWaitTime = frameRate;
            }
            else
            {
                if (frameIndex == 0)
                {
                    millisecondsWaitTime = frameRate;
                }
                else if (frameIndex == framesPerSecond - 1)
                {
                    millisecondsWaitTime = frameRate + (deficit > 0 ? 1 : -1);
                }
                else
                {
                    double interval = framesPerSecond / Math.Abs((double)deficit);
                    double pre_dis = mod_dis(frameIndex - 1, interval);
                    double cur_dis = mod_dis(frameIndex, interval);
                    double next_dis = mod_dis(frameIndex + 1, interval);

                    millisecondsWaitTime = frameRate + ((cur_dis <= pre_dis && cur_dis < next_dis) ? (deficit > 0 ? 1 : -1) : 0);
                }
            }

            return millisecondsWaitTime;
        }

        private static double mod_dis(int framesIndex, double interval)
        {
            double dis2 = (framesIndex + 1) % interval;
            double dis1 = interval - dis2;
            return (dis1 < dis2 ? dis1 : dis2);
        }

        #endregion
    }
}