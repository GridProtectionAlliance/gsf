//******************************************************************************************************
//  ICancellationToken.cs - Gbtc
//
//  Copyright © 2016, Grid Protection Alliance.  All Rights Reserved.
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
//  02/02/2016 - Stephen C. Wills
//       Generated original version of source code.
//  09/13/2016 - J. Ritchie Carroll
//       Added Cancelled property as an interface requirement.
//
//******************************************************************************************************

namespace GSF.Threading
{
    /// <summary>
    /// Represents a token that can be used to cancel an asynchronous operation.
    /// </summary>
    public interface ICancellationToken
    {
        /// <summary>
        /// Gets a value that indicates whether the operation has been cancelled.
        /// </summary>
        bool IsCancelled
        {
            get;
        }

        /// <summary>
        /// Cancels the operation.
        /// </summary>
        /// <returns><c>true</c> if the operation was cancelled; otherwise <c>false</c>.</returns>
        bool Cancel();
    }
}