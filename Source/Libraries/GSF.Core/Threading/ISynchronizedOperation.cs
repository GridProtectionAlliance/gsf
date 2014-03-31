//******************************************************************************************************
//  ISynchronizedOperation.cs - Gbtc
//
//  Copyright © 2014, Grid Protection Alliance.  All Rights Reserved.
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
//  03/21/2014 - Stephen C. Wills
//       Generated original version of source code.
//
//******************************************************************************************************

namespace GSF.Threading
{
    /// <summary>
    /// Represents an operation that cannot run while it is already in progress.
    /// </summary>
    public interface ISynchronizedOperation
    {
        /// <summary>
        /// Gets a value to indicate whether the synchronized
        /// operation is currently executing its action.
        /// </summary>
        bool IsRunning
        {
            get;
        }

        /// <summary>
        /// Executes the action on this thread or marks the
        /// operation as pending if the operation is already running.
        /// </summary>
        /// <remarks>
        /// When the operation is marked as pending, it will run again after the
        /// operation that is currently running has completed. This is useful if
        /// an update has invalidated the operation that is currently running and
        /// will therefore need to be run again.
        /// </remarks>
        void RunOnce();

        /// <summary>
        /// Executes the action on another thread or marks the
        /// operation as pending if the operation is already running.
        /// </summary>
        /// <remarks>
        /// When the operation is marked as pending, it will run again after the
        /// operation that is currently running has completed. This is useful if
        /// an update has invalidated the operation that is currently running and
        /// will therefore need to be run again.
        /// </remarks>
        void RunOnceAsync();

        /// <summary>
        /// Attempts to execute the action on this thread.
        /// Does nothing if the operation is already running.
        /// </summary>
        void TryRun();

        /// <summary>
        /// Attempts to execute the action on another thread.
        /// Does nothing if the operation is already running.
        /// </summary>
        void TryRunAsync();
    }
}
