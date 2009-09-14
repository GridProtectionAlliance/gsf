//*******************************************************************************************************
//  BinaryImageParserBase.cs - Gbtc
//
//  Tennessee Valley Authority, 2009
//  No copyright is claimed pursuant to 17 USC § 105.  All Other Rights Reserved.
//
//  This software is made freely available under the TVA Open Source Agreement (see below).
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  02/12/2007 - J. Ritchie Carroll
//       Generated original version of source code.
//  08/07/2009 - Josh L. Patterson
//       Edited Comments.
//  9/14/2009 - Stephen C. Wills
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
using System.ComponentModel;
using System.IO;
using System.Text;
using TVA.Units;

namespace TVA.Parsing
{
    /// <summary>
    /// This class defines the fundamental functionality for parsing any stream of binary data.
    /// </summary>
    /// <remarks>
    /// This parser is designed as a write-only stream such that data can come from any source.
    /// </remarks>
    public abstract class BinaryImageParserBase : Stream, IBinaryImageParser
    {
        #region [ Members ]

        /// <summary>
        /// Specifies the default value for the <see cref="ProtocolSyncBytes"/> property.
        /// </summary>
        public static readonly byte[] DefaultProtocolSyncBytes = new byte[] { 0xAA };

        // Events

        /// <summary>
        /// Occurs when data image fails deserialized due to an exception.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T}.Argument"/> is the remaining portion of the binary image that failed to parse.
        /// </remarks>
        public event EventHandler<EventArgs<byte[]>> DataDiscarded;

        /// <summary>
        /// Occurs when an <see cref="Exception"/> is encountered while attempting to parse data.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T}.Argument"/> is the <see cref="Exception"/> encountered while parsing data.
        /// </remarks>
        public event EventHandler<EventArgs<Exception>> ParsingException;

        // Fields
        
        /// <summary>
        /// Tracks if data stream has been initialized.
        /// </summary>
        /// <remarks>
        /// Only relevant if <see cref="ProtocolUsesSyncBytes"/> is true.
        /// </remarks>
        protected bool StreamInitialized;

        /// <summary>
        /// Remaining unparsed buffer from last parsing execution, if any.
        /// </summary>
        protected byte[] UnparsedBuffer;

