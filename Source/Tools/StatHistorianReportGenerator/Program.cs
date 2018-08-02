//******************************************************************************************************
//  Program.cs - Gbtc
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
//  02/10/2014 - Stephen C. Wills
//       Generated original version of source code.
//  04/29/2015 - J. Ritchie Carroll
//       Added report e-mailing options.
//  08/02/2018 - J. Ritchie Carroll
//       Added error logging and optional database connection arguments.
//
//******************************************************************************************************

using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using System.Xml.Linq;
using GSF;
using GSF.Console;
using GSF.Data;
using GSF.Diagnostics;
using GSF.IO;
using GSF.Net.Smtp;
using GSF.Security.Cryptography;

namespace StatHistorianReportGenerator
{
    static class Program
    {
        public static LogPublisher Log;
        private static string s_configurationString;
        private static string s_dataProviderString;

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
            Logger.FileWriter.SetLoggingFileCount(100);

            Log = Logger.CreatePublisher(typeof(Program), MessageClass.Application);

            try
            {
                // Report.NET is only capable of using the standard AFM fonts that are expected to be supported by all PDF readers.
                // As a result, high-order Unicode characters that may be produced with long date-time formats when using non-US
                // cultures can cause application to fail - as a result, culture is set to invariant to reduce failure risk. Note
                // that risk will not be completely eliminated, e.g., system configured with a company name using a bad character.
                Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
                Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
            }
            catch (Exception ex)
            {
                Log.Publish(MessageLevel.Error, "InitializeCulture", exception: ex);
            }

            if (Environment.GetCommandLineArgs().Length > 1)
            {
                Arguments args = new Arguments(Environment.CommandLine, true);

                // Check for optional parameters
                TryGetValue(args, "configFile", out ArchiveLocator.HostConfigFile);
                TryGetValue(args, "configurationString", out s_configurationString);
                TryGetValue(args, "dataProviderString", out s_dataProviderString);

                if (TryGetValue(args, "reportType", out string arg) && !string.IsNullOrWhiteSpace(arg))
                {
                    arg = arg.Trim();

                    if (arg.Equals("completeness", StringComparison.OrdinalIgnoreCase))
                        GenerateCompletenessReport();
                    else if (arg.Equals("correctness", StringComparison.OrdinalIgnoreCase))
                        GenerateCorrectnessReport();
                }
                else
                {
                    // Generate completeness report by default
                    GenerateCompletenessReport();
                }
            }
            else
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new Main());
            }
        }

        private static void GenerateCompletenessReport()
        {
            try
            {
                CompletenessReportGenerator completenessReportGenerator = new CompletenessReportGenerator();
                Arguments args = new Arguments(Environment.CommandLine, true);

                string reportLocation = "";
                string reportFileName = "";
                DateTime reportDate = DateTime.UtcNow;

                if (TryGetValue(args, "archiveLocation", out string arg))
                    completenessReportGenerator.ArchiveLocation = arg;

                if (TryGetValue(args, "reportLocation", out arg))
                    reportLocation = arg;

                if (TryGetValue(args, "reportFileName", out arg))
                    reportFileName = arg;

                if (TryGetValue(args, "title", out arg))
                    completenessReportGenerator.TitleText = arg;

                if (TryGetValue(args, "company", out arg))
                    completenessReportGenerator.CompanyText = arg;

                if (TryGetValue(args, "reportDate", out arg) && DateTime.TryParse(arg, out reportDate))
                {
                    if (reportDate.Kind == DateTimeKind.Unspecified)
                        reportDate = new DateTime(reportDate.Ticks, DateTimeKind.Utc);

                    completenessReportGenerator.ReportDate = reportDate;
                }

                if (TryGetValue(args, "level4Threshold", out arg) && double.TryParse(arg, out double threshold))
                    completenessReportGenerator.Level4Threshold = threshold;

                if (TryGetValue(args, "level3Threshold", out arg) && double.TryParse(arg, out threshold))
                    completenessReportGenerator.Level3Threshold = threshold;

                if (TryGetValue(args, "level4Alias", out arg))
                    completenessReportGenerator.Level4Alias = arg;

                if (TryGetValue(args, "level3Alias", out arg))
                    completenessReportGenerator.Level3Alias = arg;

                if (TryGetValue(args, "generateCsvReport", out arg) && bool.TryParse(arg, out bool generateCsvReport))
                    completenessReportGenerator.GenerateCsvReport = generateCsvReport;

                if (string.IsNullOrEmpty(reportFileName))
                    reportFileName = $"{completenessReportGenerator.TitleText} {completenessReportGenerator.ReportDate:yyyy-MM-dd}.pdf";

                reportLocation = FilePath.GetAbsolutePath(reportLocation);

                if (!Directory.Exists(reportLocation))
                    Directory.CreateDirectory(reportLocation);

                completenessReportGenerator.ReportFilePath = Path.Combine(reportLocation, reportFileName);

                // Generate PDF report
                completenessReportGenerator.GenerateReport().Save(completenessReportGenerator.ReportFilePath);

                // E-mail PDF report if parameters were provided
                EmailReport(args, $"{completenessReportGenerator.CompanyText} {completenessReportGenerator.TitleText} for {reportDate:MMMM dd, yyyy}", completenessReportGenerator.ReportFilePath);
            }
            catch (Exception ex)
            {
                Log.Publish(MessageLevel.Error, "GenerateCompletenessReport", exception: ex);
            }
        }

        private static void GenerateCorrectnessReport()
        {
            try
            {
                CorrectnessReportGenerator correctnessReportGenerator = new CorrectnessReportGenerator();
                Arguments args = new Arguments(Environment.CommandLine, true);

                string reportLocation = "";
                string reportFileName = "";
                DateTime reportDate = DateTime.UtcNow;

                if (TryGetValue(args, "archiveLocation", out string arg))
                    correctnessReportGenerator.ArchiveLocation = arg;

                if (TryGetValue(args, "reportLocation", out arg))
                    reportLocation = arg;

                if (TryGetValue(args, "reportFileName", out arg))
                    reportFileName = arg;

                if (TryGetValue(args, "title", out arg))
                    correctnessReportGenerator.TitleText = arg;

                if (TryGetValue(args, "company", out arg))
                    correctnessReportGenerator.CompanyText = arg;

                if (TryGetValue(args, "reportDate", out arg) && DateTime.TryParse(arg, out reportDate))
                {
                    if (reportDate.Kind == DateTimeKind.Unspecified)
                        reportDate = new DateTime(reportDate.Ticks, DateTimeKind.Utc);

                    correctnessReportGenerator.ReportDate = reportDate;
                }

                if (string.IsNullOrEmpty(reportFileName))
                    reportFileName = $"{correctnessReportGenerator.TitleText} {correctnessReportGenerator.ReportDate:yyyy-MM-dd}.pdf";

                reportLocation = FilePath.GetAbsolutePath(reportLocation);

                if (!Directory.Exists(reportLocation))
                    Directory.CreateDirectory(reportLocation);

                string reportFilePath = Path.Combine(reportLocation, reportFileName);

                // Generate PDF report
                correctnessReportGenerator.GenerateReport().Save(reportFilePath);

                // E-mail PDF report if parameters were provided
                EmailReport(args, $"{correctnessReportGenerator.CompanyText} {correctnessReportGenerator.TitleText} for {reportDate:MMMM dd, yyyy}", reportFilePath);
            }
            catch (Exception ex)
            {
                Log.Publish(MessageLevel.Error, "GenerateCorrectnessReport", exception: ex);
            }
        }

        private static void EmailReport(Arguments args, string subject, string reportFilePath)
        {
            const string CryptoKey = "0679d9ae-aca5-4702-a3f5-604415096987";

            try
            {
                if (TryGetValue(args, "smtpServer", out string smtpServer) &&
                    TryGetValue(args, "fromAddress", out string fromAddress) &&
                    TryGetValue(args, "toAddresses", out string toAddresses) &&
                    smtpServer.Length > 0 &&
                    fromAddress.Length > 0 &&
                    toAddresses.Length > 0)
                {
                    using (Mail message = new Mail(fromAddress, toAddresses, smtpServer))
                    {
                        message.Subject = subject;
                        message.Attachments = reportFilePath;
                        message.IsBodyHtml = true;
                        message.Body = "<div>\r\n" + "The attached report requires a portable document format (PDF) reader, such as the " + "<a href=\"http://get.adobe.com/reader/\">Adobe Acrobat PDF Reader</a>.\r\n" + "</div>\r\n" + "<br><br>\r\n" + "<div>\r\n" + $"<i>E-mail generated at {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss.fff} UTC.</i>\r\n" + "</div>";

                        if (TryGetValue(args, "generateCsvReport", out string generateCsvReport) && generateCsvReport.ParseBoolean())
                            message.Attachments += ";" + Path.ChangeExtension(reportFilePath, ".csv");

                        if (TryGetValue(args, "username", out string username) &&
                            TryGetValue(args, "password", out string password) &&
                            username.Length > 0 &&
                            password.Length > 0)
                        {
                            message.EnableSSL = true;
                            message.Username = username;
                            message.Password = password.Decrypt(CryptoKey, CipherStrength.Aes256);
                        }

                        message.Send();
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Publish(MessageLevel.Error, "EmailReport", exception: ex);
            }
        }

        private static bool TryGetValue(Arguments args, string arg, out string value)
        {
            value = args[arg];

            if ((object)value != null)
            {
                value = value.Trim();
                return true;
            }

            return false;
        }

        public static AdoDataConnection GetDatabaseConnection()
        {
            string connectionString = s_configurationString;
            string dataProviderString = s_dataProviderString;

            if (string.IsNullOrWhiteSpace(connectionString) || string.IsNullOrWhiteSpace(dataProviderString))
            {
                XDocument serviceConfig = XDocument.Load(ArchiveLocator.GetConfigurationFileName());

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
            }

            if (string.IsNullOrWhiteSpace(connectionString) || string.IsNullOrWhiteSpace(dataProviderString))
                return null;

            return new AdoDataConnection(connectionString, dataProviderString);
        }
    }
}
