//******************************************************************************************************
//  StringExtensionsTest.cs - Gbtc
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
//  03/20/2011 - Aniket Salver
//       Generated original version of source code.
//  12/13/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GSF.Core.Tests
{
    [TestClass]
    public class StringExtensionsTest
    {
        private readonly string strTestMethod = string.Empty;
        private string testvalue = "test";
        private string testResult = " ";
        char c = 'a';
        int intValue = 0;
        int intData = -1;
        bool boolvalue = true;
        //Func<char, bool> characterTestFunction;

        // This test method validated given String represent a boolean value.
        [TestMethod]
        public void ParseBoolean_Valid()
        {
            ////Act
            bool result = strTestMethod.ParseBoolean();
            ////Assert
            Assert.AreEqual(false, result);
        }

        // This method will validate given string parameter is not an empty or null string
        [TestMethod]
        public void NotEmptyWithParameter_Valid()
        {
            ////Act
            string result = strTestMethod.NotEmpty(testvalue);
            ////Assert
            Assert.AreEqual(testvalue, result);
        }

        // This test method Ensures parameter is not an empty or null string
        [TestMethod]
        public void NotEmpty_Valid()
        {
            ////Act
            string result = strTestMethod.NotEmpty();
            ////Assert
            Assert.AreEqual(testResult, result);
        }

        // This test method Removes all white space from a string
        [TestMethod]
        public void RemoveWhiteSpace_Valid()
        {
            ////Act
            string result = strTestMethod.RemoveWhiteSpace();
            ////Assert
            Assert.AreEqual(string.Empty, result);
        }

        // This test method Removes all white space from a string with input character
        [TestMethod]
        public void ReplaceWhiteSpacewithparameter_Valid()
        {
            ////Act
            string result = strTestMethod.ReplaceWhiteSpace(c);
            ////Assert
            Assert.AreEqual(string.Empty, result);
        }

        // This test method will Removes all control characters from a string.
        [TestMethod]
        public void RemoveControlCharacters_Valid()
        {
            ////Act
            string result = strTestMethod.RemoveControlCharacters();
            ////Assert
            Assert.AreEqual(string.Empty, result);
        }

        // This test method will Replace all control characters from a string.
        [TestMethod]
        public void ReplaceControlCharacters_Valid()
        {
            ////Act
            string result = strTestMethod.ReplaceControlCharacters();
            ////Assert
            Assert.AreEqual(string.Empty, result);
        }

        // This test method will Replace all control characters from a string with input parameter as char.
        [TestMethod]
        public void ReplaceControlCharacterswithparameter_Valid()
        {
            ////Act
            string result = strTestMethod.ReplaceControlCharacters(c);
            ////Assert
            Assert.AreEqual(string.Empty, result);
        }

        // This test method will Removes all carriage returns and line feeds from a string
        [TestMethod]
        public void RemoveCrLfs_Valid()
        {
            ////Act
            string result = strTestMethod.RemoveCrLfs();
            ////Assert
            Assert.AreEqual(string.Empty, result);
        }

        // This test method will Removes all carriage returns and line feeds from a string.
        [TestMethod]
        public void RemoveCrLfswithparameter_Valid()
        {
            ////Act
            string result = strTestMethod.ReplaceCrLfs(c);
            ////Assert
            Assert.AreEqual(string.Empty, result);
        }

        // This test method will Removes duplicate character strings (adjoining replication) in a string.
        [TestMethod]
        public void RemoveDuplicates_Valid()
        {
            ////Act
            string result = strTestMethod.RemoveDuplicates(testvalue);
            ////Assert
            Assert.AreEqual(string.Empty, result);
        }

        // This test method will Removes the terminator ('\0') from a null terminated string.
        [TestMethod]
        public void RemoveNull_Valid()
        {
            ////Act
            string result = strTestMethod.RemoveNull();
            ////Assert
            Assert.AreEqual(string.Empty, result);
        }

        // This test method will Replaces all repeating white space with specified spacing character.
        [TestMethod]
        public void RemoveDuplicateWhiteSpace_Valid()
        {
            ////Act
            string result = strTestMethod.RemoveDuplicateWhiteSpace();
            ////Assert
            Assert.AreEqual(string.Empty, result);
        }

        // This test method will Replaces all repeating white space with specified spacing character.
        [TestMethod]
        public void RemoveDuplicateWhiteSpacewithparameter_Valid()
        {
            ////Act
            string result = strTestMethod.RemoveDuplicateWhiteSpace(c);
            ////Assert
            Assert.AreEqual(string.Empty, result);
        }

        // This test method will Counts the total number of the occurrences of a character in the given string.
        [TestMethod]
        public void CharCount_Valid()
        {
            ////Act
            int result = strTestMethod.CharCount(c);
            ////Assert
            Assert.AreEqual(intValue, result);
        }

        // This test method will Tests to see if a string is contains only digits based on Char.IsDigit function.
        [TestMethod]
        public void IsAllDigits_Valid()
        {
            ////Act
            bool result = strTestMethod.IsAllDigits();
            ////Assert
            Assert.AreEqual(false, result);
        }

        // This test method will Tests to see if a string contains only numbers based on Char.IsNumber function.
        [TestMethod]
        public void IsAllNumbers_Valid()
        {
            ////Act
            bool result = strTestMethod.IsAllNumbers();
            ////Assert
            Assert.AreEqual(false, result);
        }

        // This test method will Tests to see if a string's letters are all upper case.
        [TestMethod]
        public void IsAllUpper_Valid()
        {
            ////Act
            bool result = strTestMethod.IsAllUpper();
            ////Assert
            Assert.AreEqual(false, result);
        }

        // This test method will Tests to see if a string's letters are all lower case.
        [TestMethod]
        public void IsAllLower_Valid()
        {
            ////Act
            bool result = strTestMethod.IsAllLower();
            ////Assert
            Assert.AreEqual(false, result);
        }

        // This test method will Tests to see if a string contains only letters.
        [TestMethod]
        public void IsAllLetters_Valid()
        {
            ////Act
            bool result = strTestMethod.IsAllLetters();
            ////Assert
            Assert.AreEqual(false, result);
        }

        // This test method will Tests to see if a string contains only letters.
        [TestMethod]
        public void IsAllLetterswithparameter_Valid()
        {
            ////Act
            bool result = strTestMethod.IsAllLetters(boolvalue);
            ////Assert
            Assert.AreEqual(false, result);
        }

        // This test method will Tests to see if a string contains only letters or digits.
        [TestMethod]
        public void IsAllLettersOrDigits_Valid()
        {
            ////Act
            bool result = strTestMethod.IsAllLettersOrDigits();
            ////Assert
            Assert.AreEqual(false, result);
        }

        // This test method will Tests to see if a string contains only letters or digits.
        [TestMethod]
        public void IsAllLettersOrDigitswithparameter_Valid()
        {
            ////Act
            bool result = strTestMethod.IsAllLettersOrDigits(boolvalue);
            ////Assert
            Assert.AreEqual(false, result);
        }

        // This test method will Encodes a string into a base-64 string.
        [TestMethod]
        public void Base64Encode_Valid()
        {
            ////Act
            string result = strTestMethod.Base64Encode();
            ////Assert
            Assert.AreEqual(string.Empty, result);
        }

        // This test method will Decodes a given base-64 encoded string encoded with
        [TestMethod]
        public void Base64Decode_Valid()
        {
            ////Act
            string result = strTestMethod.Base64Decode();
            ////Assert
            Assert.AreEqual(string.Empty, result);
        }

        // This test method will Converts the provided string into title case (upper case first letter of each word).
        [TestMethod]
        public void ToTitleCase_Valid()
        {
            ////Act
            string result = strTestMethod.ToTitleCase();
            ////Assert
            Assert.AreEqual(string.Empty, result);
        }

        // This test method will Truncates the provided string from the left if it is longer that specified length.
        [TestMethod]
        public void TruncateLeft_Valid()
        {
            ////Act
            string result = strTestMethod.TruncateLeft(intValue);
            ////Assert
            Assert.AreEqual(string.Empty, result);
        }

        // This test method will Truncates the provided string from the right if it is longer that specified length.
        [TestMethod]
        public void TruncateRight_Valid()
        {
            ////Act
            string result = strTestMethod.TruncateRight(intValue);
            ////Assert
            Assert.AreEqual(string.Empty, result);
        }

        // This test method will Centers text within the specified maximum length, biased to the left.
        [TestMethod]
        public void CenterText_Valid()
        {
            ////Act
            string result = strTestMethod.CenterText(intValue);
            ////Assert
            Assert.AreEqual(string.Empty, result);
        }

        // This test method will Centers text within the specified maximum length, biased to the left.
        [TestMethod]
        public void CenterTextWithParameter_Valid()
        {
            ////Act
            string result = strTestMethod.CenterText(intValue, c);
            ////Assert
            Assert.AreEqual(string.Empty, result);
        }

        // This test method will Performs a case insensitive string replacement.
        [TestMethod]
        public void ReplaceCaseInsensitive_Valid()
        {
            ////Act
            string result = strTestMethod.ReplaceCaseInsensitive(testvalue, testResult);
            ////Assert
            Assert.AreEqual(string.Empty, result);
        }

        // This test method will Ensures a string starts with a specific string.
        [TestMethod]
        public void EnsureStart_Valid()
        {
            ////Act
            string result = strTestMethod.EnsureStart(c);
            ////Assert
            Assert.AreEqual(string.Empty, result);
        }

        // This test method will Ensures a string starts with a specific string.
        [TestMethod]
        public void EnsureStart1_Valid()
        {
            ////Act
            string result = strTestMethod.EnsureStart(testvalue);
            ////Assert
            Assert.AreEqual(string.Empty, result);
        }

        /// This test method will Ensures a string starts with a specific string.
        [TestMethod]
        public void EnsureStart2_Valid()
        {
            ////Act
            string result = strTestMethod.EnsureStart(c, boolvalue);
            ////Assert
            Assert.AreEqual(string.Empty, result);
        }

        // This test method will Ensures a string ends with a specific character.
        [TestMethod]
        public void EnsureEnd_Valid()
        {
            ////Act
            string result = strTestMethod.EnsureEnd(c);
            ////Assert
            Assert.AreEqual(string.Empty, result);
        }

        // This test method will Ensures a string ends with a specific character.
        [TestMethod]
        public void EnsureEnd1_Valid()
        {
            ////Act
            string result = strTestMethod.EnsureEnd(testvalue);
            ////Assert
            Assert.AreEqual(string.Empty, result);
        }

        // This test method will Ensures a string ends with a specific string with Input parameter.
        [TestMethod]
        public void EnsureEnd2_Valid()
        {
            ////Act
            string result = strTestMethod.EnsureEnd(c, boolvalue);
            ////Assert
            Assert.AreEqual(string.Empty, result);
        }

        // This test method will Reverses the order of the characters in a string.
        [TestMethod]
        public void Reverse_Valid()
        {
            ////Act
            string result = strTestMethod.Reverse();
            ////Assert
            Assert.AreEqual(string.Empty, result);
        }

        // This test method will Searches a string for a repeated instance of the specified.
        [TestMethod]
        public void IndexOfRepeatedChar_Valid()
        {
            ////Act
            int result = strTestMethod.IndexOfRepeatedChar();
            ////Assert
            Assert.AreEqual(intData, result);
        }

        // This test method will Searches a string for a repeated instance of the specified.
        [TestMethod]
        public void IndexOfRepeatedChar2_Valid()
        {
            ////Act
            int result = strTestMethod.IndexOfRepeatedChar(c);
            ////Assert
            Assert.AreEqual(intData, result);
        }

        // This test method will Searches a string for a repeated instance of the specified.
        [TestMethod]
        public void IndexOfRepeatedChar3_Valid()
        {
            ////Act
            int result = strTestMethod.IndexOfRepeatedChar(intValue);
            ////Assert
            Assert.AreEqual(intData, result);
        }

        // This test method will Returns the index of the last repeated index of the first group of repeated characters that begin with the.
        [TestMethod]
        public void IndexOfRepeatedChar4_Valid()
        {
            ////Act
            int result = strTestMethod.IndexOfRepeatedChar(c, intValue);
            ////Assert
            Assert.AreEqual(intData, result);
        }

        // This test method will Places an ellipsis in the middle of a string as it is trimmed to length specified.
        [TestMethod]
        public void TrimWithEllipsisMiddle_Valid()
        {
            ////Act
            string result = strTestMethod.TrimWithEllipsisMiddle(intValue);
            ////Assert
            Assert.AreEqual(string.Empty, result);
        }

        // This test method will Places an ellipsis at the end of a string as it is trimmed to length specified.
        [TestMethod]
        public void TrimWithEllipsisEnd_Valid()
        {
            ////Act
            string result = strTestMethod.TrimWithEllipsisEnd(intValue);
            ////Assert
            Assert.AreEqual(string.Empty, result);
        }
    }
}
