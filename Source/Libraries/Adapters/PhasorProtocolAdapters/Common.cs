//******************************************************************************************************
//  Common.cs - Gbtc
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
//  02/18/2005 - J. Ritchie Carroll
//       Generated original version of source code.
//  09/15/2009 - Stephen C. Wills
//       Added new header and license agreement.
//  04/21/2010 - J.Ritchie Carroll
//       Added GetFormattedSignalTypeName signal type enumeration extension.
//  10/5/2012 - Gavin E. Holden
//       Added new header and license agreement.
//  12/04/2012 - J. Ritchie Carroll
//       Migrated to PhasorProtocolAdapters project.
//  12/13/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using GSF;

namespace PhasorProtocolAdapters
{
    /// <summary>
    /// Common constants, functions and extensions for phasor protocol adapters.
    /// </summary>
    public static class Common
    {
        /// <summary>
        /// Returns display friendly signal type name.
        /// </summary>
        /// <param name="signalType"><see cref="SignalType"/> to return display name for.</param>
        /// <returns>Friendly protocol display name for specified <paramref name="signalType"/>.</returns>
        public static string GetFormattedSignalTypeName(this SignalType signalType)
        {
            switch (signalType)
            {
                case SignalType.IPHM:
                    return "Current phase magnitude";
                case SignalType.IPHA:
                    return "Current phase angle";
                case SignalType.VPHM:
                    return "Voltage phase magnitude";
                case SignalType.VPHA:
                    return "Voltage phase angle";
                case SignalType.FREQ:
                    return "Frequency";
                case SignalType.DFDT:
                    return "Frequency delta (dF/dt)";
                case SignalType.ALOG:
                    return "Analog";
                case SignalType.FLAG:
                    return "Status flags";
                case SignalType.DIGI:
                    return "Digital";
                case SignalType.CALC:
                    return "Calculated";
                case SignalType.NONE:
                    return "Undefined";
                default:
                    return signalType.ToString().ToTitleCase();
            }
        }
    }
}