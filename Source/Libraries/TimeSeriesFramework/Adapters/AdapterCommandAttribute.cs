//******************************************************************************************************
//  AdapterCommandAttribute.cs - Gbtc
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
//  09/02/2010 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;

namespace GSF.TimeSeries.Adapters
{
    /// <summary>
    /// Represents an attribute that allows a method in an <see cref="IAdapter"/> class to be exposed as
    /// an invokable command.
    /// </summary>
    /// <remarks>
    /// Only public methods will be exposed as invokable.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class AdapterCommandAttribute : Attribute
    {
        #region [ Members ]

        // Fields
        private readonly string m_description;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="AdapterCommandAttribute"/> with the specified <paramref name="description"/> value.
        /// </summary>
        /// <param name="description">Assigns the description for this adapter command.</param>
        public AdapterCommandAttribute(string description)
        {
            m_description = description;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets the description of this adapter command.
        /// </summary>
        public string Description
        {
            get
            {
                return m_description;
            }
        }

        #endregion
    }
}