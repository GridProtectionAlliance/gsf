//*******************************************************************************************************
//  ComplexNumber.cs
//  Copyright © 2008 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: James R Carroll
//      Office: PSO TRAN & REL, CHATTANOOGA - MR BK-C
//       Phone: 423/751-4165
//       Email: jrcarrol@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  12/31/2008 - James R Carroll
//       Generated original version of source code.
//
//*******************************************************************************************************

using System;
using System.Text;

namespace PCS.NumericalAnalysis
{
    /// <summary>
    /// Represents a complex number.
    /// </summary>
    public struct ComplexNumber : IEquatable<ComplexNumber>
    {
        #region [ Members ]

        // Fields
        private double m_real;          // real component of this complex number
        private double m_imaginary;     // imaginary component of this complex number

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Constructs a <see cref="ComplexNumber"/> from the given values. 
        /// </summary>
        /// <param name="real">The real component of the <see cref="ComplexNumber"/>.</param>
        /// <param name="imaginary">The imaginary component of the <see cref="ComplexNumber"/>.</param>
        public ComplexNumber(double real, double imaginary)
        {
            m_real = real;
            m_imaginary = imaginary;
        }

        /// <summary>
        /// Constructs a <see cref="ComplexNumber"/> from the given <see cref="ComplexNumber"/>.
        /// </summary>
        /// <param name="z"><see cref="ComplexNumber"/> to be copied.</param>
        public ComplexNumber(ComplexNumber z)
        {
            m_real = z.m_real;
            m_imaginary = z.m_imaginary;
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
                return m_real;
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
                return m_imaginary;
            }
            set
            {
                m_imaginary = value;
            }
        }

        /// <summary>
        /// Gets the calculated absolute value (or modulus, a.k.a. the magnitude) of this <see cref="ComplexNumber"/>.
        /// </summary>
        public double AbsoluteValue
        {
            get
            {
                return Math.Sqrt(m_real * m_real + m_imaginary * m_imaginary);
            }
        }

        /// <summary>
        /// Gets the calculated angle (or argument) in radians of this <see cref="ComplexNumber"/>.
        /// </summary>
        public double Angle
        {
            get
            {
                return Math.Atan2(m_imaginary, m_real);
            }
        }

        /// <summary>
        /// Gets the complex conjugate of this <see cref="ComplexNumber"/>.
        /// </summary>
        public ComplexNumber Conjugate
        {
            get
            {
                return new ComplexNumber(m_real, -m_imaginary);
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
            return (m_real.GetHashCode() ^ m_imaginary.GetHashCode());
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

            image.Append(m_real);

            if (m_imaginary != 0.0D)
            {
                image.Append(m_imaginary > 0.0D ? " + " : " - ");
                image.Append(Math.Abs(m_imaginary));
                image.Append("i");
            }

            return image.ToString();
        }

        #endregion

        #region [ Operators ]

        /// <summary>
        /// Implicitly converts a <see cref="Double"/> to a <see cref="ComplexNumber"/>.
        /// </summary>
        public static implicit operator ComplexNumber(double value)
        {
            return new ComplexNumber(value, 0.0D);
        }

        /// <summary>
        /// Compares the two values for equality.
        /// </summary>
        public static bool operator ==(ComplexNumber value1, ComplexNumber value2)
        {
            return (value1.m_real == value2.m_real && value1.m_imaginary == value2.m_imaginary);
        }

        /// <summary>
        /// Compares the two values for inequality.
        /// </summary>
        public static bool operator !=(ComplexNumber value1, ComplexNumber value2)
        {
            return !(value1 == value2);
        }

        /// <summary>
        /// Returns the negated value.
        /// </summary>
        public static ComplexNumber operator -(ComplexNumber z)
        {
            return new ComplexNumber(-z.m_real, -z.m_imaginary);
        }

        /// <summary>
        /// Returns computed sum of values.
        /// </summary>
        public static ComplexNumber operator +(ComplexNumber value1, ComplexNumber value2)
        {
            return new ComplexNumber(value1.m_real + value2.m_real, value1.m_imaginary + value2.m_imaginary);
        }

        /// <summary>
        /// Returns computed difference of values.
        /// </summary>
        public static ComplexNumber operator -(ComplexNumber value1, ComplexNumber value2)
        {
            return new ComplexNumber(value1.m_real - value2.m_real, value1.m_imaginary - value2.m_imaginary);
        }

        /// <summary>
        /// Returns computed product of values.
        /// </summary>
        public static ComplexNumber operator *(ComplexNumber value1, ComplexNumber value2)
        {
            double real = value1.m_real * value2.m_real - value1.m_imaginary * value2.m_imaginary;
            double imaginary = value1.m_imaginary * value2.m_real + value1.m_real * value2.m_imaginary;

            return new ComplexNumber(real, imaginary);
        }

        /// <summary>
        /// Returns computed division of values.
        /// </summary>
        public static ComplexNumber operator /(ComplexNumber value1, ComplexNumber value2)
        {
            double divisor = Math.Pow(value2.m_real, 2) + Math.Pow(value2.m_imaginary, 2);
            double real = (value1.m_real * value2.m_real + value1.m_imaginary * value2.m_imaginary) / divisor;
            double imaginary = (value1.m_imaginary * value2.m_real - value1.m_real * value2.m_imaginary) / divisor;

            return new ComplexNumber(real, imaginary);
        }

        #endregion
    }
}