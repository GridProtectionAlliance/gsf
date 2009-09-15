//*******************************************************************************************************
//  BufferExtensions.cs - Gbtc
//
//  Tennessee Valley Authority, 2009
//  No copyright is claimed pursuant to 17 USC § 105.  All Other Rights Reserved.
//
//  This software is made freely available under the TVA Open Source Agreement (see below).
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  09/19/2008 - J. Ritchie Carroll
//       Generated original version of source code.
//  12/03/2008 - J. Ritchie Carroll
//       Added "Combine" and "IndexOfSequence" overloaded extensions.
//  02/13/2009 - Josh L. Patterson
//       Edited Code Comments.
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
using System.IO;

namespace TVA
{
    /// <summary>Defines extension functions related to buffer manipulation.</summary>
    public static class BufferExtensions
    {
        /// <summary>Returns a copy of the specified portion of the <paramref name="source"/> buffer.</summary>
        /// <param name="source">Source buffer.</param>
        /// <param name="startIndex">Offset into <paramref name="source"/> buffer.</param>
        /// <param name="length">Length of <paramref name="source"/> buffer to copy at <paramref name="startIndex"/> offset.</param>
        /// <returns>A buffer of data copied from the specified portion of the source buffer.</returns>
        /// <remarks>
        /// Returned buffer will be extended as needed to make it the specified <paramref name="length"/>, but
        /// it will never be less than the source buffer length - <paramref name="startIndex"/>.
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="startIndex"/> is outside the range of valid indexes for the source buffer -or-
        /// <paramref name="length"/> is less than 0.
        /// </exception>
        public static byte[] BlockCopy(this byte[] source, int startIndex, int length)
        {
            if (startIndex < 0)
                throw new ArgumentOutOfRangeException("startIndex", "cannot be negative");

            if (length < 0)
                throw new ArgumentOutOfRangeException("length", "cannot be negative");

            if (startIndex >= source.Length)
                throw new ArgumentOutOfRangeException("startIndex", "not a valid index into source buffer");

            byte[] copiedBytes = new byte[source.Length - startIndex < length ? source.Length - startIndex : length];

            Buffer.BlockCopy(source, startIndex, copiedBytes, 0, copiedBytes.Length);

            return copiedBytes;
        }

        /// <summary>
        /// Combines buffers together as a single image.
        /// </summary>
        /// <param name="source">Source buffer.</param>
        /// <param name="other">Other buffer to combine to <paramref name="source"/> buffer.</param>
        /// <returns>Combined buffers.</returns>
        public static byte[] Combine(this byte[] source, byte[] other)
        {
            return source.Combine(0, source.Length, other, 0, other.Length);
        }

        /// <summary>
        /// Combines specified portions of buffers together as a single image.
        /// </summary>
        /// <param name="source">Source buffer.</param>
        /// <param name="sourceOffset">Offset into <paramref name="source"/> buffer to begin copy.</param>
        /// <param name="sourceCount">Number of bytes to copy from <paramref name="source"/> buffer.</param>
        /// <param name="other">Other buffer to combine to <paramref name="source"/> buffer.</param>
        /// <param name="otherOffset">Offset into <paramref name="other"/> buffer to begin copy.</param>
        /// <param name="otherCount">Number of bytes to copy from <paramref name="other"/> buffer.</param>
        /// <returns>Combined specified portions of both buffers.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="sourceOffset"/> or <paramref name="otherOffset"/> is outside the range of valid indexes for the associated buffer -or-
        /// <paramref name="sourceCount"/> or <paramref name="otherCount"/> is less than 0 -or- 
        /// <paramref name="sourceOffset"/> or <paramref name="otherOffset"/>, 
        /// and <paramref name="sourceCount"/> or <paramref name="otherCount"/> do not specify a valid section in the the associated buffer.
        /// </exception>
        public static byte[] Combine(this byte[] source, int sourceOffset, int sourceCount, byte[] other, int otherOffset, int otherCount)
        {
            if (sourceOffset < 0)
                throw new ArgumentOutOfRangeException("sourceOffset", "cannot be negative");

            if (otherOffset < 0)
                throw new ArgumentOutOfRangeException("otherOffset", "cannot be negative");

            if (sourceCount < 0)
                throw new ArgumentOutOfRangeException("sourceCount", "cannot be negative");

            if (otherCount < 0)
                throw new ArgumentOutOfRangeException("otherCount", "cannot be negative");

            if (sourceOffset >= source.Length)
                throw new ArgumentOutOfRangeException("sourceOffset", "not a valid index into source buffer");

            if (otherOffset >= other.Length)
                throw new ArgumentOutOfRangeException("otherOffset", "not a valid index into other buffer");

            if (sourceOffset + sourceCount > source.Length)
                throw new ArgumentOutOfRangeException("sourceCount", "exceeds source buffer size");

            if (otherOffset + otherCount > other.Length)
                throw new ArgumentOutOfRangeException("otherCount", "exceeds other buffer size");

            // Combine buffers together as a single image
            byte[] combinedBuffer = new byte[sourceCount + otherCount];
            
            Buffer.BlockCopy(source, sourceOffset, combinedBuffer, 0, sourceCount);
            Buffer.BlockCopy(other, otherOffset, combinedBuffer, sourceCount, otherCount);
            
            return combinedBuffer;
        }

