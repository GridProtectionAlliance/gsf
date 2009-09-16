//*******************************************************************************************************
//  BinaryValue.cs - Gbtc
//
//  Tennessee Valley Authority, 2009
//  No copyright is claimed pursuant to 17 USC § 105.  All Other Rights Reserved.
//
//  This software is made freely available under the TVA Open Source Agreement (see below).
//  Code in this file licensed to TVA under one or more contributor license agreements listed below.
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  01/25/2008 - J. Ritchie Carroll
//       Initial version of source generated.
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

#region [ Contributor License Agreements ]

/**************************************************************************\
   Copyright © 2009 - J. Ritchie Carroll
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

#endregion

using System;

namespace TVA
{
    /// <summary>
    /// Represents a binary data sample stored as a byte array ordered in the
    /// endianness of the OS, but implicitly castable to most common native types.
    /// </summary>
    public class BinaryValue : BinaryValueBase<NativeEndianOrder>
    {
        #region [ Constructors ]

        /// <summary>Creates a new binary value, ordered in the endianness of the OS, from the given byte array.</summary>
        /// <param name="buffer">The buffer which contains the binary representation of the value.</param>
        /// <param name="startIndex">The offset in the buffer where the data starts.</param>
        /// <param name="length">The number of data bytes that make up the binary value.</param>
        /// <exception cref="ArgumentNullException"><paramref name="buffer"/> is null</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="startIndex"/> is outside the range of the <paramref name="buffer"/> -or-
        /// <paramref name="length"/> is less than 0 -or-
        /// <paramref name="startIndex"/> and <paramref name="length"/> do not specify a valid region in the <paramref name="buffer"/>
        /// </exception>
        /// <remarks>This constructor assumes a type code of Empty to represent "undefined".</remarks>
        public BinaryValue(byte[] buffer, int startIndex, int length)
            : base(TypeCode.Empty, buffer, startIndex, length)
        {
        }

        /// <summary>Creates a new binary value, ordered in the endianness of the OS, from the given byte array.</summary>
        /// <param name="buffer">The buffer which contains the binary representation of the value.</param>
        /// <remarks>This constructor assumes a type code of Empty to represent "undefined".</remarks>
        public BinaryValue(byte[] buffer)
            : base(TypeCode.Empty, buffer, 0, buffer.Length)
        {
        }

        /// <summary>Creates a new binary value, ordered in the endianness of the OS, from the given byte array.</summary>
        /// <param name="typeCode">The type code of the native value that the binary value represents.</param>
        /// <param name="buffer">The buffer which contains the binary representation of the value.</param>
        /// <param name="startIndex">The offset in the buffer where the data starts.</param>
        /// <param name="length">The number of data bytes that make up the binary value.</param>
        /// <exception cref="ArgumentNullException"><paramref name="buffer"/> is null</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="startIndex"/> is outside the range of the <paramref name="buffer"/> -or-
        /// <paramref name="length"/> is less than 0 -or-
        /// <paramref name="startIndex"/> and <paramref name="length"/> do not specify a valid region in the <paramref name="buffer"/>
        /// </exception>
        public BinaryValue(TypeCode typeCode, byte[] buffer, int startIndex, int length)
            : base(typeCode, buffer, startIndex, length)
        {
        }

        /// <summary>Creates a new binary value, ordered in the endianness of the OS, from the given byte array.</summary>
        /// <param name="typeCode">The type code of the native value that the binary value represents.</param>
        /// <param name="buffer">The buffer which contains the binary representation of the value.</param>
        public BinaryValue(TypeCode typeCode, byte[] buffer)
            : base(typeCode, buffer, 0, buffer.Length)
        {
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Returns a <see cref="String"/> that represents the current <see cref="BinaryValue"/>.
        /// </summary>
        /// <returns>A <see cref="String"/> that represents the current <see cref="BinaryValue"/>.</returns>
        public override string ToString()
        {
            return ((Double)ConvertToType(TypeCode.Double)).ToString();
        }

        /// <summary>
        /// Returns a <see cref="BinaryValue"/> representation of source value converted to specified <see cref="TypeCode"/>.
        /// </summary>
        /// <param name="typeCode">Desired <see cref="TypeCode"/> for destination value.</param>
        /// <returns>A <see cref="BinaryValue"/> representation of source value converted to specified <see cref="TypeCode"/>.</returns>
        /// <exception cref="InvalidOperationException">Unable to convert binary value to specified type.</exception>
        public BinaryValue ConvertToType(TypeCode typeCode)
        {
            switch (this.TypeCode)
            {
                case TypeCode.Byte:
                    switch (typeCode)
                    {
                        case TypeCode.Byte:
                            return ToByte();
                        case TypeCode.Double:
                            return (Double)ToByte();
                        case TypeCode.Int16:
                            return (Int16)ToByte();
                        case TypeCode.Int32:
                            return (Int32)ToByte();
                        case TypeCode.Int64:
                            return (Int64)ToByte();
                        case TypeCode.Single:
                            return (Single)ToByte();
                        case TypeCode.UInt16:
                            return (UInt16)ToByte();
                        case TypeCode.UInt32:
                            return (UInt32)ToByte();
                        case TypeCode.UInt64:
                            return (UInt64)ToByte();
                        default:
                            throw new InvalidOperationException("Unable to convert binary value to " + typeCode);
                    }
                case TypeCode.Int16:
                    switch (typeCode)
                    {
                        case TypeCode.Byte:
                            return (Byte)ToInt16();
                        case TypeCode.Double:
                            return (Double)ToInt16();
                        case TypeCode.Int16:
                            return ToInt16();
                        case TypeCode.Int32:
                            return (Int32)ToInt16();
                        case TypeCode.Single:
                            return (Single)ToInt16();
                        case TypeCode.Int64:
                            return (Int64)ToInt16();
                        case TypeCode.UInt16:
                            return (UInt16)ToInt16();
                        case TypeCode.UInt32:
                            return (UInt32)ToInt16();
                        case TypeCode.UInt64:
                            return (UInt64)ToInt16();
                        default:
                            throw new InvalidOperationException("Unable to convert binary value to " + typeCode);
                    }
                case TypeCode.Int32:
                    switch (typeCode)
                    {
                        case TypeCode.Byte:
                            return (Byte)ToInt32();
                        case TypeCode.Double:
                            return (Double)ToInt32();
                        case TypeCode.Int16:
                            return (Int16)ToInt32();
                        case TypeCode.Int32:
                            return ToInt32();
                        case TypeCode.Int64:
                            return (Int64)ToInt32();
                        case TypeCode.Single:
                            return (Single)ToInt32();
                        case TypeCode.UInt16:
                            return (UInt16)ToInt32();
                        case TypeCode.UInt32:
                            return (UInt32)ToInt32();
                        case TypeCode.UInt64:
                            return (UInt64)ToInt32();
                        default:
                            throw new InvalidOperationException("Unable to convert binary value to " + typeCode);
                    }
                case TypeCode.Int64:
                    switch (typeCode)
                    {
                        case TypeCode.Byte:
                            return (Byte)ToInt64();
                        case TypeCode.Double:
                            return (Double)ToInt64();
                        case TypeCode.Int16:
                            return (Int16)ToInt64();
                        case TypeCode.Int32:
                            return (Int32)ToInt64();
                        case TypeCode.Single:
                            return (Single)ToInt64();
                        case TypeCode.Int64:
                            return ToInt64();
                        case TypeCode.UInt16:
                            return (UInt16)ToInt64();
                        case TypeCode.UInt32:
                            return (UInt32)ToInt64();
                        case TypeCode.UInt64:
                            return (UInt64)ToInt64();
                        default:
                            throw new InvalidOperationException("Unable to convert binary value to " + typeCode);
                    }
                case TypeCode.Single:
                    switch (typeCode)
                    {
                        case TypeCode.Byte:
                            return (Byte)ToSingle();
                        case TypeCode.Double:
                            return (Double)ToSingle();
                        case TypeCode.Int16:
                            return (Int16)ToSingle();
                        case TypeCode.Int32:
                            return (Int32)ToSingle();
                        case TypeCode.Int64:
                            return (Int64)ToSingle();
                        case TypeCode.Single:
                            return ToSingle();
                        case TypeCode.UInt16:
                            return (UInt16)ToSingle();
                        case TypeCode.UInt32:
                            return (UInt32)ToSingle();
                        case TypeCode.UInt64:
                            return (UInt64)ToSingle();
                        default:
                            throw new InvalidOperationException("Unable to convert binary value to " + typeCode);
                    }
                case TypeCode.Double:
                    switch (typeCode)
                    {
                        case TypeCode.Byte:
                            return (Byte)ToDouble();
                        case TypeCode.Double:
                            return ToDouble();
                        case TypeCode.Int16:
                            return (Int16)ToDouble();
                        case TypeCode.Int32:
                            return (Int32)ToDouble();
                        case TypeCode.Int64:
                            return (Int64)ToDouble();
                        case TypeCode.Single:
                            return (Single)ToDouble();
                        case TypeCode.UInt16:
                            return (UInt16)ToDouble();
                        case TypeCode.UInt32:
                            return (UInt32)ToDouble();
                        case TypeCode.UInt64:
                            return (UInt64)ToDouble();
                        default:
                            throw new InvalidOperationException("Unable to convert binary value to " + typeCode);
                    }
                case TypeCode.UInt16:
                    switch (typeCode)
                    {
                        case TypeCode.Byte:
                            return (Byte)ToUInt16();
                        case TypeCode.Double:
                            return (Double)ToUInt16();
                        case TypeCode.Int16:
                            return (Int16)ToUInt16();
                        case TypeCode.Int32:
                            return (Int32)ToUInt16();
                        case TypeCode.Int64:
                            return (Int64)ToUInt16();
                        case TypeCode.Single:
                            return (Single)ToUInt16();
                        case TypeCode.UInt16:
                            return ToUInt16();
                        case TypeCode.UInt32:
                            return (UInt32)ToUInt16();
                        case TypeCode.UInt64:
                            return (UInt64)ToUInt16();
                        default:
                            throw new InvalidOperationException("Unable to convert binary value to " + typeCode);
                    }
                case TypeCode.UInt32:
                    switch (typeCode)
                    {
                        case TypeCode.Byte:
                            return (Byte)ToUInt32();
                        case TypeCode.Double:
                            return (Double)ToUInt32();
                        case TypeCode.Int16:
                            return (Int16)ToUInt32();
                        case TypeCode.Int32:
                            return (Int32)ToUInt32();
                        case TypeCode.Int64:
                            return (Int64)ToUInt32();
                        case TypeCode.Single:
                            return (Single)ToUInt32();
                        case TypeCode.UInt16:
                            return (UInt16)ToUInt32();
                        case TypeCode.UInt32:
                            return ToUInt32();
                        case TypeCode.UInt64:
                            return (UInt64)ToUInt32();
                        default:
                            throw new InvalidOperationException("Unable to convert binary value to " + typeCode);
                    }
                case TypeCode.UInt64:
                    switch (typeCode)
                    {
                        case TypeCode.Byte:
                            return (Byte)ToUInt64();
                        case TypeCode.Double:
                            return (Double)ToUInt64();
                        case TypeCode.Int16:
                            return (Int16)ToUInt64();
                        case TypeCode.Int32:
                            return (Int32)ToUInt64();
                        case TypeCode.Int64:
                            return (Int64)ToUInt64();
                        case TypeCode.Single:
                            return (Single)ToUInt64();
                        case TypeCode.UInt16:
                            return (UInt16)ToUInt64();
                        case TypeCode.UInt32:
                            return (UInt32)ToUInt64();
                        case TypeCode.UInt64:
                            return ToUInt64();
                        default:
                            throw new InvalidOperationException("Unable to convert binary value to " + typeCode);
                    }
                default:
                    throw new InvalidOperationException("Unable to convert binary value from " + this.TypeCode);
            }
        }

        #endregion

        #region [ Operators ]

        /// <summary>
        /// Implicitly converts <see cref="BinaryValue"/> to <see cref="Byte"/>.
        /// </summary>
        /// <param name="value"><see cref="BinaryValue"/> to convert to <see cref="Byte"/>.</param>
        /// <returns>A <see cref="Byte"/> representation of <see cref="BinaryValue"/>.</returns>
        public static implicit operator Byte(BinaryValue value)
        {
            return value.ToByte();
        }

        /// <summary>
        /// Implicitly converts <see cref="Byte"/> to <see cref="BinaryValue"/>.
        /// </summary>
        /// <param name="value"><see cref="Byte"/> to convert to <see cref="BinaryValue"/>.</param>
        /// <returns>A <see cref="BinaryValue"/> representation of <see cref="Byte"/>.</returns>
        public static implicit operator BinaryValue(Byte value)
        {
            return new BinaryValue(TypeCode.Byte, new byte[] { value });
        }

        /// <summary>
        /// Implicitly converts <see cref="BinaryValue"/> to <see cref="Int16"/>.
        /// </summary>
        /// <param name="value"><see cref="BinaryValue"/> to convert to <see cref="Int16"/>.</param>
        /// <returns>A <see cref="Int16"/> representation of <see cref="BinaryValue"/>.</returns>
        public static implicit operator Int16(BinaryValue value)
        {
            return value.ToInt16();
        }

        /// <summary>
        /// Implicitly converts <see cref="Int16"/> to <see cref="BinaryValue"/>.
        /// </summary>
        /// <param name="value"><see cref="Int16"/> to convert to <see cref="BinaryValue"/>.</param>
        /// <returns>A <see cref="BinaryValue"/> representation of <see cref="Int16"/>.</returns>
        public static implicit operator BinaryValue(Int16 value)
        {
            return new BinaryValue(TypeCode.Int16, m_endianOrder.GetBytes(value));
        }

        /// <summary>
        /// Implicitly converts <see cref="BinaryValue"/> to <see cref="UInt16"/>.
        /// </summary>
        /// <param name="value"><see cref="BinaryValue"/> to convert to <see cref="UInt16"/>.</param>
        /// <returns>A <see cref="UInt16"/> representation of <see cref="BinaryValue"/>.</returns>
        [CLSCompliant(false)]
        public static implicit operator UInt16(BinaryValue value)
        {
            return value.ToUInt16();
        }

        /// <summary>
        /// Implicitly converts <see cref="UInt16"/> to <see cref="BinaryValue"/>.
        /// </summary>
        /// <param name="value"><see cref="UInt16"/> to convert to <see cref="BinaryValue"/>.</param>
        /// <returns>A <see cref="BinaryValue"/> representation of <see cref="UInt16"/>.</returns>
        [CLSCompliant(false)]
        public static implicit operator BinaryValue(UInt16 value)
        {
            return new BinaryValue(TypeCode.UInt16, m_endianOrder.GetBytes(value));
        }

        /// <summary>
        /// Implicitly converts <see cref="BinaryValue"/> to <see cref="Int24"/>.
        /// </summary>
        /// <param name="value"><see cref="BinaryValue"/> to convert to <see cref="Int24"/>.</param>
        /// <returns>A <see cref="Int24"/> representation of <see cref="BinaryValue"/>.</returns>
        public static implicit operator Int24(BinaryValue value)
        {
            return value.ToInt24();
        }

        /// <summary>
        /// Implicitly converts <see cref="Int24"/> to <see cref="BinaryValue"/>.
        /// </summary>
        /// <param name="value"><see cref="Int24"/> to convert to <see cref="BinaryValue"/>.</param>
        /// <returns>A <see cref="BinaryValue"/> representation of <see cref="Int24"/>.</returns>
        public static implicit operator BinaryValue(Int24 value)
        {
            return new BinaryValue(TypeCode.Empty, m_endianOrder.GetBytes(value));
        }

        /// <summary>
        /// Implicitly converts <see cref="BinaryValue"/> to <see cref="UInt24"/>.
        /// </summary>
        /// <param name="value"><see cref="BinaryValue"/> to convert to <see cref="UInt24"/>.</param>
        /// <returns>A <see cref="UInt24"/> representation of <see cref="BinaryValue"/>.</returns>
        [CLSCompliant(false)]
        public static implicit operator UInt24(BinaryValue value)
        {
            return value.ToUInt24();
        }

        /// <summary>
        /// Implicitly converts <see cref="UInt24"/> to <see cref="BinaryValue"/>.
        /// </summary>
        /// <param name="value"><see cref="UInt24"/> to convert to <see cref="BinaryValue"/>.</param>
        /// <returns>A <see cref="BinaryValue"/> representation of <see cref="UInt24"/>.</returns>
        [CLSCompliant(false)]
        public static implicit operator BinaryValue(UInt24 value)
        {
            return new BinaryValue(TypeCode.Empty, m_endianOrder.GetBytes(value));
        }

        /// <summary>
        /// Implicitly converts <see cref="BinaryValue"/> to <see cref="Int32"/>.
        /// </summary>
        /// <param name="value"><see cref="BinaryValue"/> to convert to <see cref="Int32"/>.</param>
        /// <returns>A <see cref="Int32"/> representation of <see cref="BinaryValue"/>.</returns>
        public static implicit operator Int32(BinaryValue value)
        {
            return value.ToInt32();
        }

        /// <summary>
        /// Implicitly converts <see cref="Int32"/> to <see cref="BinaryValue"/>.
        /// </summary>
        /// <param name="value"><see cref="Int32"/> to convert to <see cref="BinaryValue"/>.</param>
        /// <returns>A <see cref="BinaryValue"/> representation of <see cref="Int32"/>.</returns>
        public static implicit operator BinaryValue(Int32 value)
        {
            return new BinaryValue(TypeCode.Int32, m_endianOrder.GetBytes(value));
        }

        /// <summary>
        /// Implicitly converts <see cref="BinaryValue"/> to <see cref="UInt32"/>.
        /// </summary>
        /// <param name="value"><see cref="BinaryValue"/> to convert to <see cref="UInt32"/>.</param>
        /// <returns>A <see cref="UInt32"/> representation of <see cref="BinaryValue"/>.</returns>
        [CLSCompliant(false)]
        public static implicit operator UInt32(BinaryValue value)
        {
            return value.ToUInt32();
        }

        /// <summary>
        /// Implicitly converts <see cref="UInt32"/> to <see cref="BinaryValue"/>.
        /// </summary>
        /// <param name="value"><see cref="UInt32"/> to convert to <see cref="BinaryValue"/>.</param>
        /// <returns>A <see cref="BinaryValue"/> representation of <see cref="UInt32"/>.</returns>
        [CLSCompliant(false)]
        public static implicit operator BinaryValue(UInt32 value)
        {
            return new BinaryValue(TypeCode.UInt32, m_endianOrder.GetBytes(value));
        }

        /// <summary>
        /// Implicitly converts <see cref="BinaryValue"/> to <see cref="Int64"/>.
        /// </summary>
        /// <param name="value"><see cref="BinaryValue"/> to convert to <see cref="Int64"/>.</param>
        /// <returns>A <see cref="Int64"/> representation of <see cref="BinaryValue"/>.</returns>
        public static implicit operator Int64(BinaryValue value)
        {
            return value.ToInt64();
        }

        /// <summary>
        /// Implicitly converts <see cref="Int64"/> to <see cref="BinaryValue"/>.
        /// </summary>
        /// <param name="value"><see cref="Int64"/> to convert to <see cref="BinaryValue"/>.</param>
        /// <returns>A <see cref="BinaryValue"/> representation of <see cref="Int64"/>.</returns>
        public static implicit operator BinaryValue(Int64 value)
        {
            return new BinaryValue(TypeCode.Int64, m_endianOrder.GetBytes(value));
        }

        /// <summary>
        /// Implicitly converts <see cref="BinaryValue"/> to <see cref="UInt64"/>.
        /// </summary>
        /// <param name="value"><see cref="BinaryValue"/> to convert to <see cref="UInt64"/>.</param>
        /// <returns>A <see cref="UInt64"/> representation of <see cref="BinaryValue"/>.</returns>
        [CLSCompliant(false)]
        public static implicit operator UInt64(BinaryValue value)
        {
            return value.ToUInt64();
        }

        /// <summary>
        /// Implicitly converts <see cref="UInt64"/> to <see cref="BinaryValue"/>.
        /// </summary>
        /// <param name="value"><see cref="UInt64"/> to convert to <see cref="BinaryValue"/>.</param>
        /// <returns>A <see cref="BinaryValue"/> representation of <see cref="UInt64"/>.</returns>
        [CLSCompliant(false)]
        public static implicit operator BinaryValue(UInt64 value)
        {
            return new BinaryValue(TypeCode.UInt64, m_endianOrder.GetBytes(value));
        }

        /// <summary>
        /// Implicitly converts <see cref="BinaryValue"/> to <see cref="Single"/>.
        /// </summary>
        /// <param name="value"><see cref="BinaryValue"/> to convert to <see cref="Single"/>.</param>
        /// <returns>A <see cref="Single"/> representation of <see cref="BinaryValue"/>.</returns>
        public static implicit operator Single(BinaryValue value)
        {
            return value.ToSingle();
        }

        /// <summary>
        /// Implicitly converts <see cref="Single"/> to <see cref="BinaryValue"/>.
        /// </summary>
        /// <param name="value"><see cref="Single"/> to convert to <see cref="BinaryValue"/>.</param>
        /// <returns>A <see cref="BinaryValue"/> representation of <see cref="Single"/>.</returns>
        public static implicit operator BinaryValue(Single value)
        {
            return new BinaryValue(TypeCode.Single, m_endianOrder.GetBytes(value));
        }

        /// <summary>
        /// Implicitly converts <see cref="BinaryValue"/> to <see cref="Double"/>.
        /// </summary>
        /// <param name="value"><see cref="BinaryValue"/> to convert to <see cref="Double"/>.</param>
        /// <returns>A <see cref="Double"/> representation of <see cref="BinaryValue"/>.</returns>
        public static implicit operator Double(BinaryValue value)
        {
            return value.ToDouble();
        }

        /// <summary>
        /// Implicitly converts <see cref="Double"/> to <see cref="BinaryValue"/>.
        /// </summary>
        /// <param name="value"><see cref="Double"/> to convert to <see cref="BinaryValue"/>.</param>
        /// <returns>A <see cref="BinaryValue"/> representation of <see cref="Double"/>.</returns>
        public static implicit operator BinaryValue(Double value)
        {
            return new BinaryValue(TypeCode.Double, m_endianOrder.GetBytes(value));
        }

        #endregion

        #region [ Static ]

        static BinaryValue()
        {
            m_endianOrder = NativeEndianOrder.Default;
        }

        #endregion
    }
}
