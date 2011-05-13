//******************************************************************************************************
//  Phasor.cs - Gbtc
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
//  04/08/2011 - Magdiel Lorenzo
//       Generated original version of source code.
//  05/02/2011 - J. Ritchie Carroll
//       Updated for coding consistency.
//  05/03/2011 - Mehulbhai P Thakkar
//       Guid field related changes as well as static functions update.
//  05/05/2011 - Mehulbhai P Thakkar
//       Added NULL handling for Save() operation.
//  05/13/2011 - Mehulbhai P Thakkar
//       Modified static methods to filter data by device.
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
    /// <summary>
    /// Represents a record of <see cref="Phasor"/> information as defined in the database.
    /// </summary>
    public class Phasor : DataModelBase
    {
        #region [ Members ]

        // Fields
        private int m_id;
        private int m_deviceID;
        private string m_label;
        private string m_type;
        private string m_phase;
        private int? m_destinationPhasorID;
        private int m_sourceIndex;
        //private string m_destinationPhasorLabel;
        //private string m_deviceAcronym;
        //private string m_phasorType;
        //private string m_phaseType;
        private DateTime m_createdOn;
        private string m_createdBy;
        private DateTime m_updatedOn;
        private string m_updatedBy;

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the <see cref="Phasor"/> ID.
        /// </summary>
        // Field is populated by database via auto-increment and has no screen interaction, so no validation attributes are applied.
        public int ID
        {
            get
            {
                return m_id;
            }
            set
            {
                m_id = value;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="Phasor"/> DeviceID.
        /// </summary>
        [Required(ErrorMessage = "Phasor device ID is a required field, please provide value.")]
        public int DeviceID
        {
            get
            {
                return m_deviceID;
            }
            set
            {
                m_deviceID = value;
                OnPropertyChanged("DeviceID");
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="Phasor"/> Label.
        /// </summary>
        [Required(ErrorMessage = "Phasor label is a required field, please provide value.")]
        [StringLength(200, ErrorMessage = "Phasor label must not exceed 200 characters.")]
        public string Label
        {
            get
            {
                return m_label;
            }
            set
            {
                m_label = value;
                OnPropertyChanged("Label");
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="Phasor"/> Type.
        /// </summary>
        [DefaultValue("V")]
        [StringLength(1, ErrorMessage = "Phasor type must be 1 character.")]
        public string Type
        {
            get
            {
                return m_type;
            }
            set
            {
                m_type = value;
                OnPropertyChanged("Type");
            }
        }

        /// <summary>
        /// Gets or sets the Phase of the current <see cref="Phasor"/>.
        /// </summary>
        [DefaultValue("+")]
        [StringLength(1, ErrorMessage = "Phasor phase must be 1 character.")]
        public string Phase
        {
            get
            {
                return m_phase;
            }
            set
            {
                m_phase = value;
                OnPropertyChanged("Phase");
            }
        }

        /// <summary>
        /// Gets or sets Destination <see cref="Phasor"/> ID for the current Phasor.
        /// </summary>
        // Because of Database design, no validation attributes are applied
        public int? DestinationPhasorID
        {
            get
            {
                return m_destinationPhasorID;
            }
            set
            {
                m_destinationPhasorID = value;
                OnPropertyChanged("DestinationPhasorID");
            }
        }

        /// <summary>
        /// Gets or sets Source Index for the current Phasor.
        /// </summary>
        [DefaultValue(typeof(int), "0")]
        public int SourceIndex
        {
            get
            {
                return m_sourceIndex;
            }
            set
            {
                m_sourceIndex = value;
                OnPropertyChanged("SourceIndex");
            }
        }

        ///// <summary>
        ///// Gets the Destination <see cref="Phasor"/> Label for the current Phasor.
        ///// </summary>
        //public string DestinationPhasorLabel
        //{
        //    get
        //    {
        //        return m_destinationPhasorLabel;
        //    }
        //}

        ///// <summary>
        ///// Gets the Device Acronym for the current <see cref="Phasor"/>.
        ///// </summary>
        //public string DeviceAcronym
        //{
        //    get
        //    {
        //        return m_deviceAcronym;
        //    }
        //}

        ///// <summary>
        ///// Gets <see cref="Phasor"/> Type for the current Phasor.
        ///// </summary>
        //public string PhasorType
        //{
        //    get
        //    {
        //        return m_phasorType;
        //    }
        //}

        ///// <summary>
        ///// Gets Phase Type for the current <see cref="Phasor"/>.
        ///// </summary>
        //public string PhaseType
        //{
        //    get
        //    {
        //        return m_phaseType;
        //    }
        //}

        /// <summary>
        /// Gets or sets the Date or Time the current <see cref="Phasor"/> was created on.
        /// </summary>
        // Field is populated by trigger and has no screen interaction, so no validation attributes are applied
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
        /// Gets or sets who the current <see cref="Phasor"/> was created by.
        /// </summary>
        // Field is populated by trigger and has no screen interaction, so no validation attributes are applied
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
        /// Gets or sets the Date or Time the current <see cref="Phasor"/> was updated on.
        /// </summary>
        // Field is populated by trigger and has no screen interaction, so no validation attributes are applied
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
        /// Gets or sets who the current <see cref="Phasor"/> was updated by.
        /// </summary>
        // Field is populated by trigger and has no screen interaction, so no validation attributes are applied
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

        // Static Methods

        /// <summary>
        /// Loads <see cref="Phasor"/> information as an <see cref="ObservableCollection{T}"/> style list.
        /// </summary>
        /// <param name="database"><see cref="AdoDataConnection"/> to connection to database.</param>
        /// <param name="deviceID">ID of the <see cref="Device"/> to filter data.</param>
        /// <returns>Collection of <see cref="Phasor"/>.</returns>
        public static ObservableCollection<Phasor> Load(AdoDataConnection database, int deviceID)
        {
            bool createdConnection = false;

            try
            {
                createdConnection = CreateConnection(ref database);

                ObservableCollection<Phasor> phasorList = new ObservableCollection<Phasor>();
                DataTable phasorTable = database.Connection.RetrieveData(database.AdapterType, "SELECT ID, DeviceID, Label, Type, Phase, DestinationPhasorID, SourceIndex, " +
                    "CreatedBy, CreatedOn, UpdatedBy, UpdatedOn FROM Phasor WHERE DeviceID = @deviceID ORDER BY DeviceID", DefaultTimeout, deviceID);

                foreach (DataRow row in phasorTable.Rows)
                {
                    phasorList.Add(new Phasor()
                    {
                        ID = row.Field<int>("ID"),
                        DeviceID = row.Field<int>("DeviceID"),
                        Label = row.Field<string>("Label"),
                        Type = row.Field<string>("Type"),
                        Phase = row.Field<string>("Phase"),
                        //DestinationPhasorID = row.Field<int>("DestinationPhasorID"),
                        SourceIndex = row.Field<int>("SourceIndex"),
                        CreatedBy = row.Field<string>("CreatedBy"),
                        CreatedOn = row.Field<DateTime>("CreatedOn"),
                        UpdatedBy = row.Field<string>("UpdatedBy"),
                        UpdatedOn = row.Field<DateTime>("UpdatedOn")
                    });
                }

                return phasorList;
            }
            finally
            {
                if (createdConnection && database != null)
                    database.Dispose();
            }
        }

        /// <summary>
        /// Gets a <see cref="Dictionary{T1,T2}"/> style list of <see cref="Phasor"/> information.
        /// </summary>
        /// <param name="database"><see cref="AdoDataConnection"/> to connection to database.</param>
        /// <param name="deviceID">ID of the <see cref="Device"/> to filter data.</param>
        /// <param name="isOptional">Indicates if selection on UI is optional for this collection.</param>
        /// <returns><see cref="Dictionary{T1,T2}"/> containing ID and Label of phasors defined in the database.</returns>
        public static Dictionary<int, string> GetLookupList(AdoDataConnection database, int deviceID, bool isOptional = true)
        {
            bool createdConnection = false;

            try
            {
                createdConnection = CreateConnection(ref database);

                Dictionary<int, string> phasorList = new Dictionary<int, string>();
                if (isOptional)
                    phasorList.Add(0, "Select Phasor");

                if (deviceID == 0)
                    return phasorList;

                DataTable phasorTable = database.Connection.RetrieveData(database.AdapterType, "SELECT ID, Label FROM Phasor WHERE DeviceID = @deviceID " +
                    "ORDER BY SourceIndex", DefaultTimeout, deviceID);

                foreach (DataRow row in phasorTable.Rows)
                    phasorList[row.Field<int>("ID")] = row.Field<string>("Label");

                return phasorList;
            }
            finally
            {
                if (createdConnection && database != null)
                    database.Dispose();
            }
        }

        /// <summary>
        /// Saves <see cref="Phasor"/> information to database.
        /// </summary>
        /// <param name="database"><see cref="AdoDataConnection"/> to connection to database.</param>
        /// <param name="phasor">Information about <see cref="Phasor"/>.</param>        
        /// <returns>String, for display use, indicating success.</returns>
        public static string Save(AdoDataConnection database, Phasor phasor)
        {
            bool createdConnection = false;

            try
            {
                createdConnection = CreateConnection(ref database);

                if (phasor.ID == 0)
                    database.Connection.ExecuteNonQuery("INSERT INTO Phasor (DeviceID, Label, Type, Phase, SourceIndex, UpdatedBy, UpdatedOn, CreatedBy, CreatedOn) " +
                        "VALUES (@DeviceID, @Label, @Type, @Phase, @sourceIndex, @updatedBy, @updatedOn, @createdBy, @createdOn)", DefaultTimeout, phasor.DeviceID, phasor.Label,
                        phasor.Type, phasor.Phase, phasor.SourceIndex, CommonFunctions.CurrentUser, database.UtcNow(), CommonFunctions.CurrentUser, database.UtcNow());
                else
                    database.Connection.ExecuteNonQuery("UPDATE Phasor SET DeviceID = @deviceID, Label = @label, Type = @type, Phase = @phase, SourceIndex = @sourceIndex, " +
                        "UpdatedBy = @updatedBy, UpdatedOn = @updatedOn WHERE ID = @id", DefaultTimeout, phasor.DeviceID, phasor.Label, phasor.Type,
                        phasor.Phase, phasor.SourceIndex, CommonFunctions.CurrentUser, database.UtcNow(), phasor.ID);

                return "Phasor information saved successfully";
            }
            finally
            {
                if (createdConnection && database != null)
                    database.Dispose();
            }
        }

        /// <summary>
        /// Deletes specified <see cref="Phasor"/> record from database.
        /// </summary>
        /// <param name="database"><see cref="AdoDataConnection"/> to connection to database.</param>
        /// <param name="phasorID">ID of the record to be deleted.</param>
        /// <returns>String, for display use, indicating success.</returns>
        public static string Delete(AdoDataConnection database, int phasorID)
        {
            bool createdConnection = false;

            try
            {
                createdConnection = CreateConnection(ref database);

                // Setup current user context for any delete triggers
                CommonFunctions.SetCurrentUserContext(database);

                database.Connection.ExecuteNonQuery("DELETE FROM Phasor WHERE ID = @phasorID", DefaultTimeout, phasorID);

                return "Phasor deleted successfully";
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