        /// <summary>
        /// Combines buffers together as a single image.
        /// </summary>
        /// <param name="source">Source buffer.</param>
        /// <param name="other1">First buffer to combine to <paramref name="source"/> buffer.</param>
        /// <param name="other2">Second buffer to combine to <paramref name="source"/> buffer.</param>
        /// <returns>Combined buffers.</returns>
        public static byte[] Combine(this byte[] source, byte[] other1, byte[] other2)
        {
            return (new byte[][] { source, other1, other2 }).Combine();
        }

        /// <summary>
        /// Combines buffers together as a single image.
        /// </summary>
        /// <param name="source">Source buffer.</param>
        /// <param name="other1">First buffer to combine to <paramref name="source"/> buffer.</param>
        /// <param name="other2">Second buffer to combine to <paramref name="source"/> buffer.</param>
        /// <param name="other3">Third buffer to combine to <paramref name="source"/> buffer.</param>
        /// <returns>Combined buffers.</returns>
        public static byte[] Combine(this byte[] source, byte[] other1, byte[] other2, byte[] other3)
        {
            return (new byte[][] { source, other1, other2, other3 }).Combine();
        }

        /// <summary>
        /// Combines buffers together as a single image.
        /// </summary>
        /// <param name="source">Source buffer.</param>
        /// <param name="other1">First buffer to combine to <paramref name="source"/> buffer.</param>
        /// <param name="other2">Second buffer to combine to <paramref name="source"/> buffer.</param>
        /// <param name="other3">Third buffer to combine to <paramref name="source"/> buffer.</param>
        /// <param name="other4">Fourth buffer to combine to <paramref name="source"/> buffer.</param>
        /// <returns>Combined buffers.</returns>
        public static byte[] Combine(this byte[] source, byte[] other1, byte[] other2, byte[] other3, byte[] other4)
        {
            return (new byte[][] { source, other1, other2, other3, other4 }).Combine();
        }

        /// <summary>
        /// Combines array of buffers together as a single image.
        /// </summary>
        /// <param name="buffers">Array of byte buffers.</param>
        /// <returns>Combined buffers.</returns>
        public static byte[] Combine(this byte[][] buffers)
        {
            MemoryStream combinedBuffer = new MemoryStream();

            // Combine all currently queued buffers
            for (int x = 0; x < buffers.Length; x++)
            {
                combinedBuffer.Write(buffers[x], 0, buffers[x].Length);
            }

            // return combined data buffers
            return combinedBuffer.ToArray();
        }

        /// <summary>
        /// Searches for the specified sequence of <paramref name="bytesToFind"/> and returns the index of the first occurrence within the <paramref name="buffer"/>.
        /// </summary>
        /// <param name="buffer">Buffer to search.</param>
        /// <param name="bytesToFind">Byte sequence to search for.</param>
        /// <returns>The zero-based index of the first occurance of the sequence of <paramref name="bytesToFind"/> in the <paramref name="buffer"/>, if found; otherwise, -1.</returns>
        public static int IndexOfSequence(this byte[] buffer, byte[] bytesToFind)
        {
            return buffer.IndexOfSequence(bytesToFind, 0, buffer.Length);
        }

