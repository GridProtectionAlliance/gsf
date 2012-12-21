//******************************************************************************************************
//  IConfigurationCellParsingState.cs - Gbtc
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
        CreateNewDefinitionFunction<IPhasorDefinition> CreateNewPhasorDefinition
        {
            get;
        }

        /// <summary>
        /// Gets reference to <see cref="CreateNewDefinitionFunction{T}"/> delegate used to create new <see cref="IFrequencyDefinition"/> objects.
        /// </summary>
        CreateNewDefinitionFunction<IFrequencyDefinition> CreateNewFrequencyDefinition
        {
            get;
        }

        /// <summary>
        /// Gets reference to <see cref="CreateNewDefinitionFunction{T}"/> delegate used to create new <see cref="IAnalogDefinition"/> objects.
        /// </summary>
        CreateNewDefinitionFunction<IAnalogDefinition> CreateNewAnalogDefinition
        {
            get;
        }

        /// <summary>
        /// Gets reference to <see cref="CreateNewDefinitionFunction{T}"/> delegate used to create new <see cref="IDigitalDefinition"/> objects.
        /// </summary>
        CreateNewDefinitionFunction<IDigitalDefinition> CreateNewDigitalDefinition
        {
            get;
        }

        // below here; new items needed for ConfigFrame3 
        /*
        /// <summary>
        /// Gets or sets the GUID of the PMU associated with the <see cref="IConfigurationCell"/> being parsed.
        /// </summary>
        System.Guid G_PMU_ID { get; set; }

        /// <summary>
        /// Gets or sets the phasor and channel names (CHNAM) associated with the <see cref="IConfigurationCell"/> being parsed.
        /// </summary>
        string ChannelName { get; set; }

        string[] PhasorName { get; set; }
        string[] AnalogName { get; set; }
        string[] DigitalName { get; set; }

        /// <summary>
        /// Gets or sets the conversions factor for phasor channels (PHSCALE) associated with the <see cref="IConfigurationCell"/> being parsed.
        /// </summary>
        byte[][] PhasorScale { get; set; }

        /// <summary>
        /// Gets or sets the latitude of the device (PMU_LAT) associated with the <see cref="IConfigurationCell"/> being parsed.
        /// </summary>
        float DeviceLatitude { get; set; }

        /// <summary>
        /// Gets or sets the longitude of the device (PMU_LON) associated with the <see cref="IConfigurationCell"/> being parsed.
        /// </summary>
        float DeviceLongitude { get; set; }

        /// <summary>
        /// Gets or sets the elevation of the divice (PMU_ELEV) associated with the <see cref="IConfigurationCell"/> being parsed.
        /// </summary>
        float DeviceElevation { get; set; }

        /// <summary>
        /// Gets or sets the service class (SVC_CLASS) associated with the <see cref="IConfigurationCell"/> being parsed.
        /// </summary>
        char ServiceClass { get; set; }

        /// <summary>
        /// Gets or sets the phasor measurement window (WINDOW) associated with the <see cref="IConfigurationCell"/> being parsed.
        /// </summary>
        int MeasurementWindow { get; set; }
        
        /// <summary>
        /// Gets or sets the phasor measurement group delay (GRP_DLY) associated with the <see cref="IConfigurationCell"/> being parsed.
        /// </summary>
        int GroupDelay { get; set; }

        /// <summary>
        /// Gets or sets the nominal line frequency code and flags (FNOM) associated with the <see cref="IConfigurationCell"/> being parsed.
        /// </summary>
        ushort FNOM { get; set; }

        ushort CFGCNT { get; set; }

        byte[][] ANSCALE { get; set; }

        byte[][] DigitalStatus { get; set; }
        */
    }
}