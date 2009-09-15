//*******************************************************************************************************
//  ComplexNumber.cs - Gbtc
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
//  8/3/2009 - Josh L. Patterson
//       Updated comments.
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
using System.Text;
using TVA.Units;

namespace TVA
{
    /// <summary>
    /// Represents a complex number.
    /// </summary>
    public struct ComplexNumber : IEquatable<ComplexNumber>
    {
        #region [ Members ]

        // Fields
        private double? m_real;         // Real value of complex number
        private double? m_imaginary;    // Imaginary value of complex number

        // Polar value fields will only cache values until both exist in order
        // to create equivalent rectangular values
        private double? m_angle;        // Temporary angle value of complex number
        private double? m_magnitude;    // Temporary magnitude value of complex number
        
        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a <see cref="ComplexNumber"/> from the given rectangular values. 
        /// </summary>
        /// <param name="real">The real component of the <see cref="ComplexNumber"/>.</param>
        /// <param name="imaginary">The imaginary component of the <see cref="ComplexNumber"/>.</param>
        public ComplexNumber(double real, double imaginary)
            : this()
        {
            m_real = real;
            m_imaginary = imaginary;
        }

        /// <summary>
        /// Creates a <see cref="ComplexNumber"/> from the given polar values.
        /// </summary>
        /// <param name="angle">The angle component, in radians, of the <see cref="ComplexNumber"/>.</param>
        /// <param name="magnitude">The magnitude (or absolute value) component of the <see cref="ComplexNumber"/>.</param>
        public ComplexNumber(Angle angle, double magnitude)
            : this()
        {
            m_angle = angle;
            m_magnitude = magnitude;

            CalculateRectangularFromPolar();
        }

