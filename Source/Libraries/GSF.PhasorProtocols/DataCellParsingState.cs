//******************************************************************************************************
//  DataCellParsingState.cs - Gbtc
//
//  Copyright © 2012, Grid Protection Alliance.  All Rights Reserved.
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
    /// Represents the protocol independent common implementation of the parsing state used by any <see cref="IDataCell"/>.
    /// </summary>
    public class DataCellParsingState : ChannelCellParsingStateBase, IDataCellParsingState
    {
        #region [ Members ]

        // Fields
        private readonly IConfigurationCell m_configurationCell;
        private readonly CreateNewValueFunction<IPhasorDefinition, IPhasorValue> m_createNewPhasorValue;
        private readonly CreateNewValueFunction<IFrequencyDefinition, IFrequencyValue> m_createNewFrequencyValue;
        private readonly CreateNewValueFunction<IAnalogDefinition, IAnalogValue> m_createNewAnalogValue;
        private readonly CreateNewValueFunction<IDigitalDefinition, IDigitalValue> m_createNewDigitalValue;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="DataCellParsingState"/> from specified parameters.
        /// </summary>
        /// <param name="configurationCell">Reference to the <see cref="IConfigurationCell"/> associated with the <see cref="IDataCell"/> being parsed.</param>
        /// <param name="createNewPhasorValue">Reference to delegate to create new <see cref="IPhasorValue"/> instances.</param>
        /// <param name="createNewFrequencyValue">Reference to delegate to create new <see cref="IFrequencyValue"/> instances.</param>
        /// <param name="createNewAnalogValue">Reference to delegate to create new <see cref="IAnalogValue"/> instances.</param>
        /// <param name="createNewDigitalValue">Reference to delegate to create new <see cref="IDigitalValue"/> instances.</param>
        public DataCellParsingState(IConfigurationCell configurationCell, CreateNewValueFunction<IPhasorDefinition, IPhasorValue> createNewPhasorValue, CreateNewValueFunction<IFrequencyDefinition, IFrequencyValue> createNewFrequencyValue, CreateNewValueFunction<IAnalogDefinition, IAnalogValue> createNewAnalogValue, CreateNewValueFunction<IDigitalDefinition, IDigitalValue> createNewDigitalValue)
        {
            m_configurationCell = configurationCell;
            m_createNewPhasorValue = createNewPhasorValue;
            m_createNewFrequencyValue = createNewFrequencyValue;
            m_createNewAnalogValue = createNewAnalogValue;
            m_createNewDigitalValue = createNewDigitalValue;

            if (m_configurationCell != null)
            {
                PhasorCount = m_configurationCell.PhasorDefinitions.Count;
                AnalogCount = m_configurationCell.AnalogDefinitions.Count;
                DigitalCount = m_configurationCell.DigitalDefinitions.Count;
            }
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets reference to the <see cref="IConfigurationCell"/> associated with the <see cref="IDataCell"/> being parsed.
        /// </summary>
        public virtual IConfigurationCell ConfigurationCell
        {
            get
            {
                return m_configurationCell;
            }
        }

        /// <summary>
        /// Gets reference to <see cref="CreateNewValueFunction{TDefinition,TValue}"/> delegate used to create new <see cref="IPhasorValue"/> objects.
        /// </summary>
        public virtual CreateNewValueFunction<IPhasorDefinition, IPhasorValue> CreateNewPhasorValue
        {
            get
            {
                return m_createNewPhasorValue;
            }
        }

        /// <summary>
        /// Gets reference to <see cref="CreateNewValueFunction{TDefinition,TValue}"/> delegate used to create new <see cref="IFrequencyValue"/> objects.
        /// </summary>
        public virtual CreateNewValueFunction<IFrequencyDefinition, IFrequencyValue> CreateNewFrequencyValue
        {
            get
            {
                return m_createNewFrequencyValue;
            }
        }

        /// <summary>
        /// Gets reference to <see cref="CreateNewValueFunction{TDefinition,TValue}"/> delegate used to create new <see cref="IAnalogValue"/> objects.
        /// </summary>
        public virtual CreateNewValueFunction<IAnalogDefinition, IAnalogValue> CreateNewAnalogValue
        {
            get
            {
                return m_createNewAnalogValue;
            }
        }

        /// <summary>
        /// Gets reference to <see cref="CreateNewValueFunction{TDefinition,TValue}"/> delegate used to create new <see cref="IDigitalValue"/> objects.
        /// </summary>
        public virtual CreateNewValueFunction<IDigitalDefinition, IDigitalValue> CreateNewDigitalValue
        {
            get
            {
                return m_createNewDigitalValue;
            }
        }

        #endregion
    }
}