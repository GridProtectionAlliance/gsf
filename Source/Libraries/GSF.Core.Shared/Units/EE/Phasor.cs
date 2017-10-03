//******************************************************************************************************
//  Phasor.cs - Gbtc
//
//  Copyright © 2010, Grid Protection Alliance.  All Rights Reserved.
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
//  07/17/2014 - J. Ritchie Carroll
//       Generated original version of source code.
//  12/02/2015 - J. Ritchie Carroll
//       Added common power calculation functions and implicit interaction with .NET Complex type.
//
//******************************************************************************************************

using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

#if DNF46
using System.Numerics;
#endif

namespace GSF.Units.EE
{
    /// <summary>
    /// Represents a phasor as a complex number value and a type (i.e., a voltage or a current).
    /// </summary>
    public struct Phasor : IEquatable<Phasor>
    {
        #region [ Members ]

        // Fields

        /// <summary>
        /// Phasor type.
        /// </summary>
        public readonly PhasorType Type;

        /// <summary>
        /// Phasor value.
        /// </summary>
        public readonly ComplexNumber Value;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a <see cref="Phasor"/> of the specified <paramref name="type"/> from the given rectangular values. 
        /// </summary>
        /// <param name="type">Type of phasor, i.e., current or voltage.</param>
        /// <param name="real">The real component of the <see cref="ComplexNumber"/>.</param>
        /// <param name="imaginary">The imaginary component of the <see cref="ComplexNumber"/>.</param>
        public Phasor(PhasorType type, double real, double imaginary)
            : this()
        {
            Type = type;
            Value = new ComplexNumber(real, imaginary);
        }

        /// <summary>
        /// Creates a <see cref="Phasor"/> of the specified <paramref name="type"/> from the given polar values.
        /// </summary>
        /// <param name="type">Type of phasor, i.e., current or voltage.</param>
        /// <param name="angle">The <see cref="Angle"/> component, in radians, of the <see cref="ComplexNumber"/>.</param>
        /// <param name="magnitude">The magnitude (or absolute value) component of the <see cref="ComplexNumber"/>.</param>
        public Phasor(PhasorType type, Angle angle, double magnitude)
            : this()
        {
            Type = type;
            Value = new ComplexNumber(angle, magnitude);
        }

        /// <summary>
        /// Creates a <see cref="Phasor"/> of the specified <paramref name="type"/> from the given <see cref="ComplexNumber"/>.
        /// </summary>
        /// <param name="type">Type of phasor, i.e., current or voltage.</param>
        /// <param name="z"><see cref="ComplexNumber"/> to be copied.</param>
        public Phasor(PhasorType type, ComplexNumber z)
            : this()
        {
            Type = type;
            Value = z;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets the complex conjugate of this <see cref="Phasor"/>.
        /// </summary>
        public ComplexNumber Conjugate => Value.Conjugate;

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Returns a value indicating whether this instance is equal to a specified object.
        /// </summary>
        /// <param name="obj">An object to compare, or null.</param>
        /// <returns>
        /// True if <paramref name="obj"/> is an instance of Phasor and equals the value of this instance;
        /// otherwise, False.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (obj is Phasor)
                return Equals((Phasor)obj);

            return false;
        }

        /// <summary>
        /// Returns a value indicating whether this instance is equal to a specified Phasor value.
        /// </summary>
        /// <param name="obj">A <see cref="Phasor"/> to compare to this instance.</param>
        /// <returns>
        /// True if <paramref name="obj"/> has the same value as this instance; otherwise, False.
        /// </returns>
        public bool Equals(Phasor obj) => this == obj;

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>
        /// A 32-bit signed integer hash code.
        /// </returns>
        public override int GetHashCode() => Value.GetHashCode() ^ Type.GetHashCode();

        /// <summary>
        /// Converts the numeric value of this instance to its equivalent string representation.
        /// </summary>
        /// <returns>
        /// The string representation of the value of this <see cref="ComplexNumber"/> instance.
        /// </returns>
        public override string ToString() => $"{Type}:{Value}";

        #endregion

        #region [ Operators ]

        /// <summary>
        /// Implicitly converts a <see cref="Phasor"/> to a <see cref="ComplexNumber"/>.
        /// </summary>
        /// <param name="phasor">Operand.</param>
        /// <returns>ComplexNumber representing the result of the operation.</returns>
        public static implicit operator ComplexNumber(Phasor phasor) => phasor.Value;

        #if DNF46

        /// <summary>
        /// Implicitly converts a <see cref="Phasor"/> to a .NET <see cref="Complex"/> value.
        /// </summary>
        /// <param name="phasor">Operand.</param>
        /// <returns>ComplexNumber representing the result of the operation.</returns>
        public static implicit operator Complex(Phasor phasor) => phasor.Value;

