//******************************************************************************************************
//  ClaimAttribute.cs - Gbtc
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
//  09/22/2021 - Billy Ernest
//       Generated original version of source code.
//
//******************************************************************************************************



using System;
using System.Linq;
using System.Security.Claims;

namespace GSF.Data.Model
{
    /// <summary>
    /// Defines an attribute that will allow setting claims for function roles for a modeled table in GSF.Web.Model.ModelController.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public sealed class ClaimAttribute : Attribute
    {
        /// <summary>
        /// Gets Claim to use for property.
        /// </summary>
        public Claim Claim { get; }

        /// <summary>
        /// Get Verb to use for property
        /// </summary>
        public string Verb { get; }

        /// <summary>
        /// Creates a new <see cref="ClaimAttribute"/> for use in GSF.Web.Model.ModelController"/>.
        /// </summary>
        /// <param name="verb">HTTP verbs of GET, POST, PATCH, or DELETE.</param>
        /// <param name="type">Type of claim, use specific <see cref="ClaimTypes"/> .</param>
        /// <param name="value">value of the claim.</param>
        public ClaimAttribute(string verb, string type, string value)
        {
            string[] verbs = new string[] { "GET", "POST", "PATCH", "DELETE" };
            Verb = verb.ToUpper();
            if (!verbs.Any(v => v == Verb))
                throw new Exception("Incorrect verbs used in defining model claims, please use GET, POST, PATCH, or DELETE");
            string[] claimTypes = typeof(ClaimTypes).GetFields().Where(x => x.FieldType.FullName == "System.String").Select(field => typeof(ClaimTypes).GetField(field.Name).GetRawConstantValue().ToString()).ToArray();
            if (!claimTypes.Any(c => c == type))
                throw new Exception("Incorrect type used in defining model type, please use specific System.Security.Claims.ClaimTypes when defining claims");

            Claim = new Claim(type, value);
        }
    }
}