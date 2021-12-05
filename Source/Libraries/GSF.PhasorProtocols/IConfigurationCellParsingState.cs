//******************************************************************************************************
//  IConfigurationCellParsingState.cs - Gbtc
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
    /// Defines function signature for creating new <see cref="IChannelDefinition"/> objects.
    /// </summary>
    /// <param name="parent">Reference to parent <see cref="IConfigurationCell"/>.</param>
    /// <param name="buffer">Binary image to parse <see cref="IChannelDefinition"/> from.</param>
    /// <param name="startIndex">Start index into <paramref name="buffer"/> to begin parsing.</param>
    /// <param name="parsedLength">Returns the total number of bytes parsed from <paramref name="buffer"/>.</param>
    /// <returns>New <see cref="IChannelDefinition"/> object.</returns>
    /// <typeparam name="T">Specific <see cref="IChannelDefinition"/> type of object that the <see cref="CreateNewDefinitionFunction{T}"/> creates.</typeparam>
    public delegate T CreateNewDefinitionFunction<T>(IConfigurationCell parent, byte[] buffer, int startIndex, out int parsedLength)
        where T : IChannelDefinition;

    /// <summary>
    /// Represents a protocol independent interface representation of the parsing state of any kind of <see cref="IConfigurationCell"/>.
    /// </summary>
    public interface IConfigurationCellParsingState : IChannelCellParsingState
    {
        /// <summary>
        /// Gets reference to <see cref="CreateNewDefinitionFunction{T}"/> delegate used to create new <see cref="IPhasorDefinition"/> objects.
        /// </summary>
        CreateNewDefinitionFunction<IPhasorDefinition> CreateNewPhasorDefinition { get; }

        /// <summary>
        /// Gets reference to <see cref="CreateNewDefinitionFunction{T}"/> delegate used to create new <see cref="IFrequencyDefinition"/> objects.
        /// </summary>
        CreateNewDefinitionFunction<IFrequencyDefinition> CreateNewFrequencyDefinition { get; }

        /// <summary>
        /// Gets reference to <see cref="CreateNewDefinitionFunction{T}"/> delegate used to create new <see cref="IAnalogDefinition"/> objects.
        /// </summary>
        CreateNewDefinitionFunction<IAnalogDefinition> CreateNewAnalogDefinition { get; }

        /// <summary>
        /// Gets reference to <see cref="CreateNewDefinitionFunction{T}"/> delegate used to create new <see cref="IDigitalDefinition"/> objects.
        /// </summary>
        CreateNewDefinitionFunction<IDigitalDefinition> CreateNewDigitalDefinition { get; }
    }
}