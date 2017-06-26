//******************************************************************************************************
//  MainWindow.xaml.cs - Gbtc
//
//  Copyright © 2012, Grid Protection Alliance.  All Rights Reserved.
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
//  08/10/2010 - Stephen C. Wills
//       Generated original version of source code.
//  12/18/2011 - J. Ritchie Carroll
//       Set likely default archive locations on initial startup and removed disabled points from the
//       display list.
//  09/29/2012 - J. Ritchie Carroll
//       Updated to code to use roll-over yielding ArchiveReader.
//  12/20/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml.Linq;
using GSF;
using GSF.COMTRADE;
using GSF.Historian;
using GSF.Historian.Files;
using GSF.IO;
using GSF.Units.EE;
using Microsoft.Win32;

namespace HistorianView
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        #region [ Members ]

        // Nested Types

        /// <summary>
        /// Defines a comparison class to property sort metadata.
        /// </summary>
        public class MetadataSorter : IComparer<MetadataWrapper>
        {
            /// <summary>
            /// Compares one metadata record to another.
            /// </summary>
            /// <param name="left">Left metadata record to compare.</param>
            /// <param name="right">Right metadata record to compare.</param>
            /// <returns>Comparison sort order of metadata record.</returns>
            public int Compare(MetadataWrapper left, MetadataWrapper right)
            {
                // Perform initial sort based on analogs followed by status flags, then digitals
                int result = ChannelMetadataSorter.Default.Compare(ConvertToChannelMetadata(left), ConvertToChannelMetadata(right));

                // Fall back on historian ID for secondary sort order
                if (result == 0)
                    result = left.PointNumber.CompareTo(right.PointNumber);

                return result;
            }

            /// <summary>
            /// Default instance of the metadata record sorter.
            /// </summary>
            public static readonly MetadataSorter Default = new MetadataSorter();
        }

        /// <summary>
        /// This is a wrapper around a MetadataRecord that allows auto-generation
        /// of columns in the data grid based on the public properties of this class.
        /// </summary>
        public class MetadataWrapper : INotifyPropertyChanged
        {
            #region [ Members ]

            // Events

            /// <summary>
            /// Event triggered when a property is changed.
            /// </summary>
            public event PropertyChangedEventHandler PropertyChanged;

            // Fields
            private readonly ArchiveReader m_archiveReader;
            private readonly MetadataRecord m_metadata;
            private bool m_export;

            #endregion

            #region [ Constructors ]

            /// <summary>
            /// Creates a new instance of the <see cref="MetadataWrapper"/> class.
            /// </summary>
            /// <param name="archiveReader">The <see cref="ArchiveReader"/> that the metadata record came from.</param>
            /// <param name="metadata">The <see cref="MetadataRecord"/> to be wrapped.</param>
            public MetadataWrapper(ArchiveReader archiveReader, MetadataRecord metadata)
            {
                metadata.Synonym2 = ValidateSynonym2(metadata.Synonym2);

                m_archiveReader = archiveReader;
                m_metadata = metadata;
            }

            #endregion

            #region [ Properties ]

            /// <summary>
            /// Determines whether the measurement represented by this metadata record
            /// should be exported to CSV or displayed on the graph by the Historian Data Viewer.
            /// </summary>
            public bool Export
            {
                get
                {
                    return m_export;
                }
                set
                {
                    m_export = value;
                    OnPropertyChanged("Export");
                }
            }

            /// <summary>
            /// Gets the point number of the measurement.
            /// </summary>
            public int PointNumber
            {
                get
                {
                    return m_metadata.HistorianID;
                }
            }

            /// <summary>
            /// Gets the name of the measurement.
            /// </summary>
            public string Name
            {
                get
                {
                    // This formats name in accordance with COMTRADE standard Annex H (may need to make this optional)
                    return FormatName(m_metadata.Name, m_metadata.Synonym1, m_metadata.Synonym2);
                }
            }

            /// <summary>
            /// Gets the point tag of the measurement.
            /// </summary>
            public string PointTag
            {
                get
                {
                    return m_metadata.Name;
                }
            }

            /// <summary>
            /// Gets the description of the measurement.
            /// </summary>
            public string Description
            {
                get
                {
                    return m_metadata.Description;
                }
            }

            /// <summary>
            /// Gets the first alternate name for the measurement.
            /// </summary>
            public string Synonym1
            {
                get
                {
                    return m_metadata.Synonym1;
                }
            }

            /// <summary>
            /// Gets the second alternate name for the measurement.
            /// </summary>
            public string Synonym2
            {
                get
                {
                    return m_metadata.Synonym2;
                }
            }

            /// <summary>
            /// Gets the third alternate name for the measurement.
            /// </summary>
            public string Synonym3
            {
                get
                {
                    return m_metadata.Synonym3;
                }
            }

            /// <summary>
            /// Gets the system name.
            /// </summary>
            public string System
            {
                get
                {
                    return m_metadata.SystemName;
                }
            }

            /// <summary>
            /// Gets the low range of the measurement.
            /// </summary>
            public Single LowRange
            {
                get
                {
                    return m_metadata.Summary.LowRange;
                }
            }

            /// <summary>
            /// Gets the high range of the measurement.
            /// </summary>
            public Single HighRange
            {
                get
                {
                    return m_metadata.Summary.HighRange;
                }
            }

            /// <summary>
            /// Gets the engineering units used to measure the values.
            /// </summary>
            public string EngineeringUnits
            {
                get
                {
                    return m_metadata.AnalogFields.EngineeringUnits;
                }
            }

            /// <summary>
            /// Gets the unit number of the measurement.
            /// </summary>
            public int Unit
            {
                get
                {
                    return m_metadata.UnitNumber;
                }
            }

            #endregion

            #region [ Methods ]

            /// <summary>
            /// Returns the <see cref="ArchiveReader"/> that the metadata record came from.
            /// </summary>
            /// <returns>The archive reader.</returns>
            public ArchiveReader GetArchiveReader()
            {
                return m_archiveReader;
            }

            /// <summary>
            /// Returns the wrapped <see cref="MetadataRecord"/>.
            /// </summary>
            /// <returns>The wrapped metadata record.</returns>
            public MetadataRecord GetMetadata()
            {
                return m_metadata;
            }

            // Triggers the PropertyChanged event.
            private void OnPropertyChanged(string propertyName)
            {
                if ((object)PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }

            private string ValidateSynonym2(string synonym2)
            {
                return !string.IsNullOrEmpty(synonym2) ? synonym2.Trim() : "ALOG";
            }

            private string FormatName(string name, string synonym1, string synonym2)
            {
                string[] parts;
                int lastIndexOf;

                if (string.IsNullOrEmpty(name))
                    return name;

                parts = name.Split(':');

                if (parts.Length > 1)
                {
                    // Separate name from point tag suffix
                    name = parts[0];

                    switch (synonym2)
                    {
                        case "FLAG":
                            return name;

                        case "DIGI":
                        case "ALOG":
                        case "STAT":
                        case "ALRM":
                            lastIndexOf = synonym1.LastIndexOf('-');

                            if (lastIndexOf > 0)
                                return name + ':' + synonym1.Substring(lastIndexOf + 1);

                            return name + ':' + synonym2;

                        default:
                            lastIndexOf = name.LastIndexOf('-');

                            if (lastIndexOf > 0)
                                return name.Substring(0, lastIndexOf) + ':' + name.Substring(lastIndexOf + 1);

                            return name + ':' + synonym2;
                    }
                }

                return name + ':' + synonym2;
            }

            #endregion
        }

        // Fields
        private readonly ICollection<ArchiveReader> m_archiveReaders;
        private readonly List<MetadataWrapper> m_metadata;
        private string[] m_tokens;

        private string m_currentSessionPath;

        private DateTime m_startTime;
        private DateTime m_endTime;
        private bool m_showDisabledPoints;

        private FileType m_exportFileType;
        private int m_exportFrameRate;
        private double m_exportNominalFrequency;
        private bool m_alignTimestampsInExport;

        private readonly ICollection<MenuItem> m_contextMenuItems;
        private readonly ICollection<string> m_visibleColumns;
        private ChartWindow m_chartWindow;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new instance of the <see cref="MainWindow"/> class.
        /// </summary>
        public MainWindow()
        {
            m_archiveReaders = new List<ArchiveReader>();
            m_metadata = new List<MetadataWrapper>();
            m_contextMenuItems = new List<MenuItem>();
            m_visibleColumns = new HashSet<string>();

            InitializeComponent();
            InitializeChartWindow();

            m_exportFrameRate = 30;
            m_exportNominalFrequency = 60.0D;
            m_alignTimestampsInExport = true;

            m_currentTimeCheckBox.IsChecked = true;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets a boolean that determines whether an archive is open or not.
        /// </summary>
        public bool ArchiveIsOpen
        {
            get
            {
                return m_saveButton.IsEnabled;
            }
            set
            {
                m_saveButton.IsEnabled = value;
                m_saveMenuItem.IsEnabled = value;
                m_saveAsMenuItem.IsEnabled = value;

                m_trendButton.IsEnabled = value;
                m_trendMenuItem.IsEnabled = value;

                m_exportButton.IsEnabled = value;
                m_exportMenuItem.IsEnabled = value;
                ShowDisabledMenuItem.IsEnabled = value;
            }
        }

        /// <summary>
        /// Gets or sets the start time to be displayed on the graph or exported to CSV.
        /// </summary>
        public DateTime StartTime
        {
            get
            {
                return m_startTime;
            }
            set
            {
                m_startTime = value;
                m_startTimeDatePicker.SelectedDate = value;
                m_startTimeTextBox.Text = value.ToString("HH:mm:ss.fff");
            }
        }

        /// <summary>
        /// Gets or sets the end time to be displayed on the graph or exported to CSV.
        /// </summary>
        public DateTime EndTime
        {
            get
            {
                return m_endTime;
            }
            set
            {
                m_endTime = value;
                m_endTimeDatePicker.SelectedDate = value;
                m_endTimeTextBox.Text = value.ToString("HH:mm:ss.fff");
            }
        }

        #endregion

        #region [ Methods ]

        // Initializes the chart window.
        private void InitializeChartWindow()
        {
            if (m_chartWindow == null)
            {
                m_chartWindow = new ChartWindow();
                m_chartWindow.ChartUpdated += ChartWindow_ChartUpdated;
                m_chartWindow.Closing += ChildWindow_Closing;
                TrySetChartInterval(null);
            }
        }

        // Gets a string representation of the start time to be used when reading from the archive.
        private string GetStartTime()
        {
            StringBuilder startTimeBuilder = new StringBuilder();

            if (!m_currentTimeCheckBox.IsChecked.GetValueOrDefault())
            {
                startTimeBuilder.Append(m_startTime.ToString("MM/dd/yyyy HH:mm:ss.fff", CultureInfo.InvariantCulture));
            }
            else
            {
                startTimeBuilder.Append("*-");
                startTimeBuilder.Append(m_currentTimeTextBox.Text);
                startTimeBuilder.Append(m_currentTimeComboBox.SelectionBoxItem.ToString()[0]);
            }

            return startTimeBuilder.ToString();
        }

        // Gets a string representation of the end time to be used when reading from the archive.
        private string GetEndTime()
        {
            if (m_currentTimeCheckBox.IsChecked.GetValueOrDefault())
                return "*";

            return m_endTime.ToString("MM/dd/yyyy HH:mm:ss.fff", CultureInfo.InvariantCulture);
        }

        private void OpenArchives(IEnumerable<string> archiveLocations)
        {
            ArchiveReader archiveReader;

            ClearArchives();

            foreach (string archivelocation in archiveLocations)
            {
                archiveReader = OpenArchiveReader(archivelocation);

                if ((object)archiveReader != null)
                    m_archiveReaders.Add(archiveReader);
            }

            foreach (ArchiveReader reader in m_archiveReaders)
            {
                foreach (MetadataRecord record in reader.MetadataFile.Read())
                {
                    m_metadata.Add(new MetadataWrapper(reader, record));
                }
            }

            ArchiveIsOpen = m_archiveReaders.Any();
            FilterBySearchResults();

            m_chartWindow.VisiblePoints.Clear();
            TrySetChartInterval(null);
            m_chartWindow.UpdateChart();
        }

        private void ReopenArchives()
        {
            foreach (ArchiveReader reader in m_archiveReaders)
            {
                reader.Open(reader.FileName, reader.ArchiveOffloadLocation);
            }
        }

        // Prompts the user to open one or more archive files.
        private void ShowOpenArchiveFileDialog()
        {
            OpenArchivesDialog dialog = new OpenArchivesDialog();

            dialog.ArchiveLocations = m_archiveReaders
                .Select(reader => string.Format("{0}|{1}", GetArchiveLocation(reader), reader.ArchiveOffloadLocation))
                .ToList();

            dialog.Owner = this;
            dialog.ShowDialog();
            OpenArchives(dialog.ArchiveLocations);
        }

        // Prompts the user to open a previously saved session file.
        private void ShowOpenSessionDialog()
        {
            OpenFileDialog openSessionDialog = new OpenFileDialog();

            openSessionDialog.Filter = "Historian Data Viewer files|*.hdv";
            openSessionDialog.CheckFileExists = true;

            if (openSessionDialog.ShowDialog() == true)
                OpenSessionFile(openSessionDialog.FileName, true);
        }

        // Prompts the user for the location to save the current session.
        private void ShowSaveSessionDialog()
        {
            const string ErrorMessage = "Unable to save current time interval. Please enter a start time that is less than or equal to the end time.";

            if (TrySetChartInterval(ErrorMessage))
            {
                SaveFileDialog saveSessionDialog = new SaveFileDialog();

                saveSessionDialog.Filter = "Historian Data Viewer files|*.hdv";
                saveSessionDialog.DefaultExt = "hdv";
                saveSessionDialog.AddExtension = true;
                saveSessionDialog.CheckPathExists = true;

                if (saveSessionDialog.ShowDialog() == true)
                    SaveSession(saveSessionDialog.FileName, true);

                Focus();
            }
        }

        // Opens an archive reader and returns the ArchiveReader object.
        private ArchiveReader OpenArchiveReader(string archiveLocation)
        {
            string[] paths = archiveLocation.Split('|');
            string archiveFilePath;
            ArchiveReader file;

            if (paths[0].EndsWith("_archive.d"))
                archiveFilePath = paths[0];
            else if (paths[0].EndsWith("_dbase.dat", StringComparison.OrdinalIgnoreCase))
                archiveFilePath = string.Format("{0}_archive.d", paths[0].Remove(paths[0].Length - 10));
            else if (paths[0].EndsWith("_dbase.dat2", StringComparison.OrdinalIgnoreCase))
                archiveFilePath = string.Format("{0}_archive.d", paths[0].Remove(paths[0].Length - 11));
            else
                return null;

            file = new ArchiveReader();

            if (paths.Length > 1)
                file.Open(archiveFilePath, paths[1]);
            else
                file.Open(archiveFilePath);

            return file;
        }

        // Opens a previously saved session file.
        private void OpenSessionFile(string filePath, bool setAsCurrentSession)
        {
            IEnumerable<MetadataWrapper> wrappers;
            IEnumerable<string> archiveLocations;

            XDocument document;
            XElement root;

            string value;
            FileType fileTypeValue;
            DateTime dateTimeValue;
            int intValue;
            double doubleValue;

            // If the hdv file doesn't exist, there's nothing to do
            if (!File.Exists(filePath))
                return;

            try
            {
                // Load the hdv file as an XML document
                document = XDocument.Load(filePath);
                root = document.Root;
            }
            catch
            {
                MessageBox.Show($"{filePath} is not a valid session file.");
                return;
            }

            if (root == null)
                return;

            // Determine the values of start time, end time, chart resolution, and show disabled points
            value = (string)root.Attribute("startTime") ?? string.Empty;

            if (DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out dateTimeValue))
            {
                m_currentTimeCheckBox.IsChecked = false;

                StartTime = dateTimeValue;

                if (DateTime.TryParse((string)root.Attribute("endTime"), CultureInfo.InvariantCulture, DateTimeStyles.None, out dateTimeValue))
                    EndTime = dateTimeValue;
                else
                    EndTime = StartTime + TimeSpan.FromMinutes(5.0D);
            }
            else if (Regex.IsMatch(value, @"\*-\d+[smhd]"))
            {
                m_currentTimeCheckBox.IsChecked = true;
                m_currentTimeTextBox.Text = value.Substring(2, value.Length - 3);

                m_currentTimeComboBox.SelectedItem = m_currentTimeComboBox.Items
                    .Cast<ComboBoxItem>()
                    .Where(item => item.Content.ToString().StartsWith(value[value.Length - 1].ToString()))
                    .DefaultIfEmpty(m_currentTimeComboBox.Items[1])
                    .First();
            }

            m_chartResolutionTextBox.Text = (string)root.Attribute("chartResolution") ?? m_chartResolutionTextBox.Text;
            m_showDisabledPoints = ((string)root.Attribute("showDisabledPoints") ?? string.Empty).ParseBoolean();
            ShowDisabledCheckMark.Visibility = m_showDisabledPoints ? Visibility.Visible : Visibility.Collapsed;

            // Get the export settings for the session
            foreach (XAttribute exportAttribute in root.Elements("export").Attributes())
            {
                switch (exportAttribute.Name.LocalName)
                {
                    case "fileType":
                        if (Enum.TryParse((string)exportAttribute, out fileTypeValue))
                            m_exportFileType = fileTypeValue;

                        break;

                    case "frameRate":
                        if (int.TryParse((string)exportAttribute, out intValue))
                            m_exportFrameRate = intValue;

                        break;

                    case "nominalFrequency":
                        if (double.TryParse((string)exportAttribute, out doubleValue))
                            m_exportNominalFrequency = doubleValue;

                        break;

                    case "alignTimestamps":
                        m_alignTimestampsInExport = ((string)exportAttribute).ToNonNullString().ParseBoolean();
                        break;
                }
            }

            // Determine which archives to open based on the archive tags in the hdv file
            archiveLocations = root.Elements("archive")
                .Where(element => (object)element.Attribute("path") != null)
                .Select(element => string.Format("{0}|{1}", (string)element.Attribute("path"), (string)element.Attribute("offloadLocation") ?? string.Empty))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (!archiveLocations.Any())
            {
                // Fall back on the archivePath attribute of the metadata tags if no valid archive tags are specified
                archiveLocations = root.Elements("metadata").Attributes("archivePath")
                    .Select(attribute => (string)attribute)
                    .Where(location => (object)location != null)
                    .Distinct(StringComparer.OrdinalIgnoreCase);
            }

            // Open the archives found in the hdv file
            OpenArchives(archiveLocations);

            // Determine which points to select using the metadata tags in the hdv file
            wrappers = root
                .Elements("metadata")
                .Select(element => m_metadata.FirstOrDefault(wrapper => (string)element.Attribute("archivePath") == wrapper.GetArchiveReader().FileName && (string)element.Attribute("historianId") == wrapper.GetMetadata().HistorianID.ToString()))
                .Where(wrapper => (object)wrapper != null);

            foreach (MetadataWrapper wrapper in wrappers)
                wrapper.Export = true;

            FilterBySearchResults();

            if (setAsCurrentSession)
            {
                // Set the current session path and the title
                // of the window to reflect the current session
                m_currentSessionPath = filePath;
                Title = Path.GetFileName(filePath) + " - Historian Data Viewer";
            }

            // Update chart based on the selected points in the newly opened session
            m_chartWindow.VisiblePoints = m_metadata
                .Where(wrapper => wrapper.Export)
                .Select(wrapper => Tuple.Create(wrapper.GetArchiveReader(), wrapper.GetMetadata()))
                .ToList();

            TrySetChartInterval(null);
            m_chartWindow.UpdateChart();
        }

        // Saves the current session file with the current session's file path
        // or prompts the user if the current file path has not been set.
        private void SaveCurrentSession()
        {
            const string ErrorMessage = "Unable to save current time interval. Please enter a start time that is less than or equal to the end time.";

            if (m_currentSessionPath == null)
            {
                ShowSaveSessionDialog();
            }
            else if (TrySetChartInterval(ErrorMessage))
            {
                SaveSession(m_currentSessionPath, true);
            }
        }

        // Saves the current session to a session file.
        private void SaveSession(string filePath, bool setAsCurrentSession)
        {
            XDocument document;
            XElement root;

            // Save general settings and export settings
            root = new XElement("historianDataViewer",
                new XAttribute("startTime", GetStartTime()),
                new XAttribute("endTime", GetEndTime()),
                new XAttribute("chartResolution", m_chartResolutionTextBox.Text),
                new XAttribute("showDisabledPoints", m_showDisabledPoints),

                new XElement("export",
                    new XAttribute("fileType", m_exportFileType),
                    new XAttribute("frameRate", m_exportFrameRate),
                    new XAttribute("nominalFrequency", m_exportNominalFrequency),
                    new XAttribute("alignTimestamps", m_alignTimestampsInExport)));

            // Save information about the currently open archives
            foreach (ArchiveReader archiveReader in m_archiveReaders)
            {
                root.Add(new XElement("archive",
                    new XAttribute("path", archiveReader.FileName),
                    new XAttribute("offloadLocation", archiveReader.ArchiveOffloadLocation)));
            }

            // Save information about the currently selected points
            foreach (MetadataWrapper wrapper in m_metadata.Where(m => m.Export))
            {
                root.Add(new XElement("metadata",
                    new XAttribute("archivePath", wrapper.GetArchiveReader().FileName),
                    new XAttribute("historianId", wrapper.GetMetadata().HistorianID)));
            }

            // Save the document to the disk
            document = new XDocument(root);
            document.Save(filePath);

            if (setAsCurrentSession)
            {
                // Set the current session path and the title
                // of the window to reflect the current session
                m_currentSessionPath = filePath;
                Title = Path.GetFileName(filePath) + " - Historian Data Viewer";
            }
        }

        // Creates an item for the data grid header area's context menu.
        private MenuItem CreateContextMenuItem(string header, bool isChecked)
        {
            MenuItem item = new MenuItem();

            item.Header = header;
            item.IsCheckable = true;
            item.IsChecked = isChecked;
            item.Click += ColumnContextMenuItem_Click;

            return item;
        }

        // Converts an automatically generated data grid header to a more human readable header.
        private string ToProperHeader(string header)
        {
            if (header == "PointNumber")
                return "Pt.#";

            StringBuilder properHeader = new StringBuilder();

            foreach (char c in header)
            {
                if (properHeader.Length != 0 && (char.IsUpper(c) || char.IsDigit(c)))
                    properHeader.Append(' ');

                properHeader.Append(c);
            }

            return properHeader.ToString();
        }

        // Preemptively updates the corresponding item's property when a data grid check box is checked or unchecked.
        private void DataGridCheckBoxCheckedChanged(DataGridCell cell, bool isChecked)
        {
            DataGridCellInfo cellInfo = new DataGridCellInfo(cell);
            string currentColumnHeader = cellInfo.Column.Header.ToString();
            PropertyInfo property = cellInfo.Item.GetType().GetProperties().Single(propertyInfo => propertyInfo.Name == currentColumnHeader);
            property.SetValue(cellInfo.Item, isChecked, null);
        }

        // Filters the data grid rows by the results of the user's search.
        //The earlier version of this function produces a output grid with spaces whenever a record was empty
        //These empty records are filtered by removing the records which do not have a measurement name.
        //The records are also arranged in Ascending order. 
        private void FilterBySearchResults()
        {
            m_tokens = m_searchBox.Text.Split(' ');

            m_dataGrid.ItemsSource = m_metadata.Where(metadata => !string.IsNullOrEmpty(metadata.Name) && (m_showDisabledPoints || metadata.GetMetadata().GeneralFlags.Enabled))
                .Select(metadata => new Tuple<MetadataWrapper, bool>(metadata, SearchProperties(metadata, m_tokens)))
                .Where(tuple => tuple.Item1.Export || tuple.Item2)
                .OrderByDescending(tuple => tuple.Item2)
                .ThenBy(tuple => tuple.Item1.Export)
                .ThenBy(tuple => tuple.Item1.PointNumber)
                .Select(tuple => tuple.Item1)
                .ToList();
        }

        // Selects or deselects the export field for all records in the data grid.
        private void SetExportForAll(bool selected)
        {
            m_dataGrid.ItemsSource.Cast<MetadataWrapper>().ToList()
                .ForEach(metadata => metadata.Export = selected);
        }

        // Searches the non-boolean properties of an object to determine if their string representations collectively contain a set of tokens.
        private bool SearchProperties(object obj, IEnumerable<string> tokens)
        {
            IEnumerable<PropertyInfo> properties = obj.GetType().GetProperties().Where(property => property.PropertyType != typeof(bool));
            IEnumerable<string> propertyValues = properties.Select(property => property.GetValue(obj, null).ToString());
            return tokens.All(token => propertyValues.Any(propertyValue => propertyValue.ToLower().Contains(token.ToLower())));
        }

        // Displays the chart window.
        private void TrendRequested()
        {
            const string ChartIntervalErrorMessage = "Unable to set x-axis boundaries for the chart. Please enter a start time that is less than or equal to the end time.";

            if (!TrySetChartInterval(ChartIntervalErrorMessage))
                return;

            SetChartResolution();

            ReopenArchives();

            m_chartWindow.VisiblePoints = m_metadata
                .Where(wrapper => wrapper.Export)
                .Select(wrapper => Tuple.Create(wrapper.GetArchiveReader(), wrapper.GetMetadata()))
                .ToList();

            m_chartWindow.UpdateChart();
            m_chartWindow.Show();
            m_chartWindow.Focus();
        }

        // Exports measurements to a CSV or COMTRADE file.
        private void ExportRequested()
        {
            ExportDialog dialog = new ExportDialog();
            string exportFileName;

            dialog.Owner = this;
            dialog.FileType = m_exportFileType;
            dialog.FrameRate = m_exportFrameRate;
            dialog.NominalFrequency = m_exportNominalFrequency;
            dialog.AlignTimestamps = m_alignTimestampsInExport;

            if (dialog.ShowDialog() == true)
            {
                exportFileName = GetExportFilePath(dialog.FileType);

                if ((object)exportFileName != null)
                {
                    StreamWriter dataFileWriter = null;
                    StreamWriter configFileWriter = null;

                    m_exportFileType = dialog.FileType;
                    m_exportFrameRate = dialog.FrameRate;
                    m_exportNominalFrequency = dialog.NominalFrequency;
                    m_alignTimestampsInExport = dialog.AlignTimestamps;

                    try
                    {
                        int index = 0;

                        ReopenArchives();

                        Dictionary<MetadataWrapper, ArchiveReader> metadata = m_metadata
                            .Where(wrapper => wrapper.Export)
                            .OrderBy(wrapper => wrapper, MetadataSorter.Default)
                            .ToDictionary(
                                wrapper => wrapper,
                                wrapper => wrapper.GetArchiveReader()
                            );

                        Dictionary<TimeTag, List<string[]>> data = new Dictionary<TimeTag, List<string[]>>();
                        TimeTag startTime = TimeTag.Parse(GetStartTime());
                        TimeTag endTime = TimeTag.Parse(GetEndTime());

                        List<double> averages = new List<double>();
                        List<double> maximums = new List<double>();
                        List<double> minimums = new List<double>();

                        if (m_alignTimestampsInExport)
                            PopulateWithAlignedTimestamps(data, startTime, endTime, m_exportFrameRate, metadata.Count);

                        foreach (MetadataWrapper wrapper in metadata.Keys)
                        {
                            double min = double.NaN;
                            double max = double.NaN;
                            double total = 0.0;
                            int count = 0;

                            foreach (IDataPoint point in metadata[wrapper].ReadData(wrapper.GetMetadata().HistorianID, startTime, endTime))
                            {
                                TimeTag time = point.Time;
                                List<string[]> rowList;
                                string[] row;
                                int rowIndex;

                                if (m_alignTimestampsInExport)
                                    time = new TimeTag(new DateTime(Ticks.RoundToSubsecondDistribution(point.Time.ToDateTime().Ticks, m_exportFrameRate)));

                                // Get or create the list of rows for the timetag of the current point.
                                if (!data.TryGetValue(time, out rowList))
                                {
                                    rowList = new List<string[]>();
                                    data.Add(time, rowList);
                                }

                                // Attempt to add the value of the current point to an existing list.
                                for (rowIndex = 0; rowIndex < rowList.Count; rowIndex++)
                                {
                                    row = rowList[rowIndex];

                                    // When aligning timestamps, ensure that multiple
                                    // rows are not added for duplicate timestamps
                                    if (m_alignTimestampsInExport || row[index] == null)
                                    {
                                        row[index] = (m_exportFileType == FileType.ComtradeAscii || m_exportFileType == FileType.ComtradeBinary)
                                            ? point.Value.ToString(CultureInfo.InvariantCulture)
                                            : point.Value.ToString();

                                        break;
                                    }
                                }

                                // If all rows were already occupied with
                                // a value for this point, add a new row.
                                if (rowIndex >= rowList.Count)
                                {
                                    row = new string[metadata.Count];
                                    row[index] = point.Value.ToString();
                                    rowList.Add(row);
                                }

                                // Determine if this is the new minimum value.
                                if (!(point.Value >= min))
                                    min = point.Value;

                                // Determine if this is the new maximum value.
                                if (!(point.Value <= max))
                                    max = point.Value;

                                // Increase total and count for average calculation.
                                total += point.Value;
                                count++;
                            }

                            averages.Add(total / count);
                            maximums.Add(max);
                            minimums.Add(min);

                            index++;
                        }

                        if (m_exportFileType == FileType.ComtradeAscii || m_exportFileType == FileType.ComtradeBinary)
                        {
                            // COMTRADE Export
                            string rootFileName = FilePath.GetDirectoryName(exportFileName) + FilePath.GetFileNameWithoutExtension(exportFileName);
                            string configFileName = rootFileName + ".cfg";
                            string dataFileName = rootFileName + ".dat";
                            bool isBinary = m_exportFileType == FileType.ComtradeBinary;

                            configFileWriter = new StreamWriter(new FileStream(configFileName, FileMode.Create, FileAccess.Write), Encoding.ASCII);
                            Schema schema = WriteComtradeConfigFile(configFileWriter, metadata, data.Count, data.Keys.Min().ToDateTime().Ticks, isBinary);

                            dataFileWriter = new StreamWriter(new FileStream(dataFileName, FileMode.Create, FileAccess.Write), Encoding.ASCII);
                            WriteComtradeDataFile(dataFileWriter, schema, data, isBinary);
                        }
                        else
                        {
                            // CSV Export
                            dataFileWriter = new StreamWriter(new FileStream(exportFileName, FileMode.Create, FileAccess.Write));
                            WriteCsvDataFile(dataFileWriter, metadata, data, averages, maximums, minimums);
                        }
                    }
                    finally
                    {
                        if ((object)dataFileWriter != null)
                            dataFileWriter.Close();

                        if ((object)configFileWriter != null)
                            configFileWriter.Close();
                    }
                }
            }
        }

        private void PopulateWithAlignedTimestamps(Dictionary<TimeTag, List<string[]>> data, TimeTag startTime, TimeTag endTime, int framesPerSecond, int dataColumns)
        {
            Ticks[] subsecondDistribution = Ticks.SubsecondDistribution(framesPerSecond);
            Ticks startTimeTicks = startTime.ToDateTime().Ticks;
            Ticks endTimeTicks = endTime.ToDateTime().Ticks;

            Ticks currentSecond = startTimeTicks.BaselinedTimestamp(BaselineTimeInterval.Second);
            Ticks[] currentSecondDistribution = subsecondDistribution.Select(time => currentSecond + time).ToArray();

            while (currentSecond <= endTimeTicks)
            {
                foreach (Ticks timestamp in currentSecondDistribution)
                {
                    if (timestamp > endTimeTicks)
                        break;

                    if (timestamp >= startTimeTicks)
                        data.Add(new TimeTag(new DateTime(timestamp)), new List<string[]>() { new string[dataColumns] });
                }

                currentSecond += Ticks.PerSecond;
                currentSecondDistribution = subsecondDistribution.Select(time => currentSecond + time).ToArray();
            }
        }

        private Schema WriteComtradeConfigFile(StreamWriter configFileWriter, Dictionary<MetadataWrapper, ArchiveReader> metadata, int sampleCount, Ticks startTime, bool isBinary)
        {
            // Convert openHistorian metadata to COMTRADE channel metadata
            IEnumerable<ChannelMetadata> channelMetadata = metadata.Keys
                .Select(ConvertToChannelMetadata);

            // Create new COMTRADE configuration schema
            Schema schema = Writer.CreateSchema(
                channelMetadata,
                "openHistorian Export",
                "Source=" + m_archiveReaders.First().FileName.Replace(',', '_'),
                startTime,
                sampleCount,
                isBinary,
                1.0D,
                m_exportFrameRate,
                m_exportNominalFrequency);

            // Write new schema file
            configFileWriter.Write(schema.FileImage);

            return schema;
        }

        private void WriteComtradeDataFile(StreamWriter dataFileWriter, Schema schema, Dictionary<TimeTag, List<string[]>> data, bool isBinary)
        {
            FileStream dataFileStream = (FileStream)dataFileWriter.BaseStream;
            uint sample = 0;

            if (isBinary)
            {
                foreach (KeyValuePair<TimeTag, List<string[]>> pair in data.OrderBy(p => p.Key))
                {
                    // It is expected that the normal case is that there will only be one row per timestamp, however,
                    // for the historian it is possible to have multiple records at the same timestamp - this can happen
                    // when there is a leap-second and the exact same second repeats. At least this way the data will be
                    // exported - the end user will need to cipher out which rows come first based on the data.
                    foreach (string[] row in pair.Value)
                    {
                        Writer.WriteNextRecordBinary(dataFileStream, schema, pair.Key.ToDateTime().Ticks, row.Select(value => double.Parse(value ?? double.NaN.ToString(CultureInfo.InvariantCulture), CultureInfo.InvariantCulture)).ToArray(), sample++);
                    }
                }
            }
            else
            {
                foreach (KeyValuePair<TimeTag, List<string[]>> pair in data.OrderBy(p => p.Key))
                {
                    // It is expected that the normal case is that there will only be one row per timestamp, however,
                    // for the historian it is possible to have multiple records at the same timestamp - this can happen
                    // when there is a leap-second and the exact same second repeats. At least this way the data will be
                    // exported - the end user will need to cipher out which rows come first based on the data.
                    foreach (string[] row in pair.Value)
                    {
                        Writer.WriteNextRecordAscii(dataFileWriter, schema, pair.Key.ToDateTime().Ticks, row.Select(value => double.Parse(value ?? double.NaN.ToString(CultureInfo.InvariantCulture), CultureInfo.InvariantCulture)).ToArray(), sample++);
                    }
                }

                // Write EOF marker
                dataFileWriter.Flush();
                dataFileStream.WriteByte(0x1A);
            }
        }

        private void WriteCsvDataFile(StreamWriter dataFileWriter, Dictionary<MetadataWrapper, ArchiveReader> metadata, Dictionary<TimeTag, List<string[]>> data, IEnumerable<double> averages, IEnumerable<double> maximums, IEnumerable<double> minimums)
        {
            StringBuilder line = new StringBuilder();

            // Write time interval to the CSV file.
            dataFileWriter.Write("Historian Data Viewer Export: ");
            dataFileWriter.Write(GetStartTime());
            dataFileWriter.Write(" to ");
            dataFileWriter.Write(GetEndTime());
            dataFileWriter.WriteLine();
            dataFileWriter.WriteLine();

            // Write average, min, and max for each measurement to the CSV file.
            line.Append("Average,");
            foreach (double average in averages)
            {
                line.Append(ToCsvValue(average.ToString()));
                line.Append(',');
            }
            line.Remove(line.Length - 1, 1);
            dataFileWriter.WriteLine(line.ToString());
            line.Clear();

            line.Append("Maximum,");
            foreach (double max in maximums)
            {
                line.Append(ToCsvValue(max.ToString()));
                line.Append(',');
            }
            line.Remove(line.Length - 1, 1);
            dataFileWriter.WriteLine(line.ToString());
            line.Clear();

            line.Append("Minimum,");
            foreach (double min in minimums)
            {
                line.Append(ToCsvValue(min.ToString()));
                line.Append(',');
            }
            line.Remove(line.Length - 1, 1);
            dataFileWriter.WriteLine(line.ToString());
            line.Clear();

            // Write header for the data points to the CSV file.
            dataFileWriter.WriteLine();
            line.Append("Time,");

            foreach (MetadataRecord record in metadata.Keys.Select(wrapper => wrapper.GetMetadata()))
            {
                line.Append(ToCsvValue($"{record.Name} {record.Description.Replace(',', '-')}"));
                line.Append(',');
            }

            line.Remove(line.Length - 1, 1);
            dataFileWriter.WriteLine(line.ToString());
            line.Clear();

            // Write data to the CSV file.
            foreach (KeyValuePair<TimeTag, List<string[]>> pair in data.OrderBy(p => p.Key))
            {
                TimeTag time = pair.Key;

                // It is expected that the normal case is that there will only be one row per timestamp, however,
                // for the historian it is possible to have multiple records at the same timestamp - this can happen
                // when there is a leap-second and the exact same second repeats. At least this way the data will be
                // exported - the end user will need to cipher out which rows come first based on the data.
                foreach (string[] row in pair.Value)
                {
                    line.Append(ToCsvValue(time.ToString()));
                    line.Append(',');

                    foreach (string value in row)
                    {
                        line.Append(ToCsvValue(value ?? double.NaN.ToString()));
                        line.Append(',');
                    }

                    line.Remove(line.Length - 1, 1);
                    dataFileWriter.WriteLine(line.ToString());
                    line.Clear();
                }
            }
        }

        // Wraps the value in quotes if it contains a comma.
        private string ToCsvValue(string value)
        {
            if (value.Contains(','))
                return value.QuoteWrap();

            return value;
        }

        // Sets the x-axis boundaries of the chart based on the current start time and end time.
        private bool TrySetChartInterval(string errorMessage)
        {
            try
            {
                bool canSetInterval = m_currentTimeCheckBox.IsChecked.GetValueOrDefault() || m_startTime < m_endTime;

                if (canSetInterval)
                    m_chartWindow.SetInterval(GetStartTime(), GetEndTime());
                else if (errorMessage != null)
                    MessageBox.Show(errorMessage);

                return canSetInterval;
            }
            catch (OverflowException)
            {
                m_currentTimeTextBox.Text = int.MaxValue.ToString();
                return TrySetChartInterval(errorMessage);
            }
            catch (ArgumentOutOfRangeException)
            {
                m_chartWindow.SetInterval(new DateTime().ToString(), DateTime.Now.ToString());
                return true;
            }
        }

        // Sets the chart resolution or displays an error message if user input is invalid.
        private void SetChartResolution()
        {
            int resolution;

            if (int.TryParse(m_chartResolutionTextBox.Text, out resolution))
                m_chartWindow.ChartResolution = resolution;
            else
            {
                m_chartResolutionTextBox.Text = int.MaxValue.ToString();
                m_chartWindow.ChartResolution = int.MaxValue;
            }
        }

        // Gets the archive location of the reader
        private string GetArchiveLocation(ArchiveReader reader)
        {
            string metadataFilePath;

            if (!File.Exists(reader.FileName))
            {
                metadataFilePath = string.Format("{0}_dbase.dat", reader.FileName.Remove(reader.FileName.Length - 10));

                if (File.Exists(metadataFilePath))
                    return metadataFilePath;
            }

            return reader.FileName;
        }

        // Gets the file path of the CSV file in which to export measurements.
        private string GetExportFilePath(FileType fileType)
        {
            SaveFileDialog csvDialog = new SaveFileDialog();

            switch (fileType)
            {
                case FileType.CSV:
                    csvDialog.Filter = "CSV Files|*.csv|All Files|*.*";
                    csvDialog.DefaultExt = "csv";
                    break;

                case FileType.ComtradeAscii:
                case FileType.ComtradeBinary:
                    csvDialog.Filter = "COMTRADE Files|*.dat|All Files|*.*";
                    csvDialog.DefaultExt = "dat";
                    break;
            }

            csvDialog.AddExtension = true;
            csvDialog.CheckPathExists = true;

            if (csvDialog.ShowDialog() == true)
                return csvDialog.FileName;

            return null;
        }

        // Updates the visibility of the data grid's columns.
        private void UpdateColumns()
        {
            foreach (DataGridColumn column in m_dataGrid.Columns)
            {
                if (m_visibleColumns.Contains(column.Header))
                    column.Visibility = Visibility.Visible;
                else
                    column.Visibility = Visibility.Collapsed;
            }
        }

        // Closes the archive files and clears the archive file collection.
        private void ClearArchives()
        {
            foreach (ArchiveReader reader in m_archiveReaders)
                reader.Dispose();

            m_archiveReaders.Clear();
            m_metadata.Clear();
        }

        // Occurs when the main window is loaded at applicatoin startup.
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            string[] defaultArchivePaths = new string[0];

            OpenSessionFile(Path.Combine(FilePath.GetApplicationDataFolder(), "LastSession.hdv"), false);

            if (m_archiveReaders.Count == 0)
            {
                // See if a local archive folder exists with a valid archive
                string defaultArchiveLocation = FilePath.GetAbsolutePath("Archive");

                if (Directory.Exists(defaultArchiveLocation))
                    defaultArchivePaths = Directory.GetFiles(defaultArchiveLocation, "*_archive.d");

                if (defaultArchivePaths.Length == 0)
                {
                    // See if a local statistics folder exists with a valid archive
                    defaultArchiveLocation = FilePath.GetAbsolutePath("Statistics");

                    if (Directory.Exists(defaultArchiveLocation))
                        defaultArchivePaths = Directory.GetFiles(defaultArchiveLocation, "*_archive.d");
                }

                if (defaultArchivePaths.Length > 0)
                    OpenArchives(defaultArchivePaths);
            }
        }

        // Occurs when the starting date is changed.
        private void StartTimeDatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            m_startTime = m_startTimeDatePicker.SelectedDate.GetValueOrDefault().Date + m_startTime.TimeOfDay;
        }

        // Occurs when the ending date is changed.
        private void EndTimeDatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            m_endTime = m_endTimeDatePicker.SelectedDate.GetValueOrDefault().Date + m_endTime.TimeOfDay;
        }

        // Occurs when the user changes the starting time.
        private void StartTimeTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TimeSpan startTime;

            if (TimeSpan.TryParse(m_startTimeTextBox.Text, out startTime))
                m_startTime = m_startTime.Date + startTime;
        }

        // Occurs when the user changes the ending time.
        private void EndTimeTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TimeSpan endTime;

            if (TimeSpan.TryParse(m_endTimeTextBox.Text, out endTime))
                m_endTime = m_endTime.Date + endTime;
        }

        // Filters text input so that only numbers can be entered into the text box.
        private void NumericTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            int num;

            if (!int.TryParse(e.Text, out num))
                e.Handled = true;
        }

        // Allows pasting into a numeric text box.
        private void NumericTextBox_PasteCommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            e.Handled = true;
        }

        // Only numeric input can be pasted into a numeric text box.
        private void NumericTextBox_PasteCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            TextBox numericTextBox = sender as TextBox;

            if (numericTextBox != null)
            {
                string text = Clipboard.GetText();
                int num;

                if (int.TryParse(text, out num))
                    numericTextBox.Paste();

                e.Handled = true;
            }
        }

        // Occurs when the user selects a control that indicates they are ready to select archive files.
        private void OpenArchivesControl_Click(object sender, RoutedEventArgs e)
        {
            ShowOpenArchiveFileDialog();
        }

        // Occurs when the user selects a control that indicates they are ready to open a previously saved session.
        private void OpenControl_Click(object sender, RoutedEventArgs e)
        {
            ShowOpenSessionDialog();
        }

        // Occurs when the user selects a control that indicates they are ready to save the current session.
        private void SaveControl_Click(object sender, RoutedEventArgs e)
        {
            SaveCurrentSession();
        }

        // Occurs when the user selects the "Save as..." menu item from the file menu.
        private void SaveAsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ShowSaveSessionDialog();
        }

        // Checks for Ctrl+O key combination to prompt the user to open archive files.
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            bool altDown = Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt);
            bool ctrlDown = Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl);
            bool shiftDown = Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift);

            e.Handled = true;

            if (!altDown && ctrlDown && !shiftDown && e.Key == Key.R)
                ShowOpenArchiveFileDialog();
            else if (!altDown && ctrlDown && !shiftDown && e.Key == Key.O)
                ShowOpenSessionDialog();
            else if (!altDown && ctrlDown && !shiftDown && e.Key == Key.S && ArchiveIsOpen)
                SaveCurrentSession();
            else if (!altDown && ctrlDown && shiftDown && e.Key == Key.S && ArchiveIsOpen)
                ShowSaveSessionDialog();
            else
                e.Handled = false;
        }

        // Occurs when the context menu for the data grid header area has been initialized.
        private void ColumnContextMenu_Initialized(object sender, EventArgs e)
        {
            ContextMenu menu = sender as ContextMenu;

            if (menu != null)
                menu.ItemsSource = m_contextMenuItems;
        }

        // Sets the visibility of the data grid columns based on context menu selections made by the user.
        private void ColumnContextMenuItem_Click(object sender, RoutedEventArgs e)
        {
            MenuItem item = sender as MenuItem;

            if (item != null)
            {
                if (item.IsChecked)
                    m_visibleColumns.Add(item.Header.ToString());
                else
                    m_visibleColumns.Remove(item.Header.ToString());
            }

            UpdateColumns();
        }

        // Fixes headers and visibility of columns in the data grid when they are created.
        private void DataGrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            string header = ToProperHeader(e.Column.Header.ToString());

            switch (header)
            {
                case "Export":
                case "Display":
                    m_visibleColumns.Add(header);
                    break;

                case "Name":
                case "Point Tag":
                case "Description":
                case "Pt.#":
                    if (!m_visibleColumns.Contains(header))
                        m_visibleColumns.Add(header);

                    if (m_contextMenuItems.All(menuItem => menuItem.Header.ToString() != header))
                        m_contextMenuItems.Add(CreateContextMenuItem(header, true));

                    break;

                default:
                    if (m_contextMenuItems.All(menuItem => menuItem.Header.ToString() != header))
                        m_contextMenuItems.Add(CreateContextMenuItem(header, false));
                    break;
            }

            e.Column.Header = header;
        }

        // Sets the visibility of columns after they've all been generated.
        private void DataGrid_AutoGeneratedColumns(object sender, EventArgs e)
        {
            UpdateColumns();
        }

        // Focuses and selects cells ahead of time to allow single-click editing of data grid cells.
        private void DataGridCell_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DataGridCell cell = sender as DataGridCell;

            if (cell != null && !cell.IsFocused)
            {
                cell.Focus();
                cell.IsSelected = true;
            }
        }

        // Updates the value of the object's property as soon as the check box is checked.
        private void DataGridCell_CheckBoxChecked(object sender, RoutedEventArgs e)
        {
            DataGridCell cell = sender as DataGridCell;

            if (cell != null)
                DataGridCheckBoxCheckedChanged(cell, true);
        }

        // Updates the value of the object's property as soon as the check box is unchecked.
        private void DataGridCell_CheckBoxUnchecked(object sender, RoutedEventArgs e)
        {
            DataGridCell cell = sender as DataGridCell;

            if (cell != null)
                DataGridCheckBoxCheckedChanged(cell, false);
        }

        // Occurs when the user chooses the start time relative to current time.
        private void CurrentTimeCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            m_currentTimeStackPanel.Visibility = Visibility.Visible;
            m_historicTimeGrid.Visibility = Visibility.Collapsed;
        }

        // Occurs when the user chooses the start time not relative to current time.
        private void CurrentTimeCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            m_currentTimeStackPanel.Visibility = Visibility.Collapsed;
            m_historicTimeGrid.Visibility = Visibility.Visible;
        }

        // Occurs when the user enters text into the search box.
        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            FilterBySearchResults();
        }

        // Selects the export field for all records in the data grid.
        private void SelectAllButton_Clicked(object sender, RoutedEventArgs e)
        {
            SetExportForAll(true);
        }

        // Deselects the export field for all records in the data grid.
        private void DeselectAllButton_Click(object sender, RoutedEventArgs e)
        {
            SetExportForAll(false);
        }

        // Displays the chart window.
        private void TrendControl_Click(object sender, RoutedEventArgs e)
        {
            TrendRequested();
        }

        // Exports measurements to a CSV file.
        private void ExportControl_Click(object sender, RoutedEventArgs e)
        {
            ExportRequested();
        }

        // Shows or hides disabled points in the grid.
        private void ShowDisabledControl_Click(object sender, RoutedEventArgs e)
        {
            m_showDisabledPoints = !m_showDisabledPoints;
            ShowDisabledCheckMark.Visibility = m_showDisabledPoints ? Visibility.Visible : Visibility.Collapsed;
            FilterBySearchResults();
        }

        // Displays the chart window.
        private void ChartResolutionTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && ArchiveIsOpen)
                TrendRequested();
        }

        // Updates the start time and end time based on the latest chart boundaries.
        private void ChartWindow_ChartUpdated(object sender, EventArgs e)
        {
            StartTime = m_chartWindow.StartTime;
            EndTime = m_chartWindow.EndTime;
        }

        // Hides the child window rather than closing it so it can be shown again later.
        private void ChildWindow_Closing(object sender, CancelEventArgs e)
        {
            Window child = sender as Window;

            if (child != null)
            {
                e.Cancel = true;
                child.Hide();
            }
        }

        // Closes the window.
        private void CloseMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        // Clears the archives and closes child windows.
        private void Window_Closing(object sender, CancelEventArgs e)
        {
            try
            {
                string applicationDataFolder = FilePath.GetApplicationDataFolder();
                Directory.CreateDirectory(applicationDataFolder);
                SaveSession(Path.Combine(applicationDataFolder, "LastSession.hdv"), false);
                ClearArchives();
            }
            catch
            {
                // Just die...
            }

            try
            {
                m_chartWindow.Closing -= ChildWindow_Closing;
                m_chartWindow.Close();
            }
            catch
            {
                // Just die...
            }
        }

        #endregion

        #region [ Static ]

        // Static Methods

        // Converts an openHistorian metadata record to a COMTRADE metadata record
        private static ChannelMetadata ConvertToChannelMetadata(MetadataWrapper historianRecord)
        {
            ChannelMetadata channelRecord = new ChannelMetadata
            {
                Name = historianRecord.Name,
                IsDigital = historianRecord.GetMetadata().GeneralFlags.DataType == DataType.Digital
            };

            if (!Enum.TryParse(historianRecord.Synonym2, true, out channelRecord.SignalType))
                channelRecord.SignalType = SignalType.NONE;

            return channelRecord;
        }

        #endregion
    }
}
