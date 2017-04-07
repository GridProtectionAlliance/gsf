//******************************************************************************************************
//  Device.cs - Gbtc
//
//  Copyright © 2017, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  03/29/2017 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.ComponentModel.DataAnnotations;
using GSF.Data.Model;
using System.ComponentModel;

namespace LoadImpedanceCalcs.Model
{
    public class Device
    {
        public Guid NodeID
        {
            get;
            set;
        }

        [PrimaryKey(true)]
        public int ID
        {
            get;
            set;
        }

        public int? ParentID
        {
            get;
            set;
        }

        public Guid UniqueID
        {
            get;
            set;
        }

        [Required]
        [StringLength(200)]
        public string Acronym
        {
            get;
            set;
        }

        [StringLength(200)]
        public string Name
        {
            get;
            set;
        }

        [StringLength(20)]
        public string OriginalSource
        {
            get;
            set;
        }

        public bool IsConcentrator
        {
            get;
            set;
        }

        public int? CompanyID
        {
            get;
            set;
        }

        public int? HistorianID
        {
            get;
            set;
        }

        public int AccessID
        {
            get;
            set;
        }

        public int? VendorDeviceID
        {
            get;
            set;
        }

        public int? ProtocolID
        {
            get;
            set;
        }

        public decimal? Longitude
        {
            get;
            set;
        }

        public decimal? Latitude
        {
            get;
            set;
        }

        public int? InterconnectionID
        {
            get;
            set;
        }

        public string ConnectionString
        {
            get;
            set;
        }

        [StringLength(200)]
        public string TimeZone
        {
            get;
            set;
        }

        public int? FramesPerSecond
        {
            get;
            set;
        }

        public long TimeAdjustmentTicks
        {
            get;
            set;
        }

        [DefaultValue(5.0D)]
        public double DataLossInterval
        {
            get;
            set;
        }

        [DefaultValue(10)]
        public int AllowedParsingExceptions
        {
            get;
            set;
        }

        [DefaultValue(5.0D)]
        public double ParsingExceptionWindow
        {
            get;
            set;
        }

        [DefaultValue(5)]
        public double DelayedConnectionInterval
        {
            get;
            set;
        }

        [DefaultValue(true)]
        public bool AllowUseOfCachedConfiguration
        {
            get;
            set;
        }

        [DefaultValue(true)]
        public bool AutoStartDataParsingSequence
        {
            get;
            set;
        }

        public bool SkipDisableRealTimeData
        {
            get;
            set;
        }

        [DefaultValue(100000)]
        public int MeasurementReportingInterval
        {
            get;
            set;
        }

        [DefaultValue(true)]
        public bool ConnectOnDemand
        {
            get;
            set;
        }

        public string ContactList
        {
            get;
            set;
        }

        public int? MeasuredLines
        {
            get;
            set;
        }

        public int LoadOrder
        {
            get;
            set;
        }

        public bool Enabled
        {
            get;
            set;
        }

        public DateTime CreatedOn
        {
            get;
            set;
        }

        [Required]
        [StringLength(50)]
        public string CreatedBy
        {
            get;
            set;
        }

        public DateTime UpdatedOn
        {
            get;
            set;
        }

        [Required]
        [StringLength(50)]
        public string UpdatedBy
        {
            get;
            set;
        }
    }
}
