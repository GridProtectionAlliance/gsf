//******************************************************************************************************
//  Device.cs - Gbtc
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
//  04/11/2011 - Aniket Salver
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
    /// Represents a record of Device information as defined in the database.
    /// </summary>
    public class Device : DataModelBase
    {
        #region [Members]

        //fields
        //private Guid NodeID ;
        private string m_nodeID;
        private int m_ID;
        private int? m_parentID;
        private string m_acronym;
        private string m_name;
        private bool m_isConcentrator;
        private int? m_companyID;
        private int? m_historianID;
        private int m_accessID;
        private int? m_vendorDeviceID;
        private int? m_protocolID;
        private decimal? m_longitude;
        private decimal? m_latitude;
        private int? m_interconnectionID;
        private string m_connectionString;
        private string m_timeZone;
        private int? m_framesPerSecond;
        private long m_timeAdjustmentTicks;
        private double m_dataLossInterval;
        private string m_contactList;
        private int? m_measuredLines;
        private int m_loadOrder;
        private bool m_enabled;
        private int m_allowedParsingExceptions;
        private double m_parsingExceptionWindow;
        private double m_delayedConnectionInterval;
        private bool m_allowUseOfCachedConfiguration;
        private bool m_autoStartDataParsingSequence;
        private bool m_skipDisableRealTimeData;
        private int m_measurementReportingInterval;
        private string m_CompanyName;
        private string m_CompanyAcronym;
        private string m_historianAcronym;
        private string m_VendorDeviceName;
        private string m_vendorAcronym;
        private string m_protocolName;
        private string m_interconnectionName;
        private string m_nodeName;
        private string m_ParentAcronym;
        private DateTime m_createdOn;
        private string m_createdBy;
        private DateTime m_updatedOn;
        private string m_updatedBy;

        #endregion

        #region [Properties]

        /// <summary>
        /// Gets or sets <see cref="Device"/> NodeID.
        /// </summary>
        [Required(ErrorMessage = "Device NodeID is a required field, please provide value.")]
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
        /// Gets or sets <see cref="Device"/> ID.
        /// </summary>
        // Field is populated by database via auto-increment and has no screen interaction, so no validation attributes are applied
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
        /// Gets or sets <see cref="Device"/> ParentID.
        /// </summary>
        public int? ParentID 
        {
            get
            {
                return m_parentID;
            }
            set
            {
                m_parentID = value;
                OnPropertyChanged("ParentId");
            } 
        }

        /// <summary>
        ///  Gets or sets <see cref="Device"/> Acronym.
        /// </summary>
        [Required(ErrorMessage = "Device Acronym is a required field, please provide value.")]
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
        /// Gets or sets <see cref="Device"/>  Name.
        /// </summary>
        // Field is populated by database via auto-increment and has no screen interaction, so no validation attributes are applied
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
        ///  Gets or sets <see cref="Device"/> IsConcenttrator.
        /// </summary>
        [Required(ErrorMessage = " Device IsConcentrator is a required field, please provide value.")]
        [DefaultValue(typeof(bool), "0")]
        public bool IsConcentrator 
        {
            get
            {
                return m_isConcentrator;
            }
            set
            {
                m_isConcentrator = value;
                OnPropertyChanged("IsConcentrator ");
            } 
        }

        /// <summary>
        ///  Gets or sets <see cref="Device"/> CompanyID.
        /// </summary>
        // Field is populated by database via auto-increment and has no screen interaction, so no validation attributes are applied
        public int? CompanyID 
        {
            get
            {
                return m_companyID;
            }
            set
            {
                m_companyID = value;
                OnPropertyChanged("CompanyID");  
            } 
        }

        /// <summary>
        ///  Gets or sets <see cref="Device"/> HistrianID.
        /// </summary>
        // Field is populated by database via auto-increment and has no screen interaction, so no validation attributes are applied
        public int? HistorianID 
        {
            get
            {
                return m_historianID;
            }
            set
            {
                m_historianID = value;
                OnPropertyChanged("HistorianID");
            } 
        }

        /// <summary>
        ///  Gets or sets <see cref="Device"/> AccessID.
        /// </summary>
        [Required(ErrorMessage = "AccessID is required field.")]
        [DefaultValue(typeof(int), "1")]
        public int AccessID 
        {
            get
            {
                return m_accessID;
            }
            set
            {
                m_accessID = value;
                OnPropertyChanged("AccessID");
            } 
        }

        /// <summary>
        ///  Gets or sets <see cref="Device"/> VendorDeviceID.
        /// </summary>
        // Field is populated by database via auto-increment and has no screen interaction, so no validation attributes are applied
        public int? VendorDeviceID 
        {
            get
            {
                return m_vendorDeviceID;
            }
            set
            {
                m_vendorDeviceID = value;
                OnPropertyChanged("VendorDeviceID");
            } 
        }

        /// <summary>
        ///  Gets or sets <see cref="Device"/> protocolID.
        /// </summary>
        // Field is populated by database via auto-increment and has no screen interaction, so no validation attributes are applied
        public int? ProtocolID 
        {
            get
            {
                return m_protocolID;
            }
            set
            {
                m_protocolID = value;
               OnPropertyChanged("ProtocolID");
            } 
        }

        /// <summary>
        ///  Gets or sets <see cref="Device"/> Longitude.
        /// </summary>
        // Field is populated by database via auto-increment and has no screen interaction, so no validation attributes are applied
        public decimal? Longitude 
        {
            get
            {
                return m_longitude;
            }
            set
            {
                m_longitude = value;
                OnPropertyChanged("Longitude");
            } 
        }

        /// <summary>
        ///  Gets or sets <see cref="Device"/> Latitude.
        /// </summary>
        // Field is populated by database via auto-increment and has no screen interaction, so no validation attributes are applied
        public decimal? Latitude 
        {
            get
            {
                return m_latitude;
            }
            set
            {
                m_latitude = value;
                OnPropertyChanged("Latitude");
            }
        }

        /// <summary>
        ///  Gets or sets <see cref="Device"/> InterconnectionID.
        /// </summary>
        // Field is populated by database via auto-increment and has no screen interaction, so no validation attributes are applied
        public int? InterconnectionID 
        {
            get
            {
                return m_interconnectionID;
            }
            set
            {
                m_interconnectionID = value;
                OnPropertyChanged("InterconnectionID");
            }
        }

        /// <summary>
        ///  Gets or sets <see cref="Device"/> ConnectionString.
        /// </summary>
        // Field is populated by database via auto-increment and has no screen interaction, so no validation attributes are applied
        public string ConnectionString 
        {
            get
            {
                return m_connectionString;
            }
            set
            {
                m_connectionString = value;
                OnPropertyChanged("connectionString");
            }
        }

        /// <summary>
        /// Gets or sets <see cref="Device"/> TimeZone.
        /// </summary>
        // Field is populated by database via auto-increment and has no screen interaction, so no validation attributes are applied
        public string TimeZone
        {
            get
            {
                return m_timeZone;
            }
            set
            {
                m_timeZone = value;
                OnPropertyChanged("TimeZone");
            }
        }

        /// <summary>
        /// Gets or sets <see cref="Device"/> FramesPerSecond.
        /// </summary>
        //Field is populated by database via auto-increment and has no screen interaction, so no validation attributes are applied
        public int? FramesPerSecond
        {
            get
            {
                return m_framesPerSecond;
            }
            set
            {
                m_framesPerSecond = value;
                OnPropertyChanged("framePerSecond");
            }
        }

        /// <summary>
        /// Gets or sets <see cref="Device"/> TimeAdjustmentTicks.
        /// </summary>
        [Required(ErrorMessage = "TimeAdjustmentTicks is required field.")]
        [DefaultValue(typeof(long), "0")]
        public long TimeAdjustmentTicks
        {
            get
            {
                return m_timeAdjustmentTicks;
            }
            set
            {
                m_timeAdjustmentTicks = value;
                OnPropertyChanged("TimeAdjustment");
            }
        }

        /// <summary>
        /// Gets or sets <see cref="Device"/> DataLossInterval.
        /// </summary>
        [Required(ErrorMessage = "DataLossInterval is required field.")]
        [DefaultValue(typeof(double), "5")]
        public double DataLossInterval
        {
            get
            {
                return m_dataLossInterval;
            }
            set
            {
                m_dataLossInterval = value;
                OnPropertyChanged("DataLossInterval");
            }
        }

        /// <summary>
        /// Gets or sets <see cref="Device"/> ContactList.
        /// </summary>
        // Field is populated by database via auto-increment and has no screen interaction, so no validation attributes are applied
        public string ContactList
        {
            get
            {
                return m_contactList;
            }
            set
            {
                m_contactList = value;
                OnPropertyChanged("ContactList");
            }
        }

        /// <summary>
        /// Gets or sets <see cref="Device"/> MeasuredLines.
        /// </summary>
        // Field is populated by database via auto-increment and has no screen interaction, so no validation attributes are applied
        public int? MeasuredLines
        {
            get
            {
                return m_measuredLines;
            }
            set
            {
                m_measuredLines = value;
                OnPropertyChanged("MeasuredLines");
            }
        }

        /// <summary>
        /// Gets or sets <see cref="Device"/> LoadOrder.
        /// </summary>
        [Required(ErrorMessage = "DataLossInterval is required field.")]
        [DefaultValue(typeof(double), "5")]
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
        /// Gets or sets <see cref="Device"/> Enabled.
        /// </summary>
        /// Field is populated by database via auto-increment and has no screen interaction, so no validation attributes are applied
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
        /// Gets or sets <see cref="Device"/> AllowedParsingExceptions.
        /// </summary>
        [Required(ErrorMessage = "AllowedParsingExceptions is required field.")]
        [DefaultValue(typeof(int), "10")]
        public int AllowedParsingExceptions
        {
            get
            {
                return m_allowedParsingExceptions;
            }
            set
            {
                m_allowedParsingExceptions = value;
                OnPropertyChanged("AllowedParsingExceptions");
            }
        }

        /// <summary>
        /// Gets or sets <see cref="Device"/> ParsingExceptionWindow.
        /// </summary>
        [Required(ErrorMessage = "ParsingExceptionWindow is required field.")]
        [DefaultValue(typeof(double), "5")]
        public double ParsingExceptionWindow
        {
            get
            {
                return m_parsingExceptionWindow;
            }
            set
            {
                m_parsingExceptionWindow = value;
                OnPropertyChanged("ParsingExceptionWindow");
            }
        }

        /// <summary>
        /// Gets or sets <see cref="Device"/> DelayedConnectionInterval.
        /// </summary>
        [Required(ErrorMessage = "DelayedConnectionInterval is required field.")]
        [DefaultValue(typeof(double), "5")]
        public double DelayedConnectionInterval
        {
            get
            {
                return m_delayedConnectionInterval;
            }
            set
            {
                m_delayedConnectionInterval = value;
                OnPropertyChanged("DelayedConnectionInterval");
            }
        }

        /// <summary>
        /// Gets or sets <see cref="Device"/> AllowUseOfCachedConfiguration.
        /// </summary>
        [Required(ErrorMessage = "AllowUseOfCachedConfiguration is required field.")]
        [DefaultValue(typeof(bool), "1")]
        public bool AllowUseOfCachedConfiguration
        {
            get
            {
                return m_allowUseOfCachedConfiguration;
            }
            set
            {
                m_allowUseOfCachedConfiguration = value;
                OnPropertyChanged("AllowUseOfCachedConfiguration");
               
            }
        }

        /// <summary>
        /// Gets or sets <see cref="Device"/> AutoStartDataParsingSequence.
        /// </summary>
        [Required(ErrorMessage = "AutoStartDataParsingSequence is required field.")]
        [DefaultValue(typeof(bool), "1")]
        public bool AutoStartDataParsingSequence
        {
            get
            {
                return m_autoStartDataParsingSequence;
            }
            set
            {
                m_autoStartDataParsingSequence = value;
                OnPropertyChanged("AutoStartDataParsingSequence");
            }
        }

        /// <summary>
        /// Gets or sets <see cref="Device"/> SkipDisableRealTimeData.
        /// </summary>
        [Required(ErrorMessage = "SkipDisableRealTimeData is required field.")]
        [DefaultValue(typeof(bool), "0")]
        public bool SkipDisableRealTimeData
        {
            get
            {
                return m_skipDisableRealTimeData;
            }
            set
            {
                m_skipDisableRealTimeData = value;
                OnPropertyChanged("SkipDisableRealTimeData");
            }
        }

        /// <summary>
        /// Gets or sets <see cref="Device"/> MeasurementReportingInterval.
        /// </summary>
        [Required(ErrorMessage = "SkipDisableRealTimeData is required field.")]
        [DefaultValue(typeof(int), "100000")]
        public int MeasurementReportingInterval
        {
            get
            {
                return m_measurementReportingInterval;
            }
            set
            {
                m_measurementReportingInterval = value;
                OnPropertyChanged("MeasurementReportingInterval");
            }
        }

        /// <summary>
        /// Gets or sets <see cref="Device"/> CompanyName.
        /// </summary>
        // Field is populated by database via auto-increment and has no screen interaction, so no validation attributes are applied
        public string CompanyName
        {
            get
            {
                return m_CompanyName;
            }
            set
            {
                m_CompanyName = value;
                OnPropertyChanged("CompanyName");
            }
        }

        /// <summary>
        /// Gets or sets <see cref="Device"/> CompanyAcronym.
        /// </summary>
        // Field is populated by database via auto-increment and has no screen interaction, so no validation attributes are applied
        public string CompanyAcronym
        {
            get
            {
                return m_CompanyAcronym;
            }
            set
            {
                m_CompanyAcronym = value;
                OnPropertyChanged("CompanyAcronym");
            }
        }

        /// <summary>
        /// Gets or sets <see cref="Device"/> HistorianAcronym.
        /// </summary>
        // Field is populated by database via auto-increment and has no screen interaction, so no validation attributes are applied
        public string HistorianAcronym
        {
            get
            {
                return m_historianAcronym;
            }
            set
            {
                m_historianAcronym = value;
                OnPropertyChanged("HistorianAcronym");
            }
        }

        /// <summary>
        /// Gets or sets <see cref="Device"/> VendorDeviceName.
        /// </summary>
        // Field is populated by database via auto-increment and has no screen interaction, so no validation attributes are applied
        public string VendorDeviceName
        {
            get
            {
                return m_VendorDeviceName;
            }
            set
            {
                m_VendorDeviceName = value;
                OnPropertyChanged("VendorDeviceName");
            }
        }

        /// <summary>
        /// Gets or sets <see cref="Device"/> VendorAcronym.
        /// </summary>
        // Field is populated by database via auto-increment and has no screen interaction, so no validation attributes are applied
        public string VendorAcronym
        {
            get
            {
                return m_vendorAcronym;
            }
            set
            {
                m_vendorAcronym = value;
                OnPropertyChanged("VendorAcronym");
                
            }
        }

        /// <summary>
        /// Gets or sets <see cref="Device"/> ProtocolName.
        /// </summary>
        // Field is populated by database via auto-increment and has no screen interaction, so no validation attributes are applied
        public string ProtocolName
        {
            get
            {
                return m_protocolName;
            }
            set
            {
                m_protocolName = value;
                OnPropertyChanged("ProtocolName");
            }
        }

        /// <summary>
        /// Gets or sets <see cref="Device"/> InterconnectionName.
        /// </summary>
        // Field is populated by database via auto-increment and has no screen interaction, so no validation attributes are applied
        public string InterconnectionName
        {
            get
            {
                return m_interconnectionName;
            }
            set
            {
                m_interconnectionName = value;
                OnPropertyChanged("InterconnectionName");
            }
        }

        /// <summary>
        /// Gets or sets <see cref="Device"/> NodeName.
        /// </summary>
        // Field is populated by database via auto-increment and has no screen interaction, so no validation attributes are applied
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
        /// Gets or sets <see cref="Device"/> ParentAcronym.
        /// </summary>
        // Field is populated by database via auto-increment and has no screen interaction, so no validation attributes are applied
        public string ParentAcronym
        {
            get
            {
                return m_ParentAcronym;
            }
            set
            {
                m_ParentAcronym = value;
                OnPropertyChanged("ParentAcronym");
                
            }
        }

        /// <summary>
        /// Gets or sets <see cref="Device"/> CreatedOn.
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
        /// Gets or sets <see cref="Device"/> CreatedBy.
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
        ///  Gets or sets <see cref="Device"/> UpdatedOn.
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
                OnPropertyChanged("UpdatedOn");
            }
        }

        /// <summary>
        ///  Gets or sets <see cref="Device"/> UpdatedBy.
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
