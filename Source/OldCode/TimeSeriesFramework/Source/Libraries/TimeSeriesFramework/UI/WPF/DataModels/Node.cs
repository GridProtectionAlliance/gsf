//******************************************************************************************************
//  Node.cs - Gbtc
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
    /// Creates a new object that represents a Node
    /// </summary>
    public class Node
    {

        
        #region [ Members ]
        private string m_ID;
        private string m_Name;
        private int? m_CompanyID;
        private decimal? m_Longitude;
        private decimal? m_Latitude;
        private string m_Description;
        private string m_Image;
        private bool m_Master;
        private int m_LoadOrder;
        private bool m_Enabled;
        private string m_TimeSeriesDataServiceUrl;
        private string m_RemoteStatusServiceUrl;
        private string m_RealTimeStatisticServiceUrl;
        private string m_CompanyName;
        private DateTime m_CreatedOn;
        private string m_CreatedBy;
        private DateTime m_UpdatedOn;


        private string m_UpdatedBy;

        
        #endregion

        #region [ Constructors ]

        #endregion

        #region [ Properties ]
        /// <summary>
        /// Gets and sets the current Node's ID
        /// </summary>
        public string ID
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
        /// Gets and sets the current Node's Name
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
        /// Gets and sets the current Node's Comapny ID
        /// </summary>
        public int? CompanyID
        {
            get 
            { 
                return m_CompanyID; 
            }
            set 
            {
                m_CompanyID = value; 
            }
        }
        /// <summary>
        /// Gets and sets the current Node's Longitude
        /// </summary>
        public decimal? Longitude
        {
            get 
            { 
                return m_Longitude; 
            }
            set 
            { 
                m_Longitude = value; 
            }
        }
        /// <summary>
        /// Gets and sets the current Node's Latitude
        /// </summary>
        public decimal? Latitude
        {
            get 
            { 
                return m_Latitude; 
            }
            set 
            { 
                m_Latitude = value; 
            }
        }
        /// <summary>
        /// Gets and sets the current Node's Description
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
        /// Gets and sets the current Node's Image
        /// </summary>
        public string Image
        {
            get 
            { 
                return m_Image; 
            }
            set 
            {
                m_Image = value; 
            }
        }
        /// <summary>
        /// Gets and sets whether the current node is the master Node
        /// </summary>
        public bool Master
        {
            get 
            { 
                return m_Master; 
            }
            set 
            {
                m_Master = value; 
            }
        }
        /// <summary>
        /// Gets and sets the current Node's Load Order
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
        /// Gets and sets whether the current Node is enabled
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
        /// Gets and sets the current Node's Time Series Data Service URL
        /// </summary>
        public string TimeSeriesDataServiceUrl
        {
            get 
            { 
                return m_TimeSeriesDataServiceUrl; 
            }
            set 
            { 
                m_TimeSeriesDataServiceUrl = value; 
            }
        }
        /// <summary>
        /// Gets and sets the current Node's Remote Status Service URL
        /// </summary>
        public string RemoteStatusServiceUrl
        {
            get 
            { 
                return m_RemoteStatusServiceUrl; 
            }
            set 
            { 
                m_RemoteStatusServiceUrl = value; 
            }
        }
        /// <summary>
        /// Gets and sets the current Node's Real Time Statistic Service URL
        /// </summary>
        public string RealTimeStatisticServiceUrl
        {
            get 
            { 
                return m_RealTimeStatisticServiceUrl; 
            }
            set 
            { 
                m_RealTimeStatisticServiceUrl = value; 
            }
        }
        /// <summary>
        /// Gets and sets the current Node's Company Name
        /// </summary>
        public string CompanyName
        {
            get 
            { 
                return m_CompanyName; 
            }
            set 
            {
                m_CompanyName = value; 
            }
        }
        /// <summary>
        /// Gets and sets the Date and Time the current Node was created on
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
        /// Gets and sets who the current Node was created by
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
        /// Gets and sets the Date and Time when the current Node was updated on
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
        /// Gets and sets who the current Node was updated by
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
