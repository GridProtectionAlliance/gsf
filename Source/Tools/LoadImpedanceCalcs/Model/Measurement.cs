//******************************************************************************************************
//  Measurement.cs - Gbtc
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
using GSF;

namespace LoadImpedanceCalcs.Model
{
    public class Measurement
    {
        [PrimaryKey(true)]
        public int PointID { get; set; }

        [DefaultValueExpression("Guid.NewGuid()")]
        public Guid SignalID { get; set; }

        public int? HistorianID { get; set; }

        public int? DeviceID { get; set; }

        [Required]
        [StringLength(200)]
        public string PointTag { get; set; }

        public string AlternateTag { get; set; }

        //[DefaultValueExpression("Connection.ExecuteScalar('SELECT ID FROM SignalType WHERE Acronym=\\'CALC\\'')")]
        [DefaultValueExpression("NotNull(Connection.ExecuteScalar('SELECT ID FROM SignalType WHERE Acronym=\\'CALC\\''), 10)")]
        public int SignalTypeID { get; set; }

        public int? PhasorSourceIndex { get; set; }

        [Required]
        [StringLength(200)]
        public string SignalReference { get; set; }

        [DefaultValue(0.0D)]
        public double Adder { get; set; }

        [DefaultValue(1.0D)]
        public double Multiplier { get; set; }

        public string Description { get; set; }

        [DefaultValue(true)]
        public bool Internal { get; set; }

        public bool Subscribed { get; set; }

        public bool Enabled { get; set; }

        [DefaultValueExpression("DateTime.UtcNow")]
        public DateTime CreatedOn { get; set; }

        [Required]
        [StringLength(200)]
        [DefaultValueExpression("UserInfo.CurrentUserID")]
        public string CreatedBy { get; set; }

        [DefaultValueExpression("DateTime.UtcNow")]
        public DateTime UpdatedOn { get; set; }

        [Required]
        [StringLength(200)]
        [DefaultValueExpression("UserInfo.CurrentUserID")]
        public string UpdatedBy { get; set; }
    }
}
