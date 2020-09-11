//******************************************************************************************************
//  Snmp.cs - Gbtc
//
//  Copyright © 2020, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may not use this
//  file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  09/10/2020 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Threading.Tasks;
using GSF.Configuration;
using GSF.Diagnostics;
using GSF.Net.Snmp.Messaging;
using GSF.Net.Snmp.Security;
using GSF.Security.Cryptography;

namespace GSF.Net.Snmp
{
    /// <summary>
    /// Exposes simple SNMP functionality for GSF.
    /// </summary>
    public static class Snmp
    {
        /// <summary>
        /// Defines default IP end point for SNMP client agent.
        /// </summary>
        public const string DefaultIPEndPoint = "255.255.255.255:162";

        /// <summary>
        /// Root GSF OID (1.3.6.1.4.1.56056). This is the private enterprise number defined for the Grid Protection Alliance.
        /// </summary>
        public static readonly ObjectIdentifier EnterpriseRoot = new ObjectIdentifier(OID.EnterpriseRoot);

        /// <summary>
        /// GSF SNMP engine ID (D3F30A26E61843918ED5ABD87E).
        /// </summary>
        public static readonly OctetString EngineID = new OctetString(ByteTool.Convert("D3F30A26E61843918ED5ABD87E"));

        /// <summary>
        /// Sends an integer based SNMP version 3 trap.
        /// </summary>
        /// <param name="oid">Target OID.</param>
        /// <param name="value">Notification value.</param>
        public static void SendTrap(uint[] oid, int value) => SendTrapAsync(oid, value).Wait();

        /// <summary>
        /// Sends a floating-point based SNMP version 3 trap.
        /// </summary>
        /// <param name="oid">Target OID.</param>
        /// <param name="value">Notification value.</param>
        /// <remarks>
        /// SNMP does not support floating point values via binary encoding, so value is encoded as a string.
        /// </remarks>
        public static void SendTrap(uint[] oid, double value) => SendTrapAsync(oid, value).Wait();

        /// <summary>
        /// Sends a string based SNMP version 3 trap.
        /// </summary>
        /// <param name="oid">Target OID.</param>
        /// <param name="value">Notification value.</param>
        public static void SendTrap(uint[] oid, string value) => SendTrapAsync(oid, value).Wait();

        /// <summary>
        /// Sends an SNMP version 3 trap.
        /// </summary>
        /// <param name="oid">Target OID.</param>
        /// <param name="data">Notification data.</param>
        public static void SendTrap(uint[] oid, ISnmpData data) => SendTrapAsync(oid, data).Wait();

        /// <summary>
        /// Sends an SNMP version 3 trap.
        /// </summary>
        /// <param name="variables">Variables.</param>
        public static void SendTrap(params Variable[] variables) => SendTrapAsync(variables).Wait();

        /// <summary>
        /// Sends an integer based SNMP version 3 trap.
        /// </summary>
        /// <param name="oid">Target OID.</param>
        /// <param name="value">Notification value.</param>
        public static Task SendTrapAsync(uint[] oid, int value) => SendTrapAsync(oid, new Integer32(value));

        /// <summary>
        /// Sends a floating-point based SNMP version 3 trap.
        /// </summary>
        /// <param name="oid">Target OID.</param>
        /// <param name="value">Notification value.</param>
        /// <remarks>
        /// SNMP does not support floating point values via binary encoding, so value is encoded as a string.
        /// </remarks>
        public static Task SendTrapAsync(uint[] oid, double value) => SendTrapAsync(oid, new OctetString(value.ToString(CultureInfo.InvariantCulture)));

        /// <summary>
        /// Sends a string based SNMP version 3 trap.
        /// </summary>
        /// <param name="oid">Target OID.</param>
        /// <param name="value">Notification value.</param>
        public static Task SendTrapAsync(uint[] oid, string value) => SendTrapAsync(oid, new OctetString(value));

