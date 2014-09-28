//******************************************************************************************************
//  CustomActions.cs - Gbtc
//
//  Copyright © 2013, Grid Protection Alliance.  All Rights Reserved.
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
//  04/08/2013 - Stephen C. Wills
//       Generated original version of source code. Portions of code derived from Code Project article:
//       http://www.codeproject.com/Articles/6164/A-ServiceInstaller-Extension-That-Enables-Recovery
//       written by Neil Baliga
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Xml.Linq;
using GSF.Identity;
using GSF.Interop;
using GSF.ServiceProcess;
using Microsoft.Deployment.WindowsInstaller;

namespace GSF.InstallerActions
{
    /// <summary>
    /// Class that contains custom actions for a WiX installer.
    /// </summary>
    public class CustomActions
    {
        /// <summary>
        /// Custom action to attempt to authenticate the service account.
        /// </summary>
        /// <param name="session">Session object containing data from the installer.</param>
        /// <returns>Result of the custom action.</returns>
        [CustomAction]
        public static ActionResult AuthenticateServiceAccountAction(Session session)
        {
            UserInfo serviceAccountInfo;
            IPrincipal servicePrincipal;
            string serviceAccount;
            string servicePassword;

            string[] splitServiceAccount;
            string serviceDomain = string.Empty;
            string serviceUser = string.Empty;

            bool isSystemAccount;
            bool isManagedServiceAccount;
            bool isManagedServiceAccountValid;

            session.Log("Begin AuthenticateServiceAccountAction");

            serviceAccount = session["SERVICEACCOUNT"];
            servicePassword = session["SERVICEPASSWORD"];

            splitServiceAccount = serviceAccount.Split('\\');

            switch (splitServiceAccount.Length)
            {
                case 1:
                    serviceDomain = UserInfo.CurrentUserID.Split('\\')[0];
                    serviceUser = splitServiceAccount[0];
                    break;

                case 2:
                    serviceDomain = splitServiceAccount[0];
                    serviceUser = splitServiceAccount[1];
                    break;
            }

            isSystemAccount =
                serviceAccount.Equals("LocalSystem", StringComparison.OrdinalIgnoreCase) ||
                serviceAccount.StartsWith(@"NT AUTHORITY\", StringComparison.OrdinalIgnoreCase) ||
                serviceAccount.StartsWith(@"NT SERVICE\", StringComparison.OrdinalIgnoreCase);

            isManagedServiceAccount = serviceAccount.EndsWith("$", StringComparison.Ordinal);

            if (isSystemAccount)
            {
                session["SERVICEPASSWORD"] = string.Empty;
                session["SERVICEAUTHENTICATED"] = "yes";
            }
            else if (isManagedServiceAccount)
            {
                serviceAccountInfo = new UserInfo(serviceAccount);

                isManagedServiceAccountValid = serviceAccountInfo.Exists &&
                    !serviceAccountInfo.AccountIsDisabled &&
                    !serviceAccountInfo.AccountIsLockedOut &&
                    serviceAccountInfo.GetUserPropertyValue("msDS-HostServiceAccountBL").Split(',')[0].Equals("CN=" + Environment.MachineName, StringComparison.CurrentCultureIgnoreCase);

                if (isManagedServiceAccountValid)
                {
                    session["SERVICEPASSWORD"] = string.Empty;
                    session["SERVICEAUTHENTICATED"] = "yes";
                }
                else
                {
                    session["SERVICEAUTHENTICATED"] = null;
                }
            }
            else
            {
                servicePrincipal = UserInfo.AuthenticateUser(serviceDomain, serviceUser, servicePassword);

                if ((object)servicePrincipal != null && servicePrincipal.Identity.IsAuthenticated)
                    session["SERVICEAUTHENTICATED"] = "yes";
                else
                    session["SERVICEAUTHENTICATED"] = null;
            }

            session.Log("End AuthenticateServiceAccountAction");

            return ActionResult.Success;
        }

        /// <summary>
        /// Custom action to write CompanyName and CompanyAcronym settings to the configuration file of an installed service.
        /// </summary>
        /// <param name="session">Session object containing data from the installer.</param>
        /// <returns>Result of the custom action.</returns>
        [CustomAction]
        public static ActionResult CompanyInfoAction(Session session)
        {
            string serviceName;
            string configPath;
            XDocument config;

            session.Log("Begin CompanyInfoAction");

            serviceName = session.CustomActionData["SERVICENAME"];
            configPath = Path.Combine(session.CustomActionData["INSTALLDIR"], string.Format("{0}.exe.config", serviceName));

            if (File.Exists(configPath))
            {
                config = XDocument.Load(configPath);

                foreach (XElement systemSettings in config.Descendants("systemSettings"))
                {
                    // Search for existing CompanyName settings and update their values
                    foreach (XElement companyNameElement in systemSettings.Elements("add").Where(element => element.Attributes("name").Any(nameAttribute => string.Compare(nameAttribute.Value, "CompanyName", true) == 0)))
                        companyNameElement.Attributes("value").ToList().ForEach(valueAttribute => valueAttribute.Value = session.CustomActionData["COMPANYNAME"]);

                    // Search for existing CompanyAcronym settings and update their values
                    foreach (XElement companyAcronymElement in systemSettings.Elements("add").Where(element => element.Attributes("name").Any(nameAttribute => string.Compare(nameAttribute.Value, "CompanyAcronym", true) == 0)))
                        companyAcronymElement.Attributes("value").ToList().ForEach(valueAttribute => valueAttribute.Value = session.CustomActionData["COMPANYACRONYM"]);

                    // Add CompanyName setting if no such setting exists
                    if (!systemSettings.Elements("add").Any(element => element.Attributes("name").Any(nameAttribute => string.Compare(nameAttribute.Value, "CompanyName", true) == 0)))
                    {
                        systemSettings.Add(new XElement("add",
                            new XAttribute("name", "CompanyName"),
                            new XAttribute("value", session.CustomActionData["COMPANYNAME"]),
                            new XAttribute("description", string.Format("The name of the company who owns this instance of the {0}.", serviceName)),
                            new XAttribute("encrypted", "false")
                        ));
                    }

                    // Add CompanyAcronym setting if no such setting exists
                    if (!systemSettings.Elements("add").Any(element => element.Attributes("name").Any(nameAttribute => string.Compare(nameAttribute.Value, "CompanyAcronym", true) == 0)))
                    {
                        systemSettings.Add(new XElement("add",
                            new XAttribute("name", "CompanyAcronym"),
                            new XAttribute("value", session.CustomActionData["COMPANYACRONYM"]),
                            new XAttribute("description", string.Format("The acronym representing the company who owns this instance of the {0}.", serviceName)),
                            new XAttribute("encrypted", "false")
                        ));
                    }
                }

                config.Save(configPath);
            }

            session.Log("End CompanyInfoAction");

            return ActionResult.Success;
        }

        /// <summary>
        /// Custom action to configure various failure settings of an installed service.
        /// </summary>
        /// <param name="session">Session object containing data from the installer.</param>
        /// <returns>Result of the custom action.</returns>
        [CustomAction]
        public static ActionResult ConfigureServiceAction(Session session)
        {
            session.Log("Begin ConfigureServiceAction");
            UpdateServiceConfig(session);
            session.Log("End ConfigureServiceAction");
            return ActionResult.Success;
        }

        /// <summary>
        /// Custom action to set up the group and necessary permissions for the service account.
        /// </summary>
        /// <param name="session">Session object containing data from the installer.</param>
        /// <returns>Result of the custom action.</returns>
        [CustomAction]
        public static ActionResult ServiceAccountAction(Session session)
        {
            string serviceName;
            string serviceAccount;
            string groupName;

            string servicePorts;

            session.Log("Begin ServiceAccountAction");

            // Get properties from the installer session
            serviceName = session.CustomActionData["SERVICENAME"];
            serviceAccount = session.CustomActionData["SERVICEACCOUNT"];
            groupName = string.Format("{0} Admins", serviceName);

            // Create service admins group and add service account to that group as well as the Performance Log Users group
            try
            {
                session.Log("Adding {0} user to {1} group...", serviceAccount, groupName);
                UserInfo.CreateLocalGroup(groupName, string.Format("Members in this group have the necessary rights to administrate the {0} service.", serviceName));
                UserInfo.AddUserToLocalGroup(groupName, serviceAccount);
                UserInfo.AddUserToLocalGroup("Performance Log Users", serviceAccount);
                AddPrivileges(serviceAccount, "SeServiceLogonRight");
                session.Log("Done adding {0} user to {1} group.", serviceAccount, groupName);
            }
            catch (Exception)
            {
                session.Log("Failed to add {0} user to {1} group!", serviceAccount, groupName);
            }

            if (session.CustomActionData.TryGetValue("HTTPSERVICEPORTS", out servicePorts))
            {
                session.Log("Adding namespace reservations for default web services...");

                foreach (string port in servicePorts.Split(',').Select(p => p.Trim()))
                {
                    // Remove existing HTTP namespace reservations in case
                    // the current user does not match the service account
                    RemoveHttpNamespaceReservation(port);
                    AddHttpNamespaceReservation(serviceAccount, port);
                }

                session.Log("Done adding namespace reservations for default web services.");
            }

            session.Log("End ServiceAccountAction");

            return ActionResult.Success;
        }

        // Create an http namespace reservation
        private static void AddHttpNamespaceReservation(string serviceAccount, string port)
        {
            ProcessStartInfo psi;
            string parameters;

            // Vista, Windows 2008, Window 7, etc use "netsh" for reservations
            parameters = string.Format(@"http add urlacl url=http://+:{0}/ user=""{1}""", port, serviceAccount);

            psi = new ProcessStartInfo("netsh", parameters)
            {
                Verb = "runas",
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                UseShellExecute = false,
                Arguments = parameters
            };

            using (Process shell = Process.Start(psi))
            {
                if ((object)shell != null && !shell.WaitForExit(5000))
                    shell.Kill();
            }
        }

        // Delete an http namespace reservation
        private static void RemoveHttpNamespaceReservation(string port)
        {
            ProcessStartInfo psi;
            string parameters;

            // Vista, Windows 2008, Window 7, etc use "netsh" for reservations
            parameters = string.Format(@"http delete urlacl url=http://+:{0}", port);

            psi = new ProcessStartInfo("netsh", parameters)
            {
                Verb = "runas",
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                UseShellExecute = false,
                Arguments = parameters
            };

            using (Process shell = Process.Start(psi))
            {
                if ((object)shell != null && !shell.WaitForExit(5000))
                    shell.Kill();
            }
        }

        // Method to log to console and event log
        private static void LogInstallMessage(Session session, EventLogEntryType logLevel, string msg)
        {
            session.Log(msg);

            try
            {
                EventLog.WriteEntry(session.CustomActionData["SERVICENAME"], msg, logLevel);
            }
            catch (Exception ex)
            {
                session.Log(ex.ToString());
            }
        }

        #region [ Interop ]

        // Interop code below derived from Code Project article:
        // http://www.codeproject.com/Articles/6164/A-ServiceInstaller-Extension-That-Enables-Recovery
        // written by Neil Baliga

        private const int SC_MANAGER_ALL_ACCESS = 0xF003F;
        private const int SERVICE_ALL_ACCESS = 0xF01FF;
        private const int SERVICE_CONFIG_FAILURE_ACTIONS = 0x2;
        private const int SERVICE_CONFIG_FAILURE_ACTIONS_FLAG = 0x4;

        // The worker method to set all the extension properties for the service
        private static void UpdateServiceConfig(Session session)
        {
            // The failure actions to be defined for the service
            List<WindowsApi.SC_ACTION> failureActionsList = new List<WindowsApi.SC_ACTION>
            {
                new WindowsApi.SC_ACTION { Type = (WindowsApi.SC_ACTION_TYPE)(uint)RecoverAction.Restart, Delay = 2000 },
                new WindowsApi.SC_ACTION { Type = (WindowsApi.SC_ACTION_TYPE)(uint)RecoverAction.None, Delay = 2000 }
            };

            // We've got work to do
            IntPtr serviceManagerHandle = IntPtr.Zero;
            IntPtr serviceHandle = IntPtr.Zero;
            IntPtr serviceLockHandle = IntPtr.Zero;
            IntPtr actionsPtr = IntPtr.Zero;
            IntPtr failureActionsPtr = IntPtr.Zero;

            // Name of the service
            string serviceName = session.CustomActionData["SERVICENAME"];

            // Err check var
            bool result;

            // Place all our code in a try block
            try
            {
                // Open the service control manager
                serviceManagerHandle = WindowsApi.OpenSCManager(null, null, SC_MANAGER_ALL_ACCESS);

                if (serviceManagerHandle.ToInt32() <= 0)
                {
                    LogInstallMessage(session, EventLogEntryType.Error, "UpdateServiceConfig: Failed to Open Service Control Manager");
                    return;
                }

                // Lock the Service Database
                serviceLockHandle = WindowsApi.LockServiceDatabase(serviceManagerHandle);

                if (serviceLockHandle.ToInt32() <= 0)
                {
                    LogInstallMessage(session, EventLogEntryType.Error, "UpdateServiceConfig: Failed to Lock Service Database for Write");
                    return;
                }

                // Open the service
                serviceHandle = WindowsApi.OpenService(serviceManagerHandle, serviceName, SERVICE_ALL_ACCESS);

                if (serviceHandle.ToInt32() <= 0)
                {
                    LogInstallMessage(session, EventLogEntryType.Information, "UpdateServiceConfig: Failed to Open Service ");
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
                actionsPtr = Marshal.AllocHGlobal(scActionSize * failureActionsList.Count);

                // Set up the restart actions array by copying in each failure action structure
                for (int i = 0; i < failureActionsList.Count; i++)
                {
                    // Handle pointer math as 64-bit, cast will convert back to 32-bit if needed
                    Marshal.StructureToPtr(failureActionsList[i], (IntPtr)((Int64)actionsPtr + i * scActionSize), false);

                    if (failureActionsList[i].Type == WindowsApi.SC_ACTION_TYPE.SC_ACTION_REBOOT)
                        needShutdownPrivilege = true;
                }


                // If we need shutdown privilege, then grant it to this process
                if (needShutdownPrivilege)
                {
                    result = GrantShutdownPrivilege(session);

                    if (!result)
                        return;
                }

                // Set up the failure actions
                WindowsApi.SERVICE_FAILURE_ACTIONS failureActions = new WindowsApi.SERVICE_FAILURE_ACTIONS();
                failureActions.cActions = failureActionsList.Count;
                failureActions.dwResetPeriod = 120;
                failureActions.lpCommand = null;
                failureActions.lpRebootMsg = null;
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
                        throw new Exception("UpdateServiceConfig: Access Denied while setting service failure actions");
                }

                LogInstallMessage(session, EventLogEntryType.Information, "UpdateServiceConfig: Successfully configured service failure actions");

                // Failure actions flag only applies on Vista / 2008 or better
                if (Environment.OSVersion.Platform == PlatformID.Win32NT && Environment.OSVersion.Version.Major >= 6)
                {
                    WindowsApi.SERVICE_FAILURE_ACTIONS_FLAG failureActionFlag = new WindowsApi.SERVICE_FAILURE_ACTIONS_FLAG();
                    failureActionFlag.bFailureAction = true;
                    result = WindowsApi.ChangeServiceConfig2(serviceHandle, SERVICE_CONFIG_FAILURE_ACTIONS_FLAG, ref failureActionFlag);

                    // Error setting description?
                    if (!result)
                        throw new Exception("UpdateServiceConfig: Failed to set failure actions flag");
                }
            }
            // Catch all exceptions
            catch (Exception ex)
            {
                LogInstallMessage(session, EventLogEntryType.Error, ex.Message);
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
        private static bool GrantShutdownPrivilege(Session session)
        {
            bool grantSuccess = false;
            IntPtr processToken = IntPtr.Zero;
            IntPtr processHandle;
            WindowsApi.TOKEN_PRIVILEGES tokenPrivileges = new WindowsApi.TOKEN_PRIVILEGES();
            long luid = 0;
            int returnLen = 0;

            try
            {
                processHandle = WindowsApi.GetCurrentProcess();

                bool result = WindowsApi.OpenProcessToken(processHandle, WindowsApi.TOKEN_ADJUST_PRIVILEGES | WindowsApi.TOKEN_QUERY, ref processToken);

                if (!result)
                    return false;

                WindowsApi.LookupPrivilegeValue(null, WindowsApi.SE_SHUTDOWN_NAME, ref luid);

                tokenPrivileges.PrivilegeCount = 1;
                tokenPrivileges.Privileges.Luid = luid;
                tokenPrivileges.Privileges.Attributes = WindowsApi.SE_PRIVILEGE_ENABLED;

                WindowsApi.AdjustTokenPrivileges(processToken, false, ref tokenPrivileges, 0, IntPtr.Zero, ref returnLen);

                if (WindowsApi.GetLastError() != 0)
                    throw new Exception("Failed to grant shutdown privilege");

                grantSuccess = true;

            }
            catch (Exception ex)
            {
                LogInstallMessage(session, EventLogEntryType.Error, "GrantShutdownPrivilege: " + ex.Message);
            }
            finally
            {
                if (processToken != IntPtr.Zero)
                    WindowsApi.CloseHandle(processToken);
            }

            return grantSuccess;
        }

        private static void AddPrivileges(string account, string privilege)
        {
            uint result;

            // Pointer and size for the SID
            IntPtr sid = IntPtr.Zero;
            int sidSize = 0;

            // StringBuilder and size for the domain name
            StringBuilder domainName = new StringBuilder();
            int nameSize = 0;

            // Account-type variable for lookup
            int accountType = 0;

            // Get required buffer size
            WindowsApi.LookupAccountName(string.Empty, account, sid, ref sidSize, domainName, ref nameSize, ref accountType);

            // Allocate buffers
            domainName = new StringBuilder(nameSize);
            sid = Marshal.AllocHGlobal(sidSize);

            // Look up SID for the account
            if (WindowsApi.LookupAccountName(string.Empty, account, sid, ref sidSize, domainName, ref nameSize, ref accountType))
            {
                // Initialize an empty Unicode-string
                WindowsApi.LSA_UNICODE_STRING systemName = new WindowsApi.LSA_UNICODE_STRING();

                // Initialize a pointer for the policy handle
                IntPtr policyHandle;

                // These attributes are not used, but LsaOpenPolicy wants them to exist
                WindowsApi.LSA_OBJECT_ATTRIBUTES objectAttributes = new WindowsApi.LSA_OBJECT_ATTRIBUTES();

                // Get a policy handle
                result = WindowsApi.LsaOpenPolicy(ref systemName, ref objectAttributes, (int)WindowsApi.LsaAccess.POLICY_ALL_ACCESS, out policyHandle);

                if (result == 0)
                {
                    // Initialize a Unicode-string for the privilege name
                    WindowsApi.LSA_UNICODE_STRING[] userRights = new WindowsApi.LSA_UNICODE_STRING[1];

                    userRights[0] = new WindowsApi.LSA_UNICODE_STRING();
                    userRights[0].Buffer = Marshal.StringToHGlobalUni(privilege);
                    userRights[0].Length = (UInt16)(privilege.Length * UnicodeEncoding.CharSize);
                    userRights[0].MaximumLength = (UInt16)((privilege.Length + 1) * UnicodeEncoding.CharSize);

                    // Add the privilege to the account 
                    WindowsApi.LsaAddAccountRights(policyHandle, sid, userRights, 1);

                    // Close LSA policy handle
                    WindowsApi.LsaClose(policyHandle);
                }

                // Free SID
                WindowsApi.FreeSid(sid);
            }
        }

        #endregion
    }
}
