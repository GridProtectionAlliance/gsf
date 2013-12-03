//******************************************************************************************************
//  WindowsApi.cs - Gbtc
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
using System.Runtime.InteropServices;
using System.Text;

#pragma warning disable 1591

namespace GSF.Interop
{
#if !MONO
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
        /// Access is denied.
        /// </summary>
        public const int ERROR_ACCESS_DENIED = 5;

        public const string SE_ASSIGNPRIMARYTOKEN_NAME = "SeAssignPrimaryTokenPrivilege";
        public const string SE_AUDIT_NAME = "SeAuditPrivilege";
        public const string SE_BACKUP_NAME = "SeBackupPrivilege";
        public const string SE_CHANGE_NOTIFY_NAME = "SeChangeNotifyPrivilege";
        public const string SE_CREATE_GLOBAL_NAME = "SeCreateGlobalPrivilege";
        public const string SE_CREATE_PAGEFILE_NAME = "SeCreatePagefilePrivilege";
        public const string SE_CREATE_PERMANENT_NAME = "SeCreatePermanentPrivilege";
        public const string SE_CREATE_SYMBOLIC_LINK_NAME = "SeCreateSymbolicLinkPrivilege";
        public const string SE_CREATE_TOKEN_NAME = "SeCreateTokenPrivilege";
        public const string SE_DEBUG_NAME = "SeDebugPrivilege";
        public const string SE_ENABLE_DELEGATION_NAME = "SeEnableDelegationPrivilege";
        public const string SE_IMPERSONATE_NAME = "SeImpersonatePrivilege";
        public const string SE_INC_BASE_PRIORITY_NAME = "SeIncreaseBasePriorityPrivilege";
        public const string SE_INCREASE_QUOTA_NAME = "SeIncreaseQuotaPrivilege";
        public const string SE_INC_WORKING_SET_NAME = "SeIncreaseWorkingSetPrivilege";
        public const string SE_LOAD_DRIVER_NAME = "SeLoadDriverPrivilege";
        public const string SE_LOCK_MEMORY_NAME = "SeLockMemoryPrivilege";
        public const string SE_MACHINE_ACCOUNT_NAME = "SeMachineAccountPrivilege";
        public const string SE_MANAGE_VOLUME_NAME = "SeManageVolumePrivilege";
        public const string SE_PROF_SINGLE_PROCESS_NAME = "SeProfileSingleProcessPrivilege";
        public const string SE_RELABEL_NAME = "SeRelabelPrivilege";
        public const string SE_REMOTE_SHUTDOWN_NAME = "SeRemoteShutdownPrivilege";
        public const string SE_RESTORE_NAME = "SeRestorePrivilege";
        public const string SE_SECURITY_NAME = "SeSecurityPrivilege";
        public const string SE_SHUTDOWN_NAME = "SeShutdownPrivilege";
        public const string SE_SYNC_AGENT_NAME = "SeSyncAgentPrivilege";
        public const string SE_SYSTEM_ENVIRONMENT_NAME = "SeSystemEnvironmentPrivilege";
        public const string SE_SYSTEM_PROFILE_NAME = "SeSystemProfilePrivilege";
        public const string SE_SYSTEMTIME_NAME = "SeSystemtimePrivilege";
        public const string SE_TAKE_OWNERSHIP_NAME = "SeTakeOwnershipPrivilege";
        public const string SE_TCB_NAME = "SeTcbPrivilege";
        public const string SE_TIME_ZONE_NAME = "SeTimeZonePrivilege";
        public const string SE_TRUSTED_CREDMAN_ACCESS_NAME = "SeTrustedCredManAccessPrivilege";
        public const string SE_UNDOCK_NAME = "SeUndockPrivilege";
        public const string SE_UNSOLICITED_INPUT_NAME = "SeUnsolicitedInputPrivilege";

        public const uint SE_PRIVILEGE_ENABLED_BY_DEFAULT = 0x00000001;
        public const uint SE_PRIVILEGE_ENABLED = 0x00000002;
        public const uint SE_PRIVILEGE_REMOVED = 0x00000004;
        public const uint SE_PRIVILEGE_USED_FOR_ACCESS = 0x80000000;

