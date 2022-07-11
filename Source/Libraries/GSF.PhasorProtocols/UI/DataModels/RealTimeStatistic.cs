//******************************************************************************************************
//  RealTimeStatistic.cs - Gbtc
//
//  Copyright © 2010, Grid Protection Alliance.  All Rights Reserved.
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
//  09/28/2011 - Mehulbhai P Thakkar
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Windows.Media;
using GSF.Data;
using GSF.TimeSeries.Statistics;
using GSF.TimeSeries.UI;

// ReSharper disable AccessToDisposedClosure
namespace GSF.PhasorProtocols.UI.DataModels
{
    /// <summary>
    /// Represents collection of statistics for the configured devices.
    /// </summary>
    public class RealTimeStatistic : DataModelBase
    {
        #region [ Members ]

        // Fields
        private string m_sourceType;
        private bool m_expanded;
        private ObservableCollection<StreamStatistic> m_streamStatisticList;

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets <see cref="RealTimeStatistic"/>'s SourceType.
        /// </summary>
        public string SourceType
        {
            get => m_sourceType;
            set
            {
                m_sourceType = value;
                OnPropertyChanged("SourceType");
            }
        }

        /// <summary>
        /// Gets or sets <see cref="RealTimeStatistic"/>'s SourceType.
        /// </summary>
        public bool Expanded
        {
            get => m_expanded;
            set
            {
                m_expanded = value;
                OnPropertyChanged("Expanded");
            }
        }

        /// <summary>
        /// Gets or sets <see cref="RealTimeStatistic"/>'s SourceType.
        /// </summary>
        public ObservableCollection<StreamStatistic> StreamStatisticList
        {
            get => m_streamStatisticList;
            set
            {
                m_streamStatisticList = value;
                OnPropertyChanged("StreamStatisticList");
            }
        }

        #endregion

        #region [ Methods ]

        // Static

