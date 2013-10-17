//******************************************************************************************************
//  IPersistSettings.cs - Gbtc
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
//  03/21/2007 - Pinal C. Patel
//       Generated original version of source code.
//  09/16/2008 - Pinal C. Patel
//       Converted code to C#.
//  09/29/2008 - Pinal C. Patel
//       Reviewed code comments.
//  09/14/2009 - Stephen C. Wills
//       Added new header and license agreement
//  12/14/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

namespace GSF.Configuration
{
    /// <summary>
    /// Defines as interface that specifies that this object can persists settings to a config file.
    /// </summary>
    public interface IPersistSettings
    {
        /// <summary>
        /// Determines whether the object settings are to be persisted to the config file.
        /// </summary>
        bool PersistSettings { get; set; }

        /// <summary>
        /// Gets or sets the category name under which the object settings are persisted in the config file.
        /// </summary>
        string SettingsCategory { get; set; }

        /// <summary>
        /// Saves settings to the config file.
        /// </summary>
        void SaveSettings();

        /// <summary>
        /// Loads saved settings from the config file.
        /// </summary>
        void LoadSettings();
    }
}
