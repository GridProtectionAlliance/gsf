//******************************************************************************************************
//  ObjectToVisibilityConverter.cs - Gbtc
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
//  08/16/2011 - Mehulbhai P Thakkar
//       Generated original version of source code.
//  12/20/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace GSF.TimeSeries.UI.Converters
{
    /// <summary>
    /// Represents an <see cref="IValueConverter"/> class, which takes object value and converts to <see cref="System.Windows.Visibility"/> enumeration.
    /// </summary>
    public class ObjectToVisibilityConverter : IValueConverter
    {
        #region [ IValueConverter Members ]

        /// <summary>
        /// Converts object value to <see cref="System.Windows.Visibility"/> enumeration.
        /// </summary>
        /// <param name="value">Object value to be converted.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use in conversion.</param>
        /// <param name="culture">The culture to use in conversion.</param>
        /// <returns><see cref="System.Windows.Visibility"/> enumeration.</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((object)value != null)
            {
                // if value is int, then if it is greater than zero then, set it to true otherwise false.
                // Once we set value to boolean instead of integer, next code will take care of visibility.
                if (value is int)
                {
                    int temp;
                    if (int.TryParse(value.ToString(), out temp))
                    {
                        if (temp > 0)
                            value = true;
                        else
                            value = false;
                    }
                }

                // Hanlde boolean to visibility conversion here.
                if (value is bool)
                {
                    // if boolean parameter is provided and is true then invert original boolean value.
                    if (parameter != null && parameter is bool)
                    {
                        if ((bool)parameter)
                            value = !(bool)value;
                    }

                    if (!(bool)value)
                        return Visibility.Collapsed;
                }
            }

            return Visibility.Visible;
        }

        /// <summary>
        /// Converts <see cref="System.Windows.Visibility"/> to object.
        /// </summary>
        /// <param name="value"><see cref="System.Windows.Visibility"/> value to be converted.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use in conversion.</param>
        /// <param name="culture">The culture to use in conversion.</param>
        /// <returns>object value.</returns>
        /// <remarks>This method has not been implemented.</remarks>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }

        #endregion

    }
}
