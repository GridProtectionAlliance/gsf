//******************************************************************************************************
//  IConfigurationCell.cs - Gbtc
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
//  04/16/2009 - J. Ritchie Carroll
//       Generated original version of source code.
//  12/17/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using GSF.Units.EE;

namespace GSF.PhasorProtocols
{
    /// <summary>
    /// Represents a protocol independent interface representation of any kind of <see cref="IConfigurationFrame"/> cell.
    /// </summary>
    public interface IConfigurationCell : IChannelCell, IEquatable<IConfigurationCell>, IComparable<IConfigurationCell>, IComparable
    {
        /// <summary>
        /// Gets a reference to the parent <see cref="IConfigurationFrame"/> for this <see cref="IConfigurationCell"/>.
        /// </summary>
        new IConfigurationFrame Parent { get; set; }

        /// <summary>
        /// Gets or sets the parsing state for the this <see cref="IConfigurationCell"/>.
        /// </summary>
        new IConfigurationCellParsingState State { get; set; }

        /// <summary>
        /// Gets or sets the station name of this <see cref="IConfigurationCell"/>.
        /// </summary>
        string StationName { get; set; }

        /// <summary>
        /// Gets the binary image of the <see cref="StationName"/> of this <see cref="IConfigurationCell"/>.
        /// </summary>
        byte[] StationNameImage { get; }

        /// <summary>
        /// Gets the maximum length of the <see cref="StationName"/> of this <see cref="IConfigurationCell"/>.
        /// </summary>
        int MaximumStationNameLength { get; }

        /// <summary>
        /// Gets or sets the ID label of this <see cref="IConfigurationCell"/>.
        /// </summary>
        string IDLabel { get; set; }

        /// <summary>
        /// Gets the binary image of the <see cref="IDLabel"/> of this <see cref="IConfigurationCell"/>.
        /// </summary>
        byte[] IDLabelImage { get; }

        /// <summary>
        /// Gets the length of the <see cref="IDLabel"/> of this <see cref="IConfigurationCell"/>.
        /// </summary>
        int IDLabelLength { get; }

        /// <summary>
        /// Gets a reference to the <see cref="PhasorDefinitionCollection"/> of this <see cref="IConfigurationCell"/>.
        /// </summary>
        PhasorDefinitionCollection PhasorDefinitions { get; }

        /// <summary>
        /// Gets or sets the <see cref="DataFormat"/> for the <see cref="IPhasorDefinition"/> objects in the <see cref="PhasorDefinitions"/> of this <see cref="IConfigurationCell"/>.
        /// </summary>
        DataFormat PhasorDataFormat { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="CoordinateFormat"/> for the <see cref="IPhasorDefinition"/> objects in the <see cref="PhasorDefinitions"/> of this <see cref="IConfigurationCell"/>.
        /// </summary>
        CoordinateFormat PhasorCoordinateFormat { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="AngleFormat"/> for the <see cref="IPhasorDefinition"/> objects in the <see cref="PhasorDefinitions"/> of this <see cref="IConfigurationCell"/>.
        /// </summary>
        AngleFormat PhasorAngleFormat { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="IFrequencyDefinition"/> of this <see cref="IConfigurationCell"/>.
        /// </summary>
        IFrequencyDefinition FrequencyDefinition { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="DataFormat"/> of the <see cref="FrequencyDefinition"/> of this <see cref="IConfigurationCell"/>.
        /// </summary>
        DataFormat FrequencyDataFormat { get; set; }

        /// <summary>
        /// Gets or sets the nominal <see cref="LineFrequency"/> of the <see cref="FrequencyDefinition"/> of this <see cref="IConfigurationCell"/>.
        /// </summary>
        LineFrequency NominalFrequency { get; set; }

        /// <summary>
        /// Gets a reference to the <see cref="AnalogDefinitionCollection"/> of this <see cref="IConfigurationCell"/>.
        /// </summary>
        AnalogDefinitionCollection AnalogDefinitions { get; }

        /// <summary>
        /// Gets or sets the <see cref="DataFormat"/> for the <see cref="IAnalogDefinition"/> objects in the <see cref="AnalogDefinitions"/> of this <see cref="IConfigurationCell"/>.
        /// </summary>
        DataFormat AnalogDataFormat { get; set; }

        /// <summary>
        /// Gets a reference to the <see cref="DigitalDefinitionCollection"/> of this <see cref="IConfigurationCell"/>.
        /// </summary>
        DigitalDefinitionCollection DigitalDefinitions { get; }

        /// <summary>
        /// Gets the specified frame rate of this <see cref="IConfigurationCell"/>.
        /// </summary>
        ushort FrameRate { get; }

        /// <summary>
        /// Gets or sets the revision count of this <see cref="IConfigurationCell"/>.
        /// </summary>
        ushort RevisionCount { get; set; }
    }
}