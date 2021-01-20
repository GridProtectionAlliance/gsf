//******************************************************************************************************
//  DataFrameParsingState.cs - Gbtc
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
//  12/17/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

namespace GSF.PhasorProtocols.BPAPDCstream
{
    /// <summary>
    /// Represents the BPA PDCstream protocol implementation of the parsing state used by a <see cref="DataFrame"/>.
    /// </summary>
    public class DataFrameParsingState : PhasorProtocols.DataFrameParsingState
    {
        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="DataFrameParsingState"/> from specified parameters.
        /// </summary>
        /// <param name="parsedBinaryLength">Binary length of the <see cref="IDataFrame"/> being parsed.</param>
        /// <param name="configurationFrame">Reference to the <see cref="IConfigurationFrame"/> associated with the <see cref="IDataFrame"/> being parsed.</param>
        /// <param name="createNewCellFunction">Reference to delegate to create new <see cref="IDataCell"/> instances.</param>
        /// <param name="trustHeaderLength">Determines if header lengths should be trusted over parsed byte count.</param>
        /// <param name="validateCheckSum">Determines if frame's check-sum should be validated.</param>
        public DataFrameParsingState(int parsedBinaryLength, IConfigurationFrame configurationFrame, CreateNewCellFunction<IDataCell> createNewCellFunction, bool trustHeaderLength, bool validateCheckSum)
            : base(parsedBinaryLength, configurationFrame, createNewCellFunction, trustHeaderLength, validateCheckSum)
        {
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets reference to the <see cref="ConfigurationFrame"/> associated with the <see cref="DataFrame"/> being parsed.
        /// </summary>
        public new ConfigurationFrame ConfigurationFrame => base.ConfigurationFrame as ConfigurationFrame;

        /// <summary>
        /// Gets or sets the remaining number of PMU's the PDC block to be parsed.
        /// </summary>
        public int RemainingPdcBlockPmus { get; set; }

    #endregion
    }
}