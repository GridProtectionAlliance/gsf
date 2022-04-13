//******************************************************************************************************
//  FtpTransferResult.cs - Gbtc
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
//  05/12/2020 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System.Collections;

namespace GSF.Net.VirtualFtpClient;

/// <summary>
/// Represents a virtual FTP transfer result for the specified target <see cref="FtpType"/>.
/// </summary>
public class FtpTransferResult
{
    /// <summary>
    /// FTP transfer result completed index.
    /// </summary>
    public const int Complete = 0;

    /// <summary>
    /// FTP transfer result failed index.
    /// </summary>
    public const int Fail = 1;

    /// <summary>
    /// FTP transfer result aborted index.
    /// </summary>
    public const int Abort = 2;

    private readonly BitArray m_result;

    internal FtpTransferResult(string message, int responseCode, int result)
    {
        m_result = new BitArray(3);
        Message = message;
        ResponseCode = responseCode;
        m_result[result] = true;
    }

    /// <summary>
    /// Returns true if asynchronous transfer completed successfully.
    /// </summary>
    public bool IsSuccess => m_result[Complete];

    /// <summary>
    /// Returns true if asynchronous transfer failed.
    /// </summary>
    public bool IsFailed => m_result[Fail];

    /// <summary>
    /// Returns true if asynchronous transfer was aborted.
    /// </summary>
    public bool IsAborted => m_result[Abort];

    /// <summary>
    /// Gets any message associated with transfer.
    /// </summary>
    public string Message { get; }

    /// <summary>
    /// Gets response code from transfer.
    /// </summary>
    public int ResponseCode { get; }

    internal static int GetResult(bool isSuccess, bool isFailed, bool isAborted) => 
        isSuccess ? Complete : isFailed ? Fail : isAborted ? Abort : Complete;
}