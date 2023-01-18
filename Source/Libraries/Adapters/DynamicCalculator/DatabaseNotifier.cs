//******************************************************************************************************
//  DatabaseNotifier.cs - Gbtc
//
//  Copyright © 2018, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may not use this
//  file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  08/03/2018 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Text;
using GSF;
using GSF.Data;
using GSF.Diagnostics;
using GSF.Parsing;
using GSF.Threading;
using GSF.TimeSeries;
using GSF.TimeSeries.Adapters;

namespace DynamicCalculator
{
    /// <summary>
    /// The DatabaseNotifier is an action adapter which takes multiple input measurements and defines
    /// a boolean expression such that when the expression is true a database operation is triggered.
    /// </summary>
    [Description("Database Notifier: Executes a database operation based on a custom boolean expression")]
    public class DatabaseNotifier : DynamicCalculator
    {
        #region [ Members ]

        // Constants
        private const string DefaultExpressionText = "x > 0";
        private const string DefaultDatabaseConnectionString = "";
        private const string DefaultDatabaseProviderString = "AssemblyName={System.Data, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089}; ConnectionType=System.Data.SqlClient.SqlConnection; AdapterType=System.Data.SqlClient.SqlDataAdapter";
        private const string DefaultDatabaseCommand = "sp_LogSsamEvent";
        private const string DefaultDatabaseCommandParameters = "1,1,'FL_PMU_{Acronym}_HEARTBEAT','','{Acronym} adapter heartbeat at {Timestamp} UTC',''";
        private const double DefaultDatabaseMaximumWriteInterval = DelayedSynchronizedOperation.DefaultDelay / 1000.0D;
        private const int DefaultFramesPerSecond = 30;
        private const double DefaultLagTime = 5.0D;
        private const double DefaultLeadTime = 5.0D;

        // Fields
        private DelayedSynchronizedOperation m_databaseOperation;
        private long m_expressionSuccesses;
        private long m_expressionFailures;
        private long m_totalDatabaseOperations;
        private object m_lastDatabaseOperationResult;

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the boolean expression used to determine if the database operation should be executed.
        /// </summary>
        [ConnectionStringParameter]
        [Description("Defines the boolean expression used to determine if the database operation should be executed.")]
        [DefaultValue(DefaultExpressionText)]
        public new string ExpressionText // Redeclared to provide a more relevant description and example value for this adapter
        {
            get => base.ExpressionText;
            set => base.ExpressionText = value;
        }

        /// <summary>
        /// Gets or sets the connection string used for database operation. Leave blank to use local configuration database defined in "systemSettings".
        /// </summary>
        [ConnectionStringParameter]
        [Description("Defines the connection string used for database operation. Leave blank to use local configuration database defined in \"systemSettings\".")]
        [DefaultValue(DefaultDatabaseConnectionString)]
        public string DatabaseConnnectionString
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the provider string used for database operation. Defaults to a SQL Server provider string.
        /// </summary>
        [ConnectionStringParameter]
        [Description("Defines the provider string used for database operation. Defaults to a SQL Server provider string.")]
        [DefaultValue(DefaultDatabaseProviderString)]
        public string DatabaseProviderString
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the command used for database operation, e.g., a stored procedure name or SQL expression like "INSERT".
        /// </summary>
        [ConnectionStringParameter]
        [Description("Defines the command used for database operation, e.g., a stored procedure name or SQL expression like \"INSERT\".")]
        [DefaultValue(DefaultDatabaseCommand)]
        public string DatabaseCommand
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the parameters for the command that includes any desired value substitutions used for database operation. Available substitutions: {Acronym} and {Timestamp}.
        /// </summary>
        [ConnectionStringParameter]
        [Description("Defines the parameters for the command that includes any desired value substitutions used for database operation. Available substitutions: {Acronym} and {Timestamp}.")]
        [DefaultValue(DefaultDatabaseCommandParameters)]
        public string DatabaseCommandParameters
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the maximum interval, in seconds, at which the adapter can execute database operations. Set to zero for no delay.
        /// </summary>
        [ConnectionStringParameter]
        [Description("Defines the maximum interval, in seconds, at which the adapter can execute database operations. Set to zero for no delay.")]
        [DefaultValue(DefaultDatabaseMaximumWriteInterval)]
        public double DatabaseMaximumWriteInterval
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the number of frames per second.
        /// </summary>
        /// <remarks>
        /// Valid frame rates for a <see cref="ConcentratorBase"/> are greater than 0 frames per second.
        /// </remarks>
        [ConnectionStringParameter]
        [Description("Defines the number of frames per second expected by the adapter.")]
        [DefaultValue(DefaultFramesPerSecond)]
        public new int FramesPerSecond // Redeclared to provide a default value since property is not commonly used
        {
            get => base.FramesPerSecond;
            set => base.FramesPerSecond = value;
        }

