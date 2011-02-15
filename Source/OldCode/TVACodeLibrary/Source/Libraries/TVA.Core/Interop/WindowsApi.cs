//*******************************************************************************************************
//  WindowsApi.cs - Gbtc
//
//  Tennessee Valley Authority, 2009
//  No copyright is claimed pursuant to 17 USC § 105.  All Other Rights Reserved.
//
//  This software is made freely available under the TVA Open Source Agreement (see below).
//  Code in this file licensed to TVA under one or more contributor license agreements listed below.
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  01/24/2006 - J. Ritchie Carroll
//       Initial version of source created.
//  09/10/2008 - J. Ritchie Carroll
//       Converted to C#.
//  08/05/2009 - Josh L. Patterson
//       Edited Comments.
//  09/14/2009 - Stephen C. Wills
//       Added new header and license agreement.
//  02/04/2011 - J. Ritchie Carroll
//       Added WindowsAPI methods used to define automation of service failure actions.
//
//*******************************************************************************************************

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

/////////////////////////////////////////////////////////////////////////////
//
// Original Name    : Verifide.ServiceUtils.dll
// Description		: Extension to System.ServiceProcess.ServiceInstallerEx
//					  to enable configuration of advanced options
// Date				: 1/14/04
//
//	Copyright (C) 2004 Narendra (Neil) Baliga
//
//	This library is free software; you can redistribute it and/or
//	modify it as you wish. It is distributed in the hope that it will 
//	be useful,but WITHOUT ANY WARRANTY; without even the implied 
//	warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
//
/////////////////////////////////////////////////////////////////////////////

#endregion

using System;
using System.Runtime.InteropServices;

namespace TVA.Interop
{
    /// <summary>
    /// Defines common Windows API constants, enumerations, structures and functions.
    /// </summary>
    public static class WindowsApi
    {
        /// <summary>
        /// Use the standard logon provider for the system. The default security provider is NTLM. 
        /// </summary>
        public const int LOGON32_PROVIDER_DEFAULT = 0;

        /// <summary>
        /// This logon type is intended for users who will be interactively using the computer, such as a user being logged on by a terminal server, 
        /// remote shell, or similar process. This logon type has the additional expense of caching logon information for disconnected operations; 
        /// therefore, it is inappropriate for some client/server applications, such as a mail server. 
        /// </summary>
        public const int LOGON32_LOGON_INTERACTIVE = 2;
        
        /// <summary>
        /// This logon type is intended for high performance servers to authenticate plaintext passwords. The LogonUserEx function does not cache
        /// credentials for this logon type. 
        /// </summary>
        public const int LOGON32_LOGON_NETWORK = 3;
        
        /// <summary>
        /// Impersonate a client at the impersonation level.
        /// </summary>
        public const int SECURITY_IMPERSONATION = 2;

        /// <summary>
        /// Win32 SERVICE_DESCRIPTION structure.
        /// </summary>
        /// <remarks>
        /// <para>
        /// A description of NULL indicates no service description exists. The service description is NULL when the service is created.
        /// </para>
        /// <para>
        /// The description is simply a comment that explains the purpose of the service. For example, for the DHCP service,
        /// you could use the description "Provides internet addresses for computer on your network."
        /// </para>
        /// </remarks>
        [StructLayout(LayoutKind.Sequential)]
        public struct SERVICE_DESCRIPTION
        {
            /// <summary>
            /// The description of the service.
            /// </summary>
            /// <remarks>
            /// If this member is NULL, the description remains unchanged. If this value is an empty string (""), the current description is deleted.
            /// </remarks>
            public string lpDescription;
        }

        /// <summary>
        /// Win32 SC_ACTION_TYPE enumeration.
        /// </summary>
        [CLSCompliant(false)]
        public enum SC_ACTION_TYPE : uint
        {
            /// <summary>
            /// No action.
            /// </summary>
            SC_ACTION_NONE = 0x00000000,
            /// <summary>
            /// Restart the service.
            /// </summary>
            SC_ACTION_RESTART = 0x00000001,
            /// <summary>
            /// Reboot the computer.
            /// </summary>
            SC_ACTION_REBOOT = 0x00000002,
            /// <summary>
            /// Run a command.
            /// </summary>
            SC_ACTION_RUN_COMMAND = 0x00000003
        }

