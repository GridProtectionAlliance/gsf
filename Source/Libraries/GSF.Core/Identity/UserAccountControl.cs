//******************************************************************************************************
//  UserAccountControl.cs - Gbtc
//
//  Copyright © 2012, Grid Protection Alliance.  All Rights Reserved.
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
//  02/18/2011 - J. Ritchie Carroll
//       Generated original version of source code.
//  09/21/2011 - J. Ritchie Carroll
//       Excluded class from Mono deployments due to P/Invoke requirements.
//  12/14/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

// These functions are strictly for Windows, so we don't even have the class show up in Mono for now
// TODO: Can test if this works under Mono on Windows then just exit code when not POSIX

#if !MONO
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Security.Principal;
using GSF.Interop;
using Microsoft.Win32;

namespace GSF.Identity
{
    /// <summary>
    /// Provides facilities for enabling and disabling User Account Control (UAC), determining elevation and virtualization status, and launching a process under elevated credentials.
    /// </summary>
    public static class UserAccountControl
    {
        private const string UacRegistryKey = "Software\\Microsoft\\Windows\\CurrentVersion\\Policies\\System";
        private const string UacRegistryValue = "EnableLUA";

        /// <summary>
        /// Creates a process under the elevated token, regardless of UAC settings or the manifest associated with that process.
        /// </summary>
        /// <param name="fileName">The path to the executable file.</param>
        /// <param name="arguments">The command-line arguments to pass to the process.</param>
        /// <returns>A <see cref="Process"/> object representing the newly created process.</returns>
        public static Process CreateProcessAsAdmin(string fileName, string arguments = "")
        {
            ProcessStartInfo startInfo = new(fileName, arguments);
            startInfo.UseShellExecute = true;
            startInfo.Verb = "runas";
            return Process.Start(startInfo);
        }

