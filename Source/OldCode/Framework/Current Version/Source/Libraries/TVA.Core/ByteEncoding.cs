//*******************************************************************************************************
//  ByteEncoding.cs - Gbtc
//
//  Tennessee Valley Authority, 2009
//  No copyright is claimed pursuant to 17 USC § 105.  All Other Rights Reserved.
//
//  This software is made freely available under the TVA Open Source Agreement (see below).
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  01/25/2008 - J. Ritchie Carroll
//       Initial version of source generated.
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

/**************************************************************************\
   Copyright © 2009 - Gbtc, James Ritchie Carroll
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
  
\**************************************************************************/

using System;
using System.ComponentModel;
using System.Text;
using System.Text.RegularExpressions;

namespace TVA
{
    /// <summary>
    /// Defines a set of methods used to convert byte buffers to and from user presentable data formats.
    /// </summary>
    public abstract class ByteEncoding
    {
        #region [ Members ]

        // Nested Types
        #region [ Hexadecimal Encoding Class ]

        /// <summary>
        /// Handles conversion of byte buffers to and from a hexadecimal data format.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public class HexadecimalEncoding : ByteEncoding
        {
            internal HexadecimalEncoding()
            {
                // This class is meant for internal instatiation only.
            }

            /// <summary>Decodes given string back into a byte buffer.</summary>
            /// <param name="hexData">Encoded hexadecimal data string to decode.</param>
            /// <param name="spacingCharacter">Original spacing character that was inserted between encoded bytes.</param>
            /// <returns>Decoded bytes.</returns>
            public override byte[] GetBytes(string hexData, char spacingCharacter)
            {
                if (!string.IsNullOrEmpty(hexData))
                {
                    // Removes spacing characters, if needed.
                    hexData = hexData.Trim();
                    if (spacingCharacter != NoSpacing) hexData = hexData.Replace(spacingCharacter.ToString(), "");

                    // Processes the string only if it has data in hex format (Example: 48 65 6C 6C 21).
                    if (Regex.Matches(hexData, "[^a-fA-F0-9]").Count == 0)
                    {
                        // Trims the end of the string to discard any additional characters, if present in the string,
                        // that would prevent the string from being a hex encoded string.
                        // Note: Requires that each character be represented by its 2 character hex value.
                        hexData = hexData.Substring(0, hexData.Length - hexData.Length % 2);

                        byte[] bytes = new byte[hexData.Length / 2];
                        int index = 0;

                        for (int x = 0; x <= hexData.Length - 1; x += 2)
                        {
                            bytes[index] = Convert.ToByte(hexData.Substring(x, 2), 16);
                            index++;
                        }

                        return bytes;
                    }
                    else
                    {
                        throw new ArgumentException("Input string is not a valid hex encoded string - invalid characters encountered", "hexData");
                    }
                }
                else
                {
                    throw new ArgumentNullException("hexData", "Input string cannot be null or empty");
                }
            }

            /// <summary>Encodes given buffer into a user presentable representation.</summary>
            /// <param name="bytes">Bytes to encode.</param>
            /// <param name="offset">Offset into buffer to begin encoding.</param>
            /// <param name="length">Length of buffer to encode.</param>
            /// <param name="spacingCharacter">Spacing character to place between encoded bytes.</param>
            /// <returns>String of encoded bytes.</returns>
            public override string GetString(byte[] bytes, int offset, int length, char spacingCharacter)
            {
                return BytesToString(bytes, offset, length, spacingCharacter, "X2");
            }
        }

        #endregion

        #region [ Decimal Encoding Class ]

