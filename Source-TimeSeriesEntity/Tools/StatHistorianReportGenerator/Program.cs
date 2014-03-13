using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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
            ReportGenerator reportGenerator;
            Arguments args;
            string arg;

            string reportLocation = "";
            string reportFileName = "";
            DateTime reportDate;
            double threshold;

            if (Environment.GetCommandLineArgs().Length > 1)
            {
                reportGenerator = new ReportGenerator();
                args = new Arguments(Environment.CommandLine, true);

                if (TryGetValue(args, "archiveLocation", out arg))
                    reportGenerator.ArchiveLocation = arg;

                if (TryGetValue(args, "reportLocation", out arg))
                    reportLocation = arg;

                if (TryGetValue(args, "reportFileName", out arg))
                    reportFileName = arg;

                if (TryGetValue(args, "title", out arg))
                    reportGenerator.TitleText = arg;

                if (TryGetValue(args, "company", out arg))
                    reportGenerator.CompanyText = arg;

                if (TryGetValue(args, "reportDate", out arg) && DateTime.TryParse(arg, out reportDate))
                    reportGenerator.ReportDate = reportDate;

                if (TryGetValue(args, "level4Threshold", out arg) && double.TryParse(arg, out threshold))
                    reportGenerator.Level4Threshold = threshold;

                if (TryGetValue(args, "level3Threshold", out arg) && double.TryParse(arg, out threshold))
                    reportGenerator.Level3Threshold = threshold;

                if (TryGetValue(args, "level4Alias", out arg))
                    reportGenerator.Level4Alias = arg;

                if (TryGetValue(args, "level3Alias", out arg))
                    reportGenerator.Level3Alias = arg;

                if (string.IsNullOrEmpty(reportFileName))
                    reportFileName = string.Format("{0} {1:yyyy-MM-dd}.pdf", reportGenerator.TitleText, reportGenerator.ReportDate);

                reportLocation = FilePath.GetAbsolutePath(reportLocation);

                if (!Directory.Exists(reportLocation))
                    Directory.CreateDirectory(reportLocation);

                reportGenerator.GenerateReport().Save(Path.Combine(reportLocation, reportFileName));
            }
            else
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new Main());
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
    }
}