        /// <summary>
        /// Gets or sets the allowed past time deviation tolerance, in seconds (can be sub-second).
        /// </summary>
        /// <remarks>
        /// <para>Defines the time sensitivity to past measurement timestamps.</para>
        /// <para>The number of seconds allowed before assuming a measurement timestamp is too old.</para>
        /// <para>This becomes the amount of delay introduced by the concentrator to allow time for data to flow into the system.</para>
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException">LagTime must be greater than zero, but it can be less than one.</exception>
        [ConnectionStringParameter]
        [Description("Defines the allowed past time deviation tolerance, in seconds (can be sub-second).")]
        [DefaultValue(DefaultLagTime)]
        public new double LagTime // Redeclared to provide a default value since property is not commonly used
        {
            get => base.LagTime;
            set => base.LagTime = value;
        }

        /// <summary>
        /// Gets or sets the allowed future time deviation tolerance, in seconds (can be sub-second).
        /// </summary>
        /// <remarks>
        /// <para>Defines the time sensitivity to future measurement timestamps.</para>
        /// <para>The number of seconds allowed before assuming a measurement timestamp is too advanced.</para>
        /// <para>This becomes the tolerated +/- accuracy of the local clock to real-time.</para>
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException">LeadTime must be greater than zero, but it can be less than one.</exception>
        [ConnectionStringParameter]
        [Description("Defines the allowed future time deviation tolerance, in seconds (can be sub-second).")]
        [DefaultValue(DefaultLeadTime)]
        public new double LeadTime // Redeclared to provide a default value since property is not commonly used
        {
            get => base.LeadTime;
            set => base.LeadTime = value;
        }

        /// <summary>
        /// Gets or sets output measurements that the action adapter will produce, if any.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override IMeasurement[] OutputMeasurements // Redeclared to hide property - not relevant to this adapter
        {
            get => base.OutputMeasurements;
            set => base.OutputMeasurements = value;
        }

        /// <summary>
        /// Gets or sets the source of the timestamps of the calculated values.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new TimestampSource TimestampSource // Redeclared to hide property - not relevant to this adapter
        {
            get => base.TimestampSource;
            set => base.TimestampSource = value;
        }

        /// <summary>
        /// Gets flag that determines if the implementation of the <see cref="DynamicCalculator"/> requires an output measurement.
        /// </summary>
        protected override bool ExpectsOutputMeasurement => false;

