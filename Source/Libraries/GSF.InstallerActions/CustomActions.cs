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
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
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
        /// Custom action to create group for an installed service and add the service account to the group.
        /// </summary>
        /// <param name="session">Session object containing data from the installer.</param>
        /// <returns>Result of the custom action.</returns>
        [CustomAction]
        public static ActionResult ServiceGroupAction(Session session)
        {
            string serviceName;
            string userName;
            string groupName;

            session.Log("Begin ServiceGroupAction");

            serviceName = session.CustomActionData["SERVICENAME"];
            userName = string.Format(@"NT SERVICE\{0}", serviceName);
            groupName = string.Format("{0} Users", serviceName);
            UserInfo.CreateLocalGroup(groupName, string.Format("Members in this group have the necessary rights to interact with the {0} service.", serviceName));
            UserInfo.AddUserToLocalGroup(groupName, userName);
            UserInfo.AddUserToLocalGroup("Performance Log Users", userName);

            session.Log("End ServiceGroupAction");

            return ActionResult.Success;
        }

        #region [ Interop ]

        private const int SC_MANAGER_ALL_ACCESS = 0xF003F;
        private const int SERVICE_ALL_ACCESS = 0xF01FF;
        private const int SERVICE_CONFIG_FAILURE_ACTIONS = 0x2;
        private const int SERVICE_CONFIG_FAILURE_ACTIONS_FLAG = 0x4;

        // The worker method to set all the extension properties for the service
        private static void UpdateServiceConfig(Session session)
        {
            // The failure actions to be defined for the service
            List<WindowsApi.SC_ACTION> failureActionsList = new List<WindowsApi.SC_ACTION>()
            {
                new WindowsApi.SC_ACTION() { Type = (WindowsApi.SC_ACTION_TYPE)(uint)RecoverAction.Restart, Delay = 2000 },
                new WindowsApi.SC_ACTION() { Type = (WindowsApi.SC_ACTION_TYPE)(uint)RecoverAction.None, Delay = 2000 }
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
                LogInstallMessage(session, EventLogEntryType.Error, "GrantShutdownPrivilege: " + ex.Message);
            }
            finally
            {
                if (processToken != IntPtr.Zero)
                    WindowsApi.CloseHandle(processToken);
            }

            return grantSuccess;
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

        #endregion
    }
}
