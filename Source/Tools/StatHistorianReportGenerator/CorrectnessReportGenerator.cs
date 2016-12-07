//******************************************************************************************************
//  CorrectnessReportGenerator.cs - Gbtc
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
//  10/20/2014 - Stephen C. Wills
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
using System.Text.RegularExpressions;
using System.Windows.Forms.DataVisualization.Charting;
using GSF.Collections;
using GSF.Historian;
using GSF.Historian.Files;
using Root.Reports;
using Encoder = System.Drawing.Imaging.Encoder;

namespace StatHistorianReportGenerator
{
    public class CorrectnessReportGenerator
    {
        #region [ Members ]

        // Nested Types
        private class DeviceStats
        {
            public string Name;
            public Aggregate MeasurementsExpected;
            public Aggregate MeasurementsReceived;
            public Dictionary<string, SignalStats> SignalStatsLookup;

            public double GetMeasurementsLatched(int index)
            {
                return SignalStatsLookup.Values
                    .Sum(signalStats => signalStats.MeasurementsLatched[index]);
            }

            public double GetMeasurementsUnreasonable(int index)
            {
                return SignalStatsLookup.Values
                    .Sum(signalStats => signalStats.MeasurementsUnreasonable[index]);
            }

            public double GetCorrectness(int index)
            {
                return 100.0D * (MeasurementsReceived[index] - GetMeasurementsLatched(index) - GetMeasurementsUnreasonable(index)) / MeasurementsExpected[index];
            }
        }

        private class SignalStats
        {
            public string Name;
            public Aggregate MeasurementsLatched;
            public Aggregate MeasurementsUnreasonable;
        }

        private class Aggregate
        {
            private double[] m_values;

            public Aggregate(int valueCount)
            {
                m_values = new double[valueCount];
            }

            public void Add(int index, double value)
            {
                m_values[index] += value;
            }

            public double this[int index]
            {
                get
                {
                    return m_values[index];
                }
            }
        }

        // Constants

        // Note: Many of the calculations in this class use conversion
        // factors of 96 dots per inch and 25.4 millimeters per inch

        private const int ReportDays = 30;  // Gather data for 30 days - necessary for 30-day history chart
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

