//******************************************************************************************************
//  ReportingProcess.cs - Gbtc
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
//  03/05/2014 - Stephen C. Wills
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Configuration;
using System.Diagnostics;
using GSF.Configuration;
using GSF.IO;

namespace GSF.TimeSeries
{
    /// <summary>
    /// Represents the process that generates reports for the time-series service.
    /// </summary>
    public class DataQualityReportingProcess : IPersistSettings
    {
        #region [ Members ]

        // Fields
        private bool m_persistSettings;
        private string m_settingsCategory;

        private string m_archiveFilePath;
        private string m_reportLocation;
        private string m_title;
        private string m_company;
        private DateTime m_reportDate;
        private double m_level4Threshold;
        private double m_level3Threshold;
        private string m_level4Alias;
        private string m_level3Alias;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new instance of the <see cref="DataQualityReportingProcess"/> class.
        /// </summary>
        public DataQualityReportingProcess()
        {
            m_archiveFilePath = "Eval(statArchiveFile.FileName)";
            m_reportLocation = "Reports";
            m_title = "Eval(securityProvider.ApplicationName) Data Quality Report";
            m_company = "Eval(systemSettings.CompanyName)";
            m_level4Threshold = 99.0D;
            m_level3Threshold = 90.0D;
            m_level4Alias = "Good";
            m_level3Alias = "Fair";
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the date of the report to be generated.
        /// </summary>
        public DateTime ReportDate
        {
            get
            {
                return m_reportDate;
            }
            set
            {
                m_reportDate = value;
            }
        }

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

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Executes the reporting process to generate the report.
        /// </summary>
        public void Execute()
        {
            using (Process process = new Process())
            {
                process.StartInfo.FileName = FilePath.GetAbsolutePath("StatHistorianReportGenerator.exe");
                process.StartInfo.Arguments = GetArguments();
                process.Start();
                process.WaitForExit();
                process.Close();
            }
        }

        /// <summary>
        /// Gets the command line arguments for the reporting process.
        /// </summary>
        /// <returns>The command line arguments for the reporting process.</returns>
        public string GetArguments()
        {
            return string.Format("--archiveLocation=\" {0} \" --reportLocation=\" {1} \" --title=\" {2} \" --company=\" {3} \" " +
                "--reportDate=\" {4:yyyy-MM-dd} \" --level4threshold=\" {5} \" --level3threshold=\" {6} \" --level4alias=\" {7} \" " +
                "--level3alias=\" {8} \"", FilePath.GetDirectoryName(m_archiveFilePath).Replace("\"", "\\\""),
                m_reportLocation.Replace("\"", "\\\""), m_title.Replace("\"", "\\\""), m_company.Replace("\"", "\\\""), m_reportDate,
                m_level4Threshold, m_level3Threshold, m_level4Alias.Replace("\"", "\\\""), m_level3Alias.Replace("\"", "\\\""));
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

            ArchiveFilePath = settings["ArchiveFilePath"].ValueAs(m_archiveFilePath);
            ReportLocation = settings["ReportLocation"].ValueAs(m_reportLocation);
            Title = settings["Title"].ValueAs(m_title);
            Company = settings["Company"].ValueAs(m_company);
            Level4Threshold = settings["Level4Threshold"].ValueAs(m_level4Threshold);
            Level3Threshold = settings["Level3Threshold"].ValueAs(m_level3Threshold);
            Level4Alias = settings["Level4Alias"].ValueAs(m_level4Alias);
            Level3Alias = settings["Level3Alias"].ValueAs(m_level3Alias);
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
            config.Save();
        }

        #endregion
    }
}
