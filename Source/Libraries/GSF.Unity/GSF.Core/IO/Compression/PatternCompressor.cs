//*******************************************************************************************************
//  PatternCompressor.cs - Gbtc
//
//  Tennessee Valley Authority, 2011
//  No copyright is claimed pursuant to 17 USC § 105.  All Other Rights Reserved.
//
//  This software is made freely available under the TVA Open Source Agreement (see below).
//  Code in this file licensed to GSF under one or more contributor license agreements listed below.
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  10/27/2011 - J. Ritchie Carroll
//       Initial version of source generated.
//  01/25/2012 - Stephen C. Wills
//       Removed 64-bit compression routine, added 32-bit decompression routine.
//  01/26/2012 - Stephen C. Wills
//       Changed class to non-static so that compressor instances
//       can be created to handle stream compression.
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
 Original Software Title: The GSF Open Source Phasor Data Concentrator
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

#region [ Contributor License Agreements ]

//******************************************************************************************************
//
//  Copyright © 2011, Grid Protection Alliance.  All Rights Reserved.
//
//  The GPA licenses this file to you under the Eclipse Public License -v 1.0 (the "License"); you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://www.opensource.org/licenses/eclipse-1.0.php
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//******************************************************************************************************

#endregion

using GSF.Parsing;
using System;

namespace GSF.IO.Compression
{
    /// <summary>
    /// Defines functions used for high-speed pattern compression against native types.
    /// </summary>
    public class PatternCompressor
    {
        #region [ Members ]

        // Fields
        private byte[] m_compressedBuffer;
        private int m_compressedBufferLength;
        private int m_maxCompressedBufferLength;

        private uint[] m_backBuffer;
        private int m_backBufferStart;
        private int m_backBufferLength;

