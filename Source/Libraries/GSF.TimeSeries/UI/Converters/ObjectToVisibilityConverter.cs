//******************************************************************************************************
//  ObjectToVisibilityConverter.cs - Gbtc
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
            // If value is int, any value greater than zero resolves to true; otherwise, false
            // Once we have boolean value instead of integer, next step will handle visibility
            if (value is int)
            {
                if (int.TryParse(value.ToString(), out int intVal))
                    value = intVal > 0;
            }

            // Handle boolean to visibility conversion
            if (value is bool boolVal)
            {
                // If parameter is provided and is 'true' boolean, invert original boolean value
                if (parameter is true)
                    boolVal = !boolVal;

                // If parameter is provided and is "Hidden" string, return hidden instead of collapsed
                if (!boolVal)
                    return parameter is "Hidden" ? Visibility.Hidden : Visibility.Collapsed;
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
    }
}
