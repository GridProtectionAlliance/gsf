//******************************************************************************************************
//  CommandMeasurement.cs - Gbtc
//
//  Copyright © 2016, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  11/18/2016 - Stephen C. Wills
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.ComponentModel;
using System.Configuration;
using System.Globalization;

namespace GSF.TimeSeries.Transport
{
    /// <summary>
    /// Model for measurements published by the subscriber.
    /// </summary>
    public class CommandMeasurement
    {
        /// <summary>
        /// Gets or sets the signal ID of the measurement.
        /// </summary>
        [Setting]
        public Guid SignalID { get; set; }

        /// <summary>
        /// Gets or sets the time that the measurement was taken.
        /// </summary>
        [Setting]
        [TypeConverter(typeof(DateTimeConverter))]
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Gets or sets the value of the measurement.
        /// </summary>
        [Setting]
        public double Value { get; set; }

        /// <summary>
        /// Gets or sets the flags that define the quality of the measurement.
        /// </summary>
        [Setting]
        public MeasurementStateFlags StateFlags { get; set; }

        #region [ DateTime Converter ]

        private class DateTimeConverter : TypeConverter
        {
            TypeConverter defaultConverter = TypeDescriptor.GetConverter(typeof(DateTime));

            public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
            {
                return defaultConverter.CanConvertFrom(context, sourceType);
            }

            public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
            {
                return defaultConverter.CanConvertTo(context, destinationType);
            }

            public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
            {
                return defaultConverter.ConvertFrom(context, culture, value);
            }

            public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
            {
                if (destinationType == typeof(string))
                    return ((DateTime)value).ToString("yyyy-MM-dd HH:mm:ss.fffffff");

                return base.ConvertTo(context, culture, value, destinationType);
            }
        }

        #endregion
    }
}
