//*******************************************************************************************************
//  Common.cs
//  Copyright © 2008 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: James R Carroll
//      Office: PSO TRAN & REL, CHATTANOOGA - MR 2W-C
//       Phone: 423/751-2827
//       Email: jrcarrol@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  02/23/2003 - J. Ritchie Carroll
//       Generated original version of source code.
//  01/24/2006 - J. Ritchie Carroll
//       Migrated 2.0 version of source code from 1.1 source (TVA.Shared.String).
//  06/01/2006 - J. Ritchie Carroll
//       Added ParseBoolean function to parse strings representing booleans that may be numeric.
//  07/07/2006 - J. Ritchie Carroll
//       Added GetStringSegments function to break a string up into smaller chunks for parsing
//       and/or displaying.
//  08/02/2007 - J. Ritchie Carroll
//       Added a CenterText method for centering strings in console applications or fixed width fonts.
//  08/03/2007 - Pinal C. Patel
//       Modified the CenterText method to handle multiple lines.
//  08/21/2007 - Darrell Zuercher
//       Edited code comments.
//  09/25/2007 - J. Ritchie Carroll
//       Added TitleCase function to format a string with the first letter of each word capitalized
//  04/16/2008 - Pinal C. Patel
//       Made the keys of the string dictionary returned by ParseKeyValuePairs function case-insensitive.
//       Added JoinKeyValuePairs overloads that does the exact opposite of ParseKeyValuePairs.
//  09/19/2008 - James R Carroll
//       Converted to C#.
//
//*******************************************************************************************************

using System;
using System.Text;
using System.Collections.Generic;

namespace TVA
{
    /// <summary>Defines common global functions related to string manipulation.</summary>
    public sealed class Common
    {


        /// <summary>Function signature for testing a character against certain criteria.</summary>
        /// <param name="c">Character to test.</param>
        /// <returns>True, if specified character passed test; otherwise, false.</returns>
        public delegate bool CharacterTestFunctionSignature(char c);

        private Common()
        {

            // This class contains only global functions and is not meant to be instantiated.

        }

        /// <summary>Performs a fast concatenation of a given string array.</summary>
        /// <param name="values">String array to concatenate.</param>
        /// <returns>The concatenated string representation of the values of the elements in <paramref name="values" /> string array.</returns>
        /// <remarks>
        /// <para>This is a replacement for the String.Concat function. Tests show that the system implemenation of this function is slow:
        /// http://www.developer.com/net/cplus/article.php/3304901
        /// </para>
        /// </remarks>
        [Obsolete("Latest .NET versions of the String.Concat function have been optimized - this function will be removed from future builds of the code library.", false)]
        public static string Concat(params string[] values)
        {

            if (values == null)
            {
                return "";
            }
            else
            {
                if (values.Length == 2)
                {
                    return string.Concat(values[0], values[1]);
                }
                else if (values.Length == 3)
                {
                    return string.Concat(values[0], values[1], values[3]);
                }
                else
                {
                    int x;
                    int size;

                    // Precalculates needed size of string buffer.
                    for (x = 0; x <= values.Length - 1; x++)
                    {
                        if (!string.IsNullOrEmpty(values[x]))
                        {
                            size += values[x].Length;
                        }
                    }

                    System.Char with_1 = new StringBuilder(size);
                    for (x = 0; x <= values.Length - 1; x++)
                    {
                        if (!string.IsNullOrEmpty(values[x]))
                        {
                            with_1.Append(values[x]);
                        }
                    }

                    return with_1.ToString();
                }
            }

        }

