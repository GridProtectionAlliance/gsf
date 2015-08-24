//*******************************************************************************************************
//  StringExtensionsTest.cs - Gbtc
//
//  Tennessee Valley Authority, 2010
//  No copyright is claimed pursuant to 17 USC § 105.  All Other Rights Reserved.
//  Code in this file licensed to TVA under one or more contributor license agreements listed below.
//
//  This software is made freely available under the TVA Open Source Agreement (see below).
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  03/20/2011 - Aniket Salver
//       Generated original version of source code.
//
//*******************************************************************************************************

#region [ TVA Open Source Agreement ]
/*

 THIS OPEN SOURCE AGREEMENT ("AGREEMENT") DEFINES THE RIGHTS OF USE,REPRODUCTION, DISTRIBUTION,
 MODIFICATION AND REDISTRIBUTION OF CERTAIN COMPUTER SOFTWARE ORIGINALLY RELEASED BY THE
 TENNESSEE VALLEY AUTHORITY, A CORPORATE AGENCY AND INSTRUMENTALITY OF THE UNITED STATES GOVERNMENT
 ("GOVERNMENT AGENCY"). GOVERNMENT AGENCY IS AN INTENDED THIRD-PARTY BENEFICIARY OF ALL SUBSEQUENT
 DISTRIBUTIONS OR REDISTRIBUTIONS OF THE SUBJECT SOFTWARE. ANYONE WHO USES, REPRODUCES, DISTRIBUTES,
 MODIFIES OR REDISTRIBUTES THE SUBJECT SOFTWARE, AS DEFINED HEREIN, OR ANY PART THEREOF, IS, BY THAT
 ACTION, ACCEPTING IN FULL THE RESPONSIBILITIES AND OBLIGATIONS CONTAINED IN THIS AGREEMENT.

 Original Software Designation: openPDC
 Original Software Title: The TVA Open Source Phasor Data Concentrator
 User Registration Requested. Please Visit https://naspi.tva.com/Registration/
 Point of Contact for Original Software: J. Ritchie Carroll <mailto:jrcarrol@tva.gov>

 1. DEFINITIONS

 A. "Contributor" means Government Agency, as the developer of the Original Software, and any entity
 that makes a Modification.

 B. "Covered Patents" mean patent claims licensable by a Contributor that are necessarily infringed by
 the use or sale of its Modification alone or when combined with the Subject Software.

 C. "Display" means the showing of a copy of the Subject Software, either directly or by means of an
 image, or any other device.

 D. "Distribution" means conveyance or transfer of the Subject Software, regardless of means, to
 another.

 E. "Larger Work" means computer software that combines Subject Software, or portions thereof, with
 software separate from the Subject Software that is not governed by the terms of this Agreement.

 F. "Modification" means any alteration of, including addition to or deletion from, the substance or
 structure of either the Original Software or Subject Software, and includes derivative works, as that
 term is defined in the Copyright Statute, 17 USC § 101. However, the act of including Subject Software
 as part of a Larger Work does not in and of itself constitute a Modification.

 G. "Original Software" means the computer software first released under this Agreement by Government
 Agency entitled openPDC, including source code, object code and accompanying documentation, if any.

 H. "Recipient" means anyone who acquires the Subject Software under this Agreement, including all
 Contributors.

 I. "Redistribution" means Distribution of the Subject Software after a Modification has been made.

 J. "Reproduction" means the making of a counterpart, image or copy of the Subject Software.

 K. "Sale" means the exchange of the Subject Software for money or equivalent value.

 L. "Subject Software" means the Original Software, Modifications, or any respective parts thereof.

 M. "Use" means the application or employment of the Subject Software for any purpose.

 2. GRANT OF RIGHTS

 A. Under Non-Patent Rights: Subject to the terms and conditions of this Agreement, each Contributor,
 with respect to its own contribution to the Subject Software, hereby grants to each Recipient a
 non-exclusive, world-wide, royalty-free license to engage in the following activities pertaining to
 the Subject Software:

 1. Use

 2. Distribution

 3. Reproduction

 4. Modification

 5. Redistribution

 6. Display

 B. Under Patent Rights: Subject to the terms and conditions of this Agreement, each Contributor, with
 respect to its own contribution to the Subject Software, hereby grants to each Recipient under Covered
 Patents a non-exclusive, world-wide, royalty-free license to engage in the following activities
 pertaining to the Subject Software:

 1. Use

 2. Distribution

 3. Reproduction

 4. Sale

 5. Offer for Sale

 C. The rights granted under Paragraph B. also apply to the combination of a Contributor's Modification
 and the Subject Software if, at the time the Modification is added by the Contributor, the addition of
 such Modification causes the combination to be covered by the Covered Patents. It does not apply to
 any other combinations that include a Modification. 

 D. The rights granted in Paragraphs A. and B. allow the Recipient to sublicense those same rights.
 Such sublicense must be under the same terms and conditions of this Agreement.

 3. OBLIGATIONS OF RECIPIENT

 A. Distribution or Redistribution of the Subject Software must be made under this Agreement except for
 additions covered under paragraph 3H. 

 1. Whenever a Recipient distributes or redistributes the Subject Software, a copy of this Agreement
 must be included with each copy of the Subject Software; and

 2. If Recipient distributes or redistributes the Subject Software in any form other than source code,
 Recipient must also make the source code freely available, and must provide with each copy of the
 Subject Software information on how to obtain the source code in a reasonable manner on or through a
 medium customarily used for software exchange.

 B. Each Recipient must ensure that the following copyright notice appears prominently in the Subject
 Software:

          No copyright is claimed pursuant to 17 USC § 105.  All Other Rights Reserved.

 C. Each Contributor must characterize its alteration of the Subject Software as a Modification and
 must identify itself as the originator of its Modification in a manner that reasonably allows
 subsequent Recipients to identify the originator of the Modification. In fulfillment of these
 requirements, Contributor must include a file (e.g., a change log file) that describes the alterations
 made and the date of the alterations, identifies Contributor as originator of the alterations, and
 consents to characterization of the alterations as a Modification, for example, by including a
 statement that the Modification is derived, directly or indirectly, from Original Software provided by
 Government Agency. Once consent is granted, it may not thereafter be revoked.

 D. A Contributor may add its own copyright notice to the Subject Software. Once a copyright notice has
 been added to the Subject Software, a Recipient may not remove it without the express permission of
 the Contributor who added the notice.

 E. A Recipient may not make any representation in the Subject Software or in any promotional,
 advertising or other material that may be construed as an endorsement by Government Agency or by any
 prior Recipient of any product or service provided by Recipient, or that may seek to obtain commercial
 advantage by the fact of Government Agency's or a prior Recipient's participation in this Agreement.

 F. In an effort to track usage and maintain accurate records of the Subject Software, each Recipient,
 upon receipt of the Subject Software, is requested to register with Government Agency by visiting the
 following website: https://naspi.tva.com/Registration/. Recipient's name and personal information
 shall be used for statistical purposes only. Once a Recipient makes a Modification available, it is
 requested that the Recipient inform Government Agency at the web site provided above how to access the
 Modification.

 G. Each Contributor represents that that its Modification does not violate any existing agreements,
 regulations, statutes or rules, and further that Contributor has sufficient rights to grant the rights
 conveyed by this Agreement.

 H. A Recipient may choose to offer, and to charge a fee for, warranty, support, indemnity and/or
 liability obligations to one or more other Recipients of the Subject Software. A Recipient may do so,
 however, only on its own behalf and not on behalf of Government Agency or any other Recipient. Such a
 Recipient must make it absolutely clear that any such warranty, support, indemnity and/or liability
 obligation is offered by that Recipient alone. Further, such Recipient agrees to indemnify Government
 Agency and every other Recipient for any liability incurred by them as a result of warranty, support,
 indemnity and/or liability offered by such Recipient.

 I. A Recipient may create a Larger Work by combining Subject Software with separate software not
 governed by the terms of this agreement and distribute the Larger Work as a single product. In such
 case, the Recipient must make sure Subject Software, or portions thereof, included in the Larger Work
 is subject to this Agreement.

 J. Notwithstanding any provisions contained herein, Recipient is hereby put on notice that export of
 any goods or technical data from the United States may require some form of export license from the
 U.S. Government. Failure to obtain necessary export licenses may result in criminal liability under
 U.S. laws. Government Agency neither represents that a license shall not be required nor that, if
 required, it shall be issued. Nothing granted herein provides any such export license.

 4. DISCLAIMER OF WARRANTIES AND LIABILITIES; WAIVER AND INDEMNIFICATION

 A. No Warranty: THE SUBJECT SOFTWARE IS PROVIDED "AS IS" WITHOUT ANY WARRANTY OF ANY KIND, EITHER
 EXPRESSED, IMPLIED, OR STATUTORY, INCLUDING, BUT NOT LIMITED TO, ANY WARRANTY THAT THE SUBJECT
 SOFTWARE WILL CONFORM TO SPECIFICATIONS, ANY IMPLIED WARRANTIES OF MERCHANTABILITY, FITNESS FOR A
 PARTICULAR PURPOSE, OR FREEDOM FROM INFRINGEMENT, ANY WARRANTY THAT THE SUBJECT SOFTWARE WILL BE ERROR
 FREE, OR ANY WARRANTY THAT DOCUMENTATION, IF PROVIDED, WILL CONFORM TO THE SUBJECT SOFTWARE. THIS
 AGREEMENT DOES NOT, IN ANY MANNER, CONSTITUTE AN ENDORSEMENT BY GOVERNMENT AGENCY OR ANY PRIOR
 RECIPIENT OF ANY RESULTS, RESULTING DESIGNS, HARDWARE, SOFTWARE PRODUCTS OR ANY OTHER APPLICATIONS
 RESULTING FROM USE OF THE SUBJECT SOFTWARE. FURTHER, GOVERNMENT AGENCY DISCLAIMS ALL WARRANTIES AND
 LIABILITIES REGARDING THIRD-PARTY SOFTWARE, IF PRESENT IN THE ORIGINAL SOFTWARE, AND DISTRIBUTES IT
 "AS IS."

 B. Waiver and Indemnity: RECIPIENT AGREES TO WAIVE ANY AND ALL CLAIMS AGAINST GOVERNMENT AGENCY, ITS
 AGENTS, EMPLOYEES, CONTRACTORS AND SUBCONTRACTORS, AS WELL AS ANY PRIOR RECIPIENT. IF RECIPIENT'S USE
 OF THE SUBJECT SOFTWARE RESULTS IN ANY LIABILITIES, DEMANDS, DAMAGES, EXPENSES OR LOSSES ARISING FROM
 SUCH USE, INCLUDING ANY DAMAGES FROM PRODUCTS BASED ON, OR RESULTING FROM, RECIPIENT'S USE OF THE
 SUBJECT SOFTWARE, RECIPIENT SHALL INDEMNIFY AND HOLD HARMLESS  GOVERNMENT AGENCY, ITS AGENTS,
 EMPLOYEES, CONTRACTORS AND SUBCONTRACTORS, AS WELL AS ANY PRIOR RECIPIENT, TO THE EXTENT PERMITTED BY
 LAW.  THE FOREGOING RELEASE AND INDEMNIFICATION SHALL APPLY EVEN IF THE LIABILITIES, DEMANDS, DAMAGES,
 EXPENSES OR LOSSES ARE CAUSED, OCCASIONED, OR CONTRIBUTED TO BY THE NEGLIGENCE, SOLE OR CONCURRENT, OF
 GOVERNMENT AGENCY OR ANY PRIOR RECIPIENT.  RECIPIENT'S SOLE REMEDY FOR ANY SUCH MATTER SHALL BE THE
 IMMEDIATE, UNILATERAL TERMINATION OF THIS AGREEMENT.

 5. GENERAL TERMS

 A. Termination: This Agreement and the rights granted hereunder will terminate automatically if a
 Recipient fails to comply with these terms and conditions, and fails to cure such noncompliance within
 thirty (30) days of becoming aware of such noncompliance. Upon termination, a Recipient agrees to
 immediately cease use and distribution of the Subject Software. All sublicenses to the Subject
 Software properly granted by the breaching Recipient shall survive any such termination of this
 Agreement.

 B. Severability: If any provision of this Agreement is invalid or unenforceable under applicable law,
 it shall not affect the validity or enforceability of the remainder of the terms of this Agreement.

 C. Applicable Law: This Agreement shall be subject to United States federal law only for all purposes,
 including, but not limited to, determining the validity of this Agreement, the meaning of its
 provisions and the rights, obligations and remedies of the parties.

 D. Entire Understanding: This Agreement constitutes the entire understanding and agreement of the
 parties relating to release of the Subject Software and may not be superseded, modified or amended
 except by further written agreement duly executed by the parties.

 E. Binding Authority: By accepting and using the Subject Software under this Agreement, a Recipient
 affirms its authority to bind the Recipient to all terms and conditions of this Agreement and that
 Recipient hereby agrees to all terms and conditions herein.

 F. Point of Contact: Any Recipient contact with Government Agency is to be directed to the designated
 representative as follows: J. Ritchie Carroll <mailto:jrcarrol@tva.gov>.

*/
#endregion

