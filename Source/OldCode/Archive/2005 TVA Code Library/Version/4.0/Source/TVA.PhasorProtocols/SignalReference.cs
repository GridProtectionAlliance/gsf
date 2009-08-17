//*******************************************************************************************************
//  SignalReference.cs
//  Copyright © 2009 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: James R. Carroll
//      Office: PSO TRAN & REL, CHATTANOOGA - MR BK-C
//       Phone: 423/751-4165
//       Email: jrcarrol@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  05/05/2009 - James R. Carroll
//       Generated original version of source code.
//
//*******************************************************************************************************

using System;

namespace TVA.PhasorProtocols
{
    #region [ Enumerations ]

    /// <summary>
    /// Signal type enumeration.
    /// </summary>
    [Serializable()]
    public enum SignalType
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
        /// Gets or sets the <see cref="SignalType"/> of this <see cref="SignalReference"/>.
        /// </summary>
        public SignalType Type;

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
                    Type = SignalReference.GetSignalType(signalType.Substring(0, 2));
                    
                    if (Type != SignalType.Unknown)
                        Index = int.Parse(signalType.Substring(2));
                }
                else
                    Type = SignalReference.GetSignalType(signalType);
            }
            else
            {
                // This represents an error - best we can do is assume entire string is the acronym
                Acronym = signal.Trim().ToUpper();
                Type = SignalType.Unknown;
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
            return SignalReference.ToString(Acronym, Type, Index);
        }

        /// <summary>
        /// Returns the hash code for this <see cref="SignalReference"/>.
        /// </summary>
        /// <returns>A 32-bit signed integer hash code.</returns>
        public override int GetHashCode()
        {
            return SignalReference.ToString(Acronym, Type, Index).GetHashCode();
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
            return (string.Compare(Acronym, other.Acronym, true) == 0 && Type == other.Type && Index == other.Index);
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
                int signalTypeCompare = Type < other.Type ? -1 : (Type > other.Type ? 1 : 0);

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
        /// Gets the <see cref="SignalType"/> for the specified <paramref name="acronym"/>.
        /// </summary>
        /// <param name="acronym">Acronym of the desired <see cref="SignalType"/>.</param>
        /// <returns>The <see cref="SignalType"/> for the specified <paramref name="acronym"/>.</returns>
        public static SignalType GetSignalType(string acronym)
        {
            switch (acronym)
            {
                case "PA": // Phase Angle
                    return SignalType.Angle;
                case "PM": // Phase Magnitude
                    return SignalType.Magnitude;
                case "FQ": // Frequency
                    return SignalType.Frequency;
                case "DF": // dF/dt
                    return SignalType.DfDt;
                case "SF": // Status Flags
                    return SignalType.Status;
                case "DV": // Digital Value
                    return SignalType.Digital;
                case "AV": // Analog Value
                    return SignalType.Analog;
                case "CV": // Calculated Value
                    return SignalType.Calculation;
                default:
                    return SignalType.Unknown;
            }
        }

        /// <summary>
        /// Gets the acronym for the specified <see cref="SignalType"/>.
        /// </summary>
        /// <param name="signal"><see cref="SignalType"/> to convert to an acronym.</param>
        /// <returns>The acronym for the specified <see cref="SignalType"/>.</returns>
        public static string GetSignalTypeAcronym(SignalType signal)
        {
            switch (signal)
            {
                case SignalType.Angle:
                    return "PA"; // Phase Angle
                case SignalType.Magnitude:
                    return "PM"; // Phase Magnitude
                case SignalType.Frequency:
                    return "FQ"; // Frequency
                case SignalType.DfDt:
                    return "DF"; // dF/dt
                case SignalType.Status:
                    return "SF"; // Status Flags
                case SignalType.Digital:
                    return "DV"; // Digital Value
                case SignalType.Analog:
                    return "AV"; // Analog Value
                case SignalType.Calculation:
                    return "CV"; // Calculated Value
                default:
                    return "?";
            }
        }

        /// <summary>
        /// Returns a <see cref="string"/> that represents the specified <paramref name="acronym"/> and <see cref="SignalType"/>.
        /// </summary>
        /// <param name="acronym">Acronym portion of the desired <see cref="string"/> representation.</param>
        /// <param name="type"><see cref="SignalType"/> portion of the desired <see cref="string"/> representation.</param>
        /// <returns>A <see cref="string"/> that represents the specified <paramref name="acronym"/> and <see cref="SignalType"/>.</returns>
        public static string ToString(string acronym, SignalType type)
        {
            return ToString(acronym, type, 0);
        }

        /// <summary>
        /// Returns a <see cref="string"/> that represents the specified <paramref name="acronym"/>, <see cref="SignalType"/> and <paramref name="signalIndex"/>.
        /// </summary>
        /// <param name="acronym">Acronym portion of the desired <see cref="string"/> representation.</param>
        /// <param name="type"><see cref="SignalType"/> portion of the desired <see cref="string"/> representation.</param>
        /// <param name="index">Index of <see cref="SignalType"/> portion of the desired <see cref="string"/> representation.</param>
        /// <returns>A <see cref="string"/> that represents the specified <paramref name="acronym"/>, <see cref="SignalType"/> and <paramref name="signalIndex"/>.</returns>
        public static string ToString(string acronym, SignalType type, int index)
        {
            if (index > 0)
                return string.Format("{0}-{1}{2}", acronym, GetSignalTypeAcronym(type), index);
            else
                return string.Format("{0}-{1}", acronym, GetSignalTypeAcronym(type));
        }

        #endregion
	}	
}