        /// <summary>
        /// Creates <see cref="ObservableCollection{T}"/> type collection of <see cref="RealTimeStatistic"/>.
        /// </summary>
        /// <param name="database"><see cref="AdoDataConnection"/> to connection to database.</param>
        /// <returns>Collection of <see cref="RealTimeStatistic"/>.</returns>
        public static ObservableCollection<RealTimeStatistic> Load(AdoDataConnection database)
        {
            bool createdConnection = false;
            try
            {
                createdConnection = CreateConnection(ref database);
                ObservableCollection<RealTimeStatistic> realTimeStatisticList = new();

                DataSet resultSet = new();
                resultSet.EnforceConstraints = false;

                // Get PDCs and directly connected devices.
                resultSet.Tables.Add(database.Connection.RetrieveData(database.AdapterType, database.ParameterizedQueryString("SELECT ID, Acronym, Name FROM DeviceDetail " +
                    "WHERE NodeID = {0} AND (IsConcentrator = {1} OR ParentAcronym = {2} OR ParentAcronym IS NULL) AND Enabled = {3} ORDER BY Acronym", "nodeID", "isConcentrator", "parentAcronym", "enabled"),
                    DefaultTimeout, database.CurrentNodeID(), database.Bool(true), string.Empty, database.Bool(true)).Copy());

                resultSet.Tables[0].TableName = "DirectDevices";

                // Get all the devices connected via PDC.
                resultSet.Tables.Add(database.Connection.RetrieveData(database.AdapterType, database.ParameterizedQueryString("SELECT ID, Acronym, Name, ParentID, " +
                    "ParentAcronym FROM DeviceDetail WHERE NodeID = {0} AND IsConcentrator = {1} AND Enabled = {2} AND ParentID > 0 ORDER BY Acronym", "nodeID", "isConcentrator", "enabled"),
                    DefaultTimeout, database.CurrentNodeID(), database.Bool(false), database.Bool(true)).Copy());

                resultSet.Tables[1].TableName = "PdcDevices";

                // Get output stream information.
                resultSet.Tables.Add(database.Connection.RetrieveData(database.AdapterType, database.ParameterizedQueryString("SELECT ID, Acronym, Name FROM OutputStream WHERE " +
                    "NodeID = {0} AND Enabled = {1} ORDER BY Acronym", "nodeID", "enabled"), DefaultTimeout, database.CurrentNodeID(), database.Bool(true)).Copy());

                resultSet.Tables[2].TableName = "OutputStreams";

                resultSet.Tables.Add(database.Connection.RetrieveData(database.AdapterType, database.ParameterizedQueryString("SELECT ID, AdapterName FROM CustomActionAdapter WHERE " +
                    "NodeID = {0} AND TypeName = {1} AND Enabled = {2} ORDER BY AdapterName", "nodeID", "typeName", "enabled"), DefaultTimeout, database.CurrentNodeID(), "GSF.TimeSeries.Transport.DataPublisher", database.Bool(true)).Copy());

                resultSet.Tables[3].TableName = "DataPublishers";

                // Get list of statistic measurements detail.
                ObservableCollection<StatisticMeasurement> statisticMeasurements = GetStatisticMeasurements(database);
                bool expanded = (statisticMeasurements.Count < 100);

                // We do this for later use in refreshing data.
                StatisticMeasurements = new Dictionary<Guid, StatisticMeasurement>();

                foreach (StatisticMeasurement statisticMeasurement in statisticMeasurements)
                    StatisticMeasurements.Add(statisticMeasurement.SignalID, statisticMeasurement);

                // Create a system statistics list.
                ObservableCollection<StreamStatistic> systemStatistics = new();

                systemStatistics.Add(new StreamStatistic()
                {
                    ID = 0,
                    Acronym = "SYSTEM",
                    Name = "System",
                    StatusColor = "Green",
                    Expanded = expanded,
                    StatisticMeasurementList = new ObservableCollection<StatisticMeasurement>(statisticMeasurements.Where(sm => sm.SignalReference.Contains("!SYSTEM"))),
                    DeviceStatisticList = new ObservableCollection<PdcDeviceStatistic>()
                });

                SystemStatistics = new Dictionary<int, StreamStatistic>();
                foreach (StreamStatistic streamStatistic in systemStatistics)
                {
                    // We do this to associate statistic measurement to parent output stream easily.
                    foreach (StatisticMeasurement measurement in streamStatistic.StatisticMeasurementList)
                        measurement.DeviceID = streamStatistic.ID;

                    streamStatistic.DeviceStatisticList.Insert(0, new PdcDeviceStatistic()
                    {
                        DeviceID = 0,
                        DeviceAcronym = "Run-time Statistics",
                        DeviceName = "",
                        Expanded = expanded,
                        StatisticMeasurementList = streamStatistic.StatisticMeasurementList
                    });

                    streamStatistic.StatisticMeasurementList = null;

                    // We do this for later use in refreshing data.
                    SystemStatistics.Add(streamStatistic.ID, streamStatistic);
                }

                // Create an input stream statistics list.
                ObservableCollection<StreamStatistic> inputStreamStatistics = new                
                (
                    from stream in resultSet.Tables["DirectDevices"].AsEnumerable()
                    select new StreamStatistic
                    {
                        ID = Convert.ToInt32(stream.Field<object>("ID")),
                        Acronym = stream.Field<string>("Acronym"),
                        Name = stream.Field<string>("Name"),
                        StatusColor = "Gray",
                        Expanded = expanded,
                        StatisticMeasurementList = new ObservableCollection<StatisticMeasurement>
                        (
                            (
                                from statisticMeasurement in statisticMeasurements
                                where statisticMeasurement.DeviceID == Convert.ToInt32(stream.Field<object>("ID"))
                                select statisticMeasurement
                            ).OrderBy(sm => sm.Source).ThenBy(sm => sm.LoadOrder)
                        ),
                        DeviceStatisticList = new ObservableCollection<PdcDeviceStatistic>
                        (
                            from pdcdevice in resultSet.Tables["PdcDevices"].AsEnumerable()
                            where Convert.ToInt32(pdcdevice.Field<object>("ParentID")) == Convert.ToInt32(stream.Field<object>("ID"))
                            select new PdcDeviceStatistic()
                            {
                                DeviceID = Convert.ToInt32(pdcdevice.Field<object>("ID")),
                                ParentID = Convert.ToInt32(pdcdevice.Field<object>("ParentID")),
                                DeviceAcronym = pdcdevice.Field<string>("Acronym"),
                                DeviceName = pdcdevice.Field<string>("Name"),
                                Expanded = expanded,
                                StatisticMeasurementList = new ObservableCollection<StatisticMeasurement>
                                (
                                    (
                                        from statisticMeasurement in statisticMeasurements
                                        where statisticMeasurement.DeviceID == Convert.ToInt32(pdcdevice.Field<object>("ID"))
                                        select statisticMeasurement
                                    ).OrderBy(sm => sm.LoadOrder)
                                )
                            }
                        )
                    }
                );

                InputStreamStatistics = new Dictionary<int, StreamStatistic>();
                DevicesWithStatisticMeasurements = new Dictionary<int, ObservableCollection<StatisticMeasurement>>();

                foreach (StreamStatistic streamStatistic in inputStreamStatistics)
                {
                    streamStatistic.DeviceStatisticList.Insert(0, new PdcDeviceStatistic()
                    {
                        DeviceID = 0,
                        DeviceAcronym = "Run-time Statistics",
                        DeviceName = "",
                        Expanded = expanded,
                        StatisticMeasurementList = new ObservableCollection<StatisticMeasurement>(streamStatistic.StatisticMeasurementList)
                    });

                    // We do this for later use in refreshing data.
                    InputStreamStatistics.Add(streamStatistic.ID, streamStatistic);

                    // We do this for use in Input Status & Monitoring screen to set proper status color.
                    if (streamStatistic.ID > 0)
                    {
                        DevicesWithStatisticMeasurements.Add(streamStatistic.ID, streamStatistic.StatisticMeasurementList);
                        foreach (PdcDeviceStatistic device in streamStatistic.DeviceStatisticList)
                        {
                            if (device.DeviceID > 0)
                                DevicesWithStatisticMeasurements.Add(device.DeviceID, device.StatisticMeasurementList);
                        }
                    }

                    streamStatistic.StatisticMeasurementList = null;

                }

                // Create an output stream statistics list.
                ObservableCollection<StreamStatistic> outputStreamStatistics = new                
                (
                    from outputStream in resultSet.Tables["OutputStreams"].AsEnumerable()
                    select new StreamStatistic()
                    {
                        ID = Convert.ToInt32(outputStream.Field<object>("ID")),
                        Acronym = outputStream.Field<string>("Acronym"),
                        Name = outputStream.Field<string>("Name"),
                        StatusColor = "Gray",
                        DeviceStatisticList = new ObservableCollection<PdcDeviceStatistic>(),
                        Expanded = expanded,
                        StatisticMeasurementList = new ObservableCollection<StatisticMeasurement>
                        (
                            (
                                from statisticMeasurement in statisticMeasurements
                                where statisticMeasurement.SignalReference.StartsWith($"{outputStream.Field<string>("Acronym")}!OS-")
                                select statisticMeasurement
                            ).OrderBy(sm => sm.Source).ThenBy(sm => sm.LoadOrder)
                        )
                    }
                );

                OutputStreamStatistics = new Dictionary<int, StreamStatistic>();

                foreach (StreamStatistic streamStatistic in outputStreamStatistics)
                {
                    // We do this to associate statistic measurement to parent output stream easily.
                    foreach (StatisticMeasurement measurement in streamStatistic.StatisticMeasurementList)
                        measurement.DeviceID = streamStatistic.ID;

                    streamStatistic.DeviceStatisticList.Insert(0, new PdcDeviceStatistic
                    {
                        DeviceID = 0,
                        DeviceAcronym = "Run-time Statistics",
                        DeviceName = "",
                        Expanded = expanded,
                        StatisticMeasurementList = streamStatistic.StatisticMeasurementList
                    });

                    streamStatistic.StatisticMeasurementList = null;

                    // We do this for later use in refreshing data.
                    OutputStreamStatistics.Add(streamStatistic.ID, streamStatistic);
                }

                // Create a data publisher statistics list
                ObservableCollection<StreamStatistic> dataPublisherStatistics = new
                (
                    from publisher in resultSet.Tables["DataPublishers"].AsEnumerable()
                    select new StreamStatistic()
                    {
                        ID = Convert.ToInt32(publisher.Field<object>("ID")),
                        Acronym = publisher.Field<string>("AdapterName"),
                        Name = "",
                        StatusColor = "Gray",
                        DeviceStatisticList = new ObservableCollection<PdcDeviceStatistic>(),
                        Expanded = expanded,
                        StatisticMeasurementList = new ObservableCollection<StatisticMeasurement>
                        (
                            (
                                from statisticMeasurement in statisticMeasurements
                                where statisticMeasurement.SignalReference.StartsWith($"{publisher.Field<string>("AdapterName")}!PUB-")
                                select statisticMeasurement
                            ).OrderBy(sm => sm.Source).ThenBy(sm => sm.LoadOrder)
                        )
                    }
                );

                DataPublisherStatistics = new Dictionary<int, StreamStatistic>();

                foreach (StreamStatistic streamStatistic in dataPublisherStatistics)
                {
                    // We do this to associate statistic measurement to parent output stream easily.
                    foreach (StatisticMeasurement measurement in streamStatistic.StatisticMeasurementList)
                        measurement.DeviceID = streamStatistic.ID;

                    streamStatistic.DeviceStatisticList.Insert(0, new PdcDeviceStatistic()
                    {
                        DeviceID = 0,
                        DeviceAcronym = "Run-time Statistics",
                        DeviceName = "",
                        Expanded = expanded,
                        StatisticMeasurementList = streamStatistic.StatisticMeasurementList
                    });

                    streamStatistic.StatisticMeasurementList = null;

                    // We do this for later use in refreshing data.
                    DataPublisherStatistics.Add(streamStatistic.ID, streamStatistic);
                }

                // Merge system, input and output stream statistics to create a realtime statistics list.
                realTimeStatisticList.Add(new RealTimeStatistic()
                {
                    SourceType = "System",
                    Expanded = false,
                    StreamStatisticList = systemStatistics
                });

                realTimeStatisticList.Add(new RealTimeStatistic()
                {
                    SourceType = "Input Streams",
                    Expanded = false,
                    StreamStatisticList = inputStreamStatistics
                });

                realTimeStatisticList.Add(new RealTimeStatistic()
                {
                    SourceType = "Output Streams",
                    Expanded = false,
                    StreamStatisticList = outputStreamStatistics
                });

                realTimeStatisticList.Add(new RealTimeStatistic()
                {
                    SourceType = "Data Publisher",
                    Expanded = false,
                    StreamStatisticList = dataPublisherStatistics
                });

                return realTimeStatisticList;
            }
            finally
            {
                if (createdConnection && database is not null)
                    database.Dispose();
            }
        }

