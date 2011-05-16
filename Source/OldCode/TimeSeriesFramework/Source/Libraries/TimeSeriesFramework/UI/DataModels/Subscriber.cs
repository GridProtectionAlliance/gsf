//******************************************************************************************************
//  Subscriber.cs - Gbtc
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
//  05/10/2011 - Magdiel Lorenzo
//       Generated original version of source code.
// 05/13/2011 - Aniket Salver
//                  Modified the way Guid is retrived from the Data Base.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.Collections.ObjectModel;
using TVA.Data;
using System.Data;

namespace TimeSeriesFramework.UI.DataModels
{
    /// <summary>
    /// Represents a record of <see cref="Subscriber"/> information as defined in the database.
    /// </summary>
    public class Subscriber : DataModelBase
    {

        #region [ Members ]

        private Guid m_nodeID;
        private Guid m_ID;
        private string m_acronym;
        private string m_name;
        private string m_sharedSecret;
        private string m_authKey;
        private string m_validIpAddresses;
        private bool m_enabled;
        private DateTime m_createdOn;
        private string m_createdBy;
        private DateTime m_updatedOn;
        private string m_updatedBy;

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the current <see cref="Subscriber"/>'s node ID.
        /// </summary>
        [Required(ErrorMessage="The Subscriber node ID is required, please provide value.")]
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
        /// Gets or sets the current <see cref="Subscriber"/>'s ID.
        /// </summary>
        public Guid ID
        {
            get
            {
                return m_ID;
            }
            set
            {
                m_ID = value;
            }
        }

        /// <summary>
        /// Gets or sets the current <see cref="Subscriber"/>'s acronym.
        /// </summary>
        [Required(ErrorMessage="The Subscriber acronym is a required field, please provide a value.")]
        [StringLength(200, ErrorMessage="The subscriber acronym cannot exceed 200 characters.")]
        public string Acronym
        {
            get
            {
                return m_acronym;
            }
            set
            {
                m_acronym = value;
                OnPropertyChanged("Acronym");
            }
        }

        /// <summary>
        /// Gets or sets the current <see cref="Subscriber"/> name.
        /// </summary>
        [StringLength(200, ErrorMessage="The subscriber name cannot exceed 200 characters.")]
        public string Name
        {
            get
            {
                return m_name;
            }
            set
            {
                m_name = value;
                OnPropertyChanged("Name");
            }
        }

        /// <summary>
        /// Gets or sets the current <see cref="Subscriber"/>'s shared secret.
        /// </summary>
        [Required(ErrorMessage="The Subscriber shared secret is a required field, please provide a value.")]
        public string SharedSecret
        {
            get
            {
                return m_sharedSecret;
            }
            set
            {
                m_sharedSecret = value;
                OnPropertyChanged("SharedSecret");
            }
        }

        /// <summary>
        /// Gets or sets the authorization key for the current <see cref="Subscriber"/>.
        /// </summary>
        [Required(ErrorMessage="The subscriber authorization key is a required field, please provide value.")]
        public string AuthKey
        {
            get
            {
                return m_authKey;
            }
            set
            {
                m_authKey = value;
            }
        }

        /// <summary>
        /// Gets or sets the valid IP addresses of the current <see cref="Subscriber"/>.
        /// </summary>
        [Required(ErrorMessage="Subscriber valid IP addresses is a required field, please provide a value.")]
        public string ValidIPAddresses
        {
            get
            {
                return m_validIpAddresses;
            }
            set
            {
                m_validIpAddresses = value;
                OnPropertyChanged("ValidIPAddresses");
            }
        }

        /// <summary>
        /// Gets or sets whether the current <see cref="Subscriber"/> is enabled.
        /// </summary>
        public bool Enabled
        {
            get
            {
                return m_enabled;
            }
            set
            {
                m_enabled = value;
                OnPropertyChanged("Enabled");
            }
        }

        /// <summary>
        /// Gets or sets when the current <see cref="Subscriber"/> was created.
        /// </summary>
        // Field is populated by trigger and has no screen interaction, so no validation attributes are applied.
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
        /// Gets or sets who the current <see cref="Subscriber"/> was created by.
        /// </summary>
        // Field is populated by trigger and has no screen interaction, so no validation attributes are applied.
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
        /// Gets or sets when the current <see cref="Subscriber"/> was updated last.
        /// </summary>
        // Field is populated by trigger and has no screen interaction, so no validation attributes are applied.
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
        /// Gets or sets who the current <see cref="Subscriber"/> was updated by.
        /// </summary>
        // Field is populated by trigger and has no screen interaction, so no validation attributes are applied.
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

        #region [ Static ]

