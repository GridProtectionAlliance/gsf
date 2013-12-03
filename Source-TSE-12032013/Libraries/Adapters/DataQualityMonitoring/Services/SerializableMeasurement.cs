//******************************************************************************************************
//  SerializableMeasurement.cs - Gbtc
//
//  Copyright © 2012, Grid Protection Alliance.  All Rights Reserved.
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
//  12/16/2009 - Stephen C. Wills
//       Generated original version of source code.
//  12/13/2012 - Starlynn Danyelle Gilliam
//       MOdified Header.
//
//******************************************************************************************************

using System;
using System.Data;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using GSF;
using GSF.TimeSeries;

namespace DataQualityMonitoring
{
    /// <summary>
    /// Represents a serializable <see cref="IMeasurement"/> that can be serialized using <see cref="XmlSerializer"/>, <see cref="DataContractSerializer"/> or <see cref="System.Runtime.Serialization.Json.DataContractJsonSerializer"/>.
    /// </summary>
    [XmlType("Measurement"), DataContract(Name = "Measurement", Namespace = "")]
    public class SerializableMeasurement
    {

        #region [ Members ]

        // Events
        /// <summary>
        /// Occurs when an System.Exception is encountered when processing a request.
        /// </summary>
        /// <remarks><see cref="EventArgs{T}.Argument"/> is the exception encountered when processing a request.</remarks>
        public event EventHandler<EventArgs<Exception>> ProcessException;

        // Fields
        readonly IMeasurement m_sourceMeasurement;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Initializes a new instance of the <see cref="SerializableMeasurement"/> class.
        /// </summary>
        public SerializableMeasurement()
        {
            Key = string.Empty;
            SignalID = string.Empty;
            Timestamp = string.Empty;
            SignalType = string.Empty;
            Device = string.Empty;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SerializableMeasurement"/> class.
        /// </summary>
        /// <param name="measurement"><see cref="IMeasurement"/> from which <see cref="SerializableMeasurement"/> is to be initialized.</param>
        public SerializableMeasurement(IMeasurement measurement)
        {
            m_sourceMeasurement = measurement;
            Key = measurement.Key.ToString();
            SignalID = measurement.ID.ToString();
            Value = measurement.AdjustedValue;
            Timestamp = ((DateTime)measurement.Timestamp).ToString("yyyy-MM-dd HH:mm:ss.fff");
            SignalType = string.Empty;
            Device = string.Empty;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the <see cref="IMeasurement.Key"/>.
        /// </summary>
        [XmlAttribute, DataMember(Order = 0)]
        public string Key
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the <see cref="ITimeSeriesValue.ID"/>.
        /// </summary>
        [XmlAttribute, DataMember(Order = 1)]
        public string SignalID
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the <see cref="IMeasurement.AdjustedValue"/>.
        /// </summary>
        [XmlAttribute, DataMember(Order = 2)]
        public double Value
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the <see cref="ITimeSeriesValue.Timestamp"/> in <see cref="DateTime"/> string format.
        /// </summary>
        [XmlAttribute, DataMember(Order = 3)]
        public string Timestamp
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the signal type of the <see cref="IMeasurement"/>.
        /// </summary>
        [XmlAttribute, DataMember(Order = 4)]
        public string SignalType
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the device of the <see cref="IMeasurement"/>.
        /// </summary>
        [XmlAttribute, DataMember(Order = 5)]
        public string Device
        {
            get;
            set;
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Acquires and sets the device and signal type of the source measurement.
        /// </summary>
        /// <param name="dataSource"><see cref="DataSet"/> which contains information about the device and the signal type of the source measurement.</param>
        public void SetDeviceAndSignalType(DataSet dataSource)
        {
            try
            {
                DataRow row = dataSource.Tables["ActiveMeasurements"].Select(string.Format("ID = '{0}'", m_sourceMeasurement.Key.ToString()))[0];
                TrySetDevice(row);
                TrySetSignalType(row);
            }
            catch (Exception ex)
            {
                OnProcessException(ex);
            }
        }

        /// <summary>
        /// Raises the <see cref="ProcessException"/> event.
        /// </summary>
        /// <param name="exception"><see cref="Exception"/> to send to <see cref="ProcessException"/> event.</param>
        protected virtual void OnProcessException(Exception exception)
        {
            if (ProcessException != null)
                ProcessException(this, new EventArgs<Exception>(exception));
        }

        private bool TrySetDevice(DataRow row)
        {
            try
            {
                Device = row["Device"].ToString();
                return true;
            }
            catch (Exception ex)
            {
                OnProcessException(ex);
                return false;
            }
        }

        private bool TrySetSignalType(DataRow row)
        {
            try
            {
                SignalType = row["SignalType"].ToString();
                return true;
            }
            catch (Exception ex)
            {
                OnProcessException(ex);
                return false;
            }
        }

        #endregion
    }
}
