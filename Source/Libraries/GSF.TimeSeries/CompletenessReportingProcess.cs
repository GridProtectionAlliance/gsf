//******************************************************************************************************
//  ReportingProcess.cs - Gbtc
//
//  Copyright © 2014, Grid Protection Alliance.  All Rights Reserved.
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
//  03/05/2014 - Stephen C. Wills
//       Generated original version of source code.
//  04/29/2015 - J. Ritchie Carroll
//       Added report e-mail parameters.
//
//******************************************************************************************************

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Security.Principal;
using System.Text.RegularExpressions;
using GSF.Configuration;
using GSF.IO;
using GSF.Threading;

namespace GSF.TimeSeries
{
    /// <summary>
    /// Represents the process that generates reports for the time-series service.
    /// </summary>
    public class CompletenessReportingProcess : IPersistSettings
    {
        #region [ Members ]

        // Fields
        private readonly ConcurrentQueue<DateTime> m_reportGenerationQueue;
        private readonly LongSynchronizedOperation m_executeOperation;

        private bool m_persistSettings;
        private string m_settingsCategory;

        private string m_archiveFilePath;
        private string m_reportLocation;
        private string m_title;
        private string m_company;
        private double m_level4Threshold;
        private double m_level3Threshold;
        private string m_level4Alias;
        private string m_level3Alias;
        private double m_idleReportLifetime;
        private bool m_enableReportEmail;
        private string m_smtpServer;
        private string m_fromAddress;
        private string m_toAddresses;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new instance of the <see cref="CompletenessReportingProcess"/> class.
        /// </summary>
        public CompletenessReportingProcess()
        {
            m_reportGenerationQueue = new ConcurrentQueue<DateTime>();
            m_executeOperation = new LongSynchronizedOperation(Execute)
            {
                IsBackground = true
            };

            m_archiveFilePath = "Eval(statArchiveFile.FileName)";
            m_reportLocation = "Reports";
            m_title = "Eval(securityProvider.ApplicationName) Completeness Report";
            m_company = "Eval(systemSettings.CompanyName)";
            m_level4Threshold = 99.0D;
            m_level3Threshold = 90.0D;
            m_level4Alias = "Good";
            m_level3Alias = "Fair";
            m_idleReportLifetime = 14.0D;
            m_enableReportEmail = false;
            m_smtpServer = "localhost";
            m_fromAddress = "reports@acme.com";
            m_toAddresses = "wile.e.coyote@acme.com";
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Determines whether the object settings are to be persisted to the config file.
        /// </summary>
        public bool PersistSettings
        {
            get
            {
                return m_persistSettings;
            }
            set
            {
                m_persistSettings = value;
            }
        }

        /// <summary>
        /// Gets or sets the category name under which the object settings are persisted in the config file.
        /// </summary>
        public string SettingsCategory
        {
            get
            {
                return m_settingsCategory;
            }
            set
            {
                m_settingsCategory = value;
            }
        }

        /// <summary>
        /// Gets or sets the path to the archive file to which
        /// the statistics required for reporting are archived.
        /// </summary>
        public string ArchiveFilePath
        {
            get
            {
                return m_archiveFilePath;
            }
            set
            {
                m_archiveFilePath = value;
            }
        }

        /// <summary>
        /// Gets or sets the directory to which reports will be written.
        /// </summary>
        public string ReportLocation
        {
            get
            {
                return m_reportLocation;
            }
            set
            {
                m_reportLocation = value;
            }
        }

        /// <summary>
        /// Gets or sets the title to be displayed on reports.
        /// </summary>
        public string Title
        {
            get
            {
                return m_title;
            }
            set
            {
                m_title = value;
            }
        }

        /// <summary>
        /// Gets or sets the name of the company to be displayed on reports.
        /// </summary>
        public string Company
        {
            get
            {
                return m_company;
            }
            set
            {
                m_company = value;
            }
        }

        /// <summary>
        /// Gets or sets the minimum percentage of measurements received from devices in level 4.
        /// </summary>
        public double Level4Threshold
        {
            get
            {
                return m_level4Threshold;
            }
            set
            {
                m_level4Threshold = value;
            }
        }

        /// <summary>
        /// Gets or sets the minimum percentage of measurements received from devices in level 3.
        /// </summary>
        public double Level3Threshold
        {
            get
            {
                return m_level3Threshold;
            }
            set
            {
                m_level3Threshold = value;
            }
        }

        /// <summary>
        /// Gets or sets the alias for the level 4 category.
        /// </summary>
        public string Level4Alias
        {
            get
            {
                return m_level4Alias;
            }
            set
            {
                m_level4Alias = value;
            }
        }

        /// <summary>
        /// Gets or sets the alias for the level 3 category.
        /// </summary>
        public string Level3Alias
        {
            get
            {
                return m_level3Alias;
            }
            set
            {
                m_level3Alias = value;
            }
        }

        /// <summary>
        /// Gets or sets the minimum lifetime of a report
        /// since the last time it was accessed, in days.
        /// </summary>
        public double IdleReportLifetime
        {
            get
            {
                return m_idleReportLifetime;
            }
            set
            {
                m_idleReportLifetime = value;
            }
        }

        /// <summary>
        /// Gets or sets flag to enable e-mailing of reports.
        /// </summary>
        public bool EnableReportEmail
        {
            get
            {
                return m_enableReportEmail;
            }
            set
            {
                m_enableReportEmail = value;
            }
        }

        /// <summary>
        /// Gets or sets SMTP server to use when e-mailing reports.
        /// </summary>
        public string SmtpServer
        {
            get
            {
                return m_smtpServer;
            }
            set
            {
                m_smtpServer = value;
            }
        }

        /// <summary>
        /// Gets or sets the "from" address to use when e-mailing reports.
        /// </summary>
        public string FromAddress
        {
            get
            {
                return m_fromAddress;
            }
            set
            {
                m_fromAddress = value;
            }
        }

        /// <summary>
        /// Gets or sets the comma separated "to" addresses to use when e-mailing reports. 
        /// </summary>
        public string ToAddresses
        {
            get
            {
                return m_toAddresses;
            }
            set
            {
                m_toAddresses = value;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Returns the list of reports that are available from the report location.
        /// </summary>
        /// <returns>The list of generated reports.</returns>
        public List<string> GetReportsList()
        {
            string reportLocation = FilePath.GetAbsolutePath(m_reportLocation)
                .EnsureEnd(Path.DirectorySeparatorChar);

            if (Directory.Exists(reportLocation))
            {
                return FilePath.GetFileList(reportLocation)
                    .Select(FilePath.GetFileName)
                    .Where(IsReportFileName)
                    .ToList();
            }

            return new List<string>();
        }

        /// <summary>
        /// Returns the list of reports which are in the queue but are yet to be generated.
        /// </summary>
        /// <returns>The list of pending reports.</returns>
        public List<string> GetPendingReportsList()
        {
            return m_reportGenerationQueue.ToArray()
                .Select(reportDate => string.Format("{0} {1:yyyy-MM-dd}.pdf", m_title, reportDate))
                .ToList();
        }

        /// <summary>
        /// Queues up a report to be generated on a separate thread.
        /// </summary>
        /// <param name="reportDate">The date of the report to be generated.</param>
        public void GenerateReport(DateTime reportDate)
        {
            // ToArray is a thread-safe operation on ConcurrentQueue
            // whereas using an enumerator directly on a ConcurrentQueue
            // can cause collection modified errors while iterating
            if (!m_reportGenerationQueue.ToArray().Contains(reportDate))
            {
                m_reportGenerationQueue.Enqueue(reportDate);
                m_executeOperation.RunOnceAsync();
            }
        }

        /// <summary>
        /// Deletes reports from the <see cref="ReportLocation"/> that have
        /// been idle for the length of the <see cref="IdleReportLifetime"/>.
        /// </summary>
        public void CleanReportLocation()
        {
            FileInfo info;

            foreach (string report in GetReportsList())
            {
                if (IsReportFileName(report))
                {
                    info = new FileInfo(FilePath.GetAbsolutePath(Path.Combine(m_reportLocation, report)));

                    if ((DateTime.UtcNow - info.LastAccessTimeUtc).TotalDays > m_idleReportLifetime)
                        File.Delete(info.FullName);
                }
            }
        }

        /// <summary>
        /// Loads saved settings from the config file.
        /// </summary>
        public void LoadSettings()
        {
            if (!m_persistSettings)
                return;

            // Ensure that settings category is specified.
            if (string.IsNullOrEmpty(m_settingsCategory))
                throw new ConfigurationErrorsException("SettingsCategory property has not been set");

            // Load settings from the specified category.
            ConfigurationFile config = ConfigurationFile.Current;
            CategorizedSettingsElementCollection settings = config.Settings[m_settingsCategory];

            settings.Add("ArchiveFilePath", m_archiveFilePath, "Path to the archive file to which the statistics required for reporting are archived.");
            settings.Add("ReportLocation", m_reportLocation, "Directory to which reports will be written.");
            settings.Add("Title", m_title, "Title to be displayed on reports.");
            settings.Add("Company", m_company, "Name of the company to be displayed on reports.");
            settings.Add("Level4Threshold", m_level4Threshold, "Minimum percentage of measurements received from devices in level 4.");
            settings.Add("Level3Threshold", m_level3Threshold, "Minimum percentage of measurements received from devices in level 3.");
            settings.Add("Level4Alias", m_level4Alias, "Alias for the level 4 category.");
            settings.Add("Level3Alias", m_level3Alias, "Alias for the level 3 category.");
            settings.Add("IdleReportLifetime", m_idleReportLifetime, "The minimum lifetime of a report since the last time it was accessed, in days.");
            settings.Add("EnableReportEmail", m_enableReportEmail, "Set to true to enable daily e-mailing of reports.");
            settings.Add("SmtpServer", m_smtpServer, "The SMTP relay server from which to send e-mails.");
            settings.Add("FromAddress", m_fromAddress, "The from address for the report e-mails.");
            settings.Add("ToAddresses", m_toAddresses, "Comma separated list of destination addresses for the report e-mails.");

            ArchiveFilePath = settings["ArchiveFilePath"].ValueAs(m_archiveFilePath);
            ReportLocation = settings["ReportLocation"].ValueAs(m_reportLocation);
            Title = settings["Title"].ValueAs(m_title);
            Company = settings["Company"].ValueAs(m_company);
            Level4Threshold = settings["Level4Threshold"].ValueAs(m_level4Threshold);
            Level3Threshold = settings["Level3Threshold"].ValueAs(m_level3Threshold);
            Level4Alias = settings["Level4Alias"].ValueAs(m_level4Alias);
            Level3Alias = settings["Level3Alias"].ValueAs(m_level3Alias);
            IdleReportLifetime = settings["IdleReportLifetime"].ValueAs(m_idleReportLifetime);
            EnableReportEmail = settings["EnableReportEmail"].ValueAsBoolean();
            SmtpServer = settings["SmtpServer"].ValueAs(m_smtpServer);
            FromAddress = settings["FromAddress"].ValueAs(m_fromAddress);
            ToAddresses = settings["ToAddresses"].ValueAs(m_toAddresses);
        }

        /// <summary>
        /// Saves settings to the config file.
        /// </summary>
        public void SaveSettings()
        {
            if (!m_persistSettings)
                return;

            // Ensure that settings category is specified.
            if (string.IsNullOrEmpty(m_settingsCategory))
                throw new ConfigurationErrorsException("SettingsCategory property has not been set");

            // Save settings under the specified category.
            ConfigurationFile config = ConfigurationFile.Current;
            CategorizedSettingsElementCollection settings = config.Settings[m_settingsCategory];
            settings["ArchiveFilePath", true].Update(m_archiveFilePath);
            settings["ReportLocation", true].Update(m_reportLocation);
            settings["Title", true].Update(m_title);
            settings["Company", true].Update(m_company);
            settings["Level4Threshold", true].Update(m_level4Threshold);
            settings["Level3Threshold", true].Update(m_level3Threshold);
            settings["Level4Alias", true].Update(m_level4Alias);
            settings["Level3Alias", true].Update(m_level3Alias);
            settings["IdleReportLifetime", true].Update(m_idleReportLifetime);
            settings["EnableReportEmail", true].Update(m_enableReportEmail);
            settings["SmtpServer", true].Update(m_smtpServer);
            settings["FromAddress", true].Update(m_fromAddress);
            settings["ToAddresses", true].Update(m_toAddresses);
            config.Save();
        }

        /// <summary>
        /// Executes the reporting process to generate the report.
        /// </summary>
        private void Execute()
        {
            WindowsIdentity currentOwner = WindowsIdentity.GetCurrent();
            DateTime reportDate;

            if ((object)currentOwner != null)
            {
                // Wait for existing processes to exit, for instance if the
                // service was restarted while a report was being generated
                foreach (Process process in Process.GetProcessesByName("StatHistorianReportGenerator.exe"))
                {
                    if (currentOwner.Name == GetProcessOwner(process.Id))
                        process.WaitForExit();
                }
            }

            while (m_reportGenerationQueue.TryPeek(out reportDate))
            {
                // Execute the reporting process
                using (Process process = new Process())
                {
                    process.StartInfo.FileName = FilePath.GetAbsolutePath("StatHistorianReportGenerator.exe");
                    process.StartInfo.Arguments = GetArguments(reportDate);
                    process.Start();
                    process.WaitForExit();
                    process.Close();
                }

                // Dequeue only after the report is generated so that it
                // remains in the list of pending reports during generation
                m_reportGenerationQueue.TryDequeue(out reportDate);
            }
        }

        /// <summary>
        /// Gets the command line arguments for the reporting process.
        /// </summary>
        public string GetArguments()
        {
            // Because we may have a archive location like "C:\Program Files\MyPath" with quotes,
            // the arguments below have an extra leading and trailing space around quoted values
            // for proper parsing.
            return string.Format(
                 "--archiveLocation=\" {0} \" " +
                 "--reportLocation=\" {1} \" " +
                 "--title=\" {2} \" " +
                 "--company=\" {3} \" " +
                 "--level4threshold=\" {4} \" " +
                 "--level3threshold=\" {5} \" " +
                 "--level4alias=\" {6} \" " +
                 "--level3alias=\" {7} \"",
                 FilePath.GetDirectoryName(ArchiveFilePath).Replace("\"", "\\\""),
                 ReportLocation.Replace("\"", "\\\""),
                 Title.Replace("\"", "\\\""),
                 Company.Replace("\"", "\\\""),
                 Level4Threshold,
                 Level3Threshold,
                 Level4Alias.Replace("\"", "\\\""),
                 Level3Alias.Replace("\"", "\\\""));
        }

        /// <summary>
        /// Gets the command line arguments for the reporting process for a given report date.
        /// </summary>
        public string GetArguments(DateTime reportDate)
        {
            string arguments = string.Format(
                "{0} " +
                "--reportDate=\" {1:yyyy-MM-dd} \"",
                GetArguments(),
                reportDate);

            if (EnableReportEmail)
                arguments = string.Format(
                    "{0} " +
                    "--smtpServer=\" {1} \" " +
                    "--fromAddress=\" {2} \" " +
                    "--toAddresses=\" {3} \"",
                    arguments,
                    SmtpServer,
                    FromAddress,
                    ToAddresses);

            return arguments;
        }

        /// <summary>
        /// Determines whether the given path is a path to a report, based on the file name.
        /// </summary>
        private bool IsReportFileName(string fileName)
        {
            string regex = string.Format(@"{0} (?<Date>[^.]+)\.pdf", m_title);
            Match match = Regex.Match(fileName, regex);
            DateTime reportDate;

            return match.Success && DateTime.TryParse(match.Groups["Date"].Value, out reportDate);
        }

        /// <summary>
        /// Determines who is the owner of the given process.
        /// </summary>
        private string GetProcessOwner(int processId)
        {
            try
            {
                string query = "Select * From Win32_Process Where ProcessID = " + processId;
                ManagementObjectSearcher searcher = new ManagementObjectSearcher(query);
                ManagementObjectCollection processList = searcher.Get();

                object[] argList;
                int returnVal;

                foreach (ManagementObject obj in processList)
                {
                    argList = new object[] { string.Empty, string.Empty };
                    returnVal = Convert.ToInt32(obj.InvokeMethod("GetOwner", argList));

                    if (returnVal == 0)
                        return argList[1] + "\\" + argList[0];
                }

                return "NO OWNER";
            }
            catch
            {
                return "NO OWNER";
            }
        }

        #endregion
    }
}
