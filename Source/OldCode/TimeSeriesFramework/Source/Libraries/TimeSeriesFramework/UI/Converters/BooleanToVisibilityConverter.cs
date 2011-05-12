//******************************************************************************************************
//  BooleanToVisibilityConverter.cs - Gbtc
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
//  05/11/2011 - Mehulbhai P Thakkar
//       Generated original version of source code.
//
//******************************************************************************************************

using System.Windows;
using System.Windows.Data;

namespace TimeSeriesFramework.UI.Converters
{
    /// <summary>
    /// Represents an <see cref="IValueConverter"/> class, which takes boolean value and converts to Visibility enumberation.
    /// </summary>
    public class BooleanToVisibilityConverter : IValueConverter
    {
        #region [ IValueConverter Members ]

        /// <summary>
        /// Method to return Visibility enumeration from boolean value.
        /// </summary>
        /// <param name="value">object to be used for conversion.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter of type boolean to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>One of the Visibility enumerations.</returns>
        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null || !(value is bool))
                return Visibility.Collapsed;

            // We will use parameter as an indicator to invert original value.
            if (parameter != null && (parameter is bool))
            {
                if ((bool)parameter)
                    value = !(bool)value;
            }

            if ((bool)value)
                return Visibility.Visible;
            else
                return Visibility.Collapsed;
        }

        /// <summary>
        /// Method to return boolean from Visibility enumeration.
        /// </summary>
        /// <param name="value">Visibility enumeration used for conversion.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>bool, to bind to UI object.</returns>
        public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }

        #endregion
    }
}
