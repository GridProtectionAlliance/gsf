//******************************************************************************************************
//  IAnalogDefinition.cs - Gbtc
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
//  10/5/2012 - Gavin E. Holden
//       Added new header and license agreement.
//  12/17/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;

namespace GSF.PhasorProtocols
{
    #region [ Enumerations ]

    /// <summary>
    /// Analog types enumeration.
    /// </summary>
    [Serializable]
    public enum AnalogType : byte
    {
        /// <summary>
        /// Single point-on-wave.
        /// </summary>
        SinglePointOnWave = 0,
        /// <summary>
        /// RMS of analog input.
        /// </summary>
        RmsOfAnalogInput = 1,
        /// <summary>
        /// Peak of analog input.
        /// </summary>
        PeakOfAnalogInput = 2
    }

    #endregion

    /// <summary>
    /// Represents a protocol independent interface representation of a definition of an <see cref="IAnalogValue"/>.
    /// </summary>
    public interface IAnalogDefinition : IChannelDefinition
    {
        /// <summary>
        /// Gets or sets <see cref="AnalogType"/> of this <see cref="IAnalogDefinition"/>.
        /// </summary>
        AnalogType AnalogType { get; set; }
    }
}