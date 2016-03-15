//******************************************************************************************************
//  InitialValueAttribute.cs - Gbtc
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

namespace GSF.Data.Model
{

    /// <summary>
    /// Defines an attribute that will define an initial value (Javascript based) for a modeled table field.
    /// </summary>
    /// <remarks>
    /// Example to set initial value to 30 days from now:
    /// <c>[InitialValue("(new Date()).addDays(30)")]</c>
    /// </remarks>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class InitialValueAttribute : Attribute
    {
        /// <summary>
        /// Gets Javascript based initial value for modeled table field.
        /// </summary>
        public string InitialValue
        {
            get;
        }

        /// <summary>
        /// Creates a new <see cref="InitialValueAttribute"/>/
        /// </summary>
        /// <param name="initialValue">Javascript based initial value for modeled table field.</param>
        public InitialValueAttribute(string initialValue)
        {
            InitialValue = initialValue;
        }
    }
}