        /// <summary>
        /// Loads <see cref="Node"/> information as an <see cref="ObservableCollection{T}"/> style list.
        /// </summary>
        /// <param name="database"><see cref="AdoDataConnection"/> to connection to database.</param>        
        /// <returns>Collection of <see cref="Subscriber"/>.</returns>
        public static ObservableCollection<Subscriber> Load(AdoDataConnection database)
        {
            bool createdConnection = false;

            try
            {
                createdConnection = CreateConnection(ref database);

                ObservableCollection<Subscriber> subscriberList = new ObservableCollection<Subscriber>();
                DataTable subscriberTable;

                subscriberTable = database.Connection.RetrieveData(database.AdapterType, "SELECT ID, NodeID, Acronym, Name, SharedSecret, AuthKey, ValidIPAddresses, Enabled FROM Subscriber ORDER BY Name");

                foreach (DataRow row in subscriberTable.Rows)
                {
                    subscriberList.Add(new Subscriber()
                    {
                        ID = database.Guid(row, "ID"), 
                        NodeID = database.Guid(row, "NodeID"),
                        Acronym = row.Field<string>("Acronym"),
                        Name = row.Field<string>("Name"),
                        SharedSecret = row.Field<string>("SharedSecret"),
                        AuthKey = row.Field<string>("AuthKey"),
                        ValidIPAddresses = row.Field<string>("ValidIPAddresses"),
                        Enabled = Convert.ToBoolean(row.Field<object>("Enabled"))
                    });
                }

                return subscriberList;
            }
            finally
            {
                if (createdConnection && database != null)
                    database.Dispose();
            }
        }

        /// <summary>
        /// Gets a <see cref="Dictionary{T1,T2}"/> style list of <see cref="Subscriber"/> information.
        /// </summary>
        /// <param name="database"><see cref="AdoDataConnection"/> to connection to database.</param>
        /// <param name="isOptional">Indicates if selection on UI is optional for this collection.</param>
        /// <returns><see cref="Dictionary{T1,T2}"/> containing ID and Name of subscribers defined in the database.</returns>
        public static Dictionary<Guid, string> GetLookupList(AdoDataConnection database, bool isOptional = false)
        {
            bool createdConnection = false;

            try
            {
                createdConnection = CreateConnection(ref database);

                Dictionary<Guid, string> subscriberList = new Dictionary<Guid, string>();

                if (isOptional)
                    subscriberList.Add(Guid.Empty, "Select Subscriber");

                DataTable subscriberTable = database.Connection.RetrieveData(database.AdapterType, "SELECT ID, Name FROM Subscriber WHERE Enabled = @enabled ORDER BY Name", DefaultTimeout, true);

                foreach (DataRow row in subscriberTable.Rows)
                {
                    subscriberList[database.Guid(row, "ID")] = row.Field<string>("Name");
                }

                return subscriberList;
            }
            finally
            {
                if (createdConnection && database != null)
                    database.Dispose();
            }
        }

        /// <summary>
        /// Saves <see cref="Subscriber"/> information to database.
        /// </summary>
        /// <param name="database"><see cref="AdoDataConnection"/> to connection to database.</param>
        /// <param name="subscriber">Information about <see cref="Subscriber"/>.</param>        
        /// <returns>String, for display use, indicating success.</returns>
        public static string Save(AdoDataConnection database, Subscriber subscriber)
        {
            bool createdConnection = false;

            try
            {
                createdConnection = CreateConnection(ref database);

                if (subscriber.ID == Guid.Empty)
                    database.Connection.ExecuteNonQuery("INSERT INTO Company (NodeID, Acronym, Name, SharedSecret, AuthKey, ValidIPAddresses, Enabled, UpdatedBy, UpdatedOn, CreatedBy, CreatedOn) VALUES (@nodeID, @acronym)" +
                "@name, @sharedSecret, @authKey, @validIpAddresses, @enabled, @updatedBy, @updatedOn, @createdBy, @createdOn", DefaultTimeout, database.Guid(subscriber.NodeID), subscriber.Acronym.ToNotNull(), subscriber.Name.ToNotNull(), subscriber.SharedSecret.ToNotNull(),
                subscriber.AuthKey.ToNotNull(), subscriber.ValidIPAddresses.ToNotNull(), subscriber.Enabled, CommonFunctions.CurrentUser, database.UtcNow(), CommonFunctions.CurrentUser, database.UtcNow());
                else
                    database.Connection.ExecuteNonQuery("UPDATE Node SET NodeID = @nodeID, Acronym = @acronym, Name = @name, SharedSecret = @sharedSecret, AuthKey = @authKey, ValidIPAddresses = @validIpAddresses, " +
                        "Enabled = @enabled, UpdatedBy = @updatedBy, UpdatedOn = @updatedOn WHERE ID = @id", DefaultTimeout, database.Guid(subscriber.NodeID), subscriber.Acronym.ToNotNull(), subscriber.Name.ToNotNull(),
                        subscriber.SharedSecret.ToNotNull(), subscriber.AuthKey.ToNotNull(), subscriber.ValidIPAddresses.ToNotNull(), subscriber.Enabled, CommonFunctions.CurrentUser, database.UtcNow(), database.Guid(subscriber.ID));

                return "Subscriber information saved successfully";
            }
            finally
            {
                if (createdConnection && database != null)
                    database.Dispose();
            }
        }


        /// <summary>
        /// Deletes specified <see cref="Subscriber"/> record from database.
        /// </summary>
        /// <param name="database"><see cref="AdoDataConnection"/> to connection to database.</param>
        /// <param name="ID">ID of the record to be deleted.</param>
        /// <returns>String, for display use, indicating success.</returns>
        public static string Delete(AdoDataConnection database, Guid ID)
        {
            bool createdConnection = false;

            try
            {
                createdConnection = CreateConnection(ref database);

                // Setup current user context for any delete triggers
                CommonFunctions.SetCurrentUserContext(database);

                database.Connection.ExecuteNonQuery("DELETE FROM Subscriber WHERE ID = @id", DefaultTimeout, database.Guid(ID));

                return "Subscriber deleted successfully";
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