        private List<DeviceStats> m_deviceStatsList;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new instance of the <see cref="CorrectnessReportGenerator"/> class.
        /// </summary>
        public CorrectnessReportGenerator()
        {
            m_titleText = "GSF Data Correctness Report";
            m_companyText = "Grid Protection Alliance";
            m_reportDate = DateTime.UtcNow.Date;
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

            // Page one
            verticalMillimeters = PageMarginMillimeters;
            verticalMillimeters += InsertTitle(fontDefinition, pageOne, verticalMillimeters) + SpacingMillimeters;
            verticalMillimeters += InsertReportDate(fontDefinition, pageOne, verticalMillimeters) + 2.0D * SpacingMillimeters;

            verticalMillimeters += InsertSectionHeader(fontDefinition, pageOne, verticalMillimeters, "5-day Correctness Summary");
            verticalMillimeters += InsertFiveDaySummary(report, fontDefinition, pageOne, verticalMillimeters) + SpacingMillimeters;

            verticalMillimeters += InsertSectionHeader(fontDefinition, pageOne, verticalMillimeters, "30-day Correctness Overview");
            verticalMillimeters += InsertBarChart(pageOne, verticalMillimeters) + SpacingMillimeters;

            verticalMillimeters += InsertSectionHeader(fontDefinition, pageOne, verticalMillimeters, "Definitions");
            verticalMillimeters += InsertDefinitions(fontDefinition, pageOne, verticalMillimeters) + SpacingMillimeters;
            InsertFooter(fontDefinition, pageOne, now, 1);

            // Page two
            verticalMillimeters = PageMarginMillimeters;
            verticalMillimeters += InsertReportDate(fontDefinition, pageTwo, verticalMillimeters) + SpacingMillimeters;

            verticalMillimeters += InsertSectionHeader(fontDefinition, pageTwo, verticalMillimeters, "Data Correctness Breakdown");
            InsertDetailsList(report, fontDefinition, pageTwo, verticalMillimeters, now, 2);

            return report;
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
            ArchiveLocator locator;
            DateTime startTime;
            DateTime endTime;

            Dictionary<int, Aggregate> aggregateLookup;
            Aggregate currentAggregate;
            int currentHistorianID;

            Dictionary<string, DeviceStats> deviceStatsLookup;
            DeviceStats deviceStats;
            SignalStats signalStats;
            string signalName;
            int index;

            deviceStatsLookup = new Dictionary<string, DeviceStats>();

            // Create the statistics reader for reading statistics from the archive
            using (StatisticsReader statisticsReader = new StatisticsReader())
            {
                // Create the archive locator to
                // determine the location of the archive
                locator = new ArchiveLocator()
                {
                    ArchiveLocation = m_archiveLocation,
                    ArchiveLocationName = "Statistics",
                    ArchiveName = "STAT"
                };

                endTime = m_reportDate.ToUniversalTime() + TimeSpan.FromDays(1);
                startTime = endTime - TimeSpan.FromDays(ReportDays);

                // Set up and open the statistics reader
                statisticsReader.StartTime = startTime;
                statisticsReader.EndTime = endTime;
                statisticsReader.ArchiveFilePath = locator.ArchiveFilePath;
                statisticsReader.Open();

                // Create lookup tables for each
                aggregateLookup = new Dictionary<int, Aggregate>();

                foreach (MetadataRecord record in statisticsReader.MetadataRecords)
                {
                    if (IsDesiredDeviceStat(record.Synonym1))
                    {
                        deviceStats = deviceStatsLookup.GetOrAdd(GetDeviceNameForStat(record.Synonym1), name => new DeviceStats()
                        {
                            Name = name,
                            MeasurementsExpected = new Aggregate(ReportDays),
                            MeasurementsReceived = new Aggregate(ReportDays),
                            SignalStatsLookup = new Dictionary<string, SignalStats>()
                        });

                        if (record.Synonym1.EndsWith("!PMU-ST5", StringComparison.Ordinal))
                            aggregateLookup[record.HistorianID] = deviceStats.MeasurementsExpected;
                        else if (record.Synonym1.EndsWith("!PMU-ST4", StringComparison.Ordinal))
                            aggregateLookup[record.HistorianID] = deviceStats.MeasurementsReceived;
                    }
                    else if (IsDesiredSignalStat(record.Synonym1))
                    {
                        signalName = GetSignalName(record.Synonym1);

                        deviceStats = deviceStatsLookup.GetOrAdd(GetDeviceNameForSignal(signalName), name => new DeviceStats()
                        {
                            Name = name,
                            MeasurementsExpected = new Aggregate(ReportDays),
                            MeasurementsReceived = new Aggregate(ReportDays),
                            SignalStatsLookup = new Dictionary<string, SignalStats>()
                        });

                        signalStats = deviceStats.SignalStatsLookup.GetOrAdd(signalName, name => new SignalStats()
                        {
                            Name = name,
                            MeasurementsLatched = new Aggregate(ReportDays),
                            MeasurementsUnreasonable = new Aggregate(ReportDays)
                        });

                        if (record.Synonym1.StartsWith("980!"))
                            aggregateLookup[record.HistorianID] = signalStats.MeasurementsLatched;
                        else if (record.Synonym1.StartsWith("900!"))
                            aggregateLookup[record.HistorianID] = signalStats.MeasurementsUnreasonable;
                    }
                }

                currentAggregate = null;
                currentHistorianID = -1;

                foreach (IDataPoint dataPoint in statisticsReader.Read(aggregateLookup.Keys))
                {
                    index = (dataPoint.Time.ToDateTime() - startTime).Days;

                    if (index < 0 || index >= ReportDays)
                        continue;

                    if (dataPoint.HistorianID != currentHistorianID)
                    {
                        aggregateLookup.TryGetValue(dataPoint.HistorianID, out currentAggregate);
                        currentHistorianID = dataPoint.HistorianID;
                    }

                    if ((object)currentAggregate != null)
                        currentAggregate.Add(index, dataPoint.Value);
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
            page.AddCB_MM(verticalMillimeters + font.rSizeMM, new RepString(font, m_reportDate.ToLongDateString()));
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
            const double TableHeightMillimeters = 30.0D;
            const double ColumnPadding = 5.0D;
            const double RowPadding = 2.0D;
            const int RowCount = 5;

            double horizontalMillimeters;
            double tableWidthMillimeters;
            double rowHeightMillimeters;

            double measurementsExpected;
            double measurementsReceived;
            double measurementsLatched;
            double measurementsUnreasonable;

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

            // Determine the height of each row in the table
            rowHeightMillimeters = (TableHeightMillimeters - ((RowCount - 1) * RowPadding)) / RowCount;

            // Get the text for the labels in the first column of the table
            labelText = new string[]
            {
                "",
                "Good",
                "Latched",
                "Unreasonable",
            };

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

            // Get the text for the measurement counts in the second column of the table
            measurementsExpected = m_deviceStatsList.Sum(deviceStats => deviceStats.MeasurementsExpected[ReportDays - 5]);
            measurementsReceived = m_deviceStatsList.Sum(deviceStats => deviceStats.MeasurementsReceived[ReportDays - 5]);
            measurementsLatched = m_deviceStatsList.Sum(deviceStats => deviceStats.GetMeasurementsLatched(ReportDays - 5));
            measurementsUnreasonable = m_deviceStatsList.Sum(deviceStats => deviceStats.GetMeasurementsUnreasonable(ReportDays - 5));

            dayOneText = new string[]
            {
                (m_reportDate - TimeSpan.FromDays(4.0D)).ToString("MM/dd"),
                ((measurementsReceived - measurementsLatched - measurementsUnreasonable) / measurementsExpected).ToString("0.00%"),
                (measurementsLatched / measurementsExpected).ToString("0.00%"),
                (measurementsUnreasonable / measurementsExpected).ToString("0.00%")
            };

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

            // Get the text for the measurement counts in the third column of the table
            measurementsExpected = m_deviceStatsList.Sum(deviceStats => deviceStats.MeasurementsExpected[ReportDays - 4]);
            measurementsReceived = m_deviceStatsList.Sum(deviceStats => deviceStats.MeasurementsReceived[ReportDays - 4]);
            measurementsLatched = m_deviceStatsList.Sum(deviceStats => deviceStats.GetMeasurementsLatched(ReportDays - 4));
            measurementsUnreasonable = m_deviceStatsList.Sum(deviceStats => deviceStats.GetMeasurementsUnreasonable(ReportDays - 4));

            dayTwoText = new string[]
            {
                (m_reportDate - TimeSpan.FromDays(3.0D)).ToString("MM/dd"),
                ((measurementsReceived - measurementsLatched - measurementsUnreasonable) / measurementsExpected).ToString("0.00%"),
                (measurementsLatched / measurementsExpected).ToString("0.00%"),
                (measurementsUnreasonable / measurementsExpected).ToString("0.00%")
            };

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

            // Get the text for the measurement counts in the fourth column of the table
            measurementsExpected = m_deviceStatsList.Sum(deviceStats => deviceStats.MeasurementsExpected[ReportDays - 3]);
            measurementsReceived = m_deviceStatsList.Sum(deviceStats => deviceStats.MeasurementsReceived[ReportDays - 3]);
            measurementsLatched = m_deviceStatsList.Sum(deviceStats => deviceStats.GetMeasurementsLatched(ReportDays - 3));
            measurementsUnreasonable = m_deviceStatsList.Sum(deviceStats => deviceStats.GetMeasurementsUnreasonable(ReportDays - 3));

            dayThreeText = new string[]
            {
                (m_reportDate - TimeSpan.FromDays(2.0D)).ToString("MM/dd"),
                ((measurementsReceived - measurementsLatched - measurementsUnreasonable) / measurementsExpected).ToString("0.00%"),
                (measurementsLatched / measurementsExpected).ToString("0.00%"),
                (measurementsUnreasonable / measurementsExpected).ToString("0.00%")
            };

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

            // Get the text for the measurement counts in the fifth column of the table
            measurementsExpected = m_deviceStatsList.Sum(deviceStats => deviceStats.MeasurementsExpected[ReportDays - 2]);
            measurementsReceived = m_deviceStatsList.Sum(deviceStats => deviceStats.MeasurementsReceived[ReportDays - 2]);
            measurementsLatched = m_deviceStatsList.Sum(deviceStats => deviceStats.GetMeasurementsLatched(ReportDays - 2));
            measurementsUnreasonable = m_deviceStatsList.Sum(deviceStats => deviceStats.GetMeasurementsUnreasonable(ReportDays - 2));

            dayFourText = new string[]
            {
                (m_reportDate - TimeSpan.FromDays(1.0D)).ToString("MM/dd"),
                ((measurementsReceived - measurementsLatched - measurementsUnreasonable) / measurementsExpected).ToString("0.00%"),
                (measurementsLatched / measurementsExpected).ToString("0.00%"),
                (measurementsUnreasonable / measurementsExpected).ToString("0.00%")
            };

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

            // Get the text for the measurement counts in the sixth column of the table
            measurementsExpected = m_deviceStatsList.Sum(deviceStats => deviceStats.MeasurementsExpected[ReportDays - 1]);
            measurementsReceived = m_deviceStatsList.Sum(deviceStats => deviceStats.MeasurementsReceived[ReportDays - 1]);
            measurementsLatched = m_deviceStatsList.Sum(deviceStats => deviceStats.GetMeasurementsLatched(ReportDays - 1));
            measurementsUnreasonable = m_deviceStatsList.Sum(deviceStats => deviceStats.GetMeasurementsUnreasonable(ReportDays - 1));

            dayFiveText = new string[]
            {
                (m_reportDate).ToString("MM/dd"),
                ((measurementsReceived - measurementsLatched - measurementsUnreasonable) / measurementsExpected).ToString("0.00%"),
                (measurementsLatched / measurementsExpected).ToString("0.00%"),
                (measurementsUnreasonable / measurementsExpected).ToString("0.00%")
            };

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

            string definitionsText = "Good: Measurements received which are neither latched nor unreasonable." + Environment.NewLine +
                                     "Latched: Measurements received which have maintained the same value for an extended period of time." + Environment.NewLine +
                                     "Unreasonable: Measurements received whose values have fallen outside of the range defined by reasonability constraints." + Environment.NewLine +
                                     "Correctness: Percentage of good measurements over total measurements expected, per device.";

            // Break the definitions text into lines
            string[] lines = definitionsText.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);

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

        // Add detailed information about each individual device, starting on the current page and adding new pages as necessary.
        private void InsertDetailsList(Report report, FontDef fontDefinition, Page page, double verticalMillimeters, DateTime now, int pageNumber)
        {
            const double ColumnPaddingMillimeters = 10.0D;
            const double RowPaddingMillimeters = 2.0D;
            const double SignalIndentMillimeters = 25.4D;

            double[] deviceColumnWidthMillimeters;
            double[] signalColumnWidthMillimeters;
            double listWidthMillimeters;
            double horizontalMillimeters;

            FontProp deviceHeaderFont = new FontPropMM(fontDefinition, 0.0D);
            FontProp deviceRowFont = new FontPropMM(fontDefinition, 0.0D);
            FontProp signalHeaderFont = new FontPropMM(fontDefinition, 0.0D);
            FontProp signalRowFont = new FontPropMM(fontDefinition, 0.0D);

            List<DeviceStats> badDevices;
            List<SignalStats> badSignals;

            int deviceIndex;
            int signalIndex;
            string[] deviceHeaders;
            string[] signalHeaders;
            string[][] deviceDetails;
            string[][][] signalDetails;

            // Set up fonts to be used in the details list
            deviceHeaderFont.rSizePoint = 12.0D;
            deviceRowFont.rSizePoint = 12.0D;
            signalHeaderFont.rSizePoint = 10.0D;
            signalRowFont.rSizePoint = 10.0D;
            deviceHeaderFont.bBold = true;
            signalHeaderFont.bBold = true;

            // Set up the column header and the initial values for the column widths
            deviceHeaders = new string[] { "Name", "Correctness" };
            signalHeaders = new string[] { "Name", "Latched", "Unreasonable", "Total" };
            deviceColumnWidthMillimeters = deviceHeaders.Select(deviceHeaderFont.rGetTextWidthMM).ToArray();
            signalColumnWidthMillimeters = signalHeaders.Select(deviceHeaderFont.rGetTextWidthMM).ToArray();

            badDevices = m_deviceStatsList
                .Where(dev => dev.GetMeasurementsLatched(ReportDays - 1) + dev.GetMeasurementsUnreasonable(ReportDays - 1) > 0)
                .OrderByDescending(dev => (dev.GetMeasurementsLatched(ReportDays - 1) + dev.GetMeasurementsUnreasonable(ReportDays - 1)) / dev.MeasurementsExpected[ReportDays - 1])
                .ToList();

            deviceIndex = 0;
            deviceDetails = new string[badDevices.Count][];
            signalDetails = new string[badDevices.Count][][];

            foreach (DeviceStats deviceStats in badDevices)
            {
                deviceDetails[deviceIndex] = new string[2];
                deviceDetails[deviceIndex][0] = deviceStats.Name;
                deviceDetails[deviceIndex][1] = deviceStats.GetCorrectness(ReportDays - 1).ToString("0.##") + "%";
                deviceColumnWidthMillimeters = deviceColumnWidthMillimeters.Zip(deviceDetails[deviceIndex], (currentWidth, text) => Math.Max(currentWidth, deviceRowFont.rGetTextWidthMM(text))).ToArray();

                badSignals = deviceStats.SignalStatsLookup.Values
                    .Where(stats => stats.MeasurementsLatched[ReportDays - 1] + stats.MeasurementsUnreasonable[ReportDays - 1] > 0)
                    .OrderByDescending(stats => stats.MeasurementsLatched[ReportDays - 1] + stats.MeasurementsUnreasonable[ReportDays - 1])
                    .ToList();

                signalIndex = 0;
                signalDetails[deviceIndex] = new string[badSignals.Count][];

                foreach (SignalStats signalStats in badSignals)
                {
                    signalDetails[deviceIndex][signalIndex] = new string[4];
                    signalDetails[deviceIndex][signalIndex][0] = signalStats.Name;
                    signalDetails[deviceIndex][signalIndex][1] = signalStats.MeasurementsLatched[ReportDays - 1].ToString("#,##0");
                    signalDetails[deviceIndex][signalIndex][2] = signalStats.MeasurementsUnreasonable[ReportDays - 1].ToString("#,##0");
                    signalDetails[deviceIndex][signalIndex][3] = (signalStats.MeasurementsLatched[ReportDays - 1] + signalStats.MeasurementsUnreasonable[ReportDays - 1]).ToString("#,##0");
                    signalColumnWidthMillimeters = signalColumnWidthMillimeters.Zip(signalDetails[deviceIndex][signalIndex], (currentWidth, text) => Math.Max(currentWidth, signalRowFont.rGetTextWidthMM(text))).ToArray();

                    signalIndex++;
                }

                deviceIndex++;
            }

            // Determine the total width of the list so that it can be centered
            listWidthMillimeters = Math.Max(deviceColumnWidthMillimeters.Sum(width => width + ColumnPaddingMillimeters) - ColumnPaddingMillimeters, SignalIndentMillimeters + signalColumnWidthMillimeters.Sum(width => width + ColumnPaddingMillimeters) - ColumnPaddingMillimeters);

            for (deviceIndex = 0; deviceIndex < deviceDetails.Length; deviceIndex++)
            {
                // If the height of the device header, plus the device row, plus the signal header, plus one signal row reaches beyond the bottom of the page, start a new page
                if (verticalMillimeters + deviceHeaderFont.rSizeMM + deviceRowFont.rSizeMM + signalHeaderFont.rSizeMM + signalRowFont.rSizeMM + 4.0D * RowPaddingMillimeters > PageHeightMillimeters - PageMarginMillimeters - FooterHeightMillimeters - SpacingMillimeters)
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

                // Add the device header to the page
                horizontalMillimeters = (PageWidthMillimeters - listWidthMillimeters) / 2.0D;

                for (int i = 0; i < deviceHeaders.Length; i++)
                {
                    page.AddMM(horizontalMillimeters, verticalMillimeters + deviceHeaderFont.rSizeMM, new RepString(deviceHeaderFont, deviceHeaders[i]));
                    horizontalMillimeters += deviceColumnWidthMillimeters[i] + ColumnPaddingMillimeters;
                }

                verticalMillimeters += deviceHeaderFont.rSizeMM + RowPaddingMillimeters;

                // Add the device row to the page
                horizontalMillimeters = (PageWidthMillimeters - listWidthMillimeters) / 2.0D;

                for (int i = 0; i < deviceDetails[deviceIndex].Length; i++)
                {
                    page.AddMM(horizontalMillimeters, verticalMillimeters + deviceRowFont.rSizeMM, new RepString(deviceRowFont, deviceDetails[deviceIndex][i]));
                    horizontalMillimeters += deviceColumnWidthMillimeters[i] + ColumnPaddingMillimeters;
                }

                verticalMillimeters += deviceRowFont.rSizeMM + RowPaddingMillimeters * 2.0D;

                // Add the signal header to the page
                horizontalMillimeters = (PageWidthMillimeters - listWidthMillimeters) / 2.0D + SignalIndentMillimeters;

                for (int i = 0; i < signalHeaders.Length; i++)
                {
                    page.AddMM(horizontalMillimeters, verticalMillimeters + signalHeaderFont.rSizeMM, new RepString(signalHeaderFont, signalHeaders[i]));
                    horizontalMillimeters += signalColumnWidthMillimeters[i] + ColumnPaddingMillimeters;
                }

                verticalMillimeters += signalHeaderFont.rSizeMM + RowPaddingMillimeters;

                // Each signal is on its own row in the details list
                foreach (string[] row in signalDetails[deviceIndex])
                {
                    // If the height of the row reaches beyond the bottom of the page, start a new page
                    if (verticalMillimeters + signalRowFont.rSizeMM > PageHeightMillimeters - PageMarginMillimeters - FooterHeightMillimeters - SpacingMillimeters)
                    {
                        // Insert footer on the current page first
                        InsertFooter(fontDefinition, page, now, pageNumber);

                        // Increment the page number and create a new page
                        page = CreatePage(report);
                        pageNumber++;

                        // Add the report date to the top of the page
                        verticalMillimeters = PageMarginMillimeters;
                        verticalMillimeters += InsertReportDate(fontDefinition, page, PageMarginMillimeters) + SpacingMillimeters;

                        // Add the device header to the new page
                        horizontalMillimeters = (PageWidthMillimeters - listWidthMillimeters) / 2.0D;

                        for (int i = 0; i < deviceHeaders.Length; i++)
                        {
                            page.AddMM(horizontalMillimeters, verticalMillimeters + deviceHeaderFont.rSizeMM, new RepString(deviceHeaderFont, deviceHeaders[i]));
                            horizontalMillimeters += deviceColumnWidthMillimeters[i] + ColumnPaddingMillimeters;
                        }

                        verticalMillimeters += deviceHeaderFont.rSizeMM + RowPaddingMillimeters;

                        // Add the device row to the new page
                        horizontalMillimeters = (PageWidthMillimeters - listWidthMillimeters) / 2.0D;

                        for (int i = 0; i < deviceDetails[deviceIndex].Length; i++)
                        {
                            page.AddMM(horizontalMillimeters, verticalMillimeters + deviceRowFont.rSizeMM, new RepString(deviceRowFont, deviceDetails[deviceIndex][i]));
                            horizontalMillimeters += deviceColumnWidthMillimeters[i] + ColumnPaddingMillimeters;
                        }

                        verticalMillimeters += deviceRowFont.rSizeMM + RowPaddingMillimeters * 2.0D;

                        // Add the signal header to the new page
                        horizontalMillimeters = (PageWidthMillimeters - listWidthMillimeters) / 2.0D + SignalIndentMillimeters;

                        for (int i = 0; i < signalHeaders.Length; i++)
                        {
                            page.AddMM(horizontalMillimeters, verticalMillimeters + signalHeaderFont.rSizeMM, new RepString(signalHeaderFont, signalHeaders[i]));
                            horizontalMillimeters += signalColumnWidthMillimeters[i] + ColumnPaddingMillimeters;
                        }

                        verticalMillimeters += signalHeaderFont.rSizeMM + RowPaddingMillimeters;
                    }

                    // Add the signal row to the details list
                    horizontalMillimeters = (PageWidthMillimeters - listWidthMillimeters) / 2.0D + SignalIndentMillimeters;

                    for (int i = 0; i < row.Length; i++)
                    {
                        page.AddMM(horizontalMillimeters, verticalMillimeters + signalRowFont.rSizeMM, new RepString(signalRowFont, row[i]));
                        horizontalMillimeters += signalColumnWidthMillimeters[i] + ColumnPaddingMillimeters;
                    }

                    verticalMillimeters += signalRowFont.rSizeMM + RowPaddingMillimeters;
                }

                // Add space between devices
                verticalMillimeters += RowPaddingMillimeters * 3;
            }

            // Insert the footer on the last page of the report
            InsertFooter(fontDefinition, page, now, pageNumber);
        }

        private Chart GetBarChart()
        {
            Chart chart = new Chart();
            ChartArea area = new ChartArea();
            Series goodSeries = new Series();
            Series badSeries = new Series();

            double measurementsExpected;
            double measurementsReceived;
            double measurementsLatched;
            double measurementsUnreasonable;

            area.AxisX.MajorGrid.Enabled = false;
            area.AxisX.Interval = 1.0D;
            area.AxisX.IntervalType = DateTimeIntervalType.Days;
            area.AxisX.LabelAutoFitMinFontSize = 60;
            area.AxisX.LabelStyle.Format = "MM/dd";
            area.AxisX.LabelStyle.IsEndLabelVisible = false;
            area.AxisX.MajorGrid.Enabled = false;
            area.AxisY.Minimum = 0.0D;
            area.AxisY.Maximum = 100.0D;
            area.AxisY.LabelAutoFitMinFontSize = 35;
            area.AxisY.LabelStyle.Format = "{0}%";
            goodSeries.ChartType = SeriesChartType.StackedColumn;
            badSeries.ChartType = SeriesChartType.StackedColumn;

            for (int i = 0; i < Month; i++)
            {
                measurementsExpected = m_deviceStatsList.Sum(deviceStats => deviceStats.MeasurementsExpected[i]);
                measurementsReceived = m_deviceStatsList.Sum(deviceStats => deviceStats.MeasurementsReceived[i]);
                measurementsLatched = m_deviceStatsList.Sum(deviceStats => deviceStats.GetMeasurementsLatched(i));
                measurementsUnreasonable = m_deviceStatsList.Sum(deviceStats => deviceStats.GetMeasurementsUnreasonable(i));

                goodSeries.Points.AddXY((m_reportDate - TimeSpan.FromDays(Month - i - 1)), (measurementsReceived - measurementsLatched - measurementsUnreasonable) / measurementsExpected * 100.0D);
                badSeries.Points.AddXY((m_reportDate - TimeSpan.FromDays(Month - i - 1)), (measurementsLatched + measurementsUnreasonable) / measurementsExpected * 100.0D);
            }

            chart.Width = 2400;
            chart.Height = 1200;
            chart.ChartAreas.Add(area);
            chart.Series.Add(goodSeries);
            chart.Series.Add(badSeries);

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

        // Determines whether the given signal reference identifies
        // a device statistic that is desired for the report.
        private bool IsDesiredDeviceStat(string signalReference)
        {
            const string DeviceStatsPattern = @"!PMU-ST4$|!PMU-ST5$";

            return Regex.IsMatch(signalReference, DeviceStatsPattern) &&
                   (signalReference.StartsWith("LOCAL$", StringComparison.Ordinal) ||
                    signalReference.IndexOf("LOCAL$", StringComparison.Ordinal) < 0);
        }

        // Determines whether the given signal reference identifies
        // a signal statistic that is desired for the report.
        private bool IsDesiredSignalStat(string signalReference)
        {
            const string SignalStatsPattern = @"^(980|900)!.+!PT-ST[0-9]+$";
            return Regex.IsMatch(signalReference, SignalStatsPattern);
        }

        private string GetDeviceNameForStat(string signalReference)
        {
            return signalReference
                .Remove(signalReference.LastIndexOf("!PMU", StringComparison.Ordinal))
                .Replace("LOCAL$", "");
        }

        private string GetDeviceNameForSignal(string signalReference)
        {
            int index = signalReference.LastIndexOf('-');

            if (index > -1)
                return signalReference.Remove(index);

            return signalReference;
        }

        private string GetSignalName(string signalReference)
        {
            int start = signalReference.IndexOf('!') + 1;
            int end = signalReference.LastIndexOf('!');
            return signalReference.Substring(start, end - start);
        }

        #endregion
    }
}
