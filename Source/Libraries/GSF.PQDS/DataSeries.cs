//******************************************************************************************************
//  DataSeries.cs - Gbtc
//
//  Copyright © 2020, Grid Protection Alliance.  All Rights Reserved.
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
//  03/06/2020 - Christoph Lackner
//       Generated original version of source code.
//
//******************************************************************************************************


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GSF.PQDS
{
    /// <summary>
    /// Represents a channel in a PQDS File.
    /// </summary>
    public class DataSeries
    {
        #region[Properties]

        private List<DataPoint> m_series;
        private string m_label;

        /// <summary>
        /// A collection of DataPoints.
        /// </summary>
        public List<DataPoint> Series 
        {
            get { return m_series; }
            set { m_series = value; }
        }

        /// <summary>
        /// Label of the <see cref="DataSeries"/>
        /// </summary>
        public string Label { get { return m_label;  } }

        /// <summary>
        /// length <see cref="DataSeries"/> in number of points
        /// </summary>
        public int Length => m_series.Count();

        #endregion[Properties]

        /// <summary>
        /// Creates a new <see cref="DataSeries"/>.
        /// </summary>
        /// <param name="label">Label of the DataSeries</param>
        public DataSeries(string label)
        {
            m_label = label;
            m_series = new List<DataPoint>();

        }
        #region[methods]
   
        #endregion[methods]
    }

    /// <summary>
    /// Represents a single Point in the <see cref="DataSeries"/>.
    /// </summary>
    public class DataPoint
    {
        /// <summary>
        /// Timestamp of the point.
        /// </summary>
        public DateTime Time;

        /// <summary>
        /// Timestamp of the point.
        /// </summary>
        public double Milliseconds;

        /// <summary>
        /// Value of the point.
        /// </summary>
        public double Value;
    }


}
