//*******************************************************************************************************
//  Volume.cs - Gbtc
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
//  09/11/2008 - J. Ritchie Carroll
//       Converted to C#.
//  08/10/2009 - Josh L. Patterson
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
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace TVA.Units
{
    /// <summary>Represents a volume measurement, in cubic meters, as a double-precision floating-point number.</summary>
    /// <remarks>
    /// This class behaves just like a <see cref="Double"/> representing a volume in cubic meters; it is implictly
    /// castable to and from a <see cref="Double"/> and therefore can be generally used "as" a double, but it
    /// has the advantage of handling conversions to and from other volume representations, specifically
    /// liters, teaspoons, tablespoons, cubic inches, fluid ounces, cups, pints, quarts, gallons and cubic feet.
    /// Metric conversions are handled simply by applying the needed <see cref="SI"/> conversion factor, for example:
    /// <example>
    /// Convert volume, in cubic meters, to cubic kilometers:
    /// <code>
    /// public double GetCubicKilometers(Volume cubicmeters)
    /// {
    ///     return cubicmeters / SI.Kilo;
    /// }
    /// </code>
    /// </example>
    /// <example>
    /// This example converts teaspoons to cups:
    /// <code>
    /// public double GetCups(double teaspoons)
    /// {
    ///     return Volume.FromTeaspoons(teaspoons).ToCups();
    /// }
    /// </code>
    /// </example>
    /// <example>
    /// This example converts liters to fluid ounces:
    /// <code>
    /// public double GetFluidOunces(double liters)
    /// {
    ///     return Volume.FromLiters(liters).ToFluidOunces();
    /// }
    /// </code>
    /// </example>
    /// </remarks>
    [Serializable()]
    public struct Volume : IComparable, IFormattable, IConvertible, IComparable<Volume>, IComparable<Double>, IEquatable<Volume>, IEquatable<Double>
    {
        #region [ Members ]

        // Constants
        private const double LitersFactor = 0.001D;

        private const double TeaspoonsFactor = 4.928921595e-6D;

        private const double MetricTeaspoonsFactor = 5.0e-6D;

        private const double TablespoonsFactor = 14.7867647825e-6D;

        private const double MetricTablespoonsFactor = 15.0e-6D;

        private const double CupsFactor = 236.5882365e-6D;

        private const double MetricCupsFactor = 250.0e-6D;

        private const double FluidOuncesFactor = 29.5735295625e-6D;

        private const double PintsFactor = 473.176473e-6D;

        private const double QuartsFactor = 946.352946e-6D;

        private const double GallonsFactor = 3.785411784e-3D;

        private const double CubicInchesFactor = 16.387064e-6D;

        private const double CubicFeetFactor = 0.028316846592D;

        // Fields
        private double m_value; // Volume value stored in cubic meters

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="Volume"/>.
        /// </summary>
        /// <param name="value">New volume value in cubic meters.</param>
        public Volume(double value)
        {
            m_value = value;
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Gets the <see cref="Volume"/> value in liters.
        /// </summary>
        /// <returns>Value of <see cref="Volume"/> in liters.</returns>
        public double ToLiters()
        {
            return m_value / LitersFactor;
        }

        /// <summary>
        /// Gets the <see cref="Volume"/> value in US teaspoons.
        /// </summary>
        /// <returns>Value of <see cref="Volume"/> in US teaspoons.</returns>
        public double ToTeaspoons()
        {
            return m_value / TeaspoonsFactor;
        }

        /// <summary>
        /// Gets the <see cref="Volume"/> value in metric teaspoons.
        /// </summary>
        /// <returns>Value of <see cref="Volume"/> in metric teaspoons.</returns>
        public double ToMetricTeaspoons()
        {
            return m_value / MetricTeaspoonsFactor;
        }

        /// <summary>
        /// Gets the <see cref="Volume"/> value in US tablespoons.
        /// </summary>
        /// <returns>Value of <see cref="Volume"/> in US tablespoons.</returns>
        public double ToTablespoons()
        {
            return m_value / TablespoonsFactor;
        }

        /// <summary>
        /// Gets the <see cref="Volume"/> value in metric tablespoons.
        /// </summary>
        /// <returns>Value of <see cref="Volume"/> in metric tablespoons.</returns>
        public double ToMetricTablespoons()
        {
            return m_value / MetricTablespoonsFactor;
        }

        /// <summary>
        /// Gets the <see cref="Volume"/> value in US cups.
        /// </summary>
        /// <returns>Value of <see cref="Volume"/> in US cups.</returns>
        public double ToCups()
        {
            return m_value / CupsFactor;
        }

        /// <summary>
        /// Gets the <see cref="Volume"/> value in metric cups.
        /// </summary>
        /// <returns>Value of <see cref="Volume"/> in metric cups.</returns>
        public double ToMetricCups()
        {
            return m_value / MetricCupsFactor;
        }

        /// <summary>
        /// Gets the <see cref="Volume"/> value in US fluid ounces.
        /// </summary>
        /// <returns>Value of <see cref="Volume"/> in US fluid ounces.</returns>
        public double ToFluidOunces()
        {
            return m_value / FluidOuncesFactor;
        }

        /// <summary>
        /// Gets the <see cref="Volume"/> value in US fluid pints.
        /// </summary>
        /// <returns>Value of <see cref="Volume"/> in US fluid pints.</returns>
        public double ToPints()
        {
            return m_value / PintsFactor;
        }

        /// <summary>
        /// Gets the <see cref="Volume"/> value in US fluid quarts.
        /// </summary>
        /// <returns>Value of <see cref="Volume"/> in US fluid quarts.</returns>
        public double ToQuarts()
        {
            return m_value / QuartsFactor;
        }

        /// <summary>
        /// Gets the <see cref="Volume"/> value in US fluid gallons.
        /// </summary>
        /// <returns>Value of <see cref="Volume"/> in US fluid gallons.</returns>
        public double ToGallons()
        {
            return m_value / GallonsFactor;
        }

        /// <summary>
        /// Gets the <see cref="Volume"/> value in cubic inches.
        /// </summary>
        /// <returns>Value of <see cref="Volume"/> in cubic inches.</returns>
        public double ToCubicInches()
        {
            return m_value / CubicInchesFactor;
        }

        /// <summary>
        /// Gets the <see cref="Volume"/> value in cubic feet.
        /// </summary>
        /// <returns>Value of <see cref="Volume"/> in cubic feet.</returns>
        public double ToCubicFeet()
        {
            return m_value / CubicFeetFactor;
        }

        #region [ Numeric Interface Implementations ]

        /// <summary>
        /// Compares this instance to a specified object and returns an indication of their relative values.
        /// </summary>
        /// <param name="value">An object to compare, or null.</param>
        /// <returns>
        /// A signed number indicating the relative values of this instance and value. Returns less than zero
        /// if this instance is less than value, zero if this instance is equal to value, or greater than zero
        /// if this instance is greater than value.
        /// </returns>
        /// <exception cref="ArgumentException">value is not a <see cref="Double"/> or <see cref="Volume"/>.</exception>
        public int CompareTo(object value)
        {
            if (value == null) return 1;

            if (!(value is double) && !(value is Volume))
                throw new ArgumentException("Argument must be a Double or a Volume");

            double num = (double)value;
            return (m_value < num ? -1 : (m_value > num ? 1 : 0));
        }

        /// <summary>
        /// Compares this instance to a specified <see cref="Volume"/> and returns an indication of their
        /// relative values.
        /// </summary>
        /// <param name="value">A <see cref="Volume"/> to compare.</param>
        /// <returns>
        /// A signed number indicating the relative values of this instance and value. Returns less than zero
        /// if this instance is less than value, zero if this instance is equal to value, or greater than zero
        /// if this instance is greater than value.
        /// </returns>
        public int CompareTo(Volume value)
        {
            return CompareTo((double)value);
        }

        /// <summary>
        /// Compares this instance to a specified <see cref="Double"/> and returns an indication of their
        /// relative values.
        /// </summary>
        /// <param name="value">A <see cref="Double"/> to compare.</param>
        /// <returns>
        /// A signed number indicating the relative values of this instance and value. Returns less than zero
        /// if this instance is less than value, zero if this instance is equal to value, or greater than zero
        /// if this instance is greater than value.
        /// </returns>
        public int CompareTo(double value)
        {
            return (m_value < value ? -1 : (m_value > value ? 1 : 0));
        }

        /// <summary>
        /// Returns a value indicating whether this instance is equal to a specified object.
        /// </summary>
        /// <param name="obj">An object to compare, or null.</param>
        /// <returns>
        /// True if obj is an instance of <see cref="Double"/> or <see cref="Volume"/> and equals the value of this instance;
        /// otherwise, False.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (obj is double || obj is Volume)
                return Equals((double)obj);

            return false;
        }

        /// <summary>
        /// Returns a value indicating whether this instance is equal to a specified <see cref="Volume"/> value.
        /// </summary>
        /// <param name="obj">A <see cref="Volume"/> value to compare to this instance.</param>
        /// <returns>
        /// True if obj has the same value as this instance; otherwise, False.
        /// </returns>
        public bool Equals(Volume obj)
        {
            return Equals((double)obj);
        }

        /// <summary>
        /// Returns a value indicating whether this instance is equal to a specified <see cref="Double"/> value.
        /// </summary>
        /// <param name="obj">A <see cref="Double"/> value to compare to this instance.</param>
        /// <returns>
        /// True if obj has the same value as this instance; otherwise, False.
        /// </returns>
        public bool Equals(double obj)
        {
            return (m_value == obj);
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>
        /// A 32-bit signed integer hash code.
        /// </returns>
        public override int GetHashCode()
        {
            return m_value.GetHashCode();
        }

        /// <summary>
        /// Converts the numeric value of this instance to its equivalent string representation.
        /// </summary>
        /// <returns>
        /// The string representation of the value of this instance, consisting of a minus sign if
        /// the value is negative, and a sequence of digits ranging from 0 to 9 with no leading zeroes.
        /// </returns>
        public override string ToString()
        {
            return m_value.ToString();
        }

        /// <summary>
        /// Converts the numeric value of this instance to its equivalent string representation, using
        /// the specified format.
        /// </summary>
        /// <param name="format">A format string.</param>
        /// <returns>
        /// The string representation of the value of this instance as specified by format.
        /// </returns>
        public string ToString(string format)
        {
            return m_value.ToString(format);
        }

        /// <summary>
        /// Converts the numeric value of this instance to its equivalent string representation using the
        /// specified culture-specific format information.
        /// </summary>
        /// <param name="provider">
        /// A <see cref="System.IFormatProvider"/> that supplies culture-specific formatting information.
        /// </param>
        /// <returns>
        /// The string representation of the value of this instance as specified by provider.
        /// </returns>
        public string ToString(IFormatProvider provider)
        {
            return m_value.ToString(provider);
        }

        /// <summary>
        /// Converts the numeric value of this instance to its equivalent string representation using the
        /// specified format and culture-specific format information.
        /// </summary>
        /// <param name="format">A format specification.</param>
        /// <param name="provider">
        /// A <see cref="System.IFormatProvider"/> that supplies culture-specific formatting information.
        /// </param>
        /// <returns>
        /// The string representation of the value of this instance as specified by format and provider.
        /// </returns>
        public string ToString(string format, IFormatProvider provider)
        {
            return m_value.ToString(format, provider);
        }

        /// <summary>
        /// Converts the string representation of a number to its <see cref="Volume"/> equivalent.
        /// </summary>
        /// <param name="s">A string containing a number to convert.</param>
        /// <returns>
        /// A <see cref="Volume"/> equivalent to the number contained in s.
        /// </returns>
        /// <exception cref="ArgumentNullException">s is null.</exception>
        /// <exception cref="OverflowException">
        /// s represents a number less than <see cref="Volume.MinValue"/> or greater than <see cref="Volume.MaxValue"/>.
        /// </exception>
        /// <exception cref="FormatException">s is not in the correct format.</exception>
        public static Volume Parse(string s)
        {
            return (Volume)double.Parse(s);
        }

        /// <summary>
        /// Converts the string representation of a number in a specified style to its <see cref="Volume"/> equivalent.
        /// </summary>
        /// <param name="s">A string containing a number to convert.</param>
        /// <param name="style">
        /// A bitwise combination of System.Globalization.NumberStyles values that indicates the permitted format of s.
        /// </param>
        /// <returns>
        /// A <see cref="Volume"/> equivalent to the number contained in s.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// style is not a System.Globalization.NumberStyles value. -or- style is not a combination of
        /// System.Globalization.NumberStyles.AllowHexSpecifier and System.Globalization.NumberStyles.HexNumber values.
        /// </exception>
        /// <exception cref="ArgumentNullException">s is null.</exception>
        /// <exception cref="OverflowException">
        /// s represents a number less than <see cref="Volume.MinValue"/> or greater than <see cref="Volume.MaxValue"/>.
        /// </exception>
        /// <exception cref="FormatException">s is not in a format compliant with style.</exception>
        public static Volume Parse(string s, NumberStyles style)
        {
            return (Volume)double.Parse(s, style);
        }

        /// <summary>
        /// Converts the string representation of a number in a specified culture-specific format to its <see cref="Volume"/> equivalent.
        /// </summary>
        /// <param name="s">A string containing a number to convert.</param>
        /// <param name="provider">
        /// A <see cref="System.IFormatProvider"/> that supplies culture-specific formatting information about s.
        /// </param>
        /// <returns>
        /// A <see cref="Volume"/> equivalent to the number contained in s.
        /// </returns>
        /// <exception cref="ArgumentNullException">s is null.</exception>
        /// <exception cref="OverflowException">
        /// s represents a number less than <see cref="Volume.MinValue"/> or greater than <see cref="Volume.MaxValue"/>.
        /// </exception>
        /// <exception cref="FormatException">s is not in the correct format.</exception>
        public static Volume Parse(string s, IFormatProvider provider)
        {
            return (Volume)double.Parse(s, provider);
        }

        /// <summary>
        /// Converts the string representation of a number in a specified style and culture-specific format to its <see cref="Volume"/> equivalent.
        /// </summary>
        /// <param name="s">A string containing a number to convert.</param>
        /// <param name="style">
        /// A bitwise combination of System.Globalization.NumberStyles values that indicates the permitted format of s.
        /// </param>
        /// <param name="provider">
        /// A <see cref="System.IFormatProvider"/> that supplies culture-specific formatting information about s.
        /// </param>
        /// <returns>
        /// A <see cref="Volume"/> equivalent to the number contained in s.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// style is not a System.Globalization.NumberStyles value. -or- style is not a combination of
        /// System.Globalization.NumberStyles.AllowHexSpecifier and System.Globalization.NumberStyles.HexNumber values.
        /// </exception>
        /// <exception cref="ArgumentNullException">s is null.</exception>
        /// <exception cref="OverflowException">
        /// s represents a number less than <see cref="Volume.MinValue"/> or greater than <see cref="Volume.MaxValue"/>.
        /// </exception>
        /// <exception cref="FormatException">s is not in a format compliant with style.</exception>
        public static Volume Parse(string s, NumberStyles style, IFormatProvider provider)
        {
            return (Volume)double.Parse(s, style, provider);
        }

        /// <summary>
        /// Converts the string representation of a number to its <see cref="Volume"/> equivalent. A return value
        /// indicates whether the conversion succeeded or failed.
        /// </summary>
        /// <param name="s">A string containing a number to convert.</param>
        /// <param name="result">
        /// When this method returns, contains the <see cref="Volume"/> value equivalent to the number contained in s,
        /// if the conversion succeeded, or zero if the conversion failed. The conversion fails if the s paraampere is null,
        /// is not of the correct format, or represents a number less than <see cref="Volume.MinValue"/> or greater than <see cref="Volume.MaxValue"/>.
        /// This paraampere is passed uninitialized.
        /// </param>
        /// <returns>true if s was converted successfully; otherwise, false.</returns>
        public static bool TryParse(string s, out Volume result)
        {
            double parseResult;
            bool parseResponse;

            parseResponse = double.TryParse(s, out parseResult);
            result = parseResult;

            return parseResponse;
        }

        /// <summary>
        /// Converts the string representation of a number in a specified style and culture-specific format to its
        /// <see cref="Volume"/> equivalent. A return value indicates whether the conversion succeeded or failed.
        /// </summary>
        /// <param name="s">A string containing a number to convert.</param>
        /// <param name="style">
        /// A bitwise combination of System.Globalization.NumberStyles values that indicates the permitted format of s.
        /// </param>
        /// <param name="result">
        /// When this method returns, contains the <see cref="Volume"/> value equivalent to the number contained in s,
        /// if the conversion succeeded, or zero if the conversion failed. The conversion fails if the s paraampere is null,
        /// is not in a format compliant with style, or represents a number less than <see cref="Volume.MinValue"/> or
        /// greater than <see cref="Volume.MaxValue"/>. This paraampere is passed uninitialized.
        /// </param>
        /// <param name="provider">
        /// A <see cref="System.IFormatProvider"/> object that supplies culture-specific formatting information about s.
        /// </param>
        /// <returns>true if s was converted successfully; otherwise, false.</returns>
        /// <exception cref="ArgumentException">
        /// style is not a System.Globalization.NumberStyles value. -or- style is not a combination of
        /// System.Globalization.NumberStyles.AllowHexSpecifier and System.Globalization.NumberStyles.HexNumber values.
        /// </exception>
        public static bool TryParse(string s, NumberStyles style, IFormatProvider provider, out Volume result)
        {
            double parseResult;
            bool parseResponse;

            parseResponse = double.TryParse(s, style, provider, out parseResult);
            result = parseResult;

            return parseResponse;
        }

        /// <summary>
        /// Returns the <see cref="TypeCode"/> for value type <see cref="Double"/>.
        /// </summary>
        /// <returns>The enumerated constant, <see cref="TypeCode.Double"/>.</returns>
        public TypeCode GetTypeCode()
        {
            return TypeCode.Double;
        }

        #region [ Explicit IConvertible Implementation ]

        // These are explicitly implemented on the native System.Double implementations, so we do the same...

        bool IConvertible.ToBoolean(IFormatProvider provider)
        {
            return Convert.ToBoolean(m_value, provider);
        }

        char IConvertible.ToChar(IFormatProvider provider)
        {
            return Convert.ToChar(m_value, provider);
        }

        sbyte IConvertible.ToSByte(IFormatProvider provider)
        {
            return Convert.ToSByte(m_value, provider);
        }

        byte IConvertible.ToByte(IFormatProvider provider)
        {
            return Convert.ToByte(m_value, provider);
        }

        short IConvertible.ToInt16(IFormatProvider provider)
        {
            return Convert.ToInt16(m_value, provider);
        }

        ushort IConvertible.ToUInt16(IFormatProvider provider)
        {
            return Convert.ToUInt16(m_value, provider);
        }

        int IConvertible.ToInt32(IFormatProvider provider)
        {
            return Convert.ToInt32(m_value, provider);
        }

        uint IConvertible.ToUInt32(IFormatProvider provider)
        {
            return Convert.ToUInt32(m_value, provider);
        }

        long IConvertible.ToInt64(IFormatProvider provider)
        {
            return Convert.ToInt64(m_value, provider);
        }

        ulong IConvertible.ToUInt64(IFormatProvider provider)
        {
            return Convert.ToUInt64(m_value, provider);
        }

        float IConvertible.ToSingle(IFormatProvider provider)
        {
            return Convert.ToSingle(m_value, provider);
        }

        double IConvertible.ToDouble(IFormatProvider provider)
        {
            return m_value;
        }

        decimal IConvertible.ToDecimal(IFormatProvider provider)
        {
            return Convert.ToDecimal(m_value, provider);
        }

        DateTime IConvertible.ToDateTime(IFormatProvider provider)
        {
            return Convert.ToDateTime(m_value, provider);
        }

        object IConvertible.ToType(Type type, IFormatProvider provider)
        {
            return Convert.ChangeType(m_value, type, provider);
        }

        #endregion

        #endregion

        #endregion

        #region [ Operators ]

        #region [ Comparison Operators ]

        /// <summary>
        /// Compares the two values for equality.
        /// </summary>
        /// <param name="value1">A <see cref="Volume"/> object as the left hand operand.</param>
        /// <param name="value2">A <see cref="Volume"/> object as the right hand operand.</param>
        /// <returns>A <see cref="Boolean"/> value as the result.</returns>
        public static bool operator ==(Volume value1, Volume value2)
        {
            return value1.Equals(value2);
        }

        /// <summary>
        /// Compares the two values for inequality.
        /// </summary>
        /// <param name="value1">A <see cref="Volume"/> object as the left hand operand.</param>
        /// <param name="value2">A <see cref="Volume"/> object as the right hand operand.</param>
        /// <returns>A <see cref="Boolean"/> value as the result.</returns>
        public static bool operator !=(Volume value1, Volume value2)
        {
            return !value1.Equals(value2);
        }

        /// <summary>
        /// Returns true if left value is less than right value.
        /// </summary>
        /// <param name="value1">A <see cref="Volume"/> object as the left hand operand.</param>
        /// <param name="value2">A <see cref="Volume"/> object as the right hand operand.</param>
        /// <returns>A <see cref="Boolean"/> value as the result.</returns>
        public static bool operator <(Volume value1, Volume value2)
        {
            return (value1.CompareTo(value2) < 0);
        }

        /// <summary>
        /// Returns true if left value is less or equal to than right value.
        /// </summary>
        /// <param name="value1">A <see cref="Volume"/> object as the left hand operand.</param>
        /// <param name="value2">A <see cref="Volume"/> object as the right hand operand.</param>
        /// <returns>A <see cref="Boolean"/> value as the result.</returns>
        public static bool operator <=(Volume value1, Volume value2)
        {
            return (value1.CompareTo(value2) <= 0);
        }

        /// <summary>
        /// Returns true if left value is greater than right value.
        /// </summary>
        /// <param name="value1">A <see cref="Volume"/> object as the left hand operand.</param>
        /// <param name="value2">A <see cref="Volume"/> object as the right hand operand.</param>
        /// <returns>A <see cref="Boolean"/> value as the result.</returns>
        public static bool operator >(Volume value1, Volume value2)
        {
            return (value1.CompareTo(value2) > 0);
        }

        /// <summary>
        /// Returns true if left value is greater than or equal to right value.
        /// </summary>
        /// <param name="value1">A <see cref="Volume"/> object as the left hand operand.</param>
        /// <param name="value2">A <see cref="Volume"/> object as the right hand operand.</param>
        /// <returns>A <see cref="Boolean"/> value as the result.</returns>
        public static bool operator >=(Volume value1, Volume value2)
        {
            return (value1.CompareTo(value2) >= 0);
        }

        #endregion

        #region [ Type Conversion Operators ]

        /// <summary>
        /// Implicitly converts value, represented in cubic meters, to a <see cref="Volume"/>.
        /// </summary>
        /// <param name="value">A <see cref="Double"/> value.</param>
        /// <returns>A <see cref="Volume"/> object.</returns>
        public static implicit operator Volume(Double value)
        {
            return new Volume(value);
        }

        /// <summary>
        /// Implicitly converts <see cref="Volume"/>, represented in cubic meters, to a <see cref="Double"/>.
        /// </summary>
        /// <param name="value">A <see cref="Volume"/> object.</param>
        /// <returns>A <see cref="Double"/> value.</returns>
        public static implicit operator Double(Volume value)
        {
            return value.m_value;
        }

        #endregion

        #region [ Arithmetic Operators ]

        /// <summary>
        /// Returns computed remainder after dividing first value by the second.
        /// </summary>
        /// <param name="value1">A <see cref="Volume"/> object as the left hand operand.</param>
        /// <param name="value2">A <see cref="Volume"/> object as the right hand operand.</param>
        /// <returns>A <see cref="Volume"/> object as the result.</returns>
        public static Volume operator %(Volume value1, Volume value2)
        {
            return value1.m_value % value2.m_value;
        }

        /// <summary>
        /// Returns computed sum of values.
        /// </summary>
        /// <param name="value1">A <see cref="Volume"/> object as the left hand operand.</param>
        /// <param name="value2">A <see cref="Volume"/> object as the right hand operand.</param>
        /// <returns>A <see cref="Volume"/> object as the result.</returns>
        public static Volume operator +(Volume value1, Volume value2)
        {
            return value1.m_value + value2.m_value;
        }

        /// <summary>
        /// Returns computed difference of values.
        /// </summary>
        /// <param name="value1">A <see cref="Volume"/> object as the left hand operand.</param>
        /// <param name="value2">A <see cref="Volume"/> object as the right hand operand.</param>
        /// <returns>A <see cref="Volume"/> object as the result.</returns>
        public static Volume operator -(Volume value1, Volume value2)
        {
            return value1.m_value - value2.m_value;
        }

        /// <summary>
        /// Returns computed product of values.
        /// </summary>
        /// <param name="value1">A <see cref="Volume"/> object as the left hand operand.</param>
        /// <param name="value2">A <see cref="Volume"/> object as the right hand operand.</param>
        /// <returns>A <see cref="Volume"/> object as the result.</returns>
        public static Volume operator *(Volume value1, Volume value2)
        {
            return value1.m_value * value2.m_value;
        }

        /// <summary>
        /// Returns computed division of values.
        /// </summary>
        /// <param name="value1">A <see cref="Volume"/> object as the left hand operand.</param>
        /// <param name="value2">A <see cref="Volume"/> object as the right hand operand.</param>
        /// <returns>A <see cref="Volume"/> object as the result.</returns>
        public static Volume operator /(Volume value1, Volume value2)
        {
            return value1.m_value / value2.m_value;
        }

        // C# doesn't expose an exponent operator but some other .NET languages do,
        // so we expose the operator via its native special IL function name

        /// <summary>
        /// Returns result of first value raised to volume of second value.
        /// </summary>
        /// <param name="value1">A <see cref="Volume"/> object as the left hand operand.</param>
        /// <param name="value2">A <see cref="Volume"/> object as the right hand operand.</param>
        /// <returns>A <see cref="Double"/> value as the result.</returns>
        [EditorBrowsable(EditorBrowsableState.Advanced), SpecialName()]
        public static double op_Exponent(Volume value1, Volume value2)
        {
            return Math.Pow((double)value1.m_value, (double)value2.m_value);
        }

        #endregion

        #endregion

        #region [ Static ]

        // Static Fields

        /// <summary>Represents the largest possible value of an <see cref="Volume"/>. This field is constant.</summary>
        public static readonly Volume MaxValue = (Volume)double.MaxValue;

        /// <summary>Represents the smallest possible value of an <see cref="Volume"/>. This field is constant.</summary>
        public static readonly Volume MinValue = (Volume)double.MinValue;

        // Static Methods

        /// <summary>
        /// Creates a new <see cref="Volume"/> value from the specified <paramref name="value"/> in liters.
        /// </summary>
        /// <param name="value">New <see cref="Volume"/> value in liters.</param>
        /// <returns>New <see cref="Volume"/> object from the specified <paramref name="value"/> in liters.</returns>
        public static Volume FromLiters(double value)
        {
            return new Volume(value * LitersFactor);
        }

        /// <summary>
        /// Creates a new <see cref="Volume"/> value from the specified <paramref name="value"/> in US teaspoons.
        /// </summary>
        /// <param name="value">New <see cref="Volume"/> value in US teaspoons.</param>
        /// <returns>New <see cref="Volume"/> object from the specified <paramref name="value"/> in US teaspoons.</returns>
        public static Volume FromTeaspoons(double value)
        {
            return new Volume(value * TeaspoonsFactor);
        }

        /// <summary>
        /// Creates a new <see cref="Volume"/> value from the specified <paramref name="value"/> in metric teaspoons.
        /// </summary>
        /// <param name="value">New <see cref="Volume"/> value in metric teaspoons.</param>
        /// <returns>New <see cref="Volume"/> object from the specified <paramref name="value"/> in metric teaspoons.</returns>
        public static Volume FromMetricTeaspoons(double value)
        {
            return new Volume(value * MetricTeaspoonsFactor);
        }

        /// <summary>
        /// Creates a new <see cref="Volume"/> value from the specified <paramref name="value"/> in US tablespoons.
        /// </summary>
        /// <param name="value">New <see cref="Volume"/> value in US tablespoons.</param>
        /// <returns>New <see cref="Volume"/> object from the specified <paramref name="value"/> in US tablespoons.</returns>
        public static Volume FromTablespoons(double value)
        {
            return new Volume(value * TablespoonsFactor);
        }

        /// <summary>
        /// Creates a new <see cref="Volume"/> value from the specified <paramref name="value"/> in metric tablespoons.
        /// </summary>
        /// <param name="value">New <see cref="Volume"/> value in metric tablespoons.</param>
        /// <returns>New <see cref="Volume"/> object from the specified <paramref name="value"/> in metric tablespoons.</returns>
        public static Volume FromMetricTablespoons(double value)
        {
            return new Volume(value * MetricTablespoonsFactor);
        }

        /// <summary>
        /// Creates a new <see cref="Volume"/> value from the specified <paramref name="value"/> in US cups.
        /// </summary>
        /// <param name="value">New <see cref="Volume"/> value in US cups.</param>
        /// <returns>New <see cref="Volume"/> object from the specified <paramref name="value"/> in US cups.</returns>
        public static Volume FromCups(double value)
        {
            return new Volume(value * CupsFactor);
        }

        /// <summary>
        /// Creates a new <see cref="Volume"/> value from the specified <paramref name="value"/> in metric cups.
        /// </summary>
        /// <param name="value">New <see cref="Volume"/> value in metric cups.</param>
        /// <returns>New <see cref="Volume"/> object from the specified <paramref name="value"/> in metric cups.</returns>
        public static Volume FromMetricCups(double value)
        {
            return new Volume(value * MetricCupsFactor);
        }

        /// <summary>
        /// Creates a new <see cref="Volume"/> value from the specified <paramref name="value"/> in US fluid ounces.
        /// </summary>
        /// <param name="value">New <see cref="Volume"/> value in US fluid ounces.</param>
        /// <returns>New <see cref="Volume"/> object from the specified <paramref name="value"/> in US fluid ounces.</returns>
        public static Volume FromFluidOunces(double value)
        {
            return new Volume(value * FluidOuncesFactor);
        }

        /// <summary>
        /// Creates a new <see cref="Volume"/> value from the specified <paramref name="value"/> in US fluid pints.
        /// </summary>
        /// <param name="value">New <see cref="Volume"/> value in US fluid pints.</param>
        /// <returns>New <see cref="Volume"/> object from the specified <paramref name="value"/> in US fluid pints.</returns>
        public static Volume FromPints(double value)
        {
            return new Volume(value * PintsFactor);
        }

        /// <summary>
        /// Creates a new <see cref="Volume"/> value from the specified <paramref name="value"/> in US fluid quarts.
        /// </summary>
        /// <param name="value">New <see cref="Volume"/> value in US fluid quarts.</param>
        /// <returns>New <see cref="Volume"/> object from the specified <paramref name="value"/> in US fluid quarts.</returns>
        public static Volume FromQuarts(double value)
        {
            return new Volume(value * QuartsFactor);
        }

        /// <summary>
        /// Creates a new <see cref="Volume"/> value from the specified <paramref name="value"/> in US fluid gallons.
        /// </summary>
        /// <param name="value">New <see cref="Volume"/> value in US fluid gallons.</param>
        /// <returns>New <see cref="Volume"/> object from the specified <paramref name="value"/> in US fluid gallons.</returns>
        public static Volume FromGallons(double value)
        {
            return new Volume(value * GallonsFactor);
        }

        /// <summary>
        /// Creates a new <see cref="Volume"/> value from the specified <paramref name="value"/> in cubic inches.
        /// </summary>
        /// <param name="value">New <see cref="Volume"/> value in cubic inches.</param>
        /// <returns>New <see cref="Volume"/> object from the specified <paramref name="value"/> in cubic inches.</returns>
        public static Volume FromCubicInches(double value)
        {
            return new Volume(value * CubicInchesFactor);
        }

        /// <summary>
        /// Creates a new <see cref="Volume"/> value from the specified <paramref name="value"/> in cubic feet.
        /// </summary>
        /// <param name="value">New <see cref="Volume"/> value in cubic feet.</param>
        /// <returns>New <see cref="Volume"/> object from the specified <paramref name="value"/> in cubic feet.</returns>
        public static Volume FromCubicFeet(double value)
        {
            return new Volume(value * CubicFeetFactor);
        }

        #endregion
    }
}