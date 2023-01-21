//******************************************************************************************************
//  CompletenessReportingProcess.cs - Gbtc
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
//  06/08/2015 - J. Ritchie Carroll
//       Modified class to derive from ReportingProcessBase.
//  10/27/2017 - Stephen Jenks
//      Added GenerateCsvReport property
//
//
//******************************************************************************************************

using System.Text;
using GSF.Configuration;
using GSF.Console;
using System.Text.RegularExpressions;
using System;

namespace GSF.TimeSeries.Reports
{
    /// <summary>
    /// Represents the process that generates completeness reports for the time-series service.
    /// </summary>
    public class CompletenessReportingProcess : ReportingProcessBase
    {
        #region [ Members ]

        // Fields

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new instance of the <see cref="CompletenessReportingProcess"/> class.
        /// </summary>
        public CompletenessReportingProcess()
            : base("Completeness")
        {
            Level4Threshold = 99.0D;
            Level3Threshold = 90.0D;
            Level4Alias = "Good";
            Level3Alias = "Fair";
            GenerateCsvReport = false;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the minimum percentage of measurements received from devices in level 4.
        /// </summary>
        public double Level4Threshold { get; set; }

        /// <summary>
        /// Gets or sets the minimum percentage of measurements received from devices in level 3.
        /// </summary>
        public double Level3Threshold { get; set; }

        /// <summary>
        /// Gets or sets the alias for the level 4 category.
        /// </summary>
        public string Level4Alias { get; set; }

        /// <summary>
        /// Gets or sets the alias for the level 3 category.
        /// </summary>
        public string Level3Alias { get; set; }

        /// <summary>
        /// Gets or sets the option to generate a csv report along with pdf report.
        /// </summary>
        public bool GenerateCsvReport { get; set; }

        /// <summary>
        /// Gets the current status details about reporting process.
        /// </summary>
        public override string Status
        {
            get
            {
                StringBuilder status = new(base.Status);

                status.AppendFormat("         Level 4 threshold: {0:N2}%", Level4Threshold);
                status.AppendLine();
                status.AppendFormat("         Level 3 threshold: {0:N2}%", Level3Threshold);
                status.AppendLine();
                status.AppendFormat("             Level 4 alias: {0}", Level4Alias ?? "undefined");
                status.AppendLine();
                status.AppendFormat("             Level 3 alias: {0}", Level3Alias ?? "undefined");
                status.AppendLine();

                return status.ToString();
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Loads saved settings from the config file.
        /// </summary>
        public override void LoadSettings()
        {
            base.LoadSettings();

            // Load settings from the specified category.
            ConfigurationFile config = ConfigurationFile.Current;
            CategorizedSettingsElementCollection settings = config.Settings[SettingsCategory];

            settings.Add(nameof(Level4Threshold), Level4Threshold, "Minimum percentage of measurements received from devices in level 4.");
            settings.Add(nameof(Level3Threshold), Level3Threshold, "Minimum percentage of measurements received from devices in level 3.");
            settings.Add(nameof(Level4Alias), Level4Alias, "Alias for the level 4 category.");
            settings.Add(nameof(Level3Alias), Level3Alias, "Alias for the level 3 category.");
            settings.Add(nameof(GenerateCsvReport), GenerateCsvReport, "Generate a csv version of the pdf report");

            Level4Threshold = settings[nameof(Level4Threshold)].ValueAs(Level4Threshold);
            Level3Threshold = settings[nameof(Level3Threshold)].ValueAs(Level3Threshold);
            Level4Alias = settings[nameof(Level4Alias)].ValueAs(Level4Alias);
            Level3Alias = settings[nameof(Level3Alias)].ValueAs(Level3Alias);
            GenerateCsvReport = settings[nameof(GenerateCsvReport)].ValueAs(GenerateCsvReport);
        }

        /// <summary>
        /// Saves settings to the config file.
        /// </summary>
        public override void SaveSettings()
        {
            base.SaveSettings();

            // Save settings under the specified category.
            ConfigurationFile config = ConfigurationFile.Current;
            CategorizedSettingsElementCollection settings = config.Settings[SettingsCategory];

            settings[nameof(Level4Threshold), true].Update(Level4Threshold);
            settings[nameof(Level3Threshold), true].Update(Level3Threshold);
            settings[nameof(Level4Alias), true].Update(Level4Alias);
            settings[nameof(Level3Alias), true].Update(Level3Alias);
            settings[nameof(GenerateCsvReport)].Update(GenerateCsvReport);
            config.Save();
        }

        /// <summary>
        /// Gets the command line arguments for the reporting process.
        /// </summary>
        public override string GetArguments()
        {
            // Because we may have a archive location like "C:\Program Files\MyPath" with quotes,
            // the arguments below have an extra leading and trailing space around quoted values
            // for proper parsing.
            return string.Format(
                 "{0} " +
                 "--level4threshold=\" {1} \" " +
                 "--level3threshold=\" {2} \" " +
                 "--level4alias=\" {3} \" " +
                 "--level3alias=\" {4} \" " +
                 "--GenerateCsvReport=\" {5} \"",
                 base.GetArguments(),
                 Level4Threshold,
                 Level3Threshold,
                 Level4Alias.Replace("\"", "\\\""),
                 Level3Alias.Replace("\"", "\\\""),
                 GenerateCsvReport);
        }

        /// <summary>
        /// Applies any received command line arguments for the reporting process.
        /// </summary>
        /// <param name="args">Received command line arguments.</param>
        public override void SetArguments(Arguments args)
        {
            base.SetArguments(args);

            string arg = args["level4Threshold"];

            if ((object)arg is not null && double.TryParse(arg.Trim(), out double value))
                Level4Threshold = value;

            arg = args["level3Threshold"];

            if ((object)arg is not null && double.TryParse(arg.Trim(), out value))
                Level3Threshold = value;

            arg = args["level4Alias"];

            if ((object)arg is not null)
                Level4Alias = arg.Trim();

            arg = args["level3Alias"];

            if ((object)arg is not null)
                Level3Alias = arg.Trim();

            arg = args[nameof(GenerateCsvReport)];

            if ((object)arg is not null && bool.TryParse(arg.Trim(), out bool value2))
                GenerateCsvReport = value2;
        }

        /// <summary>
        /// Determines whether the given path is a path to a report, based on the file name.
        /// </summary>
        public override bool IsReportFileName(string fileName)
        {
            string regex = string.Format(@"{0} (?<Date>[^.]+)\.csv", Title);
            Match match = Regex.Match(fileName, regex);

            return base.IsReportFileName(fileName) || match.Success && DateTime.TryParse(match.Groups["Date"].Value, out DateTime reportDate);
        }

        #endregion
    }
}
