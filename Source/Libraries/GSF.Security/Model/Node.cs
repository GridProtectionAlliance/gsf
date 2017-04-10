//******************************************************************************************************
//  Node.cs - Gbtc
//
//  Copyright © 2016, Grid Protection Alliance.  All Rights Reserved.
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
//  02/27/2016 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.ComponentModel.DataAnnotations;
using GSF.ComponentModel.DataAnnotations;
using GSF.Data.Model;

namespace GSF.Security.Model
{
    /// <summary>
    /// Model for Node table.
    /// </summary>
    [PrimaryLabel("Name")]
    public class Node
    {
        /// <summary>
        /// Unique node ID field.
        /// </summary>
        [PrimaryKey(true)]
        public Guid ID { get; set; }

        /// <summary>
        /// Name field.
        /// </summary>
        [Required]
        [StringLength(200)]
        public string Name { get; set; }

        /// <summary>
        /// Description field.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Enabled field.
        /// </summary>
        [InitialValueScript("true")]
        public bool Enabled { get; set; }

        /// <summary>
        /// Created on field.
        /// </summary>
        public DateTime CreatedOn { get; set; }

        /// <summary>
        /// Created by field.
        /// </summary>
        public string CreatedBy { get; set; }

        /// <summary>
        /// Updated on field.
        /// </summary>
        public DateTime UpdatedOn { get; set; }

        /// <summary>
        /// Updated by field.
        /// </summary>
        public string UpdatedBy { get; set; }
    }
}