        /// <summary>
        /// Returns the detailed status of the data input source.
        /// </summary>
        public override string Status
        {
            get
            {
                StringBuilder status = new();

                status.Append(base.Status);
                status.AppendLine();
                status.AppendFormat("      Expression Successes: {0:N0}", m_expressionSuccesses);
                status.AppendLine();
                status.AppendFormat("       Expression Failures: {0:N0}", m_expressionFailures);
                status.AppendLine();
                status.AppendFormat(" Total Database Operations: {0:N0}", m_totalDatabaseOperations);
                status.AppendLine();
                status.AppendFormat("  Last DB Operation Result: {0}", m_lastDatabaseOperationResult?.ToString() ?? "null");
                status.AppendLine();

                return status.ToString();
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Initializes <see cref="DatabaseNotifier"/>.
        /// </summary>
        public override void Initialize()
        {
            Dictionary<string, string> settings = Settings;

            if (!settings.TryGetValue(nameof(ExpressionText), out _))
                settings[nameof(ExpressionText)] = DefaultExpressionText;

            if (!settings.TryGetValue(nameof(FramesPerSecond), out _))
                settings[nameof(FramesPerSecond)] = DefaultFramesPerSecond.ToString();

            if (!settings.TryGetValue(nameof(LagTime), out _))
                settings[nameof(LagTime)] = DefaultLagTime.ToString();

            if (!settings.TryGetValue(nameof(LeadTime), out _))
                settings[nameof(LeadTime)] = DefaultLeadTime.ToString();

            base.Initialize();

            // Load optional database settings
            if (settings.TryGetValue(nameof(DatabaseConnnectionString), out string setting) && !string.IsNullOrWhiteSpace(setting))
                DatabaseConnnectionString = setting;
            else
                DatabaseConnnectionString = DefaultDatabaseConnectionString;

            if (settings.TryGetValue(nameof(DatabaseProviderString), out setting) && !string.IsNullOrWhiteSpace(setting))
                DatabaseProviderString = setting;
            else
                DatabaseProviderString = DefaultDatabaseProviderString;

            if (settings.TryGetValue(nameof(DatabaseCommand), out setting) && !string.IsNullOrWhiteSpace(setting))
                DatabaseCommand = setting;
            else
                DatabaseCommand = DefaultDatabaseCommand;

            if (settings.TryGetValue(nameof(DatabaseCommandParameters), out setting) && !string.IsNullOrWhiteSpace(setting))
                DatabaseCommandParameters = setting;
            else
                DatabaseCommandParameters = DefaultDatabaseCommandParameters;

            if (settings.TryGetValue(nameof(DatabaseMaximumWriteInterval), out setting) && double.TryParse(setting, out double interval))
                DatabaseMaximumWriteInterval = interval;
            else
                DatabaseMaximumWriteInterval = DefaultDatabaseMaximumWriteInterval;

            // Define synchronized monitoring operation
            m_databaseOperation = new DelayedSynchronizedOperation(DatabaseOperation, exception => OnProcessException(MessageLevel.Warning, exception))
            {
                Delay = (int)(DatabaseMaximumWriteInterval * 1000.0D)
            };
        }

        /// <summary>
        /// Handler for the values calculated by the <see cref="DynamicCalculator"/>.
        /// </summary>
        /// <param name="value">The value calculated by the <see cref="DynamicCalculator"/>.</param>
        protected override void HandleCalculatedValue(object value)
        {
            if (value.ToString().ParseBoolean())
            {
                m_expressionSuccesses++;
                m_databaseOperation.RunOnceAsync();
            }
            else
            {
                m_expressionFailures++;
            }
        }

        /// <summary>
        /// Queues database operation for execution. Operation will execute immediately if not already running.
        /// </summary>
        [AdapterCommand("Queues database operation for execution. Operation will execute immediately if not already running.", "Administrator", "Editor")]
        public void QueueOperation() => m_databaseOperation?.RunOnce();

        private void DatabaseOperation()
        {
            using (AdoDataConnection connection = string.IsNullOrWhiteSpace(DatabaseConnnectionString) ? new AdoDataConnection("systemSettings") : new AdoDataConnection(DatabaseConnnectionString, DatabaseProviderString))
            {
                TemplatedExpressionParser parameterTemplate = new()
                {
                    TemplatedExpression = DatabaseCommandParameters
                };

                Dictionary<string, string> substitutions = new()
                {
                    ["{Acronym}"] = Name,
                    ["{Timestamp}"] = RealTime.ToString(TimeTagBase.DefaultFormat)
                };

                List<object> parameters = new();
                string[] commandParameters = parameterTemplate.Execute(substitutions).Split(',');

                // Do some basic typing on command parameters
                foreach (string commandParameter in commandParameters)
                {
                    string parameter = commandParameter.Trim();

                    if (parameter.StartsWith("'") && parameter.EndsWith("'"))
                        parameters.Add(parameter.Length > 2 ? parameter.Substring(1, parameter.Length - 2) : "");
                    else if (int.TryParse(parameter, out int ival))
                        parameters.Add(ival);
                    else if (double.TryParse(parameter, out double dval))
                        parameters.Add(dval);
                    else if (bool.TryParse(parameter, out bool bval))
                        parameters.Add(bval);
                    else if (DateTime.TryParseExact(parameter, TimeTagBase.DefaultFormat, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out DateTime dtval))
                        parameters.Add(dtval);
                    else if (DateTime.TryParse(parameter, out dtval))
                        parameters.Add(dtval);
                    else
                        parameters.Add(parameter);
                }

                m_lastDatabaseOperationResult = connection.ExecuteScalar(DatabaseCommand, parameters.ToArray());
            }

            m_totalDatabaseOperations++;
        }

        #endregion
    }
}