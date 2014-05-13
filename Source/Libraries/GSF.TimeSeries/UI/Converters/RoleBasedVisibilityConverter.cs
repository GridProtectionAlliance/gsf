//******************************************************************************************************
//  RoleBasedVisibilityConverter.cs - Gbtc
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
//  03/28/2011 - Mehulbhai P Thakkar
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
    /// Represents an <see cref="IValueConverter"/> to convert role to Visibility.
    /// </summary>
    public class RoleBasedVisibilityConverter : IValueConverter
    {
        #region [ Methods ]

        #region [ IValueConverter Implementation ]

        /// <summary>
        /// Converts string value to visibility.
        /// </summary>
        /// <param name="value">Value to be converted to visibility.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>One of the visibility enumeration</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((object)value == null || string.IsNullOrEmpty(value.ToString()) || value.ToString() == "*" || CommonFunctions.CurrentPrincipal.IsInRole(value.ToString()))
                return Visibility.Visible;
            else
                return Visibility.Collapsed;
        }

        /// <summary>
        /// Converts visibility to string, not implemented.
        /// </summary>
        /// <param name="value">Visibility value to be converted to string.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>String, for UI use.</returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion

        #endregion

    }
}