        // Use these for DesiredAccess
        public const uint STANDARD_RIGHTS_REQUIRED = 0x000F0000;
        public const uint STANDARD_RIGHTS_READ = 0x00020000;
        public const uint TOKEN_ASSIGN_PRIMARY = 0x0001;
        public const uint TOKEN_DUPLICATE = 0x0002;
        public const uint TOKEN_IMPERSONATE = 0x0004;
        public const uint TOKEN_QUERY = 0x0008;
        public const uint TOKEN_QUERY_SOURCE = 0x0010;
        public const uint TOKEN_ADJUST_PRIVILEGES = 0x0020;
        public const uint TOKEN_ADJUST_GROUPS = 0x0040;
        public const uint TOKEN_ADJUST_DEFAULT = 0x0080;
        public const uint TOKEN_ADJUST_SESSIONID = 0x0100;
        public const uint TOKEN_READ = (STANDARD_RIGHTS_READ | TOKEN_QUERY);
        public const uint TOKEN_ALL_ACCESS = (STANDARD_RIGHTS_REQUIRED | TOKEN_ASSIGN_PRIMARY |
            TOKEN_DUPLICATE | TOKEN_IMPERSONATE | TOKEN_QUERY | TOKEN_QUERY_SOURCE |
            TOKEN_ADJUST_PRIVILEGES | TOKEN_ADJUST_GROUPS | TOKEN_ADJUST_DEFAULT |
            TOKEN_ADJUST_SESSIONID);

        /// <summary>
        /// Win32 TOKEN_INFORMATION_CLASS enumeration.
        /// </summary>
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
            TokenLogonSid,

