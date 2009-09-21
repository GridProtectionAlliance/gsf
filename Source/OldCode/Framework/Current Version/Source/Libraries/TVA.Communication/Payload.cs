//*******************************************************************************************************
//  Payload.cs - Gbtc
//
//  Tennessee Valley Authority, 2009
//  No copyright is claimed pursuant to 17 USC § 105.  All Other Rights Reserved.
//
//  This software is made freely available under the TVA Open Source Agreement (see below).
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  07/06/2006 - Pinal C. Patel
//       Original version of source code generated.
//  09/29/2008 - J. Ritchie Carroll
//       Converted to C#.
//  09/14/2009 - Stephen C. Wills
//       Added new header and license agreement.
//  09/21/2009 - Pinal C. Patel
//       Fixed a bug in AddHeader() that was putting the length of the provided buffer in the header 
//       instead of the length specified in the method parameter.
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
using System.Text;
using TVA.IO.Compression;
using TVA.Security.Cryptography;

namespace TVA.Communication
{
    /// <summary>
    /// A helper class containing methods for manipulation of payload.
    /// </summary>
    public static class Payload
    {
        /// <summary>
        /// Specifies the length of the segment in a "Payload-Aware" transmission that contains the payload length.
        /// </summary>
        public const int LengthSegment = 4;

        /// <summary>
        /// Default byte sequence used to mark the beginning of the payload in a "Payload-Aware" transmissions.
        /// </summary>
        public static byte[] DefaultMarker = { 0xAA, 0xBB, 0xCC, 0xDD };

        /// <summary>
        /// Adds header containing the <paramref name="marker"/> to the payload in the <paramref name="buffer"/> for "Payload-Aware" transmission.
        /// </summary>
        /// <param name="buffer">The buffer containing the payload.</param>
        /// <param name="offset">The offset in the <paramref name="buffer"/> at which the payload starts.</param>
        /// <param name="length">The lenght of the payload in the <paramref name="buffer"/> starting at the <paramref name="offset"/>.</param>
        /// <param name="marker">The byte sequence used to mark the beginning of the payload in a "Payload-Aware" transmissions.</param>
        public static void AddHeader(ref byte[] buffer, ref int offset, ref int length, byte[] marker)
        {
            // The resulting buffer will be at least 4 bytes bigger than the payload.

            // Resulting buffer = x bytes for payload marker + 4 bytes for the payload size + The payload
            byte[] result = new byte[(length - offset) + marker.Length + LengthSegment];

            // First, copy the the payload marker to the buffer.
            Buffer.BlockCopy(marker, 0, result, 0, marker.Length);
            // Then, copy the payload's size to the buffer after the payload marker.
            Buffer.BlockCopy(BitConverter.GetBytes(length), 0, result, marker.Length, LengthSegment);
            // At last, copy the payload after the payload marker and payload size.
            Buffer.BlockCopy(buffer, offset, result, marker.Length + LengthSegment, length);

            buffer = result;
            offset = 0;
            length = buffer.Length;
        }