#region [ Contributor License Agreements ]

//******************************************************************************************************
//
//  Copyright © 2011, Grid Protection Alliance.  All Rights Reserved.
//
//  The GPA licenses this file to you under the Eclipse Public License -v 1.0 (the "License"); you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://www.opensource.org/licenses/eclipse-1.0.php
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//******************************************************************************************************

#endregion

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Windows.Forms;

namespace TVA.Core.Tests
{
    [TestClass()]
    public class StringExtensionsTest
    {
        private string strTestMethod = string.Empty;
        private string testvalue = "test";
        private string testResult = " ";
        char c = 'a';
        int intValue = 0;
        int intData = -1;
        bool boolvalue = true;
        //Func<char, bool> characterTestFunction;

        // This test method validated given String represent a boolean value.
        [TestMethod()]
        public void ParseBoolean_Valid()
        {
                ////Act
                bool result = strTestMethod.ParseBoolean();
                ////Assert
                Assert.AreEqual(false, result);
        }

        // This method will validate given string parameter is not an empty or null string
        [TestMethod()]
        public void NotEmptyWithParameter_Valid()
        {
                ////Act
                string result = strTestMethod.NotEmpty(testvalue);
                ////Assert
                Assert.AreEqual(testvalue, result);
        }

        // This test method Ensures parameter is not an empty or null string
        [TestMethod()]
        public void NotEmpty_Valid()
        {
                ////Act
                string result = strTestMethod.NotEmpty();
                ////Assert
                Assert.AreEqual(testResult, result);
        }

