//******************************************************************************************************
//  Company.cs - Gbtc
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
//  03/25/2011 - Mehulbhai P Thakkar
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Windows.Input;
using TimeSeriesFramework.UI.Commands;

namespace TimeSeriesFramework.UI.DataModels
{
    /// <summary>
    /// Represents Company information as defined in the database.
    /// </summary>
    public class Company : DataModelBase
    {
        #region [ Members ]

        // Fields        
        private string m_error;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates an instance of <see cref="Company"/> with default values defined in the <see cref="DefaultValueAttribute"/>.
        /// </summary>
        public Company()
        {
            foreach (PropertyDescriptor property in TypeDescriptor.GetProperties(this))
            {
                // Set default value if DefaultValueAttribute is present
                DefaultValueAttribute attr = property.Attributes[typeof(DefaultValueAttribute)] as DefaultValueAttribute;
                if (attr != null)
                    property.ResetValue(this);
            }
        }

        #endregion

        #region [ Properties ]

        public int ID
        {
            get { return GetValue(() => ID); }
            set { SetValue(() => ID, value); }
        }

        [Required(ErrorMessage = "Please provide Acronym value.")]
        public string Acronym
        {
            get { return GetValue(() => Acronym); }
            set { SetValue(() => Acronym, value); }
        }

        [Required(ErrorMessage = "Please provide Map Acronym.")]
        [StringLength(3, ErrorMessage = "Map Acronym cannot exceed 3 characters.")]
        public string MapAcronym
        {
            get { return GetValue(() => MapAcronym); }
            set { SetValue(() => MapAcronym, value); }
        }

        public string Name
        {
            get { return GetValue(() => Name); }
            set { SetValue(() => Name, value); }
        }

        [DataType(DataType.Url, ErrorMessage = "Invalid URL value.")]
        public string URL
        {
            get { return GetValue(() => URL); }
            set { SetValue(() => URL, value); }
        }

        [Required(ErrorMessage = "Load Order is required field.")]
        [DefaultValue(typeof(int), "0")]
        public int LoadOrder
        {
            get { return GetValue(() => LoadOrder); }
            set { SetValue(() => LoadOrder, value); }
        }

        public DateTime CreatedOn
        {
            get { return GetValue(() => CreatedOn); }
            set { SetValue(() => CreatedOn, value); }
        }

        public string CreatedBy
        {
            get { return GetValue(() => CreatedBy); }
            set { SetValue(() => CreatedBy, value); }
        }

        public DateTime UpdatedOn
        {
            get { return GetValue(() => UpdatedOn); }
            set { SetValue(() => UpdatedOn, value); }
        }

        public string UpdatedBy
        {
            get { return GetValue(() => UpdatedBy); }
            set { SetValue(() => UpdatedBy, value); }
        }

        #endregion
    }
}
