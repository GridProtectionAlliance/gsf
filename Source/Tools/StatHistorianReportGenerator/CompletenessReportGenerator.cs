//******************************************************************************************************
//  CompletenessReportGenerator.cs - Gbtc
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
//  02/11/2014 - Stephen C. Wills
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms.DataVisualization.Charting;
using GSF.Data;
using GSF.Data.Model;
using GSF.Diagnostics;
using GSF.Historian;
using GSF.Historian.Files;
using GSF.Units;
using Root.Reports;
using Encoder = System.Drawing.Imaging.Encoder;

// ReSharper disable UnusedMember.Local
// ReSharper disable RedundantAssignment
// ReSharper disable AssignNullToNotNullAttribute
// ReSharper disable UnusedMethodReturnValue.Local
// ReSharper disable UnusedAutoPropertyAccessor.Local
namespace StatHistorianReportGenerator
{
    /// <summary>
    /// Generates GSF Data Completeness Report for a specified date.
    /// </summary>
    public class CompletenessReportGenerator
    {
        #region [ Members ]

        // Nested Types
        private class DeviceStats
        {
            public string Name;
            public double[] DataQualityErrors;
            public double[] TimeQualityErrors;
            public double[] MeasurementsReceived;
            public double[] MeasurementsExpected;
        }

        private class DataAvailability
        {
            [PrimaryKey(true)]
            public int ID { get; set; }
            public float GoodAvailableData { get; set; }
            public float BadAvailableData { get; set; }
            public float TotalAvailableData { get; set; }
        }

        // Constants

        // Note: Many of the calculations in this class use conversion
        // factors of 96 dots per inch and 25.4 millimeters per inch

        private const int ReportDays = 35;  // Gather data for 35 days - necessary for 5-day history summary
        private const int Month = 30;       // 1 month = 30 days for reporting purposes

        private const double PageMarginMillimeters = 25.4D;             // 1-inch margin
        private const double PageWidthMillimeters = 8.5D * 25.4D;       // 8.5 inch width
        private const double PageHeightMillimeters = 11.0D * 25.4D;     // 11 inch height

        private const double FooterHeightMillimeters = (10.0D / 72.0D) * 25.4D;
        private const double SpacingMillimeters = 6.0D;

        // Fields
        private string m_archiveLocation;
        private string m_titleText;
        private string m_companyText;
        private DateTime m_reportDate;
        private double m_level4Threshold;
        private double m_level3Threshold;
        private string m_level4Alias;
        private string m_level3Alias;
        private bool m_generateCsvReport;

        private List<DeviceStats> m_deviceStatsList;
        private float m_systemUpTime;

        private string m_reportFilePath;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new instance of the <see cref="CompletenessReportGenerator"/> class.
        /// </summary>
        public CompletenessReportGenerator()
        {
            m_titleText = "GSF Data Completeness Report";
            m_companyText = "Grid Protection Alliance";
            m_reportDate = DateTime.UtcNow.Date;
            m_level4Threshold = 99.0D;
            m_level3Threshold = 90.0D;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the location of the statistics archive that provides data for reporting.
        /// </summary>
        public string ArchiveLocation
        {
            get
            {
                return m_archiveLocation;
            }
            set
            {
                m_archiveLocation = value;
            }
        }

        /// <summary>
        /// Gets or sets the text that appears in the title section of the generated report.
        /// </summary>
        public string TitleText
        {
            get
            {
                return m_titleText;
            }
            set
            {
                m_titleText = value;
            }
        }

        /// <summary>
        /// Gets or sets the text that appears in the company section of the generated report.
        /// </summary>
        public string CompanyText
        {
            get
            {
                return m_companyText;
            }
            set
            {
                m_companyText = value;
            }
        }

        /// <summary>
        /// Gets or sets the date for which the report will be generated.
        /// </summary>
        public DateTime ReportDate
        {
            get
            {
                return m_reportDate;
            }
            set
            {
                m_reportDate = value.ToUniversalTime().Date;
            }
        }

        /// <summary>
        /// Gets or sets the completeness threshold at which devices will be considered to be in Level 4.
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

                if (m_level4Threshold > 100.0D)
                    m_level4Threshold = 100.0D;

                if (m_level4Threshold < 0.0D)
                    m_level4Threshold = 0.0D;

                if (m_level4Threshold < m_level3Threshold)
                    m_level3Threshold = m_level4Threshold;
            }
        }

        /// <summary>
        /// Gets or sets the completeness threshold at which devices will be considered to be in Level 3.
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

                if (m_level3Threshold > 100.0D)
                    m_level3Threshold = 100.0D;

                if (m_level3Threshold < 0.0D)
                    m_level3Threshold = 0.0D;

