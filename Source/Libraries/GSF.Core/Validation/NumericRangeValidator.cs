//******************************************************************************************************
//  NumericRangeValidator.cs - Gbtc
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

namespace GSF.Validation
{
    /// <summary>
    /// Represents a validator that can be used to ensure that a numeric value falls within a specific range.
    /// </summary>
    public class NumericRangeValidator : IValidator
    {
        #region [ Members ]

        // Fields
        private readonly decimal m_rangeMinimum;
        private readonly decimal m_rangeMaximum;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Initializes a new instance of the <see cref="NumericRangeValidator"/> class.
        /// </summary>
        /// <param name="minimum">The minimum allowed numeric value.</param>
        /// <param name="maximum">The maximum allowed numeric value.</param>
        public NumericRangeValidator(decimal minimum, decimal maximum)
        {
            m_rangeMinimum = minimum;
            m_rangeMaximum = maximum;
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Determines whether or not the specified <paramref name="value"/> is a valid number that falls within the specified range.
        /// </summary>
        /// <param name="value">The value to be validated.</param>
        /// <param name="validationError">Error message returned if the <paramref name="value"/> is invalid.</param>
        /// <returns><strong>true</strong> if the <paramref name="value"/> is valid; otherwise <strong>false</strong>.</returns>
        public bool Validate(object value, out string validationError)
        {
            decimal numericValue;

            if (decimal.TryParse(value.ToString(), out numericValue))
            {
                if (numericValue < m_rangeMinimum || numericValue > m_rangeMaximum)
                {
                    validationError = string.Format("Value must be between {0} and {1}", m_rangeMinimum, m_rangeMaximum);
                    return false;
                }

                validationError = string.Empty;
                return true;
            }

            validationError = "Value cannot be parsed as a number.";
            return false;
        }

        #endregion
    }
}
