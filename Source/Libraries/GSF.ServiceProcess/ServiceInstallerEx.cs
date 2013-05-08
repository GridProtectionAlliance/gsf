//******************************************************************************************************
//  ServiceInstallerEx.cs - Gbtc
//
//  Copyright © 2012, Grid Protection Alliance.  All Rights Reserved.
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
//  02/04/2011 - J. Ritchie Carroll
//       Initial version of source integrated into GSF Code Library.
//  09/21/2011 - J. Ritchie Carroll
//       Excluded class from Mono deployments due to P/Invoke requirements.
//  12/20/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Configuration.Install;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using System.Threading;
using GSF.Interop;

namespace GSF.ServiceProcess
{
#if !MONO
    #region [ Enumerations ]

    /// <summary>
    /// Defines the recover action to be performed upon service failure.
    /// </summary>
    /// <remarks>
    /// Enum values correspond to Win32 equivalents.
    /// </remarks>
    public enum RecoverAction
    {
        /// <summary>
        /// No action.
        /// </summary>
        None = 0,
        /// <summary>
        /// Restart the service.
        /// </summary>
        Restart = 1,
        /// <summary>
        /// Reboot the computer.
        /// </summary>
        Reboot = 2,
        /// <summary>
        /// Run a command.
        /// </summary>
        RunCommand = 3
    }

    #endregion

    /// <summary>
    /// Defines an extended service installer class that can define service failure actions at install time.
    /// </summary>
    public class ServiceInstallerEx : ServiceInstaller
    {
    #region [ Members ]

        // Constants
        private const int SC_MANAGER_ALL_ACCESS = 0xF003F;
        private const int SERVICE_ALL_ACCESS = 0xF01FF;
        private const int SERVICE_CONFIG_FAILURE_ACTIONS = 0x2;
        private const int SERVICE_CONFIG_FAILURE_ACTIONS_FLAG = 0x4;

        // Fields
        private readonly List<WindowsApi.SC_ACTION> m_failureActions;    // Service failure actions
        private int m_failResetPeriod = Timeout.Infinite;       // Service fail count reset time
        private string m_failRebootMessage = "";			    // Service reboot message
        private string m_failRunCommand = "";				    // Service fail run command
        private bool m_executeActionsOnNonCrashErrors = true;   // Windows 2008 check box to restart on exit with error
        private bool m_startOnInstall;
        private int m_startTimeout = 15000;
        private readonly string m_logMessagePrefix;

        #endregion

    #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="ServiceInstallerEx"/>.
        /// </summary>
        public ServiceInstallerEx()
        {
            // Initialize the failure actions and register for the Committed event
            m_failureActions = new List<WindowsApi.SC_ACTION>();

            // Register the event handlers for post install operations
            base.Committed += UpdateServiceConfig;
            base.Committed += StartIfNeeded;

            // Set the log message prefix
            m_logMessagePrefix = base.ServiceName + " - ServiceInstallerEx: ";
        }

        #endregion

    #region [ Properties ]

        /// <summary>
        /// Sets the time after which to reset the failure count to zero if there are no failures, in seconds.
        /// Specify <see cref="Timeout.Infinite"/> to indicate that this value should never be reset.
        /// </summary>
        public int FailResetPeriod
        {
            set
            {
                if (value < 0)
                    m_failResetPeriod = Timeout.Infinite;
                else
                    m_failResetPeriod = value;
            }
        }

        /// <summary>
        /// Sets the service fail reboot message.
        /// </summary>
        public string FailRebootMessage
        {
            set
            {
                m_failRebootMessage = value;
            }
        }

        /// <summary>
        /// Sets the service fail run command.
        /// </summary>
        public string FailRunCommand
        {
            set
            {
                m_failRunCommand = value;
            }
        }

        /// <summary>
        /// Sets flag that determines when failure actions are to be executed.
        /// </summary>
        /// <remarks>
        /// <para>
        /// If this member is <c>true</c> and the service has configured failure actions, the failure actions are queued if the service process
        /// terminates without reporting a status of SERVICE_STOPPED or if it enters the SERVICE_STOPPED state but the exit code returned in the
        /// SERVICE_STATUS structure is not ERROR_SUCCESS (0).
        /// </para>
        /// <para>
        /// If this member is <c>false</c> and the service has configured failure actions, the failure actions are queued only if the service
        /// terminates without reporting a status of SERVICE_STOPPED.
        /// </para>
        /// <para>
        /// This setting is ignored unless the service has configured failure actions.
        /// </para>
        /// </remarks>
        public bool ExecuteActionsOnNonCrashErrors
        {
            set
            {
                m_executeActionsOnNonCrashErrors = value;
            }

        }

        /// <summary>
        /// Sets the boolean value to configure the service to start after it is installed.
        /// </summary>
        public bool StartOnInstall
        {
            set
            {
                m_startOnInstall = value;
            }
        }

