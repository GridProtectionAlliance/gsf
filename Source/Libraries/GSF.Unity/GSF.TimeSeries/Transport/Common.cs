//******************************************************************************************************
//  Common.cs - Gbtc
//
//  Copyright © 2010, Grid Protection Alliance.  All Rights Reserved.
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
//  05/24/2011 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using Microsoft.Win32;
using System.Security.Cryptography;

namespace GSF.TimeSeries.Transport
{
    /// <summary>
    /// Common static methods and extensions for transport library.
    /// </summary>
    public static class Common
    {
        // Flag that determines if managed encryption wrappers can be used over FIPS-compliant algorithms if desired.
        private static readonly bool s_canUseManagedEncryption;

        // Static Constructor
        static Common()
        {
            // Determine if the operating system configuration to set to use FIPS-compliant algorithms
            s_canUseManagedEncryption = true; //(Registry.GetValue(fipsKeyNew, "Enabled", 0) ?? Registry.GetValue(fipsKeyOld, "FipsAlgorithmPolicy", 0)).ToString() == "0";
        }

        /// <summary>
        /// Gets flag that determines if managed encryption can be used.
        /// </summary>
        public static bool CanUseManagedEncryption
        {
            get
            {
                return s_canUseManagedEncryption;
            }
        }

        /// <summary>
        /// Gets an AES symmetric algorithm to use for encryption or decryption.
        /// </summary>
        public static SymmetricAlgorithm SymmetricAlgorithm
        {
            get
            {
                Aes symmetricAlgorithm;

                if (s_canUseManagedEncryption)
                    symmetricAlgorithm = new AesManaged();
                else
                    symmetricAlgorithm = new AesCryptoServiceProvider();

                symmetricAlgorithm.KeySize = 256;

                return symmetricAlgorithm;
            }
        }

        /// <summary>
        /// Returns <c>true</c> if <see cref="ServerResponse"/> code is for a solicited <see cref="ServerCommand"/>.
        /// </summary>
        /// <remarks>
        /// Only the <see cref="ServerResponse.Succeeded"/> and <see cref="ServerResponse.Failed"/> response codes are from solicited server commands.
        /// </remarks>
        /// <param name="responseCode"><see cref="ServerResponse"/> code to test.</param>
        /// <returns><c>true</c> if <see cref="ServerResponse"/> code is for a solicited <see cref="ServerCommand"/>; otherwise <c>false</c>.</returns>
        public static bool IsSolicited(this ServerResponse responseCode)
        {
            // Only the succeeded and failed response codes are from solicited commands
            if (responseCode == ServerResponse.Succeeded || responseCode == ServerResponse.Failed)
                return true;

            return false;
        }
    }
}
