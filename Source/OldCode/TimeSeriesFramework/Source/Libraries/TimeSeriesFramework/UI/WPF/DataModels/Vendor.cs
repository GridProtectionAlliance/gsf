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
    class Vendor
    {

        #region [ Members ]

        // Nested Types

        // Constants

        // Delegates

        // Events

        // Fields
        private int m_ID;
        private string m_Acronym;
        private string m_Name;
        private string m_PhoneNumber;
        private string m_ContactEmail;
        private string m_URL;
        private DateTime m_CreatedOn;
        private string m_CreatedBy;
        private DateTime m_UpdatedOn;
        private string m_UpdatedBy;

        
        #endregion

        #region [ Constructors ]

        #endregion

        #region [ Properties ]

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

        public string Acronym
        {
            get 
            { 
                return m_Acronym; 
            }
            set 
            { 
                m_Acronym = value; 
            }
        }

        public string Name
        {
            get 
            { 
                return m_Name; 
            }
            set 
            { 
                m_Name = value; 
            }
        }

        public string PhoneNumber
        {
            get 
            { 
                return m_PhoneNumber; 
            }
            set 
            { 
                m_PhoneNumber = value; 
            }
        }

        public string ContactEmail
        {
            get 
            { 
                return m_ContactEmail; 
            }
            set 
            { 
                m_ContactEmail = value; 
            }
        }

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

        public DateTime CreatedOn
        {
            get 
            { 
                return m_CreatedOn; 
            }
            set 
            { 
                m_CreatedOn = value; 
            }
        }

        public string CreatedBy
        {
            get 
            { 
                return m_CreatedBy; 
            }
            set 
            { 
                m_CreatedBy = value; 
            }
        }

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

        public string UpdatedBy
        {
            get 
            { 
                return m_UpdatedBy; 
            }
            set 
            {
                m_UpdatedBy = value; 
            }
        }

        #endregion

        #region [ Methods ]

        #endregion

        #region [ Operators ]

        #endregion

        #region [ Static ]

        // Static Fields

        // Static Constructor

        // Static Properties

        // Static Methods

        #endregion
        
    }
}