        /// <summary>
        /// Gets statistic measurements from the database for current node.
        /// </summary>
        /// <param name="database"><see cref="AdoDataConnection"/> to connection to database.</param>
        /// <returns>Collection of <see cref="StatisticMeasurement"/>.</returns>
        public static ObservableCollection<StatisticMeasurement> GetStatisticMeasurements(AdoDataConnection database)
        {
            bool createdConnection = false;

            try
            {
                createdConnection = CreateConnection(ref database);

                // Get Statistic Definitions
                ObservableCollection<Statistic> statisticDefinitions = Statistic.Load(database);

                // Get statistics measurements.
                DataTable statisticMeasurements = database.Connection.RetrieveData(database.AdapterType, database.ParameterizedQueryString("SELECT SignalID, ID, DeviceID, PointID, PointTag, SignalReference, Description " +
                    "FROM StatisticMeasurement WHERE NodeID = {0}", "nodeID"), DefaultTimeout, database.CurrentNodeID());

                // Assign min and max point IDs as we will need them to request data from web service.
                MinPointID = int.MaxValue;
                MaxPointID = int.MinValue;

                foreach (DataRow row in statisticMeasurements.Rows)
                {
                    int pointID = Convert.ToInt32(row.Field<object>("PointID"));
                    MinPointID = Math.Min(MinPointID, pointID);
                    MaxPointID = Math.Max(MaxPointID, pointID);
                }


                // Takes datarow from statisticMeasurements data table, and associates each row to their statistic source and returns KeyValuePair.
                Func<DataRow, KeyValuePair<DataRow, string>> mapFunction = measurement =>
                {
                    string signalReference = measurement.Field<string>("SignalReference");
                    string measurementSource;

                    if (StatisticsEngine.RegexMatch(signalReference, "SYSTEM"))
                        measurementSource = "System";
                    else if (StatisticsEngine.RegexMatch(signalReference, "PMU"))
                        measurementSource = "Device";
                    else if (StatisticsEngine.RegexMatch(signalReference, "OS"))
                        measurementSource = "OutputStream";
                    else if (StatisticsEngine.RegexMatch(signalReference, "IS"))
                        measurementSource = "InputStream";
                    else if (StatisticsEngine.RegexMatch(signalReference, "SUB"))
                        measurementSource = "Subscriber";
                    else if (StatisticsEngine.RegexMatch(signalReference, "PUB"))
                        measurementSource = "Publisher";
                    else
                        measurementSource = "???";

                    return new KeyValuePair<DataRow, string>(measurement, measurementSource);
                };

                // Takes KeyValuePair and generates StatisticMeasurement record by maping statistic measurements with statistic definitions.
                Func<KeyValuePair<DataRow, string>, StatisticMeasurement> selectFunction = keyvaluepair =>
                {
                    DataRow measurement = keyvaluepair.Key;
                    string measurementSource = keyvaluepair.Value;
                    Debug.WriteLine(measurementSource);
                    string signalReference = measurement.Field<string>("SignalReference");
                    int signalReferenceIndex = signalReference.LastIndexOf("-ST", StringComparison.OrdinalIgnoreCase);
                    int measurementIndex = (signalReferenceIndex != -1) ? Convert.ToInt32(signalReference.Substring(signalReference.LastIndexOf("-ST", StringComparison.OrdinalIgnoreCase) + 3)) : -1;
                    Statistic statisticDefinition = null;

                    foreach (Statistic statistic in statisticDefinitions)
                    {
                        if (statistic.Source == measurementSource && statistic.SignalIndex == measurementIndex)
                        {
                            statisticDefinition = statistic;
                            break;
                        }
                    }

                    return new StatisticMeasurement()
                    {
                        SignalID = database.Guid(measurement, "SignalID"),
                        ID = measurement.Field<string>("ID"),
                        DeviceID = Convert.ToInt32(measurement.Field<object>("DeviceID") ?? -1),
                        PointID = Convert.ToInt32(measurement.Field<object>("PointID")),
                        PointTag = measurement.Field<string>("PointTag"),
                        SignalReference = signalReference,
                        Source = measurementSource,
                        StatisticName = statisticDefinition?.Name ?? measurement.Field<string>("Description"),
                        StatisticDescription = statisticDefinition?.Description ?? measurement.Field<string>("Description"),
                        DataType = statisticDefinition?.DataType ?? "System.Double",
                        DisplayFormat = statisticDefinition?.DisplayFormat ?? "{0}",
                        ConnectedState = statisticDefinition?.IsConnectedState ?? false,
                        LoadOrder = statisticDefinition?.LoadOrder ?? 0,
                        TimeTag = "n/a",
                        Quality = "n/a",
                        Value = "--"
                    };
                };

                return new ObservableCollection<StatisticMeasurement>(statisticMeasurements.Rows.Cast<DataRow>().Select(mapFunction).OrderBy(pair => pair.Value).Select(selectFunction).OrderBy(s => s.LoadOrder));

            }
            finally
            {
                if (createdConnection && database is not null)
                    database.Dispose();
            }
        }

