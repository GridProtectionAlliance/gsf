//******************************************************************************************************
//  IFrequencyDefinition.cs - Gbtc
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
//  09/15/2009 - Stephen C. Wills
//       Added new header and license agreement.
//  12/17/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using GSF.Units.EE;

namespace GSF.PhasorProtocols
{
    /// <summary>
    /// Represents a protocol independent interface representation of a definition of a <see cref="IFrequencyValue"/>.
    /// </summary>
    public interface IFrequencyDefinition : IChannelDefinition
    {
        /// <summary>
        /// Gets the nominal <see cref="LineFrequency"/> of this <see cref="IFrequencyDefinition"/>.
        /// </summary>
        LineFrequency NominalFrequency { get; }

        /// <summary>
        /// Gets or sets the df/dt offset of this <see cref="IFrequencyDefinition"/>.
        /// </summary>
        double DfDtOffset { get; set; }

        /// <summary>
        /// Gets or sets the df/dt scaling value of this <see cref="IFrequencyDefinition"/>.
        /// </summary>
        uint DfDtScalingValue { get; set; }
    }
}