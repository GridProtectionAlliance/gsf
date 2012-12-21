//******************************************************************************************************
//  NonNullStringValidator.cs - Gbtc
//
//  Copyright © 2012, Grid Protection Alliance.  All Rights Reserved.
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
//  08/19/2010 - Pinal C. Patel
//       Generated original version of source code.
//  12/14/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

namespace GSF.Validation
{
    /// <summary>
    /// Represents a validator that can be used to check for <see cref="string"/>s that are null, empty, or consists only of whitespaces.
    /// </summary>
    public class NonNullStringValidator : IValidator
    {
        #region [ Methods ]

        /// <summary>
        /// Determines whether or not the specified <paramref name="value"/> is a valid string that is not null, empty or consists only of whitespaces.
        /// </summary>
        /// <param name="value">The value to be validated.</param>
        /// <param name="validationError">Error message returned if the <paramref name="value"/> is invalid.</param>
        /// <returns><strong>true</strong> if the <paramref name="value"/> is valid; otherwise <strong>false</strong>.</returns>
        public bool Validate(object value, out string validationError)
        {
            if (string.IsNullOrWhiteSpace(value.ToString()))
            {
                validationError = string.Format("Value is null, empty, or consists only of whitespaces");
                return false;
            }
            else
            {
                validationError = string.Empty;
                return true;
            }
        }

        #endregion
    }
}