        /// <summary>
        /// Defines maximum value of statistic measurement's point id defined in the database.
        /// </summary>
        public static int MaxPointID;

        /// <summary>
        /// Defines minimum value of statistic measurement's point id defined in the database.
        /// </summary>
        public static int MinPointID;

        /// <summary>
        /// Defines a collection of <see cref="StatisticMeasurement"/>s defined in the database.
        /// </summary>
        public static Dictionary<Guid, StatisticMeasurement> StatisticMeasurements;

        /// <summary>
        /// Defines a collection of <see cref="StreamStatistic"/> defined in the database.
        /// </summary>
        public static Dictionary<int, StreamStatistic> SystemStatistics;

        /// <summary>
        /// Defines a collection of <see cref="StreamStatistic"/> defined in the database.
        /// </summary>
        public static Dictionary<int, StreamStatistic> InputStreamStatistics;

        /// <summary>
        /// Defines a collection of <see cref="StreamStatistic"/> defined in the database.
        /// </summary>
        public static Dictionary<int, StreamStatistic> OutputStreamStatistics;

        /// <summary>
        /// Defines a collection of <see cref="StreamStatistic"/> defined in the database.
        /// </summary>
        public static Dictionary<int, StreamStatistic> DataPublisherStatistics;

        /// <summary>
        /// Defines collection of device ids with associated statistical measurements.
        /// </summary>
        public static Dictionary<int, ObservableCollection<StatisticMeasurement>> DevicesWithStatisticMeasurements;