        // This test method Removes all white space from a string
        [TestMethod()]
        public void RemoveWhiteSpace_Valid()
        {
                ////Act
                string result = strTestMethod.RemoveWhiteSpace();
                ////Assert
                Assert.AreEqual(string.Empty, result);
        }

        // This test method Removes all white space from a string with input character
        [TestMethod()]
        public void ReplaceWhiteSpacewithparameter_Valid()
        {
                ////Act
                string result = strTestMethod.ReplaceWhiteSpace(c);
                ////Assert
                Assert.AreEqual(string.Empty, result);
        }

        // This test method will Removes all control characters from a string.
        [TestMethod()]
        public void RemoveControlCharacters_Valid()
        {
                ////Act
                string result = strTestMethod.RemoveControlCharacters();
                ////Assert
                Assert.AreEqual(string.Empty, result);
        }

        // This test method will Replace all control characters from a string.
        [TestMethod()]
        public void ReplaceControlCharacters_Valid()
        {
                ////Act
                string result = strTestMethod.ReplaceControlCharacters();
                ////Assert
                Assert.AreEqual(string.Empty, result);
        }

        // This test method will Replace all control characters from a string with input parameter as char.
        [TestMethod()]
        public void ReplaceControlCharacterswithparameter_Valid()
        {
                ////Act
                string result = strTestMethod.ReplaceControlCharacters(c);
                ////Assert
                Assert.AreEqual(string.Empty, result);
        }

