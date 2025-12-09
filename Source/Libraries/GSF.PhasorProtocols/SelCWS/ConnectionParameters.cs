//******************************************************************************************************
//  ConnectionParameters.cs - Gbtc
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
//  02/26/2007 - J. Ritchie Carroll & Jian Ryan Zuo
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
using GSF.Units.EE;

namespace GSF.PhasorProtocols.SelCWS;

/// <summary>
/// Represents the extra connection parameters required for a connection to a SEL CWS device.
/// </summary>
/// <remarks>
/// This class is designed to be exposed by a "PropertyGrid" so a UI can request protocol specific connection parameters.
/// As a result the <see cref="CategoryAttribute"/> and <see cref="DescriptionAttribute"/> elements should be defined for
/// each of the exposed properties.
/// </remarks>
[Serializable]
public class ConnectionParameters : ConnectionParametersBase
{
    #region [ Members ]

    // Constants

    /// <summary>
    /// Default value for <see cref="CalculatePhaseEstimates"/>.
    /// </summary>
    public const bool DefaultCalculatePhaseEstimates = true;

    // Fields
    private bool m_calculatePhaseEstimates;
    private ushort m_frameRate;
    private LineFrequency m_nominalFrequency;

    #endregion

    #region [ Constructors ]

    /// <summary>
    /// Creates a new <see cref="ConnectionParameters"/>.
    /// </summary>
    public ConnectionParameters()
    {
        m_calculatePhaseEstimates = DefaultCalculatePhaseEstimates;
        m_frameRate = Common.DefaultFrameRate;
        m_nominalFrequency = Common.DefaultNominalFrequency;
    }

    /// <summary>
    /// Creates a new <see cref="ConnectionParameters"/> from serialization parameters.
    /// </summary>
    /// <param name="info">The <see cref="SerializationInfo"/> with populated with data.</param>
    /// <param name="context">The source <see cref="StreamingContext"/> for this deserialization.</param>
    protected ConnectionParameters(SerializationInfo info, StreamingContext context)
    {
        // Deserialize connection parameters
        m_calculatePhaseEstimates = info.GetOrDefault("calculatePhaseEstimates", DefaultCalculatePhaseEstimates);
        m_frameRate = info.GetOrDefault("frameRate", Common.DefaultFrameRate);
        m_nominalFrequency = info.GetOrDefault("nominalFrequency", Common.DefaultNominalFrequency);
    }

    #endregion

    #region [ Properties ]

    /// <summary>
    /// Gets or sets flag that determines if phase estimates should be calculated for phasor measurements.
    /// </summary>
    [Category("Optional Connection Parameters")]
    [Description("Determines if phase angle estimates should be calculated for phasor magnitudes.")]
    [DefaultValue(DefaultCalculatePhaseEstimates)]
    public bool CalculatePhaseEstimates
    {
        get => m_calculatePhaseEstimates;
        set => m_calculatePhaseEstimates = value;
    }

    /// <summary>
    /// Gets or sets the configured frame rate for the SEL CWS device.
    /// </summary>
    [Category("Optional Connection Parameters")]
    [Description("Configured frame rate for SEL CWS device.")]
    [DefaultValue(Common.DefaultFrameRate)]
    public ushort FrameRate
    {
        get => m_frameRate;
        set => m_frameRate = value < 1 ? Common.DefaultFrameRate : value;
    }

    /// <summary>
    /// Gets or sets the nominal <see cref="LineFrequency"/> of this SEL CWS device.
    /// </summary>
    [Category("Optional Connection Parameters")]
    [Description("Configured nominal frequency for SEL CWS device.")]
    [DefaultValue(typeof(LineFrequency), "Hz60")]
    public LineFrequency NominalFrequency
    {
        get => m_nominalFrequency;
        set => m_nominalFrequency = value;
    }

    #endregion

    #region [ Methods ]

    /// <summary>
    /// Populates a <see cref="SerializationInfo"/> with the data needed to serialize the target object.
    /// </summary>
    /// <param name="info">The <see cref="SerializationInfo"/> to populate with data.</param>
    /// <param name="context">The destination <see cref="StreamingContext"/> for this serialization.</param>
    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        // Serialize connection parameters
        info.AddValue("calculatePhaseEstimates", m_calculatePhaseEstimates);
        info.AddValue("frameRate", m_frameRate);
        info.AddValue("nominalFrequency", m_nominalFrequency, typeof(LineFrequency));
    }

    #endregion
}