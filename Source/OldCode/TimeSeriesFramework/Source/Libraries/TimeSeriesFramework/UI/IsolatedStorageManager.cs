//******************************************************************************************************
//  IsolatedStorageManager.cs - Gbtc
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
//  09/02/2011 - Mehulbhai P Thakkar
//       Generated original version of source code.
//
//******************************************************************************************************

using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Text;

namespace TimeSeriesFramework.UI
{
    /// <summary>
    /// Static class to read/write data from/to IsolatedStorage.
    /// </summary>
    public static class IsolatedStorageManager
    {
        private static IsolatedStorageFile s_userStoreForAssembly = IsolatedStorageFile.GetUserStoreForAssembly();

        /// <summary>
        /// Writes collection values by converting collection to semi-colon seperated string to IsolatedStorage.
        /// </summary>
        /// <param name="key">Name of the isolated storage.</param>
        /// <param name="valueList"><see cref="IEnumerable{T}"/> collection to be stored in isolated storage.</param>
        public static void WriteCollectionToIsolatedStorage(string key, IEnumerable<object> valueList)
        {
            using (StreamWriter writer = new StreamWriter(new IsolatedStorageFileStream(key, FileMode.Create, s_userStoreForAssembly)))
            {
                StringBuilder sb = new StringBuilder();
                foreach (object value in valueList)
                    sb.Append(value.ToString() + ";");

                writer.Write(sb.ToString());
            }
        }

        /// <summary>
        /// Writes to isolated storage.
        /// </summary>
        /// <param name="key">Name of the isolated storage.</param>
        /// <param name="value">Value to be written to isolated storage.</param>
        public static void WriteToIsolatedStorage(string key, object value)
        {
            using (StreamWriter writer = new StreamWriter(new IsolatedStorageFileStream(key, FileMode.Create, s_userStoreForAssembly)))
                writer.Write(value.ToString());
        }

        /// <summary>
        /// Reads from isolated storage.
        /// </summary>
        /// <param name="key">Name of the isolated storage to read from.</param>
        /// <returns>Object from the isolated storage.</returns>
        public static object ReadFromIsolatedStorage(string key)
        {
            using (StreamReader reader = new StreamReader(new IsolatedStorageFileStream(key, FileMode.OpenOrCreate, s_userStoreForAssembly)))
            {
                string result;
                if (reader != null)
                {
                    result = reader.ReadToEnd();
                    return result;
                }
                else
                    return null;
            }
        }
    }
}