                if (m_level3Threshold > m_level4Threshold)
                    m_level4Threshold = m_level3Threshold;
            }
        }

        /// <summary>
        /// Gets or sets the alias to be associated with the Level 4 category.
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
        /// Gets or sets the alias to be associated with the Level 3 category.
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

        public string ReportFilePath
        {
            get
            {
                return m_reportFilePath;
            }
            set
            {
                m_reportFilePath = value;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Generates the report.
        /// </summary>
        /// <returns>The report that was generated.</returns>
        public Report GenerateReport()
        {
            DateTime now = DateTime.Now;
            Report report = new Report();
            FontDef fontDefinition = new FontDef(report, "Helvetica");
            Page pageOne = CreatePage(report);
            Page pageTwo = CreatePage(report);
            double verticalMillimeters;

            ReadStatistics();

            if (m_generateCsvReport && m_reportFilePath != null)
                GenerateCsvReportFunction(m_reportFilePath);

            PopulateDataAvailabilityTable();

            // Page one
            verticalMillimeters = PageMarginMillimeters;
            verticalMillimeters += InsertTitle(fontDefinition, pageOne, verticalMillimeters) + SpacingMillimeters;
            verticalMillimeters += InsertReportDate(fontDefinition, pageOne, verticalMillimeters);
            verticalMillimeters += InsertSystemUpTime(fontDefinition, pageOne, verticalMillimeters) + 1.5D * SpacingMillimeters;

            verticalMillimeters += InsertSectionHeader(fontDefinition, pageOne, verticalMillimeters, "5-day Device Data Completeness");
            verticalMillimeters += InsertFiveDaySummary(report, fontDefinition, pageOne, verticalMillimeters) + SpacingMillimeters;

            verticalMillimeters += InsertSectionHeader(fontDefinition, pageOne, verticalMillimeters, "Percent of Devices with Acceptable Availability (30 days)");
            verticalMillimeters += InsertBarChart(pageOne, verticalMillimeters) + SpacingMillimeters;

            verticalMillimeters += InsertSectionHeader(fontDefinition, pageOne, verticalMillimeters, "Definitions");
            verticalMillimeters += InsertDefinitions(fontDefinition, pageOne, verticalMillimeters) + SpacingMillimeters;
            InsertFooter(fontDefinition, pageOne, now, 1);

            // Page two
            verticalMillimeters = PageMarginMillimeters;
            verticalMillimeters += InsertReportDate(fontDefinition, pageTwo, verticalMillimeters) + SpacingMillimeters;

            verticalMillimeters += InsertSectionHeader(fontDefinition, pageTwo, verticalMillimeters, "Data Completeness Breakdown");
            verticalMillimeters += InsertPieChart(fontDefinition, pageTwo, verticalMillimeters) + SpacingMillimeters;

            InsertDetailsList(report, fontDefinition, pageTwo, verticalMillimeters, now, 2);

            return report;
        }

        private void PopulateDataAvailabilityTable()
        {
            const int DefaultRecordDepth = 14;

            try
            {
                using (AdoDataConnection connection = Program.GetDatabaseConnection())
                {
                    TableOperations<DataAvailability> dataAvailabilityTable = new TableOperations<DataAvailability>(connection);
                    DataAvailability[] records = dataAvailabilityTable.QueryRecords().ToArray();

                    if (records.Length == 0)
                    {
                        records = new DataAvailability[DefaultRecordDepth];

                        for (int i = 0; i < records.Length; i++)
                            records[i] = dataAvailabilityTable.NewRecord();
                    }

                    for (int i = records.Length - 1; i >= 0; i--)
                    {
                        List<DeviceStats>[] levels = GetLevels(ReportDays - (i + 1));
                        DataAvailability record = records[i];

                        record.GoodAvailableData = levels[4].Count;
                        record.BadAvailableData = levels.Take(4).Sum(level => level.Count);
                        record.TotalAvailableData = levels.Sum(level => level.Count);

                        dataAvailabilityTable.AddNewOrUpdateRecord(record);
                    }
                }
            }
            catch (Exception ex)
            {
                Program.Log.Publish(MessageLevel.Error, "PopulateDataAvailabilityTable", "Failed to populate data availability table.", exception: ex);
            }
        }

        private void GenerateCsvReportFunction(string filepath)
        {
            filepath = Path.ChangeExtension(filepath, ".csv");
            StreamWriter writer = File.CreateText(filepath);
            writer.WriteLine("Name,Completeness,DataErrors,Time Errors,StatDate");

            List<DeviceStats>[] levels = GetLevels(ReportDays);

            for(int level = 0; level < levels.Length; level++)
            {
                // Sort the devices in this level by completeness, then data errors descending, then name
                levels[level] = levels[level].OrderBy(device => Math.Round(device.MeasurementsReceived[ReportDays - 1] / device.MeasurementsExpected[ReportDays - 1], 4)).ThenByDescending(device => device.DataQualityErrors[ReportDays - 1]).ThenBy(device => device.Name).ToList();

                foreach (DeviceStats stats in levels[level])
                {
                    writer.WriteLine($"{stats.Name},{(stats.MeasurementsReceived[ReportDays - 1] / stats.MeasurementsExpected[ReportDays - 1]):P2},{stats.DataQualityErrors[ReportDays - 1]},{stats.TimeQualityErrors[ReportDays - 1]},{m_reportDate:d/M/yyyy}");
                }
            }


            writer.Dispose();
        }

        // Creates a page and sets width and height to standard 8.5x11 inches.
        private Page CreatePage(Report report)
        {
            Page page = new Page(report);
            page.rWidthMM = PageWidthMillimeters;
            page.rHeightMM = PageHeightMillimeters;
            return page;
        }

        // Reads statistics from the archive and stores them in a format that can
        // be easily manipulated to determine which devices belong in which levels.
        private void ReadStatistics()
        {
            Dictionary<string, DeviceStats> deviceStatsLookup = new Dictionary<string, DeviceStats>();
            DateTime startTime;
            DateTime endTime;

            ArchiveLocator locator;
            Dictionary<MetadataRecord, IEnumerable<IDataPoint>> measurementsReceived;
            Dictionary<MetadataRecord, IEnumerable<IDataPoint>> measurementsExpected;
            Dictionary<MetadataRecord, IEnumerable<IDataPoint>> dataQualityErrors;
            Dictionary<MetadataRecord, IEnumerable<IDataPoint>> timeQualityErrors;

            MetadataRecord dataQualityRecord;
            MetadataRecord timeQualityRecord;

            DeviceStats deviceStats;
            string signalReference;
            string deviceName;
            int index;

            // Create the statistics reader for reading statistics from the archive
            using (StatisticsReader statisticsReader = new StatisticsReader())
            {
                // Create the archive locater to
                // determine the location of the archive
                locator = new ArchiveLocator()
                {
                    ArchiveLocation = m_archiveLocation,
                    ArchiveLocationName = "Statistics",
                    ArchiveName = "STAT"
                };

                endTime = m_reportDate + TimeSpan.FromDays(1);
                startTime = endTime - TimeSpan.FromDays(ReportDays);

                // Set up and open the statistics reader
                statisticsReader.StartTime = startTime;
                statisticsReader.EndTime = endTime;
                statisticsReader.ArchiveFilePath = locator.ArchiveFilePath;
                statisticsReader.Open();

                m_systemUpTime = statisticsReader
                    .Read("SYSTEM", 15)
                    .SingleOrDefault(kvp => kvp.Key.GeneralFlags.Enabled).Value?
                    .LastOrDefault()?.Value ?? 0.0F;

                measurementsReceived = statisticsReader.Read("PMU", 4);
                measurementsExpected = statisticsReader.Read("PMU", 5);
                dataQualityErrors = statisticsReader.Read("PMU", 1);
                timeQualityErrors = statisticsReader.Read("PMU", 2);

                // Determine which devices in the archive have stats for both measurements received and measurements expected
                foreach (Tuple<MetadataRecord, MetadataRecord> tuple in measurementsReceived.Keys.Join(measurementsExpected.Keys, GetDeviceName, GetDeviceName, Tuple.Create))
                {
                    signalReference = tuple.Item1.Synonym1;
                    deviceName = GetDeviceName(tuple.Item1);

                    // Ignore statistics that were calculated by an intermediate gateway
                    if (!signalReference.StartsWith("LOCAL$") && signalReference.Contains("LOCAL$"))
                        continue;

                    // Make sure LOCAL$ statistics take precedence over other statistics calculated for the same device
                    if (deviceStatsLookup.ContainsKey(deviceName) && !signalReference.StartsWith("LOCAL$"))
                        continue;

                    dataQualityRecord = dataQualityErrors.Keys.FirstOrDefault(record => GetDeviceName(record) == deviceName);
                    timeQualityRecord = timeQualityErrors.Keys.FirstOrDefault(record => GetDeviceName(record) == deviceName);

                    // Create arrays to hold the total sum of the stats for each day being reported
                    deviceStats = new DeviceStats()
                    {
                        Name = deviceName,
                        DataQualityErrors = new double[ReportDays],
                        TimeQualityErrors = new double[ReportDays],
                        MeasurementsReceived = new double[ReportDays],
                        MeasurementsExpected = new double[ReportDays]
                    };

                    if ((object)dataQualityRecord != null)
                    {
                        // Calculate the total data quality errors for each day being reported
                        foreach (IDataPoint dataPoint in dataQualityErrors[dataQualityRecord])
                        {
                            index = (dataPoint.Time.ToDateTime() - startTime).Days;

                            if (index >= 0 && index < ReportDays)
                                deviceStats.DataQualityErrors[index] += dataPoint.Value;
                        }
                    }

                    if ((object)timeQualityRecord != null)
                    {
                        // Calculate the total time quality errors for each day being reported
                        foreach (IDataPoint dataPoint in timeQualityErrors[timeQualityRecord])
                        {
                            index = (dataPoint.Time.ToDateTime() - startTime).Days;

                            if (index >= 0 && index < ReportDays)
                                deviceStats.TimeQualityErrors[index] += dataPoint.Value;
                        }
                    }

                    // Calculate the total measurements received for each day being reported
                    foreach (IDataPoint dataPoint in measurementsReceived[tuple.Item1])
                    {
                        index = (dataPoint.Time.ToDateTime() - startTime).Days;

                        if (index >= 0 && index < ReportDays)
                            deviceStats.MeasurementsReceived[index] += dataPoint.Value;
                    }

                    // Calculate the total measurements expected for each day being reported
                    foreach (IDataPoint dataPoint in measurementsExpected[tuple.Item2])
                    {
                        index = (dataPoint.Time.ToDateTime() - startTime).Days;

                        if (index >= 0 && index < ReportDays)
                            deviceStats.MeasurementsExpected[index] += dataPoint.Value;
                    }

                    // Store the calculated stats per device
                    deviceStatsLookup[deviceName] = deviceStats;
                }
            }

            // Store the statistics data to be used in the report
            m_deviceStatsList = deviceStatsLookup.Values.ToList();
        }

        // Inserts the report date onto the given page.
        private double InsertReportDate(FontDef fontDefinition, Page page, double verticalMillimeters)
        {
            FontProp font = new FontProp(fontDefinition, 0.0D);
            font.rSizePoint = 14.0D;
            page.AddCB_MM(verticalMillimeters + font.rSizeMM, new RepString(font, m_reportDate.ToLongDateString() + " UTC"));
            return 1.8D * font.rSizeMM;
        }

        // Inserts the up-time of the system onto the given page.
        private double InsertSystemUpTime(FontDef fontDefinition, Page page, double verticalMillimeters)
        {
            FontProp font = new FontProp(fontDefinition, 0.0D);
            font.rSizePoint = 10.0D;
            page.AddCB_MM(verticalMillimeters + font.rSizeMM, new RepString(font, $"System Up-Time: {new Time(m_systemUpTime).ToString()}"));
            return font.rSizeMM;
        }

        // Inserts the given text as a section header (16-pt, bold).
        private double InsertSectionHeader(FontDef fontDefinition, Page page, double verticalMillimeters, string text)
        {
            FontProp font = new FontProp(fontDefinition, 0.0D);
            font.rSizePoint = 16.0D;
            font.bBold = true;
            page.AddCB_MM(verticalMillimeters + font.rSizeMM, new RepString(font, text));
            return 2.0D * font.rSizeMM;
        }

        // Inserts the page footer onto the given page, which includes the time of report generation as well as the page number.
        private double InsertFooter(FontDef fontDefinition, Page page, DateTime now, int pageNumber)
        {
            FontProp font = new FontProp(fontDefinition, 0.0D);
            font.rSizePoint = 12.0D;
            page.AddMM(PageMarginMillimeters, PageHeightMillimeters - PageMarginMillimeters, new RepString(font, now.ToShortDateString() + " " + now.ToShortTimeString()));
            page.AddMM(PageWidthMillimeters - PageMarginMillimeters - font.rGetTextWidthMM(pageNumber.ToString()), PageHeightMillimeters - PageMarginMillimeters, new RepString(font, pageNumber.ToString()));
            return font.rSizeMM;
        }

        // Inserts the title and company text on the given page.
        private double InsertTitle(FontDef fontDefinition, Page page, double verticalMillimeters)
        {
            FontProp titleFont = new FontProp(fontDefinition, 0.0D);
            FontProp companyFont = new FontProp(fontDefinition, 0.0D);

            titleFont.rSizePoint = 20.0D;
            companyFont.rSizePoint = 14.0D;
            titleFont.bBold = true;
            companyFont.bBold = true;

            // Title
            page.AddCB_MM(verticalMillimeters + titleFont.rSizeMM, new RepString(titleFont, m_titleText));
            verticalMillimeters += 1.5D * titleFont.rSizeMM;

            // Company
            page.AddCB_MM(verticalMillimeters + companyFont.rSizeMM, new RepString(companyFont, m_companyText));

            return (1.5D * titleFont.rSizeMM) + companyFont.rSizeMM;
        }

        // Inserts the five-day summary table on the given page.
        private double InsertFiveDaySummary(Report report, FontDef fontDefinition, Page page, double verticalMillimeters)
        {
            const double TableHeightMillimeters = 40.0D;
            const double ColumnPadding = 5.0D;
            const double RowPadding = 2.0D;
            const int RowCount = 7;

            List<DeviceStats>[] levels;
            string[] levelAliases;
            double horizontalMillimeters;
            double tableWidthMillimeters;
            double rowHeightMillimeters;

            string[] labelText;
            FontProp[] labelFonts;
            double labelsWidthMillimeters;

            string[] dayOneText;
            FontProp[] dayOneFonts;
            double dayOneWidthMillimeters;

            string[] dayTwoText;
            FontProp[] dayTwoFonts;
            double dayTwoWidthMillimeters;

            string[] dayThreeText;
            FontProp[] dayThreeFonts;
            double dayThreeWidthMillimeters;

            string[] dayFourText;
            FontProp[] dayFourFonts;
            double dayFourWidthMillimeters;

            string[] dayFiveText;
            FontProp[] dayFiveFonts;
            double dayFiveWidthMillimeters;

            // Get level data and level aliases
            levels = GetLevels(ReportDays - 4);
            levelAliases = GetLevelAliases();

            // Determine the height of each row in the table
            rowHeightMillimeters = (TableHeightMillimeters - ((RowCount - 1) * RowPadding)) / RowCount;

            // Get the text for the labels in the first column of the table
            labelText = new[] { "" }
                .Concat(Enumerable.Range(0, 5).Select(level => $"L{level}: {levelAliases[level]}").Reverse())
                .Concat(new[] { "Total" })
                .ToArray();

            labelFonts = labelText
                .Select(text =>
                {
                    FontProp font = new FontPropMM(fontDefinition, rowHeightMillimeters * 0.8D);
                    font.bBold = true;
                    return font;
                })
                .ToArray();

            labelsWidthMillimeters = labelText
                .Zip(labelFonts, (text, font) => font.rGetTextWidthMM(text))
                .Max();

            // Get the text for the device counts in the second column of the table
            dayOneText = new[] { (m_reportDate - TimeSpan.FromDays(4.0D)).ToString("MM/dd") }
                .Concat(levels.Select(level => level.Count.ToString()).Reverse())
                .Concat(new[] { levels.Sum(level => level.Count).ToString() })
                .ToArray();

            dayOneFonts = dayOneText
                .Select((text, index) =>
                {
                    FontProp font = new FontPropMM(fontDefinition, rowHeightMillimeters * 0.8D);
                    font.bBold = index == 0;
                    return font;
                })
                .ToArray();

            dayOneWidthMillimeters = dayOneText
                .Zip(dayOneFonts, (text, font) => font.rGetTextWidthMM(text))
                .Max();

            // Get the text for the device counts in the third column of the table
            levels = GetLevels(ReportDays - 3);

            dayTwoText = new[] { (m_reportDate - TimeSpan.FromDays(3.0D)).ToString("MM/dd") }
                .Concat(levels.Select(level => level.Count.ToString()).Reverse())
                .Concat(new[] { levels.Sum(level => level.Count).ToString() })
                .ToArray();

            dayTwoFonts = dayTwoText
                .Select((text, index) =>
                {
                    FontProp font = new FontPropMM(fontDefinition, rowHeightMillimeters * 0.8D);
                    font.bBold = index == 0;
                    return font;
                })
                .ToArray();

            dayTwoWidthMillimeters = dayTwoText
                .Zip(dayTwoFonts, (text, font) => font.rGetTextWidthMM(text))
                .Max();

            // Get the text for the device counts in the fourth column of the table
            levels = GetLevels(ReportDays - 2);

            dayThreeText = new[] { (m_reportDate - TimeSpan.FromDays(2.0D)).ToString("MM/dd") }
                .Concat(levels.Select(level => level.Count.ToString()).Reverse())
                .Concat(new[] { levels.Sum(level => level.Count).ToString() })
                .ToArray();

            dayThreeFonts = dayThreeText
                .Select((text, index) =>
                {
                    FontProp font = new FontPropMM(fontDefinition, rowHeightMillimeters * 0.8D);
                    font.bBold = index == 0;
                    return font;
                })
                .ToArray();

            dayThreeWidthMillimeters = dayThreeText
                .Zip(dayThreeFonts, (text, font) => font.rGetTextWidthMM(text))
                .Max();

            // Get the text for the device counts in the fifth column of the table
            levels = GetLevels(ReportDays - 1);

            dayFourText = new[] { (m_reportDate - TimeSpan.FromDays(1.0D)).ToString("MM/dd") }
                .Concat(levels.Select(level => level.Count.ToString()).Reverse())
                .Concat(new[] { levels.Sum(level => level.Count).ToString() })
                .ToArray();

            dayFourFonts = dayFourText
                .Select((text, index) =>
                {
                    FontProp font = new FontPropMM(fontDefinition, rowHeightMillimeters * 0.8D);
                    font.bBold = index == 0;
                    return font;
                })
                .ToArray();

            dayFourWidthMillimeters = dayFourText
                .Zip(dayFourFonts, (text, font) => font.rGetTextWidthMM(text))
                .Max();

            // Get the text for the device counts in the sixth column of the table
            levels = GetLevels(ReportDays);

            dayFiveText = new[] { (m_reportDate).ToString("MM/dd") }
                .Concat(levels.Select(level => level.Count.ToString()).Reverse())
                .Concat(new[] { levels.Sum(level => level.Count).ToString() })
                .ToArray();

            dayFiveFonts = dayFiveText
                .Select((text, index) =>
                {
                    FontProp font = new FontPropMM(fontDefinition, rowHeightMillimeters * 0.8D);
                    font.bBold = index == 0;
                    return font;
                })
                .ToArray();

            dayFiveWidthMillimeters = dayFiveText
                .Zip(dayFiveFonts, (text, font) => font.rGetTextWidthMM(text))
                .Max();

            // Determine the full width of the table
            tableWidthMillimeters = labelsWidthMillimeters + ColumnPadding
                + dayOneWidthMillimeters + ColumnPadding
                + dayTwoWidthMillimeters + ColumnPadding
                + dayThreeWidthMillimeters + ColumnPadding
                + dayFourWidthMillimeters + ColumnPadding
                + dayFiveWidthMillimeters;

            // Add the table to the page
            for (int i = 0; i < labelText.Length; i++)
            {
                horizontalMillimeters = (PageWidthMillimeters - tableWidthMillimeters) / 2;

                page.AddMM(horizontalMillimeters, verticalMillimeters + rowHeightMillimeters * 0.9D, new RepString(labelFonts[i], labelText[i]));
                horizontalMillimeters += labelsWidthMillimeters + ColumnPadding;

                page.AddMM(horizontalMillimeters + dayOneWidthMillimeters - dayOneFonts[i].rGetTextWidthMM(dayOneText[i]), verticalMillimeters + rowHeightMillimeters * 0.9D, new RepString(dayOneFonts[i], dayOneText[i]));
                horizontalMillimeters += dayOneWidthMillimeters + ColumnPadding;

                page.AddMM(horizontalMillimeters + dayTwoWidthMillimeters - dayTwoFonts[i].rGetTextWidthMM(dayTwoText[i]), verticalMillimeters + rowHeightMillimeters * 0.9D, new RepString(dayTwoFonts[i], dayTwoText[i]));
                horizontalMillimeters += dayTwoWidthMillimeters + ColumnPadding;

                page.AddMM(horizontalMillimeters + dayThreeWidthMillimeters - dayThreeFonts[i].rGetTextWidthMM(dayThreeText[i]), verticalMillimeters + rowHeightMillimeters * 0.9D, new RepString(dayThreeFonts[i], dayThreeText[i]));
                horizontalMillimeters += dayThreeWidthMillimeters + ColumnPadding;

                page.AddMM(horizontalMillimeters + dayFourWidthMillimeters - dayFourFonts[i].rGetTextWidthMM(dayFourText[i]), verticalMillimeters + rowHeightMillimeters * 0.9D, new RepString(dayFourFonts[i], dayFourText[i]));
                horizontalMillimeters += dayFourWidthMillimeters + ColumnPadding;

                page.AddMM(horizontalMillimeters + dayFiveWidthMillimeters - dayFiveFonts[i].rGetTextWidthMM(dayFiveText[i]), verticalMillimeters + rowHeightMillimeters * 0.9D, new RepString(dayFiveFonts[i], dayFiveText[i]));
                verticalMillimeters += rowHeightMillimeters + RowPadding;
            }

            page.AddMM((PageWidthMillimeters - tableWidthMillimeters) / 2, verticalMillimeters - rowHeightMillimeters - (1.5D * RowPadding), new RepRectMM(new BrushProp(report, Color.Black), labelsWidthMillimeters + ColumnPadding + dayOneWidthMillimeters + ColumnPadding + dayTwoWidthMillimeters + ColumnPadding + dayThreeWidthMillimeters + ColumnPadding + dayFourWidthMillimeters + ColumnPadding + dayFiveWidthMillimeters, 0.1D));

            return TableHeightMillimeters;
        }

        // Inserts the bar chart onto the given page.
        private double InsertBarChart(Page page, double verticalMillimeters)
        {
            const double ChartWidthMillimeters = (550.0D / 96.0D) * 25.4D;
            const double ChartHeightMillimeters = (275.0D / 96.0D) * 25.4D;

            using (Chart chart = GetBarChart())
                page.AddMM((PageWidthMillimeters - ChartWidthMillimeters) / 2.0D, verticalMillimeters + ChartHeightMillimeters, new RepImageMM(ChartToImage(chart), ChartWidthMillimeters, ChartHeightMillimeters));

            return ChartHeightMillimeters;
        }

        // Insert the list of definitions on the given page.
        private double InsertDefinitions(FontDef fontDefinition, Page page, double verticalMillimeters)
        {
            const double FontSize = 8.0D;
            const double FooterWidthMillimeters = 6.5D * 25.4D;

            FontProp font = new FontProp(fontDefinition, 0.0D);
            StringBuilder builder;
            int lengthWithoutWord;
            double lineHeightMillimeters;

            string definitionsText = $"Level 4: {m_level4Alias} - Devices which are reporting as expected, with a completeness of at least {m_level4Threshold:0.##}% on the report date." + Environment.NewLine +
                                     $"Level 3: {m_level3Alias} - Devices with a completeness of at least {m_level3Threshold:0.##}% on the report date." + Environment.NewLine +
                                     $"Level 2: Poor - Devices which reported on the report date, but had an completeness below {m_level3Threshold:0.##}%." + Environment.NewLine +
                "Level 1: Offline - Devices which did not report on the report date, but have reported at some time during the 30 days prior to the report date." + Environment.NewLine +
                "Level 0: Failed - Devices which have not reported during the 30 days prior to the report date." + Environment.NewLine +
                "Completeness: Percentage of measurements received over total measurements expected, per device." + Environment.NewLine +
                "Acceptable Availability: Devices which are in Level 4 or Level 3.";

            // Break the definitions text into lines
            string[] lines = definitionsText.Split(new[] { Environment.NewLine }, StringSplitOptions.None);

            // Set font size and line spacing
            font.rSizePoint = FontSize;
            lineHeightMillimeters = 1.5D * font.rSizeMM;

            foreach (string line in lines)
            {
                // String builder will be used to determine if the line is too wide to fit
                // on the page, and to break the line into multiple lines if necessary
                builder = new StringBuilder();

                // Break the line into words
                foreach (string word in line.Split())
                {
                    // Store the length of the line without an additional word so we can restore
                    // the original line if the additional word makes the line too long to fit
                    lengthWithoutWord = builder.Length;

                    // Add white space if this is not the first word in the line
                    if (builder.Length > 0)
                        builder.Append(' ');

                    // Add the word to the line
                    builder.Append(word);

                    // If the line is too long, put the original line onto the page and
                    // trim the string builder down to only the word we just added
                    if (font.rGetTextWidthMM(builder.ToString()) > FooterWidthMillimeters)
                    {
                        page.AddMM(PageMarginMillimeters, verticalMillimeters + font.rSizeMM, new RepString(font, builder.ToString(0, lengthWithoutWord)));
                        builder.Remove(0, lengthWithoutWord);
                        builder.Insert(0, "             ");
                        verticalMillimeters += lineHeightMillimeters;
                    }
                }

                // Add the remainder of the line to the page.
                page.AddMM(PageMarginMillimeters, verticalMillimeters + font.rSizeMM, new RepString(font, builder.ToString()));
                verticalMillimeters += 1.5D * lineHeightMillimeters;
            }

            return 0.0D;
        }

        // Inserts the pie chart to the given page, which includes the level summary for the report date beside it.
        private double InsertPieChart(FontDef fontDefinition, Page page, double verticalMillimeters)
        {
            const double ChartWidthMillimeters = (300.0D / 96.0D) * 25.4D;
            const double ChartHeightMillimeters = (300.0D / 96.0D) * 25.4D;
            const double ColumnPadding = 10.0D;

            List<DeviceStats>[] levels = GetLevels(ReportDays);
            Color[] colors = GetLevelColors();
            double verticalStart = verticalMillimeters;

            double totalWidthMillimeters;
            double horizontalMillimeters;
            double rowHeightMillimeters;

            string[] labelText;
            FontProp[] labelFonts;
            double labelsWidthMillimeters;

            string[] countText;
            FontProp[] countFonts;
            double countsWidthMillimeters;

            using (Chart chart = GetPieChart())
            {
                // Determine the height of each row of the level summary
                rowHeightMillimeters = (ChartHeightMillimeters * 0.8D) / levels.Length;

                // Add vertical indent of 10% of the chart height
                verticalMillimeters += ChartHeightMillimeters * 0.1D;

                // Get the text for the labels in the first column of the table
                labelText = levels
                    .Select((level, index) => "Level " + index)
                    .Reverse()
                    .ToArray();

                labelFonts = labelText
                    .Select(text =>
                    {
                        FontProp font = new FontPropMM(fontDefinition, rowHeightMillimeters * 0.6D);
                        font.bBold = true;
                        return font;
                    })
                    .Reverse()
                    .ToArray();

                labelsWidthMillimeters = labelText
                    .Zip(labelFonts, (text, font) => font.rGetTextWidthMM(text))
                    .Max();

                // Get the text for the device counts in the second column of the table
                countText = levels
                    .Select(level => level.Count.ToString())
                    .Reverse()
                    .ToArray();

                countFonts = countText
                    .Select((text, level) => (FontProp)new FontPropMM(fontDefinition, rowHeightMillimeters * 0.6D, colors[level]))
                    .Reverse()
                    .ToArray();

                countsWidthMillimeters = countText
                    .Zip(countFonts, (text, font) => font.rGetTextWidthMM(text))
                    .Max();

                // Determine the total width of the pie chart and level summary,
                // then set horizontalMillimeters such that the pie chart and level
                // summary are centered on the page
                totalWidthMillimeters = labelsWidthMillimeters + ColumnPadding + countsWidthMillimeters + ColumnPadding + ChartWidthMillimeters;
                horizontalMillimeters = (PageWidthMillimeters - totalWidthMillimeters) / 2.0D;

                // Add the table to the page
                for (int i = 0; i < levels.Length; i++)
                {
                    page.AddMM(horizontalMillimeters, verticalMillimeters + rowHeightMillimeters * 0.8D, new RepString(labelFonts[i], labelText[i]));
                    page.AddMM(horizontalMillimeters + labelsWidthMillimeters + ColumnPadding + countsWidthMillimeters - countFonts[i].rGetTextWidthMM(countText[i]), verticalMillimeters + rowHeightMillimeters * 0.8D, new RepString(countFonts[i], countText[i]));
                    verticalMillimeters += rowHeightMillimeters;
                }

                // Add the pie chart to the page
                verticalMillimeters = verticalStart;
                page.AddMM(horizontalMillimeters + labelsWidthMillimeters + ColumnPadding + countsWidthMillimeters + ColumnPadding, verticalMillimeters + ChartHeightMillimeters, new RepImageMM(ChartToImage(chart), ChartWidthMillimeters, ChartHeightMillimeters));
            }

            return ChartHeightMillimeters;
        }

        // Add detailed information about each individual device, starting on the current page and adding new pages as necessary.
        private void InsertDetailsList(Report report, FontDef fontDefinition, Page page, double verticalMillimeters, DateTime now, int pageNumber)
        {
            const double ColumnPaddingMillimeters = 10.0D;
            const double RowPaddingMillimeters = 2.0D;

            double[] columnWidthMillimeters;
            double listWidthMillimeters;
            double horizontalMillimeters;

            FontProp levelFont = new FontPropMM(fontDefinition, 0.0D);
            FontProp columnHeaderFont = new FontPropMM(fontDefinition, 0.0D);
            FontProp rowFont = new FontPropMM(fontDefinition, 0.0D);

            List<DeviceStats>[] levels = GetLevels(ReportDays);
            string[][][] levelDetails = new string[levels.Length][][];
            string[][] deviceDetails;
            string[] columnHeaders;

            // Set up fonts to be used in the details list
            levelFont.rSizePoint = 12.0D;
            columnHeaderFont.rSizePoint = 10.0D;
            rowFont.rSizePoint = 10.0D;
            levelFont.bBold = true;
            columnHeaderFont.bBold = true;

            // Set up the column header and the initial values for the column widths
            columnHeaders = new[] { "Name", "Completeness", "Data Errors", "Time Errors" };
            columnWidthMillimeters = columnHeaders.Select(columnHeaderFont.rGetTextWidthMM).ToArray();

            for (int level = 0; level < levels.Length; level++)
            {
                // Set up the device details array for the current level
                deviceDetails = new string[levels[level].Count][];
                levelDetails[level] = deviceDetails;

                // Sort the devices in this level by completeness, then data errors descending, then name
                levels[level] = levels[level].OrderBy(device => Math.Round(device.MeasurementsReceived[ReportDays - 1] / device.MeasurementsExpected[ReportDays - 1], 4)).ThenByDescending(device => device.DataQualityErrors[ReportDays - 1]).ThenBy(device => device.Name).ToList();

                for (int device = 0; device < levels[level].Count; device++)
                {
                    // Populate the device details with data for each device
                    deviceDetails[device] = new[]
                    {
                        levels[level][device].Name,
                        (levels[level][device].MeasurementsReceived[ReportDays - 1] / levels[level][device].MeasurementsExpected[ReportDays - 1]).ToString("0.##%"),
                        levels[level][device].DataQualityErrors[ReportDays - 1].ToString("#,##0"),
                        levels[level][device].TimeQualityErrors[ReportDays - 1].ToString("#,##0")
                    };

                    // Update the column widths if they need to be widened to accommodate the data
                    columnWidthMillimeters = columnWidthMillimeters.Zip(deviceDetails[device], (currentWidth, text) => Math.Max(currentWidth, rowFont.rGetTextWidthMM(text))).ToArray();
                }
            }

            // Determine the total width of the list so that it can be centered
            listWidthMillimeters = columnWidthMillimeters.Sum(width => width + ColumnPaddingMillimeters) - ColumnPaddingMillimeters;

            for (int level = 0; level < levels.Length; level++)
            {
                // Get the device details for the current level
                deviceDetails = levelDetails[level];

                // If the level has no data, don't bother adding it to the details list
                if (deviceDetails.Length > 0)
                {
                    // If the height of the level header, plus the column header, plus one row of data reaches beyond the bottom of the page, start a new page
                    if (verticalMillimeters + levelFont.rSizeMM + columnHeaderFont.rSizeMM + rowFont.rSizeMM > PageHeightMillimeters - PageMarginMillimeters - FooterHeightMillimeters - SpacingMillimeters)
                    {
                        // Insert footer on the current page first
                        InsertFooter(fontDefinition, page, now, pageNumber);

                        // Increment the page number and create a new page
                        page = CreatePage(report);
                        pageNumber++;

                        // Add the report date to the top of the page
                        verticalMillimeters = PageMarginMillimeters;
                        verticalMillimeters += InsertReportDate(fontDefinition, page, PageMarginMillimeters) + SpacingMillimeters;
                    }

                    // Add the level header to the page
                    horizontalMillimeters = (PageWidthMillimeters - listWidthMillimeters) / 2.0D;
                    page.AddMM(horizontalMillimeters, verticalMillimeters + levelFont.rSizeMM, new RepString(levelFont, "Level " + level));
                    verticalMillimeters += levelFont.rSizeMM + RowPaddingMillimeters;

                    // Add the column header to the page
                    for (int i = 0; i < columnHeaders.Length; i++)
                    {
                        page.AddMM(horizontalMillimeters, verticalMillimeters + columnHeaderFont.rSizeMM, new RepString(columnHeaderFont, columnHeaders[i]));
                        horizontalMillimeters += columnWidthMillimeters[i] + ColumnPaddingMillimeters;
                    }

                    verticalMillimeters += columnHeaderFont.rSizeMM + RowPaddingMillimeters;

                    // Each device is on its own row in the details list
                    foreach (string[] row in deviceDetails)
                    {
                        // If the height of the row reaches beyond the bottom of the page, start a new page
                        if (verticalMillimeters + rowFont.rSizeMM > PageHeightMillimeters - PageMarginMillimeters - FooterHeightMillimeters - SpacingMillimeters)
                        {
                            // Insert footer on the current page first
                            InsertFooter(fontDefinition, page, now, pageNumber);

                            // Increment the page number and create a new page
                            page = CreatePage(report);
                            pageNumber++;

                            // Add the report date to the top of the page
                            verticalMillimeters = PageMarginMillimeters;
                            verticalMillimeters += InsertReportDate(fontDefinition, page, PageMarginMillimeters) + SpacingMillimeters;

                            // Add the level header again, designated with the abbreviation "contd."
                            horizontalMillimeters = (PageWidthMillimeters - listWidthMillimeters) / 2.0D;
                            page.AddMM(horizontalMillimeters, verticalMillimeters + levelFont.rSizeMM, new RepString(levelFont, "Level " + level + " (contd.)"));
                            verticalMillimeters += levelFont.rSizeMM + RowPaddingMillimeters;

                            // Add the column header again to the top of the new page
                            for (int i = 0; i < columnHeaders.Length; i++)
                            {
                                page.AddMM(horizontalMillimeters, verticalMillimeters + columnHeaderFont.rSizeMM, new RepString(columnHeaderFont, columnHeaders[i]));
                                horizontalMillimeters += columnWidthMillimeters[i] + ColumnPaddingMillimeters;
                            }

                            verticalMillimeters += columnHeaderFont.rSizeMM + RowPaddingMillimeters;
                        }

                        // Add the device data, on its own row, to the details list
                        horizontalMillimeters = (PageWidthMillimeters - listWidthMillimeters) / 2.0D;

                        for (int column = 0; column < row.Length; column++)
                        {
                            page.AddMM(horizontalMillimeters, verticalMillimeters + rowFont.rSizeMM, new RepString(rowFont, row[column]));
                            horizontalMillimeters += columnWidthMillimeters[column] + ColumnPaddingMillimeters;
                        }

                        verticalMillimeters += rowFont.rSizeMM + RowPaddingMillimeters;
                    }

                    // Add space between levels
                    verticalMillimeters += levelFont.rSizeMM;
                }
            }

            // Insert the footer on the last page of the report
            InsertFooter(fontDefinition, page, now, pageNumber);
        }

        private Chart GetBarChart()
        {
            Chart chart = new Chart();
            ChartArea area = new ChartArea();
            Series series = new Series();

            List<DeviceStats>[] levels;
            double percentGood;

            area.AxisX.MajorGrid.Enabled = false;
            area.AxisX.Interval = 1;
            area.AxisX.IntervalType = DateTimeIntervalType.Days;
            area.AxisX.LabelAutoFitMinFontSize = 60;
            area.AxisX.LabelStyle.Format = "MM/dd";
            area.AxisX.LabelStyle.IsEndLabelVisible = false;
            area.AxisX.MajorGrid.Enabled = false;
            area.AxisY.Minimum = 0;
            area.AxisY.Maximum = 100;
            area.AxisY.LabelAutoFitMinFontSize = 35;
            area.AxisY.LabelStyle.Format = "{0}%";
            series.ChartType = SeriesChartType.Column;

            for (int i = 1; i <= Month; i++)
            {
                levels = GetLevels(i + (ReportDays - Month));
                percentGood = levels.Skip(3).Sum(level => level.Count) / (double)levels.Sum(level => level.Count);
                series.Points.AddXY((m_reportDate - TimeSpan.FromDays(Month - i)), percentGood * 100.0D);
            }

            chart.Width = 2400;
            chart.Height = 1200;
            chart.ChartAreas.Add(area);
            chart.Series.Add(series);

            return chart;
        }

        private Chart GetPieChart()
        {
            Chart chart = new Chart();
            ChartArea area = new ChartArea();
            Series series = new Series();
            DataPoint point;

            List<DeviceStats>[] levels = GetLevels(ReportDays);
            Color[] colors = GetLevelColors();
            int totalDeviceCount;

            area.Area3DStyle.Enable3D = true;
            series.ChartType = SeriesChartType.Pie;
            totalDeviceCount = levels.Sum(level => level.Count);

            for (int i = 0; i < levels.Length; i++)
            {
                if (levels[i].Count > 0)
                {
                    point = new DataPoint();
                    point.SetValueXY($"L{i} ({100.0D * levels[i].Count / totalDeviceCount:N0}%)", levels[i].Count);
                    point.Color = colors[i];
                    point.Font = new Font(FontFamily.GenericSansSerif, 40.0F);
                    series.Points.Add(point);
                }
            }

            chart.Width = 1200;
            chart.Height = 1200;
            chart.ChartAreas.Add(area);
            chart.Series.Add(series);

            return chart;
        }

        private Stream ChartToImage(Chart chart)
        {
            MemoryStream stream = new MemoryStream();
            chart.SaveImage(stream, ChartImageFormat.Bmp);
            stream.Position = 0;
            return BitmapToJpg(stream);
        }

        private Stream BitmapToJpg(Stream bitmapStream)
        {
            MemoryStream jpgStream = new MemoryStream();
            Bitmap bitmap = new Bitmap(bitmapStream);

            ImageCodecInfo encoder = ImageCodecInfo.GetImageEncoders()
                .SingleOrDefault(enc => enc.FormatID == ImageFormat.Jpeg.Guid);

            EncoderParameters parameters = new EncoderParameters(1);

            if ((object)encoder == null)
                throw new InvalidOperationException("Unable to convert bitmap to jpg.");

            parameters.Param[0] = new EncoderParameter(Encoder.Quality, 500L);
            bitmap.Save(jpgStream, encoder, parameters);
            jpgStream.Position = 0;

            return jpgStream;
        }

        private string GetDeviceName(MetadataRecord record)
        {
            string signalReference = record.Synonym1;
            return signalReference.Remove(signalReference.LastIndexOf('!')).Replace("LOCAL$", "");
        }

        private List<DeviceStats>[] GetLevels(int reportDay)
        {
            return Enumerable.Range(0, 5)
                .Select(level => m_deviceStatsList.Where(devStats => GetLevel(devStats, reportDay) == level).ToList())
                .ToArray();
        }

        private int GetLevel(DeviceStats deviceStats, int reportDay)
        {
            int reportDayIndex = reportDay - 1;
            double measurementsReceived = deviceStats.MeasurementsReceived[reportDayIndex];
            double measurementsExpected = deviceStats.MeasurementsExpected[reportDayIndex];

            if (measurementsExpected == 0.0D)
                return -1;

            if (measurementsReceived >= (m_level4Threshold * measurementsExpected) / 100.0D)
                return 4;

            if (measurementsReceived >= (m_level3Threshold * measurementsExpected) / 100.0D)
                return 3;

            if (measurementsReceived > 0.0D)
                return 2;

            if (deviceStats.MeasurementsReceived.Skip(reportDay - Month).Take(ReportDays).Any(receivedStat => receivedStat > 0.0D))
                return 1;

            return 0;
        }

        private Color[] GetLevelColors()
        {
            return new[]
            {
                Color.LightGray,
                Color.FromArgb(255, 255, 50, 50),
                Color.Gold,
                Color.DodgerBlue,
                Color.LimeGreen
            };
        }

        private string[] GetLevelAliases()
        {
            return new[]
            {
                "Failed",
                "Offline",
                "Poor",
                m_level3Alias,
                m_level4Alias
            };
        }

        #endregion
    }
}
