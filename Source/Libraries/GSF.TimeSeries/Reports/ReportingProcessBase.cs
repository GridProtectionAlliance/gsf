//******************************************************************************************************
//  ReportingProcessBase.cs - Gbtc
//
//  Copyright © 2015, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  06/08/2015 - J. Ritchie Carroll
//       Generated original version of source code.
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
using System.Security;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using GSF.Configuration;
using GSF.Console;
using GSF.IO;
using GSF.Security.Cryptography;
using GSF.Threading;

namespace GSF.TimeSeries.Reports
{
    /// <summary>
    /// Represents the base functionality for reporting processes.
    /// </summary>
    public abstract class ReportingProcessBase : IReportingProcess
    {
        #region [ Members ]

        // Fields
        private readonly string m_reportType;
        private readonly ConcurrentQueue<Tuple<DateTime, bool>> m_reportGenerationQueue;
        private readonly LongSynchronizedOperation m_executeOperation;
        private bool m_persistSettings;
        private string m_settingsCategory;
        private string m_archiveFilePath;
        private string m_reportLocation;
        private string m_title;
        private string m_company;
        private double m_idleReportLifetime;
        private bool m_enableReportEmail;
        private string m_smtpServer;
        private string m_fromAddress;
        private string m_toAddresses;
        private string m_smtpUsername;
        private SecureString m_smtpPassword;
        private long m_generatedReports;
        private DateTime m_lastReportGenerationTime;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new instance of the <see cref="ReportingProcessBase"/> class.
        /// </summary>
        /// <param name="reportType">Report type - passed into StatHistorianReportGenerator.</param>
        protected ReportingProcessBase(string reportType)
        {
            m_reportType = reportType;
            m_reportGenerationQueue = new ConcurrentQueue<Tuple<DateTime, bool>>();
            m_executeOperation = new LongSynchronizedOperation(Execute)
            {
                IsBackground = true
            };

            m_persistSettings = true;
            m_settingsCategory = string.Format("{0}Reporting", m_reportType);
            m_archiveFilePath = "Eval(statArchiveFile.FileName)";
            m_reportLocation = "Reports";
            m_title = string.Format("Eval(securityProvider.ApplicationName) {0} Report", m_reportType);
            m_company = "Eval(systemSettings.CompanyName)";
            m_idleReportLifetime = 14.0D;
            m_enableReportEmail = false;
            m_smtpServer = "localhost";
            m_fromAddress = "reports@gridprotectionalliance.org";
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
        /// Gets report type, i.e., basically the report name associated with this reporting process.
        /// </summary>
        /// <remarks>
        /// This value is passed to StatHistorianReportGenerator as "reportType" parameter.
        /// </remarks>
        public string ReportType
        {
            get
            {
                return m_reportType;
            }
        }

        string IProvideStatus.Name
        {
            get
            {
                return GetType().Name;
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

        /// <summary>
        /// Gets or sets the username used to authenticate to the SMTP server.
        /// </summary>
        public string SmtpUsername
        {
            get
            {
                return m_smtpUsername;
            }
            set
            {
                m_smtpUsername = value;
            }
        }

        /// <summary>
        /// Gets or sets the password used to authenticate to the SMTP server.
        /// </summary>
        public string SmtpPassword
        {
            get
            {
                return m_smtpPassword.ToUnsecureString();
            }
            set
            {
                m_smtpPassword = value.ToSecureString();
            }
        }

        /// <summary>
        /// Gets or sets the password used to authenticate to the SMTP server as a secure string.
        /// </summary>
        public SecureString SmtpSecurePassword
        {
            get
            {
                return m_smtpPassword;
            }
            set
            {
                m_smtpPassword = value;
            }
        }

        /// <summary>
        /// Gets the current status details about reporting process.
        /// </summary>
        public virtual string Status
        {
            get
            {
                StringBuilder status = new StringBuilder();

                status.AppendFormat("               Report type: {0}", ReportType ?? "undefined");
                status.AppendLine();
                status.AppendFormat("         Archive file path: {0}", FilePath.TrimFileName(ArchiveFilePath ?? "undefined", 51));
                status.AppendLine();
                status.AppendFormat("           Report location: {0}", FilePath.TrimFileName(ReportLocation ?? "undefined", 51));
                status.AppendLine();
                status.AppendFormat("              Report title: {0}", Title ?? "undefined");
                status.AppendLine();
                status.AppendFormat("      Idle report lifetime: {0:N2} days", IdleReportLifetime);
                status.AppendLine();
                status.AppendFormat("            Queued reports: {0:N0}", m_reportGenerationQueue.Count);
                status.AppendLine();
                status.AppendFormat("         Generated reports: {0:N0}", m_generatedReports);
                status.AppendLine();
                status.AppendFormat("    Last report generation: {0:yyyy-MM-dd HH:mm:ss} UTC", m_lastReportGenerationTime);
                status.AppendLine();
                status.AppendFormat("          Report e-mailing: {0}", EnableReportEmail ? "Enabled" : "Disabled");
                status.AppendLine();

                if (EnableReportEmail)
                {
                    status.AppendFormat("       Defined SMTP server: {0}", SmtpServer ?? "undefined");
                    status.AppendLine();
                    status.AppendFormat("              From address: {0}", FromAddress ?? "undefined");
                    status.AppendLine();
                    status.AppendFormat("              To addresses: {0}", ToAddresses ?? "undefined");
                    status.AppendLine();
                    status.AppendFormat("             SMTP username: {0}", SmtpUsername ?? "undefined");
                    status.AppendLine();
                    status.AppendFormat("             SMTP password: {0}", new string('*', SmtpSecurePassword?.Length ?? 0));
                    status.AppendLine();
                }

                return status.ToString();
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
        /// <param name="emailReport">Flag that determines if report should be e-mailed, if enabled.</param>
        public void GenerateReport(DateTime reportDate, bool emailReport)
        {
            // Align reportDate with the top of the day before using it with the report generation queue
            reportDate = reportDate.Date;

            // ToArray is a thread-safe operation on ConcurrentQueue whereas using an enumerator
            // directly on a ConcurrentQueue can cause collection modified errors while iterating
            if (m_reportGenerationQueue.ToArray().Count(tuple => tuple.Item1 == reportDate) == 0)
            {
                m_reportGenerationQueue.Enqueue(new Tuple<DateTime, bool>(reportDate, emailReport));
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
        public virtual void LoadSettings()
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
            settings.Add("IdleReportLifetime", m_idleReportLifetime, "The minimum lifetime of a report since the last time it was accessed, in days.");
            settings.Add("EnableReportEmail", m_enableReportEmail, "Set to true to enable daily e-mailing of reports.");
            settings.Add("SmtpServer", m_smtpServer, "The SMTP relay server from which to send e-mails.");
            settings.Add("FromAddress", m_fromAddress, "The from address for the report e-mails.");
            settings.Add("ToAddresses", m_toAddresses, "Comma separated list of destination addresses for the report e-mails.");
            settings.Add("SmtpUsername", m_smtpUsername, "Username to authenticate to the SMTP server.");
            settings.Add("SmtpPassword", m_smtpPassword, "Password to authenticate to the SMTP server.");

            ArchiveFilePath = settings["ArchiveFilePath"].ValueAs(m_archiveFilePath);
            ReportLocation = settings["ReportLocation"].ValueAs(m_reportLocation);
            Title = settings["Title"].ValueAs(m_title);
            Company = settings["Company"].ValueAs(m_company);
            IdleReportLifetime = settings["IdleReportLifetime"].ValueAs(m_idleReportLifetime);
            EnableReportEmail = settings["EnableReportEmail"].ValueAsBoolean();
            SmtpServer = settings["SmtpServer"].ValueAs(m_smtpServer);
            FromAddress = settings["FromAddress"].ValueAs(m_fromAddress);
            ToAddresses = settings["ToAddresses"].ValueAs(m_toAddresses);
            SmtpUsername = settings["SmtpUsername"].ValueAs(m_smtpUsername);
            SmtpPassword = settings["SmtpPassword"].ValueAs(string.Empty);
        }

        /// <summary>
        /// Saves settings to the config file.
        /// </summary>
        public virtual void SaveSettings()
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
            settings["IdleReportLifetime", true].Update(m_idleReportLifetime);
            settings["EnableReportEmail", true].Update(m_enableReportEmail);
            settings["SmtpServer", true].Update(m_smtpServer);
            settings["FromAddress", true].Update(m_fromAddress);
            settings["ToAddresses", true].Update(m_toAddresses);
            settings["SmtpUsername", true].Update(m_smtpUsername);
            settings["SmtpPassword", true].Update(SmtpPassword);
            config.Save();
        }

        /// <summary>
        /// Executes the reporting process to generate the report.
        /// </summary>
        private void Execute()
        {
            WindowsIdentity currentOwner = WindowsIdentity.GetCurrent();
            Tuple<DateTime, bool> tuple;

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

            while (m_reportGenerationQueue.TryPeek(out tuple))
            {
                // Execute the reporting process
                using (Process process = new Process())
                {
                    process.StartInfo.FileName = FilePath.GetAbsolutePath("StatHistorianReportGenerator.exe");
                    process.StartInfo.Arguments = GetArguments(tuple.Item1, tuple.Item2);
                    process.Start();
                    process.WaitForExit();
                    process.Close();
                }

                // Dequeue only after the report is generated so that it
                // remains in the list of pending reports during generation
                m_reportGenerationQueue.TryDequeue(out tuple);
                m_generatedReports++;
                m_lastReportGenerationTime = DateTime.UtcNow;
            }
        }

        /// <summary>
        /// Gets the command line arguments for the reporting process.
        /// </summary>
        public virtual string GetArguments()
        {
            // Because we may have a archive location like "C:\Program Files\MyPath" with quotes,
            // the arguments below have an extra leading and trailing space around quoted values
            // for proper parsing.
            return string.Format(
                 "--reportType=\" {0} \" " +
                 "--archiveLocation=\" {1} \" " +
                 "--reportLocation=\" {2} \" " +
                 "--title=\" {3} \" " +
                 "--company=\" {4} \"",
                 ReportType.Replace("\"", "\\\""),
                 FilePath.GetDirectoryName(ArchiveFilePath).Replace("\"", "\\\""),
                 ReportLocation.Replace("\"", "\\\""),
                 Title.Replace("\"", "\\\""),
                 Company.Replace("\"", "\\\""));
        }

        /// <summary>
        /// Gets the command line arguments for the reporting process for a given report date.
        /// </summary>
        /// <param name="reportDate">The date of the report to be generated.</param>
        /// <param name="emailReport">Flag that determines if report should be e-mailed, if enabled.</param>
        public string GetArguments(DateTime reportDate, bool emailReport)
        {
            const string CryptoKey = "0679d9ae-aca5-4702-a3f5-604415096987";

            string arguments = string.Format(
                "{0} " +
                "--reportDate=\" {1:yyyy-MM-dd} \"",
                GetArguments(),
                reportDate);

            if (emailReport && EnableReportEmail)
                arguments = string.Format(
                    "{0} " +
                    "--smtpServer=\" {1} \" " +
                    "--fromAddress=\" {2} \" " +
                    "--toAddresses=\" {3} \" " +
                    "--username=\" {4} \" " +
                    "--password=\" {5} \"",
                    arguments,
                    SmtpServer,
                    FromAddress,
                    ToAddresses,
                    SmtpUsername,
                    SmtpPassword.Encrypt(CryptoKey, CipherStrength.Aes256));

            return arguments;
        }

        /// <summary>
        /// Applies any received command line arguments for the reporting process.
        /// </summary>
        /// <param name="args">Received command line arguments.</param>
        public virtual void SetArguments(Arguments args)
        {
            double value;
            string arg = args["reportLocation"];

            if ((object)arg != null)
                ReportLocation = arg.Trim();

            arg = args["title"];

            if ((object)arg != null)
                Title = arg.Trim();

            arg = args["company"];

            if ((object)arg != null)
                Company = arg.Trim();

            arg = args["idleReportLifetime"];

            if ((object)arg != null && double.TryParse(arg.Trim(), out value))
                IdleReportLifetime = value;
        }

        /// <summary>
        /// Determines whether the given path is a path to a report, based on the file name.
        /// </summary>
        public virtual bool IsReportFileName(string fileName)
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

                // ReSharper disable once PossibleInvalidCastExceptionInForeachLoop
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