        /// <summary>
        /// Win32 SC_ACTION structure.
        /// </summary>
        [CLSCompliant(false),
        StructLayout(LayoutKind.Sequential)]
        public struct SC_ACTION
        {
            /// <summary>
            /// The <see cref="SC_ACTION_TYPE"/> to be performed. 
            /// </summary>
            [MarshalAs(UnmanagedType.U4)]
            public SC_ACTION_TYPE Type;
            /// <summary>
            /// The time to wait before performing the specified action, in milliseconds.
            /// </summary>
            [MarshalAs(UnmanagedType.U4)]
            public int Delay;
        }

        /// <summary>
        /// Win32 SERVICE_FAILURE_ACTIONS structure.
        /// </summary>
        /// <remarks>
        /// Represents the action the service controller should take on each failure of a service. A service is considered failed
        /// when it terminates without reporting a status of SERVICE_STOPPED to the service controller.
        /// </remarks>
        [StructLayout(LayoutKind.Sequential)]
        public struct SERVICE_FAILURE_ACTIONS
        {
            /// <summary>
            /// The time after which to reset the failure count to zero if there are no failures, in seconds. Specify INFINITE to indicate that this value should never be reset.
            /// </summary>
            [MarshalAs(UnmanagedType.U4)]
            public int dwResetPeriod;
            /// <summary>
            /// The message to be broadcast to server users before rebooting in response to the SC_ACTION_REBOOT service controller action. 
            /// </summary>
            /// <remarks>
            /// If this value is NULL, the reboot message is unchanged. If the value is an empty string (""), the reboot message is deleted and no message is broadcast.
            /// </remarks>
            public string lpRebootMsg;
            /// <summary>
            /// The command line of the process for the CreateProcess function to execute in response to the SC_ACTION_RUN_COMMAND service controller action. This process runs under the same account as the service. 
            /// </summary>
            /// <remarks>
            /// If this value is NULL, the command is unchanged. If the value is an empty string (""), the command is deleted and no program is run when the service fails.
            /// </remarks>
            public string lpCommand;
            /// <summary>
            /// The number of elements in the lpsaActions array. 
            /// </summary>
            /// <remarks>
            /// If this value is 0, but lpsaActions is not NULL, the reset period and array of failure actions are deleted. 
            /// </remarks>
            [MarshalAs(UnmanagedType.U4)]
            public int cActions;
            /// <summary>
            /// A pointer to an array of SC_ACTION structures. 
            /// </summary>
            /// <remarks>
            /// If this value is NULL, the cActions and dwResetPeriod members are ignored. 
            /// </remarks>
            public IntPtr lpsaActions;
        }

        /// <summary>
        /// Win32 SERVICE_FAILURE_ACTIONS_FLAG structure.
        /// </summary>
        /// <remarks>
        /// Contains the failure actions flag setting of a service. This setting determines when failure actions are to be executed.
        /// </remarks>
        [StructLayout(LayoutKind.Sequential)]
        public struct SERVICE_FAILURE_ACTIONS_FLAG
        {
            /// <summary>
            /// If this member is TRUE and the service has configured failure actions, the failure actions are queued if the service process 
            /// terminates without reporting a status of SERVICE_STOPPED or if it enters the SERVICE_STOPPED state but the dwWin32ExitCode member
            /// of the SERVICE_STATUS structure is not ERROR_SUCCESS (0). If this member is FALSE and the service has configured failure actions,
            /// the failure actions are queued only if the service terminates without reporting a status of SERVICE_STOPPED.
            /// </summary>
            public bool bFailureAction;
        }

