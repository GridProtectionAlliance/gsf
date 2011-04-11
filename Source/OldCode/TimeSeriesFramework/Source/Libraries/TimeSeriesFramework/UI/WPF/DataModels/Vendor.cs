//******************************************************************************************************
//  Vendor.cs - Gbtc
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

namespace TimeSeriesFramework.UI.DataModels
{
    /// <summary>
    /// Creates a new object that represents a Vendor
    /// </summary>
    public class Vendor
    {        
        #region [ Members ]

        // Fields
        private int m_ID;
        private string m_acronym;
        private string m_name;
        private string m_phoneNumber;
        private string m_contactEmail;
        private string m_URL;
        private DateTime m_createdOn;
        private string m_createdBy;
        private DateTime m_updatedOn;
        private string m_updatedBy;
        
        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the <see cref="Vendor"/> ID.
        /// </summary>
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
        /// Gets or sets the <see cref="Vendor"/> Acronym.
        /// </summary>
        public string Acronym
        {
            get 
            { 
                return m_acronym; 
            }
            set 
            { 
                m_acronym = value; 
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="Vendor"/> Name.
        /// </summary>
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
        /// Gets or sets <see cref="Vendor"/> Phone Number.
        /// </summary>
        public string PhoneNumber
        {
            get 
            { 
                return m_phoneNumber; 
            }
            set 
            { 
                m_phoneNumber = value; 
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="Vendor"/> Contact Email.
        /// </summary>
        public string ContactEmail
        {
            get 
            { 
                return m_contactEmail; 
            }
            set 
            { 
                m_contactEmail = value; 
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="Vendor"/> URL.
        /// </summary>
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
        /// Gets or sets the Date or Time this <see cref="Vendor"/> was created on.
        /// </summary>
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
        /// Gets or sets who this <see cref="Vendor"/> was created by.
        /// </summary>
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
        /// Gets or sets the Date Time this <see cref="Vendor"/> was updated on.
        /// </summary>
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
        /// Gets or sets who this <see cref="Vendor"/> was updated by.
        /// </summary>
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