        #endif

        /// <summary>
        /// Compares the two values for equality.
        /// </summary>
        /// <param name="phasor1">Left hand operand.</param>
        /// <param name="phasor2">Right hand operand.</param>
        /// <returns>Boolean representing the result of the addition operation.</returns>
        public static bool operator ==(Phasor phasor1, Phasor phasor2) => phasor1.Type == phasor2.Type && phasor1.Value == phasor2.Value;

        /// <summary>
        /// Compares the two values for inequality.
        /// </summary>
        /// <param name="phasor1">Left hand operand.</param>
        /// <param name="phasor2">Right hand operand.</param>
        /// <returns>Boolean representing the result of the inequality operation.</returns>
        public static bool operator !=(Phasor phasor1, Phasor phasor2) => !(phasor1 == phasor2);

        /// <summary>
        /// Returns the negated value.
        /// </summary>
        /// <param name="z">Left hand operand.</param>
        /// <returns>Phasor representing the result of the unary negation operation.</returns>
        public static Phasor operator -(Phasor z) => new Phasor(z.Type, -z.Value);

        /// <summary>
        /// Returns computed sum of values.
        /// </summary>
        /// <param name="phasor1">Left hand operand.</param>
        /// <param name="phasor2">Right hand operand.</param>
        /// <returns>ComplexNumber representing the result of the addition operation.</returns>
        /// <remarks>Resultant phasor will have <see cref="Type"/> of left hand operand, <paramref name="phasor1"/>.</remarks>
        public static Phasor operator +(Phasor phasor1, Phasor phasor2) => new Phasor(phasor1.Type, phasor1.Value + phasor2.Value);

        /// <summary>
        /// Returns computed difference of values.
        /// </summary>
        /// <param name="phasor1">Left hand operand.</param>
        /// <param name="phasor2">Right hand operand.</param>
        /// <returns>ComplexNumber representing the result of the subtraction operation.</returns>
        /// <remarks>Resultant phasor will have <see cref="Type"/> of left hand operand, <paramref name="phasor1"/>.</remarks>
        public static Phasor operator -(Phasor phasor1, Phasor phasor2) => new Phasor(phasor1.Type, phasor1.Value - phasor2.Value);

        /// <summary>
        /// Returns computed product of values.
        /// </summary>
        /// <param name="phasor1">Left hand operand.</param>
        /// <param name="phasor2">Right hand operand.</param>
        /// <returns>ComplexNumber representing the result of the multiplication operation.</returns>
        /// <remarks>Resultant phasor will have <see cref="Type"/> of left hand operand, <paramref name="phasor1"/>.</remarks>
        public static Phasor operator *(Phasor phasor1, Phasor phasor2) => new Phasor(phasor1.Type, phasor1.Value * phasor2.Value);

        /// <summary>
        /// Returns computed division of values.
        /// </summary>
        /// <param name="phasor1">Left hand operand.</param>
        /// <param name="phasor2">Right hand operand.</param>
        /// <returns>ComplexNumber representing the result of the division operation.</returns>
        /// <remarks>Resultant phasor will have <see cref="Type"/> of left hand operand, <paramref name="phasor1"/>.</remarks>
        public static Phasor operator /(Phasor phasor1, Phasor phasor2) => new Phasor(phasor1.Type, phasor1.Value / phasor2.Value);

        ///<summary>
        /// Returns specified <see cref="Phasor"/> raised to the specified power.
        ///</summary>
        ///<param name="z">Phasor to be raised to power <paramref name="y"/>.</param>
        ///<param name="y">Power to raise <see cref="Phasor"/> <paramref name="z"/>.</param>
        /// <returns>Phasor representing the result of the operation.</returns>
        public static Phasor Pow(Phasor z, double y) => new Phasor(z.Type, ComplexNumber.Pow(z.Value, y));

        // C# doesn't expose an exponent operator but some other .NET languages do,
        // so we expose the operator via its native special IL function name

        /// <summary>
        /// Returns result of first value raised to power of second value.
        /// </summary>
        ///<param name="z">Phasor to be raised to power <paramref name="y"/>.</param>
        ///<param name="y">Power to raise <see cref="Phasor"/> <paramref name="z"/>.</param>
        /// <returns>Phasor representing the result of the operation.</returns>
        [EditorBrowsable(EditorBrowsableState.Advanced), SpecialName]
        public static Phasor op_Exponent(Phasor z, double y) => Pow(z, y);

        #endregion

        #region [ Static ]

        // Static Methods