        /// <summary>
        /// Win32 LUID_AND_ATTRIBUTES structure.
        /// </summary>
        /// <remarks>
        /// An LUID_AND_ATTRIBUTES structure can represent an LUID whose attributes change frequently, 
        /// such as when the LUID is used to represent privileges in the PRIVILEGE_SET structure. 
        /// Privileges are represented by LUIDs and have attributes indicating whether they are currently enabled or disabled. 
        /// </remarks>
        [StructLayout(LayoutKind.Sequential)]
        public struct LUID_AND_ATTRIBUTES
        {
            /// <summary>
            /// Specifies an LUID value. 
            /// </summary>
            public long Luid;
            /// <summary>
            /// Specifies attributes of the LUID. This value contains up to 32 one-bit flags. Its meaning is dependent on the definition and use of the LUID.
            /// </summary>
            public int Attributes;
        }

        /// <summary>
        /// Win32 TOKEN_PRIVILEGES structure.
        /// </summary>
        /// <remarks>
        /// The TOKEN_PRIVILEGES structure contains information about a set of privileges for an access token. 
        /// </remarks>
        // The Pack attribute specified here is important. We are in essence cheating here because
        // the Privileges field is actually a variable size array of structs.  We use the Pack=1
        // to align the Privileges field exactly after the PrivilegeCount field when marshalling
        // this struct to Win32.
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct TOKEN_PRIVILEGES
        {
            /// <summary>
            /// This must be set to the number of entries in the Privileges array. 
            /// </summary>
            public int PrivilegeCount;
            /// <summary>
            /// Specifies an array of LUID_AND_ATTRIBUTES structures. Each structure contains the LUID and attributes of a privilege.
            /// </summary>
            public LUID_AND_ATTRIBUTES Privileges;
        }

        /// <summary>
        /// Win32 OpenSCManager function.
        /// </summary>
        [DllImport("advapi32.dll")]
        public static extern IntPtr OpenSCManager(string lpMachineName, string lpDatabaseName, int dwDesiredAccess);

        /// <summary>
        /// Win32 OpenService function.
        /// </summary>
        [DllImport("advapi32.dll")]
        public static extern IntPtr OpenService(IntPtr hSCManager, string lpServiceName, int dwDesiredAccess);

        /// <summary>
        /// Win32 LockServiceDatabase function.
        /// </summary>
        [DllImport("advapi32.dll")]
        public static extern IntPtr LockServiceDatabase(IntPtr hSCManager);

        /// <summary>
        /// Win32 ChangeServiceConfig2 function.
        /// </summary>
        [DllImport("advapi32.dll")]
        public static extern bool ChangeServiceConfig2(IntPtr hService, int dwInfoLevel, IntPtr lpInfo);

        /// <summary>
        /// Win32 ChangeServiceConfig2 function mapped as ChangeServiceDescription to handle <see cref="SERVICE_DESCRIPTION"/> updates.
        /// </summary>
        [DllImport("advapi32.dll", EntryPoint = "ChangeServiceConfig2")]
        public static extern bool ChangeServiceDescription(IntPtr hService, int dwInfoLevel, [MarshalAs(UnmanagedType.Struct)] ref SERVICE_DESCRIPTION lpInfo);

        /// <summary>
        /// Win32 ChangeServiceConfig2 function mapped as ChangeServiceFailureActionFlag to handle <see cref="SERVICE_FAILURE_ACTIONS_FLAG"/> updates.
        /// </summary>
        [DllImport("advapi32.dll", EntryPoint = "ChangeServiceConfig2")]
        public static extern bool ChangeServiceFailureActionFlag(IntPtr hService, int dwInfoLevel, [MarshalAs(UnmanagedType.Struct)] ref SERVICE_FAILURE_ACTIONS_FLAG lpInfo);

        /// <summary>
        /// Win32 CloseServiceHandle function.
        /// </summary>
        [DllImport("advapi32.dll")]
        public static extern bool CloseServiceHandle(IntPtr hSCObject);

        /// <summary>
        /// Win32 UnlockServiceDatabase function.
        /// </summary>
        [DllImport("advapi32.dll")]
        public static extern bool UnlockServiceDatabase(IntPtr hSCManager);

