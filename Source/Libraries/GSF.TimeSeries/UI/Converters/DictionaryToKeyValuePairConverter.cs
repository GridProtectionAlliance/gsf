//******************************************************************************************************
//  DictionaryToKeyValuePairConverter.cs - Gbtc
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
            if (parameter is null)
                return null;

            if (!(parameter is CollectionViewSource viewSource))
                return null;

            switch (viewSource.Source)
            {
                case Dictionary<int, string> intToStringMap:
                {
                    value = value ?? 0; // If we get null from database for optional field then assign default value so first item from the collection can be returned.

                    if (int.TryParse(value.ToString(), out int key))
                    {
                        if (key == 0 && intToStringMap.Count > 0)
                            return intToStringMap.First();

                        foreach (KeyValuePair<int, string> item in intToStringMap)
                        {
                            if (item.Key == key)
                                return item;
                        }
                    }

                    break;
                }
                case Dictionary<string, string> stringToStringMap:
                {
                    value = value ?? string.Empty; // If we get null from database for optional field then assign default value so first item from the collection can be returned.

                    string key = value.ToString();

                    if (string.IsNullOrEmpty(key) && stringToStringMap.Count > 0)
                        return stringToStringMap.First();

                    foreach (KeyValuePair<string, string> item in stringToStringMap)
                    {
                        if (item.Key == key)
                            return item;
                    }

                    break;
                }
                case Dictionary<Guid, string> guidToStringMap:
                {
                    value = value ?? Guid.Empty; // If we get null from database for optional field then assign default value so first item from the collection can be returned.

                    Guid key = Guid.Parse(value.ToString());

                    if (key == Guid.Empty && guidToStringMap.Count > 0)
                        return guidToStringMap.First();

                    foreach (KeyValuePair<Guid, string> item in guidToStringMap)
                    {
                        if (item.Key == key)
                            return item;
                    }

                    break;
                }
                case Dictionary<Type, string> typeToStringMap when value != null:
                {
                    KeyValuePair<Type, string> item = typeToStringMap.ToList().SingleOrDefault(pair => pair.Key.FullName == value.ToString());

                    if (!item.Equals(default(KeyValuePair<Type, string>)))
                        return item;

                    break;
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
            if (value is null)
                return null;

            switch (value)
            {
                case KeyValuePair<int, string> intStringPair:
                    return intStringPair.Key == 0 ? (object)DBNull.Value : intStringPair.Key;
                case KeyValuePair<string, string> stringStringPair:
                    return string.IsNullOrEmpty(stringStringPair.Key) ? (object)DBNull.Value : stringStringPair.Key;
                case KeyValuePair<Guid, string> guidStringPair:
                    return guidStringPair.Key == Guid.Empty ? (object)DBNull.Value : guidStringPair.Key;
                case KeyValuePair<Type, string> typeStringPair:
                    return typeStringPair.Key.FullName;
                default:
                    return null;
            }
        }

        #endregion

        #endregion
    }
}
