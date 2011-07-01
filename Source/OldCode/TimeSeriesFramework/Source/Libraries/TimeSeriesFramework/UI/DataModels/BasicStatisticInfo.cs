//******************************************************************************************************
//  BasicStatisticInfo.cs - Gbtc
//
//  Copyright © 2010, Grid Protection Alliance.  All Rights Reserved.
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
//  06/20/2011 - Magdiel Lorenzo
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using TVA.Data;
using System.Data;

namespace TimeSeriesFramework.UI.DataModels
{
    /// <summary>
    /// Represents a Basic statistic info record as defined in the database.
    /// </summary>
    public class BasicStatisticInfo : DataModelBase
    {
        #region  [ Members ]

        private string m_source;
        private string m_name;
        private string m_description;
        private string m_value;
        private string m_timeTag;
        private string m_quality;
        private string m_dataType;
        private string m_displayFormat;
        private bool m_isConnectedState;
        private int m_loadOrder;

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the Source of the current <see cref="BasicStatisticInfo"/> object.
        /// </summary>
        public string Source
        {
            get 
            { 
                return m_source; 
            }
            set 
            { 
                m_source = value;
                OnPropertyChanged("Source");
            }
        }

        /// <summary>
        /// Gets or sets the name of the current <see cref="BasicStatisticInfo"/> object.
        /// </summary>
        public string Name
        {
            get
            {
                return m_name;
            }
            set
            {
                m_name = value;
                OnPropertyChanged("Name");
            }
        }

        /// <summary>
        /// Gets or sets the description of the current <see cref="BasicStatisticInfo"/> object.
        /// </summary>
        public string Description
        {
            get
            {
                return m_description;
            }
            set
            {
                m_description = value;
                OnPropertyChanged("Description");
            }
        }

        /// <summary>
        /// Gets or sets the value of the current <see cref="BasicStatisticInfo"/> object.
        /// </summary>
        public string Value
        {
            get
            {
                return m_value;
            }
            set
            {
                m_value = value;
                OnPropertyChanged("Value");
            }
        }

        /// <summary>
        /// Gets or sets the time tag of the current <see cref="BasicStatisticInfo"/> object.
        /// </summary>
        public string TimeTag
        {
            get
            {
                return m_timeTag;
            }
            set
            {
                m_timeTag = value;
                OnPropertyChanged("TimeTag");
            }
        }

        /// <summary>
        /// Gets or sets the quality of the current <see cref="BasicStatisticInfo"/> object.
        /// </summary>
        public string Quality
        {
            get
            {
                return m_quality;
            }
            set
            {
                m_quality = value;
                OnPropertyChanged("Quality");
            }
        }

        /// <summary>
        /// Gets or sets the data type of the current <see cref="BasicStatisticInfo"/> object.
        /// </summary>
        public string DataType
        {
            get
            {
                return m_dataType;
            }
            set
            {
                m_dataType = value;
                OnPropertyChanged("DataType");
            }
        }

        /// <summary>
        /// Gets or sets the display format of the current <see cref="BasicStatisticInfo"/> object.
        /// </summary>
        public string DisplayFormat
        {
            get
            {
                return m_displayFormat;
            }
            set
            {
                m_displayFormat = value;
                OnPropertyChanged("DisplayFormat");
            }
        }

        /// <summary>
        /// Gets or sets whether the current <see cref="BasicStatisticInfo"/> object is connected
        /// </summary>
        public bool IsConnectedState
        {
            get
            {
                return m_isConnectedState;
            }
            set
            {
                m_isConnectedState = value;
                OnPropertyChanged("IsConnectedState");
            }
        }

        /// <summary>
        /// Gets or sets the load order for the current <see cref="BasicStatisticInfo"/>.
        /// </summary>
        public int LoadOrder
        {
            get
            {
                return m_loadOrder;
            }
            set
            {
                m_loadOrder = value;
                OnPropertyChanged("LoadOrder");
            }
        }

        #endregion

        #region [ Static ]

        /// <summary>
        /// Loads <see cref="BasicStatisticInfo"/> information as an <see cref="ObservableCollection{T}"/> style list.
        /// </summary>
        /// <param name="database">Connection to database</param>
        /// <param name="nodeID">Current node ID</param>
        /// <returns>Collection of <see cref="BasicStatisticInfo"/></returns>
        public static Dictionary<int, BasicStatisticInfo> Load(AdoDataConnection database, Guid nodeID)
        {
            bool createdConnection = false;
            Dictionary<int, BasicStatisticInfo> basicStatisticInfoList = new Dictionary<int, BasicStatisticInfo>();

            try
            {
                createdConnection = CreateConnection(ref database);

                DataSet resultSet = new DataSet();
                resultSet.EnforceConstraints = false;
                DataTable resultTable = database.Connection.RetrieveData(database.AdapterType, "SELECT DeviceID, PointID, PointTag, SignalReference, MeasurementSource FROM StatisticMeasurement WHERE NodeID = @nodeID ORDER BY MeasurementSource, SignalReference", database.Guid(nodeID));
                resultSet.Tables.Add(resultTable.Copy());
                resultSet.Tables[0].TableName = "StatisticMeasurements";

                resultTable = database.Connection.RetrieveData(database.AdapterType, "SELECT Source, SignalIndex, Name, Description, DataType, DisplayFormat, IsConnectedState, LoadOrder FROM Statistic ORDER BY Source, SignalIndex");
                resultSet.Tables.Add(resultTable.Copy());
                resultSet.Tables[1].TableName = "StatisticDefinitions";

                var tempCollection = (from measurement in resultSet.Tables["StatisticMeasurements"].AsEnumerable()
                                      select new KeyValuePair<int, BasicStatisticInfo>(Convert.ToInt32(measurement.Field<object>("PointID")),
                                           (from statistic in resultSet.Tables["StatisticDefinitions"].AsEnumerable()
                                            where statistic.Field<string>("Source") == measurement.Field<string>("MeasurementSource") &&
                                            statistic.Field<int>("SignalIndex") == Convert.ToInt32((measurement.Field<string>("SignalReference")).Substring((measurement.Field<string>("SignalReference")).LastIndexOf("-ST") + 3))
                                            select new BasicStatisticInfo()
                                            {
                                                Source = statistic.Field<string>("Source"),
                                                Name = statistic.Field<string>("Name"),
                                                Description = statistic.Field<string>("Description"),
                                                Quality = "N/A",
                                                TimeTag = "N/A",
                                                Value = "--",
                                                DataType = statistic.Field<object>("DataType") == null ? string.Empty : statistic.Field<string>("DataType"),
                                                DisplayFormat = statistic.Field<object>("DisplayFormat") == null ? string.Empty : statistic.Field<string>("DisplayFormat"),
                                                IsConnectedState = Convert.ToBoolean(statistic.Field<object>("IsConnectedState")),
                                                LoadOrder = Convert.ToInt32(statistic.Field<object>("LoadOrder"))
                                            }
                                                   ).First()
                                      )).Distinct();

                foreach (var item in tempCollection)
                {
                    basicStatisticInfoList.Add(item.Key, item.Value);
                }
            }
            finally
            {
                if (createdConnection && database != null)
                    database.Dispose();
            }

            return basicStatisticInfoList;
        }

        #endregion
    }
}
