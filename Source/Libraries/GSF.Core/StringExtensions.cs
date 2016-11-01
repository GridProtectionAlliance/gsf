//******************************************************************************************************
//  StringExtensions.cs - Gbtc
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
//  02/23/2003 - J. Ritchie Carroll
//       Generated original version of source code.
//  01/24/2006 - J. Ritchie Carroll
//       Migrated 2.0 version of source code from 1.1 source (GSF.Shared.String).
//  06/01/2006 - J. Ritchie Carroll
//       Added ParseBoolean function to parse strings representing boolean's that may be numeric.
//  07/07/2006 - J. Ritchie Carroll
//       Added GetStringSegments function to break a string up into smaller chunks for parsing.
//       and/or displaying.
//  08/02/2007 - J. Ritchie Carroll
//       Added a CenterText method for centering strings in console applications or fixed width fonts.
//  08/03/2007 - Pinal C. Patel
//       Modified the CenterText method to handle multiple lines.
//  08/21/2007 - Darrell Zuercher
//       Edited code comments.
//  09/25/2007 - J. Ritchie Carroll
//       Added TitleCase function to format a string with the first letter of each word capitalized.
//  04/16/2008 - Pinal C. Patel
//       Made the keys of the string dictionary returned by ParseKeyValuePairs function case-insensitive.
//       Added JoinKeyValuePairs overloads that does the exact opposite of ParseKeyValuePairs.
//  09/19/2008 - J. Ritchie Carroll
//       Converted to C# extensions.
//  12/13/2008 - F. Russell Roberson
//       Generalized ParseBoolean to include "Y", and "T".
//       Added IndexOfRepeatedChar - Returns the index of the first character that is repeated.
//       Added Reverse - Reverses the order of characters in a string.
//       Added EnsureEnd - Ensures that a string ends with a specified char or string.
//       Added EnsureStart - Ensures that a string begins with a specified char or string.
//       Added IsNumeric - Test to see if a string only includes characters that can be interpreted as a number.
//       Added TrimWithEllipsisMiddle - Adds an ellipsis in the middle of a string as it is reduced to a specified length.
//       Added TrimWithEllipsisEnd - Trims a string to not exceed a fixed length and adds a ellipsis to string end.
//  02/10/2009 - J. Ritchie Carroll
//       Added ConvertToType overloaded extensions.
//  02/17/2009 - Josh L. Patterson
//       Edited Code Comments.
//  09/14/2009 - Stephen C. Wills
//       Added new header and license agreement.
//  01/11/2010 - Galen K. Riley
//      Issue fixes for unit tests:
//      ConvertToType - Fix to throw ArgumentNullException instead of NullReferenceException for null value
//      ConvertToType - Handling failed conversions better. Calling ConvertToType<int>("\0") returns properly
//      JoinKeyValuePairs - Fix to throw ArgumentNullException instead of NullReferenceException
//      ReplaceCharacters - Fix to throw ArgumentNullException instead of NullReferenceException for null replacementCharacter
//      RemoveCharacters - Fix to throw ArgumentNullException instead of NullReferenceException for null characterTestFunction
//      ReplaceCrLfs - Fix to throw ArgumentNullException instead of NullReferenceException for null value
//      RegexDecode - Fix to throw ArgumentNullException instead of NullReferenceException for null value
//  12/03/2010 - J. Ritchie Carroll
//      Modified ParseKeyValuePairs such that it could handle nested pairs to any needed depth.
//  12/05/2010 - Pinal C. Patel
//       Added an overload for ConvertToType() that takes CultureInfo as a parameter.
//  12/07/2010 - Pinal C. Patel
//       Updated ConvertToType() to return the type default if passed in string is null or empty.
//  01/04/2011 - J. Ritchie Carroll
//       Modified ConvertToType culture default to InvariantCulture for English style parsing defaults.
//  01/14/2011 - J. Ritchie Carroll
//       Modified JoinKeyValuePairs to delineate values that contain nested key/value pair expressions
//       such that the generated expression can correctly parsed.
//  03/23/2011 - J. Ritchie Carroll
//       Modified ParseKeyValuePairs to optionally ignore duplicate keys (default behavior now).
//       Removed overloads for ParseKeyValuePairs and JoinKeyValuePairs using optional parameters.
//  08/02/2011 - Pinal C. Patel
//       Added RemoveInvalidFileNameCharacters() and ReplaceInvalidFileNameCharacters() methods.
//  10/17/2012 - F Russell Robertson
//       Added QuoteWrap() method.
//  12/14/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//  10/27/2016 - Steven E. Chisholm
//       Split this class. GSF.StringExtensions is now located in the GSF Shared Project. 
//       What remains is code that cannot be hosted inside a SQL CLR process.
//
//******************************************************************************************************

using System.Data.Entity.Design.PluralizationServices;
using System.Globalization;

namespace GSF
{
    //------------------------------------------------------------------------------------------------------------
    // Note: This code has been moved to the GSF.Core shared project. Only put string extension methods here
    //       that cannot be hosted inside of a SQL CLR process.
    //------------------------------------------------------------------------------------------------------------

    /// <summary>Defines extension functions related to string manipulation.</summary>
    public static partial class StringExtensions
    {
        private static readonly PluralizationService s_pluralizationService;

        static StringExtensions()
        {
            // Pluralization service currently only supports English, if other languages are supported in the
            // future, cached services can use to a concurrent dictionary keyed on LCID of culture
            s_pluralizationService = PluralizationService.CreateService(CultureInfo.GetCultureInfo("en-us"));
        }

        /// <summary>
        /// Returns the singular form of the specified word.
        /// </summary>
        /// <param name="value">The word to be made singular.</param>
        /// <returns>The singular form of the input parameter.</returns>
        public static string ToSingular(this string value)
        {
            if (string.IsNullOrEmpty(value))
                return "";

            return s_pluralizationService.Singularize(value);
        }

        /// <summary>
        /// Determines whether the specified word is singular.
        /// </summary>
        /// <param name="value">The word to be analyzed.</param>
        /// <returns><c>true</c> if the word is singular; otherwise, <c>false</c>.</returns>
        public static bool IsSingular(this string value)
        {
            if (string.IsNullOrEmpty(value))
                return false;

            return s_pluralizationService.IsSingular(value);
        }

        /// <summary>
        /// Returns the plural form of the specified word.
        /// </summary>
        /// <param name="value">The word to be made plural.</param>
        /// <returns>The plural form of the input parameter.</returns>
        public static string ToPlural(this string value)
        {
            if (string.IsNullOrEmpty(value))
                return "";

            return s_pluralizationService.Pluralize(value);
        }

        /// <summary>
        /// Determines whether the specified word is plural.
        /// </summary>
        /// <param name="value">The word to be analyzed.</param>
        /// <returns><c>true</c> if the word is plural; otherwise, <c>false</c>.</returns>
        public static bool IsPlural(this string value)
        {
            if (string.IsNullOrEmpty(value))
                return false;

            return s_pluralizationService.IsPlural(value);
        }

    }
}