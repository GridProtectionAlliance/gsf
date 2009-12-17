//*******************************************************************************************************
//  FrameQueue.cs - Gbtc
//
//  Tennessee Valley Authority, 2009
//  No copyright is claimed pursuant to 17 USC § 105.  All Other Rights Reserved.
//
//  This software is made freely available under the TVA Open Source Agreement (see below).
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  11/01/2007 - J. Ritchie Carroll
//       Initial version of source generated.
//  09/15/2008 - J. Ritchie Carroll
//       Converted to C#.
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

using System;
using System.Collections.Generic;

namespace TVA.Measurements
{
    /// <summary>
    /// Represents a real-time queue of <see cref="IFrame"/> instances used by the <see cref="ConcentratorBase"/> class.
    /// </summary>
    [CLSCompliant(false)]
    public class FrameQueue : IDisposable
    {            
        #region [ Members ]

        // Fields
        private ConcentratorBase m_parent;              // Reference to parent concentrator instance
        private LinkedList<IFrame> m_frameList;         // We keep this list sorted by timestamp so frames are processed in order
        private Dictionary<long, IFrame> m_frameHash;   // This list not guaranteed to be sorted, but used for fast frame lookup
        private long m_publishedTicks;                  // Timstamp of last published frame
        private IFrame m_head;                          // Reference to current top of the frame collection
        private IFrame m_last;                          // Reference to last published frame
        private long m_timeResolution;                  // Cached time resolution (max sorting resolution in ticks)
        private double m_ticksPerFrame;                 // Cached ticks per frame
        private bool m_disposed;                        // Object disposed flag

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="FrameQueue"/>.
        /// </summary>
        /// <param name="parent">Reference to parent concentrator instance.</param>
        internal FrameQueue(ConcentratorBase parent)
        {
            // Calculate initial dictionary capacity based on concentrator specifications
            int initialCapacity = (int)((1.0D + parent.LagTime + parent.LeadTime) * parent.FramesPerSecond);

            m_parent = parent;
            m_frameList = new LinkedList<IFrame>();
            m_frameHash = new Dictionary<long, IFrame>(initialCapacity);
            m_ticksPerFrame = parent.TicksPerFrame;
        }

        /// <summary>
        /// Releases the unmanaged resources before the <see cref="FrameQueue"/> object is reclaimed by <see cref="GC"/>.
        /// </summary>
        ~FrameQueue()
        {
            Dispose(false);
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets number of ticks per frame to be used by <see cref="FrameQueue"/>.
        /// </summary>
        public double TicksPerFrame
        {
            get
            {
                return m_ticksPerFrame;
            }
            set
            {
                m_ticksPerFrame = value;
            }
        }

        /// <summary>
        /// Gets or sets the maximum time resolution to use when sorting measurements by timestamps into their proper destination frame.
        /// </summary>
        public long TimeResolution
        {
            get
            {
                return m_timeResolution;
            }
            set
            {
                m_timeResolution = value;
            }
        }

        /// <summary>
        /// Returns the next <see cref="IFrame"/> in the <see cref="FrameQueue"/>, if any.
        /// </summary>
        /// <remarks>
        /// This property is tracked separately from the internal <see cref="IFrame"/> collection, as a
        /// result this property may be called at any time without a locking penalty.
        /// </remarks>
        public IFrame Head
        {
            get
            {
                // We track the head separately to avoid sync-lock on frame list to safely access first item...
                return m_head;
            }
        }

        /// <summary>
        /// Gets the last processed <see cref="IFrame"/> in the <see cref="FrameQueue"/>.
        /// </summary>
        /// <remarks>
        /// This property is tracked separately from the internal <see cref="IFrame"/> collection, as a
        /// result this property may be called at any time without a locking penalty.
        /// </remarks>
        public IFrame Last
        {
            get
            {
                return m_last;
            }
        }

        /// <summary>
        /// Returns the total number of <see cref="IFrame"/>'s currently in the <see cref="FrameQueue"/>.
        /// </summary>
        public int Count
        {
            get
            {
                return m_frameList.Count;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Releases all the resources used by the <see cref="FrameQueue"/> object.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="FrameQueue"/> object and optionally releases the managed resources.
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
                        if (m_frameList != null)
                            m_frameList.Clear();

                        m_frameList = null;

                        if (m_frameHash != null)
                            m_frameHash.Clear();

                        m_frameHash = null;

                        m_parent = null;
                        m_head = null;
                        m_last = null;
                    }
                }
                finally
                {
                    m_disposed = true;  // Prevent duplicate dispose.
                }
            }
        }

