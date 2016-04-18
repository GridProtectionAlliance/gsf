//******************************************************************************************************
//  ComplexNumber.cs - Gbtc
//
//  Copyright © 2016, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  04/14/2016 - Stephen C. Wills
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Data.SqlTypes;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.SqlServer.Server;

namespace GSF.Core.SqlClr
{
    /// <summary>
    /// Defines a complex number type that can be used in a SQL Server database.
    /// </summary>
    [Serializable]
    [SqlUserDefinedType(Format.Native, IsByteOrdered = true)]
    public struct ComplexNumber : INullable
    {
        #region [ Members ]

        // Fields
        private bool m_isNull;
        private double m_real;
        private double m_imaginary;

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets a value that indicates whether the complex number is null.
        /// </summary>
        public bool IsNull
        {
            get
            {
                return m_isNull;
            }
        }

        /// <summary>
        /// Gets or sets the real component of a complex number.
        /// </summary>
        /// <remarks>
        /// Usage example:
        /// 
        /// <code>
        /// DECLARE @tbl TABLE(Num COMPLEX)
        /// INSERT INTO @tbl VALUES('1+5i')
        /// UPDATE @tbl SET Num.Real = 5
        /// SELECT Num.ToString(), Num.Real FROM @tbl
        /// -- Returns [Num: 5+5i, Real: 5]
        /// </code>
        /// </remarks>
        public double Real
        {
            get
            {
                return m_real;
            }
            set
            {
                m_real = value;
            }
        }

        /// <summary>
        /// Gets or sets the imaginary component of a complex number.
        /// </summary>
        /// <remarks>
        /// Usage example:
        /// 
        /// <code>
        /// DECLARE @tbl TABLE(Num COMPLEX)
        /// INSERT INTO @tbl VALUES('1+5i')
        /// UPDATE @tbl SET Num.Imaginary = 1
        /// SELECT Num.ToString(), Num.Imaginary FROM @tbl
        /// -- Returns [Num: 1+1i, Imaginary: 1]
        /// </code>
        /// </remarks>
        public double Imaginary
        {
            get
            {
                return m_imaginary;
            }
            set
            {
                m_imaginary = value;
            }
        }

        /// <summary>
        /// Gets or sets the magnitude of a complex number.
        /// </summary>
        /// <remarks>
        /// Usage example:
        /// 
        /// <code>
        /// DECLARE @tbl TABLE(Num COMPLEX)
        /// INSERT INTO @tbl VALUES('1+5i')
        /// UPDATE @tbl SET Num.Magnitude = 5
        /// SELECT Num.ToString(), Num.Magnitude FROM @tbl
        /// -- Returns [Num: 0.98058+4.9029i, Magnitude: 5]
        /// </code>
        /// </remarks>
        public double Magnitude
        {
            get
            {
                return Math.Sqrt(m_real * m_real + m_imaginary * m_imaginary);
            }
            set
            {
                double angle = Angle;
                m_real = value * Math.Cos(angle);
                m_imaginary = value * Math.Sin(angle);
            }
        }

