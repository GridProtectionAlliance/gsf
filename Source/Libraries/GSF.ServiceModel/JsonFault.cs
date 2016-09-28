//******************************************************************************************************
//  JsonFault.cs - Gbtc
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
//  09/28/2016 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Runtime.Serialization;

namespace GSF.ServiceModel
{
    /// <summary>
    /// Defines a class for JSON faults.
    /// </summary>
    [DataContract]
    public class JsonFault
    {
        /// <summary>
        /// Creates a new <see cref="JsonFault"/> from an existing <see cref="Exception"/>.
        /// </summary>
        /// <param name="ex">Source exception.</param>
        public JsonFault(Exception ex)
        {
            Error = ex.Message;
            Source = ex.Source;
            FaultType = ex.GetType().Name;
        }

        /// <summary>
        /// Exception message.
        /// </summary>
        [DataMember]
        public string Error { get; set; }

        /// <summary>
        /// Exception source.
        /// </summary>
        [DataMember]
        public string Source { get; set; }

        /// <summary>
        /// Fault exception type.
        /// </summary>
        [DataMember]
        public string FaultType { get; set; }
    }
}
