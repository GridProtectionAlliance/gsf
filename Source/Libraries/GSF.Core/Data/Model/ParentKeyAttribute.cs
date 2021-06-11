//******************************************************************************************************
//  ParentKeyAttribute.cs - Gbtc
//
//  Copyright © 2021, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may not use this
//  file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  06/11/2021 - Billy Ernest
//       Generated original version of source code.
//
//******************************************************************************************************


using System;

namespace GSF.Data.Model
{
    /// <summary>
    /// Defines an attribute that will allow a foreign key in the model to point back to parent table 
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class ParentKeyAttribute : Attribute 
    {
        /// <summary>
        /// Gets field name to use for property.
        /// </summary>
        public Type Model
        {
            get;
        }


        /// <summary>
        /// Creates a new <see cref="ParentKeyAttribute"/>.
        /// </summary>
        /// <param name="model">Type of modeled table that key points back to.</param>
        public ParentKeyAttribute(Type model)
        {
            Model = model;
        }
    }
}