        /// <summary>
        /// Gets or sets the angle of a complex number.
        /// </summary>
        /// <remarks>
        /// Usage example:
        /// 
        /// <code>
        /// DECLARE @tbl TABLE(Num COMPLEX)
        /// INSERT INTO @tbl VALUES('1+5i')
        /// UPDATE @tbl SET Num.Angle = 1
        /// SELECT Num.ToString(), Num.Angle FROM @tbl
        /// -- Returns [Num: 2.75501+4.29068i, Angle: 1]
        /// </code>
        /// </remarks>
        public double Angle
        {
            get
            {
                return Math.Atan2(m_imaginary, m_real);
            }
            set
            {
                double magnitude = Magnitude;
                m_real = magnitude * Math.Cos(value);
                m_imaginary = magnitude * Math.Sin(value);
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Returns the complex conjugate of a complex number.
        /// </summary>
        /// <returns>The complex conjugate.</returns>
        /// <remarks>
        /// Usage example:
        /// 
        /// <code>
        /// DECLARE @complex COMPLEX = '1+5i'
        /// SELECT @complex.Conjugate().ToString()
        /// -- Returns 1-5i
        /// </code>
        /// </remarks>
        [SqlMethod(OnNullCall = false, IsDeterministic = true)]
        public ComplexNumber Conjugate()
        {
            ComplexNumber num = new ComplexNumber();
            num.m_real = m_real;
            num.m_imaginary = -m_imaginary;
            return num;
        }

        /// <summary>
        /// Returns the negative value of the complex number.
        /// </summary>
        /// <returns>The negative value.</returns>
        /// <remarks>
        /// Usage example:
        /// 
        /// <code>
        /// DECLARE @complex COMPLEX = '1+5i'
        /// SELECT @complex.Negate().ToString()
        /// -- Returns -1-5i
        /// </code>
        /// </remarks>
        [SqlMethod(OnNullCall = false, IsDeterministic = true)]
        public ComplexNumber Negate()
        {
            ComplexNumber num = new ComplexNumber();
            num.m_real = -m_real;
            num.m_imaginary = -m_imaginary;
            return num;
        }

        /// <summary>
        /// Returns the sum of two complex numbers.
        /// </summary>
        /// <param name="num">The number to be added to this one.</param>
        /// <returns>The sum.</returns>
        /// <remarks>
        /// Usage example:
        /// 
        /// <code>
        /// DECLARE @complex COMPLEX = '1+5i'
        /// SELECT @complex.[Add]('-5+1i').ToString()
        /// -- Returns -4+6i
        /// </code>
        /// </remarks>
        [SqlMethod(OnNullCall = false, IsDeterministic = true)]
        public ComplexNumber Add(ComplexNumber num)
        {
            ComplexNumber result = new ComplexNumber();
            result.m_real = m_real + num.m_real;
            result.m_imaginary = m_imaginary + num.m_imaginary;
            return result;
        }

        /// <summary>
        /// Returns the difference of two complex numbers.
        /// </summary>
        /// <param name="num">The number to be subtracted from this one.</param>
        /// <returns>The difference.</returns>
        /// <remarks>
        /// Usage example:
        /// 
        /// <code>
        /// DECLARE @complex COMPLEX = '1+5i'
        /// SELECT @complex.Subtract('-5+1i').ToString()
        /// -- Returns 6+4i
        /// </code>
        /// </remarks>
        [SqlMethod(OnNullCall = false, IsDeterministic = true)]
        public ComplexNumber Subtract(ComplexNumber num)
        {
            ComplexNumber result = new ComplexNumber();
            result.m_real = m_real - num.m_real;
            result.m_imaginary = m_imaginary - num.m_imaginary;
            return result;
        }

        /// <summary>
        /// Returns the product of two complex numbers.
        /// </summary>
        /// <param name="num">The number to be multiplied with this one.</param>
        /// <returns>The product.</returns>
        /// <remarks>
        /// Usage example:
        /// 
        /// <code>
        /// DECLARE @complex COMPLEX = '1+5i'
        /// SELECT @complex.Multiply('-5+1i').ToString()
        /// -- Returns -10-24i
        /// </code>
        /// </remarks>
        [SqlMethod(OnNullCall = false, IsDeterministic = true)]
        public ComplexNumber Multiply(ComplexNumber num)
        {
            ComplexNumber result = new ComplexNumber();
            result.SetPhasor(Magnitude * num.Magnitude, Angle + num.Angle);
            return result;
        }

        /// <summary>
        /// Returns the quotient of two complex numbers.
        /// </summary>
        /// <param name="num">The number to divide by this one.</param>
        /// <returns>The quotient.</returns>
        /// <remarks>
        /// Usage example:
        /// 
        /// <code>
        /// DECLARE @complex COMPLEX = '1+5i'
        /// SELECT @complex.Divide('-5+1i').ToString()
        /// -- Returns -1i
        /// </code>
        /// </remarks>
        [SqlMethod(OnNullCall = false, IsDeterministic = true)]
        public ComplexNumber Divide(ComplexNumber num)
        {
            ComplexNumber result = new ComplexNumber();
            result.SetPhasor(Magnitude / num.Magnitude, Angle - num.Angle);
            return result;
        }

        /// <summary>
        /// Sets the real and imaginary components of the complex number.
        /// </summary>
        /// <param name="real">The real component.</param>
        /// <param name="imaginary">The imaginary component.</param>
        /// <remarks>
        /// Usage example:
        /// 
        /// <code>
        /// DECLARE @tbl TABLE(Num COMPLEX)
        /// INSERT INTO @tbl VALUES('1+5i')
        /// UPDATE @tbl SET Num.SetRect(-5, 1)
        /// SELECT Num.ToString() FROM @tbl
        /// -- Returns -5+1i
        /// </code>
        /// </remarks>
        [SqlMethod(IsMutator = true, OnNullCall = false)]
        public void SetRect(double real, double imaginary)
        {
            m_real = real;
            m_imaginary = imaginary;
        }

        /// <summary>
        /// Sets the magnitude and angle of the vector described by a complex number.
        /// </summary>
        /// <param name="magnitude">The magnitude.</param>
        /// <param name="angle">The angle.</param>
        /// <remarks>
        /// Usage example:
        /// 
        /// <code>
        /// DECLARE @tbl TABLE(Num COMPLEX)
        /// INSERT INTO @tbl VALUES('1+5i')
        /// UPDATE @tbl SET Num.SetPhasor(-5, 1)
        /// SELECT Num.ToString() FROM @tbl
        /// -- Returns -2.70151-4.20735i
        /// </code>
        /// </remarks>
        [SqlMethod(IsMutator = true, OnNullCall = false)]
        public void SetPhasor(double magnitude, double angle)
        {
            m_real = magnitude * Math.Cos(angle);
            m_imaginary = magnitude * Math.Sin(angle);
        }

        /// <summary>
        /// Returns the string representation of a complex number.
        /// </summary>
        /// <returns>The string representation of the complex number.</returns>
        public override string ToString()
        {
            StringBuilder image = new StringBuilder();
            double real = Math.Round(m_real, 5);
            double imaginary = Math.Round(m_imaginary, 5);

            if (real == 0.0D && imaginary == 0.0D)
                return "0";

            if (real != 0.0D)
                image.Append(real);

            if (imaginary != 0.0D)
            {
                image.Append(imaginary > 0.0D ? "+" : "-");
                image.Append(Math.Abs(imaginary));
                image.Append('i');
            }

            return image.ToString();
        }

        #endregion

        #region [ Static ]

        // Static Properties

        /// <summary>
        /// Gets a value that represents a null complex number.
        /// </summary>
        public static ComplexNumber Null
        {
            get
            {
                ComplexNumber num = new ComplexNumber();
                num.m_isNull = true;
                return num;
            }
        }

        // Static Methods

        /// <summary>
        /// Converts the string representation of a complex number to a complex number.
        /// </summary>
        /// <param name="str">The string representation of a complex number.</param>
        /// <returns>The complex number.</returns>
        [SqlMethod(OnNullCall = false)]
        public static ComplexNumber Parse(SqlString str)
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

            ComplexNumber num;

            if (str.IsNull)
                return Null;

            match = Regex.Match(str.Value, Pattern);

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
            num = new ComplexNumber();
            num.m_real = real;
            num.m_imaginary = imaginary;
            return num;
        }

        #endregion
    }
}