        private byte[] m_protocolSyncBytes;
        private long m_buffersProcessed;
        private string m_name;
        private Ticks m_startTime;
        private Ticks m_stopTime;
        private bool m_enabled;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new instance of the <see cref="BinaryImageParserBase"/> class.
        /// </summary>
        protected BinaryImageParserBase()
	    {
            m_protocolSyncBytes = DefaultProtocolSyncBytes;
            m_name = this.GetType().Name;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets a boolean value that indicates whether the data parser is currently enabled.
        /// </summary>
        /// <remarks>
        /// Setting <see cref="Enabled"/> to true will start the <see cref="BinaryImageParserBase"/> if it is not started,
        /// setting to false will stop the <see cref="BinaryImageParserBase"/> if it is started.
        /// </remarks>
        public virtual bool Enabled
        {
            get
            {
                return m_enabled;
            }
            set
            {
                if (value && !m_enabled)
                    Start();
                else if (!value && m_enabled)
                    Stop();
            }
        }

        /// <summary>
        /// Gets flag that determines if this protocol parsing implementation uses synchronization bytes.
        /// </summary>
        public abstract bool ProtocolUsesSyncBytes
        {
            get;
        }

        /// <summary>
        /// Gets or sets synchronization bytes for this parsing implementation, if used.
        /// </summary>
        public virtual byte[] ProtocolSyncBytes
        {
            get
            {
                return m_protocolSyncBytes;
            }
            set
            {
                m_protocolSyncBytes = value;
            }
        }

        /// <summary>
        /// Gets the total amount of time, in seconds, that the <see cref="BinaryImageParserBase"/> has been active.
        /// </summary>
        public Time RunTime
        {
            get
            {
                Ticks processingTime = 0;

                if (m_startTime > 0)
                {
                    if (m_stopTime > 0)
                        processingTime = m_stopTime - m_startTime;
                    else
                        processingTime = DateTime.Now.Ticks - m_startTime;
                }

                if (processingTime < 0)
                    processingTime = 0;

                return processingTime.ToSeconds();
            }
        }

        /// <summary>
        /// Gets the total number of buffer images processed so far.
        /// </summary>
        /// <returns>Total number of buffer images processed so far.</returns>
        public long TotalProcessedBuffers
        {
            get
            {
                return m_buffersProcessed;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the current stream supports reading.
        /// </summary>
        /// <remarks>
        /// The <see cref="BinaryImageParserBase"/> is implemented as a WriteOnly stream, so this defaults to false.
        /// </remarks>
        public override bool CanRead
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the current stream supports seeking.
        /// </summary>
        /// <remarks>
        /// The <see cref="BinaryImageParserBase"/> is implemented as a WriteOnly stream, so this defaults to false.
        /// </remarks>
        public override bool CanSeek
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the current stream supports writing.
        /// </summary>
        /// <remarks>
        /// The <see cref="BinaryImageParserBase"/> is implemented as a WriteOnly stream, so this defaults to true.
        /// </remarks>
        public override bool CanWrite
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Gets current status of <see cref="BinaryImageParserBase"/>.
        /// </summary>
        public virtual string Status
        {
            get
            {
                StringBuilder status = new StringBuilder();

                status.Append("      Current parser state: ");
                status.Append(m_enabled ? "Active" : "Idle");
                status.AppendLine();

                if (ProtocolUsesSyncBytes)
                {
                    status.Append("Data synchronization bytes: 0x");
                    status.Append(ByteEncoding.Hexadecimal.GetString(ProtocolSyncBytes));
                    status.AppendLine();
                }

                status.Append("     Total parser run-time: ");
                status.Append(RunTime.ToString());
                status.AppendLine();
                status.Append("   Total buffers processed: ");
                status.Append(m_buffersProcessed);
                status.AppendLine();

                return status.ToString();
            }
        }

        /// <summary>
        /// Gets the name of <see cref="BinaryImageParserBase"/>.
        /// </summary>
        public virtual string Name
        {
            get
            {
                return m_name;
            }
            set
            {
                m_name = value;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Start the streaming data parser.
        /// </summary>
        public virtual void Start()
        {
            // Reset statistics
            m_buffersProcessed = 0;
            m_stopTime = 0;
            m_startTime = DateTime.Now.Ticks;

            // Initialized state depends whether or not derived class uses a protocol synchrnonization byte
            StreamInitialized = !ProtocolUsesSyncBytes;

            m_enabled = true;
        }

        /// <summary>
        /// Stops the streaming data parser.
        /// </summary>
        public virtual void Stop()
        {
            m_enabled = false;
            m_stopTime = DateTime.Now.Ticks;
        }

        /// <summary>
        /// Parses the object implementing the <see cref="ISupportBinaryImage"/> interface.
        /// </summary>
        /// <param name="image">Object to be parsed that implements the <see cref="ISupportBinaryImage"/> interface.</param>
        /// <remarks>
        /// This function takes the binary image from <see cref="ISupportBinaryImage"/> and writes the buffer to the <see cref="BinaryImageParserBase"/> stream for parsing.
        /// </remarks>
        public virtual void Parse(ISupportBinaryImage image)
        {
            Write(image.BinaryImage, 0, image.BinaryLength);
        }

        // Stream implementation overrides

        /// <summary>
        /// Writes a sequence of bytes onto the stream for parsing.
        /// </summary>
        /// <param name="buffer">An array of bytes. This method copies count bytes from buffer to the current stream.</param>
        /// <param name="offset">The zero-based byte offset in buffer at which to begin copying bytes to the current stream.</param>
        /// <param name="count">The number of bytes to be written to the current stream.</param>
        public override void Write(byte[] buffer, int offset, int count)
        {
            if (m_enabled)
            {
                // If ProtocolUsesSyncByte is true, first call to write after start will be uninitialized,
                // thus the attempt below to "align" data stream to specified ProtocolSyncBytes.
                if (StreamInitialized)
                {
                    // Directly parse frame using calling thread (typically communications thread)
                    ParseBuffer(buffer, offset, count);
                }
                else
                {
                    // Initial stream may be anywhere in the middle of a frame, so we attempt to locate sync byte(s) to "line-up" data stream
                    int syncBytesPosition = buffer.IndexOfSequence(ProtocolSyncBytes, offset, count);

                    if (syncBytesPosition > -1)
                    {
                        StreamInitialized = true;
                        ParseBuffer(buffer, syncBytesPosition, count - syncBytesPosition);
                    }
                }

                // Track total processed buffer images
                m_buffersProcessed++;
            }
        }

        /// <summary>
        /// When overridden in a derived class, clears all buffers for this stream and causes any buffered data
        /// to be written to the underlying device.
        /// </summary>
        public override void Flush()
        {
        }

        #region [ Unimplemented Stream Overrides ]

        // The parser is designed as a write only stream - so the following methods do not apply

        /// <summary>
        /// The parser is designed as a write only stream, so this method is not implemented.
        /// </summary>
        /// <exception cref="NotImplementedException">Cannnot read from WriteOnly stream.</exception>
        /// <param name="buffer">Array of <see cref="Byte"/>s.</param>
        /// <param name="count">An <see cref="Int32"/> value for the offset.</param>
        /// <param name="offset">An <see cref="Int32"/> value for the count.</param>
        /// <returns>An <see cref="Int32"/> as the number of bytes read. Well. It would, if implemented.</returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException("Cannnot read from WriteOnly stream.");
        }

        /// <summary>
        /// The parser is designed as a write only stream, so this method is not implemented.
        /// </summary>
        /// <exception cref="NotImplementedException">WriteOnly stream has no position.</exception>
        /// <param name="offset">A <see cref="Int64"/> value for the offset.</param>
        /// <param name="origin">A <see cref="SeekOrigin"/>.</param>
        /// <returns>Returns a <see cref="Int64"/> value indicating the point that was seeked to.</returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override long Seek(long offset, System.IO.SeekOrigin origin)
        {
            throw new NotImplementedException("WriteOnly stream has no position.");
        }

        /// <summary>
        /// The parser is designed as a write only stream, so this method is not implemented.
        /// </summary>
        /// <exception cref="NotImplementedException">WriteOnly stream has no length.</exception>
        /// <param name="value">A <see cref="Int64"/> value.</param>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override void SetLength(long value)
        {
            throw new NotImplementedException("WriteOnly stream has no length.");
        }

        /// <summary>
        /// The parser is designed as a write only stream, so this method is not implemented.
        /// </summary>
        /// <exception cref="NotImplementedException">WriteOnly stream has no length.</exception>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override long Length
        {
            get
            {
                return -1;
            }
        }

        /// <summary>
        /// The parser is designed as a write only stream, so this method is not implemented.
        /// </summary>
        /// <exception cref="NotImplementedException">WriteOnly stream has no position.</exception>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override long Position
        {
            get
            {
                return -1;
            }
            set
            {
            }
        }

        #endregion

        // Parse buffer image - user implements protocol specific "ParseFrame" function to extract data from image
        private void ParseBuffer(byte[] buffer, int offset, int count)
        {
            try
            {
                // Prepend any left over buffer data from last parse call
                if (UnparsedBuffer != null)
                {
                    // Combine remaining buffer from last call and current buffer together as a single image
                    buffer = UnparsedBuffer.Combine(0, UnparsedBuffer.Length, buffer, offset, count);
                    offset = 0;
                    count = buffer.Length;
                    UnparsedBuffer = null;
                }

                int endOfBuffer = offset + count - 1;
                int parsedFrameLength = 0;

                // Move through buffer parsing all available frames
                while (!(offset > endOfBuffer) && m_enabled)
                {
                    try
                    {
                        // Call derived class frame parsing algorithm - this is protocol specific
                        parsedFrameLength = ParseFrame(buffer, offset, endOfBuffer - offset + 1);
                    }
                    catch (Exception ex)
                    {
                        // If protocol defines synchronization bytes there's a chance at recovering any unused portion of this buffer,
                        // otherwise we'll just rethrow the exception and scrap the buffer...
                        if (ProtocolUsesSyncBytes)
                        {
                            // Attempt to locate sync byte(s) after exception to line data stream back up
                            int syncBytesPosition = buffer.IndexOfSequence(ProtocolSyncBytes, offset + 1, endOfBuffer - offset);

                            if (syncBytesPosition > -1)
                            {
                                // Found the next sync byte(s), pass through malformed frame
                                parsedFrameLength = syncBytesPosition - offset;
                                
                                // We'll still let consumer know there was an issue
                                OnDataDiscarded(buffer.BlockCopy(offset, parsedFrameLength));
                                OnParsingException(ex);
                            }
                            else
                                throw ex;
                        }
                        else
                            throw ex;
                    }

                    // Returned value represents total bytes of data in the buffer image that were
                    // parsed. There could still be data remaining in the buffer, but user parsing
                    // algorithm could have decided that there was not enough buffer available for
                    // parsing and returned a "zero" - meaning no data was parsed.
                    if (parsedFrameLength > 0)
                    {
                        // If frame was parsed, increment buffer offset by frame length
                        offset += parsedFrameLength;
                    }
                    else
                    {
                        // If not, save off remaining buffer to prepend onto next read
                        UnparsedBuffer = buffer.BlockCopy(offset, count - offset);
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                // Probable malformed data image, discard data on move on...
                StreamInitialized = !ProtocolUsesSyncBytes;
                UnparsedBuffer = null;

                if (offset < count)
                    OnDataDiscarded(buffer.BlockCopy(offset, count - offset));

                OnParsingException(ex);
            }
        }

        /// <summary>
        /// Protocol specific frame parsing algorithm.
        /// </summary>
        /// <param name="buffer">Buffer containing data to parse.</param>
        /// <param name="offset">Offset index into buffer that represents where to start parsing.</param>
        /// <param name="length">Maximum length of valid data from offset.</param>
        /// <returns>The length of the data that was parsed.</returns>
        /// <remarks>
        /// <para>
        /// Implementors can choose to focus on parsing a single frame in the buffer even if there are other frames available in the buffer.
        /// Base class will continue to move through buffer on behalf of derived class until all the buffer has been processed.  Just return
        /// the total amount of data was parsed and the remaining unparsed will be prepended to next received buffer.
        /// </para>
        /// <para>
        /// Derived implementations should return an integer value that represents the length of the data that was parsed, and zero if not
        /// enough data was able to be parsed. Note that exceptions are expensive when parsing fast moving streaming data and a good coding
        /// practice for implementations of this method will be to not throw an exception when there is not enough data to parse the data,
        /// instead check the <paramref name="length"/> property to insure there is enough data in buffer to represent the desired image. If
        /// there is not enough data to represent the image return zero and the base class will prepend buffer onto next incoming set of data.
        /// </para>
        /// <para>
        /// Because of the expense incurred when an exception is thrown, any exceptions encounted in the derived implementations of this method
        /// will cause the current data buffer to be discarded and a <see cref="ParsingException"/> event to be raised.  Doing this prevents
        /// exceptions from being thrown repeatedly for the same data. If your code implementation recognizes a malformed image, raise a custom
        /// event to indicate this instead of throwing as exception and keep moving through the buffer.
        /// </para>
        /// </remarks>
        protected abstract int ParseFrame(byte[] buffer, int offset, int length);

        /// <summary>
        /// Raises the <see cref="DataDiscarded"/> event.
        /// </summary>
        /// <param name="buffer">Source buffer that contains output that failed to parse.</param>
        protected virtual void OnDataDiscarded(byte[] buffer)
        {
            if (DataDiscarded != null)
                DataDiscarded(this, new EventArgs<byte[]>(buffer));
        }

        /// <summary>
        /// Raises the <see cref="ParsingException"/> event.
        /// </summary>
        /// <param name="ex">The <see cref="Exception"/> that was encountered during parsing.</param>
        protected virtual void OnParsingException(Exception ex)
        {
            if (ParsingException != null)
                ParsingException(this, new EventArgs<Exception>(ex));
        }

        #endregion
    }
}