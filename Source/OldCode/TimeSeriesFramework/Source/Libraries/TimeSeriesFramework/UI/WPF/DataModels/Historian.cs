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
    public class Historian : DataModelBase
    {
        #region [ Members ]

        // Fields
        private string m_nodeId;
        private int m_ID;
        private string m_acronym;
        private string m_name;
        private string m_assemblyName;
        private string m_typeName;
        private string m_connectionString;
        private bool m_isLocal;
        private string m_description;
        private int m_loadOrder;
        private bool m_enabled;
        private int m_measurementReportingInterval;
        private string m_nodeName;
        private DateTime m_createdOn;
        private string m_createdBy;
        private DateTime m_updatedOn;
        private string m_updatedBy;

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the current <see cref="Historian" />'s Node ID.
        /// </summary>
        public string NodeId
        {
            get
            {
                return m_nodeId;
            }
            set
            {
                m_nodeId = value;
            }
        }

        /// <summary>
        /// Gets or sets the current <see cref="Historian" />'s ID.
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
        /// Gets or sets the current <see cref="Historian" />'s Acronym.
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
        /// Gets or sets the current <see cref="Historian" />'s Name.
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
        /// Gets or sets the current <see cref="Historian" />'s Assembly Name.
        /// </summary>
        public string AssemblyName
        {
            get
            {
                return m_assemblyName;
            }
            set
            {
                m_assemblyName = value;
            }
        }

        /// <summary>
        /// Gets or sets the current <see cref="Historian" />'s Type Name.
        /// </summary>
        public string TypeName
        {
            get
            {
                return m_typeName;
            }
            set
            {
                m_typeName = value;
            }
        }

        /// <summary>
        /// Gets or sets the current <see cref="Historian" />'s Connection String.
        /// </summary>
        public string ConnectionString
        {
            get
            {
                return m_connectionString;
            }
            set
            {
                m_connectionString = value;
            }
        }

        /// <summary>
        /// Gets or sets whether the current <see cref="Historian" /> is local.
        /// </summary>
        public bool IsLocal
        {
            get
            {
                return m_isLocal;
            }
            set
            {
                m_isLocal = value;
            }
        }

        /// <summary>
        /// Gets or sets the current <see cref="Historian" />'s Description.
        /// </summary>
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
        /// Gets or sets the current <see cref="Historian" />'s Load Order.
        /// </summary>
        public int LoadOrder
        {
            get
            {
                return m_loadOrder;
            }
            set
            {
                m_loadOrder = value;
            }
        }

        /// <summary>
        /// Gets or sets whether the current <see cref="Historian" /> is enabled.
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
            }
        }

        /// <summary>
        /// Gets or sets the current <see cref="Historian" />'s Measurement Reporting Interval.
        /// </summary>
        public int MeasurementReportingInterval
        {
            get
            {
                return m_measurementReportingInterval;
            }
            set
            {
                m_measurementReportingInterval = value;
            }
        }

        /// <summary>
        /// Gets or sets the current <see cref="Historian" />'s Node Name.
        /// </summary>
        public string NodeName
        {
            get
            {
                return m_nodeName;
            }
            set
            {
                m_nodeName = value;
            }
        }

        /// <summary>
        /// Gets or sets when the current <see cref="Historian" /> was created.
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
        /// Gets or sets who the current <see cref="Historian" /> was created by.
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
        /// Gets or sets when the current <see cref="Historian" /> was updated.
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
        /// Gets or sets who the current <see cref="Historian" /> was updated by.
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