        #endregion
    }

    /// <summary>
    /// Represents a connection (either to a PDC or a device) with a list of associated devices and statistical measurements.
    /// </summary>
    public class StreamStatistic : DataModelBase
    {
        #region [ Members ]

        // Fields
        private int m_id;
        private string m_acronym;
        private string m_name;
        private string m_statusColor;
        private bool m_expanded;
        private ObservableCollection<PdcDeviceStatistic> m_deviceStatisticList;
        private ObservableCollection<StatisticMeasurement> m_statisticMeasurementList;

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets <see cref="StreamStatistic"/>'s ID.
        /// </summary>
        public int ID
        {
            get => m_id;
            set
            {
                m_id = value;
                OnPropertyChanged("ID");
            }
        }

        /// <summary>
        /// Gets or sets <see cref="StreamStatistic"/>'s Acronym.
        /// </summary>
        public string Acronym
        {
            get => m_acronym;
            set
            {
                m_acronym = value;
                OnPropertyChanged("Acronym");
            }
        }

        /// <summary>
        /// Gets or sets <see cref="StreamStatistic"/>'s Name.
        /// </summary>
        public string Name
        {
            get => m_name;
            set
            {
                m_name = value;
                OnPropertyChanged("Name");
            }
        }

        /// <summary>
        /// Gets or set <see cref="StreamStatistic"/>'s Foreground.
        /// </summary>
        public string StatusColor
        {
            get => m_statusColor;
            set
            {
                m_statusColor = value;
                OnPropertyChanged("StatusColor");
            }
        }

