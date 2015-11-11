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
using GSF;
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
        /// Returns true if a data operation exists to run this class. Returns false otherwise.
        /// </summary>
        /// <param name="database">Database connection to use for checking the data operation</param>
        /// <returns>True or false indicating whether the operation exists</returns>
        public static bool CheckDataOperationExists(AdoDataConnection database)
        {
            bool dataOperationExists;
            using (var cmd = database.Connection.CreateCommand())
            {
                cmd.CommandText = string.Format("SELECT count(*) FROM DataOperation WHERE TypeName='{0}' AND MethodName='ValidatePowerCalculationConfigurations'", typeof(PowerCalculationConfigurationValidation).FullName);
                dataOperationExists = Convert.ToInt32(cmd.ExecuteScalar()) > 0;
            }

            return dataOperationExists;
        }

        /// <summary>
        /// Creates a data operation to run the validations in this class.
        /// </summary>
        /// <param name="database">Database connection to use for creating the data operation</param>
        public static void CreateDataOperation(AdoDataConnection database)
        {
            using (var cmd = database.Connection.CreateCommand())
            {
                cmd.CommandText = string.Format("INSERT INTO DataOperation (Description, AssemblyName, TypeName, MethodName, Enabled) VALUES (" +
                                                "'Power Calculation Validations', 'PowerCalculations.dll', '{0}', 'ValidatePowerCalculationConfigurations', 1)", typeof(PowerCalculationConfigurationValidation).FullName);
                cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Data operation to validate power calculation configuration. This method checks that input measurements and non-null output measurements exist, are enabled, and have the correct signal type.
        /// If null output measurements are found in the configuration, this method will create the output measurements and update the configuration.
        /// </summary>
        /// <param name="database">Database connection for configuration information</param>
        /// <param name="nodeIdQueryString">Node ID for database queries</param>
        /// <param name="trackingVersion">Not used</param>
        /// <param name="arguments">Not used</param>
        /// <param name="statusMessage">Delegate for method to communicate status updates</param>
        /// <param name="processException">Exception handling delegate</param>
        public static void ValidatePowerCalculationConfigurations(AdoDataConnection database, string nodeIdQueryString, ulong trackingVersion, string arguments, Action<string> statusMessage, Action<Exception> processException)
        {
            try
            {
                CheckInputMeasurementsExist(database, nodeIdQueryString, statusMessage);
                CheckInputMeasurementsAreEnabled(database, nodeIdQueryString, statusMessage);
                CheckInputMeasurementsSignalType(database, nodeIdQueryString, statusMessage);
                CheckOutputMeasurementsExist(database, nodeIdQueryString, statusMessage);
                CheckOutputMeasurementsAreEnabled(database, nodeIdQueryString, statusMessage);
                CheckOutputMeasurementsSignalType(database, nodeIdQueryString, statusMessage);
                CreateOutputMeasurementsWhereNull(database, nodeIdQueryString, statusMessage);
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
        /// <param name="nodeIdQueryString">Node ID formatted for query</param>
        /// <param name="statusMessage">Delegate for communicating status updates</param>
        private static void CreateOutputMeasurementsWhereNull(AdoDataConnection database, string nodeIdQueryString, Action<string> statusMessage)
        {
            statusMessage("Checking for calculations with null output measurements...");

            var query = string.Format(
                    //      0                      1                      2                           3                             4                               5                            6                           7                            8                 9
                    "SELECT pc.PowerCalculationId, pc.CircuitDescription, pc.RealPowerOutputSignalId, pc.ActivePowerOutputSignalId, pc.ReactivePowerOutputSignalId, v.Acronym as vendoracronym,  d.Acronym as deviceacronym, c.Acronym as companyacronym, d.id as deviceid, d.historianid as historianid, " +
           /* 10 */ "(SELECT p.Label FROM Phasor p JOIN Measurement m on m.SignalID = pc.currentanglesignalid and m.DeviceID = p.DeviceID AND m.PhasorSourceIndex = p.SourceIndex) as currentLabel " +
                    "FROM PowerCalculation pc " +
                    "join measurement m on m.SignalID = pc.voltageanglesignalid " +
                    "left outer join device d on m.deviceid = d.id " +
                    "left outer join VendorDevice vd on vd.id = d.VendorDeviceID " +
                    "left outer join vendor v on vd.VendorID = v.id " +
                    "left outer join company c on d.CompanyID = c.id " +
                    "WHERE pc.CalculationEnabled = 1 and pc.nodeid={0}  AND (pc.RealPowerOutputSignalId IS NULL OR pc.ReactivePowerOutputSignalId IS NULL OR pc.ActivePowerOutputSignalId IS NULL)", 
                    nodeIdQueryString);

            var realPowerUpdates = new Dictionary<int, Measurement>();
            var reactivePowerUpdates = new Dictionary<int, Measurement>();
            var apparentPowerUpdates = new Dictionary<int, Measurement>();

            using (var cmd = database.Connection.CreateCommand())
            {
                cmd.CommandText = query;
                using (var rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        var powerCalculationId = rdr.GetInt32(0);
                        var companyAcronym = rdr.IsDBNull(7) ? "" : rdr.GetString(7);
                        var vendorAcronym = rdr.IsDBNull(5) ? "" : rdr.GetString(5);
                        var signalTypeAcronym = "CALC";
                        var circuitDescription = rdr.IsDBNull(1) ? "" : rdr.GetString(1);
                        var deviceId = rdr.GetInt32(8);
                        int? historianId = rdr.IsDBNull(9) ? null : (int?)rdr.GetInt32(9);

                        if (rdr.IsDBNull(2)) // Real - MW
                        {
                            // create real power output measurement
                            var measurement = CreateMeasurement(companyAcronym, circuitDescription + "-MW", vendorAcronym, signalTypeAcronym, circuitDescription, deviceId, historianId, "Real Power Calculation");
                            realPowerUpdates.Add(powerCalculationId, measurement);
                        }

                        if (rdr.IsDBNull(3)) // Apparent - MVA
                        {
                            // create active power output measurement
                            var measurement = CreateMeasurement(companyAcronym, circuitDescription + "-MVA", vendorAcronym, signalTypeAcronym, circuitDescription, deviceId, historianId, "Apparent Power Calculation");
                            apparentPowerUpdates.Add(powerCalculationId, measurement);
                        }

                        if (rdr.IsDBNull(4)) // Reactive - MVAR
                        {
                            //create reactive power output measurement
                            var measurement = CreateMeasurement(companyAcronym, circuitDescription + "-MVAR", vendorAcronym, signalTypeAcronym, circuitDescription, deviceId, historianId, "Reactive Power Calculation");
                            reactivePowerUpdates.Add(powerCalculationId, measurement);
                        }
                    }
                }
            }

            var newMeasurementsCount = realPowerUpdates.Count + reactivePowerUpdates.Count + apparentPowerUpdates.Count;

            if (newMeasurementsCount > 0)
            {
                var repo = new MeasurementRepository();
                statusMessage(string.Format("Creating {0} new output measurements for power calculation...", newMeasurementsCount));

                foreach (var update in realPowerUpdates)
                {
                    repo.Save(database, update.Value);
                    UpdatePowerCalculation(database, update.Key, realPowerOutputSignalId: update.Value.SignalId);
                }

                statusMessage("Successfully created new real power calculations.");

                foreach (var update in reactivePowerUpdates)
                {
                    repo.Save(database, update.Value);
                    UpdatePowerCalculation(database, update.Key, reactivePowerOutputSignalId: update.Value.SignalId);
                }

                statusMessage("Successfully created new reactive power calculations.");

                foreach (var update in apparentPowerUpdates)
                {
                    repo.Save(database, update.Value);
                    UpdatePowerCalculation(database, update.Key, activePowerOutputSignalId: update.Value.SignalId);
                }

                statusMessage("Successfully created new apparent power calculations.");

                statusMessage("Completed creation of new measurements for null output measurements on power calculations.");
            }
        }

        /// <summary>
        /// Creates a new measurement object for power calculation output measurements
        /// </summary>
        private static Measurement CreateMeasurement(string companyAcronym, string deviceAcronym, string vendorAcronym, string signalTypeAcronym, string circuitDescription, int deviceId, int? historianId, string descriptionSuffix)
        {
            var measurement = new Measurement
            {
                PointTag = CommonPhasorServices.CreatePointTag(companyAcronym, deviceAcronym, vendorAcronym, signalTypeAcronym),
                Adder = 0,
                Multiplier = 1,
                Description = string.Format("{0} {1}", circuitDescription, descriptionSuffix),
                DeviceId = deviceId,
                HistorianId = historianId,
                SignalTypeId = 10,
                Enabled = true,
                SignalId = Guid.Empty
            };

            if (measurement.PointTag != null)
            {
                var beginIndex = measurement.PointTag.IndexOf('_');
                measurement.SignalReference = measurement.PointTag.Substring(beginIndex + 1);
            }

            return measurement;
        }

        /// <summary>
        /// Updated power calculation to use new Signal IDs for output measurement
        /// </summary>
        /// <param name="database">Configuration database connection</param>
        /// <param name="powerCalculationId">ID of the power calculation to be updated</param>
        /// <param name="realPowerOutputSignalId">Real power output signal ID, if needed for update</param>
        /// <param name="reactivePowerOutputSignalId">Reactive power output signal ID, if needed for update</param>
        /// <param name="activePowerOutputSignalId">Active power output signal ID, if needed for update</param>
        private static void UpdatePowerCalculation(AdoDataConnection database, int powerCalculationId, Guid? realPowerOutputSignalId = null, Guid? reactivePowerOutputSignalId = null, Guid? activePowerOutputSignalId = null)
        {
            if (realPowerOutputSignalId == null && reactivePowerOutputSignalId == null && activePowerOutputSignalId == null)
                return;

            var updates = new List<string>();
            if (realPowerOutputSignalId != null)
                updates.Add(string.Format("RealPowerOutputSignalId='{0}'", realPowerOutputSignalId.Value));
            if (reactivePowerOutputSignalId != null)
                updates.Add(string.Format("ReactivePowerOutputSignalId='{0}'", reactivePowerOutputSignalId.Value));
            if (activePowerOutputSignalId != null)
                updates.Add(string.Format("ActivePowerOutputSignalId='{0}'", activePowerOutputSignalId.Value));

            var query = "UPDATE PowerCalculation SET ";
            query += string.Join(", ", updates) + " ";
            query += string.Format("WHERE PowerCalculationId={0} ", powerCalculationId);

            using (var cmd = database.Connection.CreateCommand())
            {
                cmd.CommandText = query;
                cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Verifies that output measurements are set to calculated signal type. Calculations are disabled if output measurements are not configured correctly.
        /// </summary>
        /// <param name="database">Configuration database connection</param>
        /// <param name="nodeIdQueryString">Node ID formatted for query</param>
        /// <param name="statusMessage">Delegate to communicate status updates</param>
        private static void CheckOutputMeasurementsSignalType(AdoDataConnection database, string nodeIdQueryString, Action<string> statusMessage)
        {
            statusMessage("Validating if non-null output measurements have correct signal type...");
            var query = string.Format("UPDATE PowerCalculation SET CalculationEnabled=0 WHERE PowerCalculationId IN (select PowerCalculationId " +
                                            "from PowerCalculation pc " +
                                            "left join measurement m1 " +
                                            "on pc.realpoweroutputsignalid = m1.signalid " +
                                            "left join measurement m2 " +
                                            "on pc.reactivepoweroutputsignalid = m2.signalid " +
                                            "left join Measurement m3 " +
                                            "on pc.activepoweroutputsignalid = m3.signalid " +
                                            "where pc.CalculationEnabled=1 AND pc.NodeId={0} " +
                                            "  and (m1.signaltypeid != 10  " +
                                            "   or m2.signaltypeid != 10  " +
                                            "   or m3.signaltypeid != 10))", nodeIdQueryString);

            const string failureMessage = "One or more power calculations are defined with non-null output measurements having the wrong signal type. Power calculations will be disabled.";
            RunDbValidation(database, statusMessage, query, failureMessage);
        }

        /// <summary>
        /// Verifies that output measurements are enabled. Power calculations will be disabled where outputs are disabled.
        /// </summary>
        /// <param name="database"></param>
        /// <param name="nodeIdQueryString"></param>
        /// <param name="statusMessage"></param>
        private static void CheckOutputMeasurementsAreEnabled(AdoDataConnection database, string nodeIdQueryString, Action<string> statusMessage)
        {
            statusMessage("Validating if non-null output measurements are enabled...");
            var query = string.Format("UPDATE PowerCalculation SET CalculationEnabled=0 WHERE PowerCalculationId IN (select PowerCalculationId " +
                                            "from PowerCalculation pc " +
                                            "left join measurement m1 " +
                                            "on pc.realpoweroutputsignalid = m1.signalid " +
                                            "left join measurement m2 " +
                                            "on pc.reactivepoweroutputsignalid = m2.signalid " +
                                            "left join Measurement m3 " +
                                            "on pc.activepoweroutputsignalid = m3.signalid " +
                                            "where pc.CalculationEnabled=1 AND pc.NodeId={0} " +
                                            "  and (m1.enabled=0  " +
                                            "   or m2.enabled=0  " +
                                            "   or m3.enabled=0))", nodeIdQueryString);

            const string failureMessage = "One or more power calculations are defined with output measurements that are disabled. Power calculations will be disabled.";
            RunDbValidation(database, statusMessage, query, failureMessage);
        }

        /// <summary>
        /// Verifies that output measurements exist in the measurement table. Power calculations are disabled where the configured outputs do not exist.
        /// </summary>
        /// <param name="database">Configuration database connection</param>
        /// <param name="nodeIdQueryString">Node ID formatted for query</param>
        /// <param name="statusMessage">Delegate to communicate status updates</param>
        private static void CheckOutputMeasurementsExist(AdoDataConnection database, string nodeIdQueryString, Action<string> statusMessage)
        {
            statusMessage("Validating if non-null output measurements exist...");
            var query = string.Format(
                "UPDATE PowerCalculation SET CalculationEnabled=0 WHERE CalculationEnabled=1 AND NodeId={0}" +
                "AND ((RealPowerOutputSignalId IS NOT NULL AND RealPowerOutputSignalId NOT IN (SELECT SignalId from Measurement)) " +
                "OR (ReactivePowerOutputSignalId IS NOT NULL AND ReactivePowerOutputSignalId NOT IN (SELECT SignalId from Measurement)) " +
                "OR (ActivePowerOutputSignalId IS NOT NULL AND ActivePowerOutputSignalId NOT IN (SELECT SignalId from Measurement)))", nodeIdQueryString);

            const string failureMessage = "One or more power calculations are defined with non-null output measurements that do not exist. Power calculations will be disabled.";
            RunDbValidation(database, statusMessage, query, failureMessage);

        }

        /// <summary>
        /// Validates that input measurements have the correct signal type. Power calculations are disabled where inputs have the wrong signal types.
        /// </summary>
        /// <param name="database">Configuration database connection</param>
        /// <param name="nodeIdQueryString">Node ID formatted for database query</param>
        /// <param name="statusMessage">Delegate for status updates</param>
        private static void CheckInputMeasurementsSignalType(AdoDataConnection database, string nodeIdQueryString, Action<string> statusMessage)
        {
            statusMessage("Validating if input measurements have correct signal type...");
            var query = string.Format("UPDATE PowerCalculation SET CalculationEnabled=0 WHERE PowerCalculationId IN (select PowerCalculationId " +
                                            "from PowerCalculation pc " +
                                            "left join measurement m1 " +
                                            "on pc.voltageanglesignalid = m1.signalid " +
                                            "left join measurement m2 " +
                                            "on pc.VoltageMagSignalId = m2.signalid " +
                                            "left join Measurement m3 " +
                                            "on pc.CurrentAngleSignalId = m3.signalid " +
                                            "left join measurement m4 " +
                                            "on pc.CurrentMagSignalId = m4.signalid " +
                                            "where pc.CalculationEnabled=1 AND pc.NodeId={0} " +
                                            "  and (m1.signaltypeid != 4  " +
                                            "   or m2.signaltypeid != 3  " +
                                            "   or m3.signaltypeid != 2  " +
                                            "   or m4.signaltypeid != 1))", nodeIdQueryString);

            const string failureMessage = "One or more power calculations are defined with input measurements having the wrong signal type. Power calculations will be disabled.";
            RunDbValidation(database, statusMessage, query, failureMessage);
        }

        /// <summary>
        /// Verifies that input measurements are enabled. Power calculations are disabled where input measurements are disabled.
        /// </summary>
        /// <param name="database">Configuration database connection</param>
        /// <param name="nodeIdQueryString">Node ID formatted for query</param>
        /// <param name="statusMessage">Delegate for communicating status updates</param>
        private static void CheckInputMeasurementsAreEnabled(AdoDataConnection database, string nodeIdQueryString, Action<string> statusMessage)
        {
            statusMessage("Validating if input measurements are enabled...");
            var query = string.Format("UPDATE PowerCalculation SET CalculationEnabled=0 WHERE PowerCalculationId IN (select pc.PowerCalculationId " +
                                            "from PowerCalculation pc " +
                                            "left join measurement m1 " +
                                            "on pc.voltageanglesignalid = m1.signalid " +
                                            "left join measurement m2 " +
                                            "on pc.VoltageMagSignalId = m2.signalid " +
                                            "left join Measurement m3 " +
                                            "on pc.CurrentAngleSignalId = m3.signalid " +
                                            "left join measurement m4 " +
                                            "on pc.CurrentMagSignalId = m4.signalid " +
                                            "where pc.CalculationEnabled=1 AND pc.NodeId={0} " +
                                            "  and (m1.enabled=0  " +
                                            "   or m2.enabled=0  " +
                                            "   or m3.enabled=0  " +
                                            "   or m4.enabled=0))", nodeIdQueryString);

            const string failureMessage = "One or more power calculations are defined with input measurements that are disabled. Power calculations will be disabled.";
            RunDbValidation(database, statusMessage, query, failureMessage);
        }

        /// <summary>
        /// Verifies that input measurements exist. Power calculations are disabled where input measurements do not exist.
        /// </summary>
        /// <param name="database">Configuration database connection</param>
        /// <param name="nodeIdQueryString">Node ID formatted for query</param>
        /// <param name="statusMessage">Delegate for communicating status updates</param>
        private static void CheckInputMeasurementsExist(AdoDataConnection database, string nodeIdQueryString, Action<string> statusMessage)
        {
            statusMessage("Validating if input measurements exist...");
            var query = string.Format(
                "UPDATE PowerCalculation SET CalculationEnabled=0 WHERE CalculationEnabled=1 AND NodeId={0}" +
                "AND (VoltageMagSignalId NOT IN (SELECT SignalId from Measurement) " +
                "OR VoltageAngleSignalId NOT IN (SELECT SignalId from Measurement) " +
                "OR CurrentMagSignalId NOT IN (SELECT SignalId from Measurement) " +
                "OR CurrentAngleSignalId NOT IN (SELECT SignalId from Measurement))", nodeIdQueryString);

            const string failureMessage = "One or more power calculations are defined with input measurements that do not exist. Power calculations will be disabled.";
            RunDbValidation(database, statusMessage, query, failureMessage);
        }

        private static void RunDbValidation(AdoDataConnection database, Action<string> statusMessage, string query, string failureMessage)
        {
            using (var cmd = database.Connection.CreateCommand())
            {
                cmd.CommandText = query;
                var affectedRows = cmd.ExecuteNonQuery();
                if (affectedRows > 0)
                {
                    statusMessage(failureMessage);
                }
            }
        }

        #endregion
    }
}
