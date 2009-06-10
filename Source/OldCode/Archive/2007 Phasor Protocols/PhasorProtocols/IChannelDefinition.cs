//*******************************************************************************************************
//  IChannelDefinition.cs
//  Copyright © 2009 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: James R Carroll
//      Office: PSO TRAN & REL, CHATTANOOGA - MR BK-C
//       Phone: 423/751-4165
//       Email: jrcarrol@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  02/18/2005 - James R Carroll
//       Generated original version of source code.
//
//*******************************************************************************************************

using System;
using System.Runtime.Serialization;

namespace PhasorProtocols
{
    #region [ Enumerations ]

    /// <summary>
    /// Data transmission formats enumeration.
    /// </summary>
    [Serializable()]
    public enum DataFormat : byte
    {
        /// <summary>
        /// Fixed integer data transmission format.
        /// </summary>
        FixedInteger,
        /// <summary>
        /// Floating point data transmission format.
        /// </summary>
        FloatingPoint
    }

    #endregion

    /// <summary>
    /// Represents a protocol independent interface representation of a definition of any kind of <see cref="IChannel"/> data.
    /// </summary>
    /// <remarks>
    /// Each instance of <see cref="IChannelDefinition"/> will have a more specific derived implementation
    /// (e.g., <see cref="IAnalogDefinition"/> and <see cref="IFrequencyDefinition"/>), these specific implementations of
    /// <see cref="IChannelDefinition"/> will be referenced children of a <see cref="IConfigurationCell"/>.<br/>
    /// The <see cref="IChannelDefinition"/> defines the properties of an associated <see cref="IChannelValue{T}"/>.
    /// </remarks>
    public interface IChannelDefinition : IChannel, ISerializable, IEquatable<IChannelDefinition>, IComparable<IChannelDefinition>, IComparable
    {
        /// <summary>
        /// Gets the <see cref="IConfigurationCell"/> parent of this <see cref="IChannelDefinition"/>.
        /// </summary>
        IConfigurationCell Parent { get; set; }
        
        /// <summary>
        /// Gets the <see cref="PhasorProtocols.DataFormat"/> of this <see cref="IChannelDefinition"/>.
        /// </summary>
        DataFormat DataFormat { get; }

        /// <summary>
        /// Gets or sets the index of this <see cref="IChannelDefinition"/>.
        /// </summary>
        int Index { get; set; }

        /// <summary>
        /// Gets or sets the offset of this <see cref="IChannelDefinition"/>.
        /// </summary>
        double Offset { get; set; }

        /// <summary>
        /// Gets or sets the integer scaling value of this <see cref="IChannelDefinition"/>.
        /// </summary>
        uint ScalingValue { get; set; }

        /// <summary>
        /// Gets the maximum value for the <see cref="ScalingValue"/> of this <see cref="IChannelDefinition"/>.
        /// </summary>
        uint MaximumScalingValue { get; }

        /// <summary>
        /// Gets or sets the conversion factor of this <see cref="IChannelDefinition"/>.
        /// </summary>
        double ConversionFactor { get; set; }

        /// <summary>
        /// Gets the scale/bit value of this <see cref="IChannelDefinition"/>.
        /// </summary>
        double ScalePerBit { get; }

        /// <summary>
        /// Gets or sets the label of this <see cref="IChannelDefinition"/>.
        /// </summary>
        string Label { get; set; }

        /// <summary>
        /// Gets the binary image of the <see cref="Label"/> of this <see cref="IChannelDefinition"/>.
        /// </summary>
        byte[] LabelImage { get; }

        /// <summary>
        /// Gets the maximum length of the <see cref="Label"/> of this <see cref="IChannelDefinition"/>.
        /// </summary>
        int MaximumLabelLength { get; }
    }
}