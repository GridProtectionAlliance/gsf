//*******************************************************************************************************
//  IChannelValue.cs
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

using System.Runtime.Serialization;
using PCS.Measurements;

namespace PCS.PhasorProtocols
{
    /// <summary>
    /// Represents a protocol independent interface representation any kind of <see cref="IChannel"/> data value.
    /// </summary>
    /// <remarks>
    /// Each instance of <see cref="IChannelValue{T}"/> will have a more specific derived implementation (e.g., <see cref="IDigitalValue"/> and <see cref="IPhasorValue"/>),
    /// these specific implementations of <see cref="IChannelValue{T}"/> will be referenced children of a <see cref="IDataCell"/>.<br/>
    /// The <see cref="IChannelValue{T}"/> uses the specified <see cref="IChannelDefinition"/> type to define its properties.
    /// </remarks>
    /// <typeparam name="T">Specific <see cref="IChannelDefinition"/> type that represents the <see cref="IChannelValue{T}"/> definition.</typeparam>
    public interface IChannelValue<T> : IChannel, ISerializable where T : IChannelDefinition
    {
        /// <summary>
        /// Gets the <see cref="IDataCell"/> parent of this <see cref="IChannelValue{T}"/>.
        /// </summary>
        IDataCell Parent { get; set; }

        /// <summary>
        /// Gets the <see cref="IChannelDefinition"/> associated with this <see cref="IChannelValue{T}"/>.
        /// </summary>
        T Definition { get; set; }

        /// <summary>
        /// Gets the <see cref="PhasorProtocols.DataFormat"/> of this <see cref="IChannelValue{T}"/> typically derived from <see cref="IChannelDefinition.DataFormat"/>.
        /// </summary>
        DataFormat DataFormat { get; }

        /// <summary>
        /// Gets text based label of this <see cref="IChannelValue{T}"/> typically derived from <see cref="IChannelDefinition.Label"/>.
        /// </summary>
        string Label { get; }

        /// <summary>
        /// Gets boolean value that determines if none of the composite values of <see cref="IChannelValue{T}"/> have been assigned a value.
        /// </summary>
        /// <returns>True, if no composite values have been assigned a value; otherwise, false.</returns>
        bool IsEmpty { get; }

        /// <summary>
        /// Gets the composite values of this <see cref="IChannelValue{T}"/> as an array of <see cref="IMeasurement"/> values.
        /// </summary>
        IMeasurement[] Measurements { get; }

        /// <summary>
        /// Gets total number of composite values that this <see cref="IChannelValue{T}"/> provides.
        /// </summary>
        int CompositeValueCount { get; }

        /// <summary>
        /// Gets the specified composite value of this <see cref="IChannelValue{T}"/>.
        /// </summary>
        /// <param name="index">Index of composite value to retrieve.</param>
        double GetCompositeValue(int index);
    }
}