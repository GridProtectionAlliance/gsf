//******************************************************************************************************
//  SubscriberMeasurementGroup.cs - Gbtc
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
//  05/13/2011 - Aniket Salver
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data;
using TVA.Data;

namespace TimeSeriesFramework.UI.DataModels
{
    class SubscriberMeasurementGroup : DataModelBase
    {
        #region[Members]

        // Fields
        private Guid m_nodeID;
        private Guid m_subscriberID;
        private int m_measurementGroupID;
        private bool m_allowed;
        private DateTime m_createdOn;
        private string m_createdBy;
        private DateTime m_updatedOn;
        private string m_updatedBy;
        private string m_subscriberAcronym;
        private string m_subscriberName;
        private string m_measurementGroupName;

        #endregion

        #region [Properties]

        /// <summary>
        /// Gets or sets the <see cref="SubscriberMeasurementGroup"/> NodeID.
        /// </summary>
        // Field is populated by database via auto-increment, so no validation attributes are applied.
        public Guid NodeID
        {
            get
            {
                return m_nodeID;
            }
            set
            {
                m_nodeID = value;
                OnPropertyChanged("NodeID");
            }
        }

        /// <summary>
        /// Gets or sets <see cref="SubscriberMeasurementGroup"/> SubscriberID.
        /// </summary>
        [Required(ErrorMessage = "Subscriber Measurement Group SubscriberID is a required field, please select a value.")]
        public Guid SubscriberID
        {
            get
            {
                return m_subscriberID;
            }
            set
            {
                m_subscriberID = value;
                OnPropertyChanged("SubscriberID");
            }
        }

        /// <summary>
        /// Gets or sets <see cref="SubscriberMeasurementGroup"/> MeasurementGroupID.
        /// </summary>
        [Required(ErrorMessage = "Subscriber Measurement Group MeasurementGroupID is a required field, please select a value.")]
        public int MeasurementGroupID
        {
            get
            {
                return m_measurementGroupID;
            }
            set
            {
                m_measurementGroupID = value;
                OnPropertyChanged("MeasurementGroupID");
            }
        }

        /// <summary>
        /// Gets or sets <see cref="SubscriberMeasurementGroup"/> Allowed .
        /// </summary>
        [Required(ErrorMessage = "Subscriber Measurement Group Allowed  is a required field, please select a value.")]
        [DefaultValue(false)]
        public bool Allowed
        {
            get
            {
                return m_allowed;
            }
            set
            {
                m_allowed = value;
                OnPropertyChanged("Allowed");
            }
        }

        /// <summary>
        /// Gets or sets <see cref="SubscriberMeasurementGroup"/> CreatedOn.
        /// </summary>
        // Field is populated by database via trigger and has no screen interaction, so no validation attributes are applied
        public DateTime CreatedOn
        {
            get
            {
                return m_createdOn;
            }
            set
            {
                m_createdOn = value;
            }
        }

        /// <summary>
        /// Gets or sets <see cref="SubscriberMeasurementGroup"/> CreatedBy.
        /// </summary>
        // Field is populated by database via trigger and has no screen interaction, so no validation attributes are applied
        public string CreatedBy
        {
            get
            {
                return m_createdBy;
            }
            set
            {
                m_createdBy = value;
            }
        }

        /// <summary>
        /// Gets or sets <see cref="SubscriberMeasurementGroup"/> UpdatedOn.
        /// </summary>
        // Field is populated by database via trigger and has no screen interaction, so no validation attributes are applied
        public DateTime UpdatedOn
        {
            get
            {
                return m_updatedOn;
            }
            set
            {
                m_updatedOn = value;
            }
        }

        /// <summary>
        /// Gets or sets <see cref="SubscriberMeasurementGroup"/> UpdatedBy.
        /// </summary>
        // Field is populated by database via trigger and has no screen interaction, so no validation attributes are applied
        public string UpdatedBy
        {
            get
            {
                return m_updatedBy;
            }
            set
            {
                m_updatedBy = value;
            }
        }

