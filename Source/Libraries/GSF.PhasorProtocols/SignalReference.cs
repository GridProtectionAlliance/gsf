//*******************************************************************************************************
//  SignalReference.cs - Gbtc
//
//  Tennessee Valley Authority, 2009
//  No copyright is claimed pursuant to 17 USC § 105.  All Other Rights Reserved.
//
//  This software is made freely available under the TVA Open Source Agreement (see below).
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  05/05/2009 - J. Ritchie Carroll
//       Generated original version of source code.
//  09/15/2009 - Stephen C. Wills
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

namespace GSF.PhasorProtocols
{
    #region [ Enumerations ]

    /// <summary>
    /// Fundamental signal types enumeration.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This signal type represents the basic type of a signal used to suffix a formatted signal reference. When
    /// used in context along with an optional index the fundamental signal type will identify a signal's location
    /// within a frame of data (see <see cref="SignalReference"/>).
    /// </para>
    /// <para>
    /// Contrast the <see cref="SignalKind"/> enumeration with the <see cref="SignalType"/>
    /// enumeration which defines an explicit type for a signal (e.g., a voltage or current phasor).
    /// </para>
    /// </remarks>
    [Serializable()]
    public enum SignalKind
    {
        /// <summary>
        /// Phase angle.
        /// </summary>
        Angle,
        /// <summary>
        /// Phase magnitude.
        /// </summary>
        Magnitude,
        /// <summary>
        /// Line frequency.
        /// </summary>
        Frequency,
        /// <summary>
        /// Frequency delta over time (dF/dt).
        /// </summary>
        DfDt,
        /// <summary>
        /// Status flags.
        /// </summary>
        Status,
        /// <summary>
        /// Digital value.
        /// </summary>
        Digital,
        /// <summary>
        /// Analog value.
        /// </summary>
        Analog,
        /// <summary>
        /// Calculated value.
        /// </summary>
        Calculation,
        /// <summary>
        /// Statistical value.
        /// </summary>
        Statistic,
        /// <summary>
        /// Alarm value.
        /// </summary>
        Alarm,
        /// <summary>
        /// Undetermined signal type.
        /// </summary>
        Unknown
    }

    #endregion

    /// <summary>
    /// Represents a signal that can be referenced by its constituent components.
    /// </summary>
    public struct SignalReference : IEquatable<SignalReference>, IComparable<SignalReference>
    {
        #region [ Members ]

        // Fields

        /// <summary>
        /// Gets or sets the acronym of this <see cref="SignalReference"/>.
        /// </summary>
        public string Acronym;

        /// <summary>
        /// Gets or sets the signal index of this <see cref="SignalReference"/>.
        /// </summary>
        public int Index;

        /// <summary>
        /// Gets or sets the <see cref="SignalKind"/> of this <see cref="SignalReference"/>.
        /// </summary>
        public SignalKind Kind;

