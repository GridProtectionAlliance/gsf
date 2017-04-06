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
using System.ComponentModel;

// TODO: Consider renaming to InitialExpressionAttribute, adding a target language property and moving to another namespace, attribute is not directly used by TableOperations
namespace GSF.Data.Model
{ 
    /// <summary>
    /// Defines an attribute that will define an initial value expression for a modeled table field
    /// that will get evaluated and assigned in the target use environment, e.g., Javascript.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Example to set initial value to 30 days from now using a Javascript expression:
    /// <c>[InitialValue("(new Date()).addDays(30)")]</c>
    /// </para>
    /// <para>
    /// Note that the <see cref="DefaultValueAttribute"/> should be used to set any constant values
    /// that should be set for new modeled record instances, these will get assigned when using the
    /// <see cref="TableOperations{T}.NewRecord"/> function. The <see cref="InitialValueAttribute"/>
    /// is used to initialize the property value using an expression that gets evaluated in the
    /// target environment, e.g., Javascript in a web page.
    /// </para>
    /// </remarks>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class InitialValueAttribute : Attribute
    {
        /// <summary>
        /// Gets the initial value expression for a modeled table field.
        /// </summary>
        /// <remarks>
        /// Expression should be in target language, e.g., Javascript.
        /// </remarks>
        public string InitialValue
        {
            get;
        }

        /// <summary>
        /// Creates a new <see cref="InitialValueAttribute"/>/
        /// </summary>
        /// <param name="initialValue">Initial value expression for a modeled table field.</param>
        public InitialValueAttribute(string initialValue)
        {
            InitialValue = initialValue;
        }
    }
}