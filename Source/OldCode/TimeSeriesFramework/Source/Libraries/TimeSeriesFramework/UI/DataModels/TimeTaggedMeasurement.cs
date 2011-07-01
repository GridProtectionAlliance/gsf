//******************************************************************************************************
//  TimeTaggedMeasurement.cs - Gbtc
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

namespace TimeSeriesFramework.UI.DataModels
{
    /// <summary>
    /// Represents a time-tagged measurement for real-time statistics.
    /// </summary>
    public class TimeTaggedMeasurement : DataModelBase
    {

        #region [ Members ]

        private string m_timeTag;
        private string m_currentValue;
        private string m_quality;

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the current <see cref="TimeTaggedMeasurement"/>'s time tag.
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
        /// Gets or sets the current value of the current <see cref="TimeTaggedMeasurement"/>
        /// </summary>
        public string CurrentValue
        {
            get
            {
                return m_currentValue;
            }
            set
            {
                m_currentValue = value;
                OnPropertyChanged("CurrentValue");
            }
        }

        /// <summary>
        /// Gets or sets the quality of the current <see cref="TimeTaggedMeasurement"/>
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

        #endregion        
    }

    /// <summary>
    /// Represents a statistic measurement data object.
    /// </summary>
    public class StatisticMeasurementData
    {
        public string SourceType { get; set; }
        public bool IsExpanded { get; set; }
        public ObservableCollection<StreamInfo> SourceStreamInfoList { get; set; }
    }

    /// <summary>
    /// Represents a stream info object.
    /// </summary>
    public class StreamInfo
    {
        public int ID { get; set; }
        public string Acronym { get; set; }
        public string Name { get; set; }
        public string StatusColor { get; set; }
        public bool IsExpanded { get; set; }
        public ObservableCollection<DeviceStatistic> DeviceStatisticList { get; set; }
        public ObservableCollection<DetailStatisticInfo> StatisticList { get; set; }
    }

    /// <summary>
    /// Class to hold device statistic info.
    /// </summary>
    public class DeviceStatistic
    {
        public int ID { get; set; }
        public string Acronym { get; set; }
        public string Name { get; set; }
        public bool IsExpanded { get; set; }
        public ObservableCollection<DetailStatisticInfo> StatisticList { get; set; }
    }

    /// <summary>
    /// Class to hold detail statistic info.
    /// </summary>
    public class DetailStatisticInfo
    {
        public int DeviceID { get; set; }
        public int PointID { get; set; }
        public string PointTag { get; set; }
        public string SignalReference { get; set; }
        public bool IsExpanded { get; set; }
        public BasicStatisticInfo Statistics { get; set; }
    }

    /// <summary>
    /// Class to bind statistic measurement data to view.
    /// </summary>
    public class StatisticMeasurementDataForBinding
    {
        public ObservableCollection<StatisticMeasurementData> StatisticMeasurementDataList { get; set; }
        public bool IsExpanded { get; set; }
    }
    

}
