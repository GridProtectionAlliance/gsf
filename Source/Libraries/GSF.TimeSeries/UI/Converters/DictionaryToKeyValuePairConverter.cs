//******************************************************************************************************
//  DictionaryToKeyValuePairConverter.cs - Gbtc
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
//  04/29/2011 - Mehulbhai P Thakkar
//       Generated original version of source code.
//  12/20/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace GSF.TimeSeries.UI.Converters
{
    /// <summary>
    /// Represents an <see cref="IValueConverter"/> class, which takes <see cref="Dictionary{TKey,TValue}"/> collection and returns <see cref="KeyValuePair{T1,T2}"/>    
    /// </summary>
    public class DictionaryToKeyValuePairConverter : IValueConverter
    {
        #region [ Methods ]

        #region [ IValueConverter Implementation ]

        /// <summary>
        /// Method to return KeyValuePair{T1,T2} from <see cref="Dictionary{T1,T2}"/> collection.
        /// </summary>
        /// <param name="value">object to be used as key to search from collection.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter of type <see cref="Dictionary{T1,T2}"/> to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>KeyValuePair{T1,T2} value.</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter != null)
            {
                CollectionViewSource viewSource = parameter as CollectionViewSource;

                if (viewSource.Source is Dictionary<int, string>)
                {
                    Dictionary<int, string> collection = (Dictionary<int, string>)viewSource.Source;

                    value = value ?? 0; // If we get null from database for optional field then assign default value so first item from the collection can be returned.

                    int key;
                    if (int.TryParse(value.ToString(), out key))
                    {
                        if (key == 0 && collection.Count > 0)
                            return collection.First();

                        foreach (KeyValuePair<int, string> item in collection)
                            if (item.Key == key)
                                return item;
                    }
                }
                else if (viewSource.Source is Dictionary<string, string>)
                {
                    Dictionary<string, string> collection = (Dictionary<string, string>)viewSource.Source;

                    value = value ?? string.Empty; // If we get null from database for optional field then assign default value so first item from the collection can be returned.

                    string key = value.ToString();

                    if (string.IsNullOrEmpty(key) && collection.Count > 0)
                        return collection.First();

                    foreach (KeyValuePair<string, string> item in collection)
                        if (item.Key == key)
                            return item;
                }
                else if (viewSource.Source is Dictionary<Guid, string>)
                {
                    Dictionary<Guid, string> collection = (Dictionary<Guid, string>)viewSource.Source;

                    value = value ?? Guid.Empty; // If we get null from database for optional field then assign default value so first item from the collection can be returned.

                    Guid key = Guid.Parse(value.ToString());

                    if (key == Guid.Empty && collection.Count > 0)
                        return collection.First();

                    foreach (KeyValuePair<Guid, string> item in collection)
                        if (item.Key == key)
                            return item;
                }
                else if (viewSource.Source is Dictionary<Type, string>)
                {
                    Dictionary<Type, string> collection = (Dictionary<Type, string>)viewSource.Source;

                    if ((object)value != null)
                    {
                        KeyValuePair<Type, string> item = collection.ToList().SingleOrDefault(pair => pair.Key.FullName == value.ToString());

                        if (!item.Equals(default(KeyValuePair<Type, string>)))
                            return item;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Method to return Key from KeyValuePair{T1,T2}.
        /// </summary>
        /// <param name="value">KeyValuePair{T1,T2} from which key needs to be returned.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>Integer, to bind to UI object.</returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((object)value != null)
            {
                if (value is KeyValuePair<int, string>)
                    return ((KeyValuePair<int, string>)value).Key == 0 ? (object)DBNull.Value : ((KeyValuePair<int, string>)value).Key;
                else if (value is KeyValuePair<string, string>)
                    return string.IsNullOrEmpty(((KeyValuePair<string, string>)value).Key) ? (object)DBNull.Value : ((KeyValuePair<string, string>)value).Key;
                else if (value is KeyValuePair<Guid, string>)
                    return ((KeyValuePair<Guid, string>)value).Key == Guid.Empty ? (object)DBNull.Value : ((KeyValuePair<Guid, string>)value).Key;
                else if (value is KeyValuePair<Type, string>)
                    return ((KeyValuePair<Type, string>)value).Key.FullName;
            }
            return null;
        }

        #endregion

        #endregion
    }
}
