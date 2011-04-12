//******************************************************************************************************
//  VendorDevice.cs - Gbtc
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
    /// <summary>
    /// Creates a new object that represents a VendorDevice
    /// </summary>
    public class VendorDevice
    {
        #region [ Members ]

        // Fields
        private int m_ID;
        private int m_vendorID;
        private string m_name;
        private string m_description;
        private string m_URL;
        private string m_vendorName;
        private DateTime m_createdOn;
        private string m_createdBy;
        private DateTime m_UpdatedOn;
        private string m_updatedBy;
        
        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the <see cref="VendorDevice"/>'s ID.
        /// </summary>
        // Field is populated by database via auto-increment, so no validation attributes are applied.
        public int ID
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
        /// Gets or sets the VendorID.
        /// </summary>
        [DefaultValue(typeof(int), "10")]
        public int VendorID
        {
            get 
            { 
                return m_vendorID; 
            }
            set 
            { 
                m_vendorID = value; 
            }
        }

        /// <summary>
        /// Gets or sets the Name.
        /// </summary>
        [Required(ErrorMessage="VendorDevice Name is a required field, please provide value")]
        [StringLength(100, ErrorMessage="VendorDevice Name cannot exceed 100 characters.")]
        public string Name
        {
            get 
            { 
                return m_name; 
            }
            set 
            { 
                m_name = value; 
            }
        }

        /// <summary>
        /// Gets or sets the Description.
        /// </summary>
        // Because of database design, no validation attributes are applied
        public string Description
        {
            get 
            { 
                return m_description; 
            }
            set 
            { 
                m_description = value; 
            }
        }

        /// <summary>
        /// Gets or sets the URL.
        /// </summary>
        [DataType(DataType.Url, ErrorMessage = "URL is not formatted properly.")]
        public string URL
        {
            get 
            { 
                return m_URL; 
            }
            set 
            { 
                m_URL = value; 
            }
        }

        /// <summary>
        /// Gets or sets the VendorName.
        /// </summary>
        // Because of database design, no validation attributes are applied
        public string VendorName
        {
            get 
            { 
                return m_vendorName; 
            }
            set 
            { 
                m_vendorName = value; 
            }
        }

        /// <summary>
        /// Gets or sets the Date or Time this <see cref="VendorDevice"/> was created.
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
        /// Gets or sets who this <see cref="VendorDevice"/> was created by.
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
        /// Gets or sets the Date or Time this <see cref="VendorDevice"/> was updated.
        /// </summary>
        // Field is populated by trigger and has no screen interaction, so no validation attributes are applied
        public DateTime UpdatedOn
        {
            get 
            {
                return m_UpdatedOn; 
            }
            set 
            { 
                m_UpdatedOn = value; 
            }
        }

        /// <summary>
        /// Gets or sets who this VenderDevice was updated by.
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
        
    }
}