        /// <summary>
        /// Determines whether or not the <paramref name="buffer"/> contains the header information of a "Payload-Aware" transmission.
        /// </summary>
        /// <param name="buffer">The buffer to be checked at index zero.</param>
        /// <param name="marker">The byte sequence used to mark the beginning of the payload in a "Payload-Aware" transmissions.</param>
        /// <returns>true if the buffer contains "Payload-Aware" transmission header; otherwise false.</returns>
        public static bool HasHeader(byte[] buffer, byte[] marker)
        {
            for (int i = 0; i <= marker.Length - 1; i++)
            {
                if (buffer[i] != marker[i])
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Determines the length of a payload in a "Payload-Aware" transmission from the payload header information.
        /// </summary>
        /// <param name="buffer">The buffer containg payload header information starting at index zero.</param>
        /// <param name="marker">The byte sequence used to mark the beginning of the payload in a "Payload-Aware" transmissions.</param>
        /// <returns>Length of the payload.</returns>
        public static int ExtractLength(byte[] buffer, byte[] marker)
        {
            if (buffer.Length >= (marker.Length + LengthSegment) && HasHeader(buffer, marker))
            {
                // We have a buffer that's at least as big as the payload header and has the payload marker.
                return BitConverter.ToInt32(buffer, marker.Length);
            }
            else
            {
                return -1;
            }
        }

        /// <summary>
        /// Performs the necessary uncompression and decryption on the data contained in the <paramref name="buffer"/>.
        /// </summary>
        /// <param name="buffer">The buffer containing the data to be processed.</param>
        /// <param name="offset">The offset in the <paramref name="buffer"/> from where data is to be processed.</param>
        /// <param name="length">The length of data in <paramref name="buffer"/> starting at the <paramref name="offset"/>.</param>
        /// <param name="cryptoLevel">One of the <see cref="CipherStrength"/> values.</param>
        /// <param name="cryptoKey">The key to be used for decrypting the data in the <paramref name="buffer"/>.</param>
        /// <param name="compressLevel">One of the <see cref="CompressionStrength"/> values.</param>
        public static void ProcessReceived(ref byte[] buffer, ref int offset, ref int length, CipherStrength cryptoLevel, string cryptoKey, CompressionStrength compressLevel)
        {
            if (cryptoLevel != CipherStrength.None || compressLevel != CompressionStrength.NoCompression)
            {
                // Make a copy of the data to be processed.
                byte[] temp = buffer.BlockCopy(offset, length);

                if (cryptoLevel != CipherStrength.None)
                {
                    // Decrypt the data.
                    temp = temp.Decrypt(Encoding.ASCII.GetBytes(cryptoKey), cryptoLevel);
                    offset = 0;
                    length = temp.Length;
                }
                
                if (compressLevel != CompressionStrength.NoCompression)
                {
                    // Uncompress the data.
                    temp = new MemoryStream(temp).Decompress().ToArray();
                    offset = 0;
                    length = temp.Length;
                }

                if (temp.Length > buffer.Length)
                    // Processed data cannot fit in the existing buffer.
                    buffer = temp;
                else
                    // Copy the processed data into the existing buffer.
                    Buffer.BlockCopy(temp, offset, buffer, offset, length);
            }

            //if (cryptoLevel == CipherStrength.None && compressLevel == CompressionStrength.NoCompression)
            //{
            //    if (length - offset == data.Length)
            //        return data;
            //    else
            //        return data.BlockCopy(offset, length);
            //}
            //else
            //{
            //    if (cryptoLevel != CipherStrength.None)
            //    {
            //        data = data.Decrypt(offset, length, Encoding.ASCII.GetBytes(cryptoKey), cryptoLevel);
            //        offset = 0;
            //        length = data.Length;
            //    }

            //    if (compressLevel != CompressionStrength.NoCompression)
            //    {
            //        data = new MemoryStream(data, offset, length).Decompress().ToArray();
            //    }

            //    return data;
            //}
        }

        /// <summary>
        /// Performs the necessary compression and encryption on the data contained in the <paramref name="buffer"/>.
        /// </summary>
        /// <param name="buffer">The buffer containing the data to be processed.</param>
        /// <param name="offset">The offset in the <paramref name="buffer"/> from where data is to be processed.</param>
        /// <param name="length">The length of data in <paramref name="buffer"/> starting at the <paramref name="offset"/>.</param>
        /// <param name="cryptoLevel">One of the <see cref="CipherStrength"/> values.</param>
        /// <param name="cryptoKey">The key to be used for encrypting the data in the <paramref name="buffer"/>.</param>
        /// <param name="compressLevel">One of the <see cref="CompressionStrength"/> values.</param>
        public static void ProcessTransmit(ref byte[] buffer, ref int offset, ref int length, CipherStrength cryptoLevel, string cryptoKey, CompressionStrength compressLevel)
        {
            if (compressLevel != CompressionStrength.NoCompression)
            {
                // Compress the data.
                buffer = new MemoryStream(buffer, offset, length).Compress(compressLevel).ToArray();
                offset = 0;
                length = buffer.Length;
            }

            if (cryptoLevel != CipherStrength.None)
            {
                // Encrypt the data.
                buffer = buffer.Encrypt(offset, length, Encoding.ASCII.GetBytes(cryptoKey), cryptoLevel);
                offset = 0;
                length = buffer.Length;
            }

            //if (cryptoLevel == CipherStrength.None && compressLevel == CompressionStrength.NoCompression)
            //{
            //    if (length - offset == data.Length)
            //        return data;
            //    else
            //        return data.BlockCopy(offset, length);
            //}
            //else
            //{
            //    if (compressLevel != CompressionStrength.NoCompression)
            //    {
            //        // Compress the data.
            //        data = new MemoryStream(data, offset, length).Compress(compressLevel).ToArray();
            //        offset = 0;
            //        length = data.Length;
            //    }

            //    if (cryptoLevel != CipherStrength.None)
            //    {
            //        // Encrypt the data.
            //        data = data.Encrypt(offset, length, Encoding.ASCII.GetBytes(cryptoKey), cryptoLevel);
            //    }

            //    return data;
            //}
        }
    }
}