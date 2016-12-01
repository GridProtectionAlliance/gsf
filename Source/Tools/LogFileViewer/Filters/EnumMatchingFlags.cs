//******************************************************************************************************
//  EnumMatchingFlags.cs - Gbtc
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
using GSF.IO;

namespace LogFileViewer.Filters
{
    public enum EnumMatchingFlagsMode
    {
        /// <summary>
        /// None of the supplied flags are set
        /// </summary>
        None,
        /// <summary>
        /// Any of the supplied flags are set
        /// </summary>
        Any,
        /// <summary>
        /// All of the supplied flags are set
        /// </summary>
        All,
        /// <summary>
        /// Only an exact match
        /// </summary>
        Exactly
    }

    public class EnumMatchingFlags
    {
        private EnumMatchingFlagsMode m_matchMode;
        private string m_description;
        private int m_value;

        public EnumMatchingFlags(Stream stream)
        {
            switch (stream.ReadNextByte())
            {
                case 1:
                    m_matchMode = (EnumMatchingFlagsMode)stream.ReadNextByte();
                    m_value = stream.ReadInt32();
                    m_description = stream.ReadString();
                    break;
                default:
                    throw new VersionNotFoundException();
            }
        }

        public EnumMatchingFlags(EnumMatchingFlagsMode mode, int value, string description)
        {
            m_description = description;
            m_matchMode = mode;
            m_value = value;
        }

        public void Save(Stream stream)
        {
            stream.Write((byte)1);
            stream.Write((byte)m_matchMode);
            stream.Write(m_value);
        }

        public bool IsMatch(int value)
        {
            switch (m_matchMode)
            {
                case EnumMatchingFlagsMode.None:
                    return (m_value & value) == 0;
                case EnumMatchingFlagsMode.Any:
                    return (m_value & value) != 0;
                case EnumMatchingFlagsMode.All:
                    return (m_value & value) != m_value;
                case EnumMatchingFlagsMode.Exactly:
                    return m_value == value;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public override string ToString()
        {
            switch (m_matchMode)
            {
                case EnumMatchingFlagsMode.None:
                    return "None: " + m_description;
                case EnumMatchingFlagsMode.Any:
                    return "Any: " + m_description;
                case EnumMatchingFlagsMode.All:
                    return "All: " + m_description;
                case EnumMatchingFlagsMode.Exactly:
                    return "Equals: " + m_description;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}