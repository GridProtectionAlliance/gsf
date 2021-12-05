//******************************************************************************************************
//  IPhasorDefinition.cs - Gbtc
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
//  02/18/2005 - J. Ritchie Carroll
//       Generated original version of source code.
//  12/17/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using GSF.Units.EE;

namespace GSF.PhasorProtocols
{
    #region [ Enumerations ]

    /// <summary>
    /// Phasor coordinate formats enumeration.
    /// </summary>
    [Serializable]
    public enum CoordinateFormat : byte
    {
        /// <summary>
        /// Rectangular coordinate format.
        /// </summary>
        Rectangular,
        /// <summary>
        /// Polar coordinate format.
        /// </summary>
        Polar
    }

    /// <summary>
    /// Angle format enumeration.
    /// </summary>
    public enum AngleFormat : byte
    {
        /// <summary>
        /// Serialize angles in degrees.
        /// </summary>
        Degrees,
        /// <summary>
        /// Serialize angles in radians.
        /// </summary>
        Radians
    }

    #endregion

    /// <summary>
    /// Represents a protocol independent interface representation of a definition of a <see cref="IPhasorValue"/>.
    /// </summary>
    public interface IPhasorDefinition : IChannelDefinition
    {
        /// <summary>
        /// Gets or sets the <see cref="PhasorProtocols.CoordinateFormat"/> of this <see cref="IPhasorDefinition"/>.
        /// </summary>
        CoordinateFormat CoordinateFormat { get; }

        /// <summary>
        /// Gets or sets the <see cref="PhasorProtocols.AngleFormat"/> of this <see cref="IPhasorDefinition"/>.
        /// </summary>
        AngleFormat AngleFormat { get; }

        /// <summary>
        /// Gets or sets the <see cref="PhasorType"/> of this <see cref="IPhasorDefinition"/>.
        /// </summary>
        PhasorType PhasorType { get; set; }

        /// <summary>
        /// Gets or sets the associated <see cref="IPhasorDefinition"/> that represents the voltage reference (if any).
        /// </summary>
        /// <remarks>
        /// This only applies to current phasors.
        /// </remarks>
        IPhasorDefinition VoltageReference { get; set; }
    }
}