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
		/// <summary>
		/// Saves measurement back to the configuration database
		/// </summary>
		/// <param name="database">Database connection for query. Will be created from config if this value is null.</param>
		/// <param name="measurement">Measurement to be inserted or updated</param>
		public void Save(AdoDataConnection database, Measurement measurement)
		{
			var createdConnection = false;

			try
			{
				createdConnection = CreateConnection(ref database);

				string query;
				if (measurement.SignalId == Guid.Empty)
				{
					query = database.ParameterizedQueryString("INSERT INTO Measurement (SignalId, DeviceID, PointTag, SignalTypeID, " +
						"SignalReference, Adder, Multiplier, Description, Enabled, UpdatedBy, UpdatedOn, CreatedBy, CreatedOn) VALUES ({0}, {1}, {2}, " +
						"{3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11}, {12})", "signalId", "deviceID", "pointTag", "signalTypeID",
						"signalReference", "adder", "multiplier", "description", "enabled", "updatedBy", "updatedOn",
						"createdBy", "createdOn");

					database.Connection.ExecuteNonQuery(query, DataExtensions.DefaultTimeoutDuration, measurement.DeviceId.ToNotNull(), measurement.PointTag, measurement.SignalTypeId, measurement.SignalReference,
						measurement.Adder, measurement.Multiplier, measurement.Description.ToNotNull(), database.Bool(measurement.Enabled), CommonFunctions.CurrentUser, database.UtcNow, CommonFunctions.CurrentUser, database.UtcNow);
				}
				else
				{
					query = database.ParameterizedQueryString("UPDATE Measurement SET DeviceID = {0}, PointTag = {1}, " +
						"SignalTypeID = {2}, SignalReference = {3}, Adder = {4}, Multiplier = {5}, Description = {6}, " +
						"Enabled = {7}, UpdatedBy = {8}, UpdatedOn = {9} WHERE SignalId = {10}", "deviceID", "pointTag",
						"alternateTag", "signalTypeID", "phasorSourceINdex", "signalReference", "adder", "multiplier", "description",
						"enabled", "updatedBy", "updatedOn", "signalId");

					database.Connection.ExecuteNonQuery(query, DataExtensions.DefaultTimeoutDuration, measurement.DeviceId.ToNotNull(), measurement.PointTag,
						measurement.SignalTypeId, measurement.SignalReference, measurement.Adder, measurement.Multiplier, measurement.Description.ToNotNull(), database.Bool(measurement.Enabled), CommonFunctions.CurrentUser, database.UtcNow);
				}
			}
			finally
			{
				if (createdConnection && database != null)
					database.Dispose();
			}
		}

		private static bool CreateConnection(ref AdoDataConnection database)
		{
			if (database != null) return false;
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
	}
}