        public string SubscriberAcronym
        {
            get
            {
                return m_subscriberAcronym;
            }
        }

        public string SubscriberName
        {
            get
            {
                return m_subscriberName;
            }
        }

        public string MeasurementGroupName
        {
            get
            {
                return m_measurementGroupName;
            }
        }

        #endregion

        #region[Static]

        // Static Methods

        /// <summary>
        /// Loads <see cref="SubscriberMeasurementGroup"/> information as an <see cref="ObservableCollection{T}"/> style list.
        /// </summary>
        /// <param name="database"><see cref="AdoDataConnection"/> to connection to database.</param>
        /// <returns>Collection of <see cref="SubscriberMeasurementGroup"/>.</returns>
        public static ObservableCollection<SubscriberMeasurementGroup> Load(AdoDataConnection database)
        {
            bool createdConnection = false;

            try
            {
                createdConnection = CreateConnection(ref database);

                ObservableCollection<SubscriberMeasurementGroup> SubscriberMeasurementGroupList = new ObservableCollection<SubscriberMeasurementGroup>();
                DataTable SubscriberMeasurementGroupTable = database.Connection.RetrieveData(database.AdapterType, "SELECT NodeID, SubscribeID, SubscriberAcronym, SubscriberName, " +
                "MeasurementGroupId, Allowed, MeasurementGroupName, CreatedBy, CreatedOn, UpdatedBy, UpdatedOn FROM SubscriberMeasurementGroupDetail WHERE NodeID = @nodeID ORDER BY SubcriberAcronym",
                DefaultTimeout, database.CurrentNodeID());

                foreach (DataRow row in SubscriberMeasurementGroupTable.Rows)
                {
                    SubscriberMeasurementGroupList.Add(new SubscriberMeasurementGroup()
                    {
                        NodeID = database.Guid(row, "NodeID"),
                        SubscriberID = database.Guid(row, "SubscriberID"),
                        MeasurementGroupID = row.Field<int>("MeasurementGroupID"),
                        Allowed = Convert.ToBoolean(row.Field<object>("Allowed")),
                        CreatedBy = row.Field<string>("CreatedBy"),
                        CreatedOn = row.Field<DateTime>("CreatedOn"),
                        UpdatedBy = row.Field<string>("UpdatedBy"),
                        UpdatedOn = row.Field<DateTime>("UpdatedOn"),
                        m_subscriberAcronym = row.Field<string>("SubscriberAcronym"),
                        m_subscriberName = row.Field<string>("SubscriberName"),
                        m_measurementGroupName = row.Field<string>("MeasurementGroupName")
                    });
                }

                return SubscriberMeasurementGroupList;
            }
            finally
            {
                if (createdConnection && database != null)
                    database.Dispose();
            }
        }

        /// <summary>
        /// Gets a <see cref="Dictionary{T1,T2}"/> style list of <see cref="SubscriberMeasurementGroup"/> information.
        /// </summary>
        /// <param name="database"><see cref="AdoDataConnection"/> to connection to database.</param>
        /// <param name="isOptional">Indicates if selection on UI is optional for this collection.</param>
        /// <returns><see cref="object"/> containing NodeID and Name of SubscriberMeasurementGroups defined in the database.</returns>
        public static object GetLookupList(AdoDataConnection database, bool isOptional = true)
        {
            return null;
            //bool createdConnection = false;

            //try
            //{
            //    createdConnection = CreateConnection(ref database);

            //    Dictionary<Guid, string> SubscriberMeasurementGroupList = new Dictionary<int, string>();
            //    if (isOptional)
            //        SubscriberMeasurementGroupList.Add(" ", "Select SubscriberMeasurementGroup");

            //    DataTable SubscriberMeasurementGroupTable = database.Connection.RetrieveData(database.AdapterType, "SELECT NodeID, Name FROM SubscriberMeasurementGroup ORDER BY Allowed");