        /// <summary>
        /// Gets or sets <see cref="StreamStatistic"/>'s DeviceStatisticList.
        /// </summary>
        public ObservableCollection<PdcDeviceStatistic> DeviceStatisticList
        {
            get => m_deviceStatisticList;
            set
            {
                m_deviceStatisticList = value;
                OnPropertyChanged("DeviceStatisticList");
            }
        }

        /// <summary>
        /// Gets or sets <see cref="StreamStatistic"/>'s StatisticMeasurementList.
        /// </summary>
        public ObservableCollection<StatisticMeasurement> StatisticMeasurementList
        {
            get => m_statisticMeasurementList;
            set
            {
                m_statisticMeasurementList = value;
                OnPropertyChanged("StatisticMeasurementList");
            }
        }

        /// <summary>
        /// Gets or sets <see cref="StreamStatistic"/>'s Expanded flag.
        /// </summary>
        public bool Expanded
        {
            get => m_expanded;
            set
            {
                m_expanded = value;
                OnPropertyChanged("Expanded");
            }
        }

        #endregion
    }

    /// <summary>
    /// Represents a device with a list of statistical measurement associated with it.
    /// </summary>
    public class PdcDeviceStatistic : DataModelBase
    {
        #region [ Members ]

        // Fields
        private int m_id;
        private int m_parentId;
        private string m_acronym;
        private string m_name;
        private ObservableCollection<StatisticMeasurement> m_statisticMeasurementList;
        private bool m_expanded;

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets <see cref="PdcDeviceStatistic"/>'s ID.
        /// </summary>
        public int DeviceID
        {
            get => m_id;
            set
            {
                m_id = value;
                OnPropertyChanged("DeviceID");
            }
        }

