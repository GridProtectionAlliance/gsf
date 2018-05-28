//******************************************************************************************************
//  CipherEngine.cs - Gbtc
//
//  Copyright © 2018, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  05/27/2018 - Steven E. Chisholm
//       Generated original version of source code.
//
//******************************************************************************************************

namespace GSF.Security.Cryptography.X509
{
    /// <summary>
    /// The cipher engine to use to create the certificate.
    /// </summary>
    public enum CipherEngine
    {
        /// <summary>
        /// Uses the <see cref="RSACryptoServiceProvider"/>. This is the only version supported by MONO.
        /// </summary>
        RSACryptoServiceProvider,
        /// <summary>
        /// Uses the <see cref="RSACng"/>. Requires a modern Windows OS.
        /// </summary>
        RSACng,
        /// <summary>
        /// Uses the <see cref="ECDsaCng"/>. Requires a modern Windows OS.
        /// </summary>
        ECDsaCng,
    }
}