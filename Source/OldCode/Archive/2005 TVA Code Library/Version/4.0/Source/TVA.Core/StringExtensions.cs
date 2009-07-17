//*******************************************************************************************************
//  StringExtensions.cs
//  Copyright © 2008 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: James R. Carroll
//      Office: PSO TRAN & REL, CHATTANOOGA - MR 2W-C
//       Phone: 423/751-2827
//       Email: jrcarrol@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  02/23/2003 - James R. Carroll
//       Generated original version of source code.
//  01/24/2006 - James R. Carroll
//       Migrated 2.0 version of source code from 1.1 source (TVA.Shared.String).
//  06/01/2006 - James R. Carroll
//       Added ParseBoolean function to parse strings representing booleans that may be numeric.
//  07/07/2006 - James R. Carroll
//       Added GetStringSegments function to break a string up into smaller chunks for parsing
//       and/or displaying.
//  08/02/2007 - James R. Carroll
//       Added a CenterText method for centering strings in console applications or fixed width fonts.
//  08/03/2007 - Pinal C. Patel
//       Modified the CenterText method to handle multiple lines.
//  08/21/2007 - Darrell Zuercher
//       Edited code comments.
//  09/25/2007 - James R. Carroll
//       Added TitleCase function to format a string with the first letter of each word capitalized
//  04/16/2008 - Pinal C. Patel
//       Made the keys of the string dictionary returned by ParseKeyValuePairs function case-insensitive.
//       Added JoinKeyValuePairs overloads that does the exact opposite of ParseKeyValuePairs.
//  09/19/2008 - James R. Carroll
//       Converted to C# extensions.
//  12/13/2008 - F. Russell Roberson
//       Generalized ParseBoolean to include "Y", and "T"
//       Added IndexOfRepeatedChar - Returns the index of the first character that is repeated
//       Added Reverse - Reverses the order of characters in a string
//       Added EnsureEnd - Ensures that a string ends with a specified char or string
//       Added EnsureStart - Ensures that a string begins with a specified char or string
//       Added IsNumeric - Test to see if a string only includes characters that can be interpreted as a number.
//       Added TrimWithEllipsisMiddle - Adds an ellipsis in the middle of a string as it is reduced to a specified length
//       Added TrimWithEllipsisEnd - Trims a string to not exceed a fixed length and adds a ellipsis to string end
//  02/10/2009 - James R. Carroll
//       Added ConvertToType overloaded extensions.
//  02/17/2009 - Josh Patterson
//      Edited Code Comments
//
//*******************************************************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Text.RegularExpressions;

