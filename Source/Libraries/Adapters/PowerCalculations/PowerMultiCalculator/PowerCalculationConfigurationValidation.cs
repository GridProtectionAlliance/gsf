//******************************************************************************************************
//  PowerCalculationConfigurationValidation.cs - Gbtc
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
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using GSF.Configuration;
using GSF.Data;
using PhasorProtocolAdapters;

namespace PowerCalculations.PowerMultiCalculator
{
    #region [ Static ]

    /// <summary>
    /// Implements validations for power calculations in the power calculation adapter
    /// </summary>
    public static class PowerCalculationConfigurationValidation
    {
        /// <summary>
        /// Validates that data operation and adapter instance exist within database.
        /// </summary>
        public static void ValidateDatabaseDefinitions()
        {
            using (AdoDataConnection database = new AdoDataConnection("systemSettings"))
            {
                if (!DataOperationExists(database))
                    CreateDataOperation(database);

                if (!AdapterInstanceExists(database))
                    CreateAdapterInstance(database);
            }
        }

        /// <summary>
        /// Returns true if a data operation exists to run this class. Returns false otherwise.
        /// </summary>
        /// <param name="database">Database connection to use for checking the data operation</param>
        /// <returns>True or false indicating whether the operation exists</returns>
        private static bool DataOperationExists(AdoDataConnection database)
        {
            return Convert.ToInt32(database.ExecuteScalar($"SELECT COUNT(*) FROM DataOperation WHERE TypeName='{typeof(PowerCalculationConfigurationValidation).FullName}' AND MethodName='ValidatePowerCalculationConfigurations'")) > 0;
        }

        /// <summary>
        /// Creates a data operation to run the validations in this class.
        /// </summary>
        /// <param name="database">Database connection to use for creating the data operation</param>
        private static void CreateDataOperation(AdoDataConnection database)
        {
            database.ExecuteNonQuery($"INSERT INTO DataOperation(Description, AssemblyName, TypeName, MethodName, Enabled) VALUES ('Power Calculation Validations', 'PowerCalculations.dll', '{typeof(PowerCalculationConfigurationValidation).FullName}', 'ValidatePowerCalculationConfigurations', 1)");
        }

        /// <summary>
        /// Returns true if a data operation exists to run this class. Returns false otherwise.
        /// </summary>
        /// <param name="database">Database connection to use for checking the data operation</param>
        /// <returns>True or false indicating whether the operation exists</returns>
        private static bool AdapterInstanceExists(AdoDataConnection database)
        {
            return Convert.ToInt32(database.ExecuteScalar($"SELECT COUNT(*) FROM CustomActionAdapter WHERE TypeName='{typeof(PowerMultiCalculatorAdapter).FullName}'")) > 0;
        }

        /// <summary>
        /// Creates a data operation to run the validations in this class.
        /// </summary>
        /// <param name="database">Database connection to use for creating the data operation</param>
        private static void CreateAdapterInstance(AdoDataConnection database)
        {
            database.ExecuteNonQuery($"INSERT INTO CustomActionAdapter(NodeID, AdapterName, AssemblyName, TypeName, ConnectionString, Enabled) VALUES ('{ConfigurationFile.Current.Settings["systemSettings"]["NodeID"].ValueAs<Guid>()}', 'PHASOR!POWERCALC', 'PowerCalculations.dll', '{typeof(PowerMultiCalculatorAdapter).FullName}', 'FramesPerSecond=30; LagTime=3; LeadTime=1', 1)");
        }

        /// <summary>
        /// Data operation to validate power calculation configuration. This method checks that input measurements and non-null output measurements exist, are enabled, and have the correct signal type.
        /// If null output measurements are found in the configuration, this method will create the output measurements and update the configuration.
        /// </summary>
        /// <param name="database">Database connection for configuration information</param>
        /// <param name="nodeIDQueryString">Node ID for database queries</param>
        /// <param name="trackingVersion">Not used</param>
        /// <param name="arguments">Not used</param>
        /// <param name="statusMessage">Delegate for method to communicate status updates</param>
        /// <param name="processException">Exception handling delegate</param>
        public static void ValidatePowerCalculationConfigurations(AdoDataConnection database, string nodeIDQueryString, ulong trackingVersion, string arguments, Action<string> statusMessage, Action<Exception> processException)
        {
            try
            {
                CheckInputMeasurementsExist(database, nodeIDQueryString, statusMessage);
                CheckInputMeasurementsAreEnabled(database, nodeIDQueryString, statusMessage);
                CheckInputMeasurementsSignalType(database, nodeIDQueryString, statusMessage);
                CheckOutputMeasurementsExist(database, nodeIDQueryString, statusMessage);
                CheckOutputMeasurementsAreEnabled(database, nodeIDQueryString, statusMessage);
                CheckOutputMeasurementsSignalType(database, nodeIDQueryString, statusMessage);
                CreateOutputMeasurementsWhereNull(database, nodeIDQueryString, statusMessage);
            }
            catch (Exception e)
            {
                processException(e);
            }
        }