        /// <summary>
        /// Win32 GetLastError function.
        /// </summary>
        [DllImport("kernel32.dll")]
        public static extern int GetLastError();

        /// <summary>
        /// Win32 LogonUser function.
        /// </summary>
        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern bool LogonUser(string lpszUsername, string lpszDomain, string lpszPassword, int dwLogonType, int dwLogonProvider, out IntPtr phToken);

        /// <summary>
        /// Win32 DuplicateToken function.
        /// </summary>
        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern bool DuplicateToken(IntPtr existingTokenHandle, int securityImpersonationLevel, ref IntPtr duplicateTokenHandle);

        /// <summary>
        /// Win32 AdjustTokenPrivileges function.
        /// </summary>
        [DllImport("advapi32.dll")]
        public static extern bool AdjustTokenPrivileges(IntPtr TokenHandle, bool DisableAllPrivileges, [MarshalAs(UnmanagedType.Struct)] ref TOKEN_PRIVILEGES NewState, int BufferLength, IntPtr PreviousState, ref int ReturnLength);

        /// <summary>
        /// Win32 LookupPrivilegeValue function.
        /// </summary>
        [DllImport("advapi32.dll")]
        public static extern bool LookupPrivilegeValue(string lpSystemName, string lpName, ref long lpLuid);

        /// <summary>
        /// Win32 OpenProcessToken function.
        /// </summary>
        [DllImport("advapi32.dll")]
        public static extern bool OpenProcessToken(IntPtr ProcessHandle, int DesiredAccess, ref IntPtr TokenHandle);

        /// <summary>
        /// Win32 GetCurrentProcess function.
        /// </summary>
        [DllImport("kernel32.dll")]
        public static extern IntPtr GetCurrentProcess();

        /// <summary>
        /// Win32 CloseHandle function.
        /// </summary>
        [DllImport("kernel32.dll")]
        public static extern bool CloseHandle(IntPtr hndl);

        /// <summary>
        /// Win32 FormatMessage function.
        /// </summary>
        [DllImport("kernel32.dll")]
        public static extern int FormatMessage(int dwFlags, ref IntPtr lpSource, int dwMessageId, int dwLanguageId, ref string lpBuffer, int nSize, ref IntPtr Arguments);

        /// <summary>
        /// Formats and returns a .NET string containing the Windows API level error message corresponding to the specified error code.
        /// </summary>
        /// <param name="errorCode">An <see cref="Int32"/> value corresponding to the specified error code.</param>
        /// <returns>A formatted error message corresponding to the specified error code.</returns>
        public static string GetErrorMessage(int errorCode)
        {
            const int FORMAT_MESSAGE_ALLOCATE_BUFFER = 0x100;
            const int FORMAT_MESSAGE_IGNORE_INSERTS = 0x200;
            const int FORMAT_MESSAGE_FROM_SYSTEM = 0x1000;

            int dwFlags = FORMAT_MESSAGE_ALLOCATE_BUFFER | FORMAT_MESSAGE_FROM_SYSTEM | FORMAT_MESSAGE_IGNORE_INSERTS;
            int messageSize = 255;
            string lpMsgBuf = "";
            IntPtr ptrlpSource = IntPtr.Zero;
            IntPtr prtArguments = IntPtr.Zero;

            if (FormatMessage(dwFlags, ref ptrlpSource, errorCode, 0, ref lpMsgBuf, messageSize, ref prtArguments) == 0)
                throw new InvalidOperationException("Failed to format message for error code " + errorCode);

            return lpMsgBuf;
        
        }

        /// <summary>
        /// Formats and returns a .NET string containing the Windows API level error message from the last Win32 call.
        /// </summary>
        /// <returns>A formatted error message corresponding to the last Win32 call error code.</returns>
        public static string GetLastErrorMessage()
        {
            return GetErrorMessage(Marshal.GetLastWin32Error());
        }
    }
}