        /// <summary>
        /// Gets or sets <see cref="PdcDeviceStatistic"/>'s ParentID.
        /// </summary>
        public int ParentID
        {
            get => m_parentId;
            set
            {
                m_parentId = value;
                OnPropertyChanged("ParentID");
            }
        }

        /// <summary>
        /// Gets or sets <see cref="PdcDeviceStatistic"/>'s Acronym.
        /// </summary>
        public string DeviceAcronym
        {
            get => m_acronym;
            set
            {
                m_acronym = value;
                OnPropertyChanged("DeviceAcronym");
            }
        }

        /// <summary>
        /// Gets or sets <see cref="PdcDeviceStatistic"/>'s Name.
        /// </summary>
        public string DeviceName
        {
            get => m_name;
            set
            {
                m_name = value;
                OnPropertyChanged("DeviceName");
            }
        }

        /// <summary>
        /// Gets or sets <see cref="PdcDeviceStatistic"/>'s StatisticMeasurementList.
        /// </summary>
        public ObservableCollection<StatisticMeasurement> StatisticMeasurementList
        {
            get => m_statisticMeasurementList;
            set
            {
                m_statisticMeasurementList = value;
                OnPropertyChanged("StatisticMeasurementList");
            }
        }

        /// <summary>
        /// Gets or sets <see cref="PdcDeviceStatistic"/>'s Expanded flag.
        /// </summary>
        public bool Expanded
        {
            get => m_expanded;
            set
            {
                m_expanded = value;
                OnPropertyChanged("Expanded");
            }
        }

        #endregion
    }

    /// <summary>
    /// Represents a measurement record with all related statistic information.
    /// </summary>
    public class StatisticMeasurement : DataModelBase
    {
        #region [ Members ]

        // Fields
        private Guid m_signalID;
        private int m_deviceID;
        private int m_pointID;
        private string m_id;
        private string m_pointTag;
        private string m_signalReference;
        private string m_source;
        private string m_statisticName;
        private string m_statisticDescription;
        private string m_dataType;
        private string m_displayFormat;
        private bool m_connectedState;
        private int m_loadOrder;
        private string m_timeTag;
        private string m_value;
        private string m_quality;
        private SolidColorBrush m_foreground;
        private bool m_expanded;

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets <see cref="StatisticMeasurement"/>'s SignalID.
        /// </summary>
        public Guid SignalID
        {
            get => m_signalID;
            set
            {
                m_signalID = value;
                OnPropertyChanged("SignalID");
            }
        }

        /// <summary>
        /// Gets or set <see cref="StatisticMeasurement"/>'s DeviceID.
        /// </summary>
        public int DeviceID
        {
            get => m_deviceID;
            set
            {
                m_deviceID = value;
                OnPropertyChanged("DeviceID");
            }
        }

        /// <summary>
        /// Gets or set <see cref="StatisticMeasurement"/>'s PointID.
        /// </summary>
        public int PointID
        {
            get => m_pointID;
            set
            {
                m_pointID = value;
                OnPropertyChanged("PointID");
            }
        }

        /// <summary>
        /// Gets or set <see cref="StatisticMeasurement"/>'s ID.
        /// </summary>
        public string ID
        {
            get => m_id;
            set
            {
                m_id = value;
                OnPropertyChanged("ID");
            }
        }

