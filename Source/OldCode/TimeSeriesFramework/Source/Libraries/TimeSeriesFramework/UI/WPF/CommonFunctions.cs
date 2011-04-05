//******************************************************************************************************
//  CommonFunctions.cs - Gbtc
//
//  Copyright © 2010, Grid Protection Alliance.  All Rights Reserved.
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
//  03/31/2011 - Mehulbhai P Thakkar
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Threading;
using TimeSeriesFramework.Data;
using TimeSeriesFramework.UI.DataModels;

namespace TimeSeriesFramework.UI
{
    public static class CommonFunctions
    {
        #region [ Static ]

        public static string s_currentUser = Thread.CurrentPrincipal.Identity.Name;

        #endregion

        #region [ Common Functions ]

        /// <summary>
        /// Method to add parameter to <see cref="IDbCommand"/> object.
        /// </summary>
        /// <param name="command"><see cref="IDbCommand"/> to which parameter needs to be added.</param>
        /// <param name="name">Name of the <see cref="IDbDataParameter"/> to be added.</param>
        /// <param name="value">Value of the <see cref="IDbDataParameter"/> to be added.</param>
        /// <param name="direction"><see cref="ParameterDirection"/> for <see cref="IDbDataParameter"/>.</param>
        public static void AddParameterWithValue(this IDbCommand command, string name, object value, ParameterDirection direction = ParameterDirection.Input)
        {
            IDbDataParameter param = command.CreateParameter();
            param.ParameterName = name;
            param.Value = value;
            param.Direction = direction;
            command.Parameters.Add(param);
        }

        /// <summary>
        /// Purpose of this method is to supply current user information from the UI to DELETE trigger for change logging.
        /// This method must be called before any delete operation on the database in order to log who deleted this record.
        /// For SQL server it sets user name into CONTEXT_INFO().
        /// For MySQL server it sets user name into session variable @context.
        /// MS Access is not supported for change logging.
        /// For any other database in the future, such as Oracle, this logic must be extended to support change log in the database.
        /// </summary>
        /// <param name="connection">Connection used to set user context before any delete operation.</param>
        public static void SetCurrentUserContext(DataConnection connection)
        {
            bool createdConnection = false;
            try
            {
                if (string.IsNullOrEmpty(s_currentUser))
                    s_currentUser = Thread.CurrentPrincipal.Identity.Name;

                if (!string.IsNullOrEmpty(s_currentUser))
                {
                    if (connection == null)
                    {
                        connection = new DataConnection();
                        createdConnection = true;
                    }
                    IDbCommand command;
                    //First of all set Current User for the database session for this connection.
                    if (connection.Connection.GetType().Name.ToLower() == "sqlconnection")
                    {
                        string contextSql = "DECLARE @context VARBINARY(128)\n SELECT @context = CONVERT(VARBINARY(128), CONVERT(VARCHAR(128), @userName))\n SET CONTEXT_INFO @context";
                        command = connection.Connection.CreateCommand();
                        command.CommandType = CommandType.Text;
                        command.CommandText = contextSql;
                        command.AddParameterWithValue("@userName", s_currentUser);
                        command.ExecuteNonQuery();
                    }
                    else if (connection.Connection.GetType().Name.ToLower() == "mysqlconnection")
                    {
                        command = connection.Connection.CreateCommand();
                        command.CommandType = CommandType.Text;
                        command.CommandText = "SET @context = '" + s_currentUser + "';";
                        command.ExecuteNonQuery();                     
                    }
                }
            }
            finally
            {
                if (createdConnection && connection != null)
                    connection.Dispose();
            }
        }

        #endregion

        #region [ Manage Companies Code ]

        /// <summary>
        /// Method to retrieve a collection of <see cref="Company"/>.
        /// </summary>
        /// <param name="connection"><see cref="DataConnection"/> to connect to database.</param>
        /// <returns>Collection of <see cref="Company"/>.</returns>
        public static ObservableCollection<Company> GetCompanyList(DataConnection connection)
        {   
            bool createdConnection = false;
            try
            {
                if (connection == null)
                {
                    connection = new DataConnection();
                    createdConnection = true;
                }

                List<Company> companyList = new List<Company>();
                IDbCommand command = connection.Connection.CreateCommand();
                command.CommandType = CommandType.Text;
                command.CommandText = "SELECT ID, Acronym, MapAcronym, Name, URL, LoadOrder FROM Company ORDER BY LoadOrder";

                DataTable resultTable = new DataTable();
                resultTable.Load(command.ExecuteReader());

                companyList = (from item in resultTable.AsEnumerable()
                               select new Company()
                               {
                                   ID = item.Field<int>("ID"),
                                   Acronym = item.Field<string>("Acronym"),
                                   MapAcronym = item.Field<string>("MapAcronym"),
                                   Name = item.Field<string>("Name"),
                                   URL = item.Field<string>("URL"),
                                   LoadOrder = item.Field<int>("LoadOrder")
                               }).ToList();

                return new ObservableCollection<Company>(companyList);
            }
            finally
            {
                if (createdConnection && connection != null)
                    connection.Dispose();
            }
        }

