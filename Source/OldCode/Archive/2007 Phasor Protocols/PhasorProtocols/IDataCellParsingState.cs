//*******************************************************************************************************
//  IDataCellParsingState.cs
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
//  04/16/2005 - James R Carroll
//       Generated original version of source code.
//
//*******************************************************************************************************

namespace TVA.PhasorProtocols
{
    /// <summary>
    /// Defines function signature for creating new <see cref="IChannelValue{T}"/> objects.
    /// </summary>
    /// <param name="parent">Reference to parent <see cref="IDataCell"/>.</param>
    /// <param name="definition">Refrence to associated <see cref="IChannelDefinition"/> object.</param>
    /// <param name="binaryImage">Binary image to parse <see cref="IChannelValue{T}"/> from.</param>
    /// <param name="startIndex">Start index into <paramref name="binaryImage"/> to begin parsing.</param>
    /// <param name="parsedLength">Returns the total number of bytes parsed from <paramref name="binaryImage"/>.</param>
    /// <returns>New <see cref="IChannelValue{T}"/> object.</returns>
    /// <typeparam name="TDefinition">Specific <see cref="IChannelDefinition"/> type that the <see cref="IChannelValue{TDefinition}"/> references.</typeparam>
    /// <typeparam name="TValue">Specific <see cref="IChannelValue{TDefinition}"/> type that the <see cref="CreateNewValueFunction{TDefinition,TValue}"/> creates.</typeparam>
    public delegate TValue CreateNewValueFunction<TDefinition, TValue>(IDataCell parent, TDefinition definition, byte[] binaryImage, int startIndex, out int parsedLength)
        where TDefinition : IChannelDefinition
        where TValue : IChannelValue<TDefinition>;

    /// <summary>
    /// Represents a protocol independent interface representation of the parsing state of any kind of <see cref="IDataCell"/>.
    /// </summary>
    public interface IDataCellParsingState : IChannelCellParsingState
    {
        /// <summary>
        /// Gets reference to the <see cref="IConfigurationCell"/> associated with the <see cref="IDataCell"/> being parsed.
        /// </summary>
        IConfigurationCell ConfigurationCell { get; }

        /// <summary>
        /// Gets reference to <see cref="CreateNewValueFunction{TDefinition,TValue}"/> delegate used to create new <see cref="IPhasorValue"/> objects.
        /// </summary>
        CreateNewValueFunction<IPhasorDefinition, IPhasorValue> CreateNewPhasorValue { get; }

        /// <summary>
        /// Gets reference to <see cref="CreateNewValueFunction{TDefinition,TValue}"/> delegate used to create new <see cref="IFrequencyValue"/> objects.
        /// </summary>
        CreateNewValueFunction<IFrequencyDefinition, IFrequencyValue> CreateNewFrequencyValue { get; }

        /// <summary>
        /// Gets reference to <see cref="CreateNewValueFunction{TDefinition,TValue}"/> delegate used to create new <see cref="IAnalogValue"/> objects.
        /// </summary>
        CreateNewValueFunction<IAnalogDefinition, IAnalogValue> CreateNewAnalogValue { get; }

        /// <summary>
        /// Gets reference to <see cref="CreateNewValueFunction{TDefinition,TValue}"/> delegate used to create new <see cref="IDigitalValue"/> objects.
        /// </summary>
        CreateNewValueFunction<IDigitalDefinition, IDigitalValue> CreateNewDigitalValue { get; }
    }
}