//******************************************************************************************************
//  KillOnReleaseJob.cs - Gbtc
//
//  Copyright © 2026, Grid Protection Alliance.  All Rights Reserved.
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
//  07/24/2026 - Stephen C. Wills
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using GSF.Interop;

namespace GSF.Diagnostics
{
    /// <summary>
    /// Represents a job that kills all assigned processes when the job is disposed.
    /// </summary>
    public class KillOnReleaseJob : SafeHandle
    {
        /// <summary>
        /// Creates a new job handle and sets JOB_OBJECT_LIMIT_KILL_ON_JOB_CLOSE.
        /// </summary>
        public KillOnReleaseJob()
            : base(IntPtr.Zero, true)
        {
            IntPtr jobHandle = WindowsApi.CreateJobObject(IntPtr.Zero, null);
            SetHandle(jobHandle);
            KillOnJobClose();
        }

        /// <summary>
        /// Gets a flag that indicates whether the job handle is invalid.
        /// </summary>
        public override bool IsInvalid => handle == IntPtr.Zero;

        /// <summary>
        /// Assigns a process to the job that will be killed when the job is disposed.
        /// </summary>
        /// <param name="process"></param>
        public void AssignProcess(Process process)
        {
            WindowsApi.AssignProcessToJobObject(handle, process.Handle);
        }

        /// <summary>
        /// Releases the job handle, killing all assigned processes.
        /// </summary>
        /// <returns>True if the job handle was successfully released.</returns>
        protected override bool ReleaseHandle()
        {
            return IsInvalid || WindowsApi.CloseHandle(handle);
        }

        private void KillOnJobClose()
        {
            WindowsApi.JOBOBJECT_BASIC_LIMIT_INFORMATION info = new()
            {
                LimitFlags = 0x2000
            };

            WindowsApi.JOBOBJECT_EXTENDED_LIMIT_INFORMATION extendedInfo = new()
            {
                BasicLimitInformation = info
            };

            int length = Marshal.SizeOf(typeof(WindowsApi.JOBOBJECT_EXTENDED_LIMIT_INFORMATION));
            IntPtr extendedInfoPtr = Marshal.AllocHGlobal(length);

            try
            {
                Marshal.StructureToPtr(extendedInfo, extendedInfoPtr, false);
                WindowsApi.SetInformationJobObject(handle, WindowsApi.JobObjectInfoType.ExtendedLimitInformation, extendedInfoPtr, (uint)length);
            }
            finally
            {
                Marshal.FreeHGlobal(extendedInfoPtr);
            }
        }
    }
}
