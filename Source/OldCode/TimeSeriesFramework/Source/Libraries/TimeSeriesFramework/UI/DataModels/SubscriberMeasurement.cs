//******************************************************************************************************
//  SubscriberMeasurement.cs - Gbtc
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
//  05/10/2011 - Aniket Salver
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Data;
using TVA.Data;
using System.ComponentModel;

namespace TimeSeriesFramework.UI.DataModels
{
    /// <summary>
    ///  Represents a record of <see cref="SubscriberMeasurement"/> information as defined in the database.
    /// </summary>
    public class SubscriberMeasurement: DataModelBase
    {
        #region[Members]

        // Fileds
        private Guid m_nodeID;
        private Guid m_subscriberID;
        private Guid m_signalID;
        private bool m_allowed;
        private DateTime m_createdOn;
        private string m_createdBy;
        private DateTime m_updatedOn;
        private string m_updatedBy;
      
        #endregion

        #region[Properties]

        /// <summary>
        /// Gets or sets the <see cref="SubscriberMeasurement"/> NodeID.
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
        /// Gets or sets <see cref="SubscriberMeasurement"/> SubscriberID.
        /// </summary>
        [Required(ErrorMessage = "Subscriber Measurement SubscriberID is a required field, please select a value.")]
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
        /// Gets or sets <see cref="SubscriberMeasurement"/> SignalID.
        /// </summary>
        [Required(ErrorMessage = "Subscriber Measurement SignalID is a required field, please select a value.")]
        public Guid SignalID
        {
            get
            {
                return m_signalID;
            }
            set
            {
                m_signalID = value;
                OnPropertyChanged("SignalID");
            }
        }

        /// <summary>
        /// Gets or sets <see cref="SubscriberMeasurement"/> Allowed .
        /// </summary>
        [Required(ErrorMessage = "Subscriber Measurement Allowed  is a required field, please select a value.")]
        [DefaultValue(false)]
        public bool Allowed 
        {
            get
            {
                return m_allowed ;
            }
            set
            {
                m_allowed  = value;
                OnPropertyChanged("Allowed");
            }
        }

         /// <summary>
        /// Gets or sets <see cref="SubscriberMeasurement"/> CreatedOn.
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
        /// Gets or sets <see cref="SubscriberMeasurement"/> CreatedBy.
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
        /// Gets or sets <see cref="SubscriberMeasurement"/> UpdatedOn.
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
        /// Gets or sets <see cref="SubscriberMeasurement"/> UpdatedBy.
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

        #endregion

        #region[Static]
        // Static Methods

        /// <summary>
        /// Loads <see cref="SubscriberMeasurement"/> information as an <see cref="ObservableCollection{T}"/> style list.
        /// </summary>
        /// <param name="database"><see cref="AdoDataConnection"/> to connection to database.</param>
        /// <returns>Collection of <see cref="SubscriberMeasurement"/>.</returns>
        public static ObservableCollection<SubscriberMeasurement> Load(AdoDataConnection database)
        {
            bool createdConnection = false;

            try
            {
                createdConnection = CreateConnection(ref database);

                ObservableCollection<SubscriberMeasurement> SubscriberMeasurementList = new ObservableCollection<SubscriberMeasurement>();
                DataTable SubscriberMeasurementTable = database.Connection.RetrieveData(database.AdapterType, "NodeID, SubscriberID, SignalID, Allowed, CreatedBy, CreatedOn, UpdatedBy, UpdatedOn FROM SubscriberMeasurement ORDER BY SubscriberID");

                foreach (DataRow row in SubscriberMeasurementTable.Rows)
                {
                    SubscriberMeasurementList.Add(new SubscriberMeasurement()
                    {
                        NodeID =database.Guid(row, "NodeID"),
                        SubscriberID = database.Guid(row, "SubscriberID"),
                        SignalID = database.Guid(row, "SignalID"),
                        Allowed = Convert.ToBoolean(row.Field<object>("Allowed")),
                        CreatedBy = row.Field<string>("CreatedBy"),
                        CreatedOn = row.Field<DateTime>("CreatedOn"),
                        UpdatedBy = row.Field<string>("UpdatedBy"),
                        UpdatedOn = row.Field<DateTime>("UpdatedOn")
                    });
                }

                return SubscriberMeasurementList;
            }
            finally
            {
                if (createdConnection && database != null)
                    database.Dispose();
            }
        }

