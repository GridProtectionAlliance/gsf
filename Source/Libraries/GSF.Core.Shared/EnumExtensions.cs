//******************************************************************************************************
//  EnumExtensions.cs - Gbtc
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
//  12/21/2010 - Pinal C. Patel
//       Generated original version of source code based on code from Tom Fischer.
//  12/30/2010 - J. Ritchie Carroll
//       Corrections / additions.
//  12/30/2010 - Pinal C. Patel
//       Renamed GetEnumFromDescription() to GetEnumValueByDescription() to avoid confusion.
//  12/14/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.ComponentModel;
using System.Reflection;
using System.Text;
using GSF.Reflection;

namespace GSF
{
    /// <summary>
    /// Defines extension methods related to enumerations.
    /// </summary>
    public static class EnumExtensions
    {
        /// <summary>
        /// Retrieves the description of the value that this <see cref="Enum"/> represents extracted from the <see cref="DescriptionAttribute"/>, or the enumeration name
        /// if no description is available.
        /// </summary>
        /// <param name="enumeration"><see cref="Enum"/> to operate on.</param>
        /// <returns>Description of the <see cref="Enum"/> if specified, otherwise the <see cref="string"/> representation of this <paramref name="enumeration"/>.</returns>
        public static string GetDescription(this Enum enumeration)
        {
            string name = enumeration.ToString();
            string description = enumeration.GetType().GetField(name).GetDescription();

            if (!string.IsNullOrWhiteSpace(description))
                return description;

            return name;
        }

        /// <summary>
        /// Gets the enumeration constant for this value, if defined in the enumeration, or a default value.
        /// </summary>
        /// <typeparam name="T">Type of enumeration.</typeparam>
        /// <param name="value">Value to attempt to return from enumeration.</param>
        /// <param name="defaultValue">Value to return if enumeration value is not found.</param>
        /// <returns>Enumeration value specified, if found, or a default value.</returns>
        /// <remarks>
        /// If a default value is not defined, first enumeration value will be used.
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="value"/> is null.</exception>
        /// <exception cref="ArgumentException">
        /// <typeparamref name="T"/> is not an enum. -or-
        /// The type of <paramref name="value"/> is not of type <typeparamref name="T"/>. -or-
        /// The type of <paramref name="value"/> is not an underlying type of <typeparamref name="T"/>.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// <paramref name="value"/> is not type <see cref="T:System.SByte"/>, <see cref="T:System.Int16"/>,
        /// <see cref="T:System.Int32"/>, <see cref="T:System.Int64"/>, <see cref="T:System.Byte"/>, <see cref="T:System.UInt16"/>,
        /// <see cref="T:System.UInt32"/>, or <see cref="T:System.UInt64"/>, or <see cref="T:System.String"/>.
        /// </exception>
        public static T GetEnumValueOrDefault<T>(this object value, object defaultValue = null)
        {
            return (T)value.GetEnumValueOrDefault(typeof(T), defaultValue);
        }

        /// <summary>
        /// Gets the enumeration constant for value, if defined in the enumeration, or a default value.
        /// </summary>
        /// <param name="value">Value to attempt to return from enumeration.</param>
        /// <param name="type"><see cref="Type"/> of the enumeration.</param>
        /// <param name="defaultValue">Value to return if enumeration value is not found.</param>
        /// <returns>Enumeration value specified, if found, or a default value.</returns>
        /// <remarks>
        /// If a default value is not defined, first enumeration value will be used.
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="type"/> or <paramref name="value"/> is null.</exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="type"/> is not an enum. -or-
        /// The type of <paramref name="value"/> is not of type <paramref name="type"/>. -or-
        /// The type of <paramref name="value"/> is not an underlying type of <paramref name="type"/>.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// <paramref name="value"/> is not type <see cref="T:System.SByte"/>, <see cref="T:System.Int16"/>,
        /// <see cref="T:System.Int32"/>, <see cref="T:System.Int64"/>, <see cref="T:System.Byte"/>, <see cref="T:System.UInt16"/>,
        /// <see cref="T:System.UInt32"/>, or <see cref="T:System.UInt64"/>, or <see cref="T:System.String"/>.
        /// </exception>
        public static object GetEnumValueOrDefault(this object value, Type type, object defaultValue = null)
        {
            if (Enum.IsDefined(type, value))
                return value;