        /// <summary>
        /// Sets the service start timeout in milliseconds.
        /// </summary>
        public int StartTimeout
        {
            set
            {
                m_startTimeout = value;
            }
        }

        #endregion

    #region [ Methods ]

        /// <summary>
        /// Defines a new recover action to be performed upon service failure.
        /// </summary>
        /// <param name="recoverAction"><see cref="RecoverAction"/> to execute upon service failure.</param>
        /// <param name="delay">The time to wait before performing the specified <paramref name="recoverAction"/>, in milliseconds.</param>
        public void DefineRecoverAction(RecoverAction recoverAction, int delay)
        {
            m_failureActions.Add(new WindowsApi.SC_ACTION { Type = (WindowsApi.SC_ACTION_TYPE)(uint)recoverAction, Delay = delay });
        }

        // The worker method to set all the extension properties for the service
        private void UpdateServiceConfig(object sender, InstallEventArgs e)
        {
            int actionCount = m_failureActions.Count;

            // Do we need to do any work that the base installer did not do already?
            if (actionCount == 0)
                return;

            // We've got work to do
            IntPtr serviceManagerHandle = IntPtr.Zero;
            IntPtr serviceHandle = IntPtr.Zero;
            IntPtr serviceLockHandle = IntPtr.Zero;
            IntPtr actionsPtr = IntPtr.Zero;
            IntPtr failureActionsPtr = IntPtr.Zero;

            // Err check var
            bool result = false;

            // Place all our code in a try block
            try
            {
                // Open the service control manager
                serviceManagerHandle = WindowsApi.OpenSCManager(null, null, SC_MANAGER_ALL_ACCESS);

                if (serviceManagerHandle.ToInt32() <= 0)
                {
                    LogInstallMessage(EventLogEntryType.Error, m_logMessagePrefix + "Failed to Open Service Control Manager");
                    return;
                }

                // Lock the Service Database
                serviceLockHandle = WindowsApi.LockServiceDatabase(serviceManagerHandle);

                if (serviceLockHandle.ToInt32() <= 0)
                {
                    LogInstallMessage(EventLogEntryType.Error, m_logMessagePrefix + "Failed to Lock Service Database for Write");
                    return;
                }

                // Open the service
                serviceHandle = WindowsApi.OpenService(serviceManagerHandle, base.ServiceName, SERVICE_ALL_ACCESS);

                if (serviceHandle.ToInt32() <= 0)
                {
                    LogInstallMessage(EventLogEntryType.Information, m_logMessagePrefix + "Failed to Open Service ");
                    return;
                }

                // Need to set service failure actions. Note that the API lets us set as many as
                // we want, yet the Service Control Manager GUI only lets us see the first 3.
                // Bill is aware of this and has promised no fixes. Also note that the API allows
                // granularity of seconds whereas GUI only shows days and minutes.

                // Calculate size of SC_ACTION structure
                int scActionSize = Marshal.SizeOf(typeof(WindowsApi.SC_ACTION));
                bool needShutdownPrivilege = false;

                // Allocate memory for the array of individual actions
                actionsPtr = Marshal.AllocHGlobal(scActionSize * actionCount);

                // Set up the restart actions array by copying in each failure action structure
                for (int i = 0; i < actionCount; i++)
                {
                    // Handle pointer math as 64-bit, cast will convert back to 32-bit if needed
                    Marshal.StructureToPtr(m_failureActions[i], (IntPtr)((Int64)actionsPtr + i * scActionSize), false);

                    if (m_failureActions[i].Type == WindowsApi.SC_ACTION_TYPE.SC_ACTION_REBOOT)
                        needShutdownPrivilege = true;
                }


                // If we need shutdown privilege, then grant it to this process
                if (needShutdownPrivilege)
                {
                    result = GrantShutdownPrivilege();

                    if (!result)
                        return;
                }

                // Set up the failure actions
                WindowsApi.SERVICE_FAILURE_ACTIONS failureActions = new WindowsApi.SERVICE_FAILURE_ACTIONS();
                failureActions.cActions = actionCount;
                failureActions.dwResetPeriod = m_failResetPeriod;
                failureActions.lpCommand = m_failRunCommand;
                failureActions.lpRebootMsg = m_failRebootMessage;
                failureActions.lpsaActions = actionsPtr;

                failureActionsPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(WindowsApi.SERVICE_FAILURE_ACTIONS)));
                Marshal.StructureToPtr(failureActions, failureActionsPtr, false);

                // Make the change
                result = WindowsApi.ChangeServiceConfig2(serviceHandle, SERVICE_CONFIG_FAILURE_ACTIONS, failureActionsPtr);
                    
                // Check the return
                if (!result)
                {
                    int err = WindowsApi.GetLastError();

                    if (err == WindowsApi.ERROR_ACCESS_DENIED)
                        throw new Exception(m_logMessagePrefix + "Access Denied while setting service failure actions");
                }