        /// <summary>
        /// Handles conversion of byte buffers to and from a decimal data format.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public class DecimalEncoding : ByteEncoding
        {
            internal DecimalEncoding()
            {
                // This class is meant for internal instatiation only.
            }

            /// <summary>Decodes given string back into a byte buffer.</summary>
            /// <param name="decData">Encoded decimal data string to decode.</param>
            /// <param name="spacingCharacter">Original spacing character that was inserted between encoded bytes.</param>
            /// <returns>Decoded bytes.</returns>
            public override byte[] GetBytes(string decData, char spacingCharacter)
            {
                if (!string.IsNullOrEmpty(decData))
                {
                    // Removes spacing characters, if needed.
                    decData = decData.Trim();
                    if (spacingCharacter != NoSpacing) decData = decData.Replace(spacingCharacter.ToString(), "");

                    // Processes the string only if it has data in decimal format (Example: 072 101 108 108 033).
                    if (Regex.Matches(decData, "[^0-9]").Count == 0)
                    {
                        // Trims the end of the string to discard any additional characters, if present in the
                        // string, that would prevent the string from being an integer encoded string.
                        // Note: Requires that each character be represented by its 3 character decimal value.
                        decData = decData.Substring(0, decData.Length - decData.Length % 3);

                        byte[] bytes = new byte[decData.Length / 3];
                        int index = 0;

                        for (int x = 0; x <= decData.Length - 1; x += 3)
                        {
                            bytes[index] = Convert.ToByte(decData.Substring(x, 3), 10);
                            index++;
                        }

                        return bytes;
                    }
                    else
                    {
                        throw new ArgumentException("Input string is not a valid decimal encoded string - invalid characters encountered", "decData");
                    }
                }
                else
                {
                    throw new ArgumentNullException("decData", "Input string cannot be null or empty");
                }
            }

            /// <summary>Encodes given buffer into a user presentable representation.</summary>
            /// <param name="bytes">Bytes to encode.</param>
            /// <param name="offset">Offset into buffer to begin encoding.</param>
            /// <param name="length">Length of buffer to encode.</param>
            /// <param name="spacingCharacter">Spacing character to place between encoded bytes.</param>
            /// <returns>String of encoded bytes.</returns>
            public override string GetString(byte[] bytes, int offset, int length, char spacingCharacter)
            {
                return BytesToString(bytes, offset, length, spacingCharacter, "D3");
            }
        }

        #endregion

        #region [ Binary Encoding Class ]

