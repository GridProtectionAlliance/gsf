//******************************************************************************************************
//  WindowsApi.cs - Gbtc
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
//  02/20/2011 - J. Ritchie Carroll
//       Added WindowsAPI methods used to help with UAC (User Account Control).
//  09/21/2011 - J. Ritchie Carroll
//       Excluded class from Mono deployments due to P/Invoke requirements.
//  12/14/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Text;

#pragma warning disable 1591

namespace GSF.Interop
{
    /// <summary>
    /// Defines common Windows API constants, enumerations, structures and functions.
    /// </summary>
    [SuppressMessage("Microsoft.Interoperability", "CA1414:MarkBooleanPInvokeArgumentsWithMarshalAs")]
    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "lp")]
    [SuppressMessage("Microsoft.Interoperability", "CA1401:PInvokesShouldNotBeVisible")]
    [SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference", MessageId = "2#")]
    public static class WindowsApi
    {
        /// <summary>
        /// Use the standard logon provider for the system. The default security provider is NTLM. 
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "LOGON")]
        [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
        public const int LOGON32_PROVIDER_DEFAULT = 0;

        /// <summary>
        /// This logon type is intended for users who will be interactively using the computer, such as a user being logged on by a terminal server, 
        /// remote shell, or similar process. This logon type has the additional expense of caching logon information for disconnected operations; 
        /// therefore, it is inappropriate for some client/server applications, such as a mail server. 
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "LOGON")]
        [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
        public const int LOGON32_LOGON_INTERACTIVE = 2;

        /// <summary>
        /// This logon type is intended for high performance servers to authenticate plaintext passwords. The LogonUserEx function does not cache
        /// credentials for this logon type. 
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "LOGON")]
        [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
        public const int LOGON32_LOGON_NETWORK = 3;

        /// <summary>
        /// Impersonate a client at the impersonation level.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
        public const int SECURITY_IMPERSONATION = 2;

        /// <summary>
        /// Access is denied.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
        public const int ERROR_ACCESS_DENIED = 5;

        [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
        public const int CRYPT_OID_INFO_OID_KEY = 1;
        [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
        public const int CRYPT_OID_INFO_NAME_KEY = 2;
        [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
        public const uint CRYPT_OID_DISABLE_SEARCH_DS_FLAG = 0x80000000u;
        [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
        public const int CRYPT_INSTALL_OID_INFO_BEFORE_FLAG = 1;

        [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
        public const string SE_ASSIGNPRIMARYTOKEN_NAME = "SeAssignPrimaryTokenPrivilege";
        [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
        public const string SE_AUDIT_NAME = "SeAuditPrivilege";
        [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
        public const string SE_BACKUP_NAME = "SeBackupPrivilege";
        [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
        public const string SE_CHANGE_NOTIFY_NAME = "SeChangeNotifyPrivilege";
        [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
        public const string SE_CREATE_GLOBAL_NAME = "SeCreateGlobalPrivilege";
        [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
        public const string SE_CREATE_PAGEFILE_NAME = "SeCreatePagefilePrivilege";
        [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
        public const string SE_CREATE_PERMANENT_NAME = "SeCreatePermanentPrivilege";
        [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
        public const string SE_CREATE_SYMBOLIC_LINK_NAME = "SeCreateSymbolicLinkPrivilege";
        [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
        public const string SE_CREATE_TOKEN_NAME = "SeCreateTokenPrivilege";
        [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
        public const string SE_DEBUG_NAME = "SeDebugPrivilege";
        [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
        public const string SE_ENABLE_DELEGATION_NAME = "SeEnableDelegationPrivilege";
        [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
        public const string SE_IMPERSONATE_NAME = "SeImpersonatePrivilege";
        [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
        public const string SE_INC_BASE_PRIORITY_NAME = "SeIncreaseBasePriorityPrivilege";
        [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
        public const string SE_INCREASE_QUOTA_NAME = "SeIncreaseQuotaPrivilege";
        [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
        public const string SE_INC_WORKING_SET_NAME = "SeIncreaseWorkingSetPrivilege";
        [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
        public const string SE_LOAD_DRIVER_NAME = "SeLoadDriverPrivilege";
        [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
        public const string SE_LOCK_MEMORY_NAME = "SeLockMemoryPrivilege";
        [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
        public const string SE_MACHINE_ACCOUNT_NAME = "SeMachineAccountPrivilege";
        [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
        public const string SE_MANAGE_VOLUME_NAME = "SeManageVolumePrivilege";
        [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
        public const string SE_PROF_SINGLE_PROCESS_NAME = "SeProfileSingleProcessPrivilege";
        [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
        public const string SE_RELABEL_NAME = "SeRelabelPrivilege";
        [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
        public const string SE_REMOTE_SHUTDOWN_NAME = "SeRemoteShutdownPrivilege";
        [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
        public const string SE_RESTORE_NAME = "SeRestorePrivilege";
        [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
        public const string SE_SECURITY_NAME = "SeSecurityPrivilege";
        [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
        public const string SE_SHUTDOWN_NAME = "SeShutdownPrivilege";
        [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
        public const string SE_SYNC_AGENT_NAME = "SeSyncAgentPrivilege";
        [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
        public const string SE_SYSTEM_ENVIRONMENT_NAME = "SeSystemEnvironmentPrivilege";
        [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
        public const string SE_SYSTEM_PROFILE_NAME = "SeSystemProfilePrivilege";
        [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
        public const string SE_SYSTEMTIME_NAME = "SeSystemtimePrivilege";
        [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
        public const string SE_TAKE_OWNERSHIP_NAME = "SeTakeOwnershipPrivilege";
        [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
        public const string SE_TCB_NAME = "SeTcbPrivilege";
        [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
        public const string SE_TIME_ZONE_NAME = "SeTimeZonePrivilege";
        [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
        public const string SE_TRUSTED_CREDMAN_ACCESS_NAME = "SeTrustedCredManAccessPrivilege";
        [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
        public const string SE_UNDOCK_NAME = "SeUndockPrivilege";
        [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
        public const string SE_UNSOLICITED_INPUT_NAME = "SeUnsolicitedInputPrivilege";

        [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
        public const uint SE_PRIVILEGE_ENABLED_BY_DEFAULT = 0x00000001;
        [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
        public const uint SE_PRIVILEGE_ENABLED = 0x00000002;
        [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
        public const uint SE_PRIVILEGE_REMOVED = 0x00000004;
        [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
        public const uint SE_PRIVILEGE_USED_FOR_ACCESS = 0x80000000;

        // Use these for DesiredAccess
        [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
        public const uint STANDARD_RIGHTS_REQUIRED = 0x000F0000;
        [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
        public const uint STANDARD_RIGHTS_READ = 0x00020000;
        [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
        public const uint TOKEN_ASSIGN_PRIMARY = 0x0001;
        [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
        public const uint TOKEN_DUPLICATE = 0x0002;
        [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
        public const uint TOKEN_IMPERSONATE = 0x0004;
        [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
        public const uint TOKEN_QUERY = 0x0008;
        [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
        public const uint TOKEN_QUERY_SOURCE = 0x0010;
        [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
        public const uint TOKEN_ADJUST_PRIVILEGES = 0x0020;
        [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
        public const uint TOKEN_ADJUST_GROUPS = 0x0040;
        [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
        public const uint TOKEN_ADJUST_DEFAULT = 0x0080;
        [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
        public const uint TOKEN_ADJUST_SESSIONID = 0x0100;
        [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
        public const uint TOKEN_READ = (STANDARD_RIGHTS_READ | TOKEN_QUERY);
        [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
        public const uint TOKEN_ALL_ACCESS = (STANDARD_RIGHTS_REQUIRED | TOKEN_ASSIGN_PRIMARY |
            TOKEN_DUPLICATE | TOKEN_IMPERSONATE | TOKEN_QUERY | TOKEN_QUERY_SOURCE |
            TOKEN_ADJUST_PRIVILEGES | TOKEN_ADJUST_GROUPS | TOKEN_ADJUST_DEFAULT |
            TOKEN_ADJUST_SESSIONID);

        /// <summary>
        /// Win32 TOKEN_INFORMATION_CLASS enumeration.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1008:EnumsShouldHaveZeroValue"), SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores"), SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible")]
        public enum TOKEN_INFORMATION_CLASS
        {
            /// <summary>
            /// The buffer receives a TOKEN_USER structure that contains the user account of the token.
            /// </summary>
            TokenUser = 1,

            /// <summary>
            /// The buffer receives a TOKEN_GROUPS structure that contains the group accounts associated with the token.
            /// </summary>
            TokenGroups,

            /// <summary>
            /// The buffer receives a TOKEN_PRIVILEGES structure that contains the privileges of the token.
            /// </summary>
            TokenPrivileges,

            /// <summary>
            /// The buffer receives a TOKEN_OWNER structure that contains the default owner security identifier (SID) for newly created objects.
            /// </summary>
            TokenOwner,

            /// <summary>
            /// The buffer receives a TOKEN_PRIMARY_GROUP structure that contains the default primary group SID for newly created objects.
            /// </summary>
            TokenPrimaryGroup,

            /// <summary>
            /// The buffer receives a TOKEN_DEFAULT_DACL structure that contains the default DACL for newly created objects.
            /// </summary>
            TokenDefaultDacl,

            /// <summary>
            /// The buffer receives a TOKEN_SOURCE structure that contains the source of the token. TOKEN_QUERY_SOURCE access is needed to retrieve this information.
            /// </summary>
            TokenSource,

            /// <summary>
            /// The buffer receives a TOKEN_TYPE value that indicates whether the token is a primary or impersonation token.
            /// </summary>
            TokenType,

            /// <summary>
            /// The buffer receives a SECURITY_IMPERSONATION_LEVEL value that indicates the impersonation level of the token. If the access token is not an impersonation token, the function fails.
            /// </summary>
            TokenImpersonationLevel,

            /// <summary>
            /// The buffer receives a TOKEN_STATISTICS structure that contains various token statistics.
            /// </summary>
            TokenStatistics,

            /// <summary>
            /// The buffer receives a TOKEN_GROUPS structure that contains the list of restricting SIDs in a restricted token.
            /// </summary>
            TokenRestrictedSids,

            /// <summary>
            /// The buffer receives a DWORD value that indicates the Terminal Services session identifier that is associated with the token. 
            /// </summary>
            TokenSessionId,

            /// <summary>
            /// The buffer receives a TOKEN_GROUPS_AND_PRIVILEGES structure that contains the user SID, the group accounts, the restricted SIDs, and the authentication ID associated with the token.
            /// </summary>
            TokenGroupsAndPrivileges,

            /// <summary>
            /// Reserved.
            /// </summary>
            TokenSessionReference,

            /// <summary>
            /// The buffer receives a DWORD value that is nonzero if the token includes the SANDBOX_INERT flag.
            /// </summary>
            [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "SandBox")]
            TokenSandBoxInert,

            /// <summary>
            /// Reserved.
            /// </summary>
            TokenAuditPolicy,

            /// <summary>
            /// The buffer receives a TOKEN_ORIGIN value. 
            /// </summary>
            TokenOrigin,

            /// <summary>
            /// The buffer receives a TOKEN_ELEVATION_TYPE value that specifies the elevation level of the token.
            /// </summary>
            TokenElevationType,

            /// <summary>
            /// The buffer receives a TOKEN_LINKED_TOKEN structure that contains a handle to another token that is linked to this token.
            /// </summary>
            TokenLinkedToken,

            /// <summary>
            /// The buffer receives a TOKEN_ELEVATION structure that specifies whether the token is elevated.
            /// </summary>
            TokenElevation,

            /// <summary>
            /// The buffer receives a DWORD value that is nonzero if the token has ever been filtered.
            /// </summary>
            TokenHasRestrictions,

            /// <summary>
            /// The buffer receives a TOKEN_ACCESS_INFORMATION structure that specifies security information contained in the token.
            /// </summary>
            TokenAccessInformation,

            /// <summary>
            /// The buffer receives a DWORD value that is nonzero if virtualization is allowed for the token.
            /// </summary>
            TokenVirtualizationAllowed,

            /// <summary>
            /// The buffer receives a DWORD value that is nonzero if virtualization is enabled for the token.
            /// </summary>
            TokenVirtualizationEnabled,

            /// <summary>
            /// The buffer receives a TOKEN_MANDATORY_LABEL structure that specifies the token's integrity level. 
            /// </summary>
            TokenIntegrityLevel,

            /// <summary>
            /// The buffer receives a DWORD value that is nonzero if the token has the UIAccess flag set.
            /// </summary>
            TokenUIAccess,

            /// <summary>
            /// The buffer receives a TOKEN_MANDATORY_POLICY structure that specifies the token's mandatory integrity policy.
            /// </summary>
            TokenMandatoryPolicy,

            /// <summary>
            /// The buffer receives the token's logon security identifier (SID).
            /// </summary>
            [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "Logon")]
            TokenLogonSid,

            /// <summary>
            /// The maximum value for this enumeration
            /// </summary>
            MaxTokenInfoClass
        }

        /// <summary>
        /// Win32 ProcessAccessTypes enumeration.
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA2217:DoNotMarkEnumsWithFlags"), SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible"), Flags]
        public enum ProcessAccessTypes : uint
        {
            /// <summary
            /// >Enables usage of the process handle in the TerminateProcess function to terminate the process.
            /// </summary>
            [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
            PROCESS_TERMINATE = 0x00000001,
            /// <summary>
            /// Enables usage of the process handle in the CreateRemoteThread function to create a thread in the process.
            /// </summary>
            [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
            PROCESS_CREATE_THREAD = 0x00000002,
            /// <summary>
            /// Sets the session ID.
            /// </summary>
            [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
            PROCESS_SET_SESSIONID = 0x00000004,
            /// <summary>
            /// Enables usage of the process handle in the VirtualProtectEx and WriteProcessMemory functions to modify the virtual memory of the process.
            /// </summary>
            [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
            PROCESS_VM_OPERATION = 0x00000008,
            /// <summary>
            /// Enables usage of the process handle in the ReadProcessMemory function to read from the virtual memory of the process.
            /// </summary>
            [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
            PROCESS_VM_READ = 0x00000010,
            /// <summary>
            /// Enables usage of the process handle in the WriteProcessMemory function to write to the virtual memory of the process.
            /// </summary>
            [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
            PROCESS_VM_WRITE = 0x00000020,
            /// <summary>
            /// Enables usage of the process handle as either the source or target process in the DuplicateHandle function to duplicate a handle.
            /// </summary>
            [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
            PROCESS_DUP_HANDLE = 0x00000040,
            /// <summary>
            /// Creates a process.
            /// </summary>
            [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
            PROCESS_CREATE_PROCESS = 0x00000080,
            /// <summary>
            /// Sets quota.
            /// </summary>
            [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
            PROCESS_SET_QUOTA = 0x00000100,
            /// <summary>
            /// Enables usage of the process handle in the SetPriorityClass function to set the priority class of the process.
            /// </summary>
            [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
            PROCESS_SET_INFORMATION = 0x00000200,
            /// <summary>
            /// Enables usage of the process handle in the GetExitCodeProcess and GetPriorityClass functions to read information from the process object.
            /// </summary>
            [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
            PROCESS_QUERY_INFORMATION = 0x00000400,
            /// <summary>
            /// Standard rights required.
            /// </summary>
            [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
            STANDARD_RIGHTS_REQUIRED = 0x000F0000,
            /// <summary>
            /// Enables usage of the process handle in any of the wait functions to wait for the process to terminate.
            /// </summary>
            SYNCHRONIZE = 0x00100000,
            /// <summary>
            /// Specifies all possible access flags for the process object.
            /// </summary>
            [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
            PROCESS_ALL_ACCESS = PROCESS_TERMINATE | PROCESS_CREATE_THREAD | PROCESS_SET_SESSIONID | PROCESS_VM_OPERATION |
                PROCESS_VM_READ | PROCESS_VM_WRITE | PROCESS_DUP_HANDLE | PROCESS_CREATE_PROCESS | PROCESS_SET_QUOTA |
                PROCESS_SET_INFORMATION | PROCESS_QUERY_INFORMATION | STANDARD_RIGHTS_REQUIRED | SYNCHRONIZE
        }

        /// <summary>
        /// Win32 ShutdownReason enumeration.
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA2217:DoNotMarkEnumsWithFlags"), SuppressMessage("Microsoft.Design", "CA1008:EnumsShouldHaveZeroValue"), SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible"), SuppressMessage("Microsoft.Naming", "CA1714:FlagsEnumsShouldHavePluralNames"), Flags]
        public enum ShutdownReason : uint
        {
            // Microsoft major reasons.
            [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
            SHTDN_REASON_MAJOR_OTHER = 0x00000000,
            [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
            SHTDN_REASON_MAJOR_NONE = 0x00000000,
            [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
            SHTDN_REASON_MAJOR_HARDWARE = 0x00010000,
            [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
            SHTDN_REASON_MAJOR_OPERATINGSYSTEM = 0x00020000,
            [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
            SHTDN_REASON_MAJOR_SOFTWARE = 0x00030000,
            [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
            SHTDN_REASON_MAJOR_APPLICATION = 0x00040000,
            [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
            SHTDN_REASON_MAJOR_SYSTEM = 0x00050000,
            [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
            SHTDN_REASON_MAJOR_POWER = 0x00060000,
            [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
            SHTDN_REASON_MAJOR_LEGACY_API = 0x00070000,

            // Microsoft minor reasons.
            [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
            SHTDN_REASON_MINOR_OTHER = 0x00000000,
            [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
            SHTDN_REASON_MINOR_NONE = 0x000000ff,
            [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
            SHTDN_REASON_MINOR_MAINTENANCE = 0x00000001,
            [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
            SHTDN_REASON_MINOR_INSTALLATION = 0x00000002,
            [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
            SHTDN_REASON_MINOR_UPGRADE = 0x00000003,
            [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
            SHTDN_REASON_MINOR_RECONFIG = 0x00000004,
            [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
            SHTDN_REASON_MINOR_HUNG = 0x00000005,
            [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
            SHTDN_REASON_MINOR_UNSTABLE = 0x00000006,
            [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
            SHTDN_REASON_MINOR_DISK = 0x00000007,
            [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
            SHTDN_REASON_MINOR_PROCESSOR = 0x00000008,
            [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
            SHTDN_REASON_MINOR_NETWORKCARD = 0x00000000,
            [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
            SHTDN_REASON_MINOR_POWER_SUPPLY = 0x0000000a,
            [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
            SHTDN_REASON_MINOR_CORDUNPLUGGED = 0x0000000b,
            [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
            SHTDN_REASON_MINOR_ENVIRONMENT = 0x0000000c,
            [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
            SHTDN_REASON_MINOR_HARDWARE_DRIVER = 0x0000000d,
            [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
            SHTDN_REASON_MINOR_OTHERDRIVER = 0x0000000e,
            [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
            SHTDN_REASON_MINOR_BLUESCREEN = 0x0000000F,
            [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
            SHTDN_REASON_MINOR_SERVICEPACK = 0x00000010,
            [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
            SHTDN_REASON_MINOR_HOTFIX = 0x00000011,
            [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
            SHTDN_REASON_MINOR_SECURITYFIX = 0x00000012,
            [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
            SHTDN_REASON_MINOR_SECURITY = 0x00000013,
            [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
            SHTDN_REASON_MINOR_NETWORK_CONNECTIVITY = 0x00000014,
            [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
            SHTDN_REASON_MINOR_WMI = 0x00000015,
            [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
            SHTDN_REASON_MINOR_SERVICEPACK_UNINSTALL = 0x00000016,
            [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
            SHTDN_REASON_MINOR_HOTFIX_UNINSTALL = 0x00000017,
            [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
            SHTDN_REASON_MINOR_SECURITYFIX_UNINSTALL = 0x00000018,
            [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
            SHTDN_REASON_MINOR_MMC = 0x00000019,
            [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
            SHTDN_REASON_MINOR_TERMSRV = 0x00000020,

            // Flags that end up in the event log code.
            [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
            SHTDN_REASON_FLAG_USER_DEFINED = 0x40000000,
            [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
            SHTDN_REASON_FLAG_PLANNED = 0x80000000,
            [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
            SHTDN_REASON_UNKNOWN = SHTDN_REASON_MINOR_NONE,
            [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
            SHTDN_REASON_LEGACY_API = (SHTDN_REASON_MAJOR_LEGACY_API | SHTDN_REASON_FLAG_PLANNED),

            // This mask cuts out UI flags.
            [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
            SHTDN_REASON_VALID_BIT_MASK = 0xc0ffffff
        }

        /// <summary>
        /// Win32 SECURITY_IMPERSONATION_LEVEL enumeration.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores"), SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible")]
        public enum SECURITY_IMPERSONATION_LEVEL
        {
            SecurityAnonymous,
            SecurityIdentification,
            SecurityImpersonation,
            SecurityDelegation
        }

        /// <summary>
        /// Win32 TOKEN_TYPE enumeration.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1008:EnumsShouldHaveZeroValue"), SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible"), SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
        public enum TOKEN_TYPE
        {
            TokenPrimary = 1,
            TokenImpersonation
        }

        /// <summary>
        /// Win32 TOKEN_ELEVATION_TYPE enumeration.
        /// </summary>
        /// <remarks>
        /// Enumeration indicates the elevation type of token being queried by the GetTokenInformation function or set by the SetTokenInformation function.
        /// </remarks>
        [SuppressMessage("Microsoft.Design", "CA1008:EnumsShouldHaveZeroValue"), SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores"), SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible")]
        public enum TOKEN_ELEVATION_TYPE
        {
            /// <summary>
            /// The token does not have a linked token.
            /// </summary>
            TokenElevationTypeDefault = 1,
            /// <summary>
            /// The token is an elevated token.
            /// </summary>
            TokenElevationTypeFull,
            /// <summary>
            /// The token is a limited token.
            /// </summary>
            TokenElevationTypeLimited
        }

        /// <summary>
        /// Win32 SC_ACTION_TYPE enumeration.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores"), SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible")]
        public enum SC_ACTION_TYPE : uint
        {
            /// <summary>
            /// No action.
            /// </summary>
            [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
            SC_ACTION_NONE = 0x00000000,
            /// <summary>
            /// Restart the service.
            /// </summary>
            [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
            SC_ACTION_RESTART = 0x00000001,
            /// <summary>
            /// Reboot the computer.
            /// </summary>
            [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
            SC_ACTION_REBOOT = 0x00000002,
            /// <summary>
            /// Run a command.
            /// </summary>
            [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
            SC_ACTION_RUN_COMMAND = 0x00000003
        }

        /// <summary>
        /// Win32 CreationFlags enumeration.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible"), Flags]
        public enum CreationFlags
        {
            [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
            CREATE_SUSPENDED = 0x00000004,
            [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
            CREATE_NEW_CONSOLE = 0x00000010,
            [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
            CREATE_NEW_PROCESS_GROUP = 0x00000200,
            [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
            CREATE_UNICODE_ENVIRONMENT = 0x00000400,
            [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
            CREATE_SEPARATE_WOW_VDM = 0x00000800,
            [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
            CREATE_DEFAULT_ERROR_MODE = 0x04000000,
        }

        /// <summary>
        /// Win32 LogonFlags enumeration.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "Logon"), SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible"), Flags]
        public enum LogonFlags
        {
            /// <summary>
            /// Log on, then load the user's profile in the HKEY_USERS registry key. The function
            /// returns after the profile has been loaded. Loading the profile can be time-consuming,
            /// so it is best to use this value only if you must access the information in the 
            /// HKEY_CURRENT_USER registry key. 
            /// NOTE: Windows Server 2003: The profile is unloaded after the new process has been
            /// terminated, regardless of whether it has created child processes.
            /// </summary>
            /// <remarks>See LOGON_WITH_PROFILE</remarks>
            WithProfile = 1,
            /// <summary>
            /// Log on, but use the specified credentials on the network only. The new process uses the
            /// same token as the caller, but the system creates a new logon session within LSA, and
            /// the process uses the specified credentials as the default credentials.
            /// This value can be used to create a process that uses a different set of credentials
            /// locally than it does remotely. This is useful in inter-domain scenarios where there is
            /// no trust relationship.
            /// The system does not validate the specified credentials. Therefore, the process can start,
            /// but it may not have access to network resources.
            /// </summary>
            /// <remarks>See LOGON_NETCREDENTIALS_ONLY</remarks>
            NetCredentialsOnly
        }

        /// <summary>
        /// Win32 PROCESS_INFORMATION structure.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores"), SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible"), SuppressMessage("Microsoft.Performance", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes"), StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct PROCESS_INFORMATION
        {
            [SuppressMessage("Microsoft.Security", "CA2111:PointersShouldNotBeVisible")]
            public IntPtr hProcess;
            [SuppressMessage("Microsoft.Security", "CA2111:PointersShouldNotBeVisible")]
            public IntPtr hThread;
            public int ProcessId;
            public int ThreadId;
        }

        /// <summary>
        /// Win32 STARTUPINFO structure.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible"), SuppressMessage("Microsoft.Performance", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes"), StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct STARTUPINFO
        {
            public int cb;
            public string lpReserved;
            public string lpDesktop;
            public string lpTitle;
            public uint dwX;
            public uint dwY;
            public uint dwXSize;
            public uint dwYSize;
            public uint dwXCountChars;
            public uint dwYCountChars;
            public uint dwFillAttribute;
            public uint dwFlags;
            public short wShowWindow;
            public short cbReserved2;
            [SuppressMessage("Microsoft.Security", "CA2111:PointersShouldNotBeVisible")]
            public IntPtr lpReserved2;
            [SuppressMessage("Microsoft.Security", "CA2111:PointersShouldNotBeVisible")]
            public IntPtr hStdInput;
            [SuppressMessage("Microsoft.Security", "CA2111:PointersShouldNotBeVisible")]
            public IntPtr hStdOutput;
            [SuppressMessage("Microsoft.Security", "CA2111:PointersShouldNotBeVisible")]
            public IntPtr hStdError;
        }

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
        [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores"), SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible"), SuppressMessage("Microsoft.Performance", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes"), StructLayout(LayoutKind.Sequential)]
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
        /// Win32 SC_ACTION structure.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores"), SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible"), SuppressMessage("Microsoft.Performance", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes"), StructLayout(LayoutKind.Sequential)]
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
        [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores"), SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible"), SuppressMessage("Microsoft.Performance", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes"), StructLayout(LayoutKind.Sequential)]
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
            [SuppressMessage("Microsoft.Security", "CA2111:PointersShouldNotBeVisible")]
            public IntPtr lpsaActions;
        }

        /// <summary>
        /// Win32 SERVICE_FAILURE_ACTIONS_FLAG structure.
        /// </summary>
        /// <remarks>
        /// Contains the failure actions flag setting of a service. This setting determines when failure actions are to be executed.
        /// </remarks>
        [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores"), SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible"), SuppressMessage("Microsoft.Performance", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes"), StructLayout(LayoutKind.Sequential)]
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
        /// LSA API policy access enumeration.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1008:EnumsShouldHaveZeroValue"), SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible")]
        public enum LsaAccess
        {
            [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
            POLICY_READ = 0x20006,
            [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
            POLICY_ALL_ACCESS = 0x00F0FFF,
            [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
            POLICY_EXECUTE = 0X20801,
            [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
            POLICY_WRITE = 0X207F8
        }

        /// <summary>
        /// Win32 LSA_OBJECT_ATTRIBUTES structure.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores"), SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible"), SuppressMessage("Microsoft.Performance", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes"), StructLayout(LayoutKind.Sequential)]
        public struct LSA_OBJECT_ATTRIBUTES
        {
            public int Length;
            [SuppressMessage("Microsoft.Security", "CA2111:PointersShouldNotBeVisible")]
            public IntPtr RootDirectory;
            [SuppressMessage("Microsoft.Security", "CA2111:PointersShouldNotBeVisible")]
            public IntPtr ObjectName;
            public int Attributes;
            [SuppressMessage("Microsoft.Security", "CA2111:PointersShouldNotBeVisible")]
            public IntPtr SecurityDescriptor;
            [SuppressMessage("Microsoft.Security", "CA2111:PointersShouldNotBeVisible")]
            public IntPtr SecurityQualityOfService;
        }

        /// <summary>
        /// Win32 LSA_UNICODE_STRING structure.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores"), SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible"), SuppressMessage("Microsoft.Performance", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes"), StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct LSA_UNICODE_STRING
        {
            /// <summary>
            /// Specifies the length of the string.
            /// </summary>
            public ushort Length;

            /// <summary>
            /// Specifies the maximum length of the string.
            /// </summary>
            public ushort MaximumLength;

            /// <summary>
            /// Pointer to string data.
            /// </summary>
            [SuppressMessage("Microsoft.Security", "CA2111:PointersShouldNotBeVisible")]
            public IntPtr Buffer;
        }

        /// <summary>
        /// Win32 LUID_AND_ATTRIBUTES structure.
        /// </summary>
        /// <remarks>
        /// An LUID_AND_ATTRIBUTES structure can represent an LUID whose attributes change frequently, 
        /// such as when the LUID is used to represent privileges in the PRIVILEGE_SET structure. 
        /// Privileges are represented by LUIDs and have attributes indicating whether they are currently enabled or disabled. 
        /// </remarks>
        [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores"), SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible"), SuppressMessage("Microsoft.Performance", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes"), StructLayout(LayoutKind.Sequential)]
        public struct LUID_AND_ATTRIBUTES
        {
            /// <summary>
            /// Specifies an LUID value. 
            /// </summary>
            public long Luid;

            /// <summary>
            /// Specifies attributes of the LUID. This value contains up to 32 one-bit flags. Its meaning is dependent on the definition and use of the LUID.
            /// </summary>
            public uint Attributes;
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
        [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores"), SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible"), SuppressMessage("Microsoft.Performance", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes"), StructLayout(LayoutKind.Sequential, Pack = 1)]
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
        /// Win32 SECURITY_ATTRIBUTES structure.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores"), SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible"), SuppressMessage("Microsoft.Performance", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes"), StructLayout(LayoutKind.Sequential)]
        public struct SECURITY_ATTRIBUTES
        {
            public int nLength;
            [SuppressMessage("Microsoft.Security", "CA2111:PointersShouldNotBeVisible")]
            public IntPtr lpSecurityDescriptor;
            public int bInheritHandle;
        }

        /// <summary>
        /// Win32 CRYPTOAPI_BLOB structure.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores"), SuppressMessage("Microsoft.Performance", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes"), SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible"), StructLayout(LayoutKind.Sequential)]
        public struct CRYPTOAPI_BLOB
        {
            public int cbData;
            [SuppressMessage("Microsoft.Security", "CA2111:PointersShouldNotBeVisible")]
            public IntPtr pbData;
        }

        /// <summary>
        /// Win32 CRYPT_OID_INFO class.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores"), SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible"), StructLayout(LayoutKind.Sequential)]
        public class CRYPT_OID_INFO
        {
            public int cbSize;

            [MarshalAs(UnmanagedType.LPStr)]
            public string pszOID;

            [MarshalAs(UnmanagedType.LPWStr)]
            public string pwszName;

            public int dwGroupId;
            public int dwValueOrAlgidordwLength;
            public CRYPTOAPI_BLOB ExtraInfo;

            [MarshalAs(UnmanagedType.LPWStr)]
            public string pwszCNGAlgid;

            [MarshalAs(UnmanagedType.LPWStr)]
            public string pwszCNGExtraAlgid;
        }

        /// <summary>
        /// Win32 InitiateSystemShutdownEx function.
        /// </summary>
        [SuppressMessage("Microsoft.Interoperability", "CA1414:MarkBooleanPInvokeArgumentsWithMarshalAs"), SuppressMessage("Microsoft.Interoperability", "CA1401:PInvokesShouldNotBeVisible"), DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool InitiateSystemShutdownEx(string lpMachineName, string lpMessage, uint dwTimeout, bool bForceAppsClosed, bool bRebootAfterShutdown, ShutdownReason dwReason);

        /// <summary>
        /// Win32 GetShellWindow function.
        /// </summary>
        [SuppressMessage("Microsoft.Interoperability", "CA1401:PInvokesShouldNotBeVisible"), DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr GetShellWindow();

        /// <summary>
        /// Win32 GetWindowThreadProcessId function.
        /// </summary>
        [SuppressMessage("Microsoft.Interoperability", "CA1401:PInvokesShouldNotBeVisible"), DllImport("user32.dll", SetLastError = true)]
        public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        /// <summary>
        /// Win32 OpenSCManager function.
        /// </summary>
        [SuppressMessage("Microsoft.Interoperability", "CA1401:PInvokesShouldNotBeVisible"), DllImport("advapi32.dll", SetLastError = true)]
        public static extern IntPtr OpenSCManager(string lpMachineName, string lpDatabaseName, int dwDesiredAccess);

        /// <summary>
        /// Win32 OpenService function.
        /// </summary>
        [SuppressMessage("Microsoft.Interoperability", "CA1401:PInvokesShouldNotBeVisible"), DllImport("advapi32.dll", SetLastError = true)]
        public static extern IntPtr OpenService(IntPtr hSCManager, string lpServiceName, int dwDesiredAccess);

        /// <summary>
        /// Win32 LockServiceDatabase function.
        /// </summary>
        [SuppressMessage("Microsoft.Interoperability", "CA1401:PInvokesShouldNotBeVisible"), DllImport("advapi32.dll", SetLastError = true)]
        public static extern IntPtr LockServiceDatabase(IntPtr hSCManager);

        /// <summary>
        /// Win32 ChangeServiceConfig2 function.
        /// </summary>
        [SuppressMessage("Microsoft.Interoperability", "CA1414:MarkBooleanPInvokeArgumentsWithMarshalAs"), SuppressMessage("Microsoft.Interoperability", "CA1401:PInvokesShouldNotBeVisible"), DllImport("advapi32.dll", SetLastError = true)]
        public static extern bool ChangeServiceConfig2(IntPtr hService, int dwInfoLevel, IntPtr lpInfo);

        /// <summary>
        /// Win32 ChangeServiceConfig2 function.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference", MessageId = "2#"), SuppressMessage("Microsoft.Interoperability", "CA1401:PInvokesShouldNotBeVisible"), SuppressMessage("Microsoft.Interoperability", "CA1414:MarkBooleanPInvokeArgumentsWithMarshalAs"), DllImport("advapi32.dll", SetLastError = true)]
        public static extern bool ChangeServiceConfig2(IntPtr hService, int dwInfoLevel, [MarshalAs(UnmanagedType.Struct)] ref SERVICE_DESCRIPTION lpInfo);

        /// <summary>
        /// Win32 ChangeServiceConfig2 function.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference", MessageId = "2#"), SuppressMessage("Microsoft.Interoperability", "CA1401:PInvokesShouldNotBeVisible"), SuppressMessage("Microsoft.Interoperability", "CA1414:MarkBooleanPInvokeArgumentsWithMarshalAs"), DllImport("advapi32.dll", SetLastError = true)]
        public static extern bool ChangeServiceConfig2(IntPtr hService, int dwInfoLevel, [MarshalAs(UnmanagedType.Struct)] ref SERVICE_FAILURE_ACTIONS_FLAG lpInfo);

        /// <summary>
        /// Win32 CloseServiceHandle function.
        /// </summary>
        [SuppressMessage("Microsoft.Interoperability", "CA1414:MarkBooleanPInvokeArgumentsWithMarshalAs"), SuppressMessage("Microsoft.Interoperability", "CA1401:PInvokesShouldNotBeVisible"), DllImport("advapi32.dll", SetLastError = true)]
        public static extern bool CloseServiceHandle(IntPtr hSCObject);

        /// <summary>
        /// Win32 UnlockServiceDatabase function.
        /// </summary>
        [SuppressMessage("Microsoft.Interoperability", "CA1414:MarkBooleanPInvokeArgumentsWithMarshalAs"), SuppressMessage("Microsoft.Interoperability", "CA1401:PInvokesShouldNotBeVisible"), DllImport("advapi32.dll", SetLastError = true)]
        public static extern bool UnlockServiceDatabase(IntPtr hSCManager);

        /// <summary>
        /// Win32 LookupAccountName function.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference", MessageId = "3#"), SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference", MessageId = "5#"), SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference", MessageId = "6#"), SuppressMessage("Microsoft.Interoperability", "CA1401:PInvokesShouldNotBeVisible"), SuppressMessage("Microsoft.Interoperability", "CA1414:MarkBooleanPInvokeArgumentsWithMarshalAs"), DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true, PreserveSig = true)]
        public static extern bool LookupAccountName(string lpSystemName, string lpAccountName, IntPtr psid, ref int cbsid, StringBuilder domainName, ref int cbdomainLength, ref int use);

        /// <summary>
        /// Win32 LsaOpenPolicy function.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference", MessageId = "0#"), SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference", MessageId = "1#"), SuppressMessage("Microsoft.Interoperability", "CA1401:PInvokesShouldNotBeVisible"), DllImport("advapi32.dll", PreserveSig = true)]
        public static extern uint LsaOpenPolicy(ref LSA_UNICODE_STRING SystemName, ref LSA_OBJECT_ATTRIBUTES ObjectAttributes, int DesiredAccess, out IntPtr PolicyHandle);

        /// <summary>
        /// Win32 LsaAddAccountRights function.
        /// </summary>
        [SuppressMessage("Microsoft.Interoperability", "CA1401:PInvokesShouldNotBeVisible"), DllImport("advapi32.dll", SetLastError = true, PreserveSig = true)]
        public static extern uint LsaAddAccountRights(IntPtr PolicyHandle, IntPtr AccountSid, LSA_UNICODE_STRING[] UserRights, uint CountOfRights);

        /// <summary>
        /// Win32 LsaClose function.
        /// </summary>
        [SuppressMessage("Microsoft.Interoperability", "CA1401:PInvokesShouldNotBeVisible"), DllImport("advapi32")]
        public static extern uint LsaClose(IntPtr PolicyHandle);

        /// <summary>
        /// Win32 FreeSid function.
        /// </summary>
        [SuppressMessage("Microsoft.Interoperability", "CA1401:PInvokesShouldNotBeVisible"), DllImport("advapi32.dll")]
        public static extern IntPtr FreeSid(IntPtr pSid);

        /// <summary>
        /// Win32 GetLastError function.
        /// </summary>
        [SuppressMessage("Microsoft.Interoperability", "CA1401:PInvokesShouldNotBeVisible"), SuppressMessage("Microsoft.Usage", "CA2205:UseManagedEquivalentsOfWin32Api"), DllImport("kernel32.dll")]
        public static extern int GetLastError();

        /// <summary>
        /// Win32 GetTokenInformation function.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference", MessageId = "2#"), SuppressMessage("Microsoft.Interoperability", "CA1401:PInvokesShouldNotBeVisible"), SuppressMessage("Microsoft.Interoperability", "CA1414:MarkBooleanPInvokeArgumentsWithMarshalAs"), DllImport("advapi32.dll", SetLastError = true)]
        public static extern bool GetTokenInformation(IntPtr TokenHandle, TOKEN_INFORMATION_CLASS TokenInformationClass, ref uint TokenInformation, uint TokenInformationLength, out uint ReturnLength);

        /// <summary>
        /// Win32 GetTokenInformation function.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference", MessageId = "2#"), SuppressMessage("Microsoft.Interoperability", "CA1401:PInvokesShouldNotBeVisible"), SuppressMessage("Microsoft.Interoperability", "CA1414:MarkBooleanPInvokeArgumentsWithMarshalAs"), DllImport("advapi32.dll", SetLastError = true)]
        public static extern bool GetTokenInformation(IntPtr TokenHandle, TOKEN_INFORMATION_CLASS TokenInformationClass, ref TOKEN_ELEVATION_TYPE TokenInformation, uint TokenInformationLength, out uint ReturnLength);

        /// <summary>
        /// Win32 LogonUser function.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "Username"), SuppressMessage("Microsoft.Interoperability", "CA1401:PInvokesShouldNotBeVisible"), SuppressMessage("Microsoft.Interoperability", "CA1414:MarkBooleanPInvokeArgumentsWithMarshalAs"), SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "Logon"), DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern bool LogonUser(string lpszUsername, string lpszDomain, string lpszPassword, int dwLogonType, int dwLogonProvider, out IntPtr phToken);

        /// <summary>
        /// Win32 CreateProcessWithTokenW function.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "Logon"), SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference", MessageId = "7#"), SuppressMessage("Microsoft.Interoperability", "CA1401:PInvokesShouldNotBeVisible"), SuppressMessage("Microsoft.Interoperability", "CA1414:MarkBooleanPInvokeArgumentsWithMarshalAs"), DllImport("advapi32", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern bool CreateProcessWithTokenW(IntPtr hToken, LogonFlags dwLogonFlags, string lpApplicationName, string lpCommandLine, CreationFlags dwCreationFlags, IntPtr lpEnvironment, string lpCurrentDirectory, [In] ref STARTUPINFO lpStartupInfo, out PROCESS_INFORMATION lpProcessInformation);

        /// <summary>
        /// Win32 DuplicateToken function.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference", MessageId = "2#"), SuppressMessage("Microsoft.Interoperability", "CA1401:PInvokesShouldNotBeVisible"), SuppressMessage("Microsoft.Interoperability", "CA1414:MarkBooleanPInvokeArgumentsWithMarshalAs"), DllImport("advapi32.dll", SetLastError = true)]
        public static extern bool DuplicateToken(IntPtr existingTokenHandle, int securityImpersonationLevel, ref IntPtr duplicateTokenHandle);

        /// <summary>
        /// Win32 DuplicateTokenEx function.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference", MessageId = "2#"), SuppressMessage("Microsoft.Interoperability", "CA1401:PInvokesShouldNotBeVisible"), SuppressMessage("Microsoft.Interoperability", "CA1414:MarkBooleanPInvokeArgumentsWithMarshalAs"), DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool DuplicateTokenEx(IntPtr hExistingToken, uint dwDesiredAccess, [MarshalAs(UnmanagedType.Struct)] ref SECURITY_ATTRIBUTES lpTokenAttributes, SECURITY_IMPERSONATION_LEVEL ImpersonationLevel, TOKEN_TYPE TokenType, out IntPtr phNewToken);

        /// <summary>
        /// Win32 DuplicateTokenEx function.
        /// </summary>
        [SuppressMessage("Microsoft.Interoperability", "CA1414:MarkBooleanPInvokeArgumentsWithMarshalAs"), SuppressMessage("Microsoft.Interoperability", "CA1401:PInvokesShouldNotBeVisible"), DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool DuplicateTokenEx(IntPtr hExistingToken, uint dwDesiredAccess, IntPtr lpTokenAttributes, SECURITY_IMPERSONATION_LEVEL ImpersonationLevel, TOKEN_TYPE TokenType, out IntPtr phNewToken);

        /// <summary>
        /// Win32 AdjustTokenPrivileges function.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference", MessageId = "2#"), SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference", MessageId = "5#"), SuppressMessage("Microsoft.Interoperability", "CA1414:MarkBooleanPInvokeArgumentsWithMarshalAs"), SuppressMessage("Microsoft.Interoperability", "CA1401:PInvokesShouldNotBeVisible"), DllImport("advapi32.dll", SetLastError = true)]
        public static extern bool AdjustTokenPrivileges(IntPtr TokenHandle, bool DisableAllPrivileges, [MarshalAs(UnmanagedType.Struct)] ref TOKEN_PRIVILEGES NewState, int BufferLength, IntPtr PreviousState, ref int ReturnLength);

        /// <summary>
        /// Win32 LookupPrivilegeValue function.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference", MessageId = "2#"), SuppressMessage("Microsoft.Interoperability", "CA1414:MarkBooleanPInvokeArgumentsWithMarshalAs"), SuppressMessage("Microsoft.Interoperability", "CA1401:PInvokesShouldNotBeVisible"), DllImport("advapi32.dll", SetLastError = true)]
        public static extern bool LookupPrivilegeValue(string lpSystemName, string lpName, ref long lpLuid);

        /// <summary>
        /// Win32 OpenProcessToken function.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference", MessageId = "2#"), SuppressMessage("Microsoft.Interoperability", "CA1414:MarkBooleanPInvokeArgumentsWithMarshalAs"), SuppressMessage("Microsoft.Interoperability", "CA1401:PInvokesShouldNotBeVisible"), DllImport("advapi32.dll", SetLastError = true)]
        public static extern bool OpenProcessToken(IntPtr ProcessHandle, uint DesiredAccess, ref IntPtr TokenHandle);

        /// <summary>
        /// Win32 OpenProcess function.
        /// </summary>
        [SuppressMessage("Microsoft.Interoperability", "CA1401:PInvokesShouldNotBeVisible"), DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr OpenProcess(ProcessAccessTypes dwDesiredAccess, [MarshalAs(UnmanagedType.Bool)] bool bInheritHandle, uint dwProcessId);

        /// <summary>
        /// Win32 GetCurrentProcess function.
        /// </summary>
        [SuppressMessage("Microsoft.Interoperability", "CA1401:PInvokesShouldNotBeVisible"), DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr GetCurrentProcess();

        /// <summary>
        /// Win32 CloseHandle function.
        /// </summary>
        [SuppressMessage("Microsoft.Interoperability", "CA1414:MarkBooleanPInvokeArgumentsWithMarshalAs"), SuppressMessage("Microsoft.Interoperability", "CA1401:PInvokesShouldNotBeVisible"), DllImport("kernel32.dll")]
        public static extern bool CloseHandle(IntPtr hndl);

        /// <summary>
        /// Win32 CopyMemory function.
        /// </summary>
        [DllImport("kernel32.dll", EntryPoint = "CopyMemory")]
        internal static extern void CopyMemory(IntPtr destination, IntPtr source, uint length);

        /// <summary>
        /// Win32 FormatMessage function.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference", MessageId = "1#"), SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference", MessageId = "4#"), SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference", MessageId = "6#"), SuppressMessage("Microsoft.Interoperability", "CA1401:PInvokesShouldNotBeVisible"), DllImport("kernel32.dll")]
        public static extern int FormatMessage(int dwFlags, ref IntPtr lpSource, int dwMessageId, int dwLanguageId, ref string lpBuffer, int nSize, ref IntPtr Arguments);

        /// <summary>
        /// Win32 CryptFindOIDInfo function.
        /// </summary>
        [SuppressMessage("Microsoft.Interoperability", "CA1401:PInvokesShouldNotBeVisible"), DllImport("crypt32.dll", SetLastError = true)]
        public static extern IntPtr CryptFindOIDInfo(int dwKeyType, string pvKey, uint dwGroupId);

        /// <summary>
        /// Win32 CryptRegisterOIDInfo function.
        /// </summary>
        [SuppressMessage("Microsoft.Interoperability", "CA1414:MarkBooleanPInvokeArgumentsWithMarshalAs"), SuppressMessage("Microsoft.Interoperability", "CA1401:PInvokesShouldNotBeVisible"), DllImport("crypt32.dll", SetLastError = true)]
        public static extern bool CryptRegisterOIDInfo(IntPtr pInfo, int dwFlags);

        /// <summary>
        /// Win32 CryptUnregisterOIDInfo function.
        /// </summary>
        [SuppressMessage("Microsoft.Interoperability", "CA1414:MarkBooleanPInvokeArgumentsWithMarshalAs"), SuppressMessage("Microsoft.Interoperability", "CA1401:PInvokesShouldNotBeVisible"), DllImport("crypt32.dll", SetLastError = true)]
        public static extern bool CryptUnregisterOIDInfo(IntPtr pInfo);

        /// <summary>
        /// Win32 IsWow64Process function.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference", MessageId = "1#"), SuppressMessage("Microsoft.Interoperability", "CA1401:PInvokesShouldNotBeVisible"), SuppressMessage("Microsoft.Interoperability", "CA1414:MarkBooleanPInvokeArgumentsWithMarshalAs"), DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool IsWow64Process(IntPtr hProcess, ref bool Wow64Process);

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

        #pragma warning disable 169
        #pragma warning disable 649
        // ReSharper disable MemberCanBePrivate.Local
        // ReSharper disable NotAccessedField.Local
        [SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible")]
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public class MEMORYSTATUSEX
        {
            public uint dwLength;
            public uint dwMemoryLoad;
            public ulong ullTotalPhys;
            public ulong ullAvailPhys;
            public ulong ullTotalPageFile;
            public ulong ullAvailPageFile;
            public ulong ullTotalVirtual;
            public ulong ullAvailVirtual;
            public ulong ullAvailExtendedVirtual;

            public MEMORYSTATUSEX()
            {
                dwLength = (uint)Marshal.SizeOf(typeof(MEMORYSTATUSEX));
            }
        }

        [SuppressMessage("Microsoft.Interoperability", "CA1401:PInvokesShouldNotBeVisible")]
        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool GlobalMemoryStatusEx([In, Out] MEMORYSTATUSEX lpBuffer);
    }
}