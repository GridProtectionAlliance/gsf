//*******************************************************************************************************
//  PrecisionTimer.cs - Gbtc
//
//  Tennessee Valley Authority, 2009
//  No copyright is claimed pursuant to 17 USC § 105.  All Other Rights Reserved.
//
//  This software is made freely available under the TVA Open Source Agreement (see below).
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  11/22/2003 - Leslie Sanford
//       Original version of source code for Multimedia.Timer class.
//  04/10/2008 - James Brock
//       Original version of source code for DateTimePrecise class.
//  08/21/2008 - J. Ritchie Carroll
//       Integrated, merged and adapted for general use as PrecisionTimer.
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

/**********************************************************************************\
   Copyright © 2009 for combined work, Gbtc - 
        James Ritchie Carroll, Leslie Sanford and James Brock
 
   All rights reserved.
  
   Redistribution and use in source and binary forms, with or without
   modification, are permitted provided that the following conditions
   are met:
  
      * Redistributions of source code must retain the above copyright
        notice, this list of conditions and the following disclaimer.
       
      * Redistributions in binary form must reproduce the above
        copyright notice, this list of conditions and the following
        disclaimer in the documentation and/or other materials provided
        with the distribution.
  
   THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDER "AS IS" AND ANY
   EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
   IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR
   PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR
   CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL,
   EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
   PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR
   PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY
   OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
   (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
   OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

   ------------------------------------------------------------------------------

   Multimedia.Timer class adaptation:
   Copyright (c) 2006 Leslie Sanford

   * Permission is hereby granted, free of charge, to any person obtaining a copy
   * of this software and associated documentation files (the "Software"), to
   * deal in the Software without restriction, including without limitation the
   * rights to use, copy, modify, merge, publish, distribute, sublicense, and/or
   * sell copies of the Software, and to permit persons to whom the Software is
   * furnished to do so, subject to the following conditions:
   *
   * The above copyright notice and this permission notice shall be included in
   * all copies or substantial portions of the Software.
   *
   * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
   * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
   * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
   * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
   * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
   * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
   * THE SOFTWARE.
    
   * Leslie Sanford
   * Email: jabberdabber@hotmail.com  

   ------------------------------------------------------------------------------

   DateTimePrecise adpatation:
   Copyright James Brock
  
   The Code Project Open License (CPOL):
        http://www.codeproject.com/info/cpol10.aspx

   ------------------------------------------------------------------------------

   Code Modification History:
   ------------------------------------------------------------------------------

\**********************************************************************************/

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Timers;

namespace TVA
{
    /// <summary>
    /// Represents information about the system's multimedia timer capabilities.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct TimerCapabilities
    {
        /// <summary>Minimum supported period in milliseconds.</summary>
        public int PeriodMinimum;

        /// <summary>Maximum supported period in milliseconds.</summary>
        public int PeriodMaximum;
    }