        private byte m_compressionStrength;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new instance of the PatternCompressor with the given compression strength.
        /// </summary>
        /// <param name="compressionStrength">The compression strength of the PatternCompressor.</param>
        public PatternCompressor(byte compressionStrength)
        {
            CompressionStrength = compressionStrength;
            m_maxCompressedBufferLength = -1;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the buffer into which compressed values are written.
        /// </summary>
        /// <remarks>
        /// When setting the compressed buffer, the new buffer is assumed to be empty
        /// (<see cref="CompressedBufferLength"/> is set to zero).
        /// </remarks>
        public byte[] CompressedBuffer
        {
            get
            {
                return m_compressedBuffer;
            }
            set
            {
                m_compressedBuffer = value;
                m_compressedBufferLength = 0;
            }
        }

        /// <summary>
        /// Gets the amount of compressed data in the compressed buffer.
        /// </summary>
        public int CompressedBufferLength
        {
            get
            {
                return m_compressedBufferLength;
            }
        }

        /// <summary>
        /// Gets or sets the maximum compressed buffer length.
        /// </summary>
        /// <remarks>
        /// The compressor will not write beyond the MaxCompressedBufferLength.
        /// Defaults to the full length of the compressed buffer.
        /// Set to -1 to return to the default.
        /// </remarks>
        public int MaxCompressedBufferLength
        {
            get
            {
                if (m_maxCompressedBufferLength > 0)
                    return m_maxCompressedBufferLength;
                else if (m_compressedBuffer != null)
                    return m_compressedBuffer.Length;
                else
                    return 0;
            }
            set
            {
                m_maxCompressedBufferLength = value;
            }
        }

        /// <summary>
        /// Gets or sets the compression strength of the PatternCompressor.
        /// </summary>
        /// <remarks>
        /// When setting the compression strength, the PatternCompressor
        /// is automatically reset (the back buffer is emptied).
        /// </remarks>
        public byte CompressionStrength
        {
            get
            {
                return m_compressionStrength;
            }
            set
            {
                if (value > 31)
                    throw new ArgumentOutOfRangeException("value", "Compression strength must be 0 to 31");

                if (m_backBuffer == null || m_backBuffer.Length < value + 1)
                    m_backBuffer = new uint[value + 1];

                m_backBufferStart = 0;
                m_backBufferLength = 0;

                m_compressionStrength = value;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Compresses the given value and places it in the compressed buffer.
        /// </summary>
        /// <param name="value">The value to be compressed.</param>
        /// <returns>The size, in bytes, of the compressed value.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="value"/> cannot be null.</exception>
        public unsafe int Compress(ISupportBinaryImage value)
        {
            byte[] buffer;
            int bufferLength;

            int compressedSize = 0;
            byte* iter, end;

            if (value == null)
                throw new ArgumentNullException("value cannot be null", "value");

            bufferLength = Word.AlignDoubleWord(value.BinaryLength);
            buffer = new byte[bufferLength];

            fixed (byte* start = buffer)
            {
                value.GenerateBinaryImage(buffer, 0);
                end = start + bufferLength;

                for (iter = start; iter < end; iter += 4)
                    compressedSize += Compress(iter);
            }

            return compressedSize;
        }

        /// <summary>
        /// Compresses all of the data in the given <paramref name="buffer"/>.
        /// </summary>
        /// <param name="buffer">The buffer to be compressed.</param>
        /// <returns>The size, in bytes, of the compressed value.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="buffer"/> cannot be null.</exception>
        /// <exception cref="ArgumentException"><paramref name="buffer"/> length must be a multiple of four.</exception>
        public int Compress(byte[] buffer)
        {
            return Compress(buffer, 0, buffer.Length);
        }

        /// <summary>
        /// Compresses <paramref name="length"/> bytes of data in the given <paramref name="buffer"/>.
        /// </summary>
        /// <param name="buffer">The buffer to be compressed.</param>
        /// <param name="length">The amount of data to be compressed. Must be a multiple of four.</param>
        /// <returns>The size, in bytes, of the compressed value.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="buffer"/> cannot be null.</exception>
        /// <exception cref="ArgumentException"><paramref name="length"/> must be a multiple of four.</exception>]
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="length"/> must be greater than or equal to zero.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="length"/> exceeds <paramref name="buffer"/> array boundaries</exception>
        public int Compress(byte[] buffer, int length)
        {
            return Compress(buffer, 0, length);
        }

        /// <summary>
        /// Compresses <paramref name="length"/> bytes of data in the given <paramref name="buffer"/>, starting at <paramref name="offset"/>.
        /// </summary>
        /// <param name="buffer">The buffer to be compressed.</param>
        /// <param name="offset">The amount of data to ignore at the start of the buffer.</param>
        /// <param name="length">The amount of data to be compressed. Must be a multiple of four.</param>
        /// <returns>The size, in bytes, of the compressed value.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="buffer"/> cannot be null.</exception>
        /// <exception cref="ArgumentException"><paramref name="length"/> must be a multiple of four.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="offset"/> must be greater than or equal to zero.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="length"/> must be greater than or equal to zero.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="length"/> exceeds <paramref name="buffer"/> array boundaries</exception>
        public unsafe int Compress(byte[] buffer, int offset, int length)
        {
            const int SizeOf32Bits = sizeof(uint);
            int compressedSize = 0;
            byte* start, iter, end;

            if (buffer == null)
                throw new ArgumentNullException("buffer", "Cannot write data to null buffer");

            if (length % SizeOf32Bits != 0)
                throw new ArgumentException("Buffer length must be a multiple of four", "length");

            if (offset < 0)
                throw new ArgumentOutOfRangeException("offset", "offset must be greater than or equal to zero");

            if (length < 0)
                throw new ArgumentOutOfRangeException("length", "length must be greater than or equal to zero");

            if (buffer.Length < offset + length)
                throw new ArgumentOutOfRangeException("length", "length exceeds buffer array boundaries");

            fixed (byte* pBuffer = buffer)
            {
                start = pBuffer + offset;
                end = start + length;

                for (iter = start; iter < end; iter += 4)
                    compressedSize += Compress(iter);
            }

            return compressedSize;
        }

        /// <summary>
        /// Compresses the given value and places it in the compressed buffer.
        /// </summary>
        /// <param name="value">The value to be compressed.</param>
        /// <returns>The size, in bytes, of the compressed value.</returns>
        public unsafe int Compress(double value)
        {
            uint* pItem = (uint*)&value;
            int compressedSize = 0;

            compressedSize += Compress((byte*)pItem);
            compressedSize += Compress((byte*)(pItem + 1));

            return compressedSize;
        }

        /// <summary>
        /// Compresses the given value and places it in the compressed buffer.
        /// </summary>
        /// <param name="value">The value to be compressed.</param>
        /// <returns>The size, in bytes, of the compressed value.</returns>
        public unsafe int Compress(float value)
        {
            return Compress((byte*)&value);
        }

        /// <summary>
        /// Compresses the given value and places it in the compressed buffer.
        /// </summary>
        /// <param name="value">The value to be compressed.</param>
        /// <returns>The size, in bytes, of the compressed value.</returns>
        public unsafe int Compress(long value)
        {
            uint* pItem = (uint*)&value;
            int compressedSize = 0;

            compressedSize += Compress((byte*)pItem);
            compressedSize += Compress((byte*)(pItem + 1));

            return compressedSize;
        }

        /// <summary>
        /// Compresses the given value and places it in the compressed buffer.
        /// </summary>
        /// <param name="value">The value to be compressed.</param>
        /// <returns>The size, in bytes, of the compressed value.</returns>
        [CLSCompliant(false)]
        public unsafe int Compress(ulong value)
        {
            uint* pItem = (uint*)&value;
            int compressedSize = 0;

            compressedSize += Compress((byte*)pItem);
            compressedSize += Compress((byte*)(pItem + 1));

            return compressedSize;
        }

        /// <summary>
        /// Compresses the given value and places it in the compressed buffer.
        /// </summary>
        /// <param name="value">The value to be compressed.</param>
        /// <returns>The size, in bytes, of the compressed value.</returns>
        public unsafe int Compress(int value)
        {
            return Compress((byte*)&value);
        }

        /// <summary>
        /// Compresses the given value and places it in the compressed buffer.
        /// </summary>
        /// <param name="value">The value to be compressed.</param>
        /// <returns>The size, in bytes, of the compressed value.</returns>
        [CLSCompliant(false)]
        public unsafe int Compress(uint value)
        {
            return Compress((byte*)&value);
        }

        /// <summary>
        /// Compresses the given value and places it in the compressed buffer.
        /// </summary>
        /// <param name="value">The value to be compressed.</param>
        /// <returns>The size, in bytes, of the compressed value.</returns>
        public int Compress(short value)
        {
            return Compress((int)value);
        }

        /// <summary>
        /// Compresses the given value and places it in the compressed buffer.
        /// </summary>
        /// <param name="value">The value to be compressed.</param>
        /// <returns>The size, in bytes, of the compressed value.</returns>
        [CLSCompliant(false)]
        public int Compress(ushort value)
        {
            return Compress((uint)value);
        }

        /// <summary>
        /// Compresses the given value and places it in the compressed buffer.
        /// </summary>
        /// <param name="value">The value to be compressed.</param>
        /// <returns>The size, in bytes, of the compressed value.</returns>
        public int Compress(byte value)
        {
            return Compress((uint)value);
        }

        /// <summary>
        /// Sets <see cref="CompressedBufferLength"/> to zero and allows the
        /// compressor to write over the values in the compressed buffer.
        /// </summary>
        public void EmptyCompressedBuffer()
        {
            m_compressedBufferLength = 0;
        }

        /// <summary>
        /// Resets the compressor by flushing the back buffer. Subsequent calls
        /// to the Compress methods will not be compressed using previously
        /// compressed values.
        /// </summary>
        public void Reset()
        {
            m_backBufferStart = 0;
            m_backBufferLength = 0;
        }

        // Helper method to compress a generic set of four
        // bytes, given a pointer to the first byte.
        private unsafe int Compress(byte* pValue)
        {
            uint value = *(uint*)pValue;
            int compressedSize;

            if (m_compressedBuffer == null)
                throw new InvalidOperationException("Cannot write compressed value to a null buffer");

            if (m_backBufferLength == 0)
                compressedSize = InsertFirstValue(value);
            else
                compressedSize = InsertCompressedValue(value);

            InsertIntoBackBuffer(value);
            return compressedSize;
        }

        // Inserts the given value into the compressed buffer as the first,
        // uncompressed value in the stream. This is called when the back
        // buffer is empty.
        private unsafe int InsertFirstValue(uint value)
        {
            // Size of the first value includes the 1-byte stream header
            const int FirstValueSize = 1 + sizeof(uint);

            // Ensure that the value will fit in the compressed buffer
            if (MaxCompressedBufferLength - m_compressedBufferLength < FirstValueSize)
                throw new InvalidOperationException("Value cannot fit in compressed buffer. Empty compressed buffer.");

            // Insert value into compressed buffer
            fixed (byte* pCompressedBuffer = m_compressedBuffer)
            {
                byte* compressedBufferEnd = pCompressedBuffer + m_compressedBufferLength;
                *compressedBufferEnd = (byte)(0xC0 | m_compressionStrength);
                *(uint*)(compressedBufferEnd + 1) = value;
                m_compressedBufferLength += FirstValueSize;
            }

            return FirstValueSize;
        }

        // Compresses the given value and inserts it into the compressed buffer.
        private unsafe int InsertCompressedValue(uint value)
        {
            byte* compressedBufferEnd;

            byte* pValue = (byte*)&value;
            byte decompressionKey = GetDecompressionKey(value);
            byte difference = (byte)(decompressionKey >> 5);

            int compressedSize = 1 + difference;

            if (MaxCompressedBufferLength - m_compressedBufferLength < compressedSize)
                throw new InvalidOperationException("Value cannot fit in compressed buffer. Empty compressed buffer.");

            fixed (byte* pCompressedBuffer = m_compressedBuffer)
            {
                compressedBufferEnd = pCompressedBuffer + m_compressedBufferLength;
                *compressedBufferEnd = decompressionKey;
                compressedBufferEnd++;

                if (!BitConverter.IsLittleEndian)
                    pValue += sizeof(uint) - difference;

                for (int i = 0; i < difference; i++, compressedBufferEnd++, pValue++)
                    *compressedBufferEnd = *pValue;

                m_compressedBufferLength += compressedSize;
            }

            return compressedSize;
        }

        // Inserts the value into the back buffer.
        private void InsertIntoBackBuffer(uint value)
        {
            if (m_backBufferLength < m_compressionStrength + 1)
            {
                m_backBuffer[m_backBufferLength] = value;
                m_backBufferLength++;
            }
            else
            {
                m_backBuffer[m_backBufferStart] = value;
                m_backBufferStart++;

                if (m_backBufferStart >= m_backBufferLength)
                    m_backBufferStart = 0;
            }
        }

        // Gets the 8-bit decompression key for the given value
        // based on the values that are in the back buffer.
        private unsafe byte GetDecompressionKey(uint value)
        {
            uint* backBufferIterator;

            uint result;
            byte difference;

            byte smallestDifference = 5;
            int backBufferIndex = 0;

            // Pin back buffer to navigate using pointer
            fixed (uint* pBackBuffer = m_backBuffer)
            {
                backBufferIterator = pBackBuffer;

                // Compare item with each value in the back buffer to find the smallest difference
                for (int i = 0; i < m_backBufferLength; i++, backBufferIterator++)
                {
                    result = value ^ *backBufferIterator;

                    if (result > 0xFFFFFFu)
                        difference = 4;
                    else if (result > 0xFFFFu)
                        difference = 3;
                    else if (result > 0xFFu)
                        difference = 2;
                    else if (result > 0u)
                        difference = 1;
                    else
                        difference = 0;

                    if (difference < smallestDifference)
                    {
                        smallestDifference = difference;
                        backBufferIndex = i;
                    }
                }
            }

            // Return decompression key:
            //  backBufferIndex in bits 0 through 4
            //  smallestDifference in bits 5 through 7
            return (byte)((smallestDifference << 5) | backBufferIndex);
        }

        #endregion

        #region [ Static ]

        // Static Methods

        /// <summary>
        /// Compress a byte array containing a sequential list of 32-bit structures (e.g., floating point numbers, integers or unsigned integers) using a patterned compression method.
        /// </summary>
        /// <param name="source">The <see cref="Byte"/> array containing 32-bit values to compress. Compression will happen inline on this buffer.</param>
        /// <param name="startIndex">An <see cref="Int32"/> representing the start index of the byte array.</param>
        /// <param name="dataLength">The number of bytes in the buffer that represents actual data.</param>
        /// <param name="bufferLength">The number of bytes available for use in the buffer; actual buffer length must be at least one byte larger than <paramref name="dataLength"/> since it's possible that data cannot be compressed. This extra byte will be used indicate an uncompressed buffer.</param>
        /// <param name="compressionStrength">Specifies compression strength (0 to 31). Smaller numbers will run faster, larger numbers will yield better compression.</param>
        /// <returns>The new length of the buffer after compression.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> buffer cannot be null.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="dataLength"/> must be greater than or equal to zero.</exception>
        /// <exception cref="ArgumentException"><paramref name="dataLength"/> must be an even multiple of 4.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="bufferLength"/> must be at least one byte larger than <paramref name="dataLength"/> in case data cannot be compressed.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Actual length of <paramref name="source"/> buffer is less than specified <paramref name="bufferLength"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="compressionStrength"/> must be 0 to 31.</exception>
        /// <remarks>
        /// As an optimization this function is using pointers to native structures, as such the endian order decoding and encoding of the values will always be in the native endian order of the operating system.
        /// </remarks>
        public unsafe static int CompressBuffer(byte[] source, int startIndex, int dataLength, int bufferLength, byte compressionStrength)
        {
            const int SizeOf32Bits = sizeof(uint);

            // Queue length of 32 forces reservation of 3 bits on single byte decompression key to allow for back track indicies of 0 to 31, this
            // means minimal compression size of 4 total bytes would be 1 byte (i.e., 1 byte for decompression key), or max of 75% compression.
            // Compression algorithm is best suited for data that differs fractionally over time (e.g., 60.05, 60.08, 60.09, 60.11...)

            if (source == null)
                throw new ArgumentNullException("source");

            if (dataLength < 0)
                throw new ArgumentOutOfRangeException("dataLength", "Data length must be greater than or equal to zero");

            if (dataLength % SizeOf32Bits != 0)
                throw new ArgumentException("Data length must be a multiple of 4", "dataLength");

            if (bufferLength < dataLength + 1)
                throw new ArgumentOutOfRangeException("bufferLength", "Buffer length must be at least one byte larger than original data length in case data cannot be compressed");

            if (source.Length < bufferLength)
                throw new ArgumentOutOfRangeException("source", "Actual length of source buffer is less than specified buffer length");

            if (compressionStrength > 31)
                throw new ArgumentOutOfRangeException("compressionStrength", "Compression strength must be 0 to 31");

            // Special case when there is not enough data to be able to compress
            if (dataLength <= 4)
            {
                Buffer.BlockCopy(source, startIndex, source, startIndex + 1, dataLength);
                source[0] = (byte)(0xE0 | compressionStrength);
                return dataLength + 1;
            }

            byte[] buffer = null;
            int maxQueueLength = compressionStrength + 1;
            uint* queue = stackalloc uint[maxQueueLength];
            int queueLength = 0;
            int usedLength = 0;
            int count = dataLength / SizeOf32Bits;
            int queueStartIndex = 0;

            try
            {
                // Grab a working buffer from the pool, note that maximum zero compression size would be size of all original values plus one byte for each value
                buffer = BufferPool.TakeBuffer(dataLength + count);

                // Pin buffers to be navigated so that .NET doesn't move them around
                fixed (byte* pSource = source, pBuffer = buffer)
                {
                    byte* bufferIndex = pBuffer;
                    uint* values = (uint*)pSource;

                    // Reserve initial byte for compression header
                    *bufferIndex = compressionStrength;
                    bufferIndex++;

                    // Always add first value to the buffer as-is
                    *(uint*)bufferIndex = *values;
                    bufferIndex += SizeOf32Bits;

                    // Initialize first set of queue values for back reference
                    for (int i = 0; i < (count < maxQueueLength ? count : maxQueueLength); i++, values++, queueLength++)
                    {
                        queue[i] = *values;
                    }

                    // Reset values collection pointer starting at second item
                    values = (uint*)pSource;
                    values++;

                    // Starting with second item, begin compression sequence
                    for (int index = 1; index < count; index++)
                    {
                        uint test, current = *values;
                        byte backReferenceIndex = 0;
                        int smallestDifference = SizeOf32Bits;
                        int queueIndex = queueStartIndex;

                        // Test each item in back reference queue for best compression
                        for (int i = 0; i < (index < queueLength ? index : queueLength); i++)
                        {
                            int difference;

                            // Get first item from queue
                            test = queue[queueIndex];

                            // Xor current value and queue value (interpreted as integers) for total byte differences
                            uint result = current ^ test;

                            if (result > 0xFFFFFFu)
                                difference = 4; // Value differs by 4 bytes
                            else if (result > 0xFFFFu)
                                difference = 3; // Value differs by 3 bytes
                            else if (result > 0xFFu)
                                difference = 2; // Value differs by 2 bytes
                            else if (result > 0u)
                                difference = 1; // Value differs by 1 bytes
                            else
                                difference = 0; // Value differs by 0 bytes

                            // Item with the smallest difference in the back reference queue wins
                            if (difference < smallestDifference)
                            {
                                smallestDifference = difference;
                                backReferenceIndex = (byte)queueIndex;

                                // No need to check further if we've found a full match on all possible bytes
                                if (smallestDifference == 0)
                                    break;
                            }

                            queueIndex++;

                            if (queueIndex >= queueLength)
                                queueIndex = 0;
                        }

                        // Calculate key that will be needed for proper decompression, that is: byte difference
                        // in bits 5 through 7 and the back reference xor value index in bits 0 through 4
                        byte decompressionKey = (byte)((byte)(smallestDifference << 5) | backReferenceIndex);

                        // Add decompression key to output buffer
                        *bufferIndex = decompressionKey;
                        bufferIndex++;

                        // Get a pointer to the best compression result
                        byte* pValues = (byte*)values;

                        // If desired bytes are in big endian order, then they are right most in memory so skip ahead
                        if (!BitConverter.IsLittleEndian)
                            pValues += SizeOf32Bits - smallestDifference;

                        // Add only needed bytes to the output buffer (maybe none!)
                        for (int j = 0; j < smallestDifference; j++, bufferIndex++, pValues++)
                        {
                            *bufferIndex = *pValues;
                        }

                        // After initial queue values, add newest item to the queue, replacing the old one
                        if (index >= queueLength)
                        {
                            queue[queueStartIndex] = current;

                            // Track oldest item in the queue as the starting location
                            queueStartIndex++;

                            if (queueStartIndex >= queueLength)
                                queueStartIndex = 0;
                        }

                        // Setup to compress the next value
                        values++;
                    }

                    usedLength = (int)(bufferIndex - pBuffer);

                    // Check to see if we failed to compress data (hopefully rare)
                    if (usedLength > dataLength)
                    {
                        // Set compression buffer flags to uncompressed
                        *pBuffer |= (byte)0xE0;
                        Buffer.BlockCopy(source, startIndex, buffer, 1, dataLength);
                        usedLength = dataLength + 1;
                    }

                    // Overwrite source buffer with new compressed buffer
                    Buffer.BlockCopy(buffer, 0, source, startIndex, usedLength);
                }
            }
            finally
            {
                // Return buffer to queue so it can be reused
                if (buffer != null)
                    BufferPool.ReturnBuffer(buffer);
            }

            return usedLength;
        }

        #endregion

        ///// <summary>
        ///// Compress a byte array containing a sequential list of 64-bit structures (e.g., double precision floating point numbers, long integers or unsigned long integers) using a patterned compression method.
        ///// </summary>
        ///// <param name="source">The <see cref="Byte"/> array containing 64-bit values to compress. Compression will happen inline on this buffer.</param>
        ///// <param name="startIndex">An <see cref="Int32"/> representing the start index of the byte array.</param>
        ///// <param name="dataLength">The number of bytes in the buffer that represents actual data.</param>
        ///// <param name="bufferLength">The number of bytes available for use in the buffer; actual buffer length must be at least as large as <paramref name="dataLength"/>.</param>
        ///// <param name="compressionStrength">Specifies compression strength (0 to 255). Smaller numbers will run faster, larger numbers will yield better compression.</param>
        ///// <returns>The new length of the buffer after compression.</returns>
        ///// <exception cref="ArgumentNullException"><paramref name="source"/> buffer cannot be null.</exception>
        ///// <exception cref="ArgumentOutOfRangeException"><paramref name="dataLength"/> must be greater than or equal to zero.</exception>
        ///// <exception cref="ArgumentException"><paramref name="dataLength"/> must be an even multiple of 8.</exception>
        ///// <exception cref="ArgumentOutOfRangeException"><paramref name="bufferLength"/> must be at least as large as <paramref name="dataLength"/>.</exception>
        ///// <exception cref="ArgumentOutOfRangeException">Actual length of <paramref name="source"/> buffer is less than specified <paramref name="bufferLength"/>.</exception>
        ///// <remarks>
        ///// As an optimization this function is using pointers to native structures, as such the endian order decoding and encoding of the values will always be in the native endian order of the operating system.
        ///// </remarks>
        //public unsafe static int Compress64bitEnumeration(this byte[] source, int startIndex, int dataLength, int bufferLength, byte compressionStrength = 5)
        //{
        //    const int SizeOf64Bits = sizeof(ulong);

        //    // Algorithm uses all 8-bits of decompression key plus 1 full byte for back track indicies, 256 maximum, to yield 75% maximum compression.
        //    // Compression algorithm is best suited for data that differs fractionally over time (e.g., 60.05, 60.08, 60.09, 60.11...)
        //    if (source == null)
        //        throw new ArgumentNullException("source");

        //    if (dataLength < 0)
        //        throw new ArgumentOutOfRangeException("dataLength", "Data length must be greater than or equal to zero");

        //    if (dataLength % SizeOf64Bits != 0)
        //        throw new ArgumentException("Data length must be a multiple of 8", "dataLength");

        //    if (bufferLength < dataLength)
        //        throw new ArgumentOutOfRangeException("bufferLength", "Buffer length must be at least as large as original data length");

        //    if (source.Length < bufferLength)
        //        throw new ArgumentOutOfRangeException("source", "Actual length of source buffer is less than specified buffer length");

        //    byte[] buffer = null;
        //    int maxQueueLength = compressionStrength + 1;
        //    ulong* queue = stackalloc ulong[maxQueueLength];
        //    int queueLength = 0;
        //    int usedLength = 0;
        //    int count = dataLength / SizeOf64Bits;
        //    int queueStartIndex = 0;

        //    try
        //    {
        //        // Grab a working buffer from the pool, note that maximum zero compression size would be size of all original values plus two bytes for each value
        //        buffer = BufferPool.TakeBuffer(dataLength + 2 * count);

        //        // Pin buffers to be navigated so that .NET doesn't move them around
        //        fixed (byte* pSource = source, pBuffer = buffer)
        //        {
        //            byte* bufferIndex = pBuffer;
        //            ulong* values = (ulong*)pSource;

        //            // Always add first value to the buffer as-is
        //            *(ulong*)bufferIndex = *values;
        //            bufferIndex += SizeOf64Bits;

        //            // Initialize first set of queue values for back reference
        //            for (int i = 0; i < (count < maxQueueLength ? count : maxQueueLength); i++, values++, queueLength++)
        //            {
        //                queue[i] = *values;
        //            }

        //            // Reset values collection pointer starting at second item
        //            values = (ulong*)pSource;
        //            values++;

        //            // Starting with second item, begin compression sequence
        //            for (int index = 1; index < count; index++)
        //            {
        //                ulong test, current = *values;
        //                ulong bestResult = 0;
        //                byte backReferenceIndex = 0;
        //                int smallestDifference = SizeOf64Bits;
        //                int queueIndex = queueStartIndex;

        //                // Test each item in back reference queue for best compression
        //                for (int i = 0; i < (index < queueLength ? index : queueLength); i++)
        //                {
        //                    int difference;

        //                    // Get first item from queue
        //                    test = queue[queueIndex];

        //                    // Xor current value and queue value (interpreted as integers) for total byte differences
        //                    ulong result = current ^ test;

        //                    if ((result & 0xffffffffffffff) != result)
        //                        difference = 8; // Value differs by 8 bytes
        //                    else if ((result & 0xffffffffffff) != result)
        //                        difference = 7; // Value differs by 7 bytes
        //                    else if ((result & 0xffffffffff) != result)
        //                        difference = 6; // Value differs by 6 bytes
        //                    else if ((result & 0xffffffff) != result)
        //                        difference = 5; // Value differs by 5 bytes
        //                    else if ((result & 0xffffff) != result)
        //                        difference = 4; // Value differs by 4 bytes
        //                    else if ((result & 0xffff) != result)
        //                        difference = 3; // Value differs by 3 bytes
        //                    else if ((result & 0xff) != result)
        //                        difference = 2; // Value differs by 2 bytes
        //                    else if (result != 0)
        //                        difference = 1; // Value differs by 1 bytes
        //                    else
        //                        difference = 0; // Value differs by 0 bytes

        //                    // Item with the smallest difference in the back reference queue wins
        //                    if (difference < smallestDifference)
        //                    {
        //                        smallestDifference = difference;
        //                        backReferenceIndex = (byte)queueIndex;
        //                        bestResult = result;

        //                        // No need to check further if we've found a full match on all possible bytes
        //                        if (smallestDifference == 0)
        //                            break;
        //                    }

        //                    queueIndex++;

        //                    if (queueIndex >= queueLength)
        //                        queueIndex = 0;
        //                }

        //                //// Calculate key that will be needed for proper decompression
        //                //byte decompressionKey = (byte)(((uint)1 << smallestDifference) - 1);

        //                // Add decompression key to output buffer
        //                *bufferIndex = (byte)smallestDifference;
        //                bufferIndex++;

        //                // Add queue index to the output buffer
        //                *bufferIndex = backReferenceIndex;
        //                bufferIndex++;

        //                // Get a pointer to the best compression result
        //                byte* pResult = (byte*)&bestResult;

        //                // If desired bytes are in big endian order, then they are right most in memory so skip ahead
        //                if (!BitConverter.IsLittleEndian)
        //                    pResult += SizeOf64Bits - smallestDifference;

        //                // Add only needed bytes to the output buffer
        //                for (int j = 0; j < smallestDifference; j++, bufferIndex++, pResult++)
        //                {
        //                    *bufferIndex = *pResult;
        //                }

        //                // After initial queue values, add newest item to the queue, replacing the old one
        //                if (index >= queueLength)
        //                {
        //                    queue[queueStartIndex] = current;

        //                    // Track oldest item in the queue as the starting location
        //                    queueStartIndex++;

        //                    if (queueStartIndex >= queueLength)
        //                        queueStartIndex = 0;
        //                }

        //                // Setup to compress the next value
        //                values++;
        //            }

        //            usedLength = (int)(bufferIndex - pBuffer);

        //            // Check to see if we failed to compress data (hopefully rare)
        //            if (usedLength > dataLength)
        //                return 0;

        //            // Overwrite source buffer with new compressed buffer
        //            Buffer.BlockCopy(buffer, 0, source, startIndex, usedLength);
        //        }
        //    }
        //    finally
        //    {
        //        // Return buffer to queue so it can be reused
        //        if (buffer != null)
        //            BufferPool.ReturnBuffer(buffer);
        //    }

        //    return usedLength;
        //}

        ///// <summary>
        ///// 
        ///// </summary>
        ///// <typeparam name="T"></typeparam>
        ///// <param name="values"></param>
        ///// <param name="endianOrder"></param>
        ///// <returns></returns>
        //public static byte[] Compress<T>(this IEnumerable<T> values, EndianOrder endianOrder = null) where T : struct
        //{
        //    if (values == null)
        //        throw new ArgumentNullException("values");

        //    int count = values.Count();

        //    if (count == 0)
        //        return new byte[0];

        //    if (endianOrder == null)
        //        endianOrder = NativeEndianOrder.Default;

        //    if (count == 1)
        //        return endianOrder.GetBytes(values.First());

        //    MemoryStream results = new MemoryStream();
        //    T value, lastValue = values.First();

        //    for (int i = 1; i < count; i++)
        //    {
        //        value = values.ElementAt(i);

        //    }

        //    return results.ToArray();
        //}
    }
}
