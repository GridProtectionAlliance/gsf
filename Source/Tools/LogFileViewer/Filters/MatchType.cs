//******************************************************************************************************
//  MatchType.cs - Gbtc
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
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text.RegularExpressions;
using GSF.Diagnostics;
using GSF.IO;
namespace LogFileViewer.Filters
{
    internal class MatchType : IMessageMatch
    {
        private string m_typeName;
        private bool m_includeIfMatched;

        public MatchType(LogMessage typeName)
        {
            m_typeName = typeName.EventPublisherDetails.TypeName;
        }

        public MatchType(Stream stream)
        {
            byte version = stream.ReadNextByte();
            switch (version)
            {
                case 1:
                    m_typeName = stream.ReadString();
                    m_includeIfMatched = stream.ReadBoolean();
                    break;
                default:
                    throw new VersionNotFoundException();
            }
        }

        public FilterType TypeCode => FilterType.Type;

        public void Save(Stream stream)
        {
            stream.Write((byte)1);
            stream.Write(m_typeName);
            stream.Write(m_includeIfMatched);
        }

        public bool IsIncluded(LogMessage log)
        {
            if (log.EventPublisherDetails.TypeName == m_typeName)
                return m_includeIfMatched;
            return !m_includeIfMatched;
        }

        public string Description
        {
            get
            {
                if (m_includeIfMatched)
                    return "Include if Type: " + m_typeName;
                else
                    return "Exclude if Type: " + m_typeName;
            }
        }

        public IEnumerable<Tuple<string, Func<bool>>> GetMenuButtons()
        {
            return new[]
                   {
                       Tuple.Create<string, Func<bool>>("Include Type", () => { m_includeIfMatched = true; return true; }),
                       Tuple.Create<string, Func<bool>>("Exclude Type", () => { m_includeIfMatched = false; return true;})
                   };
        }

        public void ToggleResult()
        {
            m_includeIfMatched = !m_includeIfMatched;
        }
    }
}