        // This test method will Removes all carriage returns and line feeds from a string
        [TestMethod()]
        public void RemoveCrLfs_Valid()
        {
                ////Act
                string result = strTestMethod.RemoveCrLfs();
                ////Assert
                Assert.AreEqual(string.Empty, result);
        }

        // This test method will Removes all carriage returns and line feeds from a string.
        [TestMethod()]
        public void RemoveCrLfswithparameter_Valid()
        {
                ////Act
                string result = strTestMethod.ReplaceCrLfs(c);
                ////Assert
                Assert.AreEqual(string.Empty, result);
        }

        // This test method will Removes duplicate character strings (adjoining replication) in a string.
        [TestMethod()]
        public void RemoveDuplicates_Valid()
        {
                ////Act
                string result = strTestMethod.RemoveDuplicates(testvalue);
                ////Assert
                Assert.AreEqual(string.Empty, result);
        }

        // This test method will Removes the terminator ('\0') from a null terminated string.
        [TestMethod()]
        public void RemoveNull_Valid()
        {
                ////Act
                string result = strTestMethod.RemoveNull();
                ////Assert
                Assert.AreEqual(string.Empty, result);
        }

        // This test method will Replaces all repeating white space with specified spacing character.
        [TestMethod()]
        public void RemoveDuplicateWhiteSpace_Valid()
        {
                ////Act
                string result = strTestMethod.RemoveDuplicateWhiteSpace();
                ////Assert
                Assert.AreEqual(string.Empty, result);
        }

