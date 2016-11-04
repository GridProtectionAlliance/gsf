//******************************************************************************************************
//  ComplexNumber.cs - Gbtc
//
//  Copyright © 2012, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://www.opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  01/25/2008 - J. Ritchie Carroll
//       Initial version of source generated.
//  08/3/2009 - Josh L. Patterson
//       Updated comments.
//  09/14/2009 - Stephen C. Wills
//       Added new header and license agreement.
//  12/14/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//  12/18/2012 - J. Ritchie Carroll
//       Updated operation such that class will used cached angle and magnitude values
//       when these are provided to improve accuracy and operational speed.
//  12/02/2015 - J. Ritchie Carroll
//       Added implicit operators to interact with new .NET Complex structure.
//
//******************************************************************************************************

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
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using GSF.Units;
#if DNF46
using System.Numerics;
#endif

// ReSharper disable CompareOfFloatsByEqualityOperator
// ReSharper disable PossibleInvalidOperationException
namespace GSF
{
    /// <summary>
    /// Represents a complex number.
    /// </summary>
    public struct ComplexNumber : IEquatable<ComplexNumber>
    {
        #region [ Members ]

        // Complex number will cache assigned polar components as an optimization and for increased accuracy,
        // however, preferential treatment is given to rectangular components when used in mixed mode.

        // Fields
        private double? m_real;         // Real value of complex number
        private double? m_imaginary;    // Imaginary value of complex number

