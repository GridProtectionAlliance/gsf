//******************************************************************************************************
//  DataSourceValueGroup.cs - Gbtc
//
//  Copyright © 2017, Grid Protection Alliance.  All Rights Reserved.
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
//  02/14/2017 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System.Collections.Generic;

namespace GrafanaAdapters
{
    /// <summary>
    /// Defines a class that represents an enumeration of <see cref="DataSourceValue"/> for a given target.
    /// </summary>
    /// <remarks>
    /// This is a group construct keyed on <see cref="Target"/> for data source value enumerations.
    /// </remarks>
    public class DataSourceValueGroup
    {
        /// <summary>
        /// Query target, e.g., a point-tag, representative of all <see cref="Source"/> values.
        /// </summary>
        public string Target;

        /// <summary>
        /// Defines the root target expression, without any referenced series functions.
        /// </summary>
        public string RootTarget;

        /// <summary>
        /// Defines a reference to the original target that was the source of these results.
        /// </summary>
        public Target SourceTarget;

        /// <summary>
        /// Data source values enumerable.
        /// </summary>
        public IEnumerable<DataSourceValue> Source;

        /// <summary>
        /// Defines flag that determines if empty series are produced.
        /// </summary>
        public bool DropEmptySeries;
    }
}
