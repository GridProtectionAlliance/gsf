//******************************************************************************************************
//  MeasurementRepository.cs - Gbtc
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
//  11/2/2015 - Ryan McCoy
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using GSF.Data;
using GSF.TimeSeries.UI;

namespace PowerCalculations.PowerMultiCalculator
{
	/// <summary>
	/// Class used to save measurement objects for power calculation adapter
	/// </summary>
	public class MeasurementRepository
	{
		#region [ Methods ]

        /// <summary>
        /// Saves measurement back to the configuration database
        /// </summary>
        /// <param name="database">Database connection for query. Will be created from config if this value is null.</param>
        /// <param name="measurement">Measurement to be inserted or updated</param>
        public void Save(AdoDataConnection database, PowerMeasurement measurement)
        {
            var createdConnection = false;

			try
			{
				createdConnection = CreateConnection(ref database);

				if (measurement.SignalID == Guid.Empty)
				{
					database.ExecuteNonQuery("INSERT INTO Measurement (DeviceID, PointTag, SignalTypeID, " +
                        "SignalReference, Adder, Multiplier, Description, Enabled, UpdatedBy, UpdatedOn, CreatedBy, CreatedOn) VALUES " + 
                        "({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11})", measurement.DeviceID.ToNotNull(), measurement.PointTag, 
                        measurement.SignalTypeID, measurement.SignalReference, measurement.Adder, measurement.Multiplier, measurement.Description.ToNotNull(), 
                        database.Bool(measurement.Enabled), CommonFunctions.CurrentUser, database.UtcNow, CommonFunctions.CurrentUser, database.UtcNow);

                    measurement.SignalID = Guid.Parse(database.ExecuteScalar<object>(Guid.Empty, "SELECT SignalID FROM Measurement WHERE PointTag={0}", measurement.PointTag).ToString());
				}
				else
				{
					database.ExecuteNonQuery("UPDATE Measurement SET DeviceID = {0}, PointTag = {1}, " +
                        "SignalTypeID = {2}, SignalReference = {3}, Adder = {4}, Multiplier = {5}, Description = {6}, " +
                        "Enabled = {7}, UpdatedBy = {8}, UpdatedOn = {9} WHERE SignalId = {10}", measurement.DeviceID.ToNotNull(), measurement.PointTag,
						measurement.SignalTypeID, measurement.SignalReference, measurement.Adder, measurement.Multiplier, measurement.Description.ToNotNull(), 
                        database.Bool(measurement.Enabled), CommonFunctions.CurrentUser, database.UtcNow, measurement.SignalID);
				}
			}
			finally
			{
				if (createdConnection)
					database?.Dispose();
			}
		}

		private static bool CreateConnection(ref AdoDataConnection database)
		{
			if ((object)database != null)
                return false;

			try
			{
				database = new AdoDataConnection(CommonFunctions.DefaultSettingsCategory);
				return true;
			}
			catch
			{
				return false;
			}
		}

		#endregion
	}
}
