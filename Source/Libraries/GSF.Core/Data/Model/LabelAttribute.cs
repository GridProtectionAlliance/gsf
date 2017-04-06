//******************************************************************************************************
//  LabelAttribute.cs - Gbtc
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
//  01/30/2016 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;

// TODO: Consider moving to another namespace, attribute is not directly used by TableOperations
namespace GSF.Data.Model
{
    /// <summary>
    /// Defines an attribute that will define a UI label to used for a modeled table field.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class LabelAttribute : Attribute
    {
        /// <summary>
        /// Gets UI label for modeled table field.
        /// </summary>
        public string Label
        {
            get;
        }

        /// <summary>
        /// Creates a new <see cref="LabelAttribute"/>.
        /// </summary>
        /// <param name="label">UI label for modeled table field.</param>
        public LabelAttribute(string label)
        {
            Label = label;
        }
    }
}