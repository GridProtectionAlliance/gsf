//******************************************************************************************************
//  CustomActions.cs - Gbtc
//
//  Copyright © 2013, Grid Protection Alliance.  All Rights Reserved.
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
//  04/08/2013 - Stephen C. Wills
//       Generated original version of source code. Portions of code derived from Code Project article:
//       http://www.codeproject.com/Articles/6164/A-ServiceInstaller-Extension-That-Enables-Recovery
//       written by Neil Baliga
//
//******************************************************************************************************

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using System.Xml.Linq;
using System.Xml.XPath;
using GSF.Data;
using GSF.Identity;
using GSF.Interop;
using GSF.Security.Cryptography;
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
            Logger logger = new Logger(session);

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

            logger.Log("Begin AuthenticateServiceAccountAction");

            serviceAccount = GetPropertyValue(session, "ServiceAccount");
            servicePassword = GetPropertyValue(session, "ServicePassword");

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

            logger.Log("End AuthenticateServiceAccountAction");

            return ActionResult.Success;
        }

        /// <summary>
        /// Custom action to write to an XML file.
        /// </summary>
        /// <param name="session">Session object containing data from the installer.</param>
        /// <returns>Result of the custom action.</returns>
        [CustomAction]
        public static ActionResult XmlFileAction(Session session)
        {
            Logger logger = new Logger(session);

            string filePath;
            string mode;
            string mappings;

            logger.Log("Begin XmlFileAction");

            filePath = GetPropertyValue(session, "FilePath");
            mode = GetPropertyValue(session, "Mode").ToUpper();
            mappings = GetPropertyValue(session, "PropertyMappings");

            if (File.Exists(filePath))
            {
                try
                {
                    XDocument document = XDocument.Load(filePath);

                    foreach (KeyValuePair<string, string> mapping in mappings.ParseKeyValuePairs())
                    {
                        IEnumerable query = (IEnumerable)document.XPathEvaluate(mapping.Value);

                        if (mode != "WRITE")
                        {
                            session[mapping.Key] = query.OfType<XElement>().Select(element => element.Value)
                                .Concat(query.OfType<XAttribute>().Select(attribute => attribute.Value))
                                .FirstOrDefault();
                        }
                        else
                        {
                            string value = GetPropertyValue(session, mapping.Key);

                            query.OfType<XAttribute>().ToList().ForEach(attribute =>
                            {
                                if (!string.IsNullOrEmpty(value))
                                    attribute.Value = value;
                                else
                                    attribute.Remove();
                            });

                            query.OfType<XElement>().ToList().ForEach(element =>
                            {
                                if (!string.IsNullOrEmpty(value))
                                    element.Value = value;
                                else
                                    element.Remove();
                            });

                            document.Save(filePath);
                        }
                    }
                }
                catch (Exception ex)
                {
                    string action = (mode == "WRITE") ? "update" : "read";
                    string message = $"Failed to {action} XML file: {ex.Message}";
                    logger.Log(InstallMessage.Error, message);
                    logger.Log(EventLogEntryType.Error, ex);
                    logger.Log($"FilePath = {filePath}");
                    logger.Log($"PropertyMappings = {mappings}");
                    return ActionResult.Failure;
                }
            }

            logger.Log("End XmlFileAction");

            return ActionResult.Success;
        }

        [CustomAction]
        public static ActionResult ConnectionStringAction(Session session)
        {
            Logger logger = new Logger(session);

            Dictionary<string, string> settings;
            string connectionString;
            string mappings;
            string setting;

            logger.Log("Begin ConnectionStringAction");

            connectionString = GetPropertyValue(session, "ConnectionString");
            mappings = GetPropertyValue(session, "PropertyMappings");
            settings = connectionString.ParseKeyValuePairs();

            foreach (KeyValuePair<string, string> kvp in mappings.ParseKeyValuePairs())
            {
                if (settings.TryGetValue(kvp.Value, out setting))
                    session[kvp.Key] = setting;
                else
                    session[kvp.Key] = null;
            }

            logger.Log("End ConnectionStringAction");

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
            Logger logger = new Logger(session);

            string serviceName;
            string configPath;
            XDocument config;

            logger.Log("Begin CompanyInfoAction");

            serviceName = GetPropertyValue(session, "SERVICENAME");
            configPath = Path.Combine(GetPropertyValue(session, "INSTALLDIR"), $"{serviceName}.exe.config");

            if (File.Exists(configPath))
            {
                config = XDocument.Load(configPath);

                foreach (XElement systemSettings in config.Descendants("systemSettings"))
                {
                    // Search for existing CompanyName settings and update their values
                    foreach (XElement companyNameElement in systemSettings.Elements("add").Where(element => element.Attributes("name").Any(nameAttribute => string.Compare(nameAttribute.Value, "CompanyName", true) == 0)))
                        companyNameElement.Attributes("value").ToList().ForEach(valueAttribute => valueAttribute.Value = GetPropertyValue(session, "COMPANYNAME"));

                    // Search for existing CompanyAcronym settings and update their values
                    foreach (XElement companyAcronymElement in systemSettings.Elements("add").Where(element => element.Attributes("name").Any(nameAttribute => string.Compare(nameAttribute.Value, "CompanyAcronym", true) == 0)))
                        companyAcronymElement.Attributes("value").ToList().ForEach(valueAttribute => valueAttribute.Value = GetPropertyValue(session, "COMPANYACRONYM"));

                    // Add CompanyName setting if no such setting exists
                    if (!systemSettings.Elements("add").Any(element => element.Attributes("name").Any(nameAttribute => string.Compare(nameAttribute.Value, "CompanyName", true) == 0)))
                    {
                        systemSettings.Add(new XElement("add",
                            new XAttribute("name", "CompanyName"),
                            new XAttribute("value", GetPropertyValue(session, "COMPANYNAME")),
                            new XAttribute("description", $"The name of the company who owns this instance of the {serviceName}."),
                            new XAttribute("encrypted", "false")
                        ));
                    }

                    // Add CompanyAcronym setting if no such setting exists
                    if (!systemSettings.Elements("add").Any(element => element.Attributes("name").Any(nameAttribute => string.Compare(nameAttribute.Value, "CompanyAcronym", true) == 0)))
                    {
                        systemSettings.Add(new XElement("add",
                            new XAttribute("name", "CompanyAcronym"),
                            new XAttribute("value", GetPropertyValue(session, "COMPANYACRONYM")),
                            new XAttribute("description", $"The acronym representing the company who owns this instance of the {serviceName}."),
                            new XAttribute("encrypted", "false")
                        ));
                    }
                }

                config.Save(configPath);
            }

            logger.Log("End CompanyInfoAction");

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
            Logger logger = new Logger(session);

            logger.Log("Begin ConfigureServiceAction");
            UpdateServiceConfig(session, logger);
            logger.Log("End ConfigureServiceAction");
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
            Logger logger = new Logger(session);

            string serviceName;
            string serviceAccount;
            string servicePorts;
            string groupName;

            logger.Log("Begin ServiceAccountAction");

            // Get properties from the installer session
            serviceName = GetPropertyValue(session, "SERVICENAME");
            serviceAccount = GetPropertyValue(session, "SERVICEACCOUNT");
            servicePorts = GetPropertyValue(session, "HTTPSERVICEPORTS");
            groupName = $"{serviceName} Admins";

            // Create service admins group and add service account to that group as well as the Performance Log Users group
            try
            {
                logger.Log($"Adding {serviceAccount} user to {groupName} group...");
                UserInfo.CreateLocalGroup(groupName, $"Members in this group have the necessary rights to administrate the {serviceName} service.");
                UserInfo.AddUserToLocalGroup(groupName, serviceAccount);
                UserInfo.AddUserToLocalGroup("Performance Log Users", serviceAccount);
                UserInfo.AddUserToLocalGroup("Performance Monitor Users", serviceAccount);
                AddPrivileges(serviceAccount, "SeServiceLogonRight");
                logger.Log($"Done adding {serviceAccount} user to {groupName} group.");
            }
            catch (Exception ex)
            {
                string message = $"Failed to add {serviceAccount} user to {groupName} group!";
                logger.Log(InstallMessage.Error, message);
                logger.Log(EventLogEntryType.Error, ex);
            }

            // Attempt to grant rights to start and stop the service
            try
            {
                string accountSID = UserInfo.UserNameToSID(serviceAccount);
                string groupSID = UserInfo.GroupNameToSID(groupName);
                string acl;

                using (Process process = new Process())
                {
                    process.StartInfo.FileName = "sc";
                    process.StartInfo.Arguments = $"sdshow {serviceName}";
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.RedirectStandardOutput = true;
                    process.Start();

                    acl = process.StandardOutput.ReadToEnd().Trim();
                    process.WaitForExit();

                    if (process.ExitCode != 0)
                        throw new Exception($"Non-zero exit code ({process.ExitCode}) returned from 'sc sdshow {serviceName}'.");
                }

                int index = acl.IndexOf("S:");

                if (accountSID.StartsWith("S-") && !acl.Contains(accountSID))
                    acl = acl.Insert(index, $"(A;;RPWPCR;;;{accountSID})");

                if (groupSID.StartsWith("S-") && !acl.Contains(groupSID))
                    acl = acl.Insert(index, $"(A;;RPWPCR;;;{groupSID})");

                using (Process process = Process.Start("sc", $"sdset {serviceName} \"{acl}\""))
                {
                    process.WaitForExit();

                    if (process.ExitCode != 0)
                        throw new Exception($"Non-zero exit code ({process.ExitCode}) returned from 'sc sdset {serviceName} \"{acl}\"'.");
                }
            }
            catch (Exception ex)
            {
                string message = $"Failed to grant {serviceAccount} rights to restart the service!";
                logger.Log(InstallMessage.Error, message);
                logger.Log(EventLogEntryType.Error, ex);
            }

            if (!string.IsNullOrEmpty(servicePorts))
            {
                logger.Log("Adding namespace reservations for default web services...");

                foreach (string endPoint in servicePorts.Split(',').Select(p => GetEndPoint(p.Trim())))
                {
                    // Remove existing HTTP namespace reservations in case
                    // the current user does not match the service account
                    RemoveHttpNamespaceReservation(endPoint);
                    AddHttpNamespaceReservation(serviceAccount, endPoint);
                }

                logger.Log("Done adding namespace reservations for default web services.");
            }

            logger.Log("End ServiceAccountAction");

            return ActionResult.Success;
        }

        /// <summary>
        /// Prompts the user to select a file via the open file dialog.
        /// </summary>
        /// <param name="session">Session object containing data from the installer.</param>
        /// <returns>Result of the custom action.</returns>
        [CustomAction]
        public static ActionResult BrowseFileAction(Session session)
        {
            Logger logger = new Logger(session);

            Thread staThread;

            logger.Log("Begin BrowseFileAction");

            staThread = new Thread(() =>
            {
                using (OpenFileDialog openFileDialog = new OpenFileDialog())
                {
                    openFileDialog.CheckFileExists = true;
                    openFileDialog.FileName = GetPropertyValue(session, "SelectedFile");
                    openFileDialog.DefaultExt = GetPropertyValue(session, "BrowseFileExtension");
                    openFileDialog.Filter = $"{GetPropertyValue(session, "BrowseFileExtension").ToUpper()} Files|*.{GetPropertyValue(session, "BrowseFileExtension")}|All Files|*.*";

                    if (openFileDialog.ShowDialog() == DialogResult.OK)
                        session["SELECTEDFILE"] = openFileDialog.FileName;
                    else
                        session["SELECTEDFILE"] = null;
                }
            });

            staThread.SetApartmentState(ApartmentState.STA);
            staThread.Start();
            staThread.Join();

            logger.Log("End BrowseFileAction");

            return ActionResult.Success;
        }

        /// <summary>
        /// Determines whether a file exists.
        /// </summary>
        /// <param name="session">Session object containing data from the installer.</param>
        /// <returns>Result of the custom action.</returns>
        [CustomAction]
        public static ActionResult CheckFileExistenceAction(Session session)
        {
            Logger logger = new Logger(session);

            logger.Log("Begin CheckFileExistenceAction");
            session["FILEEXISTS"] = File.Exists(GetPropertyValue(session, "FILEPATH")) ? "yes" : null;
            logger.Log("End CheckFileExistenceAction");
            return ActionResult.Success;
        }

        /// <summary>
        /// Extracts a zip file to a destination folder.
        /// </summary>
        /// <param name="session">Session object containing data from the installer.</param>
        /// <returns>Result of the custom action.</returns>
        [CustomAction]
        public static ActionResult UnzipAction(Session session)
        {
            Logger logger = new Logger(session);

            string zipFile;
            string sourceDir;
            string destinationDir;

            ZipArchive archive;
            string directoryPath;
            string filePath;

            logger.Log("Begin UnzipAction");

            zipFile = GetPropertyValue(session, "ZIPFILE");
            sourceDir = GetPropertyValue(session, "SOURCEDIR") ?? string.Empty;
            destinationDir = GetPropertyValue(session, "DESTINATIONDIR") ?? string.Empty;

            sourceDir = sourceDir.Replace('\\', '/').EnsureEnd('/');

            archive = ZipFile.OpenRead(zipFile);

            foreach (ZipArchiveEntry entry in archive.Entries.Where(entry => entry.FullName.StartsWith(sourceDir)))
            {
                if (entry.FullName.EndsWith("/"))
                    continue;

                filePath = Path.Combine(destinationDir, entry.FullName.Substring(sourceDir.Length).Replace('/', '\\'));
                directoryPath = Path.GetDirectoryName(filePath);

                if ((object)directoryPath == null)
                    continue;

                if (!Directory.Exists(directoryPath))
                    Directory.CreateDirectory(directoryPath);

                using (Stream stream = entry.Open())
                using (FileStream fileStream = File.Create(filePath))
                {
                    stream.CopyTo(fileStream);
                }
            }

            logger.Log("End UnzipAction");

            return ActionResult.Success;
        }

        /// <summary>
        /// Tests a connection to a database server.
        /// </summary>
        /// <param name="session">Session object containing data from the installer.</param>
        /// <returns>Result of the custom action.</returns>
        [CustomAction]
        public static ActionResult PasswordGenerationAction(Session session)
        {
            Logger logger = new Logger(session);

            int passwordLength;

            logger.Log("Begin PasswordGenerationAction");

            if (int.TryParse(GetPropertyValue(session, "GenPasswordLength"), out passwordLength))
                session["GENERATEDPASSWORD"] = PasswordGenerator.Default.GeneratePassword(passwordLength);
            else
                session["GENERATEDPASSWORD"] = PasswordGenerator.Default.GeneratePassword();

            logger.Log("End PasswordGenerationAction");

            return ActionResult.Success;
        }

        /// <summary>
        /// Tests a connection to a database server.
        /// </summary>
        /// <param name="session">Session object containing data from the installer.</param>
        /// <returns>Result of the custom action.</returns>
        [CustomAction]
        public static ActionResult TestDatabaseConnectionAction(Session session)
        {
            Logger logger = new Logger(session);

            string connectionString;
            string dataProviderString;

            logger.Log("Begin TestDatabaseConnectionAction");

            // Get properties from the installer session
            connectionString = GetPropertyValue(session, "ConnectionString");
            dataProviderString = GetPropertyValue(session, "DataProviderString");

            try
            {
                // Execute the database script
                using (new AdoDataConnection(connectionString, dataProviderString))
                {
                }

                session["DATABASECONNECTED"] = "yes";
            }
            catch (Exception ex)
            {
                // Log the error and return failure code
                logger.Log(EventLogEntryType.Error, ex);
                logger.Log(EventLogEntryType.Error, $"Connection string: {connectionString}{Environment.NewLine}Data provider string: {dataProviderString}");
                session["DATABASECONNECTED"] = null;
            }

            logger.Log("End TestDatabaseConnectionAction");

            return ActionResult.Success;
        }

        /// <summary>
        /// Custom action to execute a database script during installation.
        /// </summary>
        /// <param name="session">Session object containing data from the installer.</param>
        /// <returns>Result of the custom action.</returns>
        [CustomAction]
        public static ActionResult DatabaseQueryAction(Session session)
        {
            Logger logger = new Logger(session);

            string connectionString;
            string dataProviderString;
            string query;

            logger.Log("Begin DatabaseQueryAction");

            // Get properties from the installer session
            connectionString = GetPropertyValue(session, "CONNECTIONSTRING");
            dataProviderString = GetPropertyValue(session, "DATAPROVIDERSTRING");
            query = GetPropertyValue(session, "DBQUERY");

            try
            {
                // Execute the database script
                using (AdoDataConnection connection = new AdoDataConnection(connectionString, dataProviderString))
                {
                    connection.ExecuteNonQuery(query);
                }
            }
            catch (Exception ex)
            {
                // Log the error and return failure code
                string message = $"Failed to execute database query: {ex.Message}.";
                logger.Log(InstallMessage.Error, message);
                logger.Log(EventLogEntryType.Error, ex);

                logger.Log(InstallMessage.Error,
                    $"Database Query: {query}{Environment.NewLine}" +
                    $"Connection string: {connectionString}{Environment.NewLine}" +
                    $"Data provider string: {dataProviderString}");

                return ActionResult.Failure;
            }

            logger.Log("End DatabaseQueryAction");

            return ActionResult.Success;
        }

        /// <summary>
        /// Custom action to execute a database script during installation.
        /// </summary>
        /// <param name="session">Session object containing data from the installer.</param>
        /// <returns>Result of the custom action.</returns>
        [CustomAction]
        public static ActionResult DatabaseScriptAction(Session session)
        {
            Logger logger = new Logger(session);

            string connectionString;
            string dataProviderString;
            string scriptPath;

            logger.Log("Begin DatabaseScriptAction");

            // Get properties from the installer session
            connectionString = GetPropertyValue(session, "CONNECTIONSTRING");
            dataProviderString = GetPropertyValue(session, "DATAPROVIDERSTRING");
            scriptPath = GetPropertyValue(session, "SCRIPTPATH");

            try
            {
                // Execute the database script
                using (AdoDataConnection connection = new AdoDataConnection(connectionString, dataProviderString))
                {
                    connection.ExecuteScript(scriptPath);
                }
            }
            catch (Exception ex)
            {
                // Log the error and return failure code
                string message = $"Failed to execute database script: {ex.Message}.";
                logger.Log(InstallMessage.Error, message);
                logger.Log(EventLogEntryType.Error, ex);

                logger.Log(InstallMessage.Error,
                    $"Database Script: {scriptPath}{Environment.NewLine}" +
                    $"Connection string: {connectionString}{Environment.NewLine}" +
                    $"Data provider string: {dataProviderString}");

                return ActionResult.Failure;
            }

            logger.Log("End DatabaseScriptAction");

            return ActionResult.Success;
        }

        /// <summary>
        /// Custom action to start a process.
        /// </summary>
        /// <param name="session">Session object containing data from the installer.</param>
        /// <returns>Result of the custom action.</returns>
        [CustomAction]
        public static ActionResult StartProcessAction(Session session)
        {
            Logger logger = new Logger(session);

            string processStartInfo;
            Dictionary<string, string> infoLookup;
            Action<string, Action<string>> findAndExecute;
            ProcessStartInfo info;

            logger.Log("Begin StartProcessAction");

            // Get properties from the installer session
            processStartInfo = GetPropertyValue(session, "ProcessStartInfo");

            try
            {
                infoLookup = processStartInfo.ParseKeyValuePairs();

                findAndExecute = (key, action) =>
                {
                    string value;

                    if (infoLookup.TryGetValue(key, out value))
                        action(value);
                };

                info = new ProcessStartInfo();

                findAndExecute("FileName", value => info.FileName = value);
                findAndExecute("Arguments", value => info.Arguments = value);
                findAndExecute("WorkingDirectory", value => info.WorkingDirectory = value);
                findAndExecute("Verb", value => info.Verb = value);
                findAndExecute("WindowStyle", value => info.WindowStyle = (ProcessWindowStyle)Enum.Parse(typeof(ProcessWindowStyle), value));
                findAndExecute("UserName", value => info.UserName = value);
                findAndExecute("Password", value => info.Password = value.ToSecureString());
                findAndExecute("CreateNoWindow", value => info.CreateNoWindow = Convert.ToBoolean(value));
                findAndExecute("LoadUserProfile", value => info.LoadUserProfile = Convert.ToBoolean(value));
                findAndExecute("UseShellExecute", value => info.UseShellExecute = Convert.ToBoolean(value));

                // Start the process
                using (Process process = Process.Start(info))
                {
                    findAndExecute("WaitForExit", value =>
                    {
                        process.OutputDataReceived += (sender, args) => logger.Log(args.Data);

                        process.ErrorDataReceived += (sender, args) =>
                        {
                            string message = $"Error in executing process: {args.Data}";
                            logger.Log(InstallMessage.Error, message);
                            logger.Log(EventLogEntryType.Error, message);
                        };

                        process.WaitForExit();
                    });
                }
            }
            catch (Exception ex)
            {
                // Log the error and return failure code
                string message = $"Failed to start process: {ex.Message}";
                logger.Log(InstallMessage.Error, message);
                logger.Log(EventLogEntryType.Error, ex);
                return ActionResult.Failure;
            }

            logger.Log("End StartProcessAction");

            return ActionResult.Success;
        }

        // Optionally port specification can include interface, e.g.: 127.0.0.1:8005 or +:8181,
        // when only a port is specified, "+" is assumed meaning bind to all interfaces
        private static string GetEndPoint(string servicePort)
        {
            return servicePort.IndexOf(':') == -1 ? $"+:{servicePort}" : servicePort;
        }

        // Create an http namespace reservation
        private static void AddHttpNamespaceReservation(string serviceAccount, string endPoint)
        {
            ProcessStartInfo psi;
            string parameters;

            // Vista, Windows 2008, Window 7, etc use "netsh" for reservations
            parameters = $@"http add urlacl url=http://{endPoint}/ user=""{serviceAccount}""";

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
        private static void RemoveHttpNamespaceReservation(string endPoint)
        {
            ProcessStartInfo psi;
            string parameters;

            // Vista, Windows 2008, Window 7, etc use "netsh" for reservations
            parameters = $@"http delete urlacl url=http://{endPoint}";

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

        // Method to get the value of a property
        private static string GetPropertyValue(Session session, string name)
        {
            string value;

            if (session.CustomActionData.TryGetValue(name, out value))
                return value;

            if (!string.IsNullOrEmpty(session[name]))
                return session[name];

            return session[name.ToUpper()];
        }

        // Method to determine whether the custom action is immediate
        private static bool IsImmediate(Session session)
        {
            return
                !session.GetMode(InstallRunMode.Scheduled) &&
                !session.GetMode(InstallRunMode.Rollback) &&
                !session.GetMode(InstallRunMode.Commit);
        }

        private class Logger
        {
            private Session m_session;
            private string m_serviceName;
            private bool m_isImmediate;

            public Logger(Session session)
            {
                m_session = session;
                m_serviceName = GetPropertyValue(session, "SERVICENAME");
                m_isImmediate = IsImmediate(session);
            }

            public void Log(string message)
            {
                Log(InstallMessage.Info, message);
            }

            public void Log(InstallMessage messageType, string message)
            {
                using (Record record = new Record(0))
                {
                    // Square brackets and curly braces are evaluated upon logging the message so we escape them here
                    record.FormatString = Regex.Replace(message, @"[\[\]{}]", @"[\$&]");

                    // session.Log and session.Message both make use of MsiProcessMessage, which cannot be used during a DoAction ControlEvent.
                    // https://msdn.microsoft.com/en-us/library/aa368322(VS.85).aspx
                    //
                    // Setting the value of a property generates a message in the log file so we use that as a workaround.
                    // However, you cannot set session properties unless the custom action is an immediate custom action.
                    if (m_session.Message(messageType, record) == MessageResult.None && m_isImmediate)
                        m_session["LOGMESSAGE"] = message;
                }
            }

            public void Log(EventLogEntryType messageType, Exception ex)
            {
                Log(messageType, ex.ToString());
            }

            public void Log(EventLogEntryType messageType, string message)
            {
                try
                {
                    EventLog.WriteEntry(m_serviceName, message, messageType);
                }
                catch (Exception ex)
                {
                    Log(InstallMessage.Error, ex.ToString());
                }
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
        private static void UpdateServiceConfig(Session session, Logger logger)
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
            string serviceName = GetPropertyValue(session, "SERVICENAME");

            // Err check var
            bool result;

            // Place all our code in a try block
            try
            {
                // Open the service control manager
                serviceManagerHandle = WindowsApi.OpenSCManager(null, null, SC_MANAGER_ALL_ACCESS);

                if (serviceManagerHandle.ToInt32() <= 0)
                {
                    string message = "UpdateServiceConfig: Failed to Open Service Control Manager";
                    logger.Log(InstallMessage.Error, message);
                    logger.Log(EventLogEntryType.Error, message);
                    return;
                }

                // Lock the Service Database
                serviceLockHandle = WindowsApi.LockServiceDatabase(serviceManagerHandle);

                if (serviceLockHandle.ToInt32() <= 0)
                {
                    string message = "UpdateServiceConfig: Failed to Lock Service Database for Write";
                    logger.Log(InstallMessage.Error, message);
                    logger.Log(EventLogEntryType.Error, message);
                    return;
                }

                // Open the service
                serviceHandle = WindowsApi.OpenService(serviceManagerHandle, serviceName, SERVICE_ALL_ACCESS);

                if (serviceHandle.ToInt32() <= 0)
                {
                    string message = "UpdateServiceConfig: Failed to Open Service";
                    logger.Log(InstallMessage.Error, message);
                    logger.Log(EventLogEntryType.Error, message);
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
                    result = GrantShutdownPrivilege(session, logger);

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

                logger.Log(EventLogEntryType.Information, "UpdateServiceConfig: Successfully configured service failure actions");

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
                logger.Log(InstallMessage.Error, ex.Message);
                logger.Log(EventLogEntryType.Error, ex.Message);
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
        // https://msdn.microsoft.com/en-us/library/windows/desktop/aa376871(v=vs.85).aspx
        private static bool GrantShutdownPrivilege(Session session, Logger logger)
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
                string message = "GrantShutdownPrivilege: " + ex.Message;
                logger.Log(InstallMessage.Error, message);
                logger.Log(EventLogEntryType.Error, ex);
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