namespace TVA
{
    /// <summary>Defines extension functions related to string manipulation.</summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Parses a string intended to represent a boolean value.
        /// </summary>
        /// <param name="value">String representing a boolean value.</param>
        /// <returns>Parsed boolean value.</returns>
        /// <remarks>
        /// This function, unlike Boolean.Parse, correctly parses a boolean value, even if the string value
        /// specified is a number (e.g., 0 or -1). Boolean.Parse expects a string to be represented as
        /// "True" or "False" (i.e., Boolean.TrueString or Boolean.FalseString respectively).
        /// </remarks>
        public static bool ParseBoolean(this string value)
        {
            if (string.IsNullOrEmpty(value))
                return false;
            
            value = value.Trim();

            if (value.Length > 0)
            {
                // Try to parse string a number
                int iresult;

                if (int.TryParse(value, out iresult))
                {
                    return (iresult != 0);
                }
                else
                {
                    // Try to parse string as a boolean
                    bool bresult;

                    if (bool.TryParse(value, out bresult))
                    {
                        return bresult;
                    }
                    else
                    {
                        char test = value.ToUpper()[0];
                        return (test == 'T' || test == 'Y' ? true : false);
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Converts this string into the specified type.
        /// </summary>
        /// <typeparam name="T"><see cref="Type"/> to convert string to.</typeparam>
        /// <param name="value">Source string to convert to type.</param>
        /// <returns><see cref="String"/> converted to specfied <see cref="Type"/>; default value of specified type T if conversion fails.</returns>
        /// <remarks>
        /// This function makes use of a <see cref="TypeConverter"/> to convert this <see cref="String"/> to the specified type T,
        /// the best way to make sure <paramref name="value"/> can be converted back to its original type is to use the same
        /// <see cref="TypeConverter"/> to convert the original object to a <see cref="String"/>; see the
        /// <see cref="Common.TypeConvertToString"/> method for an easy way to do this.
        /// </remarks>
        public static T ConvertToType<T>(this string value)
        {
            T obj = (T)value.ConvertToType(typeof(T));

            if (obj == null)
                obj = default(T);

            return obj;
        }

        /// <summary>
        /// Converts this string into the specified type.
        /// </summary>
        /// <param name="value">Source string to convert to type.</param>
        /// <param name="type"><see cref="Type"/> to convert string to.</param>
        /// <returns><see cref="String"/> converted to specfied <see cref="Type"/>; null if conversion fails.</returns>
        /// <remarks>
        /// This function makes use of a <see cref="TypeConverter"/> to convert this <see cref="String"/> to the specified <paramref name="type"/>,
        /// the best way to make sure <paramref name="value"/> can be converted back to its original type is to use the same
        /// <see cref="TypeConverter"/> to convert the original object to a <see cref="String"/>; see the
        /// <see cref="Common.TypeConvertToString"/> method for an easy way to do this.
        /// </remarks>
        public static object ConvertToType(this string value, Type type)
        {
            // If the desired return type is a string, don't do anymore work...
            if (type == typeof(string))
                return value;

            if (!string.IsNullOrEmpty(value))
            {
                if (type == typeof(bool))
                {
                    // Handle booleans as a special case to allow numeric entries as well as true/false
                    return value.ParseBoolean();
                }
                else
                {
                    if (type == typeof(IConvertible))
                    {
                        // This is faster for native types than using type converter...
                        return Convert.ChangeType(value, type);
                    }

                    // Handle objects that have type converters (e.g., Enum, Color, Point, etc.)
                    TypeConverter converter = TypeDescriptor.GetConverter(type);
                    return converter.ConvertFromString(value);
                }
            }

            return null;
        }

        /// <summary>
        /// Turns source string into an array of string segements - each with a set maximum width - for parsing or displaying.
        /// </summary>
        /// <param name="value">Input string to break up into segements.</param>
        /// <param name="segmentSize">Maximum size of returned segment.</param>
        /// <returns>Array of string segments as parsed from source string.</returns>
        /// <remarks>Returns a single element array with an empty string if source string is null or empty.</remarks>
        public static string[] GetSegments(this string value, int segmentSize)
        {
            if (string.IsNullOrEmpty(value)) return new string[] { "" };

            int totalSegments = (int)Math.Ceiling(value.Length / (double)segmentSize);
            string[] segments = new string[totalSegments];

            for (int x = 0; x < segments.Length; x++)
            {
                if (x * segmentSize + segmentSize >= value.Length)
                    segments[x] = value.Substring(x * segmentSize);
                else
                    segments[x] = value.Substring(x * segmentSize, segmentSize);
            }

            return segments;
        }

        /// <summary>
        /// Combines a dictionary of key-value pairs in to a string.
        /// </summary>
        /// <param name="pairs">Dictionary of key-value pairs.</param>
        /// <returns>A string of key-value pairs.</returns>
        public static string JoinKeyValuePairs(this Dictionary<string, string> pairs)
        {
            return pairs.JoinKeyValuePairs(';', '=');
        }

        /// <summary>
        /// Combines a dictionary of key-value pairs in to a string.
        /// </summary>
        /// <param name="pairs">Dictionary of key-value pairs.</param>
        /// <param name="parameterDelimeter">Character that delimits one key-value pair from another (eg. ';').</param>
        /// <param name="keyValueDelimeter">Character that delimits a key from its value (eg. '=').</param>
        /// <returns>A string of key-value pairs.</returns>
        public static string JoinKeyValuePairs(this Dictionary<string, string> pairs, char parameterDelimeter, char keyValueDelimeter)
        {
            StringBuilder result = new StringBuilder();
            
            foreach (string key in pairs.Keys)
            {
                result.AppendFormat("{0}{1}{2}{3}", key, keyValueDelimeter, pairs[key], parameterDelimeter);
            }

            return result.ToString();
        }

        /// <summary>
        /// Parses key value pair parameters from a string. Parameter pairs are delimited by an equals sign, and multiple
        /// pairs separated by a semi-colon.
        /// </summary>
        /// <param name="value">Key pair string to parse.</param>
        /// <returns>Dictionary of key/value pairs.</returns>
        /// <remarks>
        /// <para>
        /// Parses a string formatted like a typical connection string, e.g.:
        /// <c>IP=localhost; Port=1002; MaxEvents=50; UseTimeout=True</c>.
        /// Note that "keys" are case-insensitive.
        /// </para>
        /// <para>
        /// Values can be escaped within braces to contain nested key value pair expressions like
        /// the following: <c>normalKVP=-1; nestedKVP={p1=true; p2=0.001}</c>.
        /// Only one level of nesting is supported.
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException">value is null.</exception>
        /// <exception cref="FormatException">Only one level of tagged value expressions are allowed -or-
        /// encountered end value delimeter '}' before start value delimeter '{'.</exception>
        public static Dictionary<string, string> ParseKeyValuePairs(this string value)
        {
            return value.ParseKeyValuePairs(';', '=');
        }

        /// <summary>
        /// Parses key value pair parameters from a string. Parameter pairs are delimited by <paramref name="keyValueDelimeter"/>,
        /// and multiple pairs separated by <paramref name="parameterDelimeter"/>.
        /// </summary>
        /// <param name="value">Key pair string to parse.</param>
        /// <param name="parameterDelimeter">
        /// Character that delimits one key value pair from another (e.g., would be a ";" in a typical connection string).
        /// </param>
        /// <param name="keyValueDelimeter">
        /// Character that delimits key from value (e.g., would be an "=" in a typical connection string).
        /// </param>
        /// <returns>Dictionary of key/value pairs.</returns>
        /// <remarks>
        /// <para>
        /// Parses a key value string that contains one or many pairs. Note that "keys" are case-insensitive.
        /// </para>
        /// <para>
        /// Values can be escaped within braces to contain nested key value pair expressions like
        /// the following: <c>normalKVP=-1; nestedKVP={p1=true; p2=0.001}</c>.
        /// Only one level of nesting is supported.
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException">value is null.</exception>
        /// <exception cref="ArgumentException">All delimeters must be unique.</exception>
        /// <exception cref="FormatException">Only one level of tagged value expressions are allowed -or-
        /// encountered end value delimeter '}' before start value delimeter '{'.</exception>
        public static Dictionary<string, string> ParseKeyValuePairs(this string value, char parameterDelimeter, char keyValueDelimeter)
        {
            return value.ParseKeyValuePairs(parameterDelimeter, keyValueDelimeter, '{', '}');
        }

        /// <summary>
        /// Parses key value pair parameters from a string. Parameter pairs are delimited by <paramref name="keyValueDelimeter"/>,
        /// and multiple pairs separated by <paramref name="parameterDelimeter"/>.
        /// </summary>
        /// <param name="value">Key pair string to parse.</param>
        /// <param name="parameterDelimeter">Character that delimits one key value pair from another (e.g., would be a ";" in a typical connection
        /// string).</param>
        /// <param name="keyValueDelimeter">Character that delimits key from value (e.g., would be an "=" in a typical connection string).</param>
        /// <param name="startValueDelimeter">Optional character that marks the start of a value such that value could contain other
        /// <paramref name="parameterDelimeter"/> or <paramref name="keyValueDelimeter"/> characters (e.g., "{").</param>
        /// <param name="endValueDelimeter">Optional character that marks the end of a value such that value could contain other
        /// <paramref name="parameterDelimeter"/> or <paramref name="keyValueDelimeter"/> characters (e.g., "}").</param>
        /// <returns>Dictionary of key/value pairs.</returns>
        /// <remarks>
        /// <para>
        /// Parses a key value string that contains one or many pairs. Note that "keys" are case-insensitive.
        /// </para>
        /// <para>
        /// If value includes <paramref name="startValueDelimeter"/>, it must include <paramref name="endValueDelimeter"/>
        /// otherwise results will be unexpected. Only one level of start/end value delimeter nesting is supported.
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException">value is null.</exception>
        /// <exception cref="ArgumentException">All delimeters must be unique.</exception>
        /// <exception cref="FormatException">Only one level of tagged value expressions are allowed -or-
        /// encountered <paramref name="endValueDelimeter"/> before <paramref name="startValueDelimeter"/>.</exception>
        public static Dictionary<string, string> ParseKeyValuePairs(this string value, char parameterDelimeter, char keyValueDelimeter, char startValueDelimeter, char endValueDelimeter)
        {
            if (value == null)
                throw new ArgumentNullException("value");

            if (parameterDelimeter == keyValueDelimeter || 
                parameterDelimeter == startValueDelimeter || 
                parameterDelimeter == endValueDelimeter ||
                keyValueDelimeter == startValueDelimeter || 
                keyValueDelimeter == endValueDelimeter ||
                startValueDelimeter == endValueDelimeter)
                    throw new ArgumentException("All delimeters must be unique.");

            Dictionary<string, string> keyValuePairs = new Dictionary<string, string>(StringComparer.CurrentCultureIgnoreCase);
            string[] elements;
            string escapedParameterDelimeter = parameterDelimeter.RegexEncode();
            string escapedKeyValueDelimeter = keyValueDelimeter.RegexEncode();
            StringBuilder escapedValue = new StringBuilder();
            bool valueEscaped = false;
            char character;

            // Escape any parameter or key value delimeters within tagged value sequences
            //      For example, the following string:
            //          "normalKVP=-1; nestedKVP={p1=true; p2=false}")
            //      would be encoded as:
            //          "normalKVP=-1; nestedKVP=p1\\u003dtrue\\u003b p2\\u003dfalse")
            for (int x = 0; x < value.Length; x++)
            {
                character = value[x];

                if (character == startValueDelimeter)
                {
                    if (!valueEscaped)
                    {
                        valueEscaped = true;
                        continue;   // Don't add tag start delimeter to final value
                    }
                    else
                    {
                        throw new FormatException("Only one level of tagged value expressions are allowed.");
                    }
                }

                if (character == endValueDelimeter)
                {
                    if (valueEscaped)
                    {
                        valueEscaped = false;
                        continue;   // Don't add tag stop delimeter to final value
                    }
                    else
                    {
                        throw new FormatException(string.Format("Encountered end value delimeter \'{0}\' before start value delimeter \'{1}\'.", endValueDelimeter, startValueDelimeter));
                    }
                }

                if (valueEscaped)
                {
                    // Escape any parameter or key value delimeters
                    if (character == parameterDelimeter)
                        escapedValue.Append(escapedParameterDelimeter);
                    else if (character == keyValueDelimeter)
                        escapedValue.Append(escapedKeyValueDelimeter);
                    else
                        escapedValue.Append(character);
                }
                else
                {
                    escapedValue.Append(character);
                }
            }

            // Parse out key/value pairs
            foreach (string parameter in escapedValue.ToString().Split(parameterDelimeter))
            {
                // Parse out parameter's key/value elements
                elements = parameter.Split(keyValueDelimeter);
                if (elements.Length == 2)
                {
                    // Add key/value pair, unescaping value expression as needed
                    keyValuePairs.Add(
                        elements[0].ToString().Trim(),
                        elements[1].ToString().Trim().
                            Replace(escapedParameterDelimeter, parameterDelimeter.ToString()).
                            Replace(escapedKeyValueDelimeter, keyValueDelimeter.ToString()));
                }
            }

            return keyValuePairs;
        }

        /// <summary>
        /// Ensures parameter is not an empty or null string. Returns a single space if test value is empty.
        /// </summary>
        /// <param name="testValue">Value to test for null or empty.</param>
        /// <returns>A non-empty string.</returns>
        public static string NotEmpty(this string testValue)
        {
            return testValue.NotEmpty(" ");
        }

        /// <summary>
        /// Ensures parameter is not an empty or null string.
        /// </summary>
        /// <param name="testValue">Value to test for null or empty.</param>
        /// <param name="nonEmptyReturnValue">Value to return if <paramref name="testValue">testValue</paramref> is null or empty.</param>
        /// <returns>A non-empty string.</returns>
        public static string NotEmpty(this string testValue, string nonEmptyReturnValue)
        {
            if (string.IsNullOrEmpty(nonEmptyReturnValue))
                throw new ArgumentException("nonEmptyReturnValue cannot be null or empty");

            if (string.IsNullOrEmpty(testValue))
                return nonEmptyReturnValue;
            else
                return testValue;
        }

        /// <summary>
        /// Replaces all characters passing delegate test with specified replacement character.
        /// </summary>
        /// <param name="value">Input string.</param>
        /// <param name="replacementCharacter">Character used to replace characters passing delegate test.</param>
        /// <param name="characterTestFunction">Delegate used to determine whether or not character should be replaced.</param>
        /// <returns>Returns <paramref name="value" /> with all characters passing delegate test replaced.</returns>
        /// <remarks>Allows you to specify a replacement character (e.g., you may want to use a non-breaking space: Convert.ToChar(160)).</remarks>
        public static string ReplaceCharacters(this string value, char replacementCharacter, Func<char, bool> characterTestFunction)
		{
			if (string.IsNullOrEmpty(value)) return "";
			
			StringBuilder result = new StringBuilder();
			char character;
			
			for (int x = 0; x < value.Length; x++)
			{
				character = value[x];
				
				if (characterTestFunction(character))
					result.Append(replacementCharacter);
				else
					result.Append(character);
			}
			
			return result.ToString();
		}

        /// <summary>
        /// Removes all characters passing delegate test from a string.
        /// </summary>
        /// <param name="value">Input string.</param>
        /// <param name="characterTestFunction">Delegate used to determine whether or not character should be removed.</param>
        /// <returns>Returns <paramref name="value" /> with all characters passing delegate test removed.</returns>
        public static string RemoveCharacters(this string value, Func<char, bool> characterTestFunction)
		{
			if (string.IsNullOrEmpty(value)) return "";
			
			StringBuilder result = new StringBuilder();
			char character;
			
			for (int x = 0; x < value.Length; x++)
			{
				character = value[x];
				
				if (!characterTestFunction(character))
					result.Append(character);
			}
			
			return result.ToString();
		}

        /// <summary>
        /// Removes all white space (as defined by IsWhiteSpace) from a string.
        /// </summary>
        /// <param name="value">Input string.</param>
        /// <returns>Returns <paramref name="value" /> with all white space removed.</returns>
        public static string RemoveWhiteSpace(this string value)
        {
            return value.RemoveCharacters(char.IsWhiteSpace);
        }

        /// <summary>
        /// Replaces all white space characters (as defined by IsWhiteSpace) with specified replacement character.
        /// </summary>
        /// <param name="value">Input string.</param>
        /// <param name="replacementCharacter">Character used to "replace" white space characters.</param>
        /// <returns>Returns <paramref name="value" /> with all white space characters replaced.</returns>
        /// <remarks>Allows you to specify a replacement character (e.g., you may want to use a non-breaking space: Convert.ToChar(160)).</remarks>
        public static string ReplaceWhiteSpace(this string value, char replacementCharacter)
        {
            return value.ReplaceCharacters(replacementCharacter, char.IsWhiteSpace);
        }

        /// <summary>
        /// Removes all control characters from a string.
        /// </summary>
        /// <param name="value">Input string.</param>
        /// <returns>Returns <paramref name="value" /> with all control characters removed.</returns>
        public static string RemoveControlCharacters(this string value)
        {
            return value.RemoveCharacters(char.IsControl);
        }

        /// <summary>
        /// Replaces all control characters in a string with a single space.
        /// </summary>
        /// <param name="value">Input string.</param>
        /// <returns>Returns <paramref name="value" /> with all control characters replaced as a single space.</returns>
        public static string ReplaceControlCharacters(this string value)
        {
            return value.ReplaceControlCharacters(' ');
        }

        /// <summary>
        /// Replaces all control characters in a string with specified replacement character.
        /// </summary>
        /// <param name="value">Input string.</param>
        /// <param name="replacementCharacter">Character used to "replace" control characters.</param>
        /// <returns>Returns <paramref name="value" /> with all control characters replaced.</returns>
        /// <remarks>Allows you to specify a replacement character (e.g., you may want to use a non-breaking space: Convert.ToChar(160)).</remarks>
        public static string ReplaceControlCharacters(this string value, char replacementCharacter)
        {
            return value.ReplaceCharacters(replacementCharacter, char.IsControl);
        }

        /// <summary>
        /// Removes all carriage returns and line feeds from a string.
        /// </summary>
        /// <param name="value">Input string.</param>
        /// <returns>Returns <paramref name="value" /> with all CR and LF characters removed.</returns>
        public static string RemoveCrLfs(this string value)
        {
            return value.RemoveCharacters(c => c == '\r' || c == '\n');
        }

        /// <summary>
        /// Replaces all carriage return and line feed characters (as well as CR/LF sequences) in a string with specified replacement character.
        /// </summary>
        /// <param name="value">Input string.</param>
        /// <param name="replacementCharacter">Character used to "replace" CR and LF characters.</param>
        /// <returns>Returns <paramref name="value" /> with all CR and LF characters replaced.</returns>
        /// <remarks>Allows you to specify a replacement character (e.g., you may want to use a non-breaking space: Convert.ToChar(160)).</remarks>
        public static string ReplaceCrLfs(this string value, char replacementCharacter)
        {
            return value.Replace(Environment.NewLine, replacementCharacter.ToString()).ReplaceCharacters(replacementCharacter, c => c == '\r' || c == '\n');
        }

        /// <summary>
        /// Removes duplicate character strings (adjoining replication) in a string.
        /// </summary>
        /// <param name="value">Input string.</param>
        /// <param name="duplicatedValue">String whose duplicates are to be removed.</param>
        /// <returns>Returns <paramref name="value" /> with all duplicated <paramref name="duplicatedValue" /> removed.</returns>
        public static string RemoveDuplicates(this string value, string duplicatedValue)
        {
            if (string.IsNullOrEmpty(value)) return "";
            if (string.IsNullOrEmpty(duplicatedValue)) return value;

            string duplicate = duplicatedValue + duplicatedValue;

            while (value.IndexOf(duplicate) > -1)
            {
                value = value.Replace(duplicate, duplicatedValue);
            }

            return value;
        }

        /// <summary>
        /// Removes the terminator ('\0') from a null terminated string.
        /// </summary>
        /// <param name="value">Input string.</param>
        /// <returns>Returns <paramref name="value" /> with all characters to the left of the terminator.</returns>
        public static string RemoveNull(this string value)
        {
            if (string.IsNullOrEmpty(value)) return "";

            int nullPos = value.IndexOf('\0');

            if (nullPos > -1)
                return value.Substring(0, nullPos);
            else
                return value;
        }

        /// <summary>
        /// Replaces all repeating white space with a single space.
        /// </summary>
        /// <param name="value">Input string.</param>
        /// <returns>Returns <paramref name="value" /> with all duplicate white space removed.</returns>
        public static string RemoveDuplicateWhiteSpace(this string value)
        {
            return value.RemoveDuplicateWhiteSpace(' ');
        }

        /// <summary>
        /// Replaces all repeating white space with specified spacing character.
        /// </summary>
        /// <param name="value">Input string.</param>
        /// <param name="spacingCharacter">Character value to use to insert as single white space value.</param>
        /// <returns>Returns <paramref name="value" /> with all duplicate white space removed.</returns>
        /// <remarks>This function allows you to specify spacing character (e.g., you may want to use a non-breaking space: <c>Convert.ToChar(160)</c>).</remarks>
        public static string RemoveDuplicateWhiteSpace(this string value, char spacingCharacter)
		{
			if (string.IsNullOrEmpty(value)) return "";
			
			StringBuilder result = new StringBuilder();
			bool lastCharWasSpace = false;
            char character;
			
			for (int x = 0; x < value.Length; x++)
			{
				character = value[x];
				
				if (char.IsWhiteSpace(character))
				{
					lastCharWasSpace = true;
				}
				else
				{
					if (lastCharWasSpace)
						result.Append(spacingCharacter);

                    result.Append(character);
					lastCharWasSpace = false;
				}
			}
			
			return result.ToString();
		}

        /// <summary>
        /// Counts the total number of the occurances of a character in the given string.
        /// </summary>
        /// <param name="value">Input string.</param>
        /// <param name="characterToCount">Character to be counted.</param>
        /// <returns>Total number of the occurances of <paramref name="characterToCount" /> in the given string.</returns>
        public static int CharCount(this string value, char characterToCount)
        {
            if (string.IsNullOrEmpty(value)) return 0;

            int total = 0;

            for (int x = 0; x < value.Length; x++)
            {
                if (value[x] == characterToCount)
                    total++;
            }

            return total;
        }

        /// <summary>
        /// Tests to see if a string is contains only digits based on Char.IsDigit function.
        /// </summary>
        /// <param name="value">Input string.</param>
        /// <returns>True, if all string's characters are digits; otherwise, false.</returns>
        /// <seealso cref="char.IsDigit(char)"/>
        public static bool IsAllDigits(this string value)
        {
            if (string.IsNullOrEmpty(value)) return false;

            value = value.Trim();
            if (value.Length == 0) return false;

            for (int x = 0; x < value.Length; x++)
            {
                if (!char.IsDigit(value[x]))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Tests to see if a string contains only numbers based on Char.IsNumber function.
        /// </summary>
        /// <param name="value">Input string.</param>
        /// <returns>True, if all string's characters are numbers; otherwise, false.</returns>
        /// <seealso cref="char.IsNumber(char)"/>
        public static bool IsAllNumbers(this string value)
        {
            if (string.IsNullOrEmpty(value)) return false;

            value = value.Trim();
            if (value.Length == 0) return false;

            for (int x = 0; x < value.Length; x++)
            {
                if (!char.IsNumber(value[x]))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Tests to see if a string's letters are all upper case.
        /// </summary>
        /// <param name="value">Input string.</param>
        /// <returns>True, if all string's letter characters are upper case; otherwise, false.</returns>
        public static bool IsAllUpper(this string value)
        {
            if (string.IsNullOrEmpty(value)) return false;

            value = value.Trim();
            if (value.Length == 0) return false;

            for (int x = 0; x < value.Length; x++)
            {
                if (char.IsLetter(value[x]) && !char.IsUpper(value[x]))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Tests to see if a string's letters are all lower case.
        /// </summary>
        /// <param name="value">Input string.</param>
        /// <returns>True, if all string's letter characters are lower case; otherwise, false.</returns>
        public static bool IsAllLower(this string value)
        {
            if (string.IsNullOrEmpty(value)) return false;

            value = value.Trim();
            if (value.Length == 0) return false;

            for (int x = 0; x < value.Length; x++)
            {
                if (char.IsLetter(value[x]) && !char.IsLower(value[x]))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Tests to see if a string contains only letters.
        /// </summary>
        /// <param name="value">Input string.</param>
        /// <returns>True, if all string's characters are letters; otherwise, false.</returns>
        /// <remarks>Any non-letter character (e.g., punctuation marks) causes this function to return false (See overload to ignore punctuation
        /// marks.).</remarks>
        public static bool IsAllLetters(this string value)
        {
            return value.IsAllLetters(false);
        }

        /// <summary>
        /// Tests to see if a string contains only letters.
        /// </summary>
        /// <param name="value">Input string.</param>
        /// <param name="ignorePunctuation">Set to True to ignore punctuation.</param>
        /// <returns>True, if all string's characters are letters; otherwise, false.</returns>
        public static bool IsAllLetters(this string value, bool ignorePunctuation)
        {
            if (string.IsNullOrEmpty(value)) return false;

            value = value.Trim();
            if (value.Length == 0) return false;

            for (int x = 0; x < value.Length; x++)
            {
                if (ignorePunctuation)
                {
                    if (!(char.IsLetter(value[x]) || char.IsPunctuation(value[x])))
                        return false;
                }
                else
                {
                    if (!char.IsLetter(value[x]))
                        return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Tests to see if a string contains only letters or digits.
        /// </summary>
        /// <param name="value">Input string.</param>
        /// <returns>True, if all string's characters are either letters or digits; otherwise, false.</returns>
        /// <remarks>Any non-letter, non-digit character (e.g., punctuation marks) causes this function to return false (See overload to ignore
        /// punctuation marks.).</remarks>
        public static bool IsAllLettersOrDigits(this string value)
        {
            return value.IsAllLettersOrDigits(false);
        }

        /// <summary>
        /// Tests to see if a string contains only letters or digits.
        /// </summary>
        /// <param name="value">Input string.</param>
        /// <param name="ignorePunctuation">Set to True to ignore punctuation.</param>
        /// <returns>True, if all string's characters are letters or digits; otherwise, false.</returns>
        public static bool IsAllLettersOrDigits(this string value, bool ignorePunctuation)
        {
            if (string.IsNullOrEmpty(value)) return false;

            value = value.Trim();
            if (value.Length == 0) return false;

            for (int x = 0; x < value.Length; x++)
            {
                if (ignorePunctuation)
                {
                    if (!(char.IsLetterOrDigit(value[x]) || char.IsPunctuation(value[x])))
                        return false;
                }
                else
                {
                    if (!char.IsLetterOrDigit(value[x]))
                        return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Decodes the specified Regular Expression character back into a standard Unicode character.
        /// </summary>
        /// <param name="value">Regular Expression character to decode back into a Unicode character.</param>
        /// <returns>Standard Unicode character representation of specified Regular Expression character.</returns>
        public static char RegexDecode(this string value)
        {
            return Convert.ToChar(Convert.ToUInt16(value.Replace("\\u", "0x"), 16));
        }

        /// <summary>
        /// Encodes a string into a base-64 string.
        /// </summary>
        /// <param name="value">Input string.</param>
        /// <remarks>
        /// <para>Performs a base-64 style of string encoding useful for data obfuscation or safe XML data string transmission.</para>
        /// <para>Note: This function encodes a "String". Use the Convert.ToBase64String function to encode a binary data buffer.</para>
        /// </remarks>
        /// <returns>A <see cref="String"></see> value representing the encoded input of <paramref name="value"/>.</returns>
        public static string Base64Encode(this string value)
        {
            return Convert.ToBase64String(Encoding.Unicode.GetBytes(value));
        }

        /// <summary>
        /// Decodes a given base-64 encoded string encoded with <see cref="Base64Encode" />.
        /// </summary>
        /// <param name="value">Input string.</param>
        /// <remarks>Note: This function decodes value back into a "String". Use the Convert.FromBase64String function to decode a base-64 encoded
        /// string back into a binary data buffer.</remarks>
        /// <returns>A <see cref="String"></see> value representing the decoded input of <paramref name="value"/>.</returns>
        public static string Base64Decode(this string value)
        {
            return Encoding.Unicode.GetString(Convert.FromBase64String(value));
        }

        /// <summary>
        /// Converts the provided string into title case (upper case first letter of each word).
        /// </summary>
        /// <param name="value">Input string.</param>
        /// <remarks>Note: This function performs "ToLower" in input string then applies TextInfo.ToTitleCase for CurrentCulture. This way, even
        /// strings formatted in all-caps will still be properly formatted.</remarks>
        /// <returns>A <see cref="String"/> that has the first letter of each word capitalized.</returns>
        public static string ToTitleCase(this string value)
        {
            if (string.IsNullOrEmpty(value))
                return "";

            return System.Globalization.CultureInfo.CurrentUICulture.TextInfo.ToTitleCase(value.ToLower());
        }

        /// <summary>
        /// Truncates the provided string from the left if it is longer that specified length.
        /// </summary>
        /// <param name="value">A <see cref="String"/> value that is to be truncated.</param>
        /// <param name="maxLength">The maximum number of characters that <paramref name="value"/> can be.</param>
        /// <returns>A <see cref="String"/> that is the truncated version of the <paramref name="value"/> string.</returns>
        public static string TruncateLeft(this string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value))
                return "";

            if (value.Length > maxLength)
                return value.Substring(value.Length - maxLength);

            return value;
        }

        /// <summary>
        /// Truncates the provided string from the right if it is longer that specified length.
        /// </summary>
        /// <param name="value">A <see cref="String"/> value that is to be truncated.</param>
        /// <param name="maxLength">The maximum number of characters that <paramref name="value"/> can be.</param>
        /// <returns>A <see cref="String"/> that is the truncated version of the <paramref name="value"/> string.</returns>
        public static string TruncateRight(this string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value))
                return "";

            if (value.Length > maxLength)
                return value.Substring(0, maxLength);

            return value;
        }

        /// <summary>
        /// Centers text within the specified maximum length, biased to the left.
        /// Text will be padded to the left and right with spaces.
        /// If value is greater than specified maximum length, value returned will be truncated from the right.
        /// </summary>
        /// <remarks>
        /// Handles multiple lines of text separated by Environment.NewLine.
        /// </remarks>
        /// <param name="value">A <see cref="String"/> to be centered.</param>
        /// <param name="maxLength">An <see cref="Int32"/> that is the maximum length of padding.</param>
        /// <returns>The centered string value.</returns>
        public static string CenterText(this string value, int maxLength)
        {
            return value.CenterText(maxLength, ' ');
        }

        /// <summary>
        /// Centers text within the specified maximum length, biased to the left.
        /// Text will be padded to the left and right with specified padding character.
        /// If value is greater than specified maximum length, value returned will be truncated from the right.
        /// </summary>
        /// <remarks>
        /// Handles multiple lines of text separated by <c>Environment.NewLine</c>.
        /// </remarks>
        /// <param name="value">A <see cref="String"/> to be centered.</param>
        /// <param name="maxLength">An <see cref="Int32"/> that is the maximum length of padding.</param>
        /// <param name="paddingCharacter">The <see cref="char"/> value to pad with.</param>
        /// <returns>The centered string value.</returns>
        public static string CenterText(this string value, int maxLength, char paddingCharacter)
        {
            if (value == null) value = "";

            // If the text to be centered contains multiple lines, centers all the lines individually.
            StringBuilder result = new StringBuilder();
            string[] lines = value.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
            string line;
            int lastLineIndex = lines.Length - 1; //(lines.Length != 0 && lines[lines.Length - 1].Trim() == string.Empty ? lines.Length - 2 : lines.Length - 1);

            for (int i = 0; i <= lastLineIndex; i++)
            {                
                // Gets current line.
                line = lines[i];
                
                // Skips the last empty line as a result of split if original text had multiple lines.
                if (i == lastLineIndex && line.Trim() == string.Empty)
                    continue;

                if (line.Length >= maxLength)
                {
                    // Truncates excess characters on the right.
                    result.Append(line.Substring(0, maxLength));
                }
                else
                {
                    int remainingSpace = maxLength - line.Length;
                    int leftSpaces;
                    int rightSpaces;

                    // Splits remaining space between the left and the right.
                    leftSpaces = remainingSpace / 2;
                    rightSpaces = leftSpaces;

                    // Adds any remaining odd space to the right (bias text to the left).
                    if (remainingSpace % 2 > 0) rightSpaces++;

                    result.Append(new string(paddingCharacter, leftSpaces));
                    result.Append(line);
                    result.Append(new string(paddingCharacter, rightSpaces));
                }

                // Creates a new line only if the original text contains multiple lines.
                if (i < lastLineIndex)
                    result.AppendLine();
            }

            return result.ToString();
        }
        /// <summary>
        /// Performs a case insensitive string replacement.
        /// </summary>
        /// <param name="inString">The string to examine.</param>
        /// <param name="fromText">The value to replace.</param>
        /// <param name="toText">The new value to be inserted</param>
        /// <returns>A string with replacements.</returns>
        public static string ReplaceCaseInsensitive(this string inString, string fromText, string toText)
        {
            return (new Regex(fromText, RegexOptions.IgnoreCase | RegexOptions.Multiline)).Replace(inString, toText);
        }

        /// <summary>
        /// Ensures a string starts with a specific character.
        /// </summary>
        /// <param name="value">Input string to process.</param>
        /// <param name="startChar">The character desired at string start.</param>
        /// <returns>The sent string with character at the start.</returns>
        public static string EnsureStart(this string value, char startChar)
        {
            return EnsureStart(value, startChar, false);
        }

        /// <summary>
        /// Ensures a string starts with a specific character.
        /// </summary>
        /// <param name="value">Input string to process.</param>
        /// <param name="startChar">The character desired at string start.</param>
        /// <param name="removeRepeatingChar">Set to <c>true</c> to ensure one and only one instance of <paramref name="startChar"/>.</param>
        /// <returns>The sent string with character at the start.</returns>
        public static string EnsureStart(this string value, char startChar, bool removeRepeatingChar)
        {
            if (string.IsNullOrEmpty(value))
                return "";

            if (startChar == 0)return value;

            if (value[0] == startChar)
            {
                if (removeRepeatingChar) return value.Substring(LastIndexOfRepeatedChar(value, startChar, 0));
                return value;
            }
            else
                return string.Concat(startChar, value);
        }

        /// <summary>
        /// Ensures a string starts with a specific string.
        /// </summary>
        /// <param name="value">Input string to process.</param>
        /// <param name="startString">The string desired at string start.</param>
        /// <returns>The sent string with string at the start.</returns>
        public static string EnsureStart(this string value, string startString)
        {
            if (string.IsNullOrEmpty(value))
                return "";

            if (string.IsNullOrEmpty(startString))
                return value;

            if (value.IndexOf(startString) == 0)
                return value;
            else
                return string.Concat(startString, value);
        }

        /// <summary>
        /// Ensures a string ends with a specific character.
        /// </summary>
        /// <param name="value">Input string to process.</param>
        /// <param name="endChar">The character desired at string's end.</param>
        /// <returns>The sent string with character at the end.</returns>
        public static string EnsureEnd(this string value, char endChar)
        {
            return EnsureEnd(value, endChar, false);
        }

        /// <summary>
        /// Ensures a string ends with a specific character.
        /// </summary>
        /// <param name="value">Input string to process.</param>
        /// <param name="endChar">The character desired at string's end.</param>
        /// <param name="removeRepeatingChar">Set to <c>true</c> to ensure one and only one instance of <paramref name="endChar"/>.</param>
        /// <returns>The sent string with character at the end.</returns>
        public static string EnsureEnd(this string value, char endChar, bool removeRepeatingChar)
        {
            if (string.IsNullOrEmpty(value))
                return "";

            if (endChar == 0)
                return value;

            if (value[value.Length - 1] == endChar)
            {
                if (removeRepeatingChar)
                {
                    int i = LastIndexOfRepeatedChar(value.Reverse(), endChar, 0);
                    return value.Substring(0, value.Length - i);
                }
                return value;
            }
            else
                return string.Concat(value, endChar);
        }

        /// <summary>
        /// Ensures a string ends with a specific string.
        /// </summary>
        /// <param name="value">Input string to process.</param>
        /// <param name="endString">The string desired at string's end.</param>
        /// <returns>The sent string with string at the end.</returns>
        public static string EnsureEnd(this string value, string endString)
        {
            if (string.IsNullOrEmpty(value))
                return "";

            if (string.IsNullOrEmpty(endString))
                return value;

            if (value.EndsWith(endString))
               return value;
            else
                return string.Concat(value, endString);
        }

        /// <summary>
        /// Reverses the order of the characters in a string.
        /// </summary>
        /// <param name="value">Input string to process.</param>
        /// <returns>The reversed string.</returns>
        public static string Reverse(this string value)
        {
            // Experimented with several approaches.  This is the fastest.
            // Replaced Common.DoReverse with explicid code. yielded 1.5% performance increase.
            // DoReverse is faster than Array.Reverse.

            if (string.IsNullOrEmpty(value))
                return "";

            char[] arrChar = value.ToCharArray();
            char temp;
            int arrLength =  arrChar.Length;
            int j;

            // works for odd and even length strings, since middle char is not swapped for an odd length string.
            for (int i = 0; i < arrLength / 2; i++)
            {
                j = arrLength - i - 1;
                temp = arrChar[i];
                arrChar[i] = arrChar[j];
                arrChar[j] = temp;
            }

            return new string(arrChar);
        }

        /// <summary>
        /// Searches a string for a repeated instance of the specified <paramref name="characterToFind"/> from specified <paramref name="startIndex"/>.
        /// </summary>
        /// <param name="value">The string to process.</param>
        /// <param name="characterToFind">The character of interest.</param>
        /// <param name="startIndex">The index from which to begin the search.</param>
        /// <returns>The index of the first instance of the character that is repeated or (-1) if no repeated chars found.</returns>
        public static int IndexOfRepeatedChar(this string value, char characterToFind, int startIndex)
        {
            if (string.IsNullOrEmpty(value))
                return -1;

            if (startIndex < 0)
                return -1;

            if (characterToFind == 0)
                return -1;

            char c = (char)0;

            for (int i = startIndex; i < value.Length; i++)
            {
                if (value[i] == characterToFind)
                {
                    if (value[i] != c)
                        c = value[i];
                    else
                    {
                        //at least one repeating character
                        return i - 1;
                    }
                }
                else
                    c = (char)0;
            }

            return -1;
        }

        /// <summary>
        /// Searches a string for a repeated instance of the specified <paramref name="characterToFind"/>.
        /// </summary>
        /// <param name="value">The string to process.</param>
        /// <param name="characterToFind">The character of interest.</param>
        /// <returns>The index of the first instance of the character that is repeated or (-1) if no repeated chars found.</returns>
        public static int IndexOfRepeatedChar(this string value, char characterToFind)
        {
            return IndexOfRepeatedChar(value, characterToFind, 0);
        }

        /// <summary>
        /// Searches a string for an instance of a repeated character.
        /// </summary>
        /// <param name="value">The string to process.</param>
        /// <returns>The index of the first instance of any character that is repeated or (-1) if no repeated chars found.</returns>
        public static int IndexOfRepeatedChar(this string value)
        {
            return IndexOfRepeatedChar(value, 0);
        }

        /// <summary>
        /// Searches a string for an instance of a repeated character from specified <paramref name="startIndex"/>.
        /// </summary>
        /// <param name="value">The string to process.</param>
        /// <param name="startIndex">The index from which to begin the search.</param>
        /// <returns>The index of the first instance of any character that is repeated or (-1) if no repeated chars found.</returns>
        public static int IndexOfRepeatedChar(this string value, int startIndex)
        {
            if (string.IsNullOrEmpty(value))
                return -1;

            if (startIndex < 0)
                return -1;

            char c = (char)0;
           
            for (int i = startIndex; i < value.Length; i++)
            {
                if (value[i] != c)
                    c = value[i];
                else
                {
                    //at least one repeating character
                    return i - 1;
                }
            }
            return -1;
        }

        /// <summary>
        /// Returns the index of the last repeated index of the first group of repeated characters that begin with 'value'
        /// </summary>
        /// <param name="inString">String to process</param>
        /// <param name="value">character to search for</param>
        /// <param name="startIndex">the begin search point</param>
        /// <returns>The index of the last instance of the character that is repeated or (-1) if no repeated chars found.</returns>
        private static int LastIndexOfRepeatedChar(string inString, char value, int startIndex)
        {
            if (startIndex > inString.Length - 1)
                return -1;
            
            int v = inString.IndexOf(value, startIndex);

            if (v == -1)
                return -1;

            for (int j = v + 1; j < inString.Length; j++)
            {
                if (inString[j] != value)
                    return j - 1;
            }

            return inString.Length - 1;
        }

        /// <summary>
        /// Places an ellipsis in the middle of a string as it is trimmed to length specified.
        /// </summary>
        /// <param name="value">The string to process.</param>
        /// <param name="length">The maximum returned string length; mimimum value is 5.</param>
        /// <returns>
        /// A trimmed string of the specified <paramref name="length"/> or empty string if <paramref name="value"/> is null or empty.
        /// </returns>
        /// <remarks>
        /// Returned string is not padded to fill field length if <paramref name="value"/> is shorter than length.
        /// </remarks>
        public static string TrimWithEllipsisMiddle(this string value, int length)
        {
            if (string.IsNullOrEmpty(value))
                return "";

            if (length < 5)
                length = 5;

            value = value.Trim();

            if (value.Length <= length)
                return value;

            int s1_Len = (int)(length / 2) - 1;

            return string.Concat(value.Substring(0, s1_Len), "...", value.Substring(value.Length - s1_Len + 1 -  length % 2));
        }

        /// <summary>
        /// Places an ellipsis at the end of a string as it is trimmed to length specified.
        /// </summary>
        /// <param name="value">The string to process.</param>
        /// <param name="length">The maximum returned string length; mimimum value is 5.</param>
        /// <returns>
        /// A trimmed string of the specified <paramref name="length"/> or empty string if <paramref name="value"/> is null or empty.
        /// </returns>
        /// <remarks>
        /// Returned string is not padded to fill field length if <paramref name="value"/> is shorter than length.
        /// </remarks>
        public static string TrimWithEllipsisEnd(this string value, int length)
        {
            if (string.IsNullOrEmpty(value))
                return "";
            
            if (length < 5)
                length = 5;

            value = value.Trim();

            if (value.Length <= length)
                return value;

            return string.Concat(value.Substring(0, length - 3), "...");
        }
    }
}