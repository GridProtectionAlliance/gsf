//******************************************************************************************************
//  UserAccountControl.cs - Gbtc
//
//  Tennessee Valley Authority, 2009
//  No copyright is claimed pursuant to 17 USC § 105.  All Other Rights Reserved.
//
//  This software is made freely available under the TVA Open Source Agreement (see below).
//  Code in this file licensed to TVA under one or more contributor license agreements listed below.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  02/18/2011 - J. Ritchie Carroll
//       Generated original version of source code.
//  09/21/2011 - J. Ritchie Carroll
//       Excluded class from Mono deployments due to P/Invoke requirements.
//
//******************************************************************************************************

#region [ TVA Open Source Agreement ]
/*

 THIS OPEN SOURCE AGREEMENT ("AGREEMENT") DEFINES THE RIGHTS OF USE,REPRODUCTION, DISTRIBUTION,
 MODIFICATION AND REDISTRIBUTION OF CERTAIN COMPUTER SOFTWARE ORIGINALLY RELEASED BY THE
 TENNESSEE VALLEY AUTHORITY, A CORPORATE AGENCY AND INSTRUMENTALITY OF THE UNITED STATES GOVERNMENT
 ("GOVERNMENT AGENCY"). GOVERNMENT AGENCY IS AN INTENDED THIRD-PARTY BENEFICIARY OF ALL SUBSEQUENT
 DISTRIBUTIONS OR REDISTRIBUTIONS OF THE SUBJECT SOFTWARE. ANYONE WHO USES, REPRODUCES, DISTRIBUTES,
 MODIFIES OR REDISTRIBUTES THE SUBJECT SOFTWARE, AS DEFINED HEREIN, OR ANY PART THEREOF, IS, BY THAT
 ACTION, ACCEPTING IN FULL THE RESPONSIBILITIES AND OBLIGATIONS CONTAINED IN THIS AGREEMENT.

 Original Software Designation: openPDC
 Original Software Title: The TVA Open Source Phasor Data Concentrator
 User Registration Requested. Please Visit https://naspi.tva.com/Registration/
 Point of Contact for Original Software: J. Ritchie Carroll <mailto:jrcarrol@tva.gov>

 1. DEFINITIONS

 A. "Contributor" means Government Agency, as the developer of the Original Software, and any entity
 that makes a Modification.

 B. "Covered Patents" mean patent claims licensable by a Contributor that are necessarily infringed by
 the use or sale of its Modification alone or when combined with the Subject Software.

 C. "Display" means the showing of a copy of the Subject Software, either directly or by means of an
 image, or any other device.

 D. "Distribution" means conveyance or transfer of the Subject Software, regardless of means, to
 another.

 E. "Larger Work" means computer software that combines Subject Software, or portions thereof, with
 software separate from the Subject Software that is not governed by the terms of this Agreement.

 F. "Modification" means any alteration of, including addition to or deletion from, the substance or
 structure of either the Original Software or Subject Software, and includes derivative works, as that
 term is defined in the Copyright Statute, 17 USC § 101. However, the act of including Subject Software
 as part of a Larger Work does not in and of itself constitute a Modification.

 G. "Original Software" means the computer software first released under this Agreement by Government
 Agency entitled openPDC, including source code, object code and accompanying documentation, if any.

 H. "Recipient" means anyone who acquires the Subject Software under this Agreement, including all
 Contributors.

 I. "Redistribution" means Distribution of the Subject Software after a Modification has been made.

 J. "Reproduction" means the making of a counterpart, image or copy of the Subject Software.

 K. "Sale" means the exchange of the Subject Software for money or equivalent value.

 L. "Subject Software" means the Original Software, Modifications, or any respective parts thereof.

 M. "Use" means the application or employment of the Subject Software for any purpose.

 2. GRANT OF RIGHTS

 A. Under Non-Patent Rights: Subject to the terms and conditions of this Agreement, each Contributor,
 with respect to its own contribution to the Subject Software, hereby grants to each Recipient a
 non-exclusive, world-wide, royalty-free license to engage in the following activities pertaining to
 the Subject Software:

 1. Use

 2. Distribution

 3. Reproduction

 4. Modification

 5. Redistribution

 6. Display

 B. Under Patent Rights: Subject to the terms and conditions of this Agreement, each Contributor, with
 respect to its own contribution to the Subject Software, hereby grants to each Recipient under Covered
 Patents a non-exclusive, world-wide, royalty-free license to engage in the following activities
 pertaining to the Subject Software:

 1. Use

 2. Distribution

 3. Reproduction

 4. Sale

 5. Offer for Sale

 C. The rights granted under Paragraph B. also apply to the combination of a Contributor's Modification
 and the Subject Software if, at the time the Modification is added by the Contributor, the addition of
 such Modification causes the combination to be covered by the Covered Patents. It does not apply to
 any other combinations that include a Modification. 

 D. The rights granted in Paragraphs A. and B. allow the Recipient to sublicense those same rights.
 Such sublicense must be under the same terms and conditions of this Agreement.

 3. OBLIGATIONS OF RECIPIENT

 A. Distribution or Redistribution of the Subject Software must be made under this Agreement except for
 additions covered under paragraph 3H. 

 1. Whenever a Recipient distributes or redistributes the Subject Software, a copy of this Agreement
 must be included with each copy of the Subject Software; and

 2. If Recipient distributes or redistributes the Subject Software in any form other than source code,
 Recipient must also make the source code freely available, and must provide with each copy of the
 Subject Software information on how to obtain the source code in a reasonable manner on or through a
 medium customarily used for software exchange.

 B. Each Recipient must ensure that the following copyright notice appears prominently in the Subject
 Software:

          No copyright is claimed pursuant to 17 USC § 105.  All Other Rights Reserved.

 C. Each Contributor must characterize its alteration of the Subject Software as a Modification and
 must identify itself as the originator of its Modification in a manner that reasonably allows
 subsequent Recipients to identify the originator of the Modification. In fulfillment of these
 requirements, Contributor must include a file (e.g., a change log file) that describes the alterations
 made and the date of the alterations, identifies Contributor as originator of the alterations, and
 consents to characterization of the alterations as a Modification, for example, by including a
 statement that the Modification is derived, directly or indirectly, from Original Software provided by
 Government Agency. Once consent is granted, it may not thereafter be revoked.

 D. A Contributor may add its own copyright notice to the Subject Software. Once a copyright notice has
 been added to the Subject Software, a Recipient may not remove it without the express permission of
 the Contributor who added the notice.

 E. A Recipient may not make any representation in the Subject Software or in any promotional,
 advertising or other material that may be construed as an endorsement by Government Agency or by any
 prior Recipient of any product or service provided by Recipient, or that may seek to obtain commercial
 advantage by the fact of Government Agency's or a prior Recipient's participation in this Agreement.

 F. In an effort to track usage and maintain accurate records of the Subject Software, each Recipient,
 upon receipt of the Subject Software, is requested to register with Government Agency by visiting the
 following website: https://naspi.tva.com/Registration/. Recipient's name and personal information
 shall be used for statistical purposes only. Once a Recipient makes a Modification available, it is
 requested that the Recipient inform Government Agency at the web site provided above how to access the
 Modification.

 G. Each Contributor represents that that its Modification does not violate any existing agreements,
 regulations, statutes or rules, and further that Contributor has sufficient rights to grant the rights
 conveyed by this Agreement.

 H. A Recipient may choose to offer, and to charge a fee for, warranty, support, indemnity and/or
 liability obligations to one or more other Recipients of the Subject Software. A Recipient may do so,
 however, only on its own behalf and not on behalf of Government Agency or any other Recipient. Such a
 Recipient must make it absolutely clear that any such warranty, support, indemnity and/or liability
 obligation is offered by that Recipient alone. Further, such Recipient agrees to indemnify Government
 Agency and every other Recipient for any liability incurred by them as a result of warranty, support,
 indemnity and/or liability offered by such Recipient.

 I. A Recipient may create a Larger Work by combining Subject Software with separate software not
 governed by the terms of this agreement and distribute the Larger Work as a single product. In such
 case, the Recipient must make sure Subject Software, or portions thereof, included in the Larger Work
 is subject to this Agreement.

 J. Notwithstanding any provisions contained herein, Recipient is hereby put on notice that export of
 any goods or technical data from the United States may require some form of export license from the
 U.S. Government. Failure to obtain necessary export licenses may result in criminal liability under
 U.S. laws. Government Agency neither represents that a license shall not be required nor that, if
 required, it shall be issued. Nothing granted herein provides any such export license.

 4. DISCLAIMER OF WARRANTIES AND LIABILITIES; WAIVER AND INDEMNIFICATION

 A. No Warranty: THE SUBJECT SOFTWARE IS PROVIDED "AS IS" WITHOUT ANY WARRANTY OF ANY KIND, EITHER
 EXPRESSED, IMPLIED, OR STATUTORY, INCLUDING, BUT NOT LIMITED TO, ANY WARRANTY THAT THE SUBJECT
 SOFTWARE WILL CONFORM TO SPECIFICATIONS, ANY IMPLIED WARRANTIES OF MERCHANTABILITY, FITNESS FOR A
 PARTICULAR PURPOSE, OR FREEDOM FROM INFRINGEMENT, ANY WARRANTY THAT THE SUBJECT SOFTWARE WILL BE ERROR
 FREE, OR ANY WARRANTY THAT DOCUMENTATION, IF PROVIDED, WILL CONFORM TO THE SUBJECT SOFTWARE. THIS
 AGREEMENT DOES NOT, IN ANY MANNER, CONSTITUTE AN ENDORSEMENT BY GOVERNMENT AGENCY OR ANY PRIOR
 RECIPIENT OF ANY RESULTS, RESULTING DESIGNS, HARDWARE, SOFTWARE PRODUCTS OR ANY OTHER APPLICATIONS
 RESULTING FROM USE OF THE SUBJECT SOFTWARE. FURTHER, GOVERNMENT AGENCY DISCLAIMS ALL WARRANTIES AND
 LIABILITIES REGARDING THIRD-PARTY SOFTWARE, IF PRESENT IN THE ORIGINAL SOFTWARE, AND DISTRIBUTES IT
 "AS IS."

 B. Waiver and Indemnity: RECIPIENT AGREES TO WAIVE ANY AND ALL CLAIMS AGAINST GOVERNMENT AGENCY, ITS
 AGENTS, EMPLOYEES, CONTRACTORS AND SUBCONTRACTORS, AS WELL AS ANY PRIOR RECIPIENT. IF RECIPIENT'S USE
 OF THE SUBJECT SOFTWARE RESULTS IN ANY LIABILITIES, DEMANDS, DAMAGES, EXPENSES OR LOSSES ARISING FROM
 SUCH USE, INCLUDING ANY DAMAGES FROM PRODUCTS BASED ON, OR RESULTING FROM, RECIPIENT'S USE OF THE
 SUBJECT SOFTWARE, RECIPIENT SHALL INDEMNIFY AND HOLD HARMLESS  GOVERNMENT AGENCY, ITS AGENTS,
 EMPLOYEES, CONTRACTORS AND SUBCONTRACTORS, AS WELL AS ANY PRIOR RECIPIENT, TO THE EXTENT PERMITTED BY
 LAW.  THE FOREGOING RELEASE AND INDEMNIFICATION SHALL APPLY EVEN IF THE LIABILITIES, DEMANDS, DAMAGES,
 EXPENSES OR LOSSES ARE CAUSED, OCCASIONED, OR CONTRIBUTED TO BY THE NEGLIGENCE, SOLE OR CONCURRENT, OF
 GOVERNMENT AGENCY OR ANY PRIOR RECIPIENT.  RECIPIENT'S SOLE REMEDY FOR ANY SUCH MATTER SHALL BE THE
 IMMEDIATE, UNILATERAL TERMINATION OF THIS AGREEMENT.

 5. GENERAL TERMS

 A. Termination: This Agreement and the rights granted hereunder will terminate automatically if a
 Recipient fails to comply with these terms and conditions, and fails to cure such noncompliance within
 thirty (30) days of becoming aware of such noncompliance. Upon termination, a Recipient agrees to
 immediately cease use and distribution of the Subject Software. All sublicenses to the Subject
 Software properly granted by the breaching Recipient shall survive any such termination of this
 Agreement.

 B. Severability: If any provision of this Agreement is invalid or unenforceable under applicable law,
 it shall not affect the validity or enforceability of the remainder of the terms of this Agreement.

 C. Applicable Law: This Agreement shall be subject to United States federal law only for all purposes,
 including, but not limited to, determining the validity of this Agreement, the meaning of its
 provisions and the rights, obligations and remedies of the parties.

 D. Entire Understanding: This Agreement constitutes the entire understanding and agreement of the
 parties relating to release of the Subject Software and may not be superseded, modified or amended
 except by further written agreement duly executed by the parties.

 E. Binding Authority: By accepting and using the Subject Software under this Agreement, a Recipient
 affirms its authority to bind the Recipient to all terms and conditions of this Agreement and that
 Recipient hereby agrees to all terms and conditions herein.

 F. Point of Contact: Any Recipient contact with Government Agency is to be directed to the designated
 representative as follows: J. Ritchie Carroll <mailto:jrcarrol@tva.gov>.

*/
#endregion

