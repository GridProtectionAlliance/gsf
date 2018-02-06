//******************************************************************************************************
//  ChildProcessManager.cs - Gbtc
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
//  02/05/2018 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace GSF.Diagnostics
{
    /// <summary>
    /// Represents a manager for automatically terminating child processes.
    /// </summary>
    public sealed class ChildProcessManager : IDisposable
    {
        #region [ Members ]

        // Nested Types

        // ReSharper disable FieldCanBeMadeReadOnly.Local
        // ReSharper disable UnusedMember.Local
        // ReSharper disable InconsistentNaming
        // ReSharper disable MemberCanBePrivate.Local
        [StructLayout(LayoutKind.Sequential)]
        private struct IO_COUNTERS
        {
            public ulong ReadOperationCount;
            public ulong WriteOperationCount;
            public ulong OtherOperationCount;
            public ulong ReadTransferCount;
            public ulong WriteTransferCount;
            public ulong OtherTransferCount;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct JOBOBJECT_BASIC_LIMIT_INFORMATION
        {
            public long PerProcessUserTimeLimit;
            public long PerJobUserTimeLimit;
            public uint LimitFlags;
            public UIntPtr MinimumWorkingSetSize;
            public UIntPtr MaximumWorkingSetSize;
            public uint ActiveProcessLimit;
            public UIntPtr Affinity;
            public uint PriorityClass;
            public uint SchedulingClass;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct SECURITY_ATTRIBUTES
        {
            public uint nLength;
            public IntPtr lpSecurityDescriptor;
            public int bInheritHandle;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct JOBOBJECT_EXTENDED_LIMIT_INFORMATION
        {
            public JOBOBJECT_BASIC_LIMIT_INFORMATION BasicLimitInformation;
            public IO_COUNTERS IoInfo;
            public UIntPtr ProcessMemoryLimit;
            public UIntPtr JobMemoryLimit;
            public UIntPtr PeakProcessMemoryUsed;
            public UIntPtr PeakJobMemoryUsed;
        }

        private enum JobObjectInfoType
        {
            AssociateCompletionPortInformation = 7,
            BasicLimitInformation = 2,
            BasicUIRestrictions = 4,
            EndOfJobTimeInformation = 6,
            ExtendedLimitInformation = 9,
            SecurityLimitInformation = 5,
            GroupInformation = 11
        }
        // ReSharper restore FieldCanBeMadeReadOnly.Local
        // ReSharper restore UnusedMember.Local
        // ReSharper restore InconsistentNaming
        // ReSharper restore MemberCanBePrivate.Local

        private sealed class SafeJobHandle : SafeHandleZeroOrMinusOneIsInvalid
        {
            public SafeJobHandle(IntPtr handle) : base(true)
            {
                SetHandle(handle);
            }

            protected override bool ReleaseHandle()
            {
                return CloseHandle(handle);
            }
        }

        // Events

        /// <summary>
        /// Raised when there is an exception while attempting to terminate child process.
        /// </summary>
        /// <remarks>
        /// This is currently only raised on non-Windows operating systems.
        /// </remarks>
        public event EventHandler<EventArgs<Exception>> TerminationException;

        // Fields
        private readonly List<WeakReference<Process>> m_childProcesses;
        private SafeJobHandle m_jobHandle;
        private bool m_disposed;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="ChildProcessManager"/>.
        /// </summary>
        public ChildProcessManager()
        {
            if (Common.IsPosixEnvironment)
            {
                // On non-Windows operating systems we just track associated processes
                m_childProcesses = new List<WeakReference<Process>>();
            }
            else
            {
                // Let safe handle manage terminations on Windows
                GC.SuppressFinalize(this);

                // On Windows we add child processes to a job object such that when the job
                // is terminated, so are the child processes. Since safe handle ensures proper
                // closing of job handle, child processes will be terminated even if parent 
                // process is abnormally terminated
                m_jobHandle = new SafeJobHandle(CreateJobObject(IntPtr.Zero, null));

                JOBOBJECT_BASIC_LIMIT_INFORMATION info = new JOBOBJECT_BASIC_LIMIT_INFORMATION
                {
                    LimitFlags = 0x2000
                };

                JOBOBJECT_EXTENDED_LIMIT_INFORMATION extendedInfo = new JOBOBJECT_EXTENDED_LIMIT_INFORMATION
                {
                    BasicLimitInformation = info
                };

                int length = Marshal.SizeOf(typeof(JOBOBJECT_EXTENDED_LIMIT_INFORMATION));

                IntPtr extendedInfoPtr = Marshal.AllocHGlobal(length);
                Marshal.StructureToPtr(extendedInfo, extendedInfoPtr, false);

                if (!SetInformationJobObject(m_jobHandle, JobObjectInfoType.ExtendedLimitInformation, extendedInfoPtr, (uint)length))
                    throw new InvalidOperationException($"Unable to set information for ChildProcessManager job. Error: {Marshal.GetLastWin32Error()}");
            }
        }

        /// <summary>
        /// Make sure child processes get disposed.
        /// </summary>
        ~ChildProcessManager()
        {
            Dispose();
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Releases all the resources used by the <see cref="ChildProcessManager"/> object.
        /// </summary>
        public void Dispose()
        {
            if (m_disposed)
                return;

            try
            {
                if (Common.IsPosixEnvironment)
                {
                    foreach (WeakReference<Process> childProcessReference in m_childProcesses)
                    {
                        Process childProcess;

                        if (!childProcessReference.TryGetTarget(out childProcess))
                            continue;

                        try
                        {
                            childProcess.Kill();
                        }
                        catch (Exception ex)
                        {
                            TerminationException?.Invoke(this, new EventArgs<Exception>(ex));
                        }
                    }
                }
                else
                {
                    m_jobHandle?.Dispose();
                    m_jobHandle = null;
                }
            }
            finally
            {
                m_disposed = true;
            }
        }

        /// <summary>
        /// Associates the specified <paramref name="process"/> as a child of this <see cref="ChildProcessManager"/> instance.
        /// </summary>
        /// <param name="process">The <see cref="Process"/> to associate.</param>
        /// <remarks>
        /// <para>
        /// The <paramref name="process"/> will be managed as an associated process of this <see cref="ChildProcessManager"/>
        /// instance. When this <see cref="ChildProcessManager"/> instance is disposed or garbage collected, the children
        /// processes will be terminated.
        /// </para>
        /// <para>
        /// Creating an instance of this class with lifetime scope of the executing application will cause any child processes
        /// to be terminated when the parent process shuts down, on Windows environments this will happen even when the parent
        /// process termination is abnormal.
        /// </para>
        /// </remarks>
        public void AddProcess(Process process)
        {
            if (m_disposed)
                throw new ObjectDisposedException(nameof(ChildProcessManager));

            if (Common.IsPosixEnvironment)
            {
                m_childProcesses.Add(new WeakReference<Process>(process));
            }
            else
            {
                if (!AssignProcessToJobObject(m_jobHandle, process.SafeHandle))
                    throw new InvalidOperationException($"Unable to add process to ChildProcessManager job. Error: {Marshal.GetLastWin32Error()}");
            }
        }

        #endregion

        #region [ Static ]

        // Static Methods

        // ReSharper disable InconsistentNaming
        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        private static extern IntPtr CreateJobObject(IntPtr hObject, string lpName);

        [DllImport("kernel32", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetInformationJobObject(SafeJobHandle jobHandle, JobObjectInfoType infoType, IntPtr lpJobObjectInfo, uint cbJobObjectInfoLength);

        [DllImport("kernel32", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool AssignProcessToJobObject(SafeJobHandle jobHandle, SafeProcessHandle process);

        [DllImport("kernel32")]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CloseHandle(IntPtr hObject);
        // ReSharper restore InconsistentNaming

        #endregion
    }
}