        /// <summary>
        /// Searches for the specified sequence of <paramref name="bytesToFind"/> and returns the index of the first occurrence within the range of elements in the <paramref name="buffer"/> that starts at the specified index.
        /// </summary>
        /// <param name="buffer">Buffer to search.</param>
        /// <param name="bytesToFind">Byte sequence to search for.</param>
        /// <param name="startIndex">Start index in the <paramref name="buffer"/> to start searching.</param>
        /// <returns>The zero-based index of the first occurance of the sequence of <paramref name="bytesToFind"/> in the <paramref name="buffer"/>, if found; otherwise, -1.</returns>
        public static int IndexOfSequence(this byte[] buffer, byte[] bytesToFind, int startIndex)
        {
            return buffer.IndexOfSequence(bytesToFind, startIndex, buffer.Length - startIndex);
        }

        /// <summary>
        /// Searches for the specified sequence of <paramref name="bytesToFind"/> and returns the index of the first occurrence within the range of elements in the <paramref name="buffer"/> that starts at the specified index and contains the specified number of elements.
        /// </summary>
        /// <param name="buffer">Buffer to search.</param>
        /// <param name="bytesToFind">Byte sequence to search for.</param>
        /// <param name="startIndex">Start index in the <paramref name="buffer"/> to start searching.</param>
        /// <param name="length">Number of bytes in the <paramref name="buffer"/> to search through.</param>
        /// <returns>The zero-based index of the first occurance of the sequence of <paramref name="bytesToFind"/> in the <paramref name="buffer"/>, if found; otherwise, -1.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="bytesToFind"/> is null or has zero length.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="startIndex"/> is outside the range of valid indexes for the source buffer -or-
        /// <paramref name="length"/> is less than 0.
        /// </exception>
        public static int IndexOfSequence(this byte[] buffer, byte[] bytesToFind, int startIndex, int length)
        {
            if (bytesToFind == null || bytesToFind.Length == 0)
                throw new ArgumentNullException("bytesToFind");

            if (startIndex < 0)
                throw new ArgumentOutOfRangeException("startIndex", "cannot be negative");

            if (length < 0)
                throw new ArgumentOutOfRangeException("length", "cannot be negative");

            if (startIndex >= buffer.Length)
                throw new ArgumentOutOfRangeException("startIndex", "not a valid index into source buffer");

            if (startIndex + length > buffer.Length)
                throw new ArgumentOutOfRangeException("length", "exceeds buffer size");

            // Search for first byte in the sequence, if this doesn't exist then sequence doesn't exist
            int index = Array.IndexOf(buffer, bytesToFind[0], startIndex, length);

            if (bytesToFind.Length > 1)
            {
                bool foundSequence = false;

                while (index > 0 && !foundSequence)
                {
                    // See if next bytes in sequence match
                    for (int x = 1; x < bytesToFind.Length; x++)
                    {
                        // Make sure there's enough buffer remaining to accomodate this byte
                        if (index + x < startIndex + length)
                        {
                            // If sequence doesn't match, search for next first-byte
                            if (buffer[index + x] != bytesToFind[x])
                            {
                                index = Array.IndexOf(buffer, bytesToFind[0], index + 1, length - (index - startIndex));
                                break;
                            }

                            // If each byte to find matched, we found the sequence
                            foundSequence = (x == bytesToFind.Length - 1);
                        }
                        else
                        {
                            // Ran out of buffer, return -1
                            index = -1;
                        }
                    }
                }
            }

            return index;
        }

        /// <summary>Returns comparision results of two binary buffers.</summary>
        /// <param name="source">Source buffer.</param>
        /// <param name="other">Other buffer to compare to <paramref name="source"/> buffer.</param>
        /// <returns>
        /// <para>
        /// A signed integer that indicates the relative comparison of <paramref name="source"/> buffer and <paramref name="other"/> buffer.
        /// </para>
        /// <para>
        /// <list type="table">
        ///     <listheader>
        ///         <term>Return Value</term>
        ///         <description>Description</description>
        ///     </listheader>
        ///     <item>
        ///         <term>Less than zero</term>
        ///         <description>Source buffer is less than other buffer.</description>
        ///     </item>
        ///     <item>
        ///         <term>Zero</term>
        ///         <description>Source buffer is equal to other buffer.</description>
        ///     </item>
        ///     <item>
        ///         <term>Greater than zero</term>
        ///         <description>Source buffer is greater than other buffer.</description>
        ///     </item>
        /// </list>
        /// </para>
        /// </returns>
        public static int CompareTo(this byte[] source, byte[] other)
        {
            if (source == null && other == null)
            {
                // Both buffers are assumed equal if both are nothing.
                return 0;
            }
            else if (source == null)
            {
                // Buffer 2 has data, and buffer 1 is nothing. Buffer 2 is assumed larger.
                return 1;
            }
            else if (other == null)
            {
                // Buffer 1 has data, and buffer 2 is nothing. Buffer 1 is assumed larger.
                return -1;
            }
            else
            {
                int length1 = source.Length;
                int length2 = other.Length;

                if (length1 == length2)
                {
                    int comparision = 0;

                    // Compares elements of buffers that are of equal size.
                    for (int x = 0; x < length1; x++)
                    {
                        comparision = source[x].CompareTo(other[x]);

                        if (comparision != 0)
                            break;
                    }

                    return comparision;
                }
                else
                {
                    // Buffer lengths are unequal. Buffer with largest number of elements is assumed to be largest.
                    return length1.CompareTo(length2);
                }
            }
        }

