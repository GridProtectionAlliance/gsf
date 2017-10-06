//******************************************************************************************************
//  StringExtensions.cs - Gbtc
//
//  Copyright © 2017, Grid Protection Alliance.  All Rights Reserved.
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
//  06/21/2017 - F. Russell Robertson
//       Created original version of source code.
//
//******************************************************************************************************


using System;


namespace GSF.SELEventParser
{
    // Implements the SEL method for checking the byte-wise integrity of each line in cev files
    public class ByteSum
    {
        #region [ Members ]

        // Constants
        const char separator = ',';

        // Fields
        private ulong m_byteSumValue;
        private string m_byteSumHexValue;
        private string m_lineData;
        private string m_lineByteSum;
        private bool m_match;

        private Action<string> m_loggingFunction;

        #endregion

        #region [ Constructors ]

        public ByteSum()
            : this(message => { })
        {
        }

        public ByteSum(Action<string> loggingFunction)
        {
            if ((object)loggingFunction == null)
                throw new ArgumentNullException(nameof(loggingFunction));

            m_loggingFunction = loggingFunction;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets/Sets the computed sum-of-the-bytes (ByteSum) as a truncated (4 char) hex value
        /// </summary>
        public string HexValue
        {
            get { return m_byteSumHexValue; }
            set { m_byteSumHexValue = value; }
        }

        /// <summary>
        /// Gets/Sets the computed sum-of-the-bytes (BtyeSum) as a unsigned 64 bit integer
        /// </summary>
        public ulong Value
        {
            get { return m_byteSumValue; }
            set { m_byteSumValue = value; }
        }

        /// <summary>
        /// True if the computed ByteSum matches the ByteSum provided at the end of the line
        /// </summary>
        public bool Match
        {
            get { return m_match; }
            set { m_match = value; }
        }

        /// <summary>
        /// Get/Sets the data portion of the line (all characters to the left of the provided ByteSum)
        /// </summary>
        public string SentData
        {
            get { return m_lineData; }
            set { m_lineData = value; }
        }

        /// <summary>
        /// Gets/Sets the BtyeSum provided at the end of the line
        /// </summary>
        public string SentHexValue
        {
            get { return m_lineByteSum; }
            set { m_lineByteSum = value; }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Implements the SEL method for checking the byte-wise integrity of each line in cev files
        /// </summary>
        /// <param name="line">The source string to test that contains "data" and the "truncated ByteSum value in Hex"</param>
        /// <param name="fileIdentifier">The file name or identifier.</param>
        /// <param name="hexValueLength">The fixed length of the hex value in the "line" string.</param>
        /// <returns>Sets class properties.</returns>
        public void Check(string line, string fileIdentifier = "", int hexValueLength = 4)
        {
            if (hexValueLength <= 0)
                throw new ArgumentException(string.Format("ByteSum.Check hexValueLenth={0} is invalid.", hexValueLength.ToString()));

            string s = string.Empty;

            Match = false;
            Value = 0;
            HexValue = new string('0', hexValueLength);
            SentData = string.Empty;
            SentHexValue = string.Empty;

            if (string.IsNullOrEmpty(line))
            {
                m_loggingFunction(string.Format("ByteSum check error.  Requested a check of null or empty line. File:{0}", fileIdentifier));
                Match = true; //by definition
                return;
            }

            string left = GetData(line, false);
            if (string.IsNullOrEmpty(left))
            {
                m_loggingFunction(string.Format("ByteSum check error.  Requested a check of a line that contains no data.", fileIdentifier));
                Match = true; //by definition
                return;
            }

            SentData = left;
            Value = ComputeSum(left.ToCharArray());
            HexValue = GetHexValue(Value, hexValueLength);

            string right = GetByteSum(line, true).Trim();
            if (string.IsNullOrEmpty(right))
            {
                m_loggingFunction(string.Format("ByteSum check error.  No BtyeSum found to check in file:{0}. Line data:{1}", fileIdentifier, SentData));
                return;
            }
            //if (right.Contains("FID"))
            //{
            //    log.Info(string.Format("No BtyeSum found to check. However, 'FID' in the bytesum position can be valid. File: {0} Line data:{1}", fileIdentifier, byteSum.SentData));
            //    byteSum.SentHexValue = right;
            //    byteSum.Match = true;
            //    return byteSum;
            //}
            if (!System.Text.RegularExpressions.Regex.IsMatch(right, @"\A\b[0-9a-fA-F]+\b\Z"))
            {
                m_loggingFunction(string.Format("ByteSum check error.  ByteSum provided:\"{0}\" is not a valid hex value. File:{1}  Line data:{2}", right, fileIdentifier, SentData));
                return;
            }
            if (right.Length != hexValueLength)
            {
                m_loggingFunction(string.Format("ByteSum check error.  ByteSum provided:\"{0}\" does not have the required length of {1}. File:{2} Line data:{3}", right, hexValueLength.ToString(), fileIdentifier, SentData));
                return;
            }

            SentHexValue = right;
            Match = string.Equals(right, HexValue);

            if (!Match)
                m_loggingFunction(string.Format("ByteSum check error.  Provided ByteSum:\"{0}\"  Computed ByteSum:\"{1}\"   File:{2} Line data:{3}", SentHexValue, HexValue, fileIdentifier, SentData));

            return;
        }

        #endregion

        #region [ Static ]

        // Static Methods

        /// <summary>
        /// Returns the byte sum as a hex value (string)
        /// </summary>
        /// <param name="hexValueLength">the length of the return string. Padded left with zero if short. Set to 0 to disable. Default 4.</param>
        /// <returns>The sum as a hex value</returns>
        private static string GetHexValue(ulong value, int hexValueLength = 4)
        {
            string result = value.ToString("X").PadLeft(hexValueLength, '0');
            return result.SubstringEnd(hexValueLength);
        }

        private static ulong ComputeSum(char[] inArray)
        {
            ulong total = 0;
            foreach (char c in inArray)
            {
                total += c;
            }
            return total;
        }

        private static string GetByteSum(string line, bool quoteUnwrap = true)
        {
            if (string.IsNullOrEmpty(line))
                return string.Empty;

            if (line.IndexOf(separator) < 0)
                return string.Empty;

            string inStringRev = line.Reverse();
            int pos = inStringRev.IndexOf(separator);

            if (quoteUnwrap)
                return line.Substring(line.Length - pos).ToUpper().Trim('\"');

            return line.Substring(line.Length - pos).ToUpper().Trim();
        }

        private static string GetData(string line, bool twoCSVvaluesRequired = true)
        {
            if (string.IsNullOrEmpty(line))
                return string.Empty;

            if (line.IndexOf(separator) < 0 && twoCSVvaluesRequired)
                return string.Empty;

            if (line.IndexOf(separator) < 0)
                return line;

            string inStringRev = line.Reverse();
            int pos = inStringRev.IndexOf(separator);

            return line.Substring(0, line.Length - pos);
        }

        #endregion
    }
}
