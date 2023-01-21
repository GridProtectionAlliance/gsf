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
        private readonly ConcurrentQueue<Tuple<DateTime, bool>> m_reportGenerationQueue;
        private readonly LongSynchronizedOperation m_executeOperation;
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
            ReportType = reportType;
            m_reportGenerationQueue = new ConcurrentQueue<Tuple<DateTime, bool>>();
            m_executeOperation = new LongSynchronizedOperation(Execute)
            {
                IsBackground = true
            };

            PersistSettings = true;
            SettingsCategory = string.Format("{0}Reporting", ReportType);
            ArchiveFilePath = "Eval(statArchiveFile.FileName)";
            ReportLocation = nameof(Reports);
            Title = string.Format("Eval(securityProvider.ApplicationName) {0} Report", ReportType);
            Company = "Eval(systemSettings.CompanyName)";
            IdleReportLifetime = 14.0D;
            EnableReportEmail = false;
            SmtpServer = "localhost";
            FromAddress = "reports@gridprotectionalliance.org";
            ToAddresses = "wile.e.coyote@acme.com";
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Determines whether the object settings are to be persisted to the config file.
        /// </summary>
        public bool PersistSettings { get; set; }

        /// <summary>
        /// Gets report type, i.e., basically the report name associated with this reporting process.
        /// </summary>
        /// <remarks>
        /// This value is passed to StatHistorianReportGenerator as "reportType" parameter.
        /// </remarks>
        public string ReportType { get; }

        string IProvideStatus.Name => GetType().Name;

        /// <summary>
        /// Gets or sets the category name under which the object settings are persisted in the config file.
        /// </summary>
        public string SettingsCategory { get; set; }

        /// <summary>
        /// Gets or sets the path to the archive file to which
        /// the statistics required for reporting are archived.
        /// </summary>
        public string ArchiveFilePath { get; set; }

        /// <summary>
        /// Gets or sets the directory to which reports will be written.
        /// </summary>
        public string ReportLocation { get; set; }

        /// <summary>
        /// Gets or sets the title to be displayed on reports.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the name of the company to be displayed on reports.
        /// </summary>
        public string Company { get; set; }

        /// <summary>
        /// Gets or sets the minimum lifetime of a report
        /// since the last time it was accessed, in days.
        /// </summary>
        public double IdleReportLifetime { get; set; }

        /// <summary>
        /// Gets or sets flag to enable e-mailing of reports.
        /// </summary>
        public bool EnableReportEmail { get; set; }

        /// <summary>
        /// Gets or sets SMTP server to use when e-mailing reports.
        /// </summary>
        public string SmtpServer { get; set; }

        /// <summary>
        /// Gets or sets the "from" address to use when e-mailing reports.
        /// </summary>
        public string FromAddress { get; set; }

        /// <summary>
        /// Gets or sets the comma separated "to" addresses to use when e-mailing reports. 
        /// </summary>
        public string ToAddresses { get; set; }

        /// <summary>
        /// Gets or sets the username used to authenticate to the SMTP server.
        /// </summary>
        public string SmtpUsername { get; set; }

        /// <summary>
        /// Gets or sets the password used to authenticate to the SMTP server.
        /// </summary>
        public string SmtpPassword
        {
            get => SmtpSecurePassword.ToUnsecureString();
            set => SmtpSecurePassword = value.ToSecureString();
        }

        /// <summary>
        /// Gets or sets the password used to authenticate to the SMTP server as a secure string.
        /// </summary>
        public SecureString SmtpSecurePassword { get; set; }

        /// <summary>
        /// Gets the current status details about reporting process.
        /// </summary>
        public virtual string Status
        {
            get
            {
                StringBuilder status = new();

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
            string reportLocation = FilePath.GetAbsolutePath(ReportLocation)
                .EnsureEnd(Path.DirectorySeparatorChar);

            if (Directory.Exists(reportLocation))
            {
                return FilePath.GetFileList(reportLocation)
                    .Select(FilePath.GetFileName)
                    .Where(filename => IsReportFileName(filename) && Path.GetExtension(filename).ToLower() == ".pdf")
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
                .Select(reportDate => string.Format("{0} {1:yyyy-MM-dd}.pdf", Title, reportDate))
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
                    info = new FileInfo(FilePath.GetAbsolutePath(Path.Combine(ReportLocation, report)));

                    if ((DateTime.UtcNow - info.LastAccessTimeUtc).TotalDays > IdleReportLifetime)
                        File.Delete(info.FullName);
                }
            }
        }

        /// <summary>
        /// Loads saved settings from the config file.
        /// </summary>
        public virtual void LoadSettings()
        {
            if (!PersistSettings)
                return;

            // Ensure that settings category is specified.
            if (string.IsNullOrEmpty(SettingsCategory))
                throw new ConfigurationErrorsException("SettingsCategory property has not been set");

            // Load settings from the specified category.
            ConfigurationFile config = ConfigurationFile.Current;
            CategorizedSettingsElementCollection settings = config.Settings[SettingsCategory];

            settings.Add(nameof(ArchiveFilePath), ArchiveFilePath, "Path to the archive file to which the statistics required for reporting are archived.");
            settings.Add(nameof(ReportLocation), ReportLocation, "Directory to which reports will be written.");
            settings.Add(nameof(Title), Title, "Title to be displayed on reports.");
            settings.Add(nameof(Company), Company, "Name of the company to be displayed on reports.");
            settings.Add(nameof(IdleReportLifetime), IdleReportLifetime, "The minimum lifetime of a report since the last time it was accessed, in days.");
            settings.Add(nameof(EnableReportEmail), EnableReportEmail, "Set to true to enable daily e-mailing of reports.");
            settings.Add(nameof(SmtpServer), SmtpServer, "The SMTP relay server from which to send e-mails.");
            settings.Add(nameof(FromAddress), FromAddress, "The from address for the report e-mails.");
            settings.Add(nameof(ToAddresses), ToAddresses, "Comma separated list of destination addresses for the report e-mails.");
            settings.Add(nameof(SmtpUsername), SmtpUsername, "Username to authenticate to the SMTP server.");
            settings.Add(nameof(SmtpPassword), SmtpSecurePassword, "Password to authenticate to the SMTP server.");

            ArchiveFilePath = settings[nameof(ArchiveFilePath)].ValueAs(ArchiveFilePath);
            ReportLocation = settings[nameof(ReportLocation)].ValueAs(ReportLocation);
            Title = settings[nameof(Title)].ValueAs(Title);
            Company = settings[nameof(Company)].ValueAs(Company);
            IdleReportLifetime = settings[nameof(IdleReportLifetime)].ValueAs(IdleReportLifetime);
            EnableReportEmail = settings[nameof(EnableReportEmail)].ValueAsBoolean();
            SmtpServer = settings[nameof(SmtpServer)].ValueAs(SmtpServer);
            FromAddress = settings[nameof(FromAddress)].ValueAs(FromAddress);
            ToAddresses = settings[nameof(ToAddresses)].ValueAs(ToAddresses);
            SmtpUsername = settings[nameof(SmtpUsername)].ValueAs(SmtpUsername);
            SmtpPassword = settings[nameof(SmtpPassword)].ValueAs(string.Empty);
        }

        /// <summary>
        /// Saves settings to the config file.
        /// </summary>
        public virtual void SaveSettings()
        {
            if (!PersistSettings)
                return;

            // Ensure that settings category is specified.
            if (string.IsNullOrEmpty(SettingsCategory))
                throw new ConfigurationErrorsException("SettingsCategory property has not been set");

            // Save settings under the specified category.
            ConfigurationFile config = ConfigurationFile.Current;
            CategorizedSettingsElementCollection settings = config.Settings[SettingsCategory];
            settings[nameof(ArchiveFilePath), true].Update(ArchiveFilePath);
            settings[nameof(ReportLocation), true].Update(ReportLocation);
            settings[nameof(Title), true].Update(Title);
            settings[nameof(Company), true].Update(Company);
            settings[nameof(IdleReportLifetime), true].Update(IdleReportLifetime);
            settings[nameof(EnableReportEmail), true].Update(EnableReportEmail);
            settings[nameof(SmtpServer), true].Update(SmtpServer);
            settings[nameof(FromAddress), true].Update(FromAddress);
            settings[nameof(ToAddresses), true].Update(ToAddresses);
            settings[nameof(SmtpUsername), true].Update(SmtpUsername);
            settings[nameof(SmtpPassword), true].Update(SmtpPassword);
            config.Save();
        }

        /// <summary>
        /// Executes the reporting process to generate the report.
        /// </summary>
        private void Execute()
        {
            WindowsIdentity currentOwner = WindowsIdentity.GetCurrent();

            if (currentOwner is not null)
            {
                // Wait for existing processes to exit, for instance if the
                // service was restarted while a report was being generated
                foreach (Process process in Process.GetProcessesByName("StatHistorianReportGenerator.exe"))
                {
                    if (currentOwner.Name == GetProcessOwner(process.Id))
                        process.WaitForExit();
                }
            }

            while (m_reportGenerationQueue.TryPeek(out Tuple<DateTime, bool> tuple))
            {
                // Execute the reporting process
                using (Process process = new())
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
            string arg = args["reportLocation"];

            if ((object)arg is not null)
                ReportLocation = arg.Trim();

            arg = args["title"];

            if ((object)arg is not null)
                Title = arg.Trim();

            arg = args["company"];

            if ((object)arg is not null)
                Company = arg.Trim();

            arg = args["idleReportLifetime"];

            if ((object)arg is not null && double.TryParse(arg.Trim(), out double value))
                IdleReportLifetime = value;
        }

        /// <summary>
        /// Determines whether the given path is a path to a report, based on the file name.
        /// </summary>
        public virtual bool IsReportFileName(string fileName)
        {
            string regex = string.Format(@"{0} (?<Date>[^.]+)\.pdf", Title);
            Match match = Regex.Match(fileName, regex);

            return match.Success && DateTime.TryParse(match.Groups["Date"].Value, out DateTime reportDate);
        }

        /// <summary>
        /// Determines who is the owner of the given process.
        /// </summary>
        private string GetProcessOwner(int processId)
        {
            try
            {
                string query = "Select * From Win32_Process Where ProcessID = " + processId;
                ManagementObjectSearcher searcher = new(query);
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
