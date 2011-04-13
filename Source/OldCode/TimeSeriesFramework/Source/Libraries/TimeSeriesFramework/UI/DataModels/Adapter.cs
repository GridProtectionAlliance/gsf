//******************************************************************************************************
//  ErrorLog.cs - Gbtc
//
//  Copyright © 2010, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the Eclipse private License -v 1.0 (the "License"); you may
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
//  04/13/2011 - Aniket Salver
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace TimeSeriesFramework.UI.DataModels
{
    #region [ Enumerations ]

    /// <summary>
    /// AdapterType method enumeration.
    /// </summary>
    public enum AdapterType
    {
        /// <summary>
        /// Action Adapter.
        /// </summary>
        /// <remarks>
        /// Use this option to process the incoming data.
        /// </remarks>
        Action,
        /// <summary>
        /// Input Adapter.
        /// </summary>
        /// <remarks>
        /// Use this option to collect stream data and assign incoming measurements an ID.
        /// </remarks>
        Input,
        /// <summary>
        /// Output Adapter.
        /// </summary>
        /// <remarks>
        /// Use this option to queue up data for archival.
        /// </remarks>
        Output
    }

    #endregion

    /// <summary>
    /// Represents a record of <see cref="Adapter"/> information as defined in the database.
    /// </summary>
   public class Adapter : DataModelBase
    {
        #region[Members]

       // Fields

        private string m_nodeID;
        private int m_ID;
        private string m_adapterName;
        private string m_assemblyName;
        private string m_typeName;
        private string m_connectionString;
        private int m_loadOrder;
        private bool m_enabled;
        private string m_nodeName;
        private Adapter m_adapterType;
        private DateTime m_createdOn;
        private string m_createdBy;
        private DateTime m_updatedOn;
        private string m_updatedBy;
       
        #endregion

        #region[Properties]

        /// <summary>
        /// Gets or sets <see cref="Adapter"/> NodeID.
        /// </summary>
        [Required(ErrorMessage = " Adapter NodeID is a required field, please provide value.")]
        [StringLength(36, ErrorMessage = "Adapter NodeID cannot exceed 36 characters.")]
        public string NodeID
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
        /// Gets or sets <see cref="Adapter"/> ID
        /// </summary>
        // Field is populated by trigger and has no screen interaction, so no validation attributes are applied
        public int ID
        {
            get
            {
                return m_ID;
            }
            set
            {
                m_ID = value;
                OnPropertyChanged("ID");
            }
        }

        /// <summary>
        /// Gets or sets <see cref="Adapter"/> AdapterName
        /// </summary>
        [Required(ErrorMessage = " Adapter AdapterName is a required field, please provide value.")]
        [StringLength(50, ErrorMessage = " Adapter AdapterName cannot exceed 50 characters.")]
        public string AdapterName
        {
            get
            {
                return m_adapterName;
            }
            set
            {
                m_adapterName = value;
                OnPropertyChanged("AdapterName");
            }
        }

        /// <summary>
        /// Gets or sets <see cref="Adapter"/> AssemblyName.
        /// </summary>
        [Required(ErrorMessage = " Adapter AssemblyName is a required field, please provide value.")]
        public string AssemblyName
        {
            get
            {
                return m_assemblyName;
            }
            set
            {
                m_assemblyName = value;
                OnPropertyChanged("AssemblyName");
            }
        }

        /// <summary>
        /// Gets or sets <see cref="Adapter"/> TypeName.
        /// </summary>
        [Required(ErrorMessage = " Adapter TypeName is a required field, please provide value.")]
        public string TypeName
        {
            get
            {
                return m_typeName;
            }
            set
            {
                m_typeName = value;
                OnPropertyChanged("TypeName");
            }
        }

        /// <summary>
        /// Gets or sets <see cref="Adapter"/> ConnectionString.
        /// </summary>
        // Field is populated by trigger and has no screen interaction, so no validation attributes are applied
        public string ConnectionString
        {
            get
            {
                return m_connectionString;
            }
            set
            {
                m_connectionString = value;
                OnPropertyChanged("ConnectionString");
            }
        }

        /// <summary>
        /// Gets or sets <see cref="Adapter"/> LoadOrder
        /// </summary>
        [Required(ErrorMessage = " Adapter LoadOrder is a required field, please provide value.")]
        [DefaultValue(typeof(int), "0")]
        public int LoadOrder
        {
            get
            {
                return m_loadOrder;
            }
            set
            {
                m_loadOrder = value;
                OnPropertyChanged("LoadOrder");

            }
        }

        /// <summary>
        /// Gets or sets <see cref="Adapter"/> Enabled.
        /// </summary>
        [Required(ErrorMessage = " Adapter Enabled is a required field, please provide value.")]
        [DefaultValue(typeof(int), "0")]
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
        /// Gets or sets <see cref="Adapter"/> NodeName.
        /// </summary>
        // Field is populated by trigger and has no screen interaction, so no validation attributes are applied
        public string NodeName
        {
            get
            {
                return m_nodeName;
            }
            set
            {
                m_nodeName = value;
                OnPropertyChanged("NodeName");
            }
        }

        /// <summary>
        /// Gets or sets <see cref="Adapter"/> adapterType
        /// </summary>
        // Field is populated by trigger and has no screen interaction, so no validation attributes are applied
        public Adapter AdapterType
        {
            get
            {
                return m_adapterType;
            }
            set
            {
                m_adapterType = value;
                OnPropertyChanged("AdapterType");
            }
        }

        /// <summary>
        /// Gets or sets <see cref="Adapter"/> CreatedOn.
        /// </summary>
        // Field is populated by database via auto-increment and has no screen interaction, so no validation attributes are applied
        public DateTime CreatedOn
        {
            get
            {
                return m_createdOn;
            }
            set
            {
                m_createdOn = value;
                OnPropertyChanged("CreatedOn");
            }
        }

        /// <summary>
        /// Gets or sets <see cref="Adapter"/> CreatedBy.
        /// </summary>
        // Field is populated by database via auto-increment and has no screen interaction, so no validation attributes are applied
        public string CreatedBy
        {
            get
            {
                return m_createdBy;
            }
            set
            {
                m_createdBy = value;
                OnPropertyChanged("CreatedBy");
            }
        }

        /// <summary>
        /// Gets or sets <see cref="Adapter"/> UpdatedOn.
        /// </summary>
        // Field is populated by database via auto-increment and has no screen interaction, so no validation attributes are applied
        public DateTime UpdatedOn
        {
            get
            {
                return m_updatedOn;
            }
            set
            {
                m_updatedOn = value;
                OnPropertyChanged("UpdateOn");
            }
        }

        /// <summary>
        /// Gets or sets <see cref="Adapter"/> UpdatedBy.
        /// </summary>
        // Field is populated by database via auto-increment and has no screen interaction, so no validation attributes are applied
        public string UpdatedBy
        {
            get
            {
                return m_updatedBy;
            }
            set
            {
                m_updatedBy = value;
                OnPropertyChanged("UpdatedBy");
            }
        }

        #endregion
    }
}