//*******************************************************************************************************
//  MultiSourceFrameImageParserBase.cs - Gbtc
//
//  Tennessee Valley Authority, 2009
//  No copyright is claimed pursuant to 17 USC § 105.  All Other Rights Reserved.
//
//  This software is made freely available under the TVA Open Source Agreement (see below).
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  12/03/2008 - J. Ritchie Carroll
//       Generated original version of source code.
//  06/19/2009 - Pinal C. Patel
//       Parsed output is now being delivered in a new collection instead of reusing a single collection.
//  08/07/2009 - Josh L. Patterson
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

#pragma warning disable 0809

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using TVA.Collections;

namespace TVA.Parsing
{
    /// <summary>
    /// This class defines a basic implementation of parsing functionality suitable for automating the parsing of multiple
    /// binary data streams, each represented as frames with common headers and returning the parsed data via an event.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This class is more specific than the <see cref="BinaryImageParserBase"/> in that it can automate the parsing of a
    /// particular protocol that is formatted as a series of frames that have a common method of identification.
    /// Automation of type creation occurs by loading implementations of common types that implement the
    /// <see cref="ISupportFrameImage{TTypeIdentifier}"/> interface. The common method of identification is handled by
    /// creating a class derived from the <see cref="ICommonHeader{TTypeIdentifier}"/> which primarily includes a
    /// TypeID property, but also should include any state information needed to parse a particular frame if necessary.
    /// Derived classes override the <see cref="FrameImageParserBase{TTypeIdentifier, TOutputType}.ParseCommonHeader"/>
    /// method in order to parse the <see cref="ICommonHeader{TTypeIdentifier}"/> from a provided binary image.
    /// </para>
    /// </remarks>
    /// <typeparam name="TSourceIdentifier">Type of identifier for the data source.</typeparam>
    /// <typeparam name="TTypeIdentifier">Type of identifier used to distinguish output types.</typeparam>
    /// <typeparam name="TOutputType">Type of the interface or class used to represent outputs.</typeparam>
    public abstract class MultiSourceFrameImageParserBase<TSourceIdentifier, TTypeIdentifier, TOutputType> : FrameImageParserBase<TTypeIdentifier, TOutputType> where TOutputType : ISupportFrameImage<TTypeIdentifier>
    {
        #region [ Members ]

        // Events

        /// <summary>
        /// Occurs when a data image is deserialized successfully to one or more objects of the <see cref="Type"/> 
        /// that the data image was for.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T1,T2}.Argument1"/> is the ID of the source for the data image.<br/>
        /// <see cref="EventArgs{T1,T2}.Argument2"/> is a list of objects deserialized from the data image.
        /// </remarks>
        [Description("Occurs when a data image is deserialized successfully to one or more object of the Type that the data image was for.")]
        public new event EventHandler<EventArgs<TSourceIdentifier, IList<TOutputType>>> DataParsed;

