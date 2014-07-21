//******************************************************************************************************
//  IConfigurationFrame.cs - Gbtc
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
//  01/14/2005 - J. Ritchie Carroll
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
    /// Represents a protocol independent interface representation of any kind of configuration frame that contains
    /// a collection of <see cref="IConfigurationCell"/> objects.
    /// </summary>
    public interface IConfigurationFrame : IChannelFrame
    {
        /// <summary>
        /// Gets reference to the <see cref="ConfigurationCellCollection"/> for this <see cref="IConfigurationFrame"/>.
        /// </summary>
        new ConfigurationCellCollection Cells { get; }

        /// <summary>
        /// Gets or sets the parsing state for the this <see cref="IConfigurationFrame"/>.
        /// </summary>
        new IConfigurationFrameParsingState State { get; set; }

            /// <summary>
        /// Gets or sets defined frame rate of this <see cref="IConfigurationFrame"/>.
        /// </summary>
        ushort FrameRate { get; set; }

        /// <summary>
        /// Gets the defined <see cref="Ticks"/> per frame of this <see cref="IConfigurationFrame"/>.
        /// </summary>
        decimal TicksPerFrame { get; }

        /// <summary>
        /// Sets a new nominal <see cref="LineFrequency"/> for all <see cref="IFrequencyDefinition"/> elements of each <see cref="IConfigurationCell"/> in the <see cref="Cells"/> collection.
        /// </summary>
        /// <param name="value">New nominal <see cref="LineFrequency"/> for <see cref="IFrequencyDefinition"/> elements.</param>
        void SetNominalFrequency(LineFrequency value);
    }
}