        // This test method will Replaces all repeating white space with specified spacing character.
        [TestMethod()]
        public void RemoveDuplicateWhiteSpacewithparameter_Valid()
        {
                ////Act
                string result = strTestMethod.RemoveDuplicateWhiteSpace(c);
                ////Assert
                Assert.AreEqual(string.Empty, result);
        }

        // This test method will Counts the total number of the occurances of a character in the given string.
        [TestMethod()]
        public void CharCount_Valid()
        {
                ////Act
                int result = strTestMethod.CharCount(c);
                ////Assert
                Assert.AreEqual(intValue, result);
        }

        // This test method will Tests to see if a string is contains only digits based on Char.IsDigit function.
        [TestMethod()]
        public void IsAllDigits_Valid()
        {
                ////Act
                bool result = strTestMethod.IsAllDigits();
                ////Assert
                Assert.AreEqual(false, result);
        }

        // This test method will Tests to see if a string contains only numbers based on Char.IsNumber function.
        [TestMethod()]
        public void IsAllNumbers_Valid()
        {
                ////Act
                bool result = strTestMethod.IsAllNumbers();
                ////Assert
                Assert.AreEqual(false, result);
        }

        // This test method will Tests to see if a string's letters are all upper case.
        [TestMethod()]
        public void IsAllUpper_Valid()
        {
                ////Act
                bool result = strTestMethod.IsAllUpper();
                ////Assert
                Assert.AreEqual(false, result);
        }

        // This test method will Tests to see if a string's letters are all lower case.
        [TestMethod()]
        public void IsAllLower_Valid()
        {
                ////Act
                bool result = strTestMethod.IsAllLower();
                ////Assert
                Assert.AreEqual(false, result);
        }

        // This test method will Tests to see if a string contains only letters.
        [TestMethod()]
        public void IsAllLetters_Valid()
        {
                ////Act
                bool result = strTestMethod.IsAllLetters();
                ////Assert
                Assert.AreEqual(false, result);
        }

        // This test method will Tests to see if a string contains only letters.
        [TestMethod()]
        public void IsAllLetterswithparameter_Valid()
        {
                ////Act
                bool result = strTestMethod.IsAllLetters(boolvalue);
                ////Assert
                Assert.AreEqual(false, result);
        }

        // This test method will Tests to see if a string contains only letters or digits.
        [TestMethod()]
        public void IsAllLettersOrDigits_Valid()
        {
                ////Act
                bool result = strTestMethod.IsAllLettersOrDigits();
                ////Assert
                Assert.AreEqual(false, result);
        }

        // This test method will Tests to see if a string contains only letters or digits.
        [TestMethod()]
        public void IsAllLettersOrDigitswithparameter_Valid()
        {
                ////Act
                bool result = strTestMethod.IsAllLettersOrDigits(boolvalue);
                ////Assert
                Assert.AreEqual(false, result);
        }

        // This test method will Encodes a string into a base-64 string.
        [TestMethod()]
        public void Base64Encode_Valid()
        {
                ////Act
                string result = strTestMethod.Base64Encode();
                ////Assert
                Assert.AreEqual(string.Empty, result);
        }

        // This test method will Decodes a given base-64 encoded string encoded with
        [TestMethod()]
        public void Base64Decode_Valid()
        {
                ////Act
                string result = strTestMethod.Base64Decode();
                ////Assert
                Assert.AreEqual(string.Empty, result);
        }

        // This test method will Converts the provided string into title case (upper case first letter of each word).
        [TestMethod()]
        public void ToTitleCase_Valid()
        {
                ////Act
                string result = strTestMethod.ToTitleCase();
                ////Assert
                Assert.AreEqual(string.Empty, result);
        }

        // This test method will Truncates the provided string from the left if it is longer that specified length.
        [TestMethod()]
        public void TruncateLeft_Valid()
        {
                ////Act
                string result = strTestMethod.TruncateLeft(intValue);
                ////Assert
                Assert.AreEqual(string.Empty, result);
        }

        // This test method will Truncates the provided string from the right if it is longer that specified length.
        [TestMethod()]
        public void TruncateRight_Valid()
        {
                ////Act
                string result = strTestMethod.TruncateRight(intValue);
                ////Assert
                Assert.AreEqual(string.Empty, result);
        }

        // This test method will Centers text within the specified maximum length, biased to the left.
        [TestMethod()]
        public void CenterText_Valid()
        {
                ////Act
                string result = strTestMethod.CenterText(intValue);
                ////Assert
                Assert.AreEqual(string.Empty, result);
        }

