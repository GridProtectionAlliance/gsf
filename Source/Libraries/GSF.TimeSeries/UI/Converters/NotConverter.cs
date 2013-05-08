//******************************************************************************************************
//  NotConverter.cs - Gbtc
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
//  03/24/2011 - Mehulbhai P Thakkar
//       Generated original version of source code.
//  12/20/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Globalization;
using System.Windows.Data;

namespace GSF.TimeSeries.UI.Converters
{
    /// <summary>
    /// Represents an <see cref="IValueConverter"/> to invert boolean value.
    /// </summary>
    [ValueConversion(typeof(bool), typeof(bool))]
    public class NotConverter : IValueConverter
    {
        #region [ IValueConverter Members ]
        
        /// <summary>
        /// Inverts value of boolean object.
        /// </summary>
        /// <param name="value">Boolean object to be inverted.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>Boolean value for UI use.</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return InvertBoolean(value);
        }

        /// <summary>
        /// Inverts value of boolean object.
        /// </summary>
        /// <param name="value">Boolean object to be inverted.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>Boolean value for UI use.</returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return InvertBoolean(value);
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Inverts value of boolean object.
        /// </summary>
        /// <param name="value">boolean value to be inverted.</param>
        /// <returns></returns>
        private static object InvertBoolean(object value)
        {
            return !value.ToString().ParseBoolean();
        }

        #endregion
    }
}
