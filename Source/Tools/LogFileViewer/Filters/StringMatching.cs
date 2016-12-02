//******************************************************************************************************
//  StringMatching.cs - Gbtc
//
//  Copyright © 2016, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may not use this
//  file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  12/01/2016 - Steven E. Chisholm
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Data;
using System.IO;
using System.Text.RegularExpressions;
using GSF.IO;

namespace LogFileViewer.Filters
{
    public enum StringMatchingMode
    {
        Exact,
        StartsWith,
        Contains,
        EndsWith,
        Regex
    }

    public class StringMatching
    {
        public StringMatchingMode MatchMode { get; private set; }
        public string MatchText { get; private set; }
        private Regex m_matchRegex;

        public StringMatching(Stream stream)
        {
            switch (stream.ReadNextByte())
            {
                case 1:
                    MatchMode = (StringMatchingMode)stream.ReadNextByte();
                    MatchText = stream.ReadString();
                    if (MatchMode == StringMatchingMode.Regex)
                        m_matchRegex = new Regex(MatchText);
                    break;
                default:
                    throw new VersionNotFoundException();
            }
        }

        public StringMatching(StringMatchingMode mode, string value)
        {
            MatchText = value;
            MatchMode = mode;
            if (MatchMode == StringMatchingMode.Regex)
                m_matchRegex = new Regex(MatchText);
        }

        public void Save(Stream stream)
        {
            stream.Write((byte)1);
            stream.Write((byte)MatchMode);
            stream.Write(MatchText);
        }

        public bool IsMatch(string value)
        {
            switch (MatchMode)
            {
                case StringMatchingMode.Exact:
                    return value == MatchText;
                case StringMatchingMode.StartsWith:
                    return value.StartsWith(MatchText);
                case StringMatchingMode.Contains:
                    return value.Contains(MatchText);
                case StringMatchingMode.EndsWith:
                    return value.EndsWith(MatchText);
                case StringMatchingMode.Regex:
                    return m_matchRegex.IsMatch(value);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public override string ToString()
        {
            switch (MatchMode)
            {
                case StringMatchingMode.Exact:
                    return "Equals: " + MatchText;
                case StringMatchingMode.StartsWith:
                    return "Starts With: " + MatchText;
                case StringMatchingMode.Contains:
                    return "Contains: " + MatchText;
                case StringMatchingMode.EndsWith:
                    return "Ends With: " + MatchText;
                case StringMatchingMode.Regex:
                    return "Regex: " + MatchText;
                default:
                    return "Unknown";
            }
        }
    }
}