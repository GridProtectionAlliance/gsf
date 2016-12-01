////******************************************************************************************************
////  MatchType.cs - Gbtc
////
////  Copyright © 2016, Grid Protection Alliance.  All Rights Reserved.
////
////  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
////  the NOTICE file distributed with this work for additional information regarding copyright ownership.
////  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may not use this
////  file except in compliance with the License. You may obtain a copy of the License at:
////
////      http://opensource.org/licenses/MIT
////
////  Unless agreed to in writing, the subject software distributed under the License is distributed on an
////  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
////  License for the specific language governing permissions and limitations.
////
////  Code Modification History:
////  ----------------------------------------------------------------------------------------------------
////  11/01/2016 - Steven E. Chisholm
////       Generated original version of source code.
////
////******************************************************************************************************

//using System;
//using System.Collections.Generic;
//using System.Data;
//using System.IO;
//using System.Windows.Forms;
//using GSF.Diagnostics;
//using GSF.IO;
//namespace LogFileViewer.Filters
//{
//    internal class MatchType : IMessageMatch
//    {
//        private PublisherTypeDefinition m_deff;
//        private string m_typeName;
//        private MessageClass m_class;
//        private MessageLevel m_level;
//        private bool m_includeIfMatched;
//        private bool m_typeAndLevel;
//        private bool m_isRelatedMatch;

//        public MatchType(LogMessage typeName)
//        {
//            m_deff = typeName.EventPublisherDetails.TypeData;
//            m_typeName = typeName.TypeName;
//            m_class = typeName.Classification;
//            m_level = typeName.Level;
//        }

//        public MatchType(Stream stream)
//        {
//            byte version = stream.ReadNextByte();
//            switch (version)
//            {
//                case 1:
//                    m_typeName = stream.ReadString();
//                    m_includeIfMatched = stream.ReadBoolean();
//                    m_typeAndLevel = false;
//                    break;
//                case 2:
//                    m_typeName = stream.ReadString();
//                    m_includeIfMatched = stream.ReadBoolean();
//                    m_typeAndLevel = stream.ReadBoolean();
//                    m_class = (MessageClass)stream.ReadNextByte();
//                    m_level = (MessageLevel)stream.ReadNextByte();
//                    break;
//                case 3:
//                    m_typeName = stream.ReadString();
//                    m_includeIfMatched = stream.ReadBoolean();
//                    m_typeAndLevel = stream.ReadBoolean();
//                    m_class = (MessageClass)stream.ReadNextByte();
//                    m_level = (MessageLevel)stream.ReadNextByte();
//                    m_isRelatedMatch = stream.ReadBoolean();
//                    break;
//                default:
//                    throw new VersionNotFoundException();
//            }
//        }

//        public FilterType TypeCode => FilterType.Type;

//        public void Save(Stream stream)
//        {
//            stream.Write((byte)3);
//            stream.Write(m_typeName);
//            stream.Write(m_includeIfMatched);
//            stream.Write(m_typeAndLevel);
//            stream.Write((byte)m_class);
//            stream.Write((byte)m_level);
//            stream.Write(m_isRelatedMatch);
//        }

//        public bool IsIncluded(LogMessage log)
//        {
//            if (m_isRelatedMatch)
//            {
//                if (log.TypeName == m_typeName || log.RelatedTypes.Contains(m_typeName))
//                    return m_includeIfMatched;
//                return !m_includeIfMatched;
//            }
//            if (m_typeAndLevel)
//            {
//                if (log.TypeName == m_typeName && log.Classification == m_class && log.Level == m_level)
//                    return m_includeIfMatched;
//                return !m_includeIfMatched;
//            }
//            if (log.TypeName == m_typeName)
//                return m_includeIfMatched;
//            return !m_includeIfMatched;
//        }

//        public string Description
//        {
//            get
//            {
//                if (m_typeAndLevel)
//                {
//                    if (m_includeIfMatched)
//                        return $"Include if Type: {m_typeName} ({m_class} - {m_level})";
//                    else
//                        return $"Exclude if Type: {m_typeName} ({m_class} - {m_level})";
//                }
//                if (m_includeIfMatched)
//                    return "Include if Type: " + m_typeName;
//                else
//                    return "Exclude if Type: " + m_typeName;
//            }
//        }

//        public IEnumerable<Tuple<string, Func<bool>>> GetMenuButtons()
//        {
//            return new[]
//                   {
//                       Tuple.Create<string, Func<bool>>("Include Type", () => { m_includeIfMatched = true; return true; }),
//                       Tuple.Create<string, Func<bool>>("Exclude Type", () => { m_includeIfMatched = false; return true;}),
//                       Tuple.Create<string, Func<bool>>("Include Type And Level", () => { m_includeIfMatched = true; m_typeAndLevel = true; return true;}),
//                       Tuple.Create<string, Func<bool>>("Exclude Type And Level", () => { m_includeIfMatched = false; m_typeAndLevel = true; return true;}),
//                       Tuple.Create<string, Func<bool>>("Include Related Type", () =>
//                                                                           {
//                                                                               m_includeIfMatched = true;
//                                                                               m_isRelatedMatch = true;
//                                                                               using (var frm = new RelatedTypesFilter(m_deff))
//                                                                               {
//                                                                                   if (frm.ShowDialog() == DialogResult.OK)
//                                                                                   {
//                                                                                       m_typeName = frm.SelectedItems;
//                                                                                       return true;
//                                                                                   }
//                                                                                   return false;
//                                                                               }

//                                                                           }),
//                       Tuple.Create<string, Func<bool>>("Exclude Related Type", () =>
//                                                                           {
//                                                                               m_includeIfMatched = false;
//                                                                               m_isRelatedMatch = true;
//                                                                               using (var frm = new RelatedTypesFilter(m_deff))
//                                                                               {
//                                                                                   if (frm.ShowDialog() == DialogResult.OK)
//                                                                                   {
//                                                                                       m_typeName = frm.SelectedItems;
//                                                                                       return true;
//                                                                                   }
//                                                                                   return false;
//                                                                               }
//                                                                           })

//                   };
//        }

//        public void ToggleResult()
//        {
//            m_includeIfMatched = !m_includeIfMatched;
//        }
//    }
//}