        /// <summary>Parses a string intended to represent a boolean value.</summary>
        /// <param name="value">String representing a boolean value.</param>
        /// <returns>Parsed boolean value.</returns>
        /// <remarks>
        /// This function, unlike Boolean.Parse, correctly parses a boolean value, even if the string value
        /// specified is a number (e.g., 0 or -1). Boolean.Parse expects a string to be represented as
        /// "True" or "False" (i.e., Boolean.TrueString or Boolean.FalseString respectively).
        /// </remarks>
        public static bool ParseBoolean(string value)
        {

            if (string.IsNullOrEmpty(value))
            {
                return false;
            }
            value = value.Trim();

            if (value.Length > 0)
            {
                if (Information.IsNumeric(value))
                {
                    // String contains a number.
                    int result;

                    if (int.TryParse(value, ref result))
                    {
                        return (result != 0);
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    // String contains text.
                    bool result;

                    if (bool.TryParse(value, ref result))
                    {
                        return result;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            else
            {
                return false;
            }

        }

        /// <summary>Turns source string into an array of string segements - each with a set maximum width - for parsing or displaying.</summary>
        /// <param name="value">Input string to break up into segements.</param>
        /// <param name="segmentSize">Maximum size of returned segment.</param>
        /// <returns>Array of string segments as parsed from source string.</returns>
        /// <remarks>Returns a single element array with an empty string if source string is null or empty.</remarks>
        public static string[] GetStringSegments(string value, int segmentSize)
        {

            if (string.IsNullOrEmpty(value))
            {
                return new string[] { "" };
            }

            int totalSegments = Convert.ToInt32(System.Math.Ceiling(value.Length / segmentSize));
            string[] segments = new string[totalSegments];

            for (int x = 0; x <= segments.Length - 1; x++)
            {
                if (x * segmentSize + segmentSize >= value.Length)
                {
                    segments[x] = value.Substring(x * segmentSize);
                }
                else
                {
                    segments[x] = value.Substring(x * segmentSize, segmentSize);
                }
            }

            return segments;

        }

        /// <summary>
        /// Combines a dictionary of key-value pairs in to a string.
        /// </summary>
        /// <param name="pairs">Dictionary of key-value pairs.</param>
        /// <returns>A string of key-value pairs.</returns>
        public static string JoinKeyValuePairs(Dictionary<string, string> pairs)
        {

            return JoinKeyValuePairs(pairs, ';', '=');

        }

        /// <summary>
        /// Combines a dictionary of key-value pairs in to a string.
        /// </summary>
        /// <param name="pairs">Dictionary of key-value pairs.</param>
        /// <param name="parameterDelimeter">Character that delimits one key-value pair from another (eg. ';').</param>
        /// <param name="keyValueDelimeter">Character that delimits a key from its value (eg. '=').</param>
        /// <returns>A string of key-value pairs.</returns>
        public static string JoinKeyValuePairs(Dictionary<string, string> pairs, char parameterDelimeter, char keyValueDelimeter)
        {

            System.Text.StringBuilder with_1 = new StringBuilder();
            foreach (string key in pairs.Keys)
            {
                with_1.AppendFormat("{0}{1}{2}{3}", key, keyValueDelimeter, pairs(key), parameterDelimeter);
            }

            return with_1.ToString();

        }

        /// <summary>Parses key value pair parameters from a string. Parameter pairs are delimited by an equals sign, and multiple pairs separated
        /// by a semi-colon.</summary>
        /// <param name="value">Key pair string to parse.</param>
        /// <returns>Dictionary of key/value pairs.</returns>
        /// <remarks>
        /// Parses a string formatted like a typical connection string, e.g.:
        /// <code>
        /// IP=localhost; Port=1002; MaxEvents=50; UseTimeout=True
        /// </code>
        /// Note that "keys" are case-insensitive.
        /// </remarks>
        public static Dictionary<string, string> ParseKeyValuePairs(string value)
        {

            return ParseKeyValuePairs(value, ';', '=');

        }

        /// <summary>Parses key value pair parameters from a string. Parameter pairs are delimited by an equals sign, and multiple pairs separated
        /// by a semi-colon.</summary>
        /// <param name="value">Key pair string to parse.</param>
        /// <param name="parameterDelimeter">Character that delimits one key value pair from another (e.g., would be a ";" in a typical connection
        /// string).</param>
        /// <param name="keyValueDelimeter">Character that delimits key from value (e.g., would be an "=" in a typical connection string).</param>
        /// <returns>Dictionary of key/value pairs.</returns>
        /// <remarks>
        /// Parses a key value string that contains one or many pairs. Note that "keys" are case-insensitive.
        /// </remarks>
        public static Dictionary<string, string> ParseKeyValuePairs(string value, char parameterDelimeter, char keyValueDelimeter)
        {

            Dictionary<string, string> keyValuePairs = new Dictionary<string, string>(StringComparer.CurrentCultureIgnoreCase);
            string[] elements;

            // Parses out connect string parameters
            foreach (string parameter in value.Split(parameterDelimeter))
            {
                // Parses out parameter's key/value elements
                elements = parameter.Split(keyValueDelimeter);
                if (elements.Length == 2)
                {
                    KeyValuePairs.Add(elements[0].ToString().Trim(), elements[1].ToString().Trim());
                }
            }

            return keyValuePairs;

        }

        /// <summary>Ensures parameter is not an empty or null string. Returns a single space if test value is empty.</summary>
        /// <param name="testValue">Value to test for null or empty.</param>
        /// <returns>A non-empty string.</returns>
        public static string NotEmpty(string testValue)
        {

            return NotEmpty(testValue, " ");

        }

        /// <summary>Ensures parameter is not an empty or null string.</summary>
        /// <param name="testValue">Value to test for null or empty.</param>
        /// <param name="nonEmptyReturnValue">Value to return if <paramref name="testValue">testValue</paramref> is null or empty.</param>
        /// <returns>A non-empty string.</returns>
        public static string NotEmpty(string testValue, string nonEmptyReturnValue)
        {

            if (string.IsNullOrEmpty(nonEmptyReturnValue))
            {
                throw (new ArgumentException("nonEmptyReturnValue cannot be empty!"));
            }
            if (string.IsNullOrEmpty(testValue))
            {
                return nonEmptyReturnValue;
            }
            else
            {
                return testValue;
            }

        }

        /// <summary>Replaces all characters passing delegate test with specified replacement character.</summary>
        /// <param name="value">Input string.</param>
        /// <param name="replacementCharacter">Character used to replace characters passing delegate test.</param>
        /// <param name="characterTestFunction">Delegate used to determine whether or not character should be replaced.</param>
        /// <returns>Returns <paramref name="value" /> with all characters passing delegate test replaced.</returns>
        /// <remarks>Allows you to specify a replacement character (e.g., you may want to use a non-breaking space: Convert.ToChar(160)).</remarks>
        public static string ReplaceCharacters(string value, char replacementCharacter, CharacterTestFunctionSignature characterTestFunction)
			{
				
				if (string.IsNullOrEmpty(value))
				{
					return "";
				}
				
				System.Text.StringBuilder with_1 = new StringBuilder;
				char character;
				
				for (int x = 0; x <= value.Length - 1; x++)
				{
					character = value[x];
					
					if (characterTestFunction(character))
					{
						with_1.Append(replacementCharacter);
					}
					else
					{
						with_1.Append(character);
					}
				}
				
				return with_1.ToString();
				
			}

        /// <summary>Removes all characters passing delegate test from a string.</summary>
        /// <param name="value">Input string.</param>
        /// <param name="characterTestFunction">Delegate used to determine whether or not character should be removed.</param>
        /// <returns>Returns <paramref name="value" /> with all characters passing delegate test removed.</returns>
        public static string RemoveCharacters(string value, CharacterTestFunctionSignature characterTestFunction)
			{
				
				if (string.IsNullOrEmpty(value))
				{
					return "";
				}
				
				System.Text.StringBuilder with_1 = new StringBuilder;
				char character;
				
				for (int x = 0; x <= value.Length - 1; x++)
				{
					character = value[x];
					
					if (! characterTestFunction(character))
					{
						with_1.Append(character);
					}
				}
				
				return with_1.ToString();
				
			}

        /// <summary>Removes all white space (as defined by IsWhiteSpace) from a string.</summary>
        /// <param name="value">Input string.</param>
        /// <returns>Returns <paramref name="value" /> with all white space removed.</returns>
        public static string RemoveWhiteSpace(string value)
        {

            return RemoveCharacters(value, new TVA.Text.Common.CharacterTestFunctionSignature(char.IsWhiteSpace));

        }

        /// <summary>Replaces all white space characters (as defined by IsWhiteSpace) with specified replacement character.</summary>
        /// <param name="value">Input string.</param>
        /// <param name="replacementCharacter">Character used to "replace" white space characters.</param>
        /// <returns>Returns <paramref name="value" /> with all white space characters replaced.</returns>
        /// <remarks>Allows you to specify a replacement character (e.g., you may want to use a non-breaking space: Convert.ToChar(160)).</remarks>
        public static string ReplaceWhiteSpace(string value, char replacementCharacter)
        {

            return ReplaceCharacters(value, replacementCharacter, new TVA.Text.Common.CharacterTestFunctionSignature(char.IsWhiteSpace));

        }

        /// <summary>Removes all control characters from a string.</summary>
        /// <param name="value">Input string.</param>
        /// <returns>Returns <paramref name="value" /> with all control characters removed.</returns>
        public static string RemoveControlCharacters(string value)
        {

            return RemoveCharacters(value, new TVA.Text.Common.CharacterTestFunctionSignature(char.IsControl));

        }

        /// <summary>Replaces all control characters in a string with a single space.</summary>
        /// <param name="value">Input string.</param>
        /// <returns>Returns <paramref name="value" /> with all control characters replaced as a single space.</returns>
        public static string ReplaceControlCharacters(string value)
        {

            return ReplaceControlCharacters(value, ' ');

        }

        /// <summary>Replaces all control characters in a string with specified replacement character.</summary>
        /// <param name="value">Input string.</param>
        /// <param name="replacementCharacter">Character used to "replace" control characters.</param>
        /// <returns>Returns <paramref name="value" /> with all control characters replaced.</returns>
        /// <remarks>Allows you to specify a replacement character (e.g., you may want to use a non-breaking space: Convert.ToChar(160)).</remarks>
        public static string ReplaceControlCharacters(string value, char replacementCharacter)
        {

            return ReplaceCharacters(value, replacementCharacter, new TVA.Text.Common.CharacterTestFunctionSignature(char.IsControl));

        }

        /// <summary>Removes all carriage returns and line feeds from a string.</summary>
        /// <param name="value">Input string.</param>
        /// <returns>Returns <paramref name="value" /> with all CR and LF characters removed.</returns>
        public static string RemoveCrLfs(string value)
        {

            return RemoveCharacters(value, new TVA.Text.Common.CharacterTestFunctionSignature(IsCrOrLf));

        }

        /// <summary>Replaces all carriage return and line feed characters (as well as CR/LF sequences) in a string with specified replacement
        /// character.</summary>
        /// <param name="value">Input string.</param>
        /// <param name="replacementCharacter">Character used to "replace" CR and LF characters.</param>
        /// <returns>Returns <paramref name="value" /> with all CR and LF characters replaced.</returns>
        /// <remarks>Allows you to specify a replacement character (e.g., you may want to use a non-breaking space: Convert.ToChar(160)).</remarks>
        public static string ReplaceCrLfs(string value, char replacementCharacter)
        {

            return ReplaceCharacters(value.Replace(Convert.ToChar(13) + Convert.ToChar(10), replacementCharacter), replacementCharacter, new TVA.Text.Common.CharacterTestFunctionSignature(IsCrOrLf));

        }

        private static bool IsCrOrLf(char c)
        {

            return (c == Convert.ToChar(13) || c == Convert.ToChar(10));

        }

        /// <summary>Removes duplicate character strings (adjoining replication) in a string.</summary>
        /// <param name="value">Input string.</param>
        /// <param name="duplicatedValue">String whose duplicates are to be removed.</param>
        /// <returns>Returns <paramref name="value" /> with all duplicated <paramref name="duplicatedValue" /> removed.</returns>
        public static string RemoveDuplicates(string value, string duplicatedValue)
        {

            if (string.IsNullOrEmpty(value))
            {
                return "";
            }
            if (string.IsNullOrEmpty(duplicatedValue))
            {
                return value;
            }

            string duplicate = string.Concat(duplicatedValue, duplicatedValue);

            while (value.IndexOf(duplicate) > -1)
            {
                value = value.Replace(duplicate, duplicatedValue);
            }

            return value;

        }

        /// <summary>Removes the terminator (Convert.ToChar(0)) from a null terminated string (Useful for strings returned from Windows API call).</summary>
        /// <param name="value">Input string.</param>
        /// <returns>Returns <paramref name="value" /> with all characters to the left of the terminator.</returns>
        public static string RemoveNull(string value)
        {

            if (string.IsNullOrEmpty(value))
            {
                return "";
            }

            int nullPos = value.IndexOf(Convert.ToChar(0));

            if (nullPos > -1)
            {
                return value.Substring(0, nullPos);
            }
            else
            {
                return value;
            }

        }

        /// <summary>Replaces all repeating white space with a single space.</summary>
        /// <param name="value">Input string.</param>
        /// <returns>Returns <paramref name="value" /> with all duplicate white space removed.</returns>
        public static string RemoveDuplicateWhiteSpace(string value)
        {

            return RemoveDuplicateWhiteSpace(value, ' ');

        }

        /// <summary>Replaces all repeating white space with specified spacing character.</summary>
        /// <param name="value">Input string.</param>
        /// <param name="spacingCharacter">Character value to use to insert as single white space value.</param>
        /// <returns>Returns <paramref name="value" /> with all duplicate white space removed.</returns>
        /// <remarks>This function allows you to specify spacing character (e.g., you may want to use a non-breaking space: Convert.ToChar(160)).</remarks>
        public static string RemoveDuplicateWhiteSpace(string value, char spacingCharacter)
			{
				
				if (string.IsNullOrEmpty(value))
				{
					return "";
				}
				
				System.Text.StringBuilder with_1 = new StringBuilder;
				char character;
				bool lastCharWasSpace;
				
				for (int x = 0; x <= value.Length - 1; x++)
				{
					character = value[x];
					
					if (char.IsWhiteSpace(character))
					{
						lastCharWasSpace = true;
					}
					else
					{
						if (lastCharWasSpace)
						{
							with_1.Append(spacingCharacter);
						}
						with_1.Append(character);
						lastCharWasSpace = false;
					}
				}
				
				return with_1.ToString();
				
			}

        /// <summary>Counts the total number of the occurances of <paramref name="characterToCount" /> in the given string.</summary>
        /// <param name="value">Input string.</param>
        /// <param name="characterToCount">Character to be counted.</param>
        /// <returns>Total number of the occurances of <paramref name="characterToCount" /> in the given string.</returns>
        public static int CharCount(string value, char characterToCount)
        {

            if (string.IsNullOrEmpty(value))
            {
                return 0;
            }

            int total;

            for (int x = 0; x <= value.Length - 1; x++)
            {
                if (value[x] == characterToCount)
                {
                    total++;
                }
            }

            return total;

        }

        /// <summary>Tests to see if a string is contains only digits.</summary>
        /// <param name="value">Input string.</param>
        /// <returns>True, if all string's characters are digits; otherwise, false.</returns>
        public static bool IsAllDigits(string value)
        {

            if (string.IsNullOrEmpty(value))
            {
                return false;
            }

            value = value.Trim();
            if (value.Length == 0)
            {
                return false;
            }

            for (int x = 0; x <= value.Length - 1; x++)
            {
                if (!char.IsDigit(value[x]))
                {
                    return false;
                }
            }

            return true;

        }

        /// <summary>Tests to see if a string contains only numbers.</summary>
        /// <param name="value">Input string.</param>
        /// <returns>True, if all string's characters are numbers; otherwise, false.</returns>
        public static bool IsAllNumbers(string value)
        {

            if (string.IsNullOrEmpty(value))
            {
                return false;
            }

            value = value.Trim();
            if (value.Length == 0)
            {
                return false;
            }

            for (int x = 0; x <= value.Length - 1; x++)
            {
                if (!char.IsNumber(value[x]))
                {
                    return false;
                }
            }

            return true;

        }

        /// <summary>Tests to see if a string's letters are all upper case.</summary>
        /// <param name="value">Input string.</param>
        /// <returns>True, if all string's letter characters are upper case; otherwise, false.</returns>
        public static bool IsAllUpper(string value)
        {

            if (string.IsNullOrEmpty(value))
            {
                return false;
            }

            value = value.Trim();
            if (value.Length == 0)
            {
                return false;
            }

            for (int x = 0; x <= value.Length - 1; x++)
            {
                if (char.IsLetter(value[x]) && !char.IsUpper(value[x]))
                {
                    return false;
                }
            }

            return true;

        }

        /// <summary>Tests to see if a string's letters are all lower case.</summary>
        /// <param name="value">Input string.</param>
        /// <returns>True, if all string's letter characters are lower case; otherwise, false.</returns>
        public static bool IsAllLower(string value)
        {

            if (string.IsNullOrEmpty(value))
            {
                return false;
            }

            value = value.Trim();
            if (value.Length == 0)
            {
                return false;
            }

            for (int x = 0; x <= value.Length - 1; x++)
            {
                if (char.IsLetter(value[x]) && !char.IsLower(value[x]))
                {
                    return false;
                }
            }

            return true;

        }

        /// <summary>Tests to see if a string contains only letters.</summary>
        /// <param name="value">Input string.</param>
        /// <returns>True, if all string's characters are letters; otherwise, false.</returns>
        /// <remarks>Any non-letter character (e.g., punctuation marks) causes this function to return false (See overload to ignore punctuation
        /// marks.).</remarks>
        public static bool IsAllLetters(string value)
        {

            return IsAllLetters(value, false);

        }

        /// <summary>Tests to see if a string contains only letters.</summary>
        /// <param name="value">Input string.</param>
        /// <param name="ignorePunctuation">Set to True to ignore punctuation.</param>
        /// <returns>True, if all string's characters are letters; otherwise, false.</returns>
        public static bool IsAllLetters(string value, bool ignorePunctuation)
        {

            if (string.IsNullOrEmpty(value))
            {
                return false;
            }

            value = value.Trim();
            if (value.Length == 0)
            {
                return false;
            }

            for (int x = 0; x <= value.Length - 1; x++)
            {
                if (ignorePunctuation)
                {
                    if (!(char.IsLetter(value[x]) || char.IsPunctuation(value[x])))
                    {
                        return false;
                    }
                }
                else
                {
                    if (!char.IsLetter(value[x]))
                    {
                        return false;
                    }
                }
            }

            return true;

        }

        /// <summary>Tests to see if a string contains only letters or digits.</summary>
        /// <param name="value">Input string.</param>
        /// <returns>True, if all string's characters are either letters or digits; otherwise, false.</returns>
        /// <remarks>Any non-letter, non-digit character (e.g., punctuation marks) causes this function to return false (See overload to ignore
        /// punctuation marks.).</remarks>
        public static bool IsAllLettersOrDigits(string value)
        {

            return IsAllLettersOrDigits(value, false);

        }

        /// <summary>Tests to see if a string contains only letters or digits.</summary>
        /// <param name="value">Input string.</param>
        /// <param name="ignorePunctuation">Set to True to ignore punctuation.</param>
        /// <returns>True, if all string's characters are letters or digits; otherwise, false.</returns>
        public static bool IsAllLettersOrDigits(string value, bool ignorePunctuation)
        {

            if (string.IsNullOrEmpty(value))
            {
                return false;
            }

            value = value.Trim();
            if (value.Length == 0)
            {
                return false;
            }

            for (int x = 0; x <= value.Length - 1; x++)
            {
                if (ignorePunctuation)
                {
                    if (!(char.IsLetterOrDigit(value[x]) || char.IsPunctuation(value[x])))
                    {
                        return false;
                    }
                }
                else
                {
                    if (!char.IsLetterOrDigit(value[x]))
                    {
                        return false;
                    }
                }
            }

            return true;

        }

        /// <summary>Encodes the specified Unicode character in proper Regular Expression format.</summary>
        /// <param name="item">Unicode character to encode in Regular Expression format.</param>
        /// <returns>Specified Unicode character in proper Regular Expression format.</returns>
        public static string EncodeRegexChar(char item)
        {

            return "\\u" + Convert.ToUInt16(item).ToString('x').PadLeft(4, '0');

        }

        /// <summary>Decodes the specified Regular Expression character back into a standard Unicode character.</summary>
        /// <param name="value">Regular Expression character to decode back into a Unicode character.</param>
        /// <returns>Standard Unicode character representation of specified Regular Expression character.</returns>
        public static char DecodeRegexChar(string value)
        {

            return Convert.ToChar(Convert.ToUInt16(value.Replace("\\u", "0x"), 16));

        }

        /// <summary>Encodes a string into a base-64 string.</summary>
        /// <param name="value">Input string.</param>
        /// <remarks>
        /// <para>Performs a base-64 style of string encoding useful for data obfuscation or safe XML data string transmission.</para>
        /// <para>Note: This function encodes a "String". Use the Convert.ToBase64String function to encode a binary data buffer.</para>
        /// </remarks>
        public static string Base64Encode(string value)
        {

            return Convert.ToBase64String(System.Text.Encoding.Unicode.GetBytes(value));

        }

        /// <summary>Decodes a given base-64 encoded string encoded with <see cref="Base64Encode" />.</summary>
        /// <param name="value">Input string.</param>
        /// <remarks>Note: This function decodes value back into a "String". Use the Convert.FromBase64String function to decode a base-64 encoded
        /// string back into a binary data buffer.</remarks>
        public static string Base64Decode(string value)
        {

            return System.Text.Encoding.Unicode.GetString(Convert.FromBase64String(value));

        }

        /// <summary>Converts the provided string into TitleCase (upper case first letter of each word).</summary>
        /// <param name="value">Input string.</param>
        /// <remarks>Note: This function performs "ToLower" in input string then applies TextInfo.ToTitleCase for CurrentCulture. This way, even
        /// strings formatted in all-caps will still be properly formatted.</remarks>
        public static string TitleCase(string value)
        {

            return System.Globalization.CultureInfo.CurrentUICulture.TextInfo.ToTitleCase(value.ToLower());

        }

        /// <summary>
        /// Truncates the provided string from the left if it is longer that specified length.
        /// </summary>
        public static string TruncateLeft(string value, int maxLength)
        {

            if (value.Length > maxLength)
            {
                return value.Substring(value.Length - maxLength);
            }
            return value;

        }

        /// <summary>
        /// Truncates the provided string from the right if it is longer that specified length.
        /// </summary>
        public static string TruncateRight(string value, int maxLength)
        {

            if (value.Length > maxLength)
            {
                return value.Substring(0, maxLength);
            }
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
        public static string CenterText(string value, int maxLength)
        {

            return CenterText(value, maxLength, ' ');

        }

        /// <summary>
        /// Centers text within the specified maximum length, biased to the left.
        /// Text will be padded to the left and right with specified padding character.
        /// If value is greater than specified maximum length, value returned will be truncated from the right.
        /// </summary>
        /// <remarks>
        /// Handles multiple lines of text separated by Environment.NewLine.
        /// </remarks>
        static string[] CenterText_lineDelimiters = new string[] { Environment.NewLine };
        static string[] CenterText_lineDelimiters = new string[] { Environment.NewLine };
        public static string CenterText(string value, int maxLength, char paddingCharacter)
        {

            // If the text to be centered contains multiple lines, centers all the lines individually.
            StringBuilder result = new StringBuilder();
            string[] lines = value.Split(CenterText_lineDelimiters, StringSplitOptions.None);
            string line;
            int lastLineIndex = lines.Length - 1;

            for (int i = 0; i <= lastLineIndex; i++)
            {
                // Gets current line.
                line = lines[i];

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
                    if (remainingSpace % 2 > 0)
                    {
                        rightSpaces++;
                    }

                    result.Append(new string(paddingCharacter, leftSpaces));
                    result.Append(line);
                    result.Append(new string(paddingCharacter, rightSpaces));
                }

                // Creates a new line only if the original text contains multiple lines.
                if (i < lastLineIndex)
                {
                    result.AppendLine();
                }
            }

            return result.ToString();

        }

    }
}