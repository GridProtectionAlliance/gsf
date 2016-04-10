//******************************************************************************************************
//  IDLabel.cs - Gbtc
//
//  Copyright © 2016, Grid Protection Alliance.  All Rights Reserved.
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
//  04/01/2016 - Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

namespace GSF.Web.Model
{
    /// <summary>
    /// Defines a ID and label object for JSON serialized data in dynamic lookups.
    /// </summary>
    /// <remarks>
    /// Useful for creating serialized JSON like "[{ id: "value", label : "name" }, ...]" that can be used in dynamic lookup lists.
    /// </remarks>
    public class IDLabel
    {
        /// <summary>
        /// ID value.
        /// </summary>
        public string id;

        /// <summary>
        /// Label value.
        /// </summary>
        public string label;

        /// <summary>
        /// Creates a new <see cref="IDLabel"/>.
        /// </summary>
        /// <param name="id">ID value.</param>
        /// <param name="label">Label value.</param>
        public IDLabel(string id, string label)
        {
            this.id = id;
            this.label = label;
        }

        /// <summary>
        /// Creates a new <see cref="IDLabel"/> instance.
        /// </summary>
        /// <param name="id">ID value.</param>
        /// <param name="label">Label value.</param>
        /// <returns>New <see cref="Label"/> instance.</returns>
        public static IDLabel Create(string id, string label)
        {
            return new IDLabel(id, label);
        }
    }
}