        private double? m_angle;        // Angle value of complex number
        private double? m_magnitude;    // Magnitude value of complex number

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
        /// <param name="angle">The <see cref="Angle"/> component, in radians, of the <see cref="ComplexNumber"/>.</param>
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
            // Make sure state of source complex number is replicated exactly
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
                return m_real ?? double.NaN;
            }
            set
            {
                m_real = value;

                // If we are updating a rectangular component, clear polar components so they will be recalculated
                m_angle = null;
                m_magnitude = null;
            }
        }

        /// <summary>
        /// Gets or sets the imaginary component of this <see cref="ComplexNumber"/>.
        /// </summary>
        public double Imaginary
        {
            get
            {
                return m_imaginary ?? double.NaN;
            }
            set
            {
                m_imaginary = value;

                // If we are updating a rectangular component, clear polar components so they will be recalculated
                m_angle = null;
                m_magnitude = null;
            }
        }

        /// <summary>
        /// Gets or sets the magnitude (a.k.a. the modulus or absolute value) of this <see cref="ComplexNumber"/>.
        /// </summary>
        public double Magnitude
        {
            get
            {
                // Return any assigned magnitude value
                if (m_magnitude.HasValue)
                    return m_magnitude.Value;

                if (!AllAssigned)
                    return double.NaN;

                // If complex number is internally represented in rectangular coordinates and no magnitude has been assigned,
                // return a calculated magnitude
                double real = m_real.Value;
                double imaginary = m_imaginary.Value;

                return Math.Sqrt(real * real + imaginary * imaginary);
            }
            set
            {
                // Cache assigned magnitude value
                m_magnitude = value;

                if (!m_real.HasValue || !m_imaginary.HasValue)
                {
                    // Rectangular coordinates have yet to be fully assigned, if all composite polar values have been received,
                    // we can calculate real and imaginary values
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
                // Return any assigned angle value
                if (m_angle.HasValue)
                    return m_angle.Value;

                if (!AllAssigned)
                    return double.NaN;

                // If complex number is internally represented in rectangular coordinates and no angle has been assigned,
                // return a calculated angle
                return Math.Atan2(m_imaginary.Value, m_real.Value);
            }
            set
            {
                // Cache assigned angle value
                m_angle = value;

                if (!m_real.HasValue || !m_imaginary.HasValue)
                {
                    // Rectangular coordinates have yet to be fully assigned, if all composite polar values have been received,
                    // we can calculate real and imaginary values
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
        public ComplexNumber Conjugate => new ComplexNumber(Real, -Imaginary);

        /// <summary>
        /// Gets a boolean value indicating if each composite value of the <see cref="ComplexNumber"/> (i.e., real and imaginary) has been assigned a value.
        /// </summary>
        /// <returns>True, if all composite values have been assigned a value; otherwise, false.</returns>
        public bool AllAssigned => m_real.HasValue && m_imaginary.HasValue;

        /// <summary>
        /// Gets a boolean value indicating if each composite value of the <see cref="ComplexNumber"/> (i.e., real and imaginary) has not been assigned a value.
        /// </summary>
        /// <returns>True, if none of the composite values have been assigned a value; otherwise, false.</returns>
        public bool NoneAssigned => !m_real.HasValue && !m_imaginary.HasValue;

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Returns a value indicating whether this instance is equal to a specified object.
        /// </summary>
        /// <param name="obj">An object to compare, or null.</param>
        /// <returns>
        /// True if <paramref name="obj"/> is an instance of ComplexNumber and equals the value of this instance;
        /// otherwise, False.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (obj is ComplexNumber)
                return Equals((ComplexNumber)obj);

            return false;
        }

        /// <summary>
        /// Returns a value indicating whether this instance is equal to a specified ComplexNumber value.
        /// </summary>
        /// <param name="obj">A <see cref="ComplexNumber"/> to compare to this instance.</param>
        /// <returns>
        /// True if <paramref name="obj"/> has the same value as this instance; otherwise, False.
        /// </returns>
        public bool Equals(ComplexNumber obj) => this == obj;

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>
        /// A 32-bit signed integer hash code.
        /// </returns>
        public override int GetHashCode() => Real.GetHashCode() ^ Imaginary.GetHashCode();

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
            }
        }

        #endregion

        #region [ Operators ]

        /// <summary>
        /// Implicitly converts a <see cref="Double"/> to a <see cref="ComplexNumber"/>.
        /// </summary>
        /// <param name="value">Operand.</param>
        /// <returns>ComplexNumber representing the result of the operation.</returns>
        public static implicit operator ComplexNumber(double value) => new ComplexNumber(value, 0.0D);

        #if DNF46

        /// <summary>
        /// Implicitly converts a <see cref="ComplexNumber"/> to a .NET <see cref="Complex"/> value.
        /// </summary>
        /// <param name="value">Operand.</param>
        /// <returns>Complex representing the result of the operation.</returns>
        public static implicit operator Complex(ComplexNumber value) => new Complex(value.Real, value.Imaginary);

        /// <summary>
        /// Implicitly converts a .NET <see cref="Complex"/> value to a <see cref="ComplexNumber"/>.
        /// </summary>
        /// <param name="value">Operand.</param>
        /// <returns>Complex representing the result of the operation.</returns>
        public static implicit operator ComplexNumber(Complex value) => new ComplexNumber(value.Real, value.Imaginary);

        #endif

        /// <summary>
        /// Compares the two values for equality.
        /// </summary>
        /// <param name="value1">Left hand operand.</param>
        /// <param name="value2">Right hand operand.</param>
        /// <returns>Boolean representing the result of the addition operation.</returns>
        public static bool operator ==(ComplexNumber value1, ComplexNumber value2) => value1.Real == value2.Real && value1.Imaginary == value2.Imaginary;

        /// <summary>
        /// Compares the two values for inequality.
        /// </summary>
        /// <param name="value1">Left hand operand.</param>
        /// <param name="value2">Right hand operand.</param>
        /// <returns>Boolean representing the result of the inequality operation.</returns>
        public static bool operator !=(ComplexNumber value1, ComplexNumber value2) => !(value1 == value2);

        /// <summary>
        /// Returns the negated value.
        /// </summary>
        /// <param name="z">Left hand operand.</param>
        /// <returns>ComplexNumber representing the result of the unary negation operation.</returns>
        public static ComplexNumber operator -(ComplexNumber z) => new ComplexNumber(-z.Real, -z.Imaginary);

        /// <summary>
        /// Returns computed sum of values.
        /// </summary>
        /// <param name="value1">Left hand operand.</param>
        /// <param name="value2">Right hand operand.</param>
        /// <returns>ComplexNumber representing the result of the addition operation.</returns>
        public static ComplexNumber operator +(ComplexNumber value1, ComplexNumber value2) => new ComplexNumber(value1.Real + value2.Real, value1.Imaginary + value2.Imaginary);

        /// <summary>
        /// Returns computed difference of values.
        /// </summary>
        /// <param name="value1">Left hand operand.</param>
        /// <param name="value2">Right hand operand.</param>
        /// <returns>ComplexNumber representing the result of the subtraction operation.</returns>
        public static ComplexNumber operator -(ComplexNumber value1, ComplexNumber value2) => new ComplexNumber(value1.Real - value2.Real, value1.Imaginary - value2.Imaginary);

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
        /// Returns specified <see cref="ComplexNumber"/> raised to the specified power.
        ///</summary>
        ///<param name="z">Complex number to be raised to power <paramref name="y"/>.</param>
        ///<param name="y">Power to raise <see cref="ComplexNumber"/> <paramref name="z"/>.</param>
        /// <returns>ComplexNumber representing the result of the operation.</returns>
        public static ComplexNumber Pow(ComplexNumber z, double y) => new ComplexNumber(z.Angle * y, Math.Pow(z.Magnitude, y));

        // C# doesn't expose an exponent operator but some other .NET languages do,
        // so we expose the operator via its native special IL function name

        /// <summary>
        /// Returns result of first value raised to power of second value.
        /// </summary>
        ///<param name="z">Complex number to be raised to power <paramref name="y"/>.</param>
        ///<param name="y">Power to raise <see cref="ComplexNumber"/> <paramref name="z"/>.</param>
        /// <returns>ComplexNumber representing the result of the operation.</returns>
        [EditorBrowsable(EditorBrowsableState.Advanced), SpecialName]
        public static ComplexNumber op_Exponent(ComplexNumber z, double y) => Pow(z, y);

        #endregion

        #region [ Static ]

        // Static Methods

        /// <summary>
        /// Converts the string representation of a complex number to its complex number equivalent.
        /// </summary>
        /// <param name="str">A string that contains a number to convert.</param>
        /// <returns>A complex number that is equivalent to the numeric value or symbols specified in <paramref name="str"/>.</returns>
        public static ComplexNumber Parse(string str)
        {
            // Regex pattern to match a single number
            const string NumberPattern = @"(?<Number>[+-]?(?:[ij][0-9eE\.]+|[0-9eE\.]+[ij]?))";

            // Regex pattern to match the operator
            const string OperatorPattern = @"(?<Operator>[+-])";

            // Regex pattern to match the whole complex number
            //
            const string Pattern = @"^" +               // Start of string
                                   @"\s*" +             // Whitespace
                                   NumberPattern +      // Number
                                   @"\s*" +             // Whitespace
                                   "(?:" +              // Start of optional of noncapturing group
                                   OperatorPattern +    // Operator
                                   @"\s*" +             // Whitespace
                                   NumberPattern +      // Number
                                   @"\s*" +             // Whitespace
                                   ")?" +               // End of optional noncapturing group
                                   "$";                 // End of string

            Match match;
            CaptureCollection numberCaptures;
            CaptureCollection operatorCaptures;

            double op = 1.0;
            double real = 0.0D;
            double imaginary = 0.0D;

            // Parses the string as a double by first removing the i or j
            Func<string, double> parse = s => double.Parse(Regex.Replace(s, "[ij]", ""));
            Func<string, bool> isImaginary = s => Regex.IsMatch(s, "[ij]");

            match = Regex.Match(str, Pattern);

            // String format is invalid if regex does not match
            if (!match.Success)
                throw new FormatException("Input string was not in a correct format.");

            // Get the captures for the Number and Operator groups
            numberCaptures = match.Groups["Number"].Captures;
            operatorCaptures = match.Groups["Operator"].Captures;

            // If the string defines two numbers, ensure that exactly one of them is imaginary
            if (numberCaptures.Count == 2 && !(isImaginary(numberCaptures[0].Value) ^ isImaginary(numberCaptures[1].Value)))
                throw new FormatException("Input string was not in a correct format.");

            // Parse the first capture group to the
            // appropriate part of the complex number
            if (isImaginary(numberCaptures[0].Value))
                imaginary = parse(numberCaptures[0].Value);
            else
                real = parse(numberCaptures[0].Value);
            
            if (numberCaptures.Count == 2)
            {
                // Determine if the sign needs to be
                // inverted based on the operator
                if (operatorCaptures[0].Value == "-")
                    op = -1.0;

                // Parse the second capture group to the
                // appropriate part of the complex number
                if (isImaginary(numberCaptures[1].Value))
                    imaginary = op * parse(numberCaptures[1].Value);
                else
                    real = op * parse(numberCaptures[1].Value);
            }

            // Return the complex number
            return new ComplexNumber(real, imaginary);
        }

        #endregion
    }
}