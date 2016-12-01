//******************************************************************************************************
//  TimestampFilter.cs - Gbtc
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
//  11/01/2016 - Steven E. Chisholm
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Data;
using System.IO;
using GSF.Diagnostics;
using GSF.IO;

namespace LogFileViewer.Filters
{
    internal class TypeFilter : FilterBase
    {
        public enum Mode
        {
            Type,
            Assembly,
            RelatedType,
        }

        public enum MatchMode
        {
            Equals,
            Contains,
            StartsWith
        }

        private string[] m_matchingNames;
        private Mode m_mode;
        private MatchMode m_matchMode;

        public TypeFilter(Mode mode, MatchMode matchMode, params string[] matchingNames)
        {
            m_matchMode = matchMode;
            m_matchingNames = matchingNames;
            m_mode = mode;
        }

        public TypeFilter(Stream stream)
        {
            byte version = stream.ReadNextByte();
            switch (version)
            {
                case 1:
                    m_mode = (Mode)stream.ReadNextByte();
                    m_matchMode = (MatchMode)stream.ReadNextByte();
                    m_matchingNames = new string[stream.ReadInt32()];
                    for (int x = 0; x < m_matchingNames.Length; x++)
                    {
                        m_matchingNames[x] = stream.ReadString();
                    }
                    break;
                default:
                    throw new VersionNotFoundException();
            }
        }

        public override bool IsMatch(LogMessage log)
        {
            switch (m_mode)
            {
                case Mode.Type:
                    switch (m_matchMode)
                    {
                        case MatchMode.Equals:
                            return m_matchingNames[0] == log.TypeName;
                        case MatchMode.Contains:
                            return log.TypeName.Contains(m_matchingNames[0]);
                        case MatchMode.StartsWith:
                            return log.TypeName.StartsWith(m_matchingNames[0]);
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                case Mode.RelatedType:
                    foreach (var item in m_matchingNames)
                    {
                        if (!(log.TypeName == item || log.RelatedTypes.Contains(item)))
                            return false;
                    }
                    return true;
                case Mode.Assembly:
                    switch (m_matchMode)
                    {
                        case MatchMode.Equals:
                            return m_matchingNames[0] == log.AssemblyName;
                        case MatchMode.Contains:
                            return log.AssemblyName.Contains(m_matchingNames[0]);
                        case MatchMode.StartsWith:
                            return log.AssemblyName.StartsWith(m_matchingNames[0]);
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        protected override void SaveInternal(Stream stream)
        {
            stream.Write((byte)1);
            stream.Write((byte)m_mode);
            stream.Write((byte)m_matchMode);
            stream.Write(m_matchingNames.Length);
            foreach (var name in m_matchingNames)
            {
                stream.Write(name);
            }
        }

        protected override string DescriptionInternal
        {
            get
            {

                switch (m_mode)
                {
                    case Mode.Type:
                        switch (m_matchMode)
                        {
                            case MatchMode.Equals:
                                return "Type: " + m_matchingNames[0];
                            case MatchMode.Contains:
                                return "Type Contains: " + m_matchingNames[0];
                            case MatchMode.StartsWith:
                                return "Type Starts With: " + m_matchingNames[0];
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    case Mode.RelatedType:
                        return "Type: " + string.Join(", ", m_matchingNames);
                    case Mode.Assembly:
                        switch (m_matchMode)
                        {
                            case MatchMode.Equals:
                                return "Assembly: " + m_matchingNames[0];
                            case MatchMode.Contains:
                                return "Assembly Contains: " + m_matchingNames[0];
                            case MatchMode.StartsWith:
                                return "Assembly Starts With: " + m_matchingNames[0];
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
    }
}