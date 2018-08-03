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
using System.Text;
using GSF;
using GSF.Data;
using GSF.Diagnostics;
using GSF.Parsing;
using GSF.Threading;
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
        private const string DefaultDatabaseProviderString = "AssemblyName={System.Data, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089}; ConnectionType=System.Data.SqlClient.SqlConnection; AdapterType=System.Data.SqlClient.SqlDataAdapter";
        private const string DefaultDatabaseCommand = "sp_LogSsamEvent";
        private const string DefaultDatabaseCommandParameters = "1,1,'FL_PMU_{Acronym}_HEARTBEAT','','{Acronym} adapter heartbeat at {Timestamp} UTC',''";

        // Fields
        private ShortSynchronizedOperation m_databaseOperation;
        private long m_expressionSuccesses;
        private long m_expressionFailures;
        private long m_totalDatabaseOperations;

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the boolean expression used to determine if the database operation should be executed.
        /// </summary>
        [ConnectionStringParameter]
        [Description("Define the boolean expression used to determine if the database operation should be executed.")]
        public new string ExpressionText // Redeclared to provide a more relevant description for this adapter
        {
            get => base.ExpressionText;
            set => base.ExpressionText = value;
        }

        /// <summary>
        /// Gets or sets the connection string used for database operation.
        /// </summary>
        [ConnectionStringParameter]
        [Description("Defines the connection string used for database operation.")]
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
        /// Gets or sets the source of the timestamps of the calculated values.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new TimestampSource TimestampSource // Redeclared to hide property - not relevant to this adapter
        {
            get => base.TimestampSource;
            set => base.TimestampSource = value;
        }

        /// <summary>
        /// Returns the detailed status of the data input source.
        /// </summary>
        public override string Status
        {
            get
            {
                StringBuilder status = new StringBuilder();

                status.AppendFormat("      Expression Successes: {0:N0}", m_expressionSuccesses);
                status.AppendLine();
                status.AppendFormat("       Expression Failures: {0:N0}", m_expressionFailures);
                status.AppendLine();
                status.AppendFormat(" Total Database Operations: {0:N0}", m_totalDatabaseOperations);
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
            const string MissingRequiredDatabaseSetting = "Missing required database setting: \"{0}\"";

            base.Initialize();

            Dictionary<string, string> settings = Settings;

            // Load required database settings
            if (settings.TryGetValue(nameof(DatabaseConnnectionString), out string setting) && !string.IsNullOrWhiteSpace(setting))
                DatabaseConnnectionString = setting;
            else
                throw new ArgumentException(string.Format(MissingRequiredDatabaseSetting, nameof(DatabaseConnnectionString)));

            // Load optional database settings
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

            // Define synchronized monitoring operation
            m_databaseOperation = new ShortSynchronizedOperation(DatabaseOperation, exception => OnProcessException(MessageLevel.Warning, exception));
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

        private void DatabaseOperation()
        {
            using (AdoDataConnection connection = new AdoDataConnection(DatabaseConnnectionString, DatabaseProviderString))
            {
                TemplatedExpressionParser parameterTemplate = new TemplatedExpressionParser
                {
                    TemplatedExpression = DatabaseCommandParameters
                };

                Dictionary<string, string> substitutions = new Dictionary<string, string>
                {
                    ["Acronym"] = Name,
                    ["Timestamp"] = RealTime.ToString(TimeTagBase.DefaultFormat)
                };

                List<object> parameters = new List<object>();
                string commandParameters = parameterTemplate.Execute(substitutions);
                string[] splitParameters = commandParameters.Split(',');

                // Do some basic typing on command parameters
                foreach (string splitParameter in splitParameters)
                {
                    string parameter = splitParameter.Trim();

                    if (parameter.StartsWith("'") && parameter.EndsWith("'"))
                        parameters.Add(parameter.Length > 2 ? parameter.Substring(1, parameter.Length - 2) : "");
                    else if (int.TryParse(parameter, out int ival))
                        parameters.Add(ival);
                    else if (double.TryParse(parameter, out double dval))
                        parameters.Add(dval);
                    else if (bool.TryParse(parameter, out bool bval))
                        parameters.Add(bval);
                    else
                        parameters.Add(parameter);
                }

                connection.ExecuteScalar(DatabaseCommand, parameters);
            }

            m_totalDatabaseOperations++;
        }

        #endregion
    }
}