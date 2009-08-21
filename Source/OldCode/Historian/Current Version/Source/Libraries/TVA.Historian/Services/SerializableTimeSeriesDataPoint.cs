//*******************************************************************************************************
//  SerializableTimeSeriesDataPoint.cs
//  Copyright © 2009 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: Pinal C. Patel
//      Office: PSO TRAN & REL, CHATTANOOGA - MR BK-C
//       Phone: 423/751-3024
//       Email: pcpatel@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  08/21/2009 - Pinal C. Patel
//       Generated original version of source code.
//
//*******************************************************************************************************

using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace TVA.Historian.Services
{
    /// <summary>
    /// Represents a time-series data-point that can be serialized using <see cref="XmlSerializer"/>, <see cref="DataContractSerializer"/> or <see cref="System.Runtime.Serialization.Json.DataContractJsonSerializer"/>.
    /// </summary>
    /// <example>
    /// This is the output for <see cref="SerializableTimeSeriesDataPoint"/> serialized using <see cref="XmlSerializer"/>:
    /// <code>
    /// <![CDATA[
    /// <?xml version="1.0"?>
    /// <TimeSeriesDataPoint xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema" 
    ///   HistorianID="1" Time="21-Aug-2009 14:21:23.236" Value="60.0419579" Quality="Good" />
    /// ]]>
    /// </code>
    /// This is the output for <see cref="SerializableTimeSeriesDataPoint"/> serialized using <see cref="DataContractSerializer"/>:
    /// <code>
    /// <![CDATA[
    /// <TimeSeriesDataPoint xmlns="http://schemas.datacontract.org/2004/07/TVA.Historian.Services" xmlns:i="http://www.w3.org/2001/XMLSchema-instance">
    ///   <HistorianID>1</HistorianID>
    ///   <Time>21-Aug-2009 14:21:54.612</Time>
    ///   <Value>60.025547</Value>
    ///   <Quality>Good</Quality>
    /// </TimeSeriesDataPoint>
    /// ]]>
    /// </code>
    /// This is the output for <see cref="SerializableTimeSeriesDataPoint"/> serialized using <see cref="System.Runtime.Serialization.Json.DataContractJsonSerializer"/>:
    /// <code>
    /// {
    ///   "HistorianID":1,
    ///   "Time":"21-Aug-2009 14:22:26.971",
    ///   "Value":59.9974136,
    ///   "Quality":29
    /// }
    /// </code>
    /// </example>
    /// <seealso cref="IDataPoint"/>
    /// <seealso cref="XmlSerializer"/>
    /// <seealso cref="DataContractSerializer"/>
    /// <seealso cref="System.Runtime.Serialization.Json.DataContractJsonSerializer"/>
    [XmlType("TimeSeriesDataPoint"), DataContract(Name = "TimeSeriesDataPoint")]
    public class SerializableTimeSeriesDataPoint
    {
        #region [ Constructors ]

        /// <summary>
        /// Initializes a new instance of the <see cref="SerializableTimeSeriesDataPoint"/> class.
        /// </summary>
        public SerializableTimeSeriesDataPoint()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SerializableTimeSeriesDataPoint"/> class.
        /// </summary>
        /// <param name="dataPoint"><see cref="IDataPoint"/> from which <see cref="SerializableTimeSeriesDataPoint"/> is to be initialized.</param>
        /// <exception cref="ArgumentNullException"><paramref name="dataPoint"/> is null.</exception>
        public SerializableTimeSeriesDataPoint(IDataPoint dataPoint)
        {
            if (dataPoint == null)
                throw new ArgumentNullException("dataPoint");

            HistorianID = dataPoint.HistorianID;
            Time = dataPoint.Time.ToString();
            Value = dataPoint.Value;
            Quality = dataPoint.Quality;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the <see cref="IDataPoint.HistorianID"/>.
        /// </summary>
        [XmlAttribute(), DataMember(Order = 0)]
        public int HistorianID { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="String"/> representation of <see cref="IDataPoint.Time"/>.
        /// </summary>
        [XmlAttribute(), DataMember(Order = 1)]
        public string Time { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="IDataPoint.Value"/>.
        /// </summary>
        [XmlAttribute(), DataMember(Order = 2)]
        public float Value { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="IDataPoint.Quality"/>.
        /// </summary>
        [XmlAttribute(), DataMember(Order = 3)]
        public Quality Quality { get; set; }

        #endregion
    }
}