        /// <summary>
        /// Creates a <see cref="ComplexNumber"/> from the given <see cref="ComplexNumber"/>.
        /// </summary>
        /// <param name="z"><see cref="ComplexNumber"/> to be copied.</param>
        public ComplexNumber(ComplexNumber z)
            : this()
        {
            // Make sure state of source complex number is replicated extactly
            m_real = z.m_real;
            m_imaginary = z.m_imaginary;
            
            m_angle = z.m_angle;
            m_magnitude = z.m_magnitude;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the real component of this <see cref="ComplexNumber"/>.
        /// </summary>
        public double Real
        {
            get
            {
                return m_real.GetValueOrDefault();
            }
            set
            {
                m_real = value;
            }
        }

        /// <summary>
        /// Gets or sets the imaginary component of this <see cref="ComplexNumber"/>.
        /// </summary>
        public double Imaginary
        {
            get
            {
                return m_imaginary.GetValueOrDefault();
            }
            set
            {
                m_imaginary = value;
            }
        }

        /// <summary>
        /// Gets or sets the magnitude (a.k.a. the modulus or absolute value) of this <see cref="ComplexNumber"/>.
        /// </summary>
        public double Magnitude
        {
            get
            {
                if (AllAssigned)
                {
                    // Complex number is internally represented in rectangluar coordinates, so we return calculated magnitude
                    double real = m_real.Value;
                    double imaginary = m_imaginary.Value;

                    return Math.Sqrt(real * real + imaginary * imaginary);
                }
                else if (m_magnitude.HasValue)
                {
                    // Return any assigned value if magnitude can't be calculated
                    return m_magnitude.Value;
                }
                else
                    return double.NaN;
            }
            set
            {
                if (!m_real.HasValue || !m_imaginary.HasValue)
                {
                    // Complex number is internally represented in rectangluar coordinates but these values have yet to be
                    // fully assigned so we cache magnitude so we can calculate the real and imaginary components once we
                    // also receive the angle value
                    m_magnitude = value;

                    // If all composite polar values have been received, we can calculate real and imaginary values
                    CalculateRectangularFromPolar();
                }
                else
                {
                    // Rectangular values have already been assigned, user is simply requesting to change complex number
                    // by updating its absolute value so we calculate a new complex number based on the updated polar
                    // coordinates and then update the real and imaginary components
                    ComplexNumber updatedValue = new ComplexNumber(Angle, value);

                    m_real = updatedValue.m_real;
                    m_imaginary = updatedValue.m_imaginary;
                }
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="Angle"/> (a.k.a. the argument) in radians of this <see cref="ComplexNumber"/>.
        /// </summary>
        public Angle Angle
        {
            get
            {
                if (AllAssigned)
                {
                    // Complex number is internally represented in rectangluar coordinates, so we return calculated angle
                    return Math.Atan2(m_imaginary.Value, m_real.Value);
                }
                else if (m_angle.HasValue)
                {
                    // Return any assigned value if angle can't be calculated
                    return m_angle.Value;
                }
                else
                    return double.NaN;
            }
            set
            {
                if (!m_real.HasValue || !m_imaginary.HasValue)
                {
                    // Complex number is internally represented in rectangluar coordinates but these values have yet to be
                    // fully assigned so we cache angle so we can calculate the real and imaginary components once we also
                    // receive the magnitude value
                    m_angle = value;

                    // If all composite polar values have been received, we can calculate real and imaginary values
                    CalculateRectangularFromPolar();
                }
                else
                {
                    // Rectangular values have already been assigned, user is simply requesting to change complex number
                    // by updating its angle value so we calculate a new complex number based on the updated polar
                    // coordinates and then update the real and imaginary components
                    ComplexNumber updatedValue = new ComplexNumber(value, Magnitude);

                    m_real = updatedValue.m_real;
                    m_imaginary = updatedValue.m_imaginary;
                }
            }
        }

        /// <summary>
        /// Gets the complex conjugate of this <see cref="ComplexNumber"/>.
        /// </summary>
        public ComplexNumber Conjugate
        {
            get
            {
                return new ComplexNumber(Real, -Imaginary);
            }
        }

        /// <summary>
        /// Gets a boolean value indicating if each composite value of the <see cref="ComplexNumber"/> (i.e., real and imaginary) has been assigned a value.
        /// </summary>
        /// <returns>True, if all composite values have been assigned a value; otherwise, false.</returns>
        public bool AllAssigned
        {
            get
            {
                return m_real.HasValue && m_imaginary.HasValue;
            }
        }

        /// <summary>
        /// Gets a boolean value indicating if each composite value of the <see cref="ComplexNumber"/> (i.e., real and imaginary) has not been assigned a value.
        /// </summary>
        /// <returns>True, if none of the composite values have been assigned a value; otherwise, false.</returns>
        public bool NoneAssigned
        {
            get
            {
                return !m_real.HasValue && !m_imaginary.HasValue;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Returns a value indicating whether this instance is equal to a specified object.
        /// </summary>
        /// <param name="obj">An object to compare, or null.</param>
        /// <returns>
        /// True if obj is an instance of ComplexNumber and equals the value of this instance;
        /// otherwise, False.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (obj is ComplexNumber) return Equals((ComplexNumber)obj);
            return false;
        }

        /// <summary>
        /// Returns a value indicating whether this instance is equal to a specified Int24 value.
        /// </summary>
        /// <param name="obj">A <see cref="ComplexNumber"/> to compare to this instance.</param>
        /// <returns>
        /// True if <paramref name="obj"/> has the same value as this instance; otherwise, False.
        /// </returns>
        public bool Equals(ComplexNumber obj)
        {
            return (this == obj);
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>
        /// A 32-bit signed integer hash code.
        /// </returns>
        public override int GetHashCode()
        {
            return (Real.GetHashCode() ^ Imaginary.GetHashCode());
        }

        /// <summary>
        /// Converts the numeric value of this instance to its equivalent string representation.
        /// </summary>
        /// <returns>
        /// The string representation of the value of this <see cref="ComplexNumber"/> instance.
        /// </returns>
        public override string ToString()
        {
            StringBuilder image = new StringBuilder();

            image.Append(Real);

            if (Imaginary != 0.0D)
            {
                image.Append(Imaginary > 0.0D ? " + " : " - ");
                image.Append(Math.Abs(Imaginary));
                image.Append('i');
            }

            return image.ToString();
        }

        // Calculate real and imaginary components from angle and magnitude
        private void CalculateRectangularFromPolar()
        {
            if (m_angle.HasValue && m_magnitude.HasValue)
            {
                // All values assigned, calculate a new rectangular based complex number from its polar composite values
                double angle = m_angle.Value;
                double magnitude = m_magnitude.Value;

                m_real = magnitude * Math.Cos(angle);
                m_imaginary = magnitude * Math.Sin(angle);

                // Once rectangular values are available, polar values are no longer needed
                m_angle = null;
                m_magnitude = null;
            }
        }

        #endregion

        #region [ Operators ]

        /// <summary>
        /// Implicitly converts a <see cref="Double"/> to a <see cref="ComplexNumber"/>.
        /// </summary>
        /// <param name="value">Operand.</param>
        /// <returns>ComplexNumber representing the result of the operation.</returns>
        public static implicit operator ComplexNumber(double value)
        {
            return new ComplexNumber(value, 0.0D);
        }

        /// <summary>
        /// Compares the two values for equality.
        /// </summary>
        /// <param name="value1">Left hand operand.</param>
        /// <param name="value2">Right hand operand.</param>
        /// <returns>Boolean representing the result of the addition operation.</returns>
        public static bool operator ==(ComplexNumber value1, ComplexNumber value2)
        {
            return (value1.Real == value2.Real && value1.Imaginary == value2.Imaginary);
        }

        /// <summary>
        /// Compares the two values for inequality.
        /// </summary>
        /// <param name="value1">Left hand operand.</param>
        /// <param name="value2">Right hand operand.</param>
        /// <returns>Boolean representing the result of the inequality operation.</returns>
        public static bool operator !=(ComplexNumber value1, ComplexNumber value2)
        {
            return !(value1 == value2);
        }

        /// <summary>
        /// Returns the negated value.
        /// </summary>
        /// <param name="z">Left hand operand.</param>
        /// <returns>ComplexNumber representing the result of the unary negation operation.</returns>
        public static ComplexNumber operator -(ComplexNumber z)
        {
            return new ComplexNumber(-z.Real, -z.Imaginary);
        }

        /// <summary>
        /// Returns computed sum of values.
        /// </summary>
        /// <param name="value1">Left hand operand.</param>
        /// <param name="value2">Right hand operand.</param>
        /// <returns>ComplexNumber representing the result of the addition operation.</returns>
        public static ComplexNumber operator +(ComplexNumber value1, ComplexNumber value2)
        {
            return new ComplexNumber(value1.Real + value2.Real, value1.Imaginary + value2.Imaginary);
        }

        /// <summary>
        /// Returns computed difference of values.
        /// </summary>
        /// <param name="value1">Left hand operand.</param>
        /// <param name="value2">Right hand operand.</param>
        /// <returns>ComplexNumber representing the result of the subtraction operation.</returns>
        public static ComplexNumber operator -(ComplexNumber value1, ComplexNumber value2)
        {
            return new ComplexNumber(value1.Real - value2.Real, value1.Imaginary - value2.Imaginary);
        }

        /// <summary>
        /// Returns computed product of values.
        /// </summary>
        /// <param name="value1">Left hand operand.</param>
        /// <param name="value2">Right hand operand.</param>
        /// <returns>ComplexNumber representing the result of the multiplication operation.</returns>
        public static ComplexNumber operator *(ComplexNumber value1, ComplexNumber value2)
        {
            double real = value1.Real * value2.Real - value1.Imaginary * value2.Imaginary;
            double imaginary = value1.Imaginary * value2.Real + value1.Real * value2.Imaginary;

            return new ComplexNumber(real, imaginary);
        }

        /// <summary>
        /// Returns computed division of values.
        /// </summary>
        /// <param name="value1">Left hand operand.</param>
        /// <param name="value2">Right hand operand.</param>
        /// <returns>ComplexNumber representing the result of the division operation.</returns>
        public static ComplexNumber operator /(ComplexNumber value1, ComplexNumber value2)
        {
            double divisor = Math.Pow(value2.Real, 2) + Math.Pow(value2.Imaginary, 2);
            double real = (value1.Real * value2.Real + value1.Imaginary * value2.Imaginary) / divisor;
            double imaginary = (value1.Imaginary * value2.Real - value1.Real * value2.Imaginary) / divisor;

            return new ComplexNumber(real, imaginary);
        }

        ///<summary>
        ///Returns specified <see cref="ComplexNumber"/> raised to the specified power.
        ///</summary>
        ///<param name="z">Complex number to be raised to power <paramref name="y"/>.</param>
        ///<param name="y">Power to raise <see cref="ComplexNumber"/> <paramref name="z"/>.</param>
        /// <returns>ComplexNumber representing the result of the operation.</returns>
        public static ComplexNumber Pow(ComplexNumber z, double y)
        {
            return new ComplexNumber(z.Angle * y, Math.Pow(z.Magnitude, y));
        }

        #endregion
    }
}