        /// <summary>
        /// Handles conversion of byte buffers to and from a binary (i.e., 0 and 1's) data format.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public class BinaryEncoding : ByteEncoding
        {
            private string[] m_byteImages;
            private bool m_reverse;

            // This class is meant for internal instantiation only.
            internal BinaryEncoding(Endianness targetEndianness)
            {
                if (targetEndianness == Endianness.BigEndian)
                {
                    if (BitConverter.IsLittleEndian)
                    {
                        // If OS is little endian and we want big endian, this reverses the bit order.
                        m_reverse = true;
                    }
                    else
                    {
                        // If OS is big endian and we want big endian, this keeps the OS bit order.
                        m_reverse = false;
                    }
                }
                else
                {
                    if (BitConverter.IsLittleEndian)
                    {
                        // If OS is little endian and we want little endian, this keeps OS bit order.
                        m_reverse = false;
                    }
                    else
                    {
                        // If OS is big endian and we want little endian, this reverses the bit order.
                        m_reverse = true;
                    }
                }
            }

            /// <summary>Decodes given string back into a byte buffer.</summary>
            /// <param name="binaryData">Encoded binary data string to decode.</param>
            /// <param name="spacingCharacter">Original spacing character that was inserted between encoded bytes.</param>
            /// <returns>Decoded bytes.</returns>
            public override byte[] GetBytes(string binaryData, char spacingCharacter)
            {
                if (!string.IsNullOrEmpty(binaryData))
                {
                    // Removes spacing characters, if needed.
                    binaryData = binaryData.Trim();
                    if (spacingCharacter != NoSpacing) binaryData = binaryData.Replace(spacingCharacter.ToString(), "");

                    // Processes the string only if it has data in binary format (Example: 01010110 1010101).
                    if (Regex.Matches(binaryData, "[^0-1]").Count == 0)
                    {
                        // Trims the end of the string to discard any additional characters, if present in the
                        // string, that would prevent the string from being a binary encoded string.
                        // Note: Requires each character be represented by its 8 character binary value.
                        binaryData = binaryData.Substring(0, binaryData.Length - binaryData.Length % 8);

                        byte[] bytes = new byte[binaryData.Length / 8];
                        int index = 0;

                        for (int x = 0; x <= binaryData.Length - 1; x += 8)
                        {
                            bytes[index] = (byte)Bits.Nil;

                            if (m_reverse)
                            {
                                if (binaryData[x + 7] == '1') bytes[index] = bytes[index].SetBits(Bits.Bit00);
                                if (binaryData[x + 6] == '1') bytes[index] = bytes[index].SetBits(Bits.Bit01);
                                if (binaryData[x + 5] == '1') bytes[index] = bytes[index].SetBits(Bits.Bit02);
                                if (binaryData[x + 4] == '1') bytes[index] = bytes[index].SetBits(Bits.Bit03);
                                if (binaryData[x + 3] == '1') bytes[index] = bytes[index].SetBits(Bits.Bit04);
                                if (binaryData[x + 2] == '1') bytes[index] = bytes[index].SetBits(Bits.Bit05);
                                if (binaryData[x + 1] == '1') bytes[index] = bytes[index].SetBits(Bits.Bit06);
                                if (binaryData[x + 0] == '1') bytes[index] = bytes[index].SetBits(Bits.Bit07);
                            }
                            else
                            {
                                if (binaryData[x + 0] == '1') bytes[index] = bytes[index].SetBits(Bits.Bit00);
                                if (binaryData[x + 1] == '1') bytes[index] = bytes[index].SetBits(Bits.Bit01);
                                if (binaryData[x + 2] == '1') bytes[index] = bytes[index].SetBits(Bits.Bit02);
                                if (binaryData[x + 3] == '1') bytes[index] = bytes[index].SetBits(Bits.Bit03);
                                if (binaryData[x + 4] == '1') bytes[index] = bytes[index].SetBits(Bits.Bit04);
                                if (binaryData[x + 5] == '1') bytes[index] = bytes[index].SetBits(Bits.Bit05);
                                if (binaryData[x + 6] == '1') bytes[index] = bytes[index].SetBits(Bits.Bit06);
                                if (binaryData[x + 7] == '1') bytes[index] = bytes[index].SetBits(Bits.Bit07);
                            }

                            index++;
                        }

                        return bytes;
                    }
                    else
                    {
                        throw new ArgumentException("Input string is not a valid binary encoded string - invalid characters encountered", "binaryData");
                    }
                }
                else
                {
                    throw new ArgumentNullException("binaryData", "Input string cannot be null or empty");
                }
            }

            /// <summary>Encodes given buffer into a user presentable representation.</summary>
            /// <param name="bytes">Bytes to encode.</param>
            /// <param name="offset">Offset into buffer to begin encoding.</param>
            /// <param name="length">Length of buffer to encode.</param>
            /// <param name="spacingCharacter">Spacing character to place between encoded bytes.</param>
            /// <returns>String of encoded bytes.</returns>
            public override string GetString(byte[] bytes, int offset, int length, char spacingCharacter)
            {
                if (bytes == null) throw new ArgumentNullException("bytes", "Input buffer cannot be null");

                // Initializes byte image array on first call for speed in future calls.
                if (m_byteImages == null)
                {
                    StringBuilder byteImage;

                    m_byteImages = new string[256];

                    for (int imageByte = byte.MinValue; imageByte <= byte.MaxValue; imageByte++)
                    {
                        byteImage = new StringBuilder();

                        if (m_reverse)
                        {
                            if (imageByte.CheckBits(Bits.Bit07)) byteImage.Append('1'); else byteImage.Append('0');
                            if (imageByte.CheckBits(Bits.Bit06)) byteImage.Append('1'); else byteImage.Append('0');
                            if (imageByte.CheckBits(Bits.Bit05)) byteImage.Append('1'); else byteImage.Append('0');
                            if (imageByte.CheckBits(Bits.Bit04)) byteImage.Append('1'); else byteImage.Append('0');
                            if (imageByte.CheckBits(Bits.Bit03)) byteImage.Append('1'); else byteImage.Append('0');
                            if (imageByte.CheckBits(Bits.Bit02)) byteImage.Append('1'); else byteImage.Append('0');
                            if (imageByte.CheckBits(Bits.Bit01)) byteImage.Append('1'); else byteImage.Append('0');
                            if (imageByte.CheckBits(Bits.Bit00)) byteImage.Append('1'); else byteImage.Append('0');
                        }
                        else
                        {
                            if (imageByte.CheckBits(Bits.Bit00)) byteImage.Append('1'); else byteImage.Append('0');
                            if (imageByte.CheckBits(Bits.Bit01)) byteImage.Append('1'); else byteImage.Append('0');
                            if (imageByte.CheckBits(Bits.Bit02)) byteImage.Append('1'); else byteImage.Append('0');
                            if (imageByte.CheckBits(Bits.Bit03)) byteImage.Append('1'); else byteImage.Append('0');
                            if (imageByte.CheckBits(Bits.Bit04)) byteImage.Append('1'); else byteImage.Append('0');
                            if (imageByte.CheckBits(Bits.Bit05)) byteImage.Append('1'); else byteImage.Append('0');
                            if (imageByte.CheckBits(Bits.Bit06)) byteImage.Append('1'); else byteImage.Append('0');
                            if (imageByte.CheckBits(Bits.Bit07)) byteImage.Append('1'); else byteImage.Append('0');
                        }

                        m_byteImages[imageByte] = byteImage.ToString();
                    }
                }

                StringBuilder binaryImage = new StringBuilder();

                for (int x = 0; x < length; x++)
                {
                    if (spacingCharacter != NoSpacing && x > 0) binaryImage.Append(spacingCharacter);
                    binaryImage.Append(m_byteImages[bytes[offset + x]]);
                }

                return binaryImage.ToString();
            }
        }