#region [ Contributor License Agreements ]

//******************************************************************************************************
//
//  Copyright © 2011, Grid Protection Alliance.  All Rights Reserved.
//
//  The GPA licenses this file to you under the Eclipse Public License -v 1.0 (the "License"); you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://www.opensource.org/licenses/eclipse-1.0.php
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//******************************************************************************************************

//******************************************************************************************************
//
//  Code translated from "User Account Control C++/CLI Library" developed by Sasha Goldshtein
//  found in the "User Account Control Helpers" project: http://uachelpers.codeplex.com/
//
//  Copyright (c) Sasha Goldshtein 2008
//  Microsoft Public License (Ms-PL): http://www.opensource.org/licenses/ms-pl
//
//******************************************************************************************************

#endregion

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Principal;
using Microsoft.Win32;
using TVA.Interop;

namespace TVA.Identity
{
#if !MONO
    /// <summary>
    /// Provides facilities for enabling and disabling User Account Control (UAC), determining elevation and virtualization status, and launching a process under elevated credentials.
    /// </summary>
    public static class UserAccountControl
    {
        private static string UacRegistryKey = "Software\\Microsoft\\Windows\\CurrentVersion\\Policies\\System";
        private static string UacRegistryValue = "EnableLUA";

        /// <summary>
        /// Creates a process under the elevated token, regardless of UAC settings or the manifest associated with that process.
        /// </summary>
        /// <param name="fileName">The path to the executable file.</param>
        /// <param name="arguments">The command-line arguments to pass to the process.</param>
        /// <returns>A <see cref="Process"/> object representing the newly created process.</returns>
        public static Process CreateProcessAsAdmin(string fileName, string arguments = "")
        {
            ProcessStartInfo startInfo = new ProcessStartInfo(fileName, arguments);
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
                WindowsApi.TOKEN_PRIVILEGES tkp = new WindowsApi.TOKEN_PRIVILEGES();
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
                uint dwShellPID;

                WindowsApi.GetWindowThreadProcessId(hShellWnd, out dwShellPID);

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
                WindowsApi.PROCESS_INFORMATION pi;
                WindowsApi.STARTUPINFO si = new WindowsApi.STARTUPINFO();

                si.cb = Marshal.SizeOf(si);

                if (!WindowsApi.CreateProcessWithTokenW(hPrimaryToken, 0, null, fileName + " " + arguments, 0, IntPtr.Zero, null, ref si, out pi))
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
                return (new WindowsPrincipal(WindowsIdentity.GetCurrent())).IsInRole("Administrators");
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

                if ((object)key != null)
                {
                    object value = key.GetValue(UacRegistryValue);

                    if (value != null)
                        return value.Equals(1);
                }

                return false;
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
                    uint dwSize;

                    if (!WindowsApi.GetTokenInformation(hToken, WindowsApi.TOKEN_INFORMATION_CLASS.TokenVirtualizationEnabled, ref virtualizationEnabled, sizeof(uint), out dwSize))
                        throw new Win32Exception(WindowsApi.GetLastError());

                    return (virtualizationEnabled != 0);
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
        public static bool IsCurrentProcessElevated
        {
            get
            {
                return GetProcessTokenElevationType() == WindowsApi.TOKEN_ELEVATION_TYPE.TokenElevationTypeFull; // elevated
            }
        }

        /// <summary>
        /// Disables User Account Control by changing the LUA registry key. The changes do not have effect until the system is restarted.
        /// </summary>
        public static void DisableUac()
        {
            SetUacRegistryValue(false);
        }

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
        public static void EnableUac()
        {
            SetUacRegistryValue(true);
        }

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
                uint dwSize;

                if (!WindowsApi.GetTokenInformation(hToken, WindowsApi.TOKEN_INFORMATION_CLASS.TokenElevationType, ref elevationType, sizeof(WindowsApi.TOKEN_ELEVATION_TYPE), out dwSize))
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
            key.SetValue(UacRegistryValue, enabled ? 1 : 0);
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
#endif
}
