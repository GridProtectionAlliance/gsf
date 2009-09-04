//*******************************************************************************************************
//  TimeSeriesDataService.cs
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
//  09/01/2009 - Pinal C. Patel
//       Generated original version of source code.
//
//*******************************************************************************************************

using System;
using System.Collections.Generic;
using System.ServiceModel;
using TVA.Historian.Files;

namespace TVA.Historian.Services
{
    /// <summary>
    /// Represents a REST web service for time-series data.
    /// </summary>
    /// <seealso cref="SerializableTimeSeriesData"/>
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class TimeSeriesDataService : Service, ITimeSeriesDataService
    {
        #region [ Constructors ]

        /// <summary>
        /// Initializes a new instance of the <see cref="TimeSeriesDataService"/> class.
        /// </summary>
        public TimeSeriesDataService()
            :base()
        {
            ServiceUri = "http://localhost:5152/historian";
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Writes <paramref name="data"/> received in <see cref="System.ServiceModel.Web.WebMessageFormat.Xml"/> format to the <see cref="Service.Archive"/>.
        /// </summary>
        /// <param name="data">An <see cref="SerializableTimeSeriesData"/> object.</param>
        public void WriteTimeSeriesDataAsXml(SerializableTimeSeriesData data)
        {
            WriteTimeSeriesData(data);
        }

        /// <summary>
        /// Writes <paramref name="data"/> received in <see cref="System.ServiceModel.Web.WebMessageFormat.Json"/> format to the <see cref="Service.Archive"/>.
        /// </summary>
        /// <param name="data">An <see cref="SerializableTimeSeriesData"/> object.</param>
        public void WriteTimeSeriesDataAsJson(SerializableTimeSeriesData data)
        {
            WriteTimeSeriesData(data);
        }

        /// <summary>
        /// Reads current time-series data from the <see cref="Service.Archive"/> and sends it in <see cref="System.ServiceModel.Web.WebMessageFormat.Xml"/> format.
        /// </summary>
        /// <param name="idList">A comma or semi-colon delimited list of IDs for which current time-series data is to be read.</param>
        /// <returns>An <see cref="SerializableTimeSeriesData"/> object.</returns>
        public SerializableTimeSeriesData ReadSelectCurrentTimeSeriesDataAsXml(string idList)
        {
            return ReadCurrentTimeSeriesData(idList);
        }

        /// <summary>
        /// Reads current time-series data from the <see cref="Service.Archive"/> and sends it in <see cref="System.ServiceModel.Web.WebMessageFormat.Xml"/> format.
        /// </summary>
        /// <param name="fromID">Starting ID in the ID range for which current time-series data is to be read.</param>
        /// <param name="toID">Ending ID in the ID range for which current time-series data is to be read.</param>
        /// <returns>An <see cref="SerializableTimeSeriesData"/> object.</returns>
        public SerializableTimeSeriesData ReadRangeCurrentTimeSeriesDataAsXml(string fromID, string toID)
        {
            return ReadCurrentTimeSeriesData(fromID, toID);
        }

        /// <summary>
        /// Reads current time-series data from the <see cref="Service.Archive"/> and sends it in <see cref="System.ServiceModel.Web.WebMessageFormat.Json"/> format.
        /// </summary>
        /// <param name="idList">A comma or semi-colon delimited list of IDs for which current time-series data is to be read.</param>
        /// <returns>An <see cref="SerializableTimeSeriesData"/> object.</returns>
        public SerializableTimeSeriesData ReadSelectCurrentTimeSeriesDataAsJson(string idList)
        {
            return ReadCurrentTimeSeriesData(idList);
        }

        /// <summary>
        /// Reads current time-series data from the <see cref="Service.Archive"/> and sends it in <see cref="System.ServiceModel.Web.WebMessageFormat.Json"/> format.
        /// </summary>
        /// <param name="fromID">Starting ID in the ID range for which current time-series data is to be read.</param>
        /// <param name="toID">Ending ID in the ID range for which current time-series data is to be read.</param>
        /// <returns>An <see cref="SerializableTimeSeriesData"/> object.</returns>
        public SerializableTimeSeriesData ReadRangeCurrentTimeSeriesDataAsJson(string fromID, string toID)
        {
            return ReadCurrentTimeSeriesData(fromID, toID);
        }

        /// <summary>
        /// Reads historic time-series data from the <see cref="Service.Archive"/> and sends it in <see cref="System.ServiceModel.Web.WebMessageFormat.Xml"/> format.
        /// </summary>
        /// <param name="idList">A comma or semi-colon delimited list of IDs for which historic time-series data is to be read.</param>
        /// <param name="startTime">Start time in <see cref="String"/> format of the timespan for which historic time-series data is to be read.</param>
        /// <param name="endTime">End time in <see cref="String"/> format of the timespan for which historic time-series data is to be read.</param>
        /// <returns>An <see cref="SerializableTimeSeriesData"/> object.</returns>
        public SerializableTimeSeriesData ReadSelectHistoricTimeSeriesDataAsXml(string idList, string startTime, string endTime)
        {
            return ReadHistoricTimeSeriesData(idList, startTime, endTime);
        }

        /// <summary>
        /// Reads historic time-series data from the <see cref="Service.Archive"/> and sends it in <see cref="System.ServiceModel.Web.WebMessageFormat.Xml"/> format.
        /// </summary>
        /// <param name="fromID">Starting ID in the ID range for which historic time-series data is to be read.</param>
        /// <param name="toID">Ending ID in the ID range for which historic time-series data is to be read.</param>
        /// <param name="startTime">Start time in <see cref="String"/> format of the timespan for which historic time-series data is to be read.</param>
        /// <param name="endTime">End time in <see cref="String"/> format of the timespan for which historic time-series data is to be read.</param>
        /// <returns>An <see cref="SerializableTimeSeriesData"/> object.</returns>
        public SerializableTimeSeriesData ReadRangeHistoricTimeSeriesDataAsXml(string fromID, string toID, string startTime, string endTime)
        {
            return ReadHistoricTimeSeriesData(fromID, toID, startTime, endTime);
        }

        /// <summary>
        /// Reads historic time-series data from the <see cref="Service.Archive"/> and sends it in <see cref="System.ServiceModel.Web.WebMessageFormat.Json"/> format.
        /// </summary>
        /// <param name="idList">A comma or semi-colon delimited list of IDs for which historic time-series data is to be read.</param>
        /// <param name="startTime">Start time in <see cref="String"/> format of the timespan for which historic time-series data is to be read.</param>
        /// <param name="endTime">End time in <see cref="String"/> format of the timespan for which historic time-series data is to be read.</param>
        /// <returns>An <see cref="SerializableTimeSeriesData"/> object.</returns>
        public SerializableTimeSeriesData ReadSelectHistoricTimeSeriesDataAsJson(string idList, string startTime, string endTime)
        {
            return ReadHistoricTimeSeriesData(idList, startTime, endTime);
        }

        /// <summary>
        /// Reads historic time-series data from the <see cref="Service.Archive"/> and sends it in <see cref="System.ServiceModel.Web.WebMessageFormat.Json"/> format.
        /// </summary>
        /// <param name="fromID">Starting ID in the ID range for which historic time-series data is to be read.</param>
        /// <param name="toID">Ending ID in the ID range for which historic time-series data is to be read.</param>
        /// <param name="startTime">Start time in <see cref="String"/> format of the timespan for which historic time-series data is to be read.</param>
        /// <param name="endTime">End time in <see cref="String"/> format of the timespan for which historic time-series data is to be read.</param>
        /// <returns>An <see cref="SerializableTimeSeriesData"/> object.</returns>
        public SerializableTimeSeriesData ReadRangeHistoricTimeSeriesDataAsJson(string fromID, string toID, string startTime, string endTime)
        {
            return ReadHistoricTimeSeriesData(fromID, toID, startTime, endTime);
        }

        private void WriteTimeSeriesData(SerializableTimeSeriesData data)
        {
            try
            {
                // Ensure that writing data is allowed.
                if (!CanWrite)
                    throw new InvalidOperationException("Write operation is prohibited.");

                // Ensure that data archive is available.
                if (Archive == null)
                    throw new ArgumentNullException("Archive");

                // Write all time-series data to the archive.
                foreach (SerializableTimeSeriesDataPoint point in data.TimeSeriesDataPoints)
                {
                    Archive.WriteData(point.Deflate());
                }
            }
            catch (Exception ex)
            {
                // Notify about the encountered processing exception.
                OnServiceProcessError(ex);
                throw;
            }
        }

        private SerializableTimeSeriesData ReadCurrentTimeSeriesData(string idList)
        {
            try
            {
                // Ensure that reading data is allowed.
                if (!CanRead)
                    throw new InvalidOperationException("Read operation is prohibited.");

                // Ensure that data archive is available.
                if (Archive == null)
                    throw new ArgumentNullException("Archive");

                // Read current time-series data from the archive.
                int id = 0;
                byte[] buffer = null;
                StateRecord state = null;
                SerializableTimeSeriesData data = new SerializableTimeSeriesData();
                List<SerializableTimeSeriesDataPoint> points = new List<SerializableTimeSeriesDataPoint>();
                foreach (string singleID in idList.Split(',', ';'))
                {
                    id = int.Parse(singleID);
                    buffer = Archive.ReadStateData(id);
                    state = new StateRecord(id, buffer, 0, buffer.Length);
                    points.Add(new SerializableTimeSeriesDataPoint(state.CurrentData));
                }
                data.TimeSeriesDataPoints = points.ToArray();

                return data;
            }
            catch (Exception ex)
            {
                // Notify about the encountered processing exception.
                OnServiceProcessError(ex);
                throw;
            }
        }

        private SerializableTimeSeriesData ReadCurrentTimeSeriesData(string fromID, string toID)
        {
            try
            {
                // Ensure that reading data is allowed.
                if (!CanRead)
                    throw new InvalidOperationException("Read operation is prohibited.");

                // Ensure that data archive is available.
                if (Archive == null)
                    throw new ArgumentNullException("Archive");

                // Read current time-series data from the archive.
                byte[] buffer = null;
                StateRecord state = null;
                SerializableTimeSeriesData data = new SerializableTimeSeriesData();
                List<SerializableTimeSeriesDataPoint> points = new List<SerializableTimeSeriesDataPoint>();
                for (int id = int.Parse(fromID); id <= int.Parse(toID); id++)
                {
                    buffer = Archive.ReadStateData(id);
                    state = new StateRecord(id, buffer, 0, buffer.Length);
                    points.Add(new SerializableTimeSeriesDataPoint(state.CurrentData));
                }
                data.TimeSeriesDataPoints = points.ToArray();

                return data;
            }
            catch (Exception ex)
            {
                // Notify about the encountered processing exception.
                OnServiceProcessError(ex);
                throw;
            }
        }

        private SerializableTimeSeriesData ReadHistoricTimeSeriesData(string idList, string startTime, string endTime)
        {
            try
            {
                // Ensure that reading data is allowed.
                if (!CanRead)
                    throw new InvalidOperationException("Read operation is prohibited.");

                // Ensure that data archive is available.
                if (Archive == null)
                    throw new ArgumentNullException("Archive");

                // Read historic time-series data from the archive.
                int id = 0;
                SerializableTimeSeriesData data = new SerializableTimeSeriesData();
                List<SerializableTimeSeriesDataPoint> points = new List<SerializableTimeSeriesDataPoint>();
                foreach (string singleID in idList.Split(',', ';'))
                {
                    id = int.Parse(singleID);
                    foreach (IDataPoint point in Archive.ReadData(id, startTime, endTime))
                    {
                        points.Add(new SerializableTimeSeriesDataPoint(point));
                    }
                }
                data.TimeSeriesDataPoints = points.ToArray();

                return data;
            }
            catch (Exception ex)
            {
                // Notify about the encountered processing exception.
                OnServiceProcessError(ex);
                throw;
            }
        }

        private SerializableTimeSeriesData ReadHistoricTimeSeriesData(string fromID, string toID, string startTime, string endTime)
        {
            try
            {
                // Ensure that reading data is allowed.
                if (!CanRead)
                    throw new InvalidOperationException("Read operation is prohibited.");

                // Ensure that data archive is available.
                if (Archive == null)
                    throw new ArgumentNullException("Archive");

                // Read historic time-series data from the archive.
                SerializableTimeSeriesData data = new SerializableTimeSeriesData();
                List<SerializableTimeSeriesDataPoint> points = new List<SerializableTimeSeriesDataPoint>();
                for (int id = int.Parse(fromID); id <= int.Parse(toID); id++)
                {
                    foreach (IDataPoint point in Archive.ReadData(id, startTime, endTime))
                    {
                        points.Add(new SerializableTimeSeriesDataPoint(point));
                    }
                }
                data.TimeSeriesDataPoints = points.ToArray();

                return data;
            }
            catch (Exception ex)
            {
                // Notify about the encountered processing exception.
                OnServiceProcessError(ex);
                throw;
            }
        }

        #endregion
    }
}