            //    foreach (DataRow row in SubscriberMeasurementGroupTable.Rows)
            //        SubscriberMeasurementGroupList[database.Guid(row, "NodeID")] = row.Field<string>("SubscriberID");

            //    return SubscriberMeasurementGroupList;
            //}
            //finally
            //{
            //    if (createdConnection && database != null)
            //        database.Dispose();
            //}
        }

        /// <summary>
        /// Saves <see cref="SubscriberMeasurementGroup"/> information to database.
        /// </summary>
        /// <param name="database"><see cref="AdoDataConnection"/> to connection to database.</param>
        /// <param name="subscriberMeasurementGroup">Information about <see cref="SubscriberMeasurementGroup"/>.</param>        
        /// <returns>String, for display use, indicating success.</returns>
        public static string Save(AdoDataConnection database, SubscriberMeasurementGroup subscriberMeasurementGroup)
        {
            bool createdConnection = false;

            try
            {
                createdConnection = CreateConnection(ref database);

                if (subscriberMeasurementGroup.NodeID == Guid.Empty)
                    database.Connection.ExecuteNonQuery("INSERT INTO SubscriberMeasurementGroup (NodeID, SubscriberID, MeasurementGroupID, Allowed, UpdatedBy, UpdatedOn, CreatedBy, CreatedOn) " +
                        "VALUES (@nodeID, @subscriberiD, @measurementGroupID, @allowed, @updatedBy, @updatedOn, @createdBy, @createdOn)", DefaultTimeout, database.Guid(subscriberMeasurementGroup.NodeID),
                        database.Guid(subscriberMeasurementGroup.SubscriberID), subscriberMeasurementGroup.MeasurementGroupID, subscriberMeasurementGroup.Allowed,
                        CommonFunctions.CurrentUser, database.UtcNow(), CommonFunctions.CurrentUser, database.UtcNow());
                else    // TODO: Check this query here specifically for WHERE clause.
                    database.Connection.ExecuteNonQuery("UPDATE SubscriberMeasurementGroup SET NodeID = @nodeID, SubscriberID = @subscriberID, MeasurementGroupID = @measurementGroupID, Allowed = @allowed, " +
                        "UpdatedBy = @updatedBy, UpdatedOn = @updatedOn WHERE NodeID = @NodeID", DefaultTimeout, database.Guid(subscriberMeasurementGroup.NodeID),
                        database.Guid(subscriberMeasurementGroup.SubscriberID), subscriberMeasurementGroup.MeasurementGroupID, subscriberMeasurementGroup.Allowed,
                         CommonFunctions.CurrentUser, database.UtcNow(), subscriberMeasurementGroup.NodeID);

                return "SubscriberMeasurementGroup information saved successfully";
            }
            finally
            {
                if (createdConnection && database != null)
                    database.Dispose();
            }
        }

        /// <summary>
        /// Deletes specified <see cref="SubscriberMeasurementGroup"/> record from database.
        /// </summary>
        /// <param name="database"><see cref="AdoDataConnection"/> to connection to database.</param>
        /// <param name="SubscriberMeasurementGroupID">ID of the record to be deleted.</param>
        /// <returns>String, for display use, indicating success.</returns>
        public static string Delete(AdoDataConnection database, int SubscriberMeasurementGroupID)
        {
            bool createdConnection = false;

            try
            {
                createdConnection = CreateConnection(ref database);

                // Setup current user context for any delete triggers
                CommonFunctions.SetCurrentUserContext(database);

                database.Connection.ExecuteNonQuery("DELETE FROM SubscriberMeasurementGroup WHERE NodeID= @subscriberMeasurementGroupID", DefaultTimeout, SubscriberMeasurementGroupID);

                return "SubscriberMeasurementGroup deleted successfully";
            }
            finally
            {
                if (createdConnection && database != null)
                    database.Dispose();
            }
        }
        #endregion
    }
}