        #endregion

        #region [ Base64 Encoding Class ]

        /// <summary>
        /// Handles conversion of byte buffers to and from a base64 data format.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public class Base64Encoding : ByteEncoding
        {
            internal Base64Encoding()
            {
                // This class is meant for internal instantiation only.
            }

            /// <summary>Decodes given string back into a byte buffer.</summary>
            /// <param name="binaryData">Encoded binary data string to decode.</param>
            /// <param name="spacingCharacter">Original spacing character that was inserted between encoded bytes.</param>
            /// <returns>Decoded bytes.</returns>
            public override byte[] GetBytes(string binaryData, char spacingCharacter)
            {
                // Removes spacing characters, if needed.
                binaryData = binaryData.Trim();
                if (spacingCharacter != NoSpacing) binaryData = binaryData.Replace(spacingCharacter.ToString(), "");
                return Convert.FromBase64String(binaryData);
            }

            /// <summary>Encodes given buffer into a user presentable representation.</summary>
            /// <param name="bytes">Bytes to encode.</param>
            /// <param name="offset">Offset into buffer to begin encoding.</param>
            /// <param name="length">Length of buffer to encode.</param>
            /// <param name="spacingCharacter">Spacing character to place between encoded bytes.</param>
            /// <returns>String of encoded bytes.</returns>
            public override string GetString(byte[] bytes, int offset, int length, char spacingCharacter)
            {
                if (bytes == null) throw new ArgumentNullException("bytes", "Input buffer cannot be null");

                string base64String = Convert.ToBase64String(bytes, offset, length);

                if (spacingCharacter == NoSpacing)
                {
                    return base64String;
                }
                else
                {
                    StringBuilder base64Image = new StringBuilder();

                    for (int x = 0; x <= base64String.Length - 1; x++)
                    {
                        if (x > 0) base64Image.Append(spacingCharacter);
                        base64Image.Append(base64String[x]);
                    }

                    return base64Image.ToString();
                }
            }
        }

        #endregion

        #region [ ASCII Encoding Class ]