        /// <summary>
        /// Calculates active (or real) power P, i.e., total watts, from imaginary and real components of a voltage and current phasor.
        /// </summary>
        /// <param name="voltage">Voltage phasor.</param>
        /// <param name="current">Current phasor.</param>
        /// <exception cref="ArgumentException"><paramref name="voltage"/> and <paramref name="current"/> must have proper <see cref="Type"/>.</exception>
        /// <returns>Calculated watts from imaginary and real components of specified <paramref name="voltage"/> and <paramref name="current"/> phasors.</returns>
        public static Power CalculateActivePower(Phasor voltage, Phasor current)
        {
            if (voltage.Type != PhasorType.Voltage)
                throw new ArgumentException("Provided voltage phasor is a current", nameof(voltage));

            if (current.Type != PhasorType.Current)
                throw new ArgumentException("Provided current phasor is a voltage", nameof(current));

            return voltage.Value.Real * current.Value.Real + voltage.Value.Imaginary * current.Value.Imaginary;

            // Polar version of calculation
            //return voltage.Value.Magnitude * current.Value.Magnitude * Math.Cos(CalculateRelativePhase(voltage, current));
        }

        /// <summary>
        /// Calculates reactive power Q, i.e., total volt-amperes of reactive power, from imaginary and real components of a voltage and current phasor.
        /// </summary>
        /// <param name="voltage">Voltage phasor.</param>
        /// <param name="current">Current phasor.</param>
        /// <exception cref="ArgumentException"><paramref name="voltage"/> and <paramref name="current"/> must have proper <see cref="Type"/>.</exception>
        /// <returns>Calculated vars from imaginary and real components of specified <paramref name="voltage"/> and <paramref name="current"/> phasors.</returns>
        public static Power CalculateReactivePower(Phasor voltage, Phasor current)
        {
            if (voltage.Type != PhasorType.Voltage)
                throw new ArgumentException("Provided voltage phasor is a current", nameof(voltage));

            if (current.Type != PhasorType.Current)
                throw new ArgumentException("Provided current phasor is a voltage", nameof(current));

            return voltage.Value.Imaginary * current.Value.Real - voltage.Value.Real * current.Value.Imaginary;

            // Polar version of calculation
            //return voltage.Value.Magnitude * current.Value.Magnitude * Math.Sin(CalculateRelativePhase(voltage, current));
        }

        /// <summary>
        /// Calculates complex power S, i.e., total volt-amperes power vector, from a voltage and current phasor.
        /// </summary>
        /// <param name="voltage">Voltage phasor.</param>
        /// <param name="current">Current phasor.</param>
        /// <exception cref="ArgumentException"><paramref name="voltage"/> and <paramref name="current"/> must have proper <see cref="Type"/>.</exception>
        /// <returns>Calculated complex volt-amperes from specified <paramref name="voltage"/> and <paramref name="current"/> phasors.</returns>
        public static ComplexNumber CalculateComplexPower(Phasor voltage, Phasor current)
        {
            if (voltage.Type != PhasorType.Voltage)
                throw new ArgumentException("Provided voltage phasor is a current", nameof(voltage));

            if (current.Type != PhasorType.Current)
                throw new ArgumentException("Provided current phasor is a voltage", nameof(current));

            return voltage.Value * current.Conjugate;
        }

        /// <summary>
        /// Calculates apparent power |S|, i.e., magnitude of complex power, from a voltage and current phasor.
        /// </summary>
        /// <param name="voltage">Voltage phasor.</param>
        /// <param name="current">Current phasor.</param>
        /// <exception cref="ArgumentException"><paramref name="voltage"/> and <paramref name="current"/> must have proper <see cref="Type"/>.</exception>
        /// <returns>Calculated complex volt-amperes magnitude from specified <paramref name="voltage"/> and <paramref name="current"/> phasors.</returns>
        public static Power CalculateApparentPower(Phasor voltage, Phasor current)
        {
            return CalculateComplexPower(voltage, current).Magnitude;
        }

        /// <summary>
        /// Calculates phase φ of voltage relative to current, i.e., angle difference between current and voltage phasor.
        /// </summary>
        /// <param name="voltage">Voltage phasor.</param>
        /// <param name="current">Current phasor.</param>
        /// <exception cref="ArgumentException"><paramref name="voltage"/> and <paramref name="current"/> must have proper <see cref="Type"/>.</exception>
        /// <returns>Calculated phase of specified <paramref name="voltage"/> phasor relative to specified <paramref name="current"/> phasor.</returns>
        public static Angle CalculateRelativePhase(Phasor voltage, Phasor current)
        {
            if (voltage.Type != PhasorType.Voltage)
                throw new ArgumentException("Provided voltage phasor is a current", nameof(voltage));

            if (current.Type != PhasorType.Current)
                throw new ArgumentException("Provided current phasor is a voltage", nameof(current));

            return voltage.Value.Angle - current.Value.Angle;
        }

        #endregion
    }
}