        /// <summary>
        /// Gets or set <see cref="StatisticMeasurement"/>'s PointTag.
        /// </summary>
        public string PointTag
        {
            get => m_pointTag;
            set
            {
                m_pointTag = value;
                OnPropertyChanged("PointTag");
            }
        }

        /// <summary>
        /// Gets or set <see cref="StatisticMeasurement"/>'s SignalReference.
        /// </summary>
        public string SignalReference
        {
            get => m_signalReference;
            set
            {
                m_signalReference = value;
                OnPropertyChanged("SignalReference");
            }
        }

        /// <summary>
        /// Gets or set <see cref="StatisticMeasurement"/>'s Source.
        /// </summary>
        public string Source
        {
            get => m_source;
            set
            {
                m_source = value;
                OnPropertyChanged("Source");
            }
        }

        /// <summary>
        /// Gets or set <see cref="StatisticMeasurement"/>'s StatisticName.
        /// </summary>
        public string StatisticName
        {
            get => m_statisticName;
            set
            {
                m_statisticName = value;
                OnPropertyChanged("StatisticName");
            }
        }

        /// <summary>
        /// Gets or set <see cref="StatisticMeasurement"/>'s StatisticDescription.
        /// </summary>
        public string StatisticDescription
        {
            get => m_statisticDescription;
            set
            {
                m_statisticDescription = value;
                OnPropertyChanged("StatisticDescription");
            }
        }

        /// <summary>
        /// Gets or set <see cref="StatisticMeasurement"/>'s DataType.
        /// </summary>
        public string DataType
        {
            get => m_dataType;
            set
            {
                m_dataType = value;
                OnPropertyChanged("DataType");
            }
        }

        /// <summary>
        /// Gets or set <see cref="StatisticMeasurement"/>'s DisplayFormat.
        /// </summary>
        public string DisplayFormat
        {
            get => m_displayFormat;
            set
            {
                m_displayFormat = value;
                OnPropertyChanged("DisplayFormat");
            }
        }

        /// <summary>
        /// Gets or set <see cref="StatisticMeasurement"/>'s ConnectedState.
        /// </summary>
        public bool ConnectedState
        {
            get => m_connectedState;
            set
            {
                m_connectedState = value;
                OnPropertyChanged("ConnectedState");
            }
        }

        /// <summary>
        /// Gets or set <see cref="StatisticMeasurement"/>'s LoadOrder.
        /// </summary>
        public int LoadOrder
        {
            get => m_loadOrder;
            set
            {
                m_loadOrder = value;
                OnPropertyChanged("LoadOrder");
            }
        }

        /// <summary>
        /// Gets or set <see cref="StatisticMeasurement"/>'s TimeTag.
        /// </summary>
        public string TimeTag
        {
            get => m_timeTag;
            set
            {
                m_timeTag = value;
                OnPropertyChanged("TimeTag");
            }
        }

        /// <summary>
        /// Gets or set <see cref="StatisticMeasurement"/>'s Value.
        /// </summary>
        public string Value
        {
            get => m_value;
            set
            {
                m_value = value;
                OnPropertyChanged("Value");
            }
        }

        /// <summary>
        /// Gets or set <see cref="StatisticMeasurement"/>'s Quality.
        /// </summary>
        public string Quality
        {
            get => m_quality;
            set
            {
                m_quality = value;
                OnPropertyChanged("Quality");
            }
        }

        /// <summary>
        /// Gets or set <see cref="StatisticMeasurement"/>'s Foreground.
        /// </summary>
        public SolidColorBrush Foreground
        {
            get => m_foreground;
            set
            {
                m_foreground = value;
                OnPropertyChanged("Foreground");
            }
        }

        /// <summary>
        /// Gets or set <see cref="StatisticMeasurement"/>'s Expanded flag.
        /// </summary>
        public bool Expanded
        {
            get => m_expanded;
            set
            {
                m_expanded = value;
                OnPropertyChanged("Expanded");
            }
        }

        #endregion
    }
}
