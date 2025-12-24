//******************************************************************************************************
//  ConfigurationCell.cs - Gbtc
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
//  02/08/2007 - J. Ritchie Carroll & Jian Ryan Zuo
//       Generated original version of source code.
//  09/15/2009 - Stephen C. Wills
//       Added new header and license agreement.
//  12/17/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Runtime.Serialization;
using GSF.Units.EE;

// ReSharper disable VirtualMemberCallInConstructor
namespace GSF.PhasorProtocols.SelCWS;

/// <summary>
/// Represents the SEL CWS implementation of a <see cref="IConfigurationCell"/> that can be sent or received.
/// </summary>
[Serializable]
public class ConfigurationCell : ConfigurationCellBase
{
    #region [ Constructors ]

    /// <summary>
    /// Creates a new <see cref="ConfigurationCell"/> from specified parameters.
    /// </summary>
    /// <param name="parent">The reference to parent <see cref="ConfigurationFrame"/> of this <see cref="ConfigurationCell"/>.</param>
    /// <param name="nominalFrequency">The nominal <see cref="LineFrequency"/> of the <see cref="FrequencyDefinition"/> of this <see cref="ConfigurationCell"/>.</param>
    /// <param name="scalars">The scalar values for this <see cref="ConfigurationCell"/>.</param>
    /// <param name="stationName">The name of the station for this <see cref="ConfigurationCell"/>.</param>
    /// <param name="analogNames">The names of the analog POW points for this <see cref="ConfigurationCell"/>.</param>
    internal ConfigurationCell(ConfigurationFrame parent, LineFrequency nominalFrequency, float[] scalars, string stationName, string[] analogNames)
        : base(parent, parent.IDCode, Common.MaximumPhasorValues, Common.MaximumAnalogValues, Common.MaximumDigitalValues)
    {
        NominalFrequency = nominalFrequency;
        StationName = stationName;
        FrequencyDefinition = new FrequencyDefinition(this, "Frequency");

        for (int i = 0; i < Common.MaximumAnalogValues; i++)
        {
            AnalogDefinitions.Add(new AnalogDefinition(this, $"PoW Analog {analogNames[i]}", 1, 0.0, AnalogType.SinglePointOnWave)
            {
                Scalar = scalars[i]
            });
        }

        for (int i = 0; i < Common.MaximumPhasorValues; i++)
        {
            PhasorDefinitions.Add(new PhasorDefinition(this, analogNames[i], i < 3 ? PhasorType.Current : PhasorType.Voltage));
        }
    }

    /// <summary>
    /// Creates a new <see cref="ConfigurationCell"/> from serialization parameters.
    /// </summary>
    /// <param name="info">The <see cref="SerializationInfo"/> with populated with data.</param>
    /// <param name="context">The source <see cref="StreamingContext"/> for this deserialization.</param>
    protected ConfigurationCell(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }

    #endregion

    #region [ Properties ]

    /// <summary>
    /// Gets a reference to the parent <see cref="ConfigurationFrame"/> for this <see cref="ConfigurationCell"/>.
    /// </summary>
    public new ConfigurationFrame Parent
    {
        get => (base.Parent as ConfigurationFrame)!;
        set => base.Parent = value;
    }

    /// <summary>
    /// Gets or sets the ID code of this <see cref="ConfigurationCell"/>.
    /// </summary>
    public override ushort IDCode
    {
        // SEL CWS protocol only allows one device, so we share ID code with parent frame...
        get => Parent.IDCode;
        set
        {
            Parent.IDCode = value;
            base.IDCode = value;
        }
    }

    /// <summary>
    /// Gets or sets the <see cref="DataFormat"/> for the <see cref="IPhasorDefinition"/> objects in the <see cref="ConfigurationCellBase.PhasorDefinitions"/> of this <see cref="ConfigurationCell"/>.
    /// </summary>
    /// <remarks>
    /// This property only supports fixed-integer phasor data; SEL CWS doesn't transport floating-point phasor values.
    /// </remarks>
    /// <exception cref="NotSupportedException">SEL CWS only supports fixed-integer data.</exception>
    public override DataFormat PhasorDataFormat
    {
        get => DataFormat.FixedInteger;
        set
        {
            if (value != DataFormat.FixedInteger)
                throw new NotSupportedException("SEL CWS only supports fixed-integer data");
        }
    }

    /// <summary>
    /// Gets or sets the <see cref="CoordinateFormat"/> for the <see cref="IPhasorDefinition"/> objects in the <see cref="ConfigurationCellBase.PhasorDefinitions"/> of this <see cref="ConfigurationCell"/>.
    /// </summary>
    /// <remarks>
    /// This property only supports polar phasor data; SEL CWS doesn't transport rectangular phasor values.
    /// </remarks>
    /// <exception cref="NotSupportedException">SEL CWS only supports polar phasor data.</exception>
    public override CoordinateFormat PhasorCoordinateFormat
    {
        get => CoordinateFormat.Polar;
        set
        {
            if (value != CoordinateFormat.Polar)
                throw new NotSupportedException("SEL CWS only supports polar phasor data");
        }
    }

    /// <summary>
    /// Gets or sets the <see cref="DataFormat"/> of the <see cref="FrequencyDefinition"/> of this <see cref="ConfigurationCell"/>.
    /// </summary>
    /// <remarks>
    /// This property only supports floating-point data; SEL CWS doesn't transport scaled values.
    /// </remarks>
    /// <exception cref="NotSupportedException">SEL CWS only supports floating-point data.</exception>
    public override DataFormat FrequencyDataFormat
    {
        get => DataFormat.FloatingPoint;
        set
        {
            if (value != DataFormat.FloatingPoint)
                throw new NotSupportedException("SEL CWS only supports floating-point data");
        }
    }

    /// <summary>
    /// Gets or sets the <see cref="DataFormat"/> for the <see cref="IAnalogDefinition"/> objects in the <see cref="ConfigurationCellBase.AnalogDefinitions"/> of this <see cref="ConfigurationCell"/>.
    /// </summary>
    /// <remarks>
    /// This property only supports floating-point data; SEL CWS doesn't transport scaled values.
    /// </remarks>
    /// <exception cref="NotSupportedException">SEL CWS only supports floating-point data.</exception>
    public override DataFormat AnalogDataFormat
    {
        get => DataFormat.FloatingPoint;
        set
        {
            if (value != DataFormat.FloatingPoint)
                throw new NotSupportedException("SEL CWS only supports floating-point data");
        }
    }

    /// <summary>
    /// Gets the maximum length of the <see cref="ConfigurationCellBase.StationName"/> of this <see cref="ConfigurationCell"/>.
    /// </summary>
    public override int MaximumStationNameLength => 21;

    #endregion

    #region [ Methods ]

    // Skip common state parsing functions - SEL CWS configuration details have already been parsed at frame level

    /// <inheritdoc/>
    protected override int ParseHeaderImage(byte[] buffer, int startIndex, int length)
    {
        return 0;
    }

    /// <inheritdoc/>
    protected override int ParseBodyImage(byte[] buffer, int startIndex, int length)
    {
        return 0;
    }

    /// <inheritdoc/>
    protected override int ParseFooterImage(byte[] buffer, int startIndex, int length)
    {
        return 0;
    }

    #endregion
}