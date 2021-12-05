//******************************************************************************************************
//  IDataCellParsingState.cs - Gbtc
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
//  04/16/2005 - J. Ritchie Carroll
//       Generated original version of source code.
//  09/15/2009 - Stephen C. Wills
//       Added new header and license agreement.
//  12/17/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

namespace GSF.PhasorProtocols
{
    /// <summary>
    /// Defines function signature for creating new <see cref="IChannelValue{T}"/> objects.
    /// </summary>
    /// <param name="parent">Reference to parent <see cref="IDataCell"/>.</param>
    /// <param name="definition">Reference to associated <see cref="IChannelDefinition"/> object.</param>
    /// <param name="buffer">Binary image to parse <see cref="IChannelValue{T}"/> from.</param>
    /// <param name="startIndex">Start index into <paramref name="buffer"/> to begin parsing.</param>
    /// <param name="parsedLength">Returns the total number of bytes parsed from <paramref name="buffer"/>.</param>
    /// <returns>New <see cref="IChannelValue{T}"/> object.</returns>
    /// <typeparam name="TDefinition">Specific <see cref="IChannelDefinition"/> type that the <see cref="IChannelValue{TDefinition}"/> references.</typeparam>
    /// <typeparam name="TValue">Specific <see cref="IChannelValue{TDefinition}"/> type that the <see cref="CreateNewValueFunction{TDefinition,TValue}"/> creates.</typeparam>
    public delegate TValue CreateNewValueFunction<TDefinition, TValue>(IDataCell parent, TDefinition definition, byte[] buffer, int startIndex, out int parsedLength)
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