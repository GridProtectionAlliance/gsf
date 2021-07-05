//******************************************************************************************************
//  ReturnLimitAttribute.cs - Gbtc
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
//  06/28/2021 - Billy Ernest
//       Generated original version of source code.
//
//******************************************************************************************************


using System;

namespace GSF.Data.Model
{
    /// <summary>
    /// Defines an attribute that will allows a top number of results to be returned when queried
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class ReturnLimitAttribute : Attribute
    {
        /// <summary>
        /// Gets field name to use for property.
        /// </summary>
        public int Limit
        {
            get;
        }

        /// <summary>
        /// Creates a new <see cref="ReturnLimitAttribute"/>.
        /// </summary>
        /// <param name="limit">Number of top records to return.</param>
        public ReturnLimitAttribute(int limit)
        {
            Limit = limit;
        }
    }
}