        /// <summary>
        /// Gets a <see cref="Dictionary{T1,T2}"/> style list of <see cref="SubscriberMeasurement"/> information.
        /// </summary>
        /// <param name="database"><see cref="AdoDataConnection"/> to connection to database.</param>
        /// <param name="isOptional">Indicates if selection on UI is optional for this collection.</param>
        /// <returns><see cref="Dictionary{T1,T2}"/> containing NodeID and SubscriberID of SubscriberMeasurements defined in the database.</returns>
        public static object GetLookupList(AdoDataConnection database, bool isOptional = true)
        {
            return null;
            //bool createdConnection = false;

            //try
            //{
            //    createdConnection = CreateConnection(ref database);

            //    Dictionary<string, string> SubscriberMeasurementList = new Dictionary<string, string>();
            //    if (isOptional)
            //        SubscriberMeasurementList.Add(" ", "Select SubscriberMeasurement");

            //    DataTable SubscriberMeasurementTable = database.Connection.RetrieveData(database.AdapterType, "SELECT NodeID, SubscriberID FROM SubscriberMeasurement ORDER BY Allowed");

            //    foreach (DataRow row in SubscriberMeasurementTable.Rows)
            //        SubscriberMeasurementList[row.Field<string>("NodeID")] = row.Field<string>("SubscriberID");

            //    return SubscriberMeasurementList;
            //}
            //finally
            //{
            //    if (createdConnection && database != null)
            //        database.Dispose();
            //}
        }

        /// <summary>
        /// Saves <see cref="SubscriberMeasurement"/> information to database.
        /// </summary>
        /// <param name="database"><see cref="AdoDataConnection"/> to connection to database.</param>
        /// <param name="subscriberMeasurement">Information about <see cref="SubscriberMeasurement"/>.</param>        
        /// <returns>String, for display use, indicating success.</returns>
        public static string Save(AdoDataConnection database, SubscriberMeasurement subscriberMeasurement)
        {
            bool createdConnection = false;
            
            try
            {
                createdConnection = CreateConnection(ref database);

                if (subscriberMeasurement.NodeID == Guid.Empty)
                    database.Connection.ExecuteNonQuery("INSERT INTO SubscriberMeasurement (NodeID, SubscriberID, SignalID, Allowed, CreatedBy, CreatedOn) " +
                        "VALUES (@nodeID, @subscriberID, @signalID, @allowed, @createdBy, @createdOn)", DefaultTimeout, subscriberMeasurement.NodeID, subscriberMeasurement.SubscriberID,
                        subscriberMeasurement.SignalID, subscriberMeasurement.Allowed, CommonFunctions.CurrentUser, database.UtcNow());
                else
                    database.Connection.ExecuteNonQuery("UPDATE SubscriberMeasurement SET NodeID = @nodeID, SubscriberID = @subscriberID, SignalID = @signalID, Allowed = @allowed,  " +
                        "UpdatedBy = @updatedBy, UpdatedOn = @updatedOn WHERE NodeID = @nodeID", DefaultTimeout, subscriberMeasurement.NodeID, subscriberMeasurement.SubscriberID, subscriberMeasurement.SignalID, subscriberMeasurement.Allowed,
                         CommonFunctions.CurrentUser, database.UtcNow(), subscriberMeasurement.NodeID);

                return "SubscriberMeasurement information saved successfully";
            }
            finally
            {
                if (createdConnection && database != null)
                    database.Dispose();
            }
        }

        /// <summary>
        /// Deletes specified <see cref="SubscriberMeasurement"/> record from database.
        /// </summary>
        /// <param name="database"><see cref="AdoDataConnection"/> to connection to database.</param>
        /// <param name="SubscriberMeasurementNodeID">NodeID of the record to be deleted.</param>
        /// <returns>string, for display use, indicating success.</returns>
        public static string Delete(AdoDataConnection database,Guid SubscriberMeasurementNodeID)
        {
            bool createdConnection = false;

            try
            {
                createdConnection = CreateConnection(ref database);

                // Setup current user context for any delete triggers
                CommonFunctions.SetCurrentUserContext(database);

                database.Connection.ExecuteNonQuery("DELETE FROM SubscriberMeasurement WHERE NodeID = @SubscriberMeasurementNodeID", DefaultTimeout, SubscriberMeasurementNodeID);

                return "SubscriberMeasurement deleted successfully";
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

