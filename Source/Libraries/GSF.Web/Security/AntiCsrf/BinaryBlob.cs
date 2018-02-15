//******************************************************************************************************
//  BinaryBlob.cs - Gbtc
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

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;
using Random = GSF.Security.Cryptography.Random;

namespace GSF.Web.Security.AntiCsrf
{
    // Represents a binary blob (token) that contains random data.
    // Useful for binary data inside a serialized stream.
    [DebuggerDisplay("{" + nameof(DebuggerString) + "}")]
    internal sealed class BinaryBlob : IEquatable<BinaryBlob>
    {
        private readonly byte[] m_data;

        // Generates a new token using a specified bit length.
        public BinaryBlob(int bitLength) : this(bitLength, GenerateNewToken(bitLength))
        {
        }

        // Generates a token using an existing binary value.
        public BinaryBlob(int bitLength, byte[] data)
        {
            if (bitLength < 32 || bitLength % 8 != 0)
                throw new ArgumentOutOfRangeException(nameof(bitLength));

            if (data == null || data.Length != bitLength / 8)
                throw new ArgumentOutOfRangeException(nameof(data));

            m_data = data;
        }

        public int BitLength => checked(m_data.Length * 8);

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Called by debugger.")]
        private string DebuggerString
        {
            get
            {
                StringBuilder sb = new StringBuilder("0x", 2 + m_data.Length * 2);

                foreach (byte t in m_data)
                    sb.AppendFormat(CultureInfo.InvariantCulture, "{0:x2}", t);

                return sb.ToString();
            }
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as BinaryBlob);
        }

        public bool Equals(BinaryBlob other)
        {
            if (other == null)
                return false;

            //Contract.Assert(m_data.Length == other.m_data.Length);
            return m_data.CompareTo(other.m_data) == 0;
        }

        public byte[] GetData()
        {
            return m_data;
        }

        public override int GetHashCode()
        {
            // Since data should contain uniformly-distributed entropy, the
            // first 32 bits can serve as the hash code.
            //Contract.Assert(m_data != null && m_data.Length >= 32 / 8);
            return BitConverter.ToInt32(m_data, 0);
        }

        private static byte[] GenerateNewToken(int bitLength)
        {
            byte[] data = new byte[bitLength / 8];
            Random.GetBytes(data);
            return data;
        }
    }
}