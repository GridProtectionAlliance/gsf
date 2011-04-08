//******************************************************************************************************
//  Historian.cs - Gbtc
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
//  04/07/2011 - Magdiel Lorenzo
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
    /// Creates a new object that represents a Historian
    /// </summary>
    public class Historian
    {

        #region [ Members ]

        // Nested Types

        // Constants

        // Delegates

        // Events

        // Fields
        private string m_NodeId;
        private int m_ID;
        private string m_Acronym;
        private string m_Name;
        private string m_AssemblyName;
        private string m_TypeName;
        private string m_ConnectionString;
        private bool m_IsLocal;
        private string m_Description;
        private int m_LoadOrder;
        private bool m_Enabled;
        private int m_MeasurementReportingInterval;
        private string m_NodeName;
        private DateTime m_CreatedOn;
        private string m_CreatedBy;
        private DateTime m_UpdatedOn;
        private string m_UpdatedBy;
        #endregion

        #region [ Properties ]
        /// <summary>
        /// Gets and sets the current Historian's Node ID
        /// </summary>
        public string NodeId
        {
            get
            {
                return m_NodeId;
            }
            set
            {
                m_NodeId = value;
            }
        }
        /// <summary>
        /// Gets and sets the current Historian's ID
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
        /// Gets and sets the current Historian's Acronym
        /// </summary>
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
        /// <summary>
        /// Gets and sets the current Historian's Name
        /// </summary>
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
        /// <summary>
        /// Gets and sets the current Historian's Assembly Name
        /// </summary>
        public string AssemblyName
        {
            get
            {
                return m_AssemblyName;
            }
            set
            {
                m_AssemblyName = value;
            }
        }
        /// <summary>
        /// Gets and sets the current Historian's Type Name
        /// </summary>
        public string TypeName
        {
            get
            {
                return m_TypeName;
            }
            set
            {
                m_TypeName = value;
            }
        }
        /// <summary>
        /// Gets and sets the current Historian's Connection String
        /// </summary>
        public string ConnectionString
        {
            get
            {
                return m_ConnectionString;
            }
            set
            {
                m_ConnectionString = value;
            }
        }
        /// <summary>
        /// Gets and sets whether the current Historian is local
        /// </summary>
        public bool IsLocal
        {
            get
            {
                return m_IsLocal;
            }
            set
            {
                m_IsLocal = value;
            }
        }
        /// <summary>
        /// Gets and sets the current Historian's Description
        /// </summary>
        public string Description
        {
            get
            {
                return m_Description;
            }
            set
            {
                m_Description = value;
            }
        }
        /// <summary>
        /// Gets and sets the current Historian's Load Order
        /// </summary>
        public int LoadOrder
        {
            get
            {
                return m_LoadOrder;
            }
            set
            {
                m_LoadOrder = value;
            }
        }
        /// <summary>
        /// Gets and sets whether the current Historian is enabled
        /// </summary>
        public bool Enabled
        {
            get
            {
                return m_Enabled;
            }
            set
            {
                m_Enabled = value;
            }
        }
        /// <summary>
        /// Gets and sets the current Historian's Measurement Reporting Interval
        /// </summary>
        public int MeasurementReportingInterval
        {
            get
            {
                return m_MeasurementReportingInterval;
            }
            set
            {
                m_MeasurementReportingInterval = value;
            }
        }
        /// <summary>
        /// Gets and sets the current Historian's Node Name
        /// </summary>
        public string NodeName
        {
            get
            {
                return m_NodeName;
            }
            set
            {
                m_NodeName = value;
            }
        }
        /// <summary>
        /// Gets and sets when the current Historian was created
        /// </summary>
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
        /// <summary>
        /// Gets and sets who the current Historian was created by
        /// </summary>
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
        /// <summary>
        /// Gets and sets when the current Historian was updated
        /// </summary>
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
        /// Gets and sets who the current Historian was updated by
        /// </summary>
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
        
    }
}
