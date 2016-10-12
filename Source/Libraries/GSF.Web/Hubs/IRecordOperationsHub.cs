//******************************************************************************************************
//  IRecordOperationsHub.cs - Gbtc
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
//  03/14/2016 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using GSF.Web.Model;

namespace GSF.Web.Hubs
{
    /// <summary>
    /// Defines an interface for SignalR hubs to indicate support for record operations.
    /// </summary>
    public interface IRecordOperationsHub : IDisposable
    {
        /// <summary>
        /// Gets <see cref="RecordOperationsCache"/> for SignalR hub.
        /// </summary>
        /// <remarks>
        /// Implementors should statically cache instance since reflected cache content will not change.
        /// </remarks>
        RecordOperationsCache RecordOperationsCache { get; }

        /// <summary>
        /// Gets <see cref="Model.DataContext"/> created for this <see cref="IRecordOperationsHub"/> implementation.
        /// </summary>
        DataContext DataContext { get; }

        /// <summary>
        /// Gets active connection ID from current hub context or assigns one to use.
        /// </summary>
        string ConnectionID { get; set;}
    }
}
