//******************************************************************************************************
//  IPhasorValue.cs - Gbtc
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

using GSF.Units;
using GSF.Units.EE;

namespace GSF.PhasorProtocols
{
    /// <summary>
    /// Represents a protocol independent interface representation of a phasor value.
    /// </summary>
    public interface IPhasorValue : IChannelValue<IPhasorDefinition>
    {
        /// <summary>
        /// Gets the <see cref="PhasorProtocols.CoordinateFormat"/> of this <see cref="IPhasorValue"/>.
        /// </summary>
        CoordinateFormat CoordinateFormat { get; }

        /// <summary>
        /// Gets the <see cref="PhasorProtocols.AngleFormat"/> of this <see cref="IPhasorValue"/>.
        /// </summary>
        AngleFormat AngleFormat { get; }

        /// <summary>
        /// Gets the <see cref="PhasorType"/> of this <see cref="IPhasorValue"/>.
        /// </summary>
        PhasorType Type { get; }

        /// <summary>
        /// Gets <see cref="Units.EE.Phasor"/> value from this <see cref="IPhasorValue"/>.
        /// </summary>
        Phasor Phasor { get; }

        /// <summary>
        /// Gets or sets the <see cref="Units.Angle"/> value (a.k.a., the argument) of this <see cref="IPhasorValue"/>, in radians.
        /// </summary>
        Angle Angle { get; set; }

        /// <summary>
        /// Gets or sets the magnitude value (a.k.a., the absolute value or modulus) of this <see cref="IPhasorValue"/>.
        /// </summary>
        double Magnitude { get; set; }

        /// <summary>
        /// Gets or sets the real floating point value of this <see cref="IPhasorValue"/>.
        /// </summary>
        double Real { get; set; }

        /// <summary>
        /// Gets or sets the imaginary floating point value of this <see cref="IPhasorValue"/>.
        /// </summary>
        double Imaginary { get; set; }

        /// <summary>
        /// Gets or sets the unscaled integer representation of the real value of this <see cref="IPhasorValue"/>.
        /// </summary>
        int UnscaledReal { get; set; }

        /// <summary>
        /// Gets or sets the unscaled integer representation of the imaginary value of this <see cref="IPhasorValue"/>.
        /// </summary>
        int UnscaledImaginary { get; set; }
    }
}