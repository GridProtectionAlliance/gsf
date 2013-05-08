//******************************************************************************************************
//  IaonTree.cs - Gbtc
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
//  06/13/2011 - Magdiel Lorenzo
//       Generated original version of source code.
//  12/20/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using GSF.Data;

namespace GSF.TimeSeries.UI.DataModels
{
    /// <summary>
    /// Represents a record of <see cref="IaonTree"/> information as defined in the database.
    /// </summary>
    public class IaonTree : DataModelBase
    {
        #region [ Members ]

        private string m_adapterType;
        private ObservableCollection<Adapter> m_adapterList;

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets the adapter type.
        /// </summary>
        public string AdapterType
        {
            get
            {
                return m_adapterType;
            }
        }

        /// <summary>
        /// Gets the list of adapters associated with this Iaon tree.
        /// </summary>
        public ObservableCollection<Adapter> AdapterList
        {
            get
            {
                return m_adapterList;
            }
        }

        #endregion

        #region [ Static ]

        /// <summary>
        /// Loads <see cref="IaonTree"/> information as an <see cref="ObservableCollection{T}"/> style list.
        /// </summary>        
        /// <param name="database"><see cref="AdoDataConnection"/> to connection to database.</param>     
        /// <returns>Collection of <see cref="IaonTree"/>.</returns>
        public static ObservableCollection<IaonTree> Load(AdoDataConnection database)
        {
            bool createdConnection = false;

            try
            {
                createdConnection = CreateConnection(ref database);
                ObservableCollection<IaonTree> iaonTreeList;
                DataTable rootNodesTable = new DataTable();
                rootNodesTable.Columns.Add(new DataColumn("AdapterType", Type.GetType("System.String")));

                DataRow row;
                row = rootNodesTable.NewRow();
                row["AdapterType"] = "Input Adapters";
                rootNodesTable.Rows.Add(row);

                row = rootNodesTable.NewRow();
                row["AdapterType"] = "Action Adapters";
                rootNodesTable.Rows.Add(row);

                row = rootNodesTable.NewRow();
                row["AdapterType"] = "Output Adapters";
                rootNodesTable.Rows.Add(row);

                DataSet resultSet = new DataSet();
                resultSet.Tables.Add(rootNodesTable);

                DataTable iaonTreeTable = database.Connection.RetrieveData(database.AdapterType, database.ParameterizedQueryString("SELECT * FROM IaonTreeView WHERE NodeID = {0}", "nodeID"), database.CurrentNodeID());
                resultSet.EnforceConstraints = false;
                resultSet.Tables.Add(iaonTreeTable.Copy());
                resultSet.Tables[0].TableName = "RootNodesTable";
                resultSet.Tables[1].TableName = "AdapterData";

                iaonTreeList = new ObservableCollection<IaonTree>(from item in resultSet.Tables["RootNodesTable"].AsEnumerable()
                                                                  select new IaonTree
                                                                      {
                                                                      m_adapterType = item.Field<string>("AdapterType"),
                                                                      m_adapterList = new ObservableCollection<Adapter>(from obj in resultSet.Tables["AdapterData"].AsEnumerable()
                                                                                                                        where obj.Field<string>("AdapterType") == item.Field<string>("AdapterType")
                                                                                                                        select new Adapter
                                                                                                                            {
                                                                                                                            NodeID = database.Guid(obj, "NodeID"),
                                                                                                                            ID = obj.ConvertField<int>("ID"),
                                                                                                                            AdapterName = obj.Field<string>("AdapterName"),
                                                                                                                            AssemblyName = obj.Field<string>("AssemblyName"),
                                                                                                                            TypeName = obj.Field<string>("TypeName"),
                                                                                                                            ConnectionString = obj.Field<string>("ConnectionString")
                                                                                                                        })
                                                                  });

                return iaonTreeList;
            }
            finally
            {
                if (createdConnection && database != null)
                    database.Dispose();
            }

        }

        /// <summary>
        /// Gets a <see cref="Dictionary{T1,T2}"/> style list of <see cref="IaonTree"/> information.
        /// </summary>
        /// <returns><see cref="Dictionary{T1,T2}"/> containing ID and Name of adapters defined in the database.</returns>
        public static Dictionary<int, string> GetLookupList()
        {
            return null;
        }

        /// <summary>
        /// Saves <see cref="IaonTree"/> information to database.
        /// </summary>    
        /// <returns>String, for display use, indicating success.</returns>
        public static string Save()
        {
            return "";
        }

        /// <summary>
        /// Deletes specified <see cref="IaonTree"/> record from database.
        /// </summary>
        /// <returns>String, for display use, indicating success.</returns>
        public static string Delete()
        {
            return "";
        }

        #endregion
    }
}
