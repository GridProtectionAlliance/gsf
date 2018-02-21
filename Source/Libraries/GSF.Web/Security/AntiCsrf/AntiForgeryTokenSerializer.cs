//******************************************************************************************************
//  AntiForgeryTokenSerializer.cs - Gbtc
//
//  Copyright © 2018, Grid Protection Alliance.  All Rights Reserved.
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
//  02/15/2018 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

#region [ Contributor License Agreements ]

// Derived from AspNetWebStack (https://github.com/aspnet/AspNetWebStack) 
// Copyright (c) .NET Foundation. All rights reserved.
// See NOTICE.txt file in Source folder for more information.

#endregion

using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.IO;
using System.Web.Mvc;

namespace GSF.Web.Security.AntiCsrf
{
    internal static class AntiForgeryTokenSerializer
    {
        private const byte TokenVersion = 0xF1; // Distinguish GSF tokens from AspNet 0x01 version tokens

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Failures are homogenized; caller handles appropriately.")]
        [SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times", Justification = "MemoryStream is safe for multi-dispose.")]
        public static AntiForgeryToken Deserialize(string serializedToken)
        {
            try
            {
                using (MemoryStream stream = new MemoryStream(CryptoSystem.Unprotect(serializedToken)))
                using (BinaryReader reader = new BinaryReader(stream))
                {
                    AntiForgeryToken token = Deserialize(reader);

                    if (token != null)
                        return token;
                }
            }
            catch
            {
                // swallow all exceptions - homogenize error if something went wrong
            }

            // if we reached this point, something went wrong deserializing
            throw new HttpAntiForgeryException("Deserialization failed or the anti-forgery token could not be decrypted.");
        }

        /* The serialized format of the anti-XSRF token is as follows:
         * Version: 1 byte integer
         * SecurityToken: 16 byte binary blob
         * IsSessionToken: 1 byte Boolean
         * [if IsSessionToken = false]
         *   `- Username: UTF-8 string with 7-bit integer length prefix
         *   `- AdditionalData: UTF-8 string with 7-bit integer length prefix
         */
        private static AntiForgeryToken Deserialize(BinaryReader reader)
        {
            // we can only consume tokens of the same serialized version that we generate
            byte embeddedVersion = reader.ReadByte();

            if (embeddedVersion != TokenVersion)
                return null;

            AntiForgeryToken deserializedToken = new AntiForgeryToken();
            byte[] securityTokenBytes = reader.ReadBytes(AntiForgeryToken.SecurityTokenBitLength / 8);

            deserializedToken.SecurityToken = new BinaryBlob(AntiForgeryToken.SecurityTokenBitLength, securityTokenBytes);
            deserializedToken.IsSessionToken = reader.ReadBoolean();

            if (!deserializedToken.IsSessionToken)
            {
                deserializedToken.Username = reader.ReadString();
                deserializedToken.AdditionalData = reader.ReadString();
            }

            // if there's still unconsumed data in the stream, fail; otherwise, return deserialized token
            return reader.BaseStream.ReadByte() != -1 ? null : deserializedToken;
        }

        [SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times", Justification = "MemoryStream is safe for multi-dispose.")]
        public static string Serialize(AntiForgeryToken token)
        {
            Contract.Assert(token != null);

            using (MemoryStream stream = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                writer.Write(TokenVersion);
                writer.Write(token.SecurityToken.GetData());
                writer.Write(token.IsSessionToken);

                if (!token.IsSessionToken)
                {
                    writer.Write(token.Username);
                    writer.Write(token.AdditionalData);
                }

                writer.Flush();
                return CryptoSystem.Protect(stream.ToArray());
            }
        }
    }
}