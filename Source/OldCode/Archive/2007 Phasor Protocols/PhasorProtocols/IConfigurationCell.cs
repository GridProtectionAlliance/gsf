//*******************************************************************************************************
//  IConfigurationCell.cs
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
//  04/16/2009 - James R Carroll
//       Generated original version of source code.
//
//*******************************************************************************************************

using System;

namespace PCS.PhasorProtocols
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
        /// Gets or sets the <see cref="IFrequenceyDefinition"/> of this <see cref="IConfigurationCell"/>.
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
        short FrameRate { get; }

        /// <summary>
        /// Gets or sets the revision count of this <see cref="IConfigurationCell"/>.
        /// </summary>
        ushort RevisionCount { get; set; }
    }
}