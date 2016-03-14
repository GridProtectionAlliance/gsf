//******************************************************************************************************
//  RecordRestriction.cs - Gbtc
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
//  03/13/2016 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

namespace GSF.Data.Model
{
    /// <summary>
    /// Defines a parameterized record restriction that can be applied to queries.
    /// </summary>
    public class RecordRestriction
    {
        #region [ Members ]

        // Fields

        /// <summary>
        /// Defines filter SQL expression for restriction - does not include WHERE.
        /// </summary>
        public string FilterExpression;

        /// <summary>
        /// Defines restriction parameter values.
        /// </summary>
        public object[] Parameters = new object[0];

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new parameterized <see cref="RecordRestriction"/>.
        /// </summary>
        public RecordRestriction()
        {
        }

        /// <summary>
        /// Creates a new parameterized <see cref="RecordRestriction"/> with the specified filter and parameters.
        /// </summary>
        /// <param name="filterExpression">Filter SQL expression for restriction - does not include WHERE.</param>
        /// <param name="parameters">Restriction parameter values.</param>
        public RecordRestriction(string filterExpression, params object[] parameters)
        {
            FilterExpression = filterExpression;
            Parameters = parameters;
        }

        #endregion
    }
}