        /// <summary>
        /// Method to retrieve ID and Name KeyValuePair collection.
        /// </summary>
        /// <param name="connection"><see cref="DataConnection"/> to connect to database.</param>
        /// <param name="isOptional">Indicates if selection on UI is optional for this collection.</param>
        /// <returns>Dictionary<int, string> containing ID and Name of companies defined in the database.</returns>
        public static Dictionary<int, string> GetCompanies(DataConnection connection, bool isOptional)
        {            
            bool createdConnection = false;
            try
            {
                if (connection == null)
                {
                    connection = new DataConnection();
                    createdConnection = true;
                }

                Dictionary<int, string> companyList = new Dictionary<int, string>();
                if (isOptional)
                    companyList.Add(0, "Select Company");

                IDbCommand command = connection.Connection.CreateCommand();
                command.CommandType = CommandType.Text;
                command.CommandText = "SELECT ID, Name FROM Company ORDER BY LoadOrder";

                DataTable resultTable = new DataTable();
                resultTable.Load(command.ExecuteReader());

                int id;
                foreach (DataRow row in resultTable.Rows)
                {
                    id = int.Parse(row["ID"].ToString());

                    if (!companyList.ContainsKey(id))
                        companyList.Add(id, row["Name"].ToString());
                }
                return companyList;
            }
            finally
            {
                if (createdConnection && connection != null)
                    connection.Dispose();
            }
        }


        /// <summary>
        /// Saves <see cref="Company"/> information into database.
        /// </summary>
        /// <param name="connection"><see cref="DataConnection"/> to connect to database.</param>
        /// <param name="company">Information about <see cref="Company"/>.</param>
        /// <param name="isNew">Indicates if it is a new addition or an update to existing.</param>
        /// <returns>String indicating success.</returns>
        public static string SaveCompany(DataConnection connection, Company company, bool isNew)
        {
            bool createdConnection = false;
            try
            {
                if (connection == null)
                {
                    connection = new DataConnection();
                    createdConnection = true;
                }

                IDbCommand command = connection.Connection.CreateCommand();
                command.CommandType = CommandType.Text;

                if (isNew)
                    command.CommandText = "INSERT INTO Company (Acronym, MapAcronym, Name, URL, LoadOrder, UpdatedBy, UpdatedOn, CreatedBy, CreatedOn) VALUES (@acronym, @mapAcronym, @name, @url, @loadOrder, @updatedBy, @updatedOn, @createdBy, @createdOn)";
                else
                    command.CommandText = "UPDATE Company SET Acronym = @acronym, MapAcronym = @mapAcronym, Name = @name, URL = @url, LoadOrder = @loadOrder, UpdatedBy = @updatedBy, UpdatedOn = @updatedOn WHERE ID = @id";

                command.AddParameterWithValue("@acronym", company.Acronym.Replace(" ", "").ToUpper());
                command.AddParameterWithValue("@mapAcronym", company.MapAcronym.Replace(" ", "").ToUpper());
                command.AddParameterWithValue("@name", company.Name);
                command.AddParameterWithValue("@url", company.URL ?? string.Empty);
                command.AddParameterWithValue("@loadOrder", company.LoadOrder);
                command.AddParameterWithValue("@updatedBy", s_currentUser);
                command.AddParameterWithValue("@updatedOn", command.Connection.ConnectionString.Contains("Microsoft.Jet.OLEDB") ? DateTime.UtcNow.Date : DateTime.UtcNow);

                if (isNew)
                {
                    command.AddParameterWithValue("@createdBy", s_currentUser);
                    command.AddParameterWithValue("@createdOn", command.Connection.ConnectionString.Contains("Microsoft.Jet.OLEDB") ? DateTime.UtcNow.Date : DateTime.UtcNow);
                }
                else
                {
                    command.AddParameterWithValue("@id", company.ID);
                }

                command.ExecuteNonQuery();
                return "Company Information Saved Successfully";
            }
            finally
            {
                if (createdConnection && connection != null)
                    connection.Dispose();
            }
        }

        /// <summary>
        /// Deletes <see cref="Company"/> information from database.
        /// </summary>
        /// <param name="connection"><see cref="DataConnection"/> to connect to database.</param>
        /// <param name="companyID">ID of the record to be deleted.</param>
        /// <returns>String indicating success.</returns>
        public static string DeleteCompany(DataConnection connection, int companyID)
        {
            bool createdConnection = false;
            try
            {
                if (connection == null)
                {
                    connection = new DataConnection();
                    createdConnection = true;
                }

                //Setup current users context for Delete trigger.
                SetCurrentUserContext(connection);

                IDbCommand command = connection.Connection.CreateCommand();
                command.CommandType = CommandType.Text;
                command.CommandText = "Delete From Company Where ID = @companyID";
                command.AddParameterWithValue("@companyID", companyID);
                command.ExecuteNonQuery();

                return "Company Deleted Successfully";
            }
            finally
            {
                if (createdConnection && connection != null)
                    connection.Dispose();
            }
        }

        #endregion
    }
}