        /// <summary>
        /// Handles conversion of byte buffers to and from a ASCII data format.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public class ASCIIEncoding : ByteEncoding
        {
            internal ASCIIEncoding()
            {
                // This class is meant for internal instantiation only.
            }

            /// <summary>Decodes given string back into a byte buffer.</summary>
            /// <param name="binaryData">Encoded binary data string to decode.</param>
            /// <param name="spacingCharacter">Original spacing character that was inserted between encoded bytes.</param>
            /// <returns>Decoded bytes.</returns>
            public override byte[] GetBytes(string binaryData, char spacingCharacter)
            {
                // Removes spacing characters, if needed.
                binaryData = binaryData.Trim();
                if (spacingCharacter != NoSpacing) binaryData = binaryData.Replace(spacingCharacter.ToString(), "");
                return Encoding.ASCII.GetBytes(binaryData);
            }

            /// <summary>Encodes given buffer into a user presentable representation.</summary>
            /// <param name="bytes">Bytes to encode.</param>
            /// <param name="offset">Offset into buffer to begin encoding.</param>
            /// <param name="length">Length of buffer to encode.</param>
            /// <param name="spacingCharacter">Spacing character to place between encoded bytes.</param>
            /// <returns>String of encoded bytes.</returns>
            public override string GetString(byte[] bytes, int offset, int length, char spacingCharacter)
            {
                if (bytes == null) throw new ArgumentNullException("bytes", "Input buffer cannot be null");

                string asciiString = Encoding.ASCII.GetString(bytes, offset, length);

                if (spacingCharacter == NoSpacing)
                {
                    return asciiString;
                }
                else
                {
                    StringBuilder asciiImage = new StringBuilder();

                    for (int x = 0; x <= asciiString.Length - 1; x++)
                    {
                        if (x > 0) asciiImage.Append(spacingCharacter);
                        asciiImage.Append(asciiString[x]);
                    }

                    return asciiImage.ToString();
                }
            }
        }

        #endregion

        /// <summary>
        /// Constant used to specify that "no spacing" should be used for data conversion.
        /// </summary>
        public const char NoSpacing = char.MinValue;

        #endregion

        #region [ Methods ]

        /// <summary>Encodes given buffer into a user presentable representation.</summary>
        /// <param name="bytes">Bytes to encode.</param>
        /// <returns>String representation of byte array.</returns>
        public virtual string GetString(byte[] bytes)
        {
            return GetString(bytes, NoSpacing);
        }

        /// <summary>Encodes given buffer into a user presentable representation.</summary>
        /// <param name="bytes">Bytes to encode.</param>
        /// <param name="spacingCharacter">Spacing character to place between encoded bytes.</param>
        /// <returns>String of encoded bytes.</returns>
        public virtual string GetString(byte[] bytes, char spacingCharacter)
        {
            if (bytes == null) throw new ArgumentNullException("bytes", "Input buffer cannot be null");
            return GetString(bytes, 0, bytes.Length, spacingCharacter);
        }

        /// <summary>Encodes given buffer into a user presentable representation.</summary>
        /// <param name="bytes">Bytes to encode.</param>
        /// <param name="offset">Offset into buffer to begin encoding.</param>
        /// <param name="length">Length of buffer to encode.</param>
        /// <returns>String of encoded bytes.</returns>
        public virtual string GetString(byte[] bytes, int offset, int length)
        {
            if (bytes == null) throw new ArgumentNullException("bytes", "Input buffer cannot be null");
            return GetString(bytes, offset, length, NoSpacing);
        }

        /// <summary>Encodes given buffer into a user presentable representation.</summary>
        /// <param name="bytes">Bytes to encode.</param>
        /// <param name="offset">Offset into buffer to begin encoding.</param>
        /// <param name="length">Length of buffer to encode.</param>
        /// <param name="spacingCharacter">Spacing character to place between encoded bytes.</param>
        /// <returns>String of encoded bytes.</returns>
        public abstract string GetString(byte[] bytes, int offset, int length, char spacingCharacter);

        /// <summary>Decodes given string back into a byte buffer.</summary>
        /// <param name="value">Encoded string to decode.</param>
        /// <returns>Decoded bytes.</returns>
        public virtual byte[] GetBytes(string value)
        {
            if (string.IsNullOrEmpty(value)) throw new ArgumentNullException("value", "Input string cannot be null");
            return GetBytes(value, NoSpacing);
        }

        /// <summary>Decodes given string back into a byte buffer.</summary>
        /// <param name="value">Encoded string to decode.</param>
        /// <param name="spacingCharacter">Original spacing character that was inserted between encoded bytes.</param>
        /// <returns>Decoded bytes</returns>
        public abstract byte[] GetBytes(string value, char spacingCharacter);

