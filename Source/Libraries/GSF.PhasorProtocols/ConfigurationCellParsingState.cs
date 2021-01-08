//******************************************************************************************************
//  ConfigurationCellParsingState.cs - Gbtc
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
//  01/14/2005 - J. Ritchie Carroll
//       Generated original version of source code.
//  09/15/2009 - Stephen C. Wills
//       Added new header and license agreement.
//  10/5/2012 - Gavin E. Holden
//       Added new header and license agreement.
//  12/17/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

namespace GSF.PhasorProtocols
{
    /// <summary>
    /// Represents the protocol independent common implementation of the parsing state used by any <see cref="IConfigurationCell"/>.
    /// </summary>
    public class ConfigurationCellParsingState : ChannelCellParsingStateBase, IConfigurationCellParsingState
    {
        #region [ Members ]

        // Fields

    #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="ConfigurationCellParsingState"/> from specified parameters.
        /// </summary>
        /// <param name="createNewPhasorDefinition">Reference to delegate to create new <see cref="IPhasorDefinition"/> instances.</param>
        /// <param name="createNewFrequencyDefinition">Reference to delegate to create new <see cref="IFrequencyDefinition"/> instances.</param>
        /// <param name="createNewAnalogDefinition">Reference to delegate to create new <see cref="IAnalogDefinition"/> instances.</param>
        /// <param name="createNewDigitalDefinition">Reference to delegate to create new <see cref="IDigitalDefinition"/> instances.</param>
        public ConfigurationCellParsingState(CreateNewDefinitionFunction<IPhasorDefinition> createNewPhasorDefinition, CreateNewDefinitionFunction<IFrequencyDefinition> createNewFrequencyDefinition, CreateNewDefinitionFunction<IAnalogDefinition> createNewAnalogDefinition, CreateNewDefinitionFunction<IDigitalDefinition> createNewDigitalDefinition)
        {
            CreateNewPhasorDefinition = createNewPhasorDefinition;
            CreateNewFrequencyDefinition = createNewFrequencyDefinition;
            CreateNewAnalogDefinition = createNewAnalogDefinition;
            CreateNewDigitalDefinition = createNewDigitalDefinition;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets reference to <see cref="CreateNewDefinitionFunction{T}"/> delegate used to create new <see cref="IPhasorDefinition"/> objects.
        /// </summary>
        public virtual CreateNewDefinitionFunction<IPhasorDefinition> CreateNewPhasorDefinition { get; }

        /// <summary>
        /// Gets reference to <see cref="CreateNewDefinitionFunction{T}"/> delegate used to create new <see cref="IFrequencyDefinition"/> objects.
        /// </summary>
        public virtual CreateNewDefinitionFunction<IFrequencyDefinition> CreateNewFrequencyDefinition { get; }

        /// <summary>
        /// Gets reference to <see cref="CreateNewDefinitionFunction{T}"/> delegate used to create new <see cref="IAnalogDefinition"/> objects.
        /// </summary>
        public virtual CreateNewDefinitionFunction<IAnalogDefinition> CreateNewAnalogDefinition { get; }

        /// <summary>
        /// Gets reference to <see cref="CreateNewDefinitionFunction{T}"/> delegate used to create new <see cref="IDigitalDefinition"/> objects.
        /// </summary>
        public virtual CreateNewDefinitionFunction<IDigitalDefinition> CreateNewDigitalDefinition { get; }

    #endregion
    }
}