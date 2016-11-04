//******************************************************************************************************
//  WindowsAPI.cs - Gbtc
//
//  Copyright © 2013, Grid Protection Alliance.  All Rights Reserved.
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
//  12/20/2013 - Stephen C. Wills
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Runtime.InteropServices;

namespace NoInetFixUtil
{
    internal class WindowsApi
    {
        public const int CRYPT_OID_INFO_OID_KEY = 1;
        public const int CRYPT_OID_INFO_NAME_KEY = 2;
        public const uint CRYPT_OID_DISABLE_SEARCH_DS_FLAG = 0x80000000u;
        public const int CRYPT_INSTALL_OID_INFO_BEFORE_FLAG = 1;

        /// <summary>
        /// Win32 CRYPTOAPI_BLOB structure.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct CRYPTOAPI_BLOB
        {
            public int cbData;
            public IntPtr pbData;
        }

        /// <summary>
        /// Win32 CRYPT_OID_INFO class.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public class CRYPT_OID_INFO
        {
            public int cbSize;

            [MarshalAs(UnmanagedType.LPStr)]
            public string pszOID;

            [MarshalAs(UnmanagedType.LPWStr)]
            public string pwszName;

            public int dwGroupId;
            public int dwValueOrAlgidordwLength;
            public CRYPTOAPI_BLOB ExtraInfo;

            [MarshalAs(UnmanagedType.LPWStr)]
            public string pwszCNGAlgid;

            [MarshalAs(UnmanagedType.LPWStr)]
            public string pwszCNGExtraAlgid;
        }

        /// <summary>
        /// Win32 CryptFindOIDInfo function.
        /// </summary>
        [DllImport("crypt32.dll", SetLastError = true)]
        public static extern IntPtr CryptFindOIDInfo(int dwKeyType, string pvKey, uint dwGroupId);

        /// <summary>
        /// Win32 CryptRegisterOIDInfo function.
        /// </summary>
        [DllImport("crypt32.dll", SetLastError = true)]
        public static extern bool CryptRegisterOIDInfo(IntPtr pInfo, int dwFlags);

        /// <summary>
        /// Win32 CryptUnregisterOIDInfo function.
        /// </summary>
        [DllImport("crypt32.dll", SetLastError = true)]
        public static extern bool CryptUnregisterOIDInfo(IntPtr pInfo);
    }
}