        /// <summary>
        /// Checks power calculation configuration for null output measurements. When found, a new measurement is created, and the configuration is updated to use the new measurement.
        /// </summary>
        /// <param name="database">Configuration database connection</param>
        /// <param name="nodeIDQueryString">Node ID formatted for query</param>
        /// <param name="statusMessage">Delegate for communicating status updates</param>
        [SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities", Justification = "Code has been evaluated, SQL text used by function is not based on user input.")]
        private static void CreateOutputMeasurementsWhereNull(AdoDataConnection database, string nodeIDQueryString, Action<string> statusMessage)
        {
            statusMessage("Checking for calculations with null output measurements...");

            string query =
                $"SELECT " +
                $"    pc.ID, " +
                $"    pc.CircuitDescription, " +
                $"    pc.ActivePowerOutputSignalID, " +
                $"    pc.ApparentPowerOutputSignalID, " +
                $"    pc.ReactivePowerOutputSignalID, " +
                $"    v.Acronym AS VendorAcronym, " +
                $"    d.Acronym AS DeviceAcronym, " +
                $"    c.Acronym AS CompanyAcronym, " +
                $"    d.id AS DeviceID, " +
                $"    vm.HistorianID AS HistorianID, " +
                $"    p.Label AS CurrentLabel " +
                $"FROM " +
                $"    PowerCalculation pc JOIN " +
                $"    Measurement vm ON vm.SignalID = pc.VoltageAngleSignalID JOIN " +
                $"    Measurement im ON im.SignalID = pc.CurrentAngleSignalID LEFT OUTER JOIN " +
                $"    Phasor p ON im.DeviceID = p.DeviceID AND im.PhasorSourceIndex = p.SourceIndex LEFT OUTER JOIN " +
                $"    Device d ON vm.DeviceID = d.ID LEFT OUTER JOIN " +
                $"    VendorDevice vd ON vd.ID = d.VendorDeviceID LEFT OUTER JOIN " +
                $"    Vendor v ON vd.VendorID = v.ID LEFT OUTER JOIN " +
                $"    Company c ON d.CompanyID = c.ID " +
                $"WHERE " +
                $"    pc.Enabled <> 0 AND " +
                $"    pc.NodeID = {nodeIDQueryString} AND " +
                $"    ( " +
                $"        pc.ActivePowerOutputSignalID IS NULL OR " +
                $"        pc.ReactivePowerOutputSignalID IS NULL OR " +
                $"        pc.ApparentPowerOutputSignalID IS NULL " +
                $"    )";

            Dictionary<int, PowerMeasurement> activePowerUpdates = new Dictionary<int, PowerMeasurement>();
            Dictionary<int, PowerMeasurement> reactivePowerUpdates = new Dictionary<int, PowerMeasurement>();
            Dictionary<int, PowerMeasurement> apparentPowerUpdates = new Dictionary<int, PowerMeasurement>();

            using (IDbCommand cmd = database.Connection.CreateCommand())
            {
                cmd.CommandText = query;
                using (IDataReader rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        int powerCalculationID = rdr.GetInt32(0);
                        string companyAcronym = rdr.IsDBNull(7) ? "" : rdr.GetString(7);
                        string vendorAcronym = rdr.IsDBNull(5) ? "" : rdr.GetString(5);
                        string signalTypeAcronym = "CALC";
                        string circuitDescription = rdr.IsDBNull(1) ? "" : rdr.GetString(1);
                        int deviceID = rdr.GetInt32(8);
                        int? historianID = rdr.IsDBNull(9) ? null : (int?)rdr.GetInt32(9);

                        if (rdr.IsDBNull(2)) // Real - MW
                        {
                            // create active power output measurement
                            PowerMeasurement measurement = CreateMeasurement(companyAcronym, circuitDescription + "-MW", vendorAcronym, signalTypeAcronym, circuitDescription, deviceID, historianID, "Active Power Calculation");
                            activePowerUpdates.Add(powerCalculationID, measurement);
                        }

                        if (rdr.IsDBNull(3)) // Apparent - MVA
                        {
                            // create apparent power output measurement
                            PowerMeasurement measurement = CreateMeasurement(companyAcronym, circuitDescription + "-MVA", vendorAcronym, signalTypeAcronym, circuitDescription, deviceID, historianID, "Apparent Power Calculation");
                            apparentPowerUpdates.Add(powerCalculationID, measurement);
                        }

                        if (rdr.IsDBNull(4)) // Reactive - MVAR
                        {
                            //create reactive power output measurement
                            PowerMeasurement measurement = CreateMeasurement(companyAcronym, circuitDescription + "-MVAR", vendorAcronym, signalTypeAcronym, circuitDescription, deviceID, historianID, "Reactive Power Calculation");
                            reactivePowerUpdates.Add(powerCalculationID, measurement);
                        }
                    }
                }
            }

            int newMeasurementsCount = activePowerUpdates.Count + reactivePowerUpdates.Count + apparentPowerUpdates.Count;

            if (newMeasurementsCount > 0)
            {
                MeasurementRepository repo = new MeasurementRepository();

                statusMessage($"Creating {newMeasurementsCount} new output measurements for power calculation...");

                foreach (KeyValuePair<int, PowerMeasurement> update in activePowerUpdates)
                {
                    repo.Save(database, update.Value);
                    UpdatePowerCalculation(database, update.Key, activePowerOutputSignalID: update.Value.SignalID);
                }

                statusMessage("Successfully created new active power calculations.");

                foreach (KeyValuePair<int, PowerMeasurement> update in reactivePowerUpdates)
                {
                    repo.Save(database, update.Value);
                    UpdatePowerCalculation(database, update.Key, reactivePowerOutputSignalID: update.Value.SignalID);
                }

                statusMessage("Successfully created new reactive power calculations.");

                foreach (KeyValuePair<int, PowerMeasurement> update in apparentPowerUpdates)
                {
                    repo.Save(database, update.Value);
                    UpdatePowerCalculation(database, update.Key, apparentPowerOutputSignalID: update.Value.SignalID);
                }

                statusMessage("Successfully created new apparent power calculations.");

                statusMessage("Completed creation of new measurements for null output measurements on power calculations.");
            }
        }