                LogInstallMessage(EventLogEntryType.Information, m_logMessagePrefix + "Successfully configured service failure actions");

                // Failure actions flag only applies on Vista / 2008 or better
                if (Environment.OSVersion.Platform == PlatformID.Win32NT && Environment.OSVersion.Version.Major >= 6)
                {
                    WindowsApi.SERVICE_FAILURE_ACTIONS_FLAG failureActionFlag = new WindowsApi.SERVICE_FAILURE_ACTIONS_FLAG();
                    failureActionFlag.bFailureAction = m_executeActionsOnNonCrashErrors;
                    result = WindowsApi.ChangeServiceConfig2(serviceHandle, SERVICE_CONFIG_FAILURE_ACTIONS_FLAG, ref failureActionFlag);

                    // Error setting description?
                    if (!result)
                        throw new Exception(m_logMessagePrefix + "Failed to set failure actions flag");
                }
            }
            // Catch all exceptions
            catch (Exception ex)
            {
                LogInstallMessage(EventLogEntryType.Error, ex.Message);
            }
            finally
            {
                if (serviceManagerHandle != IntPtr.Zero)
                {
                    // Unlock the service database
                    if (serviceLockHandle != IntPtr.Zero)
                        WindowsApi.UnlockServiceDatabase(serviceLockHandle);

                    // Close the service control manager handle
                    WindowsApi.CloseServiceHandle(serviceManagerHandle);
                }

                // Close the service handle
                if (serviceHandle != IntPtr.Zero)
                    WindowsApi.CloseServiceHandle(serviceHandle);

                // Free allocated memory
                if (failureActionsPtr != IntPtr.Zero)
                    Marshal.FreeHGlobal(failureActionsPtr);

                if (actionsPtr != IntPtr.Zero)
                    Marshal.FreeHGlobal(actionsPtr);
            }
        }

        // This code mimics the MSDN defined way to adjust privilege for shutdown
        // http://msdn.microsoft.com/library/default.asp?url=/library/en-us/sysinfo/base/shutting_down.asp
        private bool GrantShutdownPrivilege()
        {
            bool grantSuccess = false;
            IntPtr processToken = IntPtr.Zero;
            IntPtr processHandle = IntPtr.Zero;
            WindowsApi.TOKEN_PRIVILEGES tokenPrivileges = new WindowsApi.TOKEN_PRIVILEGES();
            long luid = 0;
            int returnLen = 0;

            try
            {
                processHandle = WindowsApi.GetCurrentProcess();

                bool result = WindowsApi.OpenProcessToken(processHandle, WindowsApi.TOKEN_ADJUST_PRIVILEGES | WindowsApi.TOKEN_QUERY, ref processToken);

                if (!result)
                    return grantSuccess;

                WindowsApi.LookupPrivilegeValue(null, WindowsApi.SE_SHUTDOWN_NAME, ref luid);

                tokenPrivileges.PrivilegeCount = 1;
                tokenPrivileges.Privileges.Luid = luid;
                tokenPrivileges.Privileges.Attributes = WindowsApi.SE_PRIVILEGE_ENABLED;

                result = WindowsApi.AdjustTokenPrivileges(processToken, false, ref tokenPrivileges, 0, IntPtr.Zero, ref returnLen);

                if (WindowsApi.GetLastError() != 0)
                    throw new Exception("Failed to grant shutdown privilege");

                grantSuccess = true;

            }
            catch (Exception ex)
            {
                LogInstallMessage(EventLogEntryType.Error, m_logMessagePrefix + ex.Message);
            }
            finally
            {
                if (processToken != IntPtr.Zero)
                    WindowsApi.CloseHandle(processToken);
            }

            return grantSuccess;
        }

        // Method to start the service automatically after installation
        private void StartIfNeeded(object sender, InstallEventArgs e)
        {
            // Do we need to do any work?
            if (!m_startOnInstall)
                return;

            try
            {
                TimeSpan waitTo = new TimeSpan(0, 0, m_startTimeout);

                // Get a handle to the service
                ServiceController sc = new ServiceController(base.ServiceName);

                // Start the service and wait for it to start
                sc.Start();
                sc.WaitForStatus(ServiceControllerStatus.Running, waitTo);

                // Be good and release our handle
                sc.Close();

                LogInstallMessage(EventLogEntryType.Information, m_logMessagePrefix + " Service Started");

            }
            // Catch all exceptions
            catch (Exception ex)
            {
                LogInstallMessage(EventLogEntryType.Error, m_logMessagePrefix + ex.Message);
            }
        }

        // Method to log to console and event log
        private void LogInstallMessage(EventLogEntryType logLevel, string msg)
        {
            System.Console.WriteLine(msg);

            try
            {
                EventLog.WriteEntry(base.ServiceName, msg, logLevel);
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex.ToString());
            }
        }

        #endregion
    }
#endif
}