        /// <summary>
        /// Clears the <see cref="FrameQueue"/>.
        /// </summary>
        public void Clear()
        {
            lock (m_frameList)
            {
                if (m_frameList != null)
                    m_frameList.Clear();

                if (m_frameHash != null)
                    m_frameHash.Clear();
            }
        }

        /// <summary>
        /// Removes current <see cref="Head"/> frame from the <see cref="FrameQueue"/> after it has been processed and assigns a new <see cref="Head"/>.
        /// </summary>
        public void Pop()
        {
            // We track latest published ticks - don't want to allow slow moving measurements
            // to inject themselves after a certain publication timeframe has passed - this
            // avoids any possible out-of-sequence frame publication...
            m_last = m_head;
            m_head = null;
            m_publishedTicks = m_last.Timestamp;

            // Assign next node, if any, as quickly as possible. Still have to wait for queue
            // lock - tick-tock, time's-a-wastin' and user function needs a frame to publish.
            lock (m_frameList)
            {
                LinkedListNode<IFrame> nextNode = m_frameList.First.Next;

                // If next frame is available, go ahead and assign it...
                if (nextNode != null)
                    m_head = nextNode.Value;

                // Clean up frame queues
                m_frameList.RemoveFirst();
                m_frameHash.Remove(m_publishedTicks);
            }
        }

        /// <summary>
        /// Gets <see cref="IFrame"/> from the queue with the specified timestamp, in ticks.  If no <see cref="IFrame"/> exists for
        /// the specified timestamp, one will be created.
        /// </summary>
        /// <param name="ticks">Timestamp, in ticks, for which to get or create <see cref="IFrame"/>.</param>
        /// <remarks>
        /// Ticks can be any point in time so long time requested is greater than time of last published frame; this queue
        /// is used in a real-time scenario with time moving forward.  If a frame is requested for an old timestamp, null
        /// will be returned. Note that frame returned will be "best-fit" for given timestamp based on the number of 
        /// <see cref="ConcentratorBase.FramesPerSecond"/> of the parent <see cref="ConcentratorBase"/> implementation.
        /// </remarks>
        /// <returns>An existing or new <see cref="IFrame"/> from the queue for the specified timestamp.</returns>
        public IFrame GetFrame(long ticks)
        {
            // Calculate destination ticks for this frame
            long baseTicks, resolutionTicks, destinationTicks;
            IFrame frame = null;
            bool nodeAdded = false;

            // Baseline timestamp to the top of the second
            baseTicks = ticks - ticks % Ticks.PerSecond;

            // See if a maximum time resolution was specified
            if (m_timeResolution > 1)
            {
                // Truncate timestamp to time resolution (i.e., remove fractional time)
                resolutionTicks = baseTicks + ((ticks - baseTicks) / m_timeResolution) * m_timeResolution;

                // Align timestamp to nearest frame (i.e., put timestamp in the correct bucket)
                destinationTicks = baseTicks + (long)(Math.Ceiling((resolutionTicks - baseTicks) / m_ticksPerFrame) * m_ticksPerFrame);
            }
            else
                destinationTicks = ticks;


            // Make sure ticks are newer than latest published ticks...
            if (destinationTicks > m_publishedTicks)
            {
                // Wait for queue lock - we wait because calling function demands a destination frame
                lock (m_frameList)
                {
                    // See if requested frame is already available...
                    if (m_frameHash.TryGetValue(destinationTicks, out frame))
                        return frame;

                    // Didn't find frame for this timestamp so we create one
                    frame = m_parent.CreateNewFrame(destinationTicks);

                    if (m_frameList.Count > 0)
                    {
                        // Insert frame into proper sorted position...
                        LinkedListNode<IFrame> node = m_frameList.Last;

                        do
                        {
                            if (destinationTicks > node.Value.Timestamp)
                            {
                                m_frameList.AddAfter(node, frame);
                                nodeAdded = true;
                                break;
                            }

                            node = node.Previous;
                        }
                        while (node != null);
                    }

                    if (!nodeAdded)
                    {
                        m_frameList.AddFirst(frame);
                        m_head = frame;
                    }

                    // Since we'll be requesting this frame over and over, we'll use
                    // a hash table for quick frame lookups by timestamp
                    m_frameHash.Add(destinationTicks, frame);
                }
            }

            return frame;
        }

        #endregion
    }
}
