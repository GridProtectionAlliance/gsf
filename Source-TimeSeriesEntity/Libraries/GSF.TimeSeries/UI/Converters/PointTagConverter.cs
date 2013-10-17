//******************************************************************************************************
//  PointTagConverter.cs - Gbtc
//
//  Copyright © 2013, Grid Protection Alliance.  All Rights Reserved.
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
//  08/11/2013 - Stephen C. Wills
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Globalization;
using System.Windows.Data;

namespace GSF.TimeSeries.UI.Converters
{
    /// <summary>
    /// Converts a point tag of the format 'ACRO1!ACRO2!ACRO3!ACRO4!ETC:XXX#'
    /// to the shortened form 'ACRO1!...!ETC:XXX#'.
    /// </summary>
    public class PointTagConverter : IValueConverter
    {
        /// <summary>
        /// Converts a value. 
        /// </summary>
        /// <returns>
        /// A converted value. If the method returns null, the valid null value is used.
        /// </returns>
        /// <param name="value">The value produced by the binding source.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string pointTag = value as string;

            if ((object)pointTag != null)
            {
                if (pointTag.CharCount('!') > 2)
                {
                    int start = pointTag.IndexOf('!');
                    int stop = pointTag.LastIndexOf('!');

                    // Handle statistics as a special case
                    if (pointTag.Contains(":ST"))
                    {
                        stop--;

                        while (pointTag[stop] != '!')
                            stop--;
                    }

                    if (start == stop)
                        return value;

                    return pointTag.Substring(0, start + 1) + "..." + pointTag.Substring(stop);
                }
            }

            return value;
        }

        /// <summary>
        /// Not implemented.
        /// </summary>
        /// <returns>
        /// Not implemented.
        /// </returns>
        /// <param name="value">The value that is produced by the binding target.</param>
        /// <param name="targetType">The type to convert to.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