    /// <summary>
    /// Represents an exception that is thrown when a <see cref="PrecisionTimer"/> fails to start.
    /// </summary>
    [Serializable()]
    public class TimerStartException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TimerStartException"/> class.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        public TimerStartException(string message)
            : base(message)
        {
        }
    }

    /// <summary>
    /// Represents a high-resolution timer and timestamp class.
    /// </summary>
    /// <remarks>Implementation based on Windows multimedia timer.</remarks>
    public class PrecisionTimer : IDisposable
    {
        #region [ Members ]

        #region [ DateTimePrecise Adaptation ]

        /// <summary>
        /// This class provides a way to get a DateTime that exhibits the relative precision of
        /// System.Diagnostics.Stopwatch, and the absolute accuracy of DateTime.Now.
        /// </summary>
        /// <remarks>
        /// This class is based on James Brock's DateTimePrecise class which can be found on the Code Project:
        /// http://www.codeproject.com/KB/cs/DateTimePrecise.aspx?msg=2688543#xx2688543xx
        /// </remarks>
        private class PreciseTime
        {
            private sealed class ImmutableTimeState
            {
                public ImmutableTimeState(DateTime observedTime, DateTime baseTime, long elapsedTicks, long systemFrequency)
                {
                    this.ObservedTime = observedTime;
                    this.BaseTime = baseTime;
                    this.ElapsedTicks = elapsedTicks;
                    this.SystemFrequency = systemFrequency;
                }

                public readonly DateTime ObservedTime;
                public readonly DateTime BaseTime;
                public readonly long ElapsedTicks;
                public readonly long SystemFrequency;
            }

            private Stopwatch m_stopwatch;
            private ImmutableTimeState m_timeState;
            private long m_synchronizePeriodStopwatchTicks;
            private long m_synchronizePeriodClockTicks;

            /// <summary>Creates a new instance of DateTimePrecise.</summary>
            /// <remarks>
            /// A large value of synchronizePeriodSeconds may cause arithmetic overthrow
            /// exceptions to be thrown. A small value may cause the time to be unstable.
            /// A good value is 10.
            /// </remarks>
            /// <param name="synchronizePeriodSeconds">The number of seconds after which the class will synchronize itself with the system clock.</param>
            public PreciseTime(long synchronizePeriodSeconds)
            {
                m_stopwatch = Stopwatch.StartNew();
                m_stopwatch.Start();

                DateTime t = DateTime.UtcNow;
                m_timeState = new ImmutableTimeState(t, t, m_stopwatch.ElapsedTicks, Stopwatch.Frequency);

                m_synchronizePeriodStopwatchTicks = synchronizePeriodSeconds * Stopwatch.Frequency;
                m_synchronizePeriodClockTicks = synchronizePeriodSeconds * Ticks.PerSecond;
            }

            public DateTime UtcNow
            {
                get
                {
                    long elapsedTicks = m_stopwatch.ElapsedTicks;
                    ImmutableTimeState timeState = m_timeState;
                    DateTime precisionTime = timeState.BaseTime.AddTicks(((elapsedTicks - timeState.ElapsedTicks) * Ticks.PerSecond) / timeState.SystemFrequency);

                    if (elapsedTicks >= timeState.ElapsedTicks + m_synchronizePeriodStopwatchTicks)
                    {
                        // Perform clock resynchronization
                        DateTime systemTime = DateTime.UtcNow;

                        // Last parameter is a calculation that asymptotically approachs the measured system frequency
                        m_timeState = new ImmutableTimeState(systemTime, precisionTime, elapsedTicks, ((elapsedTicks - timeState.ElapsedTicks) * Ticks.PerSecond * 2) /
                            (systemTime.Ticks - timeState.ObservedTime.Ticks + systemTime.Ticks + systemTime.Ticks - precisionTime.Ticks - timeState.ObservedTime.Ticks));
                    }

                    // Return high-resolution timestamp
                    return precisionTime;
                }
            }
        }

        #endregion

        // Defines constants for the multimedia Timer's event types.
        private enum TimerMode
        {
            OneShot, // Timer event occurs once.
            Periodic // Timer event occurs periodically.
        }

        // Represents the method that is called by Windows when a timer event occurs.
        private delegate void TimerProc(int id, int msg, int user, int param1, int param2);

        /// <summary>
        /// Occurs when the <see cref="PrecisionTimer"/> has started.
        /// </summary>
        public event EventHandler Started;

        /// <summary>
        /// Occurs when the <see cref="PrecisionTimer"/> has stopped.
        /// </summary>
        public event EventHandler Stopped;

        /// <summary>
        /// Occurs when the <see cref="PrecisionTimer"/> period has elapsed.
        /// </summary>
        public event EventHandler Tick;

        // Fields
        private int m_timerID;              // Timer identifier.
        private TimerMode m_mode;           // Timer mode.
        private int m_period;               // Period between timer events in milliseconds.
        private int m_resolution;           // Timer resolution in milliseconds.
        private TimerProc m_timeProc;       // Called by Windows when a timer periodic event occurs.
        private bool m_running;             // Indicates whether or not the timer is running.
        private bool m_disposed;            // Indicates whether or not the timer has been disposed.
        private EventArgs m_eventArgs;      // Private user event args to pass into Ticks call

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Initializes a new instance of the <see cref="PrecisionTimer"/> class.
        /// </summary>
        public PrecisionTimer()
        {
            // Initialize timer with default values.
            m_mode = TimerMode.Periodic;
            m_period = Capabilities.PeriodMinimum;
            m_resolution = 1;
            m_running = false;
            m_timeProc = TimerEventCallback;
        }

        /// <summary>
        /// Releases the unmanaged resources before the <see cref="PrecisionTimer"/> object is reclaimed by <see cref="GC"/>.
        /// </summary>
        ~PrecisionTimer()
        {
            Dispose(false);
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the time between <see cref="Tick"/> events, in milliseconds.
        /// </summary>
        /// <exception cref="ObjectDisposedException">
        /// If the timer has already been disposed.
        /// </exception>
        public int Period
        {
            get
            {
                if (m_disposed)
                    throw new ObjectDisposedException("PrecisionTimer");

                return m_period;
            }
            set
            {
                if (m_disposed)
                    throw new ObjectDisposedException("PrecisionTimer");

                if (value < Capabilities.PeriodMinimum || value > Capabilities.PeriodMaximum)
                    throw new ArgumentOutOfRangeException("value", value, "Multimedia Timer period out of range.");

                m_period = value;

                if (IsRunning && m_mode == TimerMode.Periodic)
                {
                    Stop();
                    Start(m_eventArgs);
                }
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="PrecisionTimer"/> resolution, in milliseconds.
        /// </summary>
        /// <exception cref="ObjectDisposedException">
        /// If the timer has already been disposed.
        /// </exception>
        /// <remarks>
        /// The resolution is in milliseconds. The resolution increases  with smaller values;
        /// a resolution of 0 indicates periodic events  should occur with the greatest possible
        /// accuracy. To reduce system  overhead, however, you should use the maximum value
        /// appropriate for your application.
        /// </remarks>
        public int Resolution
        {
            get
            {
                if (m_disposed)
                    throw new ObjectDisposedException("PrecisionTimer");

                return m_resolution;
            }
            set
            {
                if (m_disposed)
                    throw new ObjectDisposedException("PrecisionTimer");

                if (value < 0)
                    throw new ArgumentOutOfRangeException("value", value, "Multimedia timer resolution out of range.");

                m_resolution = value;

                if (IsRunning && m_mode == TimerMode.Periodic)
                {
                    Stop();
                    Start(m_eventArgs);
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the <see cref="PrecisionTimer"/> should raise the
        /// <see cref="Tick"/> event each time the specified period elapses or only after the first
        /// time it elapses.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <returns>
        /// <c>true</c>true if the <see cref="PrecisionTimer"/> should raise the <see cref="Ticks"/>
        /// event each time the interval elapses; <c>false</c> if it should raise the event only once
        /// after the first time the interval elapses. The default is <c>true</c>.
        /// </returns>
        public bool AutoReset
        {
            get
            {
                if (m_disposed)
                    throw (new ObjectDisposedException("PrecisionTimer"));

                return (m_mode == TimerMode.Periodic);
            }
            set
            {
                if (m_disposed)
                    throw (new ObjectDisposedException("PrecisionTimer"));

                m_mode = (value ? TimerMode.Periodic : TimerMode.OneShot);

                if (IsRunning && m_mode == TimerMode.Periodic)
                {
                    Stop();
                    Start(m_eventArgs);
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="PrecisionTimer"/> is running.
        /// </summary>
        public bool IsRunning
        {
            get
            {
                return m_running;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Releases all the resources used by the <see cref="PrecisionTimer"/> object.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="PrecisionTimer"/> object and optionally releases the managed resources.
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
                        if (IsRunning)
                            Stop();
                    }
                }
                finally
                {
                    m_disposed = true;  // Prevent duplicate dispose.
                }
            }
        }

        /// <summary>
        /// Starts the <see cref="PrecisionTimer"/>.
        /// </summary>
        /// <exception cref="ObjectDisposedException">
        /// The timer has already been disposed.
        /// </exception>
        /// <exception cref="TimerStartException">
        /// The timer failed to start.
        /// </exception>
        public void Start()
        {
            Start(EventArgs.Empty);
        }

        /// <summary>
        /// Starts the <see cref="PrecisionTimer"/> with the specified <see cref="EventArgs"/>.
        /// </summary>
        /// <param name="userArgs">User defined event arguments to pass into raised <see cref="Ticks"/> event.</param>
        /// <exception cref="ObjectDisposedException">
        /// The timer has already been disposed.
        /// </exception>
        /// <exception cref="TimerStartException">
        /// The timer failed to start.
        /// </exception>
        public void Start(EventArgs userArgs)
        {
            if (m_disposed)
                throw new ObjectDisposedException("PrecisionTimer");

            if (m_running) return;

            // Cache user event args to pass into Ticks paramter
            m_eventArgs = userArgs;

            // Create and start timer.
            m_timerID = timeSetEvent(m_period, m_resolution, m_timeProc, IntPtr.Zero, m_mode);

            // If the timer was created successfully.
            if (m_timerID != 0)
            {
                m_running = true;

                if (Started != null)
                    Started(this, EventArgs.Empty);
            }
            else
            {
                throw new TimerStartException("Unable to start multimedia Timer.");
            }
        }

        /// <summary>
        /// Stops <see cref="PrecisionTimer"/>.
        /// </summary>
        /// <exception cref="ObjectDisposedException">
        /// If the timer has already been disposed.
        /// </exception>
        public void Stop()
        {
            if (m_disposed)
                throw new ObjectDisposedException("PrecisionTimer");

            if (!m_running) return;

            // Stop and destroy timer.
            timeKillEvent(m_timerID);
            m_timerID = 0;
            m_running = false;

            if (Stopped != null)
                Stopped(this, EventArgs.Empty);
        }

        // Callback method called by the Win32 multimedia timer when a timer event occurs.
        private void TimerEventCallback(int id, int msg, int user, int param1, int param2)
        {
            if (Tick != null)
                Tick(this, m_eventArgs);

            if (m_mode == TimerMode.OneShot)
                Stop();
        }

        #endregion

        #region [ Static ]

        // Static Fields
        private static TimerCapabilities m_capabilities;    // Multimedia timer capabilities.
        private static PreciseTime m_preciseTime;           // Precise time implementation.
        private static Timer m_synchronizer;                // Lightweight timer used for precise time synchronization.

        // Static Constructor
        static PrecisionTimer()
        {
            // Get multimedia timer capabilities
            timeGetDevCaps(ref m_capabilities, Marshal.SizeOf(m_capabilities));
        }

        // Static Properties

        /// <summary>
        /// Gets a high-resolution <see cref="DateTime"/> value of the current time on this computer,
        /// expressed in Coordinated Universal Time (UTC).
        /// </summary>
        /// <remarks>
        /// <para>
        /// This shared property provides a way to get a <see cref="DateTime"/> value that exhibits the relative
        /// precision of <see cref="Stopwatch"/>, and the absolute accuracy of <see cref="DateTime.UtcNow"/>.
        /// </para>
        /// <para>
        /// This property is useful for obtaining high-resolution accuarate timestamps for events that occur in the
        /// "sub-second" world (e.g., timestamping events happening hundreds or thousands of times per second).
        /// Note that the normal <see cref="DateTime.UtcNow"/> property has a maximum resolution of ~16 milliseconds.
        /// </para>
        /// </remarks>
        public static DateTime UtcNow
        {
            get
            {
                // Setup a new precise time class at first call
                if (m_preciseTime == null)
                    InitializePreciseTime();

                return m_preciseTime.UtcNow;
            }
        }

        /// <summary>
        /// Gets a high-resolution <see cref="DateTime"/> value of the current time on this computer,
        /// expressed in the local time zone.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This shared property provides a way to get a <see cref="DateTime"/> value that exhibits the relative
        /// precision of <see cref="Stopwatch"/>, and the absolute accuracy of <see cref="DateTime.Now"/>.
        /// </para>
        /// <para>
        /// This property is useful for obtaining high-resolution accuarate timestamps for events that occur in the
        /// "sub-second" world (e.g., timestamping events happening hundreds or thousands of times per second).
        /// Note that the normal <see cref="DateTime.Now"/> property has a maximum resolution of ~16 milliseconds.
        /// </para>
        /// </remarks>
        public static DateTime Now
        {
            get
            {
                return UtcNow.ToLocalTime();
            }
        }

        /// <summary>
        /// Gets the system multimedia timer capabilities.
        /// </summary>
        public static TimerCapabilities Capabilities
        {
            get
            {
                return m_capabilities;
            }
        }

        // Static Methods

        // Initializes the the precise timing mechanism
        private static void InitializePreciseTime()
        {
            // We just use the recommended synchronization period for general purpose use
            const int synchronizationPeriod = 10;

            // Create a new precise time class
            m_preciseTime = new PreciseTime(synchronizationPeriod);

            // We setup a lightweight timer that will make sure precise time mechanism gets
            // called regularly, in case user doesn't, so it can maintain synchronization
            m_synchronizer = new Timer(synchronizationPeriod * 1000.0D);
            m_synchronizer.Elapsed += m_synchronizer_Elapsed;
            m_synchronizer.Start();
        }

        // We make sure and call PreciseTime.UtcNow regularly so it can maintain synchronization
        private static void m_synchronizer_Elapsed(object sender, ElapsedEventArgs e)
        {
            DateTime now = m_preciseTime.UtcNow;
        }

        // Gets timer capabilities.
        [DllImport("winmm.dll")]
        private static extern int timeGetDevCaps(ref TimerCapabilities caps, int sizeOfTimerCaps);

        // Creates and starts the timer.
        [DllImport("winmm.dll")]
        private static extern int timeSetEvent(int delay, int resolution, TimerProc proc, IntPtr user, TimerMode mode);

        // Stops and destroys the timer.
        [DllImport("winmm.dll")]
        private static extern int timeKillEvent(int id);

        #endregion
    }
}