        /// <summary>
        /// Returns comparision results of two binary buffers.
        /// </summary>
        /// <param name="source">Source buffer.</param>
        /// <param name="sourceOffset">Offset into <paramref name="source"/> buffer to begin compare.</param>
        /// <param name="other">Other buffer to compare to <paramref name="source"/> buffer.</param>
        /// <param name="otherOffset">Offset into <paramref name="other"/> buffer to begin compare.</param>
        /// <param name="count">Number of bytes to compare in both buffers.</param>
        /// <returns>
        /// <para>
        /// A signed integer that indicates the relative comparison of <paramref name="source"/> buffer and <paramref name="other"/> buffer.
        /// </para>
        /// <para>
        /// <list type="table">
        ///     <listheader>
        ///         <term>Return Value</term>
        ///         <description>Description</description>
        ///     </listheader>
        ///     <item>
        ///         <term>Less than zero</term>
        ///         <description>Source buffer is less than other buffer.</description>
        ///     </item>
        ///     <item>
        ///         <term>Zero</term>
        ///         <description>Source buffer is equal to other buffer.</description>
        ///     </item>
        ///     <item>
        ///         <term>Greater than zero</term>
        ///         <description>Source buffer is greater than other buffer.</description>
        ///     </item>
        /// </list>
        /// </para>
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="sourceOffset"/> or <paramref name="otherOffset"/> is outside the range of valid indexes for the associated buffer -or-
        /// <paramref name="count"/> is less than 0 -or- 
        /// <paramref name="sourceOffset"/> or <paramref name="otherOffset"/> and <paramref name="count"/> do not specify a valid section in the the associated buffer.
        /// </exception>
        public static int CompareTo(this byte[] source, int sourceOffset, byte[] other, int otherOffset, int count)
        {
            if (source == null && other == null)
            {
                // Both buffers are assumed equal if both are nothing.
                return 0;
            }
            else if (source == null)
            {
                // Buffer 2 has data, and buffer 1 is nothing. Buffer 2 is assumed larger.
                return 1;
            }
            else if (other == null)
            {
                // Buffer 1 has data, and buffer 2 is nothing. Buffer 1 is assumed larger.
                return -1;
            }
            else
            {
                if (sourceOffset < 0)
                    throw new ArgumentOutOfRangeException("sourceOffset", "cannot be negative");
                
                if (otherOffset < 0)
                    throw new ArgumentOutOfRangeException("otherOffset", "cannot be negative");

                if (count < 0)
                    throw new ArgumentOutOfRangeException("count", "cannot be negative");

                if (sourceOffset >= source.Length)
                    throw new ArgumentOutOfRangeException("sourceOffset", "not a valid index into source buffer");

                if (otherOffset >= other.Length)
                    throw new ArgumentOutOfRangeException("otherOffset", "not a valid index into other buffer");

                if (sourceOffset + count > source.Length)
                    throw new ArgumentOutOfRangeException("count", "exceeds source buffer size");

                if (otherOffset + count > other.Length)
                    throw new ArgumentOutOfRangeException("count", "exceeds other buffer size");

                int comparision = 0;

                // Compares elements of buffers that are of equal size.
                for (int x = 0; x < count; x++)
                {
                    comparision = source[sourceOffset + x].CompareTo(other[otherOffset + x]);

                    if (comparision != 0)
                        break;
                }

                return comparision;
            }
        }
    }
}