        /// <summary>
        /// Creates a process under the standard user if the current process is elevated.  The identity of the standard user is
        /// determined by retrieving the user token of the currently running Explorer (shell) process.
        /// </summary>
        /// <param name="fileName">The path to the executable file.</param>
        /// <param name="arguments">The command-line arguments to pass to the process.</param>
        /// <returns>A <see cref="Process"/> object representing the newly created process.</returns>
        /// <remarks>
        /// <para>
        /// This method requires administrative privileges. An exception will be thrown if the current user is not elevated.
        /// </para>
        /// <para>
        /// This is an especially useful function if you are trying to shell an application from an installation program. With UAC
        /// enabled, an application spawned from a setup program will be the "NT AUTHORITY\SYSTEM" user - not the local user that
        /// executed the installer; this can wreak havoc if the spawned application needs to authenticate the local user.
        /// </para>
        /// </remarks>
        [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults")]
        public static Process CreateProcessAsStandardUser(string fileName, string arguments = "")
        {
            // The following implementation is roughly based on Aaron Margosis' post:
            // http://blogs.msdn.com/aaron_margosis/archive/2009/06/06/faq-how-do-i-start-a-program-as-the-desktop-user-from-an-elevated-app.aspx

            IntPtr hProcessToken = IntPtr.Zero;
            IntPtr hShellProcess = IntPtr.Zero;
            IntPtr hShellProcessToken = IntPtr.Zero;
            IntPtr hPrimaryToken = IntPtr.Zero;

            try
            {
                if (!WindowsApi.OpenProcessToken(WindowsApi.GetCurrentProcess(), WindowsApi.TOKEN_ADJUST_PRIVILEGES, ref hProcessToken))
                    throw new Win32Exception(WindowsApi.GetLastError());

                // Enable SeIncreaseQuotaPrivilege in this process.  (This requires administrative privileges.)
                WindowsApi.TOKEN_PRIVILEGES tkp = new();
                long luid = 0;
                int returnLen = 0;

                WindowsApi.LookupPrivilegeValue(null, WindowsApi.SE_INCREASE_QUOTA_NAME, ref luid);

                tkp.PrivilegeCount = 1;
                tkp.Privileges.Luid = luid;
                tkp.Privileges.Attributes = WindowsApi.SE_PRIVILEGE_ENABLED;

                WindowsApi.AdjustTokenPrivileges(hProcessToken, false, ref tkp, 0, IntPtr.Zero, ref returnLen);
                int dwLastErr = WindowsApi.GetLastError();

                if (dwLastErr != 0)
                    throw new Win32Exception(dwLastErr);

                // Get window handle representing the desktop shell.  This might not work if there is no shell window, or when
                // using a custom shell.  Also note that we're assuming that the shell is not running elevated.
                IntPtr hShellWnd = WindowsApi.GetShellWindow();

                if (hShellWnd == IntPtr.Zero)
                    throw new InvalidOperationException("Unable to locate shell window. System might be using a custom shell.");

                // Get the ID of the desktop shell process.

                WindowsApi.GetWindowThreadProcessId(hShellWnd, out uint dwShellPID);

                if (dwShellPID == 0)
                    throw new Win32Exception(WindowsApi.GetLastError());

                // Open the desktop shell process in order to get the process token.            
                hShellProcess = WindowsApi.OpenProcess(WindowsApi.ProcessAccessTypes.PROCESS_QUERY_INFORMATION, false, dwShellPID);

                if (hShellProcess == IntPtr.Zero)
                    throw new Win32Exception(WindowsApi.GetLastError());

                // Get the process token of the desktop shell.
                if (!WindowsApi.OpenProcessToken(hShellProcess, WindowsApi.TOKEN_DUPLICATE, ref hShellProcessToken))
                    throw new Win32Exception(WindowsApi.GetLastError());

                // Duplicate the shell's process token to get a primary token.
                const uint dwTokenRights = WindowsApi.TOKEN_QUERY | WindowsApi.TOKEN_ASSIGN_PRIMARY | WindowsApi.TOKEN_DUPLICATE | WindowsApi.TOKEN_ADJUST_DEFAULT | WindowsApi.TOKEN_ADJUST_SESSIONID;

                if (!WindowsApi.DuplicateTokenEx(hShellProcessToken, dwTokenRights, IntPtr.Zero, WindowsApi.SECURITY_IMPERSONATION_LEVEL.SecurityImpersonation, WindowsApi.TOKEN_TYPE.TokenPrimary, out hPrimaryToken))
                    throw new Win32Exception(WindowsApi.GetLastError());

                // Start the target process with the new token.
                WindowsApi.STARTUPINFO si = new();

                si.cb = Marshal.SizeOf(si);

                if (!WindowsApi.CreateProcessWithTokenW(hPrimaryToken, 0, null, fileName + " " + arguments, 0, IntPtr.Zero, null, ref si, out WindowsApi.PROCESS_INFORMATION pi))
                    throw new Win32Exception(WindowsApi.GetLastError());

                WindowsApi.CloseHandle(pi.hProcess);
                WindowsApi.CloseHandle(pi.hThread);

                return Process.GetProcessById(pi.ProcessId);
            }
            finally
            {
                if (hProcessToken != IntPtr.Zero)
                    WindowsApi.CloseHandle(hProcessToken);

                if (hShellProcessToken != IntPtr.Zero)
                    WindowsApi.CloseHandle(hShellProcessToken);

                if (hPrimaryToken != IntPtr.Zero)
                    WindowsApi.CloseHandle(hPrimaryToken);

                if (hShellProcess != IntPtr.Zero)
                    WindowsApi.CloseHandle(hShellProcess);
            }
        }

        /// <summary>
        /// Returns <c>true</c> if the current user has administrator privileges.
        /// </summary>
        /// <remarks>
        /// If UAC is on, then this property will return <c>true</c> even if the current process is not running elevated.
        /// If UAC is off, then this property will return <c>true</c> if the user is part of the built-in <i>Administrators</i> group.
        /// </remarks>
        public static bool IsUserAdmin
        {
            get
            {
                if (IsUacEnabled)
                    return GetProcessTokenElevationType() != WindowsApi.TOKEN_ELEVATION_TYPE.TokenElevationTypeDefault; // Split token

                // If UAC is off, we can't rely on the token; check for Admin group.
                return new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole("Administrators");
            }
        }

        /// <summary>
        /// Returns <c>true</c> if User Account Control (UAC) is enabled on this machine.
        /// </summary>
        /// <remarks>
        /// This value is obtained by checking the LUA registry key. It is possible that the user has not restarted the machine after
        /// enabling/disabling UAC. In that case, the value of the registry key does not reflect the true state of affairs.
        /// </remarks>
        public static bool IsUacEnabled
        {
            // Note: It might be possible to devise a custom solution that would provide a mechanism for tracking whether a restart occurred
            // since UAC settings were changed (using the RunOnce mechanism, temporary files, or volatile registry keys).
            get
            {
                // Check the HKLM\Software\Microsoft\Windows\CurrentVersion\Policies\System\EnableLUA registry value.
                RegistryKey key = Registry.LocalMachine.OpenSubKey(UacRegistryKey, false);

                object value = key?.GetValue(UacRegistryValue);

                return value is not null && value.Equals(1);
            }
        }

        /// <summary>
        /// Returns <c>true</c> if the current process is using UAC virtualization.
        /// </summary>
        /// <remarks>
        /// Under UAC virtualization, file system and registry accesses to specific locations performed by an application are redirected to
        /// provide backwards compatibility. 64-bit applications or applications that have an associated manifest do not enjoy UAC virtualization
        /// because they are assumed to be compatible with Vista and UAC.
        /// </remarks>
        public static bool IsCurrentProcessVirtualized
        {
            get
            {
                IntPtr hToken = IntPtr.Zero;

                try
                {
                    if (!WindowsApi.OpenProcessToken(WindowsApi.GetCurrentProcess(), WindowsApi.TOKEN_QUERY, ref hToken))
                        throw new Win32Exception(WindowsApi.GetLastError());

                    uint virtualizationEnabled = 0;

                    if (!WindowsApi.GetTokenInformation(hToken, WindowsApi.TOKEN_INFORMATION_CLASS.TokenVirtualizationEnabled, ref virtualizationEnabled, sizeof(uint), out uint _))
                        throw new Win32Exception(WindowsApi.GetLastError());

                    return virtualizationEnabled != 0;
                }
                finally
                {
                    if (hToken != IntPtr.Zero)
                        WindowsApi.CloseHandle(hToken);
                }
            }
        }

        ///<summary>
        /// Returns <c>true</c> if the current process is elevated, i.e. if the process went through an elevation consent phase.
        /// </summary>
        /// <remarks>
        /// This property will return <c>false</c> if UAC is disabled and the process is running as admin.  It only determines whether the process
        /// went through the elevation procedure.
        /// </remarks>
        public static bool IsCurrentProcessElevated => 
            GetProcessTokenElevationType() == WindowsApi.TOKEN_ELEVATION_TYPE.TokenElevationTypeFull; // elevated

        /// <summary>
        /// Disables User Account Control by changing the LUA registry key. The changes do not have effect until the system is restarted.
        /// </summary>
        public static void DisableUac() => 
            SetUacRegistryValue(false);

        /// <summary>
        /// Disables User Account Control and restarts the system.
        /// </summary>
        public static void DisableUacAndRestartWindows()
        {
            DisableUac();
            RestartWindows();
        }

        /// <summary>
        /// Enables User Account Control by changing the LUA registry key. The changes do not have effect until the system is restarted.
        /// </summary>
        public static void EnableUac() => 
            SetUacRegistryValue(true);

        /// <summary>
        /// Enables User Account Control and restarts the system.
        /// </summary>
        public static void EnableUacAndRestartWindows()
        {
            EnableUac();
            RestartWindows();
        }

        private static WindowsApi.TOKEN_ELEVATION_TYPE GetProcessTokenElevationType()
        {
            IntPtr hToken = IntPtr.Zero;

            try
            {
                if (!WindowsApi.OpenProcessToken(WindowsApi.GetCurrentProcess(), WindowsApi.TOKEN_QUERY, ref hToken))
                    throw new Win32Exception(WindowsApi.GetLastError());

                WindowsApi.TOKEN_ELEVATION_TYPE elevationType = WindowsApi.TOKEN_ELEVATION_TYPE.TokenElevationTypeDefault;

                if (!WindowsApi.GetTokenInformation(hToken, WindowsApi.TOKEN_INFORMATION_CLASS.TokenElevationType, ref elevationType, (uint)sizeof(WindowsApi.TOKEN_ELEVATION_TYPE), out uint _))
                    throw new Win32Exception(WindowsApi.GetLastError());

                return elevationType;
            }
            finally
            {
                if (hToken != IntPtr.Zero)
                    WindowsApi.CloseHandle(hToken);
            }
        }

        private static void SetUacRegistryValue(bool enabled)
        {
            RegistryKey key = Registry.LocalMachine.OpenSubKey(UacRegistryKey, true);
            key?.SetValue(UacRegistryValue, enabled ? 1 : 0);
        }

        private static void RestartWindows()
        {
            WindowsApi.InitiateSystemShutdownEx(
                null, null, 0 /*Timeout*/,
                true /*ForceAppsClosed*/,
                true /*RebootAfterShutdown*/,
                WindowsApi.ShutdownReason.SHTDN_REASON_MAJOR_OPERATINGSYSTEM |
                WindowsApi.ShutdownReason.SHTDN_REASON_MINOR_RECONFIG |
                WindowsApi.ShutdownReason.SHTDN_REASON_FLAG_PLANNED);
            // This shutdown flag corresponds to: "Operating System: Reconfiguration (Planned)".
        }
    }
}

#endif