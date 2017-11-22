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

namespace GSF.TimeSeries.Reports
{
    /// <summary>
    /// Represents the process that generates completeness reports for the time-series service.
    /// </summary>
    public class CompletenessReportingProcess : ReportingProcessBase
    {
        #region [ Members ]

        // Fields
        private double m_level4Threshold;
        private double m_level3Threshold;
        private string m_level4Alias;
        private string m_level3Alias;
        private bool m_generateCsvReport;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new instance of the <see cref="CompletenessReportingProcess"/> class.
        /// </summary>
        public CompletenessReportingProcess()
            : base("Completeness")
        {
            m_level4Threshold = 99.0D;
            m_level3Threshold = 90.0D;
            m_level4Alias = "Good";
            m_level3Alias = "Fair";
            m_generateCsvReport = false;
        }

        #endregion

        #region [ Properties ]

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
        /// Gets or sets the option to generate a csv report along with pdf report.
        /// </summary>
        public bool GenerateCsvReport
        {
            get
            {
                return m_generateCsvReport;
            }
            set
            {
                m_generateCsvReport = value;
            }
        }

        /// <summary>
        /// Gets the current status details about reporting process.
        /// </summary>
        public override string Status
        {
            get
            {
                StringBuilder status = new StringBuilder(base.Status);

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

            settings.Add("Level4Threshold", m_level4Threshold, "Minimum percentage of measurements received from devices in level 4.");
            settings.Add("Level3Threshold", m_level3Threshold, "Minimum percentage of measurements received from devices in level 3.");
            settings.Add("Level4Alias", m_level4Alias, "Alias for the level 4 category.");
            settings.Add("Level3Alias", m_level3Alias, "Alias for the level 3 category.");
            settings.Add("GenerateCsvReport", m_generateCsvReport, "Generate a csv version of the pdf report");

            Level4Threshold = settings["Level4Threshold"].ValueAs(m_level4Threshold);
            Level3Threshold = settings["Level3Threshold"].ValueAs(m_level3Threshold);
            Level4Alias = settings["Level4Alias"].ValueAs(m_level4Alias);
            Level3Alias = settings["Level3Alias"].ValueAs(m_level3Alias);
            GenerateCsvReport = settings["GenerateCsvReport"].ValueAs(m_generateCsvReport);
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

            settings["Level4Threshold", true].Update(m_level4Threshold);
            settings["Level3Threshold", true].Update(m_level3Threshold);
            settings["Level4Alias", true].Update(m_level4Alias);
            settings["Level3Alias", true].Update(m_level3Alias);
            settings["GenerateCsvReport"].Update(m_generateCsvReport);
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
                 "--level3alias=\" {4} \"" +
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

            double value;
            string arg = args["level4Threshold"];

            if ((object)arg != null && double.TryParse(arg.Trim(), out value))
                Level4Threshold = value;

            arg = args["level3Threshold"];

            if ((object)arg != null && double.TryParse(arg.Trim(), out value))
                Level3Threshold = value;

            arg = args["level4Alias"];

            if ((object)arg != null)
                Level4Alias = arg.Trim();

            arg = args["level3Alias"];

            if ((object)arg != null)
                Level3Alias = arg.Trim();

            bool value2;
            arg = args["GenerateCsvReport"];

            if ((object)arg != null && bool.TryParse(arg.Trim(), out value2))
                GenerateCsvReport = value2;
        }

        #endregion
    }
}
