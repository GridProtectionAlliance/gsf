//******************************************************************************************************
//  IIniFile.cs - Gbtc
//
//  Copyright © 2012, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://www.opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  08/25/2013 - J. Ritchie Carroll
//       Made INI file work for both Mono and Windows based implementations.
//
//******************************************************************************************************

namespace GSF.Interop
{
    /// <summary>
    /// Represents an interface for common INI functions.
    /// </summary>
    public interface IIniFile
    {
        /// <summary>
        /// File name of the INI file.
        /// </summary>
        string FileName { get; set; }

        /// <summary>
        /// Gets the value of the specified key.
        /// </summary>
        /// <param name="section">Section key exists in.</param>
        /// <param name="entry">Name of key.</param>
        /// <param name="defaultValue">Default value of key.</param>
        /// <returns>Value of key.</returns>
        /// <remarks>This is the default member of this class.</remarks>
        string this[string section, string entry, string defaultValue] { get; }

        /// <summary>
        /// Gets or sets the value of the specified key.
        /// </summary>
        /// <param name="section">Section key exists in.</param>
        /// <param name="entry">Name of key.</param>
        /// <value>The new key value to store in the INI file.</value>
        /// <returns>Value of key.</returns>
        /// <remarks>This is the default member of this class.</remarks>
        string this[string section, string entry] { set; }

        /// <summary>
        /// Gets an array of keys from the specified section in the INI file.
        /// </summary>
        /// <param name="section">Section to retrieve keys from.</param>
        /// <returns>Array of <see cref="string"/> keys from the specified section of the INI file.</returns>
        string[] GetSectionKeys(string section);

        /// <summary>
        /// Gets an array of that section names that exist in the INI file.
        /// </summary>
        /// <returns>Array of <see cref="string"/> section names from the INI file.</returns>
        string[] GetSectionNames();
    }
}