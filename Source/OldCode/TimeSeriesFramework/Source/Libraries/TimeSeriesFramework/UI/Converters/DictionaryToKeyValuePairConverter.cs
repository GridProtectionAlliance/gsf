//******************************************************************************************************
//  DictionaryToKeyValuePairConverter.cs - Gbtc
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
//  04/29/2011 - Mehulbhai P Thakkar
//       Generated original version of source code.
//
//******************************************************************************************************

using System.Collections.Generic;
using System.Windows.Data;

namespace TimeSeriesFramework.UI.Converters
{
    /// <summary>
    /// Represents an <see cref="IValueConverter"/> class, which takes <see cref="Dictionary{T1,T2}"/> collection and returns <see cref="KeyValuePair{T1,T2}"/>    
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
        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value != null && parameter != null)
            {
                CollectionViewSource viewSource = parameter as CollectionViewSource;

                if (viewSource.Source is Dictionary<int, string>)
                {
                    Dictionary<int, string> collection = (Dictionary<int, string>)viewSource.Source;
                    int key;
                    if (int.TryParse(value.ToString(), out key))
                    {
                        foreach (KeyValuePair<int, string> item in collection)
                            if (item.Key == key)
                                return item;
                    }
                }
                else if (parameter is Dictionary<string, string>)
                {
                    Dictionary<string, string> collection = (Dictionary<string, string>)viewSource.Source;
                    string key = value.ToString();
                    foreach (KeyValuePair<string, string> item in collection)
                        if (item.Key == key)
                            return item;
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
        public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value != null)
            {
                if (value is KeyValuePair<int, string>)
                    return ((KeyValuePair<int, string>)value).Key;
                else if (value is KeyValuePair<string, string>)
                    return ((KeyValuePair<string, string>)value).Key;
            }
            return null;
        }

        #endregion

        #endregion
    }
}
