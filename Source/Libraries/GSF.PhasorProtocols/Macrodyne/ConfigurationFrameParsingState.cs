//******************************************************************************************************
//  ConfigurationFrameParsingState.cs - Gbtc
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
//  02/08/2010 - James R. Carroll
//       Generated original version of source code.
//  12/17/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

namespace GSF.PhasorProtocols.Macrodyne
{
    /// <summary>
    /// Represents the Macrodyne implementation of the parsing state used by a <see cref="ConfigurationFrame"/>.
    /// </summary>
    public class ConfigurationFrameParsingState : PhasorProtocols.ConfigurationFrameParsingState
    {
        #region [ Members ]

        // Fields

    #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="ConfigurationFrameParsingState"/> from specified parameters.
        /// </summary>
        /// <param name="parsedBinaryLength">Binary length of the <see cref="ConfigurationFrame"/> being parsed.</param>
        /// <param name="headerFrame">Previously parsed header frame that contains needed station name.</param>
        /// <param name="createNewCellFunction">Reference to delegate to create new <see cref="ConfigurationCell"/> instances.</param>
        /// <param name="trustHeaderLength">Determines if header lengths should be trusted over parsed byte count.</param>
        /// <param name="validateCheckSum">Determines if frame's check-sum should be validated.</param>
        public ConfigurationFrameParsingState(int parsedBinaryLength, HeaderFrame headerFrame, CreateNewCellFunction<IConfigurationCell> createNewCellFunction, bool trustHeaderLength, bool validateCheckSum)
            : base(parsedBinaryLength, createNewCellFunction, trustHeaderLength, validateCheckSum, 1)
        {
            HeaderFrame = headerFrame;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the header frame, which contains the unit ID (i.e., station name), of the device.
        /// </summary>
        public HeaderFrame HeaderFrame { get; set; }

    #endregion
    }
}