        // This test method will Centers text within the specified maximum length, biased to the left.
        [TestMethod()]
        public void CenterTextWithParameter_Valid()
        {
                ////Act
                string result = strTestMethod.CenterText(intValue, c);
                ////Assert
                Assert.AreEqual(string.Empty, result);
        }

        // This test method will Performs a case insensitive string replacement.
        [TestMethod()]
        public void ReplaceCaseInsensitive_Valid()
        {
                ////Act
                string result = strTestMethod.ReplaceCaseInsensitive(testvalue, testResult);
                ////Assert
                Assert.AreEqual(string.Empty, result);
        }

        // This test method will Ensures a string starts with a specific string.
        [TestMethod()]
        public void EnsureStart_Valid()
        {
                ////Act
                string result = strTestMethod.EnsureStart(c);
                ////Assert
                Assert.AreEqual(string.Empty, result);
        }

        // This test method will Ensures a string starts with a specific string.
        [TestMethod()]
        public void EnsureStart1_Valid()
        {
                ////Act
                string result = strTestMethod.EnsureStart(testvalue);
                ////Assert
                Assert.AreEqual(string.Empty, result);
        }

        /// This test method will Ensures a string starts with a specific string.
        [TestMethod()]
        public void EnsureStart2_Valid()
        {
                ////Act
                string result = strTestMethod.EnsureStart(c, boolvalue);
                ////Assert
                Assert.AreEqual(string.Empty, result);
        }

        // This test method will Ensures a string ends with a specific character.
        [TestMethod()]
        public void EnsureEnd_Valid()
        {
                ////Act
                string result = strTestMethod.EnsureEnd(c);
                ////Assert
                Assert.AreEqual(string.Empty, result);
        }

        // This test method will Ensures a string ends with a specific character.
        [TestMethod()]
        public void EnsureEnd1_Valid()
        {
                ////Act
                string result = strTestMethod.EnsureEnd(testvalue);
                ////Assert
                Assert.AreEqual(string.Empty, result);
        }

        // This test method will Ensures a string ends with a specific string with Input parameter.
        [TestMethod()]
        public void EnsureEnd2_Valid()
        {
                ////Act
                string result = strTestMethod.EnsureEnd(c, boolvalue);
                ////Assert
                Assert.AreEqual(string.Empty, result);
        }

        // This test method will Reverses the order of the characters in a string.
        [TestMethod()]
        public void Reverse_Valid()
        {
                ////Act
                string result = strTestMethod.Reverse();
                ////Assert
                Assert.AreEqual(string.Empty, result);
        }

        // This test method will Searches a string for a repeated instance of the specified.
        [TestMethod()]
        public void IndexOfRepeatedChar_Valid()
        {
                ////Act
                int result = strTestMethod.IndexOfRepeatedChar();
                ////Assert
                Assert.AreEqual(intData, result);
        }

        // This test method will Searches a string for a repeated instance of the specified.
        [TestMethod()]
        public void IndexOfRepeatedChar2_Valid()
        {
                ////Act
                int result = strTestMethod.IndexOfRepeatedChar(c);
                ////Assert
                Assert.AreEqual(intData, result);
        }

        // This test method will Searches a string for a repeated instance of the specified.
        [TestMethod()]
        public void IndexOfRepeatedChar3_Valid()
        {
                ////Act
                int result = strTestMethod.IndexOfRepeatedChar(intValue);
                ////Assert
                Assert.AreEqual(intData, result);
        }

        // This test method will Returns the index of the last repeated index of the first group of repeated characters that begin with the.
        [TestMethod()]
        public void IndexOfRepeatedChar4_Valid()
        {
                ////Act
                int result = strTestMethod.IndexOfRepeatedChar(c, intValue);
                ////Assert
                Assert.AreEqual(intData, result);
        }

        // This test method will Places an ellipsis in the middle of a string as it is trimmed to length specified.
        [TestMethod()]
        public void TrimWithEllipsisMiddle_Valid()
        {
                ////Act
                string result = strTestMethod.TrimWithEllipsisMiddle(intValue);
                ////Assert
                Assert.AreEqual(string.Empty, result);
        }

        // This test method will Places an ellipsis at the end of a string as it is trimmed to length specified.
        [TestMethod()]
        public void TrimWithEllipsisEnd_Valid()
        {
                ////Act
                string result = strTestMethod.TrimWithEllipsisEnd(intValue);
                ////Assert
                Assert.AreEqual(string.Empty, result);
        }
    }
}