        /// <summary>
        /// Sends an SNMP version 3 trap.
        /// </summary>
        /// <param name="oid">Target OID.</param>
        /// <param name="data">Notification data.</param>
        public static Task SendTrapAsync(uint[] oid, ISnmpData data) => SendTrapAsync(new Variable(oid, data));

        /// <summary>
        /// Sends an SNMP version 3 trap.
        /// </summary>
        /// <param name="variables">Variables.</param>
        public static async Task SendTrapAsync(params Variable[] variables)
        {
            await new TrapV2Message
            (
                VersionCode.V3,
                Messenger.NextMessageId,
                Messenger.NextRequestId,
                s_community,
                EnterpriseRoot,
                (uint)Environment.TickCount / 10,
                new List<Variable>(variables),
                s_privacyProvider,
                Messenger.MaxMessageSize,
                EngineID, 0, 0
            )
            .SendAsync(s_endPoint);
        }

        private static readonly OctetString s_community;
        private static readonly IPEndPoint s_endPoint;
        private static readonly DESPrivacyProvider s_privacyProvider;

        static Snmp()
        {
            try
            {
                // Get configured SNMP agent settings
                CategorizedSettingsElementCollection settings = ConfigurationFile.Current.Settings["snmpAgent"];
                PasswordGenerator passwordGenerator = new PasswordGenerator(PasswordCharacterGroups);

                settings.Add("Community", "Eval(securityProvider.ApplicationName)", "Defines the SNMP agent community identifier for trap messages.");
                settings.Add("IPEndPoint", DefaultIPEndPoint, "Defines the SNMP agent target IP end point for trap messages.");
                settings.Add("AuthPhrase", passwordGenerator.GeneratePassword(16), "Defines the SNMP agent authentication phrase for trap messages.");
                settings.Add("EncryptKey", passwordGenerator.GeneratePassword(16), "Defines the SNMP agent encryption key for trap messages.");

                s_community = new OctetString(settings["Community"].ValueAs(nameof(GSF)));

                string endPoint = settings["IPEndPoint"].ValueAs(DefaultIPEndPoint);
                string[] parts = endPoint.Split(':');

                if (parts.Length != 2)
                {
                    parts = DefaultIPEndPoint.Split(':');
                    Logger.SwallowException(new Exception($"Configured SNMP agent IP end point \"{endPoint}\" format is invalid. Using default IP end point \"{DefaultIPEndPoint}\"."));
                }

                if (!IPAddress.TryParse(parts[0], out IPAddress address))
                {
                    address = IPAddress.Broadcast;
                    Logger.SwallowException(new Exception($"Configured SNMP agent IP end point \"{endPoint}\" address is invalid. Using broadcast address \"255.255.255.255\"."));
                }

                if (!ushort.TryParse(parts[1], out ushort port))
                {
                    port = 162;
                    Logger.SwallowException(new Exception($"Configured SNMP agent IP end point \"{endPoint}\" port is invalid. Using default port \"162\"."));
                }

                s_endPoint = new IPEndPoint(address, port);

                string authPhrase = settings["AuthPhrase"].Value.PadRight(16, '#');
                string encryptKey = settings["EncryptKey"].Value.PadRight(16, '#');

                s_privacyProvider = new DESPrivacyProvider(
                    new OctetString(encryptKey),
                    new MD5AuthenticationProvider(new OctetString(authPhrase)));
            }
            catch (Exception ex)
            {
                Logger.SwallowException(new Exception($"Failed to load SNMP agent configuration: {ex.Message}", ex));
            }
        }

        private static readonly IReadOnlyList<CharacterGroup> PasswordCharacterGroups = new List<CharacterGroup>()
        {
            new CharacterGroup { Characters = "abcdefghijklmnopqrstuvwxyz", MinOccurrence = 1 },
            new CharacterGroup { Characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ", MinOccurrence = 1 },
            new CharacterGroup { Characters = "0123456789", MinOccurrence = 1 },
        };
    }
}
