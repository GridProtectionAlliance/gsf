//******************************************************************************************************
//  Program.cs - Gbtc
//
//  Copyright © 2020, Grid Protection Alliance.  All Rights Reserved.
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
//  11/06/2020 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Windows.Forms;
using System.Xml.Linq;
using GSF;
using GSF.Configuration;
using GSF.Console;
using GSF.Data;
using GSF.Diagnostics;
using GSF.IO;
using Microsoft.VisualBasic.ApplicationServices;

namespace AdapterExplorer
{
    public enum Status
    {
        Disconnected,
        Connected
    }

    static class Program
    {
        // Static Properties
        public static LogPublisher Log;

        private static string s_hostConfigFileName;
        private static string s_connectionString;
        private static string s_dataProviderString;
        private static Guid s_nodeID;

        public static string HostConfigFileName => Path.GetFileName(s_hostConfigFileName ?? "undefined");
        public static Guid HostNodeID => s_nodeID;

        private class AdapterExplorerApplication : WindowsFormsApplicationBase
        {
            protected override void OnCreateMainForm()
            {
                MainForm = new MainForm();
            }
            protected override void OnCreateSplashScreen()
            {
                SplashScreen = new SplashScreen();
            }
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            string localPath = FilePath.GetAbsolutePath("");
            string logPath = string.Format("{0}{1}Logs{1}", localPath, Path.DirectorySeparatorChar);

            try
            {
                if (!Directory.Exists(logPath))
                    Directory.CreateDirectory(logPath);
            }
            catch
            {
                logPath = localPath;
            }

            Logger.FileWriter.SetPath(logPath);
            Logger.FileWriter.SetLoggingFileCount(10);
            Logger.SuppressFirstChanceExceptionLogMessages();

            Log = Logger.CreatePublisher(typeof(Program), MessageClass.Application);
            Log.Publish(MessageLevel.Info, "ApplicationStart");

            AppDomain.CurrentDomain.SetPrincipalPolicy(PrincipalPolicy.WindowsPrincipal);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            
            AdapterExplorerApplication app = new AdapterExplorerApplication();
            app.Run(Environment.GetCommandLineArgs());
        }

        public static ushort GetGEPPort()
        {
            XDocument serviceConfig = XDocument.Load(GetConfigurationFileName());

            string configurationString = serviceConfig
                .Descendants("internaldatapublisher")
                .SelectMany(systemSettings => systemSettings.Elements("add"))
                .Where(element => "ConfigurationString".Equals((string)element.Attribute("name"), StringComparison.OrdinalIgnoreCase))
                .Select(element => (string)element.Attribute("value"))
                .FirstOrDefault();

            Dictionary<string, string> settings = configurationString.ParseKeyValuePairs();

            if (settings.TryGetValue("port", out string setting) && ushort.TryParse(setting, out ushort port))
                return port;

            return 6165;
        }

        public static AdoDataConnection GetDatabaseConnection()
        {
            string connectionString = s_connectionString;
            string dataProviderString = s_dataProviderString;
            string nodeID = null;

            if (string.IsNullOrWhiteSpace(connectionString) || string.IsNullOrWhiteSpace(dataProviderString))
            {
                XDocument serviceConfig = XDocument.Load(GetConfigurationFileName());

                connectionString = serviceConfig
                    .Descendants("systemSettings")
                    .SelectMany(systemSettings => systemSettings.Elements("add"))
                    .Where(element => "ConnectionString".Equals((string)element.Attribute("name"), StringComparison.OrdinalIgnoreCase))
                    .Select(element => (string)element.Attribute("value"))
                    .FirstOrDefault();

                dataProviderString = serviceConfig
                    .Descendants("systemSettings")
                    .SelectMany(systemSettings => systemSettings.Elements("add"))
                    .Where(element => "DataProviderString".Equals((string)element.Attribute("name"), StringComparison.OrdinalIgnoreCase))
                    .Select(element => (string)element.Attribute("value"))
                    .FirstOrDefault();

                nodeID = serviceConfig
                    .Descendants("systemSettings")
                    .SelectMany(systemSettings => systemSettings.Elements("add"))
                    .Where(element => "NodeID".Equals((string)element.Attribute("name"), StringComparison.OrdinalIgnoreCase))
                    .Select(element => (string)element.Attribute("value"))
                    .FirstOrDefault();
            }

            if (string.IsNullOrWhiteSpace(connectionString) || string.IsNullOrWhiteSpace(dataProviderString))
                return null;

            // Copy target database settings into local user settings
            ConfigurationFile configFile = ConfigurationFile.Current;
            CategorizedSettingsElementCollection configSettings = configFile.Settings["systemSettings"];

            if (nodeID is null)
                nodeID = Guid.NewGuid().ToString();

            configSettings["ConnectionString"].Value = connectionString;
            configSettings["DataProviderString"].Value = dataProviderString;
            configSettings["NodeID"].Value = nodeID;

            s_connectionString = connectionString;
            s_dataProviderString = dataProviderString;
            s_nodeID = Guid.Parse(nodeID);

            return new AdoDataConnection(connectionString, dataProviderString);
        }

        private static string GetConfigurationFileName()
        {
            if (!string.IsNullOrWhiteSpace(s_hostConfigFileName) && File.Exists(s_hostConfigFileName))
                return s_hostConfigFileName;

            string getConfigurationFileName()
            {
                Arguments arguments = new Arguments(Environment.CommandLine, true);

                if (arguments.Count > 0 && File.Exists(arguments["OrderedArg1"]))
                    return arguments["OrderedArg1"];

                // Will be faster to load known config file, try a few common ones first
                string[] knownConfigurationFileNames =
                {
                    "openPDC.exe.config",
                    "SIEGate.exe.config",
                    "openHistorian.exe.config",
                    "substationSBG.exe.config",
                    "openMIC.exe.config",
                    "PDQTracker.exe.config",
                    "openECA.exe.config"
                };

                // Search for the file name in the list of known configuration files
                foreach (string fileName in knownConfigurationFileNames)
                {
                    string absolutePath = FilePath.GetAbsolutePath(fileName);

                    if (File.Exists(absolutePath))
                        return absolutePath;
                }

                // Fall back on manual search
                foreach (string configFile in FilePath.GetFileList($"{FilePath.AddPathSuffix(FilePath.GetAbsolutePath(""))}*.exe.config"))
                {
                    try
                    {
                        XDocument serviceConfig = XDocument.Load(configFile);

                        string applicationName = serviceConfig
                            .Descendants("securityProvider")
                            .SelectMany(systemSettings => systemSettings.Elements("add"))
                            .Where(element => "ApplicationName".Equals((string)element.Attribute("name"), StringComparison.OrdinalIgnoreCase))
                            .Select(element => (string)element.Attribute("value"))
                            .FirstOrDefault();

                        if (string.IsNullOrWhiteSpace(applicationName))
                            continue;

                        if (applicationName.Trim().Equals(FilePath.GetFileNameWithoutExtension(FilePath.GetFileNameWithoutExtension(configFile)), StringComparison.OrdinalIgnoreCase))
                            return configFile;
                    }
                    catch (Exception ex)
                    {
                        // Just move on to the next config file if XML parsing fails
                        Program.Log.Publish(MessageLevel.Warning, "GetConfigurationFileName", $"Failed parse config file \"{configFile}\".", exception: ex);
                    }
                }

                return ConfigurationFile.Current.Configuration.FilePath;
            }

            return s_hostConfigFileName = getConfigurationFileName();
        }
    }
}