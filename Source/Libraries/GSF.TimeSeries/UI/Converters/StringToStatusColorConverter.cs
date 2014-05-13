//******************************************************************************************************
//  StringToStatusColorConverter.cs - Gbtc
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
using System.Windows.Media;

namespace GSF.TimeSeries.UI.Converters
{
    /// <summary>
    /// Represents an <see cref="IValueConverter"/> class, which takes string value and returns <see cref="System.Windows.Media.Brush"/> object.
    /// </summary>
    public class StringToStatusColorConverter : IValueConverter
    {
        #region [ Methods ]

        #region [ IValueConverter Implementation ]

        /// <summary>
        /// Converts string value to <see cref="System.Windows.Media.Brush"/> object.
        /// </summary>
        /// <param name="value">string value to be converted.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use in the conversion.</param>
        /// <param name="culture">The culture to use in the conversion.</param>
        /// <returns>Brush object.</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string input = string.Empty;
            if ((object)value != null)
                input = value.ToString().ToLower();

            switch (input)
            {
                case "good":
                    return new SolidColorBrush(Color.FromArgb(255, 25, 200, 25));
                case "bad":
                    return new SolidColorBrush(Color.FromArgb(255, 200, 25, 25));
                case "green":
                    return Application.Current.Resources["GreenRadialGradientBrush"] as RadialGradientBrush;
                case "red":
                    return Application.Current.Resources["RedRadialGradientBrush"] as RadialGradientBrush;
                case "yellow":
                    return Application.Current.Resources["YellowRadialGradientBrush"] as RadialGradientBrush;
                case "gray":
                    return Application.Current.Resources["GrayRadialGradientBrush"] as RadialGradientBrush;
                case "transparent":
                    return new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
                default:
                    return new SolidColorBrush(Color.FromArgb(255, 200, 200, 200));
            }

        }

        /// <summary>
        /// Converts <see cref="System.Windows.Media.Brush"/> object to string value.
        /// </summary>
        /// <param name="value"><see cref="System.Windows.Media.Brush"/> object to be converted.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use in the conversion.</param>
        /// <param name="culture">The culture to use in the conversion.</param>
        /// <returns>string value.</returns>
        /// <remarks>This method is not implemented.</remarks>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }

        #endregion

        #endregion


    }
}