        /// <summary>
        /// Creates a new measurement object for power calculation output measurements
        /// </summary>
        private static PowerMeasurement CreateMeasurement(string companyAcronym, string deviceAcronym, string vendorAcronym, string signalTypeAcronym, string circuitDescription, int deviceID, int? historianID, string descriptionSuffix)
        {
            PowerMeasurement measurement = new PowerMeasurement
            {
                PointTag = CommonPhasorServices.CreatePointTag(companyAcronym, deviceAcronym, vendorAcronym, signalTypeAcronym),
                Adder = 0,
                Multiplier = 1,
                Description = $"{circuitDescription} {descriptionSuffix}",
                DeviceID = deviceID,
                HistorianID = historianID,
                SignalTypeID = 10,
                Enabled = true,
                SignalID = Guid.Empty
            };

            if (measurement.PointTag != null)
            {
                int beginIndex = measurement.PointTag.IndexOf('_');
                measurement.SignalReference = measurement.PointTag.Substring(beginIndex + 1);
            }

            return measurement;
        }

        /// <summary>
        /// Updated power calculation to use new Signal IDs for output measurement
        /// </summary>
        /// <param name="database">Configuration database connection</param>
        /// <param name="powerCalculationID">ID of the power calculation to be updated</param>
        /// <param name="activePowerOutputSignalID">Active power output signal ID, if needed for update</param>
        /// <param name="reactivePowerOutputSignalID">Reactive power output signal ID, if needed for update</param>
        /// <param name="apparentPowerOutputSignalID">Apparent power output signal ID, if needed for update</param>
        [SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities", Justification = "Code has been evaluated, SQL generated by function is based non-text based numeric input.")]
        private static void UpdatePowerCalculation(AdoDataConnection database, int powerCalculationID, Guid? activePowerOutputSignalID = null, Guid? reactivePowerOutputSignalID = null, Guid? apparentPowerOutputSignalID = null)
        {
            if (activePowerOutputSignalID == null && reactivePowerOutputSignalID == null && apparentPowerOutputSignalID == null)
                return;

            List<string> updates = new List<string>();
            if (activePowerOutputSignalID != null)
                updates.Add($"ActivePowerOutputSignalID='{activePowerOutputSignalID.Value}'");
            if (reactivePowerOutputSignalID != null)
                updates.Add($"ReactivePowerOutputSignalID='{reactivePowerOutputSignalID.Value}'");
            if (apparentPowerOutputSignalID != null)
                updates.Add($"ApparentPowerOutputSignalID='{apparentPowerOutputSignalID.Value}'");

            string query = "UPDATE PowerCalculation SET ";
            query += string.Join(", ", updates) + " ";
            query += $"WHERE ID={powerCalculationID} ";

            using (IDbCommand cmd = database.Connection.CreateCommand())
            {
                cmd.CommandText = query;
                cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Verifies that output measurements are set to calculated signal type. Calculations are disabled if output measurements are not configured correctly.
        /// </summary>
        /// <param name="database">Configuration database connection</param>
        /// <param name="nodeIDQueryString">Node ID formatted for query</param>
        /// <param name="statusMessage">Delegate to communicate status updates</param>
        private static void CheckOutputMeasurementsSignalType(AdoDataConnection database, string nodeIDQueryString, Action<string> statusMessage)
        {
            statusMessage("Validating if non-null output measurements have correct signal type...");

            string query =
                $"UPDATE PowerCalculation " +
                $"SET Enabled = 0 " +
                $"WHERE " +
                $"    Enabled = 1 AND " +
                $"    NodeID = {nodeIDQueryString} AND " +
                $"    ( " +
                $"        (SELECT Acronym FROM Measurement JOIN SignalType ON Measurement.SignalTypeID = SignalType.ID WHERE SignalID = ActivePowerOutputSignalID) <> 'CALC' OR " +
                $"        (SELECT Acronym FROM Measurement JOIN SignalType ON Measurement.SignalTypeID = SignalType.ID WHERE SignalID = ActivePowerOutputSignalID) <> 'CALC' OR " +
                $"        (SELECT Acronym FROM Measurement JOIN SignalType ON Measurement.SignalTypeID = SignalType.ID WHERE SignalID = ActivePowerOutputSignalID) <> 'CALC' " +
                $"    )";

            const string failureMessage = "One or more power calculations are defined with non-null output measurements having the wrong signal type. Power calculations will be disabled.";

            RunDatabaseValidation(database, statusMessage, query, failureMessage);
        }

        /// <summary>
        /// Verifies that output measurements are enabled. Power calculations will be disabled where outputs are disabled.
        /// </summary>
        /// <param name="database"></param>
        /// <param name="nodeIDQueryString"></param>
        /// <param name="statusMessage"></param>
        private static void CheckOutputMeasurementsAreEnabled(AdoDataConnection database, string nodeIDQueryString, Action<string> statusMessage)
        {
            statusMessage("Validating if non-null output measurements are enabled...");

            string query =
                $"UPDATE PowerCalculation " +
                $"SET Enabled = 0 " +
                $"WHERE " +
                $"    Enabled = 1 AND " +
                $"    NodeID = {nodeIDQueryString} AND " +
                $"    (SELECT COUNT(*) FROM Measurement WHERE Enabled = 0 AND SignalID IN (ActivePowerOutputSignalID, ReactivePowerOutputSignalID, ApparentPowerOutputSignalID)) > 0";

            const string failureMessage = "One or more power calculations are defined with output measurements that are disabled. Power calculations will be disabled.";

            RunDatabaseValidation(database, statusMessage, query, failureMessage);
        }

        /// <summary>
        /// Verifies that output measurements exist in the measurement table. Power calculations are disabled where the configured outputs do not exist.
        /// </summary>
        /// <param name="database">Configuration database connection</param>
        /// <param name="nodeIDQueryString">Node ID formatted for query</param>
        /// <param name="statusMessage">Delegate to communicate status updates</param>
        private static void CheckOutputMeasurementsExist(AdoDataConnection database, string nodeIDQueryString, Action<string> statusMessage)
        {
            statusMessage("Validating if non-null output measurements exist...");

            string query = 
                $"UPDATE PowerCalculation SET Enabled=0 WHERE Enabled=1 AND NodeID={nodeIDQueryString} " + 
                "AND ((ActivePowerOutputSignalID IS NOT NULL AND ActivePowerOutputSignalID NOT IN (SELECT SignalID from Measurement)) " + 
                "OR (ReactivePowerOutputSignalID IS NOT NULL AND ReactivePowerOutputSignalID NOT IN (SELECT SignalID from Measurement)) " + 
                "OR (ApparentPowerOutputSignalID IS NOT NULL AND ApparentPowerOutputSignalID NOT IN (SELECT SignalID from Measurement)))";

            const string failureMessage = "One or more power calculations are defined with non-null output measurements that do not exist. Power calculations will be disabled.";

            RunDatabaseValidation(database, statusMessage, query, failureMessage);
        }

        /// <summary>
        /// Validates that input measurements have the correct signal type. Power calculations are disabled where inputs have the wrong signal types.
        /// </summary>
        /// <param name="database">Configuration database connection</param>
        /// <param name="nodeIDQueryString">Node ID formatted for database query</param>
        /// <param name="statusMessage">Delegate for status updates</param>
        private static void CheckInputMeasurementsSignalType(AdoDataConnection database, string nodeIDQueryString, Action<string> statusMessage)
        {
            statusMessage("Validating if input measurements have correct signal type...");

            string query =
                $"UPDATE PowerCalculation " +
                $"SET Enabled = 0 " +
                $"WHERE " +
                $"    Enabled = 1 AND " +
                $"    NodeID = {nodeIDQueryString} AND " +
                $"    ( " +
                $"        (SELECT Acronym FROM Measurement JOIN SignalType ON Measurement.SignalTypeID = SignalType.ID WHERE SignalID = VoltageAngleSignalID) <> 'VPHA' OR " +
                $"        (SELECT Acronym FROM Measurement JOIN SignalType ON Measurement.SignalTypeID = SignalType.ID WHERE SignalID = VoltageMagSignalID) <> 'VPHM' OR " +
                $"        (SELECT Acronym FROM Measurement JOIN SignalType ON Measurement.SignalTypeID = SignalType.ID WHERE SignalID = CurrentAngleSignalID) <> 'IPHA' OR " +
                $"        (SELECT Acronym FROM Measurement JOIN SignalType ON Measurement.SignalTypeID = SignalType.ID WHERE SignalID = CurrentMagSignalID) <> 'IPHM' " +
                $"    )";

            const string failureMessage = "One or more power calculations are defined with input measurements having the wrong signal type. Power calculations will be disabled.";

            RunDatabaseValidation(database, statusMessage, query, failureMessage);
        }

        /// <summary>
        /// Verifies that input measurements are enabled. Power calculations are disabled where input measurements are disabled.
        /// </summary>
        /// <param name="database">Configuration database connection</param>
        /// <param name="nodeIDQueryString">Node ID formatted for query</param>
        /// <param name="statusMessage">Delegate for communicating status updates</param>
        private static void CheckInputMeasurementsAreEnabled(AdoDataConnection database, string nodeIDQueryString, Action<string> statusMessage)
        {
            statusMessage("Validating if input measurements are enabled...");

            string query =
                $"UPDATE PowerCalculation " +
                $"SET Enabled = 0 " +
                $"WHERE " +
                $"    Enabled = 1 AND " +
                $"    NodeID = {nodeIDQueryString} AND " +
                $"    (SELECT COUNT(*) FROM Measurement WHERE Enabled = 0 AND SignalID IN (VoltageAngleSignalID, VoltageMagSignalID, CurrentAngleSignalID, CurrentMagSignalID)) > 0";

            const string failureMessage = "One or more power calculations are defined with input measurements that are disabled. Power calculations will be disabled.";

            RunDatabaseValidation(database, statusMessage, query, failureMessage);
        }

        /// <summary>
        /// Verifies that input measurements exist. Power calculations are disabled where input measurements do not exist.
        /// </summary>
        /// <param name="database">Configuration database connection</param>
        /// <param name="nodeIDQueryString">Node ID formatted for query</param>
        /// <param name="statusMessage">Delegate for communicating status updates</param>
        private static void CheckInputMeasurementsExist(AdoDataConnection database, string nodeIDQueryString, Action<string> statusMessage)
        {
            statusMessage("Validating if input measurements exist...");

            string query = 
                $"UPDATE PowerCalculation SET Enabled=0 WHERE Enabled=1 AND NodeID={nodeIDQueryString}" + 
                "AND (VoltageMagSignalID NOT IN (SELECT SignalID from Measurement) " + 
                "OR VoltageAngleSignalID NOT IN (SELECT SignalID from Measurement) " + 
                "OR CurrentMagSignalID NOT IN (SELECT SignalID from Measurement) " + 
                "OR CurrentAngleSignalID NOT IN (SELECT SignalID from Measurement))";

            const string failureMessage = "One or more power calculations are defined with input measurements that do not exist. Power calculations will be disabled.";

            RunDatabaseValidation(database, statusMessage, query, failureMessage);
        }

        [SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities", Justification = "Code has been evaluated, functions calling this method create SQL text that is not based on user input.")]
        private static void RunDatabaseValidation(AdoDataConnection database, Action<string> statusMessage, string query, string failureMessage)
        {
            using (IDbCommand cmd = database.Connection.CreateCommand())
            {
                cmd.CommandText = query;

                int affectedRows = cmd.ExecuteNonQuery();

                if (affectedRows > 0)
                    statusMessage(failureMessage);
            }
        }

        #endregion
    }
}
