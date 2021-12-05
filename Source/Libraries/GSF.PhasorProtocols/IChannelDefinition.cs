//******************************************************************************************************
//  IChannelDefinition.cs - Gbtc
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
//  02/18/2005 - J. Ritchie Carroll
//       Generated original version of source code.
//  09/15/2009 - Stephen C. Wills
//       Added new header and license agreement.
//  10/5/2012 - Gavin E. Holden
//       Added new header and license agreement.
//  12/17/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Runtime.Serialization;

namespace GSF.PhasorProtocols
{
    #region [ Enumerations ]

    /// <summary>
    /// Data transmission formats enumeration.
    /// </summary>
    [Serializable]
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