            /// <summary>
            /// The maximum value for this enumeration
            /// </summary>
            MaxTokenInfoClass
        }

        /// <summary>
        /// Win32 ProcessAccessTypes enumeration.
        /// </summary>
        [Flags]
        public enum ProcessAccessTypes : uint
        {
            /// <summary
            /// >Enables usage of the process handle in the TerminateProcess function to terminate the process.
            /// </summary>
            PROCESS_TERMINATE = 0x00000001,
            /// <summary>
            /// Enables usage of the process handle in the CreateRemoteThread function to create a thread in the process.
            /// </summary>
            PROCESS_CREATE_THREAD = 0x00000002,
            /// <summary>
            /// Sets the session ID.
            /// </summary>
            PROCESS_SET_SESSIONID = 0x00000004,
            /// <summary>
            /// Enables usage of the process handle in the VirtualProtectEx and WriteProcessMemory functions to modify the virtual memory of the process.
            /// </summary>
            PROCESS_VM_OPERATION = 0x00000008,
            /// <summary>
            /// Enables usage of the process handle in the ReadProcessMemory function to read from the virtual memory of the process.
            /// </summary>
            PROCESS_VM_READ = 0x00000010,
            /// <summary>
            /// Enables usage of the process handle in the WriteProcessMemory function to write to the virtual memory of the process.
            /// </summary>
            PROCESS_VM_WRITE = 0x00000020,
            /// <summary>
            /// Enables usage of the process handle as either the source or target process in the DuplicateHandle function to duplicate a handle.
            /// </summary>
            PROCESS_DUP_HANDLE = 0x00000040,
            /// <summary>
            /// Creates a process.
            /// </summary>
            PROCESS_CREATE_PROCESS = 0x00000080,
            /// <summary>
            /// Sets quota.
            /// </summary>
            PROCESS_SET_QUOTA = 0x00000100,
            /// <summary>
            /// Enables usage of the process handle in the SetPriorityClass function to set the priority class of the process.
            /// </summary>
            PROCESS_SET_INFORMATION = 0x00000200,
            /// <summary>
            /// Enables usage of the process handle in the GetExitCodeProcess and GetPriorityClass functions to read information from the process object.
            /// </summary>
            PROCESS_QUERY_INFORMATION = 0x00000400,
            /// <summary>
            /// Standard rights required.
            /// </summary>
            STANDARD_RIGHTS_REQUIRED = 0x000F0000,
            /// <summary>
            /// Enables usage of the process handle in any of the wait functions to wait for the process to terminate.
            /// </summary>
            SYNCHRONIZE = 0x00100000,
            /// <summary>
            /// Specifies all possible access flags for the process object.
            /// </summary>
            PROCESS_ALL_ACCESS = PROCESS_TERMINATE | PROCESS_CREATE_THREAD | PROCESS_SET_SESSIONID | PROCESS_VM_OPERATION |
                PROCESS_VM_READ | PROCESS_VM_WRITE | PROCESS_DUP_HANDLE | PROCESS_CREATE_PROCESS | PROCESS_SET_QUOTA |
                PROCESS_SET_INFORMATION | PROCESS_QUERY_INFORMATION | STANDARD_RIGHTS_REQUIRED | SYNCHRONIZE
        }

        /// <summary>
        /// Win32 ShutdownReason enumeration.
        /// </summary>
        [Flags]
        public enum ShutdownReason : uint
        {
            // Microsoft major reasons.
            SHTDN_REASON_MAJOR_OTHER = 0x00000000,
            SHTDN_REASON_MAJOR_NONE = 0x00000000,
            SHTDN_REASON_MAJOR_HARDWARE = 0x00010000,
            SHTDN_REASON_MAJOR_OPERATINGSYSTEM = 0x00020000,
            SHTDN_REASON_MAJOR_SOFTWARE = 0x00030000,
            SHTDN_REASON_MAJOR_APPLICATION = 0x00040000,
            SHTDN_REASON_MAJOR_SYSTEM = 0x00050000,
            SHTDN_REASON_MAJOR_POWER = 0x00060000,
            SHTDN_REASON_MAJOR_LEGACY_API = 0x00070000,

            // Microsoft minor reasons.
            SHTDN_REASON_MINOR_OTHER = 0x00000000,
            SHTDN_REASON_MINOR_NONE = 0x000000ff,
            SHTDN_REASON_MINOR_MAINTENANCE = 0x00000001,
            SHTDN_REASON_MINOR_INSTALLATION = 0x00000002,
            SHTDN_REASON_MINOR_UPGRADE = 0x00000003,
            SHTDN_REASON_MINOR_RECONFIG = 0x00000004,
            SHTDN_REASON_MINOR_HUNG = 0x00000005,
            SHTDN_REASON_MINOR_UNSTABLE = 0x00000006,
            SHTDN_REASON_MINOR_DISK = 0x00000007,
            SHTDN_REASON_MINOR_PROCESSOR = 0x00000008,
            SHTDN_REASON_MINOR_NETWORKCARD = 0x00000000,
            SHTDN_REASON_MINOR_POWER_SUPPLY = 0x0000000a,
            SHTDN_REASON_MINOR_CORDUNPLUGGED = 0x0000000b,
            SHTDN_REASON_MINOR_ENVIRONMENT = 0x0000000c,
            SHTDN_REASON_MINOR_HARDWARE_DRIVER = 0x0000000d,
            SHTDN_REASON_MINOR_OTHERDRIVER = 0x0000000e,
            SHTDN_REASON_MINOR_BLUESCREEN = 0x0000000F,
            SHTDN_REASON_MINOR_SERVICEPACK = 0x00000010,
            SHTDN_REASON_MINOR_HOTFIX = 0x00000011,
            SHTDN_REASON_MINOR_SECURITYFIX = 0x00000012,
            SHTDN_REASON_MINOR_SECURITY = 0x00000013,
            SHTDN_REASON_MINOR_NETWORK_CONNECTIVITY = 0x00000014,
            SHTDN_REASON_MINOR_WMI = 0x00000015,
            SHTDN_REASON_MINOR_SERVICEPACK_UNINSTALL = 0x00000016,
            SHTDN_REASON_MINOR_HOTFIX_UNINSTALL = 0x00000017,
            SHTDN_REASON_MINOR_SECURITYFIX_UNINSTALL = 0x00000018,
            SHTDN_REASON_MINOR_MMC = 0x00000019,
            SHTDN_REASON_MINOR_TERMSRV = 0x00000020,

            // Flags that end up in the event log code.
            SHTDN_REASON_FLAG_USER_DEFINED = 0x40000000,
            SHTDN_REASON_FLAG_PLANNED = 0x80000000,
            SHTDN_REASON_UNKNOWN = SHTDN_REASON_MINOR_NONE,
            SHTDN_REASON_LEGACY_API = (SHTDN_REASON_MAJOR_LEGACY_API | SHTDN_REASON_FLAG_PLANNED),

            // This mask cuts out UI flags.
            SHTDN_REASON_VALID_BIT_MASK = 0xc0ffffff
        }

        /// <summary>
        /// Win32 SECURITY_IMPERSONATION_LEVEL enumeration.
        /// </summary>
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
        /// Win32 CreationFlags enumeration.
        /// </summary>
        [Flags]
        public enum CreationFlags
        {
            CREATE_SUSPENDED = 0x00000004,
            CREATE_NEW_CONSOLE = 0x00000010,
            CREATE_NEW_PROCESS_GROUP = 0x00000200,
            CREATE_UNICODE_ENVIRONMENT = 0x00000400,
            CREATE_SEPARATE_WOW_VDM = 0x00000800,
            CREATE_DEFAULT_ERROR_MODE = 0x04000000,
        }

        /// <summary>
        /// Win32 LogonFlags enumeration.
        /// </summary>
        [Flags]
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
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct PROCESS_INFORMATION
        {
            public IntPtr hProcess;
            public IntPtr hThread;
            public int ProcessId;
            public int ThreadId;
        }

        /// <summary>
        /// Win32 STARTUPINFO structure.
        /// </summary>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
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
            public IntPtr lpReserved2;
            public IntPtr hStdInput;
            public IntPtr hStdOutput;
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
        /// Win32 SC_ACTION structure.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
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
        /// LSA API policy access enumeration.
        /// </summary>
        public enum LsaAccess
        {
            POLICY_READ = 0x20006,
            POLICY_ALL_ACCESS = 0x00F0FFF,
            POLICY_EXECUTE = 0X20801,
            POLICY_WRITE = 0X207F8
        }

        /// <summary>
        /// Win32 LSA_OBJECT_ATTRIBUTES structure.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct LSA_OBJECT_ATTRIBUTES
        {
            public int Length;
            public IntPtr RootDirectory;
            public IntPtr ObjectName;
            public int Attributes;
            public IntPtr SecurityDescriptor;
            public IntPtr SecurityQualityOfService;
        }

        /// <summary>
        /// Win32 LSA_UNICODE_STRING structure.
        /// </summary>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
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
        /// Win32 SECURITY_ATTRIBUTES structure.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct SECURITY_ATTRIBUTES
        {
            public int nLength;
            public IntPtr lpSecurityDescriptor;
            public int bInheritHandle;
        }

        /// <summary>
        /// Win32 InitiateSystemShutdownEx function.
        /// </summary>
        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool InitiateSystemShutdownEx(string lpMachineName, string lpMessage, uint dwTimeout, bool bForceAppsClosed, bool bRebootAfterShutdown, ShutdownReason dwReason);

        /// <summary>
        /// Win32 GetShellWindow function.
        /// </summary>
        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr GetShellWindow();

        /// <summary>
        /// Win32 GetWindowThreadProcessId function.
        /// </summary>
        [DllImport("user32.dll", SetLastError = true)]
        public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        /// <summary>
        /// Win32 OpenSCManager function.
        /// </summary>
        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern IntPtr OpenSCManager(string lpMachineName, string lpDatabaseName, int dwDesiredAccess);

        /// <summary>
        /// Win32 OpenService function.
        /// </summary>
        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern IntPtr OpenService(IntPtr hSCManager, string lpServiceName, int dwDesiredAccess);

        /// <summary>
        /// Win32 LockServiceDatabase function.
        /// </summary>
        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern IntPtr LockServiceDatabase(IntPtr hSCManager);

        /// <summary>
        /// Win32 ChangeServiceConfig2 function.
        /// </summary>
        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern bool ChangeServiceConfig2(IntPtr hService, int dwInfoLevel, IntPtr lpInfo);

        /// <summary>
        /// Win32 ChangeServiceConfig2 function.
        /// </summary>
        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern bool ChangeServiceConfig2(IntPtr hService, int dwInfoLevel, [MarshalAs(UnmanagedType.Struct)] ref SERVICE_DESCRIPTION lpInfo);

        /// <summary>
        /// Win32 ChangeServiceConfig2 function.
        /// </summary>
        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern bool ChangeServiceConfig2(IntPtr hService, int dwInfoLevel, [MarshalAs(UnmanagedType.Struct)] ref SERVICE_FAILURE_ACTIONS_FLAG lpInfo);

        /// <summary>
        /// Win32 CloseServiceHandle function.
        /// </summary>
        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern bool CloseServiceHandle(IntPtr hSCObject);

        /// <summary>
        /// Win32 UnlockServiceDatabase function.
        /// </summary>
        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern bool UnlockServiceDatabase(IntPtr hSCManager);

        /// <summary>
        /// Win32 LookupAccountName function.
        /// </summary>
        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true, PreserveSig = true)]
        public static extern bool LookupAccountName(string lpSystemName, string lpAccountName, IntPtr psid, ref int cbsid, StringBuilder domainName, ref int cbdomainLength, ref int use);

        /// <summary>
        /// Win32 LsaOpenPolicy function.
        /// </summary>
        [DllImport("advapi32.dll", PreserveSig = true)]
        public static extern uint LsaOpenPolicy(ref LSA_UNICODE_STRING SystemName, ref LSA_OBJECT_ATTRIBUTES ObjectAttributes, int DesiredAccess, out IntPtr PolicyHandle);

        /// <summary>
        /// Win32 LsaAddAccountRights function.
        /// </summary>
        [DllImport("advapi32.dll", SetLastError = true, PreserveSig = true)]
        public static extern uint LsaAddAccountRights(IntPtr PolicyHandle, IntPtr AccountSid, LSA_UNICODE_STRING[] UserRights, uint CountOfRights);

        /// <summary>
        /// Win32 LsaClose function.
        /// </summary>
        [DllImport("advapi32")]
        public static extern uint LsaClose(IntPtr PolicyHandle);

        /// <summary>
        /// Win32 FreeSid function.
        /// </summary>
        [DllImport("advapi32.dll")]
        public static extern IntPtr FreeSid(IntPtr pSid);

        /// <summary>
        /// Win32 GetLastError function.
        /// </summary>
        [DllImport("kernel32.dll")]
        public static extern int GetLastError();

        /// <summary>
        /// Win32 GetTokenInformation function.
        /// </summary>
        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern bool GetTokenInformation(IntPtr TokenHandle, TOKEN_INFORMATION_CLASS TokenInformationClass, ref uint TokenInformation, uint TokenInformationLength, out uint ReturnLength);

        /// <summary>
        /// Win32 GetTokenInformation function.
        /// </summary>
        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern bool GetTokenInformation(IntPtr TokenHandle, TOKEN_INFORMATION_CLASS TokenInformationClass, ref TOKEN_ELEVATION_TYPE TokenInformation, uint TokenInformationLength, out uint ReturnLength);

        /// <summary>
        /// Win32 LogonUser function.
        /// </summary>
        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern bool LogonUser(string lpszUsername, string lpszDomain, string lpszPassword, int dwLogonType, int dwLogonProvider, out IntPtr phToken);

        /// <summary>
        /// Win32 CreateProcessWithTokenW function.
        /// </summary>
        [DllImport("advapi32", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern bool CreateProcessWithTokenW(IntPtr hToken, LogonFlags dwLogonFlags, string lpApplicationName, string lpCommandLine, CreationFlags dwCreationFlags, IntPtr lpEnvironment, string lpCurrentDirectory, [In] ref STARTUPINFO lpStartupInfo, out PROCESS_INFORMATION lpProcessInformation);

        /// <summary>
        /// Win32 DuplicateToken function.
        /// </summary>
        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern bool DuplicateToken(IntPtr existingTokenHandle, int securityImpersonationLevel, ref IntPtr duplicateTokenHandle);

        /// <summary>
        /// Win32 DuplicateTokenEx function.
        /// </summary>
        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public extern static bool DuplicateTokenEx(IntPtr hExistingToken, uint dwDesiredAccess, [MarshalAs(UnmanagedType.Struct)] ref SECURITY_ATTRIBUTES lpTokenAttributes, SECURITY_IMPERSONATION_LEVEL ImpersonationLevel, TOKEN_TYPE TokenType, out IntPtr phNewToken);

        /// <summary>
        /// Win32 DuplicateTokenEx function.
        /// </summary>
        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public extern static bool DuplicateTokenEx(IntPtr hExistingToken, uint dwDesiredAccess, IntPtr lpTokenAttributes, SECURITY_IMPERSONATION_LEVEL ImpersonationLevel, TOKEN_TYPE TokenType, out IntPtr phNewToken);

        /// <summary>
        /// Win32 AdjustTokenPrivileges function.
        /// </summary>
        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern bool AdjustTokenPrivileges(IntPtr TokenHandle, bool DisableAllPrivileges, [MarshalAs(UnmanagedType.Struct)] ref TOKEN_PRIVILEGES NewState, int BufferLength, IntPtr PreviousState, ref int ReturnLength);

        /// <summary>
        /// Win32 LookupPrivilegeValue function.
        /// </summary>
        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern bool LookupPrivilegeValue(string lpSystemName, string lpName, ref long lpLuid);

        /// <summary>
        /// Win32 OpenProcessToken function.
        /// </summary>
        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern bool OpenProcessToken(IntPtr ProcessHandle, uint DesiredAccess, ref IntPtr TokenHandle);

        /// <summary>
        /// Win32 OpenProcess function.
        /// </summary>
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr OpenProcess(ProcessAccessTypes dwDesiredAccess, [MarshalAs(UnmanagedType.Bool)] bool bInheritHandle, uint dwProcessId);

        /// <summary>
        /// Win32 GetCurrentProcess function.
        /// </summary>
        [DllImport("kernel32.dll", SetLastError = true)]
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
#endif
}