//******************************************************************************************************
//  VerboseLevel.cs - Gbtc
//
//  Copyright © 2016, Grid Protection Alliance.  All Rights Reserved.
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
//  10/24/2016 - Steven E. Chisholm
//       Generated original version of source code. 
//       
//
//******************************************************************************************************


namespace GSF.Diagnostics
{
    /// <summary>
    /// General Verbose Levels exposed to the user for application logging.
    /// </summary>
    public enum VerboseLevel
    {
        /// <summary>
        /// None
        /// </summary>
        None = 0,
        /// <summary>
        /// Component=Error, Framework=Error, Application=Warning
        /// </summary>
        Low = 1,
        /// <summary>
        /// Component=Warning, Framework=Warning, Application=Info
        /// </summary>
        Medium = 2,
        /// <summary>
        /// Component=Info, Framework=Info, Application=Debug
        /// </summary>
        High = 3,
        /// <summary>
        /// Component=Debug, Framework=Debug, Application=Debug
        /// </summary>
        Ultra = 4,
        /// <summary>
        /// Component=Debug, Framework=Debug, Application=Debug, Include Suppressed Logs.
        /// </summary>
        All = 5
    }
}