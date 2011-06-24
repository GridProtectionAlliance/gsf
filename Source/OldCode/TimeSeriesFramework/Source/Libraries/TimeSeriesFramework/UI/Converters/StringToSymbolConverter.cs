//******************************************************************************************************
//  StringToSymbolConverter.cs - Gbtc
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
//  06/22/2011 - Magdiel Lorenzo
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows;

namespace TimeSeriesFramework.UI.Converters
{
    class StringToSymbolConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            //SolidColorBrush scBrush = new SolidColorBrush();            
            RadialGradientBrush scBrush = new RadialGradientBrush();

            if (value == null)
                return scBrush as Brush;

            string statusColor = value.ToString();

            if (statusColor.Contains("Green"))
                scBrush = Application.Current.Resources["GreenRadialGradientBrush"] as RadialGradientBrush;
            else if (statusColor.Contains("Gray"))
                scBrush = Application.Current.Resources["GrayRadialGradientBrush"] as RadialGradientBrush;
            else if (statusColor.Contains("Yellow"))
                scBrush = Application.Current.Resources["YellowRadialGradientBrush"] as RadialGradientBrush;
            else if (statusColor.Contains("Red"))
                scBrush = Application.Current.Resources["RedRadialGradientBrush"] as RadialGradientBrush;

            return scBrush as Brush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }
}
