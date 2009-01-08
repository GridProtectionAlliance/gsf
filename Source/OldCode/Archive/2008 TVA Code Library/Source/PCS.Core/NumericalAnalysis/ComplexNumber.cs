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
    public class ComplexNumber : IEquatable<ComplexNumber>
    {
        #region [ Members ]

        // Constants
        private const int RealComponent = 0;                // Index of real component within rectangular based compound value
        private const int ImaginaryComponent = 1;           // Index of imaginary component within rectangular based compound value
        
        private const int AngleComponent = 0;               // Index of angle component within polar based compound value
        private const int MagnitudeComponent = 1;           // Index of magnitude component within polar based compound value

        // Fields
        private CompoundValue<double> m_rectangularValues;  // Compound value containing the real and imaginary components of this complex number
        private CompoundValue<double> m_polarValues;        // Compound value containing the angle and magnitude components of this complex number
        
        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Constructs a <see cref="ComplexNumber"/>. 
        /// </summary>
        public ComplexNumber()
        {
            // Create a new compound value with two composite components; complex number is internally
            // stored in rectagular coordinates.
            m_rectangularValues = new CompoundValue<double>(2);
        }

        /// <summary>
        /// Constructs a <see cref="ComplexNumber"/> from the given rectangular values. 
        /// </summary>
        /// <param name="real">The real component of the <see cref="ComplexNumber"/>.</param>
        /// <param name="imaginary">The imaginary component of the <see cref="ComplexNumber"/>.</param>
        public ComplexNumber(double real, double imaginary)
        {
            m_rectangularValues = new CompoundValue<double>();
            
            m_rectangularValues.Add(real);          // Real component added at index zero
            m_rectangularValues.Add(imaginary);     // Imaginary component added at index one
        }

        /// <summary>
        /// Constructs a <see cref="ComplexNumber"/> from the given polar values.
        /// </summary>
        /// <param name="angle">The angle component, in radians, of the <see cref="ComplexNumber"/>.</param>
        /// <param name="absoluteValue">The absolute value component of the <see cref="ComplexNumber"/>.</param>
        public ComplexNumber(Angle angle, double absoluteValue)
            : this()
        {
            m_polarValues = new CompoundValue<double>();
            
            m_polarValues.Add(angle);               // Angle component added at index zero
            m_polarValues.Add(absoluteValue);       // Magnitude component added at index one

            CalculateRectangularFromPolar();
        }

        /// <summary>
        /// Constructs a <see cref="ComplexNumber"/> from the given <see cref="ComplexNumber"/>.
        /// </summary>
        /// <param name="z"><see cref="ComplexNumber"/> to be copied.</param>
        public ComplexNumber(ComplexNumber z)
            : this(z.Real, z.Imaginary)
        {
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
                return m_rectangularValues[RealComponent].GetValueOrDefault();
            }
            set
            {
                m_rectangularValues[RealComponent] = value;
            }
        }

        /// <summary>
        /// Gets or sets the imaginary component of this <see cref="ComplexNumber"/>.
        /// </summary>
        public double Imaginary
        {
            get
            {
                return m_rectangularValues[ImaginaryComponent].GetValueOrDefault();
            }
            set
            {
                m_rectangularValues[ImaginaryComponent] = value;
            }
        }

        /// <summary>
        /// Gets or sets the absolute value (a.k.a. the modulus or magnitude) of this <see cref="ComplexNumber"/>.
        /// </summary>
        public double AbsoluteValue
        {
            get
            {
                if (m_rectangularValues.AllAssigned)
                {
                    // Complex number is internally represented in rectangluar coordinates, so we return calculated magnitude
                    double real = m_rectangularValues[RealComponent].Value;
                    double imaginary = m_rectangularValues[ImaginaryComponent].Value;

                    return Math.Sqrt(real * real + imaginary * imaginary);
                }
                else if (m_polarValues != null)
                {
                    // Return any assigned value if magnitude can't be calculated
                    return m_polarValues[MagnitudeComponent].GetValueOrDefault();
                }
                else
                    return double.NaN;
            }
            set
            {
                if (m_rectangularValues.NoneAssigned)
                {
                    // Complex number is internally represented in rectangluar coordinates but these values have yet to be
                    // assigned so we cache magnitude so we can calculate the real and imaginary components once we also
                    // receive the angle value

                    // Create a compound value for polar coordinates if it hasn't been created yet (only created if needed)
                    if (m_polarValues == null)
                        m_polarValues = new CompoundValue<double>(2);

                    // Assign absolute value component of compound value
                    m_polarValues[MagnitudeComponent] = value;

                    // If all composite polar values have been received, we can calculate real and imaginary values
                    CalculateRectangularFromPolar();
                }
                else
                {
                    // Rectangular values have already been assigned, user is simply requesting to change complex number
                    // by updating its absolute value so we calculate a new complex number based on the updated polar
                    // coordinates and then update the real and imaginary components
                    ComplexNumber updatedValue = new ComplexNumber(Angle, value);

                    m_rectangularValues[RealComponent] = updatedValue.Real;
                    m_rectangularValues[ImaginaryComponent] = updatedValue.Imaginary;
                }
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="NumericalAnalysis.Angle"/> (a.k.a. the argument) in radians of this <see cref="ComplexNumber"/>.
        /// </summary>
        public Angle Angle
        {
            get
            {
                if (m_rectangularValues.AllAssigned)
                {
                    // Complex number is internally represented in rectangluar coordinates, so we return calculated angle
                    return Math.Atan2(Imaginary, Real);
                }
                else if (m_polarValues != null)
                {
                    // Return any assigned value if angle can't be calculated
                    return m_polarValues[AngleComponent].GetValueOrDefault();
                }
                else
                    return double.NaN;
            }
            set
            {
                if (m_rectangularValues.NoneAssigned)
                {
                    // Complex number is internally represented in rectangluar coordinates but these values have yet to be
                    // assigned so we cache angle so we can calculate the real and imaginary components once we also
                    // receive the magnitude value

                    // Create a compound value for polar coordinates if it hasn't been created yet (only created if needed)
                    if (m_polarValues == null)
                        m_polarValues = new CompoundValue<double>(2);

                    // Assign angle component of compound value
                    m_polarValues[AngleComponent] = value;

                    // If all composite polar values have been received, we can calculate real and imaginary values
                    CalculateRectangularFromPolar();
                }
                else
                {
                    // Rectangular values have already been assigned, user is simply requesting to change complex number
                    // by updating its angle value so we calculate a new complex number based on the updated polar
                    // coordinates and then update the real and imaginary components
                    ComplexNumber updatedValue = new ComplexNumber(value, AbsoluteValue);

                    m_rectangularValues[RealComponent] = updatedValue.Real;
                    m_rectangularValues[ImaginaryComponent] = updatedValue.Imaginary;
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
        public bool CompositesAssigned
        {
            get
            {
                return m_rectangularValues.AllAssigned;
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
            if (m_polarValues.AllAssigned)
            {
                // All values assigned, calculate a new rectangular based complex number from its polar composite values
                double angle = (double)m_polarValues[AngleComponent];
                double magnitude = (double)m_polarValues[MagnitudeComponent];

                Real = magnitude * Math.Cos(angle);
                Imaginary = magnitude * Math.Sin(angle);

                // Once rectangular values are available, polar values are no longer needed
                m_polarValues = null;
            }
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
            return (value1.Real == value2.Real && value1.Imaginary == value2.Imaginary);
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
            return new ComplexNumber(-z.Real, -z.Imaginary);
        }

        /// <summary>
        /// Returns computed sum of values.
        /// </summary>
        public static ComplexNumber operator +(ComplexNumber value1, ComplexNumber value2)
        {
            return new ComplexNumber(value1.Real + value2.Real, value1.Imaginary + value2.Imaginary);
        }

        /// <summary>
        /// Returns computed difference of values.
        /// </summary>
        public static ComplexNumber operator -(ComplexNumber value1, ComplexNumber value2)
        {
            return new ComplexNumber(value1.Real - value2.Real, value1.Imaginary - value2.Imaginary);
        }

        /// <summary>
        /// Returns computed product of values.
        /// </summary>
        public static ComplexNumber operator *(ComplexNumber value1, ComplexNumber value2)
        {
            double real = value1.Real * value2.Real - value1.Imaginary * value2.Imaginary;
            double imaginary = value1.Imaginary * value2.Real + value1.Real * value2.Imaginary;

            return new ComplexNumber(real, imaginary);
        }

        /// <summary>
        /// Returns computed division of values.
        /// </summary>
        public static ComplexNumber operator /(ComplexNumber value1, ComplexNumber value2)
        {
            double divisor = Math.Pow(value2.Real, 2) + Math.Pow(value2.Imaginary, 2);
            double real = (value1.Real * value2.Real + value1.Imaginary * value2.Imaginary) / divisor;
            double imaginary = (value1.Imaginary * value2.Real - value1.Real * value2.Imaginary) / divisor;

            return new ComplexNumber(real, imaginary);
        }

        #endregion
   }
}