//*******************************************************************************************************
//  CharExtensions.cs
//  Copyright © 2009 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: F. Russell Robertson
//      Office: PSO TRAN & REL, CHATTANOOGA - MR BK-C
//       Phone: 423/751-4183
//       Email: frrobertson@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  12/13/2008 - F. Russell Robertson
//       Generated original version of source code.
//
//*******************************************************************************************************

using System;
using System.Text;
using System.Collections.Generic;

namespace PCS
{
    /// <summary>Defines extension functions related to character manipulation.</summary>
    public static class CharExtensions
    {
        // so that this only happens one time
        private static char[] wordSeperators = { ' ', ',', '.', '?', '!', ':', ';', '&', '\"', '/', '\\', '<', '>', '=', '{', '}', '(', ')', '[', ']', '@', '*'};
        private static char[] numericValues = { '-', '+', ',', '.' };

        /// <summary>
        /// Encodes the specified Unicode character in proper Regular Expression format.
        /// </summary>
        /// <param name="item">Unicode character to encode in Regular Expression format.</param>
        /// <returns>Specified Unicode character in proper Regular Expression format.</returns>
        public static string RegexEncode(this char item)
        {
            return "\\u" + Convert.ToUInt16(item).ToString("x").PadLeft(4, '0');
        }

        /// <summary>
        /// Tests a character to determine if it marks the end of a typical english word.
        /// </summary>
        /// <param name="value">Input character to check.</param>
        /// <returns><c>true</c> if character is a work seperator.</returns>
        /// <remarks>
        /// Preforms no testing for ASCII codes &gt; 127.<br/>
        /// Does not seperate words based on punctuation of ' %  - _  <br/>
        /// However does include the angle bracket symbols &lt; &gt; as seperators<br/>
        /// <br/>
        /// For reference the standard char tests are:
        /// <ul>
        /// <li>"IsSperator (1) == simple space (32 or 160) only.</li>
        /// <li>IsPunctuation (23) == . , ! ? : ; " ' [ ] { } ( ) \ / @ % # * &amp; - _  (plus other char's &gt; 127)</li>
        /// <li>IsSymbol (8) == $ + &lt; &gt; = ^ ` ~</li>
        /// <li>IsWhiteSpace (6) == control char's 9 thry 13, plus 32 -- TAB, LF, VT, FF, CR, SP</li>
        /// </ul>
        /// </remarks>
        public static bool IsWordTerminator(this char value)
        {
            if (value < 32)
                return true;

            return value.IsAny(wordSeperators);
        }

        /// <summary>
        /// Tests a character to determine if is a common part of a numeric string (digits or one of "+ - , .")
        /// </summary>
        /// <param name="value">The character to check.</param>
        /// <returns><c>true</c> if numeric character.</returns>
        public static bool IsNumeric(this char value)
        {
            if (char.IsDigit(value))
                return true;

            return value.IsAny(numericValues);
        }

        /// <summary>
        /// Determines if a character matches any character in a sent array.
        /// </summary>
        /// <param name="value">The character to check.</param>
        /// <param name="testChars">The array of characters to test.</param>
        /// <returns></returns>
        public static bool IsAny(this char value, IEnumerable<char> testChars)
        {
            foreach (char c in testChars)
            {
                if (value == c)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Tests a character to determine if it is between a specified character range
        /// </summary>
        /// <param name="value">Input character to process.</param>
        /// <param name="startOfRange">Beginning of range character.</param>
        /// <param name="endOfRange">End of range character.</param>
        /// <returns><c>true</c> is the character is within the range.</returns>
        public static bool IsInRange(this char value, char startOfRange, char endOfRange)
        {
            if (value >= startOfRange && value <= endOfRange)
                return true;

            return false;
        }
    }
}