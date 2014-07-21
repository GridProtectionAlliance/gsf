//******************************************************************************************************
//  Phasor.cs - Gbtc
//
//  Copyright © 2010, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the Eclipse Public License -v 1.0 (the "License"); you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://www.opensource.org/licenses/eclipse-1.0.php
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  07/17/2014 - Ritchie
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

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
        public PhasorType Type;

        /// <summary>
        /// Phasor value.
        /// </summary>
        public ComplexNumber Value;

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
        public bool Equals(Phasor obj)
        {
            return this == obj;
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>
        /// A 32-bit signed integer hash code.
        /// </returns>
        public override int GetHashCode()
        {
            return Value.GetHashCode() ^ Type.GetHashCode();
        }

        /// <summary>
        /// Converts the numeric value of this instance to its equivalent string representation.
        /// </summary>
        /// <returns>
        /// The string representation of the value of this <see cref="ComplexNumber"/> instance.
        /// </returns>
        public override string ToString()
        {
            return string.Format("{0}:{1}", Type, Value);
        }

        #endregion

        #region [ Operators ]

        /// <summary>
        /// Implicitly converts a <see cref="Phasor"/> to a <see cref="ComplexNumber"/>.
        /// </summary>
        /// <param name="phasor">Operand.</param>
        /// <returns>ComplexNumber representing the result of the operation.</returns>
        public static implicit operator ComplexNumber(Phasor phasor)
        {
            return phasor.Value;
        }

        /// <summary>
        /// Compares the two values for equality.
        /// </summary>
        /// <param name="phasor1">Left hand operand.</param>
        /// <param name="phasor2">Right hand operand.</param>
        /// <returns>Boolean representing the result of the addition operation.</returns>
        public static bool operator ==(Phasor phasor1, Phasor phasor2)
        {
            return (phasor1.Type == phasor2.Type && phasor1.Value == phasor2.Value);
        }

        /// <summary>
        /// Compares the two values for inequality.
        /// </summary>
        /// <param name="phasor1">Left hand operand.</param>
        /// <param name="phasor2">Right hand operand.</param>
        /// <returns>Boolean representing the result of the inequality operation.</returns>
        public static bool operator !=(Phasor phasor1, Phasor phasor2)
        {
            return !(phasor1 == phasor2);
        }

        /// <summary>
        /// Returns the negated value.
        /// </summary>
        /// <param name="z">Left hand operand.</param>
        /// <returns>Phasor representing the result of the unary negation operation.</returns>
        public static Phasor operator -(Phasor z)
        {
            return new Phasor(z.Type, -z.Value);
        }

        /// <summary>
        /// Returns computed sum of values.
        /// </summary>
        /// <param name="phasor1">Left hand operand.</param>
        /// <param name="phasor2">Right hand operand.</param>
        /// <returns>ComplexNumber representing the result of the addition operation.</returns>
        /// <remarks>Resultant phasor will have <see cref="Type"/> of left hand operand, <paramref name="phasor1"/>.</remarks>
        public static Phasor operator +(Phasor phasor1, Phasor phasor2)
        {
            return new Phasor(phasor1.Type, phasor1.Value + phasor2.Value);
        }

        /// <summary>
        /// Returns computed difference of values.
        /// </summary>
        /// <param name="phasor1">Left hand operand.</param>
        /// <param name="phasor2">Right hand operand.</param>
        /// <returns>ComplexNumber representing the result of the subtraction operation.</returns>
        /// <remarks>Resultant phasor will have <see cref="Type"/> of left hand operand, <paramref name="phasor1"/>.</remarks>
        public static Phasor operator -(Phasor phasor1, Phasor phasor2)
        {
            return new Phasor(phasor1.Type, phasor1.Value - phasor2.Value);
        }

        /// <summary>
        /// Returns computed product of values.
        /// </summary>
        /// <param name="phasor1">Left hand operand.</param>
        /// <param name="phasor2">Right hand operand.</param>
        /// <returns>ComplexNumber representing the result of the multiplication operation.</returns>
        /// <remarks>Resultant phasor will have <see cref="Type"/> of left hand operand, <paramref name="phasor1"/>.</remarks>
        public static Phasor operator *(Phasor phasor1, Phasor phasor2)
        {
            return new Phasor(phasor1.Type, phasor1.Value * phasor2.Value);
        }

        /// <summary>
        /// Returns computed division of values.
        /// </summary>
        /// <param name="phasor1">Left hand operand.</param>
        /// <param name="phasor2">Right hand operand.</param>
        /// <returns>ComplexNumber representing the result of the division operation.</returns>
        /// <remarks>Resultant phasor will have <see cref="Type"/> of left hand operand, <paramref name="phasor1"/>.</remarks>
        public static Phasor operator /(Phasor phasor1, Phasor phasor2)
        {
            return new Phasor(phasor1.Type, phasor1.Value / phasor2.Value);
        }

        ///<summary>
        /// Returns specified <see cref="Phasor"/> raised to the specified power.
        ///</summary>
        ///<param name="z">Phasor to be raised to power <paramref name="y"/>.</param>
        ///<param name="y">Power to raise <see cref="Phasor"/> <paramref name="z"/>.</param>
        /// <returns>Phasor representing the result of the operation.</returns>
        public static Phasor Pow(Phasor z, double y)
        {
            return new Phasor(z.Type, ComplexNumber.Pow(z.Value, y));
        }

        // C# doesn't expose an exponent operator but some other .NET languages do,
        // so we expose the operator via its native special IL function name

        /// <summary>
        /// Returns result of first value raised to power of second value.
        /// </summary>
        ///<param name="z">Phasor to be raised to power <paramref name="y"/>.</param>
        ///<param name="y">Power to raise <see cref="Phasor"/> <paramref name="z"/>.</param>
        /// <returns>Phasor representing the result of the operation.</returns>
        [EditorBrowsable(EditorBrowsableState.Advanced), SpecialName]
        public static Phasor op_Exponent(Phasor z, double y)
        {
            return Pow(z, y);
        }

        #endregion
    }
}