        #endregion

        #region [ Static ]

        // Static Fields
        private static ByteEncoding m_hexadecimalEncoding;
        private static ByteEncoding m_decimalEncoding;
        private static ByteEncoding m_bigEndianBinaryEncoding;
        private static ByteEncoding m_littleEndianBinaryEncoding;
        private static ByteEncoding m_base64Encoding;
        private static ByteEncoding m_asciiEncoding;

        /// <summary>Handles encoding and decoding of a byte buffer into a hexadecimal-based presentation format.</summary>
        public static ByteEncoding Hexadecimal
        {
            get
            {
                if (m_hexadecimalEncoding == null) m_hexadecimalEncoding = new HexadecimalEncoding();
                return m_hexadecimalEncoding;
            }
        }

        /// <summary>Handles encoding and decoding of a byte buffer into an integer-based presentation format.</summary>
        public static ByteEncoding Decimal
        {
            get
            {
                if (m_decimalEncoding == null) m_decimalEncoding = new DecimalEncoding();
                return m_decimalEncoding;
            }
        }

        /// <summary>Handles encoding and decoding of a byte buffer into a big-endian binary (i.e., 0 and 1's) based
        /// presentation format.</summary>
        /// <remarks>
        /// Although endianness is typically used in the context of byte order (see <see cref="TVA.EndianOrder"/> to handle byte
        /// order swapping), this property allows you visualize "bits" in big-endian order, right-to-left. Note that bits are
        /// normally stored in the same order as their bytes.).
        /// </remarks>
        public static ByteEncoding BigEndianBinary
        {
            get
            {
                if (m_bigEndianBinaryEncoding == null) m_bigEndianBinaryEncoding = new BinaryEncoding(Endianness.BigEndian);
                return m_bigEndianBinaryEncoding;
            }
        }

        /// <summary>Handles encoding and decoding of a byte buffer into a little-endian binary (i.e., 0 and 1's) based
        /// presentation format.</summary>
        /// <remarks>
        /// Although endianness is typically used in the context of byte order (see <see cref="TVA.EndianOrder"/> to handle byte
        /// order swapping), this property allows you visualize "bits" in little-endian order, left-to-right. Note that bits are
        /// normally stored in the same order as their bytes.
        /// </remarks>
        public static ByteEncoding LittleEndianBinary
        {
            get
            {
                if (m_littleEndianBinaryEncoding == null) m_littleEndianBinaryEncoding = new BinaryEncoding(Endianness.LittleEndian);
                return m_littleEndianBinaryEncoding;
            }
        }

        /// <summary>Handles encoding and decoding of a byte buffer into a base64 presentation format.</summary>
        public static ByteEncoding Base64
        {
            get
            {
                if (m_base64Encoding == null) m_base64Encoding = new Base64Encoding();
                return m_base64Encoding;
            }
        }

        /// <summary>Handles encoding and decoding of a byte buffer into an ASCII character presentation format.</summary>
        public static ByteEncoding ASCII
        {
            get
            {
                if (m_asciiEncoding == null) m_asciiEncoding = new ASCIIEncoding();
                return m_asciiEncoding;
            }
        }

        /// <summary>Handles byte to string conversions for implementations that are available from Byte.ToString.</summary>
        /// <param name="bytes">Encoded string to decode.</param>
        /// <param name="offset">Offset into byte array to begin decoding straing at.</param>
        /// <param name="length">Number of bytes to decode starting at <paramref name="offset"/></param>
        /// <param name="spacingCharacter">Character to insert between each byte</param>
        /// <param name="format">String decoding format.</param>
        /// <returns>Decoded string</returns>
        internal static string BytesToString(byte[] bytes, int offset, int length, char spacingCharacter, string format)
        {
            if (bytes == null) throw new ArgumentNullException("bytes", "Input buffer cannot be null");

            StringBuilder byteString = new StringBuilder();

            for (int x = 0; x <= length - 1; x++)
            {
                if (spacingCharacter != NoSpacing && x > 0) byteString.Append(spacingCharacter);
                byteString.Append(bytes[x + offset].ToString(format));
            }

            return byteString.ToString();
        }

        #endregion
    }
}