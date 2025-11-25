//******************************************************************************************************
//  DataCell.cs - Gbtc
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
using System.ComponentModel;
using System.Runtime.Serialization;
using GSF.Units;

// ReSharper disable RedundantOverriddenMember
// ReSharper disable VirtualMemberCallInConstructor
namespace GSF.PhasorProtocols.SelCWS;

/// <summary>
/// Represents the SEL CWS implementation of a <see cref="IDataCell"/> that can be sent or received.
/// </summary>
[Serializable]
public class DataCell : DataCellBase
{
    #region [ Constructors ]

    /// <summary>
    /// Creates a new <see cref="DataCell"/>.
    /// </summary>
    /// <param name="parent">The reference to parent <see cref="IDataFrame"/> of this <see cref="DataCell"/>.</param>
    /// <param name="configurationCell">The <see cref="IConfigurationCell"/> associated with this <see cref="DataCell"/>.</param>
    public DataCell(IDataFrame parent, IConfigurationCell configurationCell)
        : base(parent, configurationCell, 0x0000, Common.MaximumPhasorValues, Common.MaximumAnalogValues, Common.MaximumDigitalValues)
    {
        // Initialize frequency and df/dt
        FrequencyValue = new FrequencyValue(this, configurationCell.FrequencyDefinition);

        // Initialize phasor values
        for (int i = 0; i < configurationCell.PhasorDefinitions.Count; i++)
            PhasorValues.Add(new PhasorValue(this, configurationCell.PhasorDefinitions[i]));
    }

    /// <summary>
    /// Creates a new <see cref="DataCell"/> from serialization parameters.
    /// </summary>
    /// <param name="info">The <see cref="SerializationInfo"/> with populated with data.</param>
    /// <param name="context">The source <see cref="StreamingContext"/> for this deserialization.</param>
    protected DataCell(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }

    #endregion

    #region [ Properties ]

    /// <summary>
    /// Gets or sets the reference to parent <see cref="DataFrame"/> of this <see cref="DataCell"/>.
    /// </summary>
    public new DataFrame Parent
    {
        get => base.Parent as DataFrame;
        set => base.Parent = value;
    }

    /// <summary>
    /// Gets or sets the <see cref="ConfigurationCell"/> associated with this <see cref="DataCell"/>.
    /// </summary>
    public new ConfigurationCell ConfigurationCell
    {
        get => base.ConfigurationCell as ConfigurationCell;
        set => base.ConfigurationCell = value;
    }

    /// <inheritdoc/>
    public override bool DataIsValid
    {
        get => true;
        set
        {
            // We just ignore updates to this value; SEL CWS defines no flags to determine if data is valid
        }
    }

    /// <inheritdoc/>
    public override bool SynchronizationIsValid
    {
        get => true;
        set
        {
            // We just ignore updates to this value; SEL CWS defines no flags to determine if data is synchronized
        }
    }

    /// <inheritdoc/>
    public override DataSortingType DataSortingType
    {
        get => SynchronizationIsValid ? DataSortingType.ByTimestamp : DataSortingType.ByArrival;
        set
        {
            // We just ignore updates to this value; data sorting type has been defined as a derived value based on synchronization validity
        }
    }

    /// <inheritdoc/>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public override bool DeviceError
    {
        get => false;
        set
        {
            // We just ignore updates to this value; SEL CWS defines no flags for data errors
        }
    }

    #endregion

    #region [ Methods ]

    /// <summary>
    /// Parses the binary body image.
    /// </summary>
    /// <param name="buffer">Binary image to parse.</param>
    /// <param name="startIndex">Start index into <paramref name="buffer"/> to begin parsing.</param>
    /// <param name="length">Length of valid data within <paramref name="buffer"/>.</param>
    /// <returns>The length of the data that was parsed.</returns>
    protected override int ParseBodyImage(byte[] buffer, int startIndex, int length)
    {
        ConfigurationCell configurationCell = ConfigurationCell;
        int index = startIndex;

        if (FrequencyValue is null)
            FrequencyValue = new FrequencyValue(this, configurationCell.FrequencyDefinition as FrequencyDefinition, double.NaN, double.NaN);
        else
            FrequencyValue.Frequency = double.NaN;

        // Update (or create) phasor values
        for (int i = 0; i < Common.MaximumPhasorValues; i++)
        {
            Angle angle = double.NaN;
            double magnitude = BigEndian.ToInt32(buffer, index);
            index += 4;

            PhasorValue phasor = null;

            if (PhasorValues.Count > i)
                phasor = PhasorValues[i] as PhasorValue;

            if (phasor is null)
            {
                phasor = new PhasorValue(this, configurationCell.PhasorDefinitions[i] as PhasorDefinition, angle, magnitude);
                PhasorValues.Add(phasor);
            }
            else
            {
                phasor.Angle = angle;
                phasor.Magnitude = magnitude;
            }
        }

        return index - startIndex;
    }

    /// <summary>
    /// Populates a <see cref="SerializationInfo"/> with the data needed to serialize the target object.
    /// </summary>
    /// <param name="info">The <see cref="SerializationInfo"/> to populate with data.</param>
    /// <param name="context">The destination <see cref="StreamingContext"/> for this serialization.</param>
    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        base.GetObjectData(info, context);
    }

    #endregion

    #region [ Static ]

    // Delegate handler to create a new SEL CWS data cell
    internal static IDataCell CreateNewCell(IChannelFrame parent, IChannelFrameParsingState<IDataCell> state, int index, byte[] buffer, int startIndex, out int parsedLength)
    {
        DataCell dataCell = new(parent as IDataFrame, (state as IDataFrameParsingState)?.ConfigurationFrame.Cells[index]);

        parsedLength = dataCell.ParseBinaryImage(buffer, startIndex, 0);

        return dataCell;
    }

    #endregion
}