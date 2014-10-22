//******************************************************************************************************
//  Program.cs - Gbtc
//
//  Copyright © 2014, Grid Protection Alliance.  All Rights Reserved.
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
//  02/10/2014 - Stephen C. Wills
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.IO;
using System.Windows.Forms;
using GSF.Console;
using GSF.IO;

namespace StatHistorianReportGenerator
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Arguments args;
            string arg;

            if (Environment.GetCommandLineArgs().Length > 1)
            {
                args = new Arguments(Environment.CommandLine, true);

                if (TryGetValue(args, "reportType", out arg))
                {
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
            CompletenessReportGenerator completenessReportGenerator = new CompletenessReportGenerator();
            Arguments args = new Arguments(Environment.CommandLine, true);
            string arg;

            string reportLocation = "";
            string reportFileName = "";
            DateTime reportDate;
            double threshold;

            if (TryGetValue(args, "archiveLocation", out arg))
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
                completenessReportGenerator.ReportDate = reportDate;

            if (TryGetValue(args, "level4Threshold", out arg) && double.TryParse(arg, out threshold))
                completenessReportGenerator.Level4Threshold = threshold;

            if (TryGetValue(args, "level3Threshold", out arg) && double.TryParse(arg, out threshold))
                completenessReportGenerator.Level3Threshold = threshold;

            if (TryGetValue(args, "level4Alias", out arg))
                completenessReportGenerator.Level4Alias = arg;

            if (TryGetValue(args, "level3Alias", out arg))
                completenessReportGenerator.Level3Alias = arg;

            if (string.IsNullOrEmpty(reportFileName))
                reportFileName = string.Format("{0} {1:yyyy-MM-dd}.pdf", completenessReportGenerator.TitleText, completenessReportGenerator.ReportDate);

            reportLocation = FilePath.GetAbsolutePath(reportLocation);

            if (!Directory.Exists(reportLocation))
                Directory.CreateDirectory(reportLocation);

            completenessReportGenerator.GenerateReport().Save(Path.Combine(reportLocation, reportFileName));
        }

        private static void GenerateCorrectnessReport()
        {
            CorrectnessReportGenerator correctnessReportGenerator = new CorrectnessReportGenerator();
            Arguments args = new Arguments(Environment.CommandLine, true);
            string arg;

            string reportLocation = "";
            string reportFileName = "";
            DateTime reportDate;

            if (TryGetValue(args, "archiveLocation", out arg))
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
                correctnessReportGenerator.ReportDate = reportDate;

            if (string.IsNullOrEmpty(reportFileName))
                reportFileName = string.Format("{0} {1:yyyy-MM-dd}.pdf", correctnessReportGenerator.TitleText, correctnessReportGenerator.ReportDate);

            reportLocation = FilePath.GetAbsolutePath(reportLocation);

            if (!Directory.Exists(reportLocation))
                Directory.CreateDirectory(reportLocation);

            correctnessReportGenerator.GenerateReport().Save(Path.Combine(reportLocation, reportFileName));
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
    }
}