            return defaultValue ?? Enum.GetValues(type).GetValue(0);
        }

        /// <summary>
        /// Gets the enumeration of the specified <paramref name="type"/> whose description matches this <paramref name="description"/>.
        /// </summary>
        /// <param name="description">Description to be used for lookup of the enumeration.</param>
        /// <param name="type"><see cref="Type"/> of the enumeration.</param>
        /// <param name="ignoreCase"><c>true</c> to ignore case during the comparison; otherwise, <c>false</c>.</param>
        /// <returns>An enumeration of the specified <paramref name="type"/> if a match is found, otherwise null.</returns>
        /// <exception cref="ArgumentException">The <paramref name="type"/> is not an enumeration.</exception>
        public static object GetEnumValueByDescription(this string description, Type type, bool ignoreCase = false)
        {
            if (!type.IsEnum)
                throw new ArgumentException("Type must be an enum", nameof(type));

            foreach (object value in Enum.GetValues(type))
            {
                if (string.Compare(description, ((Enum)value).GetDescription(), ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal) == 0)
                    return value;
            }

            return null;
        }

        /// <summary>
        /// Gets the enumeration value with the specified name.
        /// </summary>
        /// <param name="name">Name to search for.</param>
        /// <param name="type"><see cref="Type"/> of the enumeration.</param>
        /// <param name="ignoreCase"><c>true</c> to ignore case during the comparison; otherwise, <c>false</c>.</param>
        /// <returns>Specific value of the enumerated constant in terms of its underlying type associated with the specified <paramref name="name"/>, or <c>null</c>
        /// if no matching enumerated value was found.</returns>
        public static object GetEnumValueByName(this string name, Type type, bool ignoreCase = false)
        {
            if (!type.IsEnum)
                throw new ArgumentException("Type must be an enum", nameof(type));

            foreach (object value in Enum.GetValues(type))
            {
                if (string.Compare(name, ((Enum)value).ToString(), ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal) == 0)
                    return value;
            }

            return null;
        }

        /// <summary>
        /// Retrieves a formatted name of the value that this <see cref="Enum"/> represents for visual display.
        /// </summary>
        /// <param name="enumeration"><see cref="Enum"/> to operate on.</param>
        /// <returns>Formatted enumeration name of the specified value for visual display.</returns>
        public static string GetFormattedName(this Enum enumeration)
        {
            StringBuilder image = new StringBuilder();
            char[] chars = enumeration.ToString().ToCharArray();
            char letter;

            for (int i = 0; i < chars.Length; i++)
            {
                letter = chars[i];

                // Create word spaces at every capital letter
                if (Char.IsUpper(letter) && image.Length > 0)
                {
                    // Check for all caps sequence (e.g., ID)
                    if (Char.IsUpper(chars[i - 1]))
                    {
                        // Look ahead for proper breaking point
                        if (i + 1 < chars.Length)
                        {
                            if (Char.IsLower(chars[i + 1]))
                            {
                                image.Append(' ');
                                image.Append(letter);
                            }
                            else
                            {
                                image.Append(letter);
                            }
                        }
                        else
                        {
                            image.Append(letter);
                        }
                    }
                    else
                    {
                        image.Append(' ');
                        image.Append(letter);
                    }
                }
                else
                {
                    image.Append(letter);
                }
            }

            return image.ToString();
        }

        // Internal extension to lookup description from DescriptionAttribute
        private static string GetDescription(this FieldInfo value)
        {
            if ((object)value != null)
            {
                DescriptionAttribute descriptionAttribute;

                if (value.TryGetAttribute(out descriptionAttribute))
                    return descriptionAttribute.Description;
            }

            return string.Empty;
        }
    }
}
