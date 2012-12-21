//******************************************************************************************************
//  EmailAddressValidator.cs - Gbtc
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
//  08/20/2010 - Pinal C. Patel
//       Generated original version of source code.
//  12/14/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System.Text.RegularExpressions;

namespace GSF.Validation
{
    /// <summary>
    /// Represents a validator that can be used to ensure the validity of an email address.
    /// </summary>
    public class EmailAddressValidator : IValidator
    {
        #region [ Members ]

        // Fields
        private const string EmailAddressRegex = @"\b[A-Z0-9._%-]+@[A-Z0-9.-]+\.[A-Z]{2,4}\b";

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Determines whether or not the specified <paramref name="value"/> is a valid email address.
        /// </summary>
        /// <param name="value">The value to be validated.</param>
        /// <param name="validationError">Error message returned if the <paramref name="value"/> is invalid.</param>
        /// <returns><strong>true</strong> if the <paramref name="value"/> is valid; otherwise <strong>false</strong>.</returns>
        public bool Validate(object value, out string validationError)
        {
            if (!Regex.IsMatch(value.ToString(), EmailAddressRegex, RegexOptions.IgnoreCase))
            {
                validationError = "Value must be in the format of 'someone@company.com'";
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