        /// <summary>
        /// Gets or sets the cell index of this <see cref="SignalReference"/>.
        /// </summary>
        public int CellIndex;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="SignalReference"/>.
        /// </summary>
        /// <param name="signal"><see cref="string"/> representation of this <see cref="SignalReference"/>.</param>
        public SignalReference(string signal)
        {
            // Signal reference may contain multiple dashes, we're interested in the last one
            int splitIndex = signal.LastIndexOf('-');

            // Assign default values to fields
            Index = 0;
            CellIndex = 0;

            if (splitIndex > -1)
            {
                string signalType = signal.Substring(splitIndex + 1).Trim().ToUpper();
                Acronym = signal.Substring(0, splitIndex).Trim().ToUpper();

                // If the length of the signal type acronym is greater than 2, then this
                // is an indexed signal type (e.g., CORDOVA-PA2)
                if (signalType.Length > 2)
                {
                    Kind = SignalReference.ParseSignalKind(signalType.Substring(0, 2));

                    if (Kind != SignalKind.Unknown)
                        Index = int.Parse(signalType.Substring(2));
                }
                else
                    Kind = SignalReference.ParseSignalKind(signalType);
            }
            else
            {
                // This represents an error - best we can do is assume entire string is the acronym
                Acronym = signal.Trim().ToUpper();
                Kind = SignalKind.Unknown;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Returns a <see cref="string"/> that represents the current <see cref="SignalReference"/>.
        /// </summary>
        /// <returns>A <see cref="string"/> that represents the current <see cref="SignalReference"/>.</returns>
        public override string ToString()
        {
            return SignalReference.ToString(Acronym, Kind, Index);
        }

        /// <summary>
        /// Returns the hash code for this <see cref="SignalReference"/>.
        /// </summary>
        /// <returns>A 32-bit signed integer hash code.</returns>
        public override int GetHashCode()
        {
            return SignalReference.ToString(Acronym, Kind, Index).GetHashCode();
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="obj">An object to compare with this object.</param>
        /// <returns><c>true</c> if the current object is equal to the other parameter; otherwise, <c>false</c>.</returns>
        /// <exception cref="ArgumentException">Object is not a <see cref="SignalReference"/>.</exception>
        public override bool Equals(object obj)
        {
            if (obj is SignalReference)
                return Equals((SignalReference)obj);

            return false;
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns><c>true</c> if the current object is equal to the other parameter; otherwise, <c>false</c>.</returns>
        public bool Equals(SignalReference other)
        {
            return (string.Compare(Acronym, other.Acronym, true) == 0 && Kind == other.Kind && Index == other.Index);
        }

        /// <summary>
        /// Compares the current object with another object of the same type.
        /// </summary>
        /// <param name="other">Another <see cref="SignalReference"/> to compare with this <see cref="SignalReference"/>.</param>
        /// <returns>A 32-bit signed integer that indicates the relative order of the objects being compared.</returns>
        public int CompareTo(SignalReference other)
        {
            int acronymCompare = string.Compare(Acronym, other.Acronym, true);

            if (acronymCompare == 0)
            {
                int signalTypeCompare = Kind < other.Kind ? -1 : (Kind > other.Kind ? 1 : 0);

                if (signalTypeCompare == 0)
                    return Index.CompareTo(other.Index);
                else
                    return signalTypeCompare;
            }
            else
                return acronymCompare;
        }

        /// <summary>
        /// Compares the current object with another object of the same type.
        /// </summary>
        /// <param name="obj">An object to compare with this <see cref="SignalReference"/>.</param>
        /// <returns>A 32-bit signed integer that indicates the relative order of the objects being compared.</returns>
        /// <exception cref="ArgumentException">Object is not a <see cref="SignalReference"/>.</exception>
        public int CompareTo(object obj)
        {
            if (obj is SignalReference)
                return CompareTo((SignalReference)obj);

            throw new ArgumentException("Object is not a SignalReference");
        }

        #endregion

        #region [ Operators ]

        /// <summary>
        /// Compares two <see cref="SignalReference"/> values for equality.
        /// </summary>
        /// <param name="signal1">A <see cref="SignalReference"/> left hand operand.</param>
        /// <param name="signal2">A <see cref="SignalReference"/> right hand operand.</param>
        /// <returns>A boolean representing the result.</returns>
        public static bool operator ==(SignalReference signal1, SignalReference signal2)
        {
            return signal1.Equals(signal2);
        }

        /// <summary>
        /// Compares two <see cref="SignalReference"/> values for inequality.
        /// </summary>
        /// <param name="signal1">A <see cref="SignalReference"/> left hand operand.</param>
        /// <param name="signal2">A <see cref="SignalReference"/> right hand operand.</param>
        /// <returns>A boolean representing the result.</returns>
        public static bool operator !=(SignalReference signal1, SignalReference signal2)
        {
            return !signal1.Equals(signal2);
        }

        /// <summary>
        /// Returns true if left <see cref="SignalReference"/> value is greater than right <see cref="SignalReference"/> value.
        /// </summary>
        /// <param name="signal1">A <see cref="SignalReference"/> left hand operand.</param>
        /// <param name="signal2">A <see cref="SignalReference"/> right hand operand.</param>
        /// <returns>A boolean representing the result.</returns>
        public static bool operator >(SignalReference signal1, SignalReference signal2)
        {
            return signal1.CompareTo(signal2) > 0;
        }

        /// <summary>
        /// Returns true if left <see cref="SignalReference"/> value is greater than or equal to right <see cref="SignalReference"/> value.
        /// </summary>
        /// <param name="signal1">A <see cref="SignalReference"/> left hand operand.</param>
        /// <param name="signal2">A <see cref="SignalReference"/> right hand operand.</param>
        /// <returns>A boolean representing the result.</returns>
        public static bool operator >=(SignalReference signal1, SignalReference signal2)
        {
            return signal1.CompareTo(signal2) >= 0;
        }

        /// <summary>
        /// Returns true if left <see cref="SignalReference"/> value is less than right <see cref="SignalReference"/> value.
        /// </summary>
        /// <param name="signal1">A <see cref="SignalReference"/> left hand operand.</param>
        /// <param name="signal2">A <see cref="SignalReference"/> right hand operand.</param>
        /// <returns>A boolean representing the result.</returns>
        public static bool operator <(SignalReference signal1, SignalReference signal2)
        {
            return signal1.CompareTo(signal2) < 0;
        }

        /// <summary>
        /// Returns true if left <see cref="SignalReference"/> value is less than or equal to right <see cref="SignalReference"/> value.
        /// </summary>
        /// <param name="signal1">A <see cref="SignalReference"/> left hand operand.</param>
        /// <param name="signal2">A <see cref="SignalReference"/> right hand operand.</param>
        /// <returns>A boolean representing the result.</returns>
        public static bool operator <=(SignalReference signal1, SignalReference signal2)
        {
            return signal1.CompareTo(signal2) <= 0;
        }

        #endregion

        #region [ Static ]

        // Static Methods

        /// <summary>
        /// Gets the <see cref="SignalKind"/> for the specified <paramref name="acronym"/>.
        /// </summary>
        /// <param name="acronym">Acronym of the desired <see cref="SignalKind"/>.</param>
        /// <returns>The <see cref="SignalKind"/> for the specified <paramref name="acronym"/>.</returns>
        public static SignalKind ParseSignalKind(string acronym)
        {
            switch (acronym)
            {
                case "PA": // Phase Angle
                    return SignalKind.Angle;
                case "PM": // Phase Magnitude
                    return SignalKind.Magnitude;
                case "FQ": // Frequency
                    return SignalKind.Frequency;
                case "DF": // dF/dt
                    return SignalKind.DfDt;
                case "SF": // Status Flags
                    return SignalKind.Status;
                case "DV": // Digital Value
                    return SignalKind.Digital;
                case "AV": // Analog Value
                    return SignalKind.Analog;
                case "CV": // Calculated Value
                    return SignalKind.Calculation;
                case "ST": // Statistical Value
                    return SignalKind.Statistic;
                case "AL": // Alarm Value
                    return SignalKind.Alarm;
                default:
                    return SignalKind.Unknown;
            }
        }

        /// <summary>
        /// Gets the acronym for the specified <see cref="SignalKind"/>.
        /// </summary>
        /// <param name="signal"><see cref="SignalKind"/> to convert to an acronym.</param>
        /// <returns>The acronym for the specified <see cref="SignalKind"/>.</returns>
        public static string GetSignalKindAcronym(SignalKind signal)
        {
            switch (signal)
            {
                case SignalKind.Angle:
                    return "PA"; // Phase Angle
                case SignalKind.Magnitude:
                    return "PM"; // Phase Magnitude
                case SignalKind.Frequency:
                    return "FQ"; // Frequency
                case SignalKind.DfDt:
                    return "DF"; // dF/dt
                case SignalKind.Status:
                    return "SF"; // Status Flags
                case SignalKind.Digital:
                    return "DV"; // Digital Value
                case SignalKind.Analog:
                    return "AV"; // Analog Value
                case SignalKind.Calculation:
                    return "CV"; // Calculated Value
                case SignalKind.Statistic:
                    return "ST"; // Statistical Value
                case SignalKind.Alarm:
                    return "AL"; // Alarm Value
                default:
                    return "??";
            }
        }

        /// <summary>
        /// Returns a <see cref="string"/> that represents the specified <paramref name="acronym"/> and <see cref="SignalKind"/>.
        /// </summary>
        /// <param name="acronym">Acronym portion of the desired <see cref="string"/> representation.</param>
        /// <param name="type"><see cref="SignalKind"/> portion of the desired <see cref="string"/> representation.</param>
        /// <returns>A <see cref="string"/> that represents the specified <paramref name="acronym"/> and <see cref="SignalKind"/>.</returns>
        public static string ToString(string acronym, SignalKind type)
        {
            return ToString(acronym, type, 0);
        }

        /// <summary>
        /// Returns a <see cref="string"/> that represents the specified <paramref name="acronym"/>, <see cref="SignalKind"/> and <paramref name="index"/>.
        /// </summary>
        /// <param name="acronym">Acronym portion of the desired <see cref="string"/> representation.</param>
        /// <param name="type"><see cref="SignalKind"/> portion of the desired <see cref="string"/> representation.</param>
        /// <param name="index">Index of <see cref="SignalKind"/> portion of the desired <see cref="string"/> representation.</param>
        /// <returns>A <see cref="string"/> that represents the specified <paramref name="acronym"/>, <see cref="SignalKind"/> and <paramref name="index"/>.</returns>
        public static string ToString(string acronym, SignalKind type, int index)
        {
            if (index > 0)
                return string.Format("{0}-{1}{2}", acronym, GetSignalKindAcronym(type), index);
            else
                return string.Format("{0}-{1}", acronym, GetSignalKindAcronym(type));
        }

        #endregion
    }
}