//******************************************************************************************************
//  IndexToBooleanConverter.cs - Gbtc
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
//  04/19/2013 - Aniket Salver 
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Globalization;
using System.Windows.Data;

namespace GSF.TimeSeries.UI.Converters
{
    /// <summary>
    /// Represents an <see cref="IValueConverter"/> class, which takes string value and converts it to boolean.
    /// </summary>
    public class IndexToBooleanConverter : IValueConverter
    {
        #region [ IValueConverter Implementation ]

        /// <summary>
        /// Converts string to boolean value.
        /// </summary>
        /// <param name="value">String value to be converted.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use in conversion.</param>
        /// <param name="culture">The culture to use in conversion.</param>
        /// <returns>Boolean value.</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((object)value != null)
            {
                int temp = (int)value;
                if (temp > 0)
                    return false;
                else
                    return true;
            }
            else
                return false;
        }

        /// <summary>
        /// Converts boolean value to string.
        /// </summary>
        /// <param name="value">Boolean value to be converted.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use in conversion.</param>
        /// <param name="culture">The culture to use in conversion.</param>
        /// <returns>String value.</returns>
        /// <remarks>This method has not been implemented.</remarks>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }

        #endregion
    }

}
