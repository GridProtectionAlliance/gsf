//******************************************************************************************************
//  UrlValidationAttribute.cs - Gbtc
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
//  04/13/2017 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System.ComponentModel.DataAnnotations;

namespace GSF.ComponentModel.DataAnnotations
{
    /// <summary>
    /// Represents a <see cref="RegularExpressionAttribute"/> for validating URL's.
    /// </summary>
    public class UrlValidationAttribute : RegularExpressionAttribute
    {
        /// <summary>
        /// Defines the regular expression pattern used to validate value. 
        /// </summary>
        public const string ValidationPattern = @"^(?:(?:[a-zA-Z][a-zA-Z0-9.+-]*:\/\/)?[a-zA-Z0-9][a-zA-Z0-9.-]*(?::[0-9]+)?(?:\/[^ ""]*)?|mailto:[a-zA-Z0-9!#$%&'*+-\/=?^_`{|}~][a-zA-Z0-9!#$%&'*+-\/=?^_`{|}~.]*@[a-zA-Z0-9][a-zA-Z0-9.-]*)$";

        /// <summary>
        /// Creates a new <see cref="UrlValidationAttribute"/>.
        /// </summary>
        public UrlValidationAttribute() : base(ValidationPattern)
        {
            ErrorMessage = "Invalid URL.";
        }
    }
}