        // Fields
        private ProcessQueue<IdentifiableItem<TSourceIdentifier, byte[]>> m_bufferQueue;
        private Dictionary<TSourceIdentifier, bool> m_sourceInitialized;
        private Dictionary<TSourceIdentifier, byte[]> m_unparsedBuffers;
        private List<TOutputType> m_parsedOutputs;
        private TSourceIdentifier m_sourceID;
        private bool m_disposed;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new instance of the <see cref="MultiSourceFrameImageParserBase{TSourceIdentifier,TTypeIdentifier,TOutputType}"/> class.
        /// </summary>
        protected MultiSourceFrameImageParserBase()
        {
            m_bufferQueue = CreateBufferQueue();
            m_bufferQueue.ProcessException += ProcessExceptionHandler;
            
            if (ProtocolUsesSyncBytes)
                m_sourceInitialized = new Dictionary<TSourceIdentifier, bool>();

            m_unparsedBuffers = new Dictionary<TSourceIdentifier, byte[]>();

            // We attach to base class events so we can cumulate outputs per data source and handle data errors
            base.DataParsed += CumulateParsedOutput;
            base.DataDiscarded += ResetDataSourceInitialization;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets the total number of buffers that are currently queued for processing, if any.
        /// </summary>
        public virtual int QueuedBuffers
        {
            get
            {
                if (m_bufferQueue != null)
                    return m_bufferQueue.Count;
                else
                    return 0;
            }
        }

        /// <summary>
        /// Gets the current run-time statistics of the <see cref="MultiSourceFrameImageParserBase{TSourceIdentifier,TTypeIdentifier,TOutputType}"/>.
        /// </summary>
        public virtual ProcessQueueStatistics CurrentStatistics
        {
            get
            {
                return m_bufferQueue.CurrentStatistics;
            }
        }

        /// <summary>
        /// Gets current status of <see cref="MultiSourceFrameImageParserBase{TSourceIdentifier,TTypeIdentifier,TOutputType}"/>.
        /// </summary>
        public override string Status
        {
            get
            {
                StringBuilder status = new StringBuilder();

                status.Append(base.Status);
                status.Append(m_bufferQueue.Status);

                return status.ToString();
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="MultiSourceFrameImageParserBase{TSourceIdentifier,TTypeIdentifier,TOutputType}"/> object and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (!m_disposed)
            {
                try
                {
                    if (disposing)
                    {
                        if (m_bufferQueue != null)
                        {
                            m_bufferQueue.ProcessException -= ProcessExceptionHandler;
                            m_bufferQueue.Dispose();
                        }
                        m_bufferQueue = null;

                        if (m_sourceInitialized != null)
                        {
                            m_sourceInitialized.Clear();
                        }
                        m_sourceInitialized = null;

                        if (m_unparsedBuffers != null)
                        {
                            m_unparsedBuffers.Clear();
                        }
                        m_unparsedBuffers = null;

                        // Detach from base class events
                        base.DataParsed -= CumulateParsedOutput;
                        base.DataDiscarded -= ResetDataSourceInitialization;
                    }
                }
                finally
                {
                    base.Dispose(disposing);    // Call base class Dispose().
                    m_disposed = true;          // Prevent duplicate dispose.
                }
            }
        }

        /// <summary>
        /// Start the streaming data parser.
        /// </summary>
        public override void Start()
        {
            base.Start();

            if (m_sourceInitialized != null)
                m_sourceInitialized.Clear();

            m_unparsedBuffers.Clear();
            m_bufferQueue.Start();
        }

        /// <summary>
        /// Stops the streaming data parser.
        /// </summary>
        public override void Stop()
        {
            m_bufferQueue.Stop();
            base.Stop();
        }

        /// <summary>
        /// Queues a sequence of bytes, from the specified data source, onto the stream for parsing.
        /// </summary>
        /// <param name="source">ID of the data source.</param>
        /// <param name="buffer">An array of bytes to queue for parsing</param>
        public virtual void Parse(TSourceIdentifier source, byte[] buffer)
        {
            m_bufferQueue.Add(new IdentifiableItem<TSourceIdentifier, byte[]>(source, buffer));
        }

        /// <summary>
        /// Queues a sequence of bytes, from the specified data source, onto the stream for parsing.
        /// </summary>
        /// <param name="source">ID of the data source.</param>
        /// <param name="buffer">An array of bytes. This method copies count bytes from buffer to the queue.</param>
        /// <param name="offset">The zero-based byte offset in buffer at which to begin copying bytes to the current stream.</param>
        /// <param name="count">The number of bytes to be written to the current stream.</param>
        public virtual void Parse(TSourceIdentifier source, byte[] buffer, int offset, int count)
        {
            m_bufferQueue.Add(new IdentifiableItem<TSourceIdentifier, byte[]>(source, buffer.BlockCopy(offset, count)));
        }

        /// <summary>
        /// Queues the object implementing the <see cref="ISupportBinaryImage"/> interface, from the specified data source, onto the stream for parsing.
        /// </summary>
        /// <param name="source">ID of the data source.</param>
        /// <param name="image">Object to be parsed that implements the <see cref="ISupportBinaryImage"/> interface.</param>
        /// <remarks>
        /// This method takes the binary image from <see cref="ISupportBinaryImage"/> and writes the buffer to the <see cref="BinaryImageParserBase"/> stream for parsing.
        /// </remarks>
        public virtual void Parse(TSourceIdentifier source, ISupportBinaryImage image)
        {
            Parse(source, image.BinaryImage);
        }

        /// <summary>
        /// Clears the internal buffer of unparsed data received from the specified <paramref name="source"/>.
        /// </summary>
        /// <param name="source">ID of the data source.</param>
        /// <remarks>
        /// This method can be used to ensure that partial data received from the <paramref name="source"/> is not kept in memory indefinitely.
        /// </remarks>
        public virtual void PurgeBuffer(TSourceIdentifier source)
        {
            lock (m_unparsedBuffers)
            {
                if (m_unparsedBuffers.ContainsKey(source))
                    m_unparsedBuffers.Remove(source);
            }
        }

        /// <summary>
        /// Not implemented. Consumers should call the <see cref="Parse(TSourceIdentifier,ISupportBinaryImage)"/> method instead to make sure data source ID gets tracked with data buffer.
        /// </summary>
        /// <exception cref="NotImplementedException">This method should not be called directly.</exception>
        /// <param name="image">A <see cref="ISupportBinaryImage"/>.</param>
        [EditorBrowsable(EditorBrowsableState.Never), Obsolete("MultiSourceFrameParser requires consumers call Parse overload that takes source identifier as an argument", true)]
        public override void Parse(ISupportBinaryImage image)
        {
            throw new NotImplementedException("This method should not be called directly, call the Parse(TSourceIdentifier,ISupportBinaryImage) method to queue data for parsing instead.");
        }

        /// <summary>
        /// Not implemented. Consumers should call the <see cref="Parse(TSourceIdentifier,byte[],int,int)"/> method instead to make sure data source ID gets tracked with data buffer.
        /// </summary>
        /// <exception cref="NotImplementedException">This method should not be called directly.</exception>
        /// <param name="buffer">A <see cref="Byte"/> array.</param>
        /// <param name="count">An <see cref="Int32"/> for the offset.</param>
        /// <param name="offset">An <see cref="Int32"/> for the count.</param>
        [EditorBrowsable(EditorBrowsableState.Never), Obsolete("MultiSourceFrameParser requires consumers call Parse overload that takes source identifier as an argument", true)]
        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException("This method should not be called directly, call the Parse(TSourceIdentifier,byte[],int,int) method to queue data for parsing instead.");
        }

        /// <summary>
        /// Clears all buffers for this stream and causes any buffered data to be parsed immediately.
        /// </summary>
        /// <remarks>
        /// <para>
        /// If the user has called <see cref="Start"/> method, this method will process all remaining buffers on the calling thread until all
        /// queued buffers have been parsed - the <see cref="MultiSourceFrameImageParserBase{TSourceIdentifier,TTypeIdentifier,TOutputType}"/>
        /// will then be automatically stopped. This method is typically called on shutdown to make sure any remaining queued buffers get
        /// parsed before the class instance is destructed.
        /// </para>
        /// <para>
        /// It is possible for items to be queued while the flush is executing. The flush will continue to parse buffers as quickly
        /// as possible until the internal buffer queue is empty. Unless the user stops queueing data to be parsed (i.e. calling the
        /// <see cref="Parse(TSourceIdentifier,byte[])"/> method), the flush call may never return (not a happy situtation on shutdown).
        /// </para>
        /// <para>
        /// The <see cref="MultiSourceFrameImageParserBase{TSourceIdentifier,TTypeIdentifier,TOutputType}"/> does not clear queue prior to destruction.
        /// If the user fails to call this method before the class is destructed, there may be data that remains unparsed in the internal buffer.
        /// </para>
        /// </remarks>
        public override void Flush()
        {
            m_bufferQueue.Flush();
        }

        /// <summary>
        /// Creates the internal buffer queue.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This method is virtual to allow derived classes to customize the style of processing queue used when consumers
        /// choose to implement an internal buffer queue.  Default type is a real-time queue with the default settings.
        /// When overriding, use the <see cref="ParseQueuedBuffers"/> method for the <see cref="ProcessQueue{T}"/>) item
        /// processing delegate.
        /// </para>
        /// <para>
        /// Note that current design only supports synchronous parsing - consumer overriding this method to return
        /// an asynchronous (i.e., multi-threaded) process queue will need to redesign the processing delegate.
        /// </para>
        /// </remarks>
        /// <returns>New internal buffer processing queue (i.e., a new <see cref="ProcessQueue{T}"/>).</returns>
        protected virtual ProcessQueue<IdentifiableItem<TSourceIdentifier, byte[]>> CreateBufferQueue()
        {
            return ProcessQueue<IdentifiableItem<TSourceIdentifier, byte[]>>.CreateRealTimeQueue(ParseQueuedBuffers);
        }

        /// <summary>
        /// This method is used by the internal <see cref="ProcessQueue{T}"/> to process all queued data buffers.
        /// </summary>
        /// <param name="buffers">Source identifiable buffers to process.</param>
        /// <remarks>
        /// This is the item processing delegate to use when overriding the <see cref="CreateBufferQueue"/> method.
        /// </remarks>
        protected virtual void ParseQueuedBuffers(IdentifiableItem<TSourceIdentifier, byte[]>[] buffers)
        {
            byte[] buffer;

            // Process all queued data buffers...
            foreach (IdentifiableItem<TSourceIdentifier, byte[]> item in buffers)
            {
                m_sourceID = item.ID;
                buffer = item.Item;

                // Check to see if this data source has been initialized
                if (m_sourceInitialized != null && !m_sourceInitialized.TryGetValue(m_sourceID, out StreamInitialized))
                    m_sourceInitialized.Add(m_sourceID, true);

                // Restore any unparsed buffers for this data source, if any
                lock (m_unparsedBuffers)
                {
                    if (!m_unparsedBuffers.TryGetValue(m_sourceID, out UnparsedBuffer))
                        m_unparsedBuffers.Add(m_sourceID, null);
                }

                // Create new collection for parsed output
                m_parsedOutputs = new List<TOutputType>();

                // Start parsing sequence for this buffer - this will cumulate new parsed outputs
                base.Write(buffer, 0, buffer.Length);

                // Track last unparsed buffer for this data source
                lock (m_unparsedBuffers)
                {
                    m_unparsedBuffers[m_sourceID] = UnparsedBuffer;
                }

                // Expose any parsed data
                if (m_parsedOutputs.Count > 0)
                    OnDataParsed(m_sourceID, m_parsedOutputs);
            }
        }

        /// <summary>
        /// Raises the <see cref="DataParsed"/> event.
        /// </summary>
        /// <param name="sourceID">Data source ID.</param>
        /// <param name="parsedData">List of parsed events.</param>
        protected virtual void OnDataParsed(TSourceIdentifier sourceID, List<TOutputType> parsedData)
        {            
            if (DataParsed != null)
                DataParsed(this, new EventArgs<TSourceIdentifier, IList<TOutputType>>(sourceID, parsedData));
        }

        // Cumulate output data for current data source
        private void CumulateParsedOutput(object sender, EventArgs<TOutputType> data)
        {
            m_parsedOutputs.Add(data.Argument);
        }

        // If an error occurs during parsing from a data source, we reset its initialization state
        private void ResetDataSourceInitialization(object sender, EventArgs<byte[]> data)
        {
            if (m_sourceInitialized != null)
                m_sourceInitialized[m_sourceID] = false;
        }

        // We just bubble any exceptions captured in process queue out to parsing exception event...
        private void ProcessExceptionHandler(object sender, EventArgs<Exception> e)
        {
            OnParsingException(e.Argument);
